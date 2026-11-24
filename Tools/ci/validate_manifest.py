#!/usr/bin/env python3
import json
import sys

def validate_manifest():
    with open('UnityProject/Packages/manifest.json') as f:
        m = json.load(f)
    
    deps = m.get('dependencies', {})
    required = ['com.unity.modules.assetbundle', 'com.unity.xr.oculus', 'com.unity.xr.management']
    
    for req in required:
        if req not in deps:
            print('Missing: ' + req)
        else:
            print('OK: ' + req + ': ' + deps[req])

if __name__ == '__main__':
    validate_manifest()
