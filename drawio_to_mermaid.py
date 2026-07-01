import sys
import os
import xml.etree.ElementTree as ET
import base64
import zlib
import urllib.parse
import re
from html import unescape

def clean_label(label):
    if not label:
        return ""
    label = unescape(label)
    label = re.sub(r'<br\s*/?>', ' ', label, flags=re.IGNORECASE)
    label = re.sub(r'</?div[^>]*>', ' ', label, flags=re.IGNORECASE)
    label = re.sub(r'</?span[^>]*>', '', label, flags=re.IGNORECASE)
    label = re.sub(r'</?font[^>]*>', '', label, flags=re.IGNORECASE)
    label = re.sub(r'<[^>]+>', '', label)
    label = re.sub(r'\s+', ' ', label)
    return label.strip()

def wrap_text(text, max_width=20):
    if not text or len(text) <= max_width:
        return text
    words = text.split()
    if not words:
        return ""
    lines = []
    current_line = []
    current_length = 0
    for word in words:
        if current_length + len(word) + (1 if current_line else 0) <= max_width:
            current_line.append(word)
            current_length += len(word) + (1 if len(current_line) > 1 else 0)
        else:
            if current_line:
                lines.append(" ".join(current_line))
            current_line = [word]
            current_length = len(word)
    if current_line:
        lines.append(" ".join(current_line))
    return "\\n".join(lines)

def get_mxgraph_model(xml_path):
    tree = ET.parse(xml_path)
    root = tree.getroot()
    
    diagram = root.find('.//diagram')
    if diagram is None:
        mx_graph_model = root.find('.//mxGraphModel')
        if mx_graph_model is not None:
            return mx_graph_model
        return root
        
    if len(diagram) == 0 and diagram.text:
        compressed_data = diagram.text.strip()
        decoded_base64 = base64.b64decode(compressed_data)
        decompressed = zlib.decompress(decoded_base64, -15)
        xml_string = urllib.parse.unquote(decompressed.decode('utf-8'))
        return ET.fromstring(xml_string)
    else:
        mx_graph_model = diagram.find('.//mxGraphModel')
        if mx_graph_model is not None:
            return mx_graph_model
        return diagram

def parse_elements(root_el):
    nodes = {}
    edges = []
    edge_labels = {}
    
    def process_cell(cell_id, cell_elem, label):
        is_vertex = cell_elem.get('vertex') == '1'
        is_edge = cell_elem.get('edge') == '1'
        style = cell_elem.get('style', '')
        outline = cell_elem.get('outline', '') or ''
        
        cleaned_lbl = clean_label(label)
        
        if is_vertex:
            shape = 'rectangle'
            
            if 'shape=mxgraph.bpmn.gateway' in style or 'mxgraph.bpmn.gateway2' in style or 'shape=mxgraph.flowchart.decision' in style or 'decision' in style:
                shape = 'decision'
                if not cleaned_lbl:
                    if 'parallel' in style or 'gwType=parallel' in style or 'gatewayType=parallel' in style:
                        cleaned_lbl = "+"
                    elif 'inclusive' in style or 'gwType=inclusive' in style or 'gatewayType=inclusive' in style:
                        cleaned_lbl = "o"
                    elif 'exclusive' in style or 'gwType=exclusive' in style or 'gatewayType=exclusive' in style:
                        if 'exclusiveMarked' in style:
                            cleaned_lbl = "x"
            elif ('shape=mxgraph.bpmn.event' in style and ('end' in outline or 'outline=end' in style)) or 'shape=mxgraph.flowchart.terminator' in style or 'terminator' in style:
                shape = 'terminator'
            elif ('shape=mxgraph.bpmn.event' in style and ('standard' in outline or 'outline=standard' in style)) or 'shape=mxgraph.flowchart.start_2' in style or 'start' in style:
                shape = 'start'
            elif 'ellipse' in style:
                shape = 'ellipse'
            elif 'parallelogram' in style:
                shape = 'parallelogram'
                
            nodes[cell_id] = {
                'label': cleaned_lbl,
                'shape': shape,
                'style': style
            }
        elif is_edge:
            source = cell_elem.get('source')
            target = cell_elem.get('target')
            if source and target:
                edges.append({
                    'id': cell_id,
                    'source': source,
                    'target': target,
                    'label': cleaned_lbl
                })

    for obj in root_el.findall('.//object'):
        obj_id = obj.get('id')
        label = obj.get('label', '')
        cell_elem = obj.find('mxCell')
        if obj_id and cell_elem is not None:
            process_cell(obj_id, cell_elem, label)

    for cell in root_el.findall('.//mxCell'):
        cell_id = cell.get('id')
        if not cell_id:
            continue
        if cell_id in nodes or any(e['id'] == cell_id for e in edges):
            continue
        
        label = cell.get('value', '')
        process_cell(cell_id, cell, label)
        
    for cell in root_el.findall('.//mxCell'):
        parent_id = cell.get('parent')
        if parent_id and cell.get('value') and cell.get('vertex') != '1' and cell.get('edge') != '1':
            edge_labels[parent_id] = clean_label(cell.get('value'))
            
    for edge in edges:
        if not edge['label'] and edge['id'] in edge_labels:
            edge['label'] = edge_labels[edge['id']]
            
    return nodes, edges

def generate_mermaid(nodes, edges, max_width=20):
    lines = [
        # Gebruik dubbele aanhalingstekens binnen de directive
        '%%{init: {"theme": "default", "themeVariables": {"fontFamily": "sans-serif"}, "flowchart": {"htmlLabels": false}}}%%',
        "flowchart TD"
    ]
    
    connected_nodes = set()
    for edge in edges:
        connected_nodes.add(edge['source'])
        connected_nodes.add(edge['target'])
    
    for node_id, data in nodes.items():
        label = data['label']
        shape = data['shape']
        style = data.get('style', '')
        
        if not label:
            continue
            
        is_plain_text = 'text' in style or ('strokeColor=none' in style and 'fillColor=none' in style)
        if node_id not in connected_nodes and is_plain_text:
            continue
            
        wrapped_label = wrap_text(label, max_width)
        label_escaped = wrapped_label.replace('"', '\\"')
        
        if shape == 'decision':
            lines.append(f'    {node_id}{{"{label_escaped}"}}')
        elif shape in ('terminator', 'start'):
            lines.append(f'    {node_id}(["{label_escaped}"])')
        elif shape == 'ellipse':
            lines.append(f'    {node_id}(("{label_escaped}"))')
        elif shape == 'parallelogram':
            lines.append(f'    {node_id}[/"{label_escaped}"/]')
        else:
            lines.append(f'    {node_id}["{label_escaped}"]')
            
    for edge in edges:
        source = edge['source']
        target = edge['target']
        label = edge['label']
        
        if source in nodes and target in nodes:
            if label:
                wrapped_label = wrap_text(label, max_width)
                label_escaped = wrapped_label.replace('"', '\\"')
                lines.append(f'    {source} -->|"{label_escaped}"| {target}')
            else:
                lines.append(f'    {source} --> {target}')
                
    return "\n".join(lines)

def main():
    if len(sys.argv) < 2:
        print("Gebruik: python drawio_to_mermaid.py <pad_naar_drawio_bestand>")
        sys.exit(1)
        
    file_path = sys.argv[1]
    if not os.path.exists(file_path):
        print(f"Fout: Bestand '{file_path}' niet gevonden.", file=sys.stderr)
        sys.exit(1)
        
    try:
        model = get_mxgraph_model(file_path)
        nodes, edges = parse_elements(model)
        mermaid_markup = generate_mermaid(nodes, edges)
        print(mermaid_markup)
    except Exception as e:
        print(f"Fout bij verwerking: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()