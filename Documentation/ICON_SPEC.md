# Icon Specification for John Wick Arsenal

## Main Mod Icon (mod.json → "icon")
- **File**: `icon.png`
- **Size**: 512 × 512 pixels
- **Format**: PNG with transparency
- **Style**: Continental Hotel gold coin / John Wick silhouette
- **Background**: Transparent or dark (#1a1a1a)
- **Color Scheme**: Gold (#D4AF37) on Black/Charcoal

### Design Concept
```
┌─────────────────────────────────────┐
│                                     │
│        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        │
│      ▓▓░░░░░░░░░░░░░░░░░░░░░▓▓      │
│     ▓░░  ▄▄▄▄▄  ░░  ▄▄▄▄▄  ░░░▓     │
│    ▓░░  █  W  █ ░░  █  K  █ ░░░▓    │
│    ▓░░  ▀▀▀▀▀  ░░  ▀▀▀▀▀  ░░░▓     │
│     ▓░░░░░░░░░░░░░░░░░░░░░░░░▓     │
│      ▓▓░░  JOHN WICK  ░░▓▓        │
│       ▓▓░░  ARSENAL  ░░▓▓         │
│         ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        │
│                                     │
└─────────────────────────────────────┘
```

Alternative: Simple gold Continental coin with "JWA" initials

---

## Category Icons (spawnmenu.json)

| Category | Icon File | Concept |
|----------|-----------|---------|
| Pistols | `jwa_pistol_icon.png` | Silhouette of P30L with compensator |
| Shotguns | `jwa_shotgun_icon.png` | Benelli M4 side profile |
| Revolvers | `jwa_revolver_icon.png` | Remington 1875 cylinder view |
| Rifles | `jwa_rifle_icon.png` | TR-1 skeletonized receiver |
| Throwables | `jwa_throwable_icon.png` | Throwing knife + pencil crossed |

**Specs**: 128 × 128 PNG, transparent background, gold (#D4AF37) line art

---

## Weapon Preview Images (Optional)

For spawn menu preview cards:
- **Size**: 256 × 144 (16:9)
- **Format**: PNG/JPG
- **Naming**: `{weapon_id}_preview.png`
- **Content**: In-game render or high-quality model screenshot

Example:
- `jwa_hk_p30l_preview.png`
- `jwa_tti_pit_viper_preview.png`
- `jwa_pencil_preview.png`

---

## Color Palette (John Wick Theme)

| Role | Hex | RGB | Usage |
|------|-----|-----|-------|
| Gold (Primary) | `#D4AF37` | 212, 175, 55 | Accents, text, icons |
| Gold Dark | `#B8962E` | 184, 150, 46 | Shadows, borders |
| Charcoal | `#1A1A1A` | 26, 26, 26 | Backgrounds |
| Gunmetal | `#2C2C2C` | 44, 44, 44 | Panels, cards |
| Blood Red | `#8B0000` | 139, 0, 0 | Danger, pencil kills |
| Continental Gray | `#3D3D3D` | 61, 61, 61 | Secondary UI |
| White Smoke | `#F5F5F5` | 245, 245, 245 | Text on dark |

---

## Asset Creation Checklist

- [ ] `icon.png` (512×512) - Main mod icon
- [ ] `jwa_category_icon.png` (128×128) - Category icon
- [ ] `jwa_pistol_icon.png` (128×128)
- [ ] `jwa_shotgun_icon.png` (128×128)
- [ ] `jwa_revolver_icon.png` (128×128)
- [ ] `jwa_rifle_icon.png` (128×128)
- [ ] `jwa_throwable_icon.png` (128×128)
- [ ] 10 weapon preview images (256×144 each)

---

## Tools for Creation

- **Photoshop / GIMP**: Full control
- **Figma**: Vector-based, easy exports
- **Krita**: Free, good for pixel art
- **Blender**: Render 3D weapon previews
- **Unity**: In-engine screenshots with post-processing

---

## Quick Placeholder Generation (Code)

```csharp
// Generate simple placeholder icons programmatically
Texture2D CreatePlaceholderIcon(string text, Color color, int size = 128)
{
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
    var pixels = new Color[size * size];
    
    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            float dx = (x - size/2f) / (size/2f);
            float dy = (y - size/2f) / (size/2f);
            float dist = Mathf.Sqrt(dx*dx + dy*dy);
            
            if (dist < 0.9f)
                pixels[y * size + x] = new Color(0.1f, 0.1f, 0.1f, 1f);
            else
                pixels[y * size + x] = Color.clear;
        }
    }
    
    tex.SetPixels(pixels);
    tex.Apply();
    return tex;
}
```

---

*Icons are the face of your mod - make them count. The Continental standard demands excellence.* 🏨