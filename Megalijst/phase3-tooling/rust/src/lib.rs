use anyhow::{Result, anyhow, bail, ensure, Context};
use quick_xml::{Reader, events::Event};
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::{collections::{BTreeMap, BTreeSet}, fs::{self, File}, io::{Read, Write, Cursor}, path::Path};
use zip::{ZipArchive, ZipWriter, write::SimpleFileOptions};

pub const APPROVED: &str = include_str!("../approved-delta.json");
const NS: &str = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
const REL: &str = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
const PKGREL: &str = "http://schemas.openxmlformats.org/package/2006/relationships";
const PARTS: [&str;4] = ["xl/sharedStrings.xml", "xl/worksheets/sheet1.xml", "xl/worksheets/sheet2.xml", "xl/worksheets/sheet3.xml"];
#[derive(Clone, Deserialize, Debug)]
pub struct Delta { pub id:String, pub sheet:usize, pub cell:String, pub old_ref:Option<usize>, pub old:Option<String>, pub new:String, pub style:usize }
#[derive(Clone, Deserialize)]
pub struct Manifest { pub allowed_parts:Vec<String>, pub sources:Vec<[String;2]>, pub bindings:Vec<[String;3]>, pub cells:Vec<Delta> }
pub fn manifest() -> Result<Manifest> { Ok(serde_json::from_str(APPROVED)?) }
pub fn hash(bytes:&[u8])->String { format!("{:x}", Sha256::digest(bytes)) }
pub fn check_manifest(text:&str)->Result<Manifest> {
    let value:serde_json::Value=serde_json::from_str(text)?;
    ensure!(value==serde_json::from_str::<serde_json::Value>(APPROVED)?, "manifest differs from compiled approved snapshot");
    let m:Manifest=serde_json::from_value(value)?; validate_deltas(&m)?; Ok(m)
}
pub fn coord(s:&str)->Result<(usize,usize)> {
    let split=s.find(|c:char| !c.is_ascii_uppercase()).ok_or_else(||anyhow!("missing row"))?;
    ensure!(split>0 && s[split..].bytes().all(|x|x.is_ascii_digit()) && !s[split..].starts_with('0'), "invalid cell reference {s}");
    let mut col=0usize; for c in s[..split].bytes(){col=col.checked_mul(26).and_then(|v|v.checked_add((c-b'A'+1) as usize)).context("column overflow")?;}
    let row=s[split..].parse::<usize>()?; ensure!(col<=16384 && row<=1048576, "cell out of bounds"); Ok((row,col))
}
fn frozen(sheet:usize, c:&str)->bool {
    let Ok((r,_))=coord(c) else{return false};
    match sheet {1=>["E3","E5","E7","E10","E11"].contains(&c),2=>[10,30,54,99,126].contains(&r),3=>[19,20,21,22,47,48,49,50,75,110].contains(&r),_=>false}
}
pub fn validate_deltas(m:&Manifest)->Result<()> {
    ensure!(m.allowed_parts==PARTS, "closed allowlist changed"); ensure!(m.cells.len()==43,"43 targets required");
    let mut seen=BTreeSet::new(); let mut ids=BTreeSet::new();
    for d in &m.cells {ensure!((1..=3).contains(&d.sheet),"unknown target part"); coord(&d.cell)?; ensure!(seen.insert((d.sheet,&d.cell)) && ids.insert(&d.id),"duplicate target"); ensure!(!frozen(d.sheet,&d.cell),"excluded overlap"); ensure!(d.old_ref.is_some()==d.old.is_some(),"inconsistent old state");}
    Ok(())
}
#[derive(Clone,Debug)]
struct Node { name:String, uri:String, attrs:BTreeMap<String,String>, start:usize, open_end:usize, close_start:usize, end:usize, children:Vec<usize>, parent:Option<usize> }
struct Xml<'a>{bytes:&'a [u8], nodes:Vec<Node>}
impl<'a> Xml<'a> {
    fn parse(bytes:&'a [u8])->Result<Self>{
        std::str::from_utf8(bytes)?; let mut reader=Reader::from_reader(bytes); let mut nodes:Vec<Node>=vec![]; let mut stack:Vec<usize>=vec![]; let mut scopes:Vec<BTreeMap<String,String>>=vec![];
        loop {let start=reader.buffer_position() as usize; let event=reader.read_event()?; let end=reader.buffer_position() as usize;
            match event {
                Event::Start(ref e)|Event::Empty(ref e)=>{
                    let name=std::str::from_utf8(e.name().as_ref())?.to_owned(); let mut attrs=BTreeMap::new(); let mut scope=scopes.last().cloned().unwrap_or_default();
                    for a in e.attributes(){let a=a?; let k=std::str::from_utf8(a.key.as_ref())?.to_owned(); let v=a.decode_and_unescape_value(reader.decoder())?.into_owned(); if k=="xmlns"{scope.insert(String::new(),v.clone());} else if let Some(p)=k.strip_prefix("xmlns:"){scope.insert(p.into(),v.clone());} ensure!(attrs.insert(k,v).is_none(),"duplicate attribute");}
                    let prefix=name.split_once(':').map(|x|x.0).unwrap_or(""); let uri=scope.get(prefix).cloned().unwrap_or_default(); ensure!(prefix.is_empty()||!uri.is_empty(),"unbound prefix");
                    let parent=stack.last().copied(); let index=nodes.len(); nodes.push(Node{name,uri,attrs,start,open_end:end,close_start:end,end,children:vec![],parent}); if let Some(p)=parent{nodes[p].children.push(index);}
                    if matches!(event,Event::Start(_)){stack.push(index); scopes.push(scope);}
                },
                Event::End(e)=>{let i=stack.pop().context("unexpected close")?; scopes.pop(); ensure!(nodes[i].name.as_bytes()==e.name().as_ref(),"mismatched close"); nodes[i].close_start=start;nodes[i].end=end;},
                Event::DocType(_)=>bail!("DTD forbidden"), Event::Eof=>break,
                Event::Text(e)=>{e.unescape()?;}, _=>{}
            }
        }
        ensure!(stack.is_empty() && nodes.iter().filter(|n|n.parent.is_none()).count()==1,"invalid XML root"); Ok(Self{bytes,nodes})
    }
    fn local(n:&Node)->&str {n.name.rsplit(':').next().unwrap()}
    fn named(&self, local:&str,uri:&str)->Vec<usize>{self.nodes.iter().enumerate().filter(|(_,n)|Self::local(n)==local&&n.uri==uri).map(|(i,_)|i).collect()}
    fn children(&self,i:usize,local:&str)->Vec<usize>{self.nodes[i].children.iter().copied().filter(|j|Self::local(&self.nodes[*j])==local&&self.nodes[*j].uri==NS).collect()}
    fn raw(&self,i:usize)->&[u8]{let n=&self.nodes[i]; &self.bytes[n.start..n.end]}
    fn content(&self,i:usize)->Result<String>{let n=&self.nodes[i];ensure!(n.children.is_empty(),"nested value");Ok(quick_xml::escape::unescape(std::str::from_utf8(&self.bytes[n.open_end..n.close_start])?)?.into_owned())}
    fn attr(&self,i:usize,key:&str)->Result<&str>{self.nodes[i].attrs.get(key).map(String::as_str).with_context(||format!("missing {key}"))}
    fn attr_patch(&self,i:usize,key:&str,old:&str,new:&str)->Result<Patch>{
        ensure!(self.attr(i,key)?==old,"wrong old attribute {key}"); let n=&self.nodes[i]; let raw=std::str::from_utf8(&self.bytes[n.start..n.open_end])?;
        // Lexically locate the attribute value, never reserialize its surrounding tag.
        let mut pos=n.name.len()+1; let b=raw.as_bytes(); while pos<b.len(){while pos<b.len()&&b[pos].is_ascii_whitespace(){pos+=1;} if pos>=b.len()||b[pos]==b'/'||b[pos]==b'>'{break;}
            let kstart=pos; while pos<b.len()&&!b[pos].is_ascii_whitespace()&&b[pos]!=b'='{pos+=1;} let k=&raw[kstart..pos]; while pos<b.len()&&b[pos].is_ascii_whitespace(){pos+=1;}ensure!(b.get(pos)==Some(&b'='),"attribute syntax"); pos+=1; while pos<b.len()&&b[pos].is_ascii_whitespace(){pos+=1;} let quote=*b.get(pos).context("quote")?; ensure!(quote==b'\''||quote==b'"',"quote");pos+=1;let vstart=pos;while pos<b.len()&&b[pos]!=quote{pos+=1;} ensure!(pos<b.len(),"quote close");if k==key{return Ok(Patch{start:n.start+vstart,end:n.start+pos,text:new.as_bytes().to_vec()});}pos+=1;
        } bail!("attribute not located")
    }
}
#[derive(Clone)]
struct Patch{start:usize,end:usize,text:Vec<u8>}
fn apply(bytes:&[u8],mut patches:Vec<Patch>)->Result<Vec<u8>>{patches.sort_by_key(|p|(p.start,p.end));let mut out=vec![];let mut cursor=0;for p in patches{ensure!(p.start>=cursor&&p.end>=p.start&&p.end<=bytes.len(),"overlapping patches");out.extend_from_slice(&bytes[cursor..p.start]);out.extend_from_slice(&p.text);cursor=p.end;}out.extend_from_slice(&bytes[cursor..]);Xml::parse(&out)?;Ok(out)}
fn tag(root:&Node,local:&str)->String{match root.name.split_once(':'){Some((p,_))=>format!("{p}:{local}"),None=>local.into()}}
fn si_text(xml:&Xml,i:usize)->Result<String>{let mut out=String::new();for &j in &xml.nodes[i].children{if xml.nodes[j].uri!=NS{continue;}match Xml::local(&xml.nodes[j]){"t"=>out.push_str(&xml.content(j)?),"r"=>for k in xml.children(j,"t"){out.push_str(&xml.content(k)?);},"rPh"|"phoneticPr"=>{},_=>bail!("unsupported si content")}}Ok(out)}
fn items(xml:&Xml)->Result<Vec<usize>>{ensure!(Xml::local(&xml.nodes[0])=="sst"&&xml.nodes[0].uri==NS,"sst namespace");Ok(xml.children(0,"si"))}
#[derive(Clone)]
struct Entry{data:Vec<u8>,raw:Vec<u8>,method:zip::CompressionMethod,crc:u32,time:String,unix:Option<u32>,extra:Vec<u8>,comment:String}
type Package=BTreeMap<String,Entry>;
fn package(bytes:&[u8])->Result<Package>{let mut z=ZipArchive::new(Cursor::new(bytes))?;let mut entries=BTreeMap::new();for i in 0..z.len(){let mut f=z.by_index(i)?;let name=f.name().to_owned();ensure!(!name.contains("..")&&!name.starts_with('/')&&!name.contains('\\'),"unsafe entry");ensure!(f.size()<=32*1024*1024,"entry too large");let start=f.data_start() as usize;let end=start.checked_add(f.compressed_size() as usize).context("size overflow")?;ensure!(end<=bytes.len(),"truncated entry");let mut data=vec![];f.read_to_end(&mut data)?;let entry=Entry{data,raw:bytes[start..end].to_vec(),method:f.compression(),crc:f.crc32(),time:format!("{:?}",f.last_modified()),unix:f.unix_mode(),extra:f.extra_data().unwrap_or(&[]).to_vec(),comment:f.comment().into()};ensure!(entries.insert(name,entry).is_none(),"duplicate ZIP entry");}Ok(entries)}
fn data<'a>(p:&'a Package,name:&str)->Result<&'a [u8]>{Ok(&p.get(name).with_context(||format!("missing part {name}"))?.data)}
fn bindings(p:&Package,m:&Manifest)->Result<()> {
    let w=Xml::parse(data(p,"xl/workbook.xml")?)?;let r=Xml::parse(data(p,"xl/_rels/workbook.xml.rels")?)?;
    let sheets=w.named("sheet",NS);ensure!(sheets.len()==3,"sheet count");for (i,b) in m.bindings.iter().enumerate(){let s=sheets[i];ensure!(w.attr(s,"name")?==b[0]&&w.attr(s,"sheetId")?==(i+1).to_string(),"sheet binding");
        let mut rid=None;for(k,v)in &w.nodes[s].attrs{if let Some((prefix,"id"))=k.split_once(':'){let mut scope=Some(s);while let Some(n)=scope{if w.nodes[n].attrs.get(&format!("xmlns:{prefix}")).is_some_and(|x|x==REL){rid=Some(v.as_str());break;}scope=w.nodes[n].parent;}}}ensure!(rid==Some(b[1].as_str()),"sheet relationship ID");
        let matches:Vec<_>=r.named("Relationship",PKGREL).into_iter().filter(|j|r.nodes[*j].attrs.get("Id")==Some(&b[1])).collect();ensure!(matches.len()==1,"relationship unique");let j=matches[0];ensure!(r.attr(j,"Target")?==b[2]&&r.attr(j,"Type")?==format!("{REL}/worksheet")&&!r.nodes[j].attrs.contains_key("TargetMode"),"relationship target/type");}
    Ok(())
}
fn cell_map(xml:&Xml)->Result<BTreeMap<String,usize>>{let mut map=BTreeMap::new();let rows=xml.named("row",NS);let mut last_row=0;for r in rows{ensure!(xml.nodes[r].parent.is_some_and(|p|Xml::local(&xml.nodes[p])=="sheetData"&&xml.nodes[p].uri==NS),"row placement");let row=xml.attr(r,"r")?.parse::<usize>()?;ensure!(row>last_row,"row order");last_row=row;let mut last_col=0;for c in xml.children(r,"c"){let reference=xml.attr(c,"r")?;let(rr,col)=coord(reference)?;ensure!(rr==row&&col>last_col,"cell order or row mismatch");last_col=col;ensure!(map.insert(reference.into(),c).is_none(),"duplicate cell");ensure!(xml.children(c,"f").is_empty(),"cell formulas forbidden");}}
    ensure!(map.len()==xml.named("c",NS).len(),"unexpected cell placement");Ok(map)}
fn inventory(p:&Package, expected_cells:usize,expected_refs:usize,expected_si:usize)->Result<()> {
    ensure!(!p.keys().any(|s|s.to_ascii_lowercase().contains("calcchain")),"calcChain forbidden");let x=Xml::parse(data(p,PARTS[0])?)?;let ss=items(&x)?;ensure!(ss.len()==expected_si&&x.attr(0,"count")?.parse::<usize>()?==expected_refs&&x.attr(0,"uniqueCount")?.parse::<usize>()?==expected_si,"SST counters/items");
    let styles=Xml::parse(data(p,"xl/styles.xml")?)?;let xfs=styles.named("cellXfs",NS);ensure!(xfs.len()==1&&styles.children(xfs[0],"xf").len()==4,"style snapshot");let mut cells=0;let mut refs=0;for part in &PARTS[1..]{let s=Xml::parse(data(p,part)?)?;ensure!(Xml::local(&s.nodes[0])=="worksheet"&&s.nodes[0].uri==NS,"worksheet namespace");for (_,i) in cell_map(&s)? {cells+=1;let n=&s.nodes[i];if let Some(style)=n.attrs.get("s"){ensure!(style.parse::<usize>()?<4,"style range");}if n.attrs.get("t").is_some_and(|x|x=="s"){let v=s.children(i,"v");ensure!(v.len()==1&&s.content(v[0])?.parse::<usize>()?<ss.len(),"shared reference invalid");refs+=1;}}}ensure!(cells==expected_cells&&refs==expected_refs,"inventory cells={cells}, shared refs={refs}");Ok(())
}
fn transform(p:&Package,m:&Manifest)->Result<BTreeMap<String,Vec<u8>>> {
    validate_deltas(m)?;bindings(p,m)?;inventory(p,1260,1163,490)?;
    let sx=Xml::parse(data(p,PARTS[0])?)?;let ss=items(&sx)?;let mut result=BTreeMap::new();let mut appends=String::new();let sin=tag(&sx.nodes[0],"si");let tn=tag(&sx.nodes[0],"t");for d in &m.cells{appends.push_str(&format!("<{sin}><{tn} xml:space=\"preserve\">{}</{tn}></{sin}>",quick_xml::escape::escape(&d.new)));}
    let mut patches=vec![sx.attr_patch(0,"count","1163","1173")?,sx.attr_patch(0,"uniqueCount","490","533")?];let insert=ss.last().map(|i|sx.nodes[*i].end).context("empty SST")?;patches.push(Patch{start:insert,end:insert,text:appends.into_bytes()});result.insert(PARTS[0].into(),apply(sx.bytes,patches)?);
    for sheet in 1..=3{let part=PARTS[sheet];let x=Xml::parse(data(p,part)?)?;let map=cell_map(&x)?;let mut patches=vec![];let mut additions:BTreeMap<usize,String>=BTreeMap::new();
        for(index,d)in m.cells.iter().enumerate().filter(|(_,d)|d.sheet==sheet){let new_index=490+index;if let Some(old)=d.old_ref{let c=*map.get(&d.cell).context("missing old cell")?;ensure!(x.attr(c,"t")?=="s"&&x.attr(c,"s")?.parse::<usize>()?==d.style,"cell type/style");let v=x.children(c,"v");ensure!(v.len()==1&&x.content(v[0])?==old.to_string(),"wrong old ref {}",d.id);ensure!(si_text(&sx,ss[old])?==d.old.as_deref().unwrap(),"wrong old text {}",d.id);let n=&x.nodes[v[0]];patches.push(Patch{start:n.open_end,end:n.close_start,text:new_index.to_string().into_bytes()});}
            else{ensure!(!map.contains_key(&d.cell),"new cell conflict");let(row,col)=coord(&d.cell)?;ensure!(sheet==3&&col>=6&&col<=7,"new coordinate");let rows:Vec<_>=x.named("row",NS).into_iter().filter(|r|x.nodes[*r].attrs.get("r")==Some(&row.to_string())).collect();ensure!(rows.len()==1,"existing row required");let r=rows[0];ensure!(x.children(r,"c").iter().all(|c|coord(x.attr(*c,"r").unwrap()).unwrap().1<6),"F/G already present");let cn=tag(&x.nodes[0],"c");let vn=tag(&x.nodes[0],"v");additions.entry(r).or_default().push_str(&format!("<{cn} r=\"{}\" s=\"{}\" t=\"s\"><{vn}>{new_index}</{vn}></{cn}>",d.cell,d.style));}
        }
        for(r,text)in additions{let cells=x.children(r,"c");let insert=x.nodes[*cells.last().context("empty row")?].end;patches.push(Patch{start:insert,end:insert,text:text.into_bytes()});}
        let dimensions=x.named("dimension",NS);ensure!(dimensions.len()==1,"dimension count");let old=match sheet{1=>"A1:E12",2=>"A1:E130",_=>"A1:E110"};ensure!(x.attr(dimensions[0],"ref")?==old,"old dimension");if sheet==3{patches.push(x.attr_patch(dimensions[0],"ref",old,"A1:G110")?);let cf=x.named("conditionalFormatting",NS);ensure!(cf.len()==1,"CF snapshot count");ensure!(x.children(cf[0],"cfRule").len()==12,"CF rules count");patches.push(x.attr_patch(cf[0],"sqref","A2:E110","A2:G110")?);}
        result.insert(part.into(),apply(x.bytes,patches)?);
    }Ok(result)
}
#[derive(Serialize)]
pub struct Proof {pub status:String,pub source_sha256:String,pub candidate_sha256:Option<String>,pub manifest_sha256:String,pub cells:usize,pub shared_refs:usize,pub si:usize,pub unchanged_cells:usize,pub frozen_cells:usize,pub preserved_parts:BTreeMap<String,String>,pub raw_compressed_sha256:BTreeMap<String,String>,pub source_hashes:BTreeMap<String,String>,pub appends:Vec<serde_json::Value>}
fn verify(original:&Package,candidate:&Package,m:&Manifest,changes:&BTreeMap<String,Vec<u8>>)->Result<(BTreeMap<String,String>,BTreeMap<String,String>)>{
    ensure!(original.keys().eq(candidate.keys()),"part set changed");bindings(candidate,m)?;inventory(candidate,1270,1173,533)?;
    let mut preserved=BTreeMap::new();let mut raw=BTreeMap::new();for(name,a)in original{let b=&candidate[name];if let Some(expected)=changes.get(name){ensure!(&b.data==expected,"target XML differs from selective patch plan");}else{ensure!(a.data==b.data&&a.raw==b.raw&&a.method==b.method&&a.crc==b.crc&&a.time==b.time&&a.unix==b.unix&&a.extra==b.extra&&a.comment==b.comment,"raw entry preservation failed {name}");preserved.insert(name.clone(),hash(&b.data));raw.insert(name.clone(),hash(&b.raw));}}
    let a=Xml::parse(data(original,PARTS[0])?)?;let b=Xml::parse(data(candidate,PARTS[0])?)?;let ai=items(&a)?;let bi=items(&b)?;for i in 0..490{ensure!(a.raw(ai[i])==b.raw(bi[i]),"original si modified");}for(i,d)in m.cells.iter().enumerate(){ensure!(si_text(&b,bi[490+i])?==d.new,"new si text");}
    let mut unchanged=0;let mut freeze=0;for sheet in 1..=3{let a=Xml::parse(data(original,PARTS[sheet])?)?;let b=Xml::parse(data(candidate,PARTS[sheet])?)?;let am=cell_map(&a)?;let bm=cell_map(&b)?;for(c,i)in am{let target=m.cells.iter().find(|d|d.sheet==sheet&&d.cell==c);let j=*bm.get(&c).context("cell removed")?;if target.is_none(){ensure!(a.raw(i)==b.raw(j),"non-target cell changed {c}");unchanged+=1;if frozen(sheet,&c){freeze+=1;}}}for(index,d)in m.cells.iter().enumerate().filter(|(_,d)|d.sheet==sheet){let i=*bm.get(&d.cell).context("target absent")?;ensure!(b.attr(i,"s")?==d.style.to_string()&&b.attr(i,"t")?=="s","target style/type");let v=b.children(i,"v");ensure!(v.len()==1&&b.content(v[0])?==(490+index).to_string(),"target COW ref");}}
    ensure!(unchanged==1227&&freeze==80,"unchanged/freeze count");Ok((preserved,raw))
}
fn write_zip(source_bytes:&[u8],changes:&BTreeMap<String,Vec<u8>>,target:&mut File)->Result<()> {
    let mut writer=ZipWriter::new(target);let mut archive=ZipArchive::new(Cursor::new(source_bytes))?;
    for i in 0..archive.len(){let entry=archive.by_index(i)?;let name=entry.name().to_owned();if let Some(bytes)=changes.get(&name){let mut options=SimpleFileOptions::default().compression_method(entry.compression());if let Some(time)=entry.last_modified(){options=options.last_modified_time(time);}if let Some(mode)=entry.unix_mode(){options=options.unix_permissions(mode);}writer.start_file(name,options)?;writer.write_all(bytes)?;}else{writer.raw_copy_file(entry)?;}}
    writer.finish()?;Ok(())
}
fn new_candidate(out:&Path)->Result<tempfile::NamedTempFile>{
    ensure!(!out.try_exists()?,"output exists: no overwrite");let parent=out.parent().context("output parent")?;ensure!(parent.is_dir(),"output parent must exist");ensure!(out.file_name().is_some(),"output filename");Ok(tempfile::NamedTempFile::new_in(parent)?)
}
fn publish(temp:tempfile::NamedTempFile,out:&Path)->Result<()> {
    temp.persist_noclobber(out).map_err(|e|anyhow!("atomic no-clobber publication failed: {}",e.error))?;Ok(())
}
fn recheck_sources(root:&Path,m:&Manifest,source:&Path,bytes:&[u8])->Result<()> {
    for[name,expected]in &m.sources{ensure!(hash(&fs::read(root.join(name))?)==*expected,"source changed before publication/report");}ensure!(fs::read(source)?==bytes,"original changed");Ok(())
}
pub fn run(root:&Path,external_manifest:&Path,output:Option<&Path>)->Result<Proof>{
    let m=check_manifest(&fs::read_to_string(external_manifest)?)?;let mut hashes=BTreeMap::new();for [name,expected]in &m.sources{let actual=hash(&fs::read(root.join(name)).with_context(||name.clone())?);ensure!(&actual==expected,"source hash mismatch {name}");hashes.insert(name.clone(),actual);}
    let source=root.join(&m.sources[0][0]);let source_bytes=fs::read(&source)?;ensure!(hash(&source_bytes)==m.sources[0][1],"source changed while reading");let p=package(&source_bytes)?;let changes=transform(&p,&m)?;
    // Dry-run is entirely read-only: verifies the prospective byte patches in memory.
    let mut projected=p.clone();for(name,bytes)in &changes{projected.get_mut(name).unwrap().data=bytes.clone();}let(_,_) = verify(&p,&projected,&m,&changes)?;
    let mut proof=Proof{status:"DRY_RUN_READ_ONLY_NO_CANDIDATE".into(),source_sha256:hash(&source_bytes),candidate_sha256:None,manifest_sha256:hash(APPROVED.as_bytes()),cells:1270,shared_refs:1173,si:533,unchanged_cells:1227,frozen_cells:80,preserved_parts:BTreeMap::new(),raw_compressed_sha256:BTreeMap::new(),source_hashes:hashes,appends:m.cells.iter().enumerate().map(|(i,d)|serde_json::json!({"id":d.id,"sheet":d.sheet,"cell":d.cell,"old_ref":d.old_ref,"new_ref":490+i,"text":d.new})).collect()};
    if let Some(out)=output{let mut temp=new_candidate(out)?;write_zip(&source_bytes,&changes,temp.as_file_mut())?;temp.as_file().sync_all()?;
        let candidate_bytes=fs::read(temp.path())?;let candidate=package(&candidate_bytes)?;let(preserved,raw)=verify(&p,&candidate,&m,&changes)?;recheck_sources(root,&m,&source,&source_bytes)?;proof.status="UNVALIDATED_CANDIDATE_PENDING_INDEPENDENT_OPENXML_VALIDATOR".into();proof.candidate_sha256=Some(hash(&candidate_bytes));proof.preserved_parts=preserved;proof.raw_compressed_sha256=raw;
        publish(temp,out)?;
    }else{let(preserved,raw)=verify(&p,&projected,&m,&changes)?;recheck_sources(root,&m,&source,&source_bytes)?;proof.preserved_parts=preserved;proof.raw_compressed_sha256=raw;}
    Ok(proof)
}
#[cfg(test)]
mod tests;
