use std::path::Path;
fn main() {
    let args:Vec<String>=std::env::args().collect();
    let result=(|| -> anyhow::Result<_> {
        anyhow::ensure!(args.len()==4 || args.len()==5,"usage: issuetracker-phase3 dry-run ROOT MANIFEST | candidate ROOT MANIFEST NEW_OUTPUT.xlsx");
        let output=match args[1].as_str(){"dry-run"=>{anyhow::ensure!(args.len()==4,"dry-run accepts no output");None},"candidate"=>{anyhow::ensure!(args.len()==5,"candidate needs new output");Some(Path::new(&args[4]))},_=>anyhow::bail!("unknown command")};
        issuetracker_phase3::run(Path::new(&args[2]),Path::new(&args[3]),output)
    })();
    match result {Ok(proof)=>println!("{}",serde_json::to_string_pretty(&proof).unwrap()),Err(e)=>{eprintln!("{}",serde_json::json!({"status":"BLOCKED","error":format!("{e:#}")}));std::process::exit(1);}}
}
