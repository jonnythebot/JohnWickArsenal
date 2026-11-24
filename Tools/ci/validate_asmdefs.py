#!/usr/bin/env python3
import json
import sys

errors = 0
for asmdef in [
    'UnityProject/Assets/JohnWickArsenal/Editor/JohnWickArsenal.Cosmetic.Editor.asmdef',
    'UnityProject/Assets/JohnWickArsenal/Editor/JohnWickArsenal.Legacy.Pipeline.Editor.asmdef',
    'UnityProject/Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef',
    'UnityProject/Assets/Scripts/JohnWickArsenal/Tests/JohnWickArsenal.Tests.asmdef']:
    with open(asmdef) as f:
        content = json.load(f)
    if 'name' not in content:
        print('Missing name: ' + asmdef)
        errors += 1
    else:
        print('OK: ' + asmdef + ': ' + content['name'])
    if 'Cosmetic.Editor' in content.get('name', '') and content.get('references'):
        print('Cosmetic asmdef has references: ' + str(content['references']))

sys.exit(errors)
