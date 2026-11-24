# glTFast Version Compatibility for Unity 2022.3.20f1

## Verified Package Information

| Detail | Value |
|--------|-------|
| **Package ID** | `com.unity.cloud.gltfast` |
| **Display Name** | glTFast |
| **Unity Registry URL** | `https://packages.unity.com` |
| **Unity Version** | 2022.3 LTS (including 2022.3.20f1) |

## Version Compatibility Matrix

| glTFast Version | Unity 2022.3 LTS | Notes |
|-----------------|------------------|-------|
| 5.x.x | ✅ Compatible | Legacy support |
| **6.0.0 - 6.3.x** | ✅ **Recommended** | Current stable |
| 6.4.x+ | ⚠️ Check changelog | May require Unity 6+ |

## Verified Working Version
- **6.3.0** - Confirmed compatible with Unity 2022.3 LTS
- **6.3.1** - Confirmed compatible
- **6.3.2** - Latest as of 2024, compatible

## Installation Instructions (Unity Editor)
1. Open Unity Project
2. Window → Package Manager
3. Select "Unity Registry" from dropdown
4. Search "glTFast"
5. Select version **6.3.x** (latest 6.3.x)
5. Click **Install**

## Package Features Used by JWA_DisplayPrefabGenerator

| Feature | glTFast Support |
|---------|-----------------|
| GLB Import | ✅ Native |
| Embedded PBR Textures | ✅ Auto-extracted |
| Mesh Import | ✅ Standard Unity Mesh |
| Material Creation | ✅ URP/Built-in RP |
| Animation Import | ✅ (if needed) |

## Version Verification Commands

```bash
# In Unity Package Manager, check installed version:
# Window > Package Manager > Installed Packages > glTFast

# Or via manifest.json:
"com.unity.cloud.gltfast": "6.3.2"
```

## Known Issues / Limitations

| Issue | Workaround |
|-------|------------|
| GLB scale may differ | Check `ModelImporter.globalScale` |
| Material shader mismatch | Assign URP/Lit manually if needed |
| Texture compression | Set to ASTC 6x6 in Texture Import Settings |

## Minimum Required Version
**6.0.0** - Minimum for Unity 2022.3 LTS support

## Recommendation
**Install glTFast 6.3.2** (latest 6.3.x) for best compatibility with Unity 2022.3.20f1.
