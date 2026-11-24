#!/usr/bin/env python3
import sys

with open('UnityProject/ProjectSettings/ProjectVersion.txt') as f:
    content = f.read()

if '2022.3.20f1' not in content:
    print('ProjectVersion.txt does not pin 2022.3.20f1')
    sys.exit(1)

if '61c2feb0970d' not in content:
    print('Missing revision 61c2feb0970d')
else:
    print('ProjectVersion.txt correct')

print('Content: ' + content.strip())
