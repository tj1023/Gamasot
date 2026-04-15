import os
import re

base_dir = r"E:\UnityProject\Gamasot\Assets\Scripts"

for root, dirs, files in os.walk(base_dir):
    for file in files:
        if file.endswith(".cs"):
            file_path = os.path.join(root, file)
            
            # Determine correct namespace
            rel_path = os.path.relpath(root, base_dir)
            if rel_path == ".":
                continue # No namespace at root or ignore
                
            correct_namespace = rel_path.replace(os.sep, ".")
            
            with open(file_path, "r", encoding="utf-8") as f:
                content = f.read()
                
            new_content = re.sub(r"^namespace\s+[\w\.]+", f"namespace {correct_namespace}", content, flags=re.MULTILINE)
            
            if new_content != content:
                with open(file_path, "w", encoding="utf-8") as f:
                    f.write(new_content)
                print(f"Updated {file_path} to namespace {correct_namespace}")
