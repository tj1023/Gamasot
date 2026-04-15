import os
import glob
import re

# 1. Load all original files
original_content = ''
for f in glob.glob('../temp_gamasot/Assets/Scripts/**/*.cs', recursive=True):
    with open(f, 'r', encoding='utf-8-sig') as file:
        original_content += file.read() + '\n\n'

original_content = original_content.replace('IngredientNode', 'IngredientEntity')

def extract_body(content, type_keyword, name):
    # Regex to find: (comments/attributes) (modifiers) type_keyword name (base_classes) {
    # It stops at the '{'
    pattern = r'((?:[ \t]*(?:\/\/\/.*|\[.*?\])\s*\n)*)[ \t]*(?:public|private|protected|internal|)[ \t]*(?:static|abstract|sealed|partial|)[ \t]*' + type_keyword + r'[ \t]+' + re.escape(name) + r'(?:[ \t]*[:<][^\{]*)?\s*\{'
    
    match = re.search(pattern, content, re.MULTILINE)
    if not match:
        return None, None, None
        
    start_idx = match.start()
    
    brace_count = 0
    idx = match.end() - 1
    in_string = False
    in_char = False
    in_line_comment = False
    in_block_comment = False
    
    for i in range(idx, len(content)):
        c = content[i]
        
        if in_string:
            if c == '"' and content[i-1] != '\\': in_string = False
            continue
        if in_char:
            if c == "'" and content[i-1] != '\\': in_char = False
            continue
        if in_line_comment:
            if c == '\n': in_line_comment = False
            continue
        if in_block_comment:
            if c == '/' and content[i-1] == '*': in_block_comment = False
            continue
            
        if c == '"': in_string = True
        elif c == "'": in_char = True
        elif c == '/' and i+1 < len(content) and content[i+1] == '/': in_line_comment = True
        elif c == '/' and i+1 < len(content) and content[i+1] == '*': in_block_comment = True
        elif c == '{': brace_count += 1
        elif c == '}':
            brace_count -= 1
            if brace_count == 0:
                end_idx = i + 1
                return content[start_idx:end_idx], start_idx, end_idx
                
    return None, None, None

for f in glob.glob('Assets/Scripts/**/*.cs', recursive=True):
    with open(f, 'r', encoding='utf-8') as file:
        content = file.read()
        
    types = re.findall(r'(?:public|private|protected|internal|)[ \t]*(?:static|abstract|sealed|partial|)[ \t]*(class|interface|enum|struct)[ \t]+([a-zA-Z0-9_]+)', content)
    
    new_content = content
    # process in reverse order to not mess up indices if we do replacements based on index
    # wait, we can just replace by string if they are unique, or use indices.
    # actually string replace is risky if there are multiple identical types (there shouldn't be)
    
    changed = False
    for type_keyword, name in types:
        orig_block, _, _ = extract_body(original_content, type_keyword, name)
        if orig_block:
            curr_block, start_idx, end_idx = extract_body(new_content, type_keyword, name)
            if curr_block and orig_block != curr_block:
                new_content = new_content[:start_idx] + orig_block + new_content[end_idx:]
                changed = True
                
    if changed:
        new_content = new_content.replace('Gameplay.RuntimeIngredient', 'RuntimeIngredient')
        with open(f, 'w', encoding='utf-8', newline='\n') as file:
            file.write(new_content)
        print(f'Restored {f}')
