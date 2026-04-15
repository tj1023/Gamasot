import os
import re

base_dir = r"E:\UnityProject\Gamasot\Assets\Scripts"

usings_to_add = [
    "using Core;",
    "using Data;",
    "using Gameplay.Commands;",
    "using Gameplay.Synergies;",
    "using Gameplay.Systems;",
    "using UI;"
]

for root, dirs, files in os.walk(base_dir):
    for file in files:
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            
            with open(file_path, "r", encoding="utf-8") as f:
                content = f.read()
            
            # Find all existing usings
            existing_usings = re.findall(r"^using\s+[\w\.]+;", content, re.MULTILINE)
            
            # Determine which ones to add
            # Note: A file shouldn't necessarily include its own namespace as a using, but it's harmless.
            missing_usings = [u for u in usings_to_add if u not in existing_usings]
            
            if missing_usings:
                # Find the position after the last using statement, or at the very beginning
                last_using_match = list(re.finditer(r"^using\s+[\w\.]+;\s*", content, re.MULTILINE))
                
                new_usings_str = "\n".join(missing_usings) + "\n"
                
                if last_using_match:
                    insert_pos = last_using_match[-1].end()
                    new_content = content[:insert_pos] + new_usings_str + content[insert_pos:]
                else:
                    new_content = new_usings_str + content
                    
                with open(file_path, "w", encoding="utf-8") as f:
                    f.write(new_content)
                print(f"Added usings to {file}")
