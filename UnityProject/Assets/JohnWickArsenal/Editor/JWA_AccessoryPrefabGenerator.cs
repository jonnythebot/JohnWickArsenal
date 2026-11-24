using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using JohnWickArsenal.Accessories;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Generates all cosmetic accessory prefabs procedurally using primitive geometry.
    /// Run via: Tools > John Wick Arsenal > Generate Accessory Prefabs
    /// </summary>
    public class JWA_AccessoryPrefabGenerator : EditorWindow
    {
        [MenuItem("Tools/John Wick Arsenal/Generate Accessory Prefabs")]
        public static void GenerateAllAccessories()
        {
            string basePath = "Assets/JohnWickArsenal/Prefabs/Accessories";

            // Create directory structure
            CreateDirectories(basePath);

            // Create shared materials
            var materials = CreateSharedMaterials(basePath + "/Materials");

            int created = 0;

            // 1. Flashlights (4 variants)
            created += CreateFlashlights(basePath + "/Flashlights", materials);

            // 2. Laser Modules (4 variants)
            created += CreateLaserModules(basePath + "/LaserModules", materials);

            // 3. Grips (5 variants)
            created += CreateGrips(basePath + "/Grips", materials);

            // 4. Sights (7 variants)
            created += CreateSights(basePath + "/Sights", materials);

            // 5. Muzzle Devices (5 variants)
            created += CreateMuzzleDevices(basePath + "/MuzzleDevices", materials);

            // 6. Rail Covers (4 variants)
            created += CreateRailCovers(basePath + "/RailCovers", materials);

            // 7. Sling Mounts (3 variants)
            created += CreateSlingMounts(basePath + "/SlingMounts", materials);

            // 8. Magwell & Charging Handles (4 variants)
            created += CreateMagwellCharging(basePath + "/MagwellCharging", materials);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[JWA_AccessoryPrefabGenerator] Created {created} accessory prefabs at {basePath}");
            EditorUtility.DisplayDialog("Complete", $"Generated {created} accessory prefabs!", "OK");
        }

        static void CreateDirectories(string basePath)
        {
            string[] dirs = new[]
            {
                basePath + "/Flashlights",
                basePath + "/LaserModules",
                basePath + "/Grips",
                basePath + "/Sights",
                basePath + "/MuzzleDevices",
                basePath + "/RailCovers",
                basePath + "/SlingMounts",
                basePath + "/MagwellCharging",
                basePath + "/Materials"
            };

            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    var parent = Path.GetDirectoryName(dir);
                    var name = Path.GetFileName(dir);
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        static Dictionary<string, Material> CreateSharedMaterials(string materialPath)
        {
            var mats = new Dictionary<string, Material>();

            // Anodized Aluminum (dark gray, metallic)
            mats["Aluminum"] = CreateMaterial(materialPath, "Mat_Aluminum",
                new Color(0.18f, 0.18f, 0.19f), 0.9f, 0.3f, true);

            // Polymer Black (near black, non-metallic)
            mats["Polymer"] = CreateMaterial(materialPath, "Mat_Polymer",
                new Color(0.05f, 0.05f, 0.06f), 0.0f, 0.1f, false);

            // Glass Lens (transparent)
            mats["Glass"] = CreateMaterial(materialPath, "Mat_Glass",
                new Color(0.1f, 0.2f, 0.4f, 0.3f), 0.0f, 1.0f, false, true);

            // Steel (muzzle devices)
            mats["Steel"] = CreateMaterial(materialPath, "Mat_Steel",
                new Color(0.12f, 0.12f, 0.13f), 0.8f, 0.2f, true);

            // Rubber (grips)
            mats["Rubber"] = CreateMaterial(materialPath, "Mat_Rubber",
                new Color(0.08f, 0.08f, 0.09f), 0.0f, 0.05f, false);

            // Lens Red (laser)
            mats["LensRed"] = CreateMaterial(materialPath, "Mat_LensRed",
                new Color(1f, 0f, 0f, 0.5f), 0.0f, 1.0f, false, true);

            // Lens Green (laser)
            mats["LensGreen"] = CreateMaterial(materialPath, "Mat_LensGreen",
                new Color(0f, 1f, 0.2f, 0.5f), 0.0f, 1.0f, false, true);

            // Lens IR (invisible but marked)
            mats["LensIR"] = CreateMaterial(materialPath, "Mat_LensIR",
                new Color(0.5f, 0f, 0.5f, 0.3f), 0.0f, 1.0f, false, true);

            // Sight Glass (clear with slight tint)
            mats["SightGlass"] = CreateMaterial(materialPath, "Mat_SightGlass",
                new Color(0.9f, 0.9f, 0.95f, 0.15f), 0.0f, 0.95f, false, true);

            // Reticle Red (for red dot sights)
            mats["ReticleRed"] = CreateMaterial(materialPath, "Mat_ReticleRed",
                new Color(1f, 0.1f, 0.1f, 0.8f), 0.0f, 1.0f, false, true);

            // Reticle Green (for holographic)
            mats["ReticleGreen"] = CreateMaterial(materialPath, "Mat_ReticleGreen",
                new Color(0.2f, 1f, 0.2f, 0.7f), 0.0f, 1.0f, false, true);

            return mats;
        }

        static Material CreateMaterial(string path, string name, Color albedo, float metallic, float smoothness, bool useNormal = false, bool transparent = false)
        {
            string fullPath = $"{path}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (existing != null) return existing;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.color = albedo;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);   // Alpha
                mat.SetFloat("_ZWrite", 0);
                mat.renderQueue = 3000;
            }

            AssetDatabase.CreateAsset(mat, fullPath);
            return mat;
        }

        // =====================================================================
        // FLASHLIGHTS
        // =====================================================================

        static int CreateFlashlights(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Tactical_Standard - Full size weapon light
            count += CreateAccessory(path, "JWA_Accessory_Flashlight_Tactical_Standard",
                JWA_AccessoryBase.AccessoryType.Flashlight, "Tactical_Standard",
                "Standard tactical weapon light", new[] { "Picatinny" }, false, 480,
                (root) => BuildFlashlightStandard(root, mats));

            // Tactical_Compact - Compact for pistols
            count += CreateAccessory(path, "JWA_Accessory_Flashlight_Tactical_Compact",
                JWA_AccessoryBase.AccessoryType.Flashlight, "Tactical_Compact",
                "Compact tactical light for pistols", new[] { "Picatinny" }, false, 280,
                (root) => BuildFlashlightCompact(root, mats));

            // Tactical_IR - IR illuminator
            count += CreateAccessory(path, "JWA_Accessory_Flashlight_Tactical_IR",
                JWA_AccessoryBase.AccessoryType.Flashlight, "Tactical_IR",
                "IR illuminator for NVG use", new[] { "Picatinny" }, false, 480,
                (root) => BuildFlashlightIR(root, mats));

            // Pressure_Switch - Remote pressure switch
            count += CreateAccessory(path, "JWA_Accessory_Flashlight_Pressure_Switch",
                JWA_AccessoryBase.AccessoryType.Flashlight, "Pressure_Switch",
                "Remote pressure switch (cosmetic)", new[] { "Picatinny" }, false, 180,
                (root) => BuildPressureSwitch(root, mats));

            return count;
        }

        static void BuildFlashlightStandard(GameObject root, Dictionary<string, Material> mats)
        {
            // Main body - cylinder
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.032f, 0.09f, 0.032f); // ~32mm dia, 90mm long
            body.transform.localPosition = new Vector3(0, 0.045f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front lens
            var lens = CreatePrimitive(root, "Lens", PrimitiveType.Cylinder);
            lens.transform.localScale = new Vector3(0.03f, 0.002f, 0.03f);
            lens.transform.localPosition = new Vector3(0, 0.091f, 0);
            lens.GetComponent<Renderer>().sharedMaterial = mats["Glass"];

            // Rear cap
            var cap = CreatePrimitive(root, "RearCap", PrimitiveType.Cylinder);
            cap.transform.localScale = new Vector3(0.034f, 0.008f, 0.034f);
            cap.transform.localPosition = new Vector3(0, -0.004f, 0);
            cap.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount base (Picatinny clamp)
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.045f, 0.018f, 0.038f);
            mount.transform.localPosition = new Vector3(0, -0.015f, 0.005f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screw
            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.006f, 0.012f, 0.006f);
            screw.transform.localPosition = new Vector3(0.015f, -0.015f, 0.005f);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Activation switch
            var switchObj = CreatePrimitive(root, "Switch", PrimitiveType.Cube);
            switchObj.transform.localScale = new Vector3(0.012f, 0.006f, 0.008f);
            switchObj.transform.localPosition = new Vector3(-0.022f, 0.07f, 0);
            switchObj.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
        }

        static void BuildFlashlightCompact(GameObject root, Dictionary<string, Material> mats)
        {
            // Main body - shorter cylinder
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.028f, 0.055f, 0.028f);
            body.transform.localPosition = new Vector3(0, 0.0275f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front lens
            var lens = CreatePrimitive(root, "Lens", PrimitiveType.Cylinder);
            lens.transform.localScale = new Vector3(0.026f, 0.002f, 0.026f);
            lens.transform.localPosition = new Vector3(0, 0.056f, 0);
            lens.GetComponent<Renderer>().sharedMaterial = mats["Glass"];

            // Rear cap
            var cap = CreatePrimitive(root, "RearCap", PrimitiveType.Cylinder);
            cap.transform.localScale = new Vector3(0.03f, 0.006f, 0.03f);
            cap.transform.localPosition = new Vector3(0, -0.003f, 0);
            cap.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.038f, 0.015f, 0.032f);
            mount.transform.localPosition = new Vector3(0, -0.012f, 0.004f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screw
            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
            screw.transform.localPosition = new Vector3(0.012f, -0.012f, 0.004f);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildFlashlightIR(GameObject root, Dictionary<string, Material> mats)
        {
            // Similar to standard but with IR marker
            BuildFlashlightStandard(root, mats);

            // IR filter cover (purple tint)
            var irFilter = CreatePrimitive(root, "IR_Filter", PrimitiveType.Cylinder);
            irFilter.transform.localScale = new Vector3(0.028f, 0.003f, 0.028f);
            irFilter.transform.localPosition = new Vector3(0, 0.092f, 0);
            irFilter.GetComponent<Renderer>().sharedMaterial = mats["LensIR"];

            // IR label marking
            var label = CreatePrimitive(root, "IR_Label", PrimitiveType.Cube);
            label.transform.localScale = new Vector3(0.015f, 0.001f, 0.008f);
            label.transform.localPosition = new Vector3(0, 0.06f, 0.02f);
            label.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
        }

        static void BuildPressureSwitch(GameObject root, Dictionary<string, Material> mats)
        {
            // Main switch body
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.035f, 0.012f, 0.022f);
            body.transform.localPosition = new Vector3(0, 0.006f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Pressure pad
            var pad = CreatePrimitive(root, "PressurePad", PrimitiveType.Cube);
            pad.transform.localScale = new Vector3(0.03f, 0.004f, 0.018f);
            pad.transform.localPosition = new Vector3(0, 0.014f, 0);
            pad.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];

            // Cable exit
            var cable = CreatePrimitive(root, "Cable", PrimitiveType.Cylinder);
            cable.transform.localScale = new Vector3(0.003f, 0.02f, 0.003f);
            cable.transform.localPosition = new Vector3(-0.017f, -0.005f, 0);
            cable.transform.localRotation = Quaternion.Euler(0, 0, 90);
            cable.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.04f, 0.01f, 0.025f);
            mount.transform.localPosition = new Vector3(0, -0.01f, 0.003f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];
        }

        // =====================================================================
        // LASER MODULES
        // =====================================================================

        static int CreateLaserModules(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Visible_Red - Red laser diode
            count += CreateAccessory(path, "JWA_Accessory_LaserModule_Visible_Red",
                JWA_AccessoryBase.AccessoryType.LaserModule, "Visible_Red",
                "Visible red laser diode housing", new[] { "Picatinny" }, false, 280,
                (root) => BuildLaserVisible(root, mats, "LensRed"));

            // Visible_Green - Green laser diode
            count += CreateAccessory(path, "JWA_Accessory_LaserModule_Visible_Green",
                JWA_AccessoryBase.AccessoryType.LaserModule, "Visible_Green",
                "Visible green laser diode housing", new[] { "Picatinny" }, false, 280,
                (root) => BuildLaserVisible(root, mats, "LensGreen"));

            // IR_PEQ - PEQ-style box
            count += CreateAccessory(path, "JWA_Accessory_LaserModule_IR_PEQ",
                JWA_AccessoryBase.AccessoryType.LaserModule, "IR_PEQ",
                "PEQ-style IR laser/illuminator housing", new[] { "Picatinny" }, false, 480,
                (root) => BuildLaserPEQ(root, mats));

            // DBAL_Combo - Combo housing
            count += CreateAccessory(path, "JWA_Accessory_LaserModule_DBAL_Combo",
                JWA_AccessoryBase.AccessoryType.LaserModule, "DBAL_Combo",
                "DBAL-style visible/IR combo housing", new[] { "Picatinny" }, false, 580,
                (root) => BuildLaserDBAL(root, mats));

            return count;
        }

        static void BuildLaserVisible(GameObject root, Dictionary<string, Material> mats, string lensMat)
        {
            // Main housing - rectangular box
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.032f, 0.028f, 0.055f);
            body.transform.localPosition = new Vector3(0, 0.014f, 0.01f);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front laser aperture
            var aperture = CreatePrimitive(root, "Aperture", PrimitiveType.Cylinder);
            aperture.transform.localScale = new Vector3(0.008f, 0.003f, 0.008f);
            aperture.transform.localPosition = new Vector3(0, 0.014f, 0.0385f);
            aperture.transform.localRotation = Quaternion.Euler(90, 0, 0);
            aperture.GetComponent<Renderer>().sharedMaterial = mats[lensMat];

            // Rear adjustment turrets
            var turret1 = CreatePrimitive(root, "WindageTurret", PrimitiveType.Cylinder);
            turret1.transform.localScale = new Vector3(0.01f, 0.008f, 0.01f);
            turret1.transform.localPosition = new Vector3(0.016f, 0.018f, -0.018f);
            turret1.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            var turret2 = CreatePrimitive(root, "ElevationTurret", PrimitiveType.Cylinder);
            turret2.transform.localScale = new Vector3(0.01f, 0.008f, 0.01f);
            turret2.transform.localPosition = new Vector3(0, 0.026f, -0.018f);
            turret2.transform.localRotation = Quaternion.Euler(90, 0, 0);
            turret2.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.04f, 0.012f, 0.035f);
            mount.transform.localPosition = new Vector3(0, -0.006f, 0.002f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screw
            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
            screw.transform.localPosition = new Vector3(0.014f, -0.006f, 0.002f);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildLaserPEQ(GameObject root, Dictionary<string, Material> mats)
        {
            // Main box body (PEQ-15 style)
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.048f, 0.038f, 0.085f);
            body.transform.localPosition = new Vector3(0, 0.019f, 0.02f);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front dual apertures (laser + illuminator)
            var ap1 = CreatePrimitive(root, "LaserAperture", PrimitiveType.Cylinder);
            ap1.transform.localScale = new Vector3(0.01f, 0.003f, 0.01f);
            ap1.transform.localPosition = new Vector3(-0.012f, 0.019f, 0.0625f);
            ap1.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ap1.GetComponent<Renderer>().sharedMaterial = mats["LensIR"];

            var ap2 = CreatePrimitive(root, "IllumAperture", PrimitiveType.Cylinder);
            ap2.transform.localScale = new Vector3(0.014f, 0.003f, 0.014f);
            ap2.transform.localPosition = new Vector3(0.012f, 0.019f, 0.0625f);
            ap2.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ap2.GetComponent<Renderer>().sharedMaterial = mats["Glass"];

            // Mode selector switch
            var selector = CreatePrimitive(root, "ModeSelector", PrimitiveType.Cube);
            selector.transform.localScale = new Vector3(0.018f, 0.01f, 0.012f);
            selector.transform.localPosition = new Vector3(0, 0.035f, -0.02f);
            selector.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Rear battery cap
            var cap = CreatePrimitive(root, "BatteryCap", PrimitiveType.Cylinder);
            cap.transform.localScale = new Vector3(0.028f, 0.01f, 0.028f);
            cap.transform.localPosition = new Vector3(0, 0.019f, -0.035f);
            cap.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount (PEQ uses throw-lever mount)
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.05f, 0.015f, 0.04f);
            mount.transform.localPosition = new Vector3(0, -0.006f, 0.005f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            var lever = CreatePrimitive(root, "ThrowLever", PrimitiveType.Cube);
            lever.transform.localScale = new Vector3(0.01f, 0.025f, 0.008f);
            lever.transform.localPosition = new Vector3(0.025f, 0.006f, 0.005f);
            lever.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildLaserDBAL(GameObject root, Dictionary<string, Material> mats)
        {
            // Main housing (DBAL-A3 style - taller)
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.042f, 0.055f, 0.075f);
            body.transform.localPosition = new Vector3(0, 0.0275f, 0.015f);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Top visible laser
            var visAp = CreatePrimitive(root, "VisibleAperture", PrimitiveType.Cylinder);
            visAp.transform.localScale = new Vector3(0.008f, 0.003f, 0.008f);
            visAp.transform.localPosition = new Vector3(0, 0.0555f, 0.0525f);
            visAp.transform.localRotation = Quaternion.Euler(90, 0, 0);
            visAp.GetComponent<Renderer>().sharedMaterial = mats["LensRed"];

            // Bottom IR laser
            var irAp = CreatePrimitive(root, "IRAperture", PrimitiveType.Cylinder);
            irAp.transform.localScale = new Vector3(0.008f, 0.003f, 0.008f);
            irAp.transform.localPosition = new Vector3(0, 0.002f, 0.0525f);
            irAp.transform.localRotation = Quaternion.Euler(90, 0, 0);
            irAp.GetComponent<Renderer>().sharedMaterial = mats["LensIR"];

            // Side IR illuminator
            var illum = CreatePrimitive(root, "Illuminator", PrimitiveType.Cylinder);
            illum.transform.localScale = new Vector3(0.012f, 0.003f, 0.012f);
            illum.transform.localPosition = new Vector3(0.023f, 0.0275f, 0.0525f);
            illum.transform.localRotation = Quaternion.Euler(0, 90, 0);
            illum.GetComponent<Renderer>().sharedMaterial = mats["Glass"];

            // Dual mode selectors
            var sel1 = CreatePrimitive(root, "Selector_Vis", PrimitiveType.Cube);
            sel1.transform.localScale = new Vector3(0.014f, 0.008f, 0.01f);
            sel1.transform.localPosition = new Vector3(-0.023f, 0.045f, -0.02f);
            sel1.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            var sel2 = CreatePrimitive(root, "Selector_IR", PrimitiveType.Cube);
            sel2.transform.localScale = new Vector3(0.014f, 0.008f, 0.01f);
            sel2.transform.localPosition = new Vector3(-0.023f, 0.015f, -0.02f);
            sel2.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Mount
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.045f, 0.012f, 0.04f);
            mount.transform.localPosition = new Vector3(0, -0.006f, 0.002f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];
        }

        // =====================================================================
        // GRIPS
        // =====================================================================

        static int CreateGrips(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Vertical_Tactical
            count += CreateAccessory(path, "JWA_Accessory_Grip_Vertical_Tactical",
                JWA_AccessoryBase.AccessoryType.Grip, "Vertical_Tactical",
                "Vertical foregrip, tactical style", new[] { "Picatinny", "M-LOK" }, false, 380,
                (root) => BuildGripVertical(root, mats));

            // Angled_Magpul
            count += CreateAccessory(path, "JWA_Accessory_Grip_Angled_Magpul",
                JWA_AccessoryBase.AccessoryType.Grip, "Angled_Magpul",
                "Angled foregrip (Magpul AFG style)", new[] { "Picatinny", "M-LOK" }, false, 380,
                (root) => BuildGripAngled(root, mats));

            // Hand_Stop
            count += CreateAccessory(path, "JWA_Accessory_Grip_Hand_Stop",
                JWA_AccessoryBase.AccessoryType.Grip, "Hand_Stop",
                "Barricade hand stop", new[] { "Picatinny", "M-LOK" }, false, 180,
                (root) => BuildGripHandStop(root, mats));

            // Pistol_Stippled
            count += CreateAccessory(path, "JWA_Accessory_Grip_Pistol_Stippled",
                JWA_AccessoryBase.AccessoryType.Grip, "Pistol_Stippled",
                "Stippled pistol grip texture", new[] { "Frame" }, true, 280,
                (root) => BuildGripPistolStippled(root, mats));

            // Pistol_Rubberized
            count += CreateAccessory(path, "JWA_Accessory_Grip_Pistol_Rubberized",
                JWA_AccessoryBase.AccessoryType.Grip, "Pistol_Rubberized",
                "Rubberized pistol grip", new[] { "Frame" }, true, 280,
                (root) => BuildGripPistolRubberized(root, mats));

            return count;
        }

        static void BuildGripVertical(GameObject root, Dictionary<string, Material> mats)
        {
            // Main grip body - angled cylinder
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.036f, 0.11f, 0.036f);
            body.transform.localPosition = new Vector3(0, 0.055f, -0.01f);
            body.transform.localRotation = Quaternion.Euler(15, 0, 0); // Slight angle
            body.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Texture ridges (simulated with thin rings)
            for (int i = 0; i < 5; i++)
            {
                var ridge = CreatePrimitive(root, $"Ridge_{i}", PrimitiveType.Cylinder);
                ridge.transform.localScale = new Vector3(0.037f, 0.003f, 0.037f);
                ridge.transform.localPosition = new Vector3(0, 0.015f + i * 0.02f, -0.01f);
                ridge.transform.localRotation = Quaternion.Euler(15, 0, 0);
                ridge.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];
            }

            // Bottom cap
            var cap = CreatePrimitive(root, "BottomCap", PrimitiveType.Cylinder);
            cap.transform.localScale = new Vector3(0.038f, 0.006f, 0.038f);
            cap.transform.localPosition = new Vector3(0, -0.002f, -0.01f);
            cap.transform.localRotation = Quaternion.Euler(15, 0, 0);
            cap.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.042f, 0.015f, 0.025f);
            mount.transform.localPosition = new Vector3(0, -0.01f, 0.01f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screws (2)
            for (int i = 0; i < 2; i++)
            {
                var screw = CreatePrimitive(root, $"MountScrew_{i}", PrimitiveType.Cylinder);
                screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
                screw.transform.localPosition = new Vector3(i == 0 ? -0.012f : 0.012f, -0.01f, 0.01f);
                screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
                screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }
        }

        static void BuildGripAngled(GameObject root, Dictionary<string, Material> mats)
        {
            // AFG-style angled grip - main body
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.04f, 0.035f, 0.085f);
            body.transform.localPosition = new Vector3(0, 0.03f, -0.005f);
            body.transform.localRotation = Quaternion.Euler(-25, 0, 0); // Angled back
            body.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Finger groove
            var groove = CreatePrimitive(root, "FingerGroove", PrimitiveType.Cube);
            groove.transform.localScale = new Vector3(0.032f, 0.008f, 0.07f);
            groove.transform.localPosition = new Vector3(0, 0.045f, -0.005f);
            groove.transform.localRotation = Quaternion.Euler(-25, 0, 0);
            groove.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];

            // Rear support
            var support = CreatePrimitive(root, "RearSupport", PrimitiveType.Cube);
            support.transform.localScale = new Vector3(0.038f, 0.04f, 0.015f);
            support.transform.localPosition = new Vector3(0, 0.01f, -0.045f);
            support.transform.localRotation = Quaternion.Euler(-25, 0, 0);
            support.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.042f, 0.012f, 0.03f);
            mount.transform.localPosition = new Vector3(0, -0.008f, 0.01f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screws
            for (int i = 0; i < 2; i++)
            {
                var screw = CreatePrimitive(root, $"MountScrew_{i}", PrimitiveType.Cylinder);
                screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
                screw.transform.localPosition = new Vector3(i == 0 ? -0.012f : 0.012f, -0.008f, 0.01f);
                screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
                screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }
        }

        static void BuildGripHandStop(GameObject root, Dictionary<string, Material> mats)
        {
            // Low profile barricade stop
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.038f, 0.018f, 0.022f);
            body.transform.localPosition = new Vector3(0, 0.009f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Front angled face
            var front = CreatePrimitive(root, "FrontFace", PrimitiveType.Cube);
            front.transform.localScale = new Vector3(0.036f, 0.016f, 0.004f);
            front.transform.localPosition = new Vector3(0, 0.009f, 0.013f);
            front.transform.localRotation = Quaternion.Euler(-30, 0, 0);
            front.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];

            // Mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.04f, 0.01f, 0.025f);
            mount.transform.localPosition = new Vector3(0, -0.005f, 0.002f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Single screw
            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.005f, 0.008f, 0.005f);
            screw.transform.localPosition = new Vector3(0, -0.005f, 0.002f);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildGripPistolStippled(GameObject root, Dictionary<string, Material> mats)
        {
            // Pistol grip shape (frame-integrated style)
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.032f, 0.095f, 0.028f);
            body.transform.localPosition = new Vector3(0, 0.04f, -0.005f);
            body.transform.localRotation = Quaternion.Euler(-15, 0, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Stipple texture (small cubes)
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    var stipple = CreatePrimitive(root, $"Stipple_{y}_{z}", PrimitiveType.Cube);
                    stipple.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
                    stipple.transform.localPosition = new Vector3(
                        0.016f,
                        0.01f + y * 0.022f,
                        -0.005f + (z == 0 ? -0.008f : 0.008f)
                    );
                    stipple.transform.localRotation = Quaternion.Euler(-15, 0, 0);
                    stipple.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];
                }
            }

            // Backstrap
            var backstrap = CreatePrimitive(root, "Backstrap", PrimitiveType.Cube);
            backstrap.transform.localScale = new Vector3(0.03f, 0.08f, 0.006f);
            backstrap.transform.localPosition = new Vector3(0, 0.04f, -0.019f);
            backstrap.transform.localRotation = Quaternion.Euler(-15, 0, 0);
            backstrap.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];

            // Beavertail
            var beavertail = CreatePrimitive(root, "Beavertail", PrimitiveType.Cube);
            beavertail.transform.localScale = new Vector3(0.034f, 0.02f, 0.012f);
            beavertail.transform.localPosition = new Vector3(0, 0.09f, -0.005f);
            beavertail.transform.localRotation = Quaternion.Euler(-15, 0, 0);
            beavertail.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
        }

        static void BuildGripPistolRubberized(GameObject root, Dictionary<string, Material> mats)
        {
            // Similar shape but rubberized
            BuildGripPistolStippled(root, mats);

            // Override materials to rubber
            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.sharedMaterial == mats["Polymer"])
                    r.sharedMaterial = mats["Rubber"];
            }

            // Add finger grooves
            for (int i = 0; i < 3; i++)
            {
                var groove = CreatePrimitive(root, $"FingerGroove_{i}", PrimitiveType.Cylinder);
                groove.transform.localScale = new Vector3(0.008f, 0.004f, 0.022f);
                groove.transform.localPosition = new Vector3(0, 0.02f + i * 0.025f, 0.012f);
                groove.transform.localRotation = Quaternion.Euler(-15, 90, 0);
                groove.GetComponent<Renderer>().sharedMaterial = mats["Rubber"];
            }
        }

        // =====================================================================
        // SIGHTS
        // =====================================================================

        static int CreateSights(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // RMR_Type2
            count += CreateAccessory(path, "JWA_Accessory_Sight_RMR_Type2",
                JWA_AccessoryBase.AccessoryType.Sight, "RMR_Type2",
                "Trijicon RMR Type 2 red dot housing", new[] { "Picatinny", "RMR" }, false, 380,
                (root) => BuildSightRMR(root, mats, false));

            // RMRcc
            count += CreateAccessory(path, "JWA_Accessory_Sight_RMRcc",
                JWA_AccessoryBase.AccessoryType.Sight, "RMRcc",
                "Compact RMR for pistols", new[] { "RMRcc" }, false, 280,
                (root) => BuildSightRMRcc(root, mats));

            // EOTech_EXPS3
            count += CreateAccessory(path, "JWA_Accessory_Sight_EOTech_EXPS3",
                JWA_AccessoryBase.AccessoryType.Sight, "EOTech_EXPS3",
                "EOTech EXPS3 holographic sight housing", new[] { "Picatinny" }, false, 480,
                (root) => BuildSightEOTech(root, mats));

            // Aimpoint_T2
            count += CreateAccessory(path, "JWA_Accessory_Sight_Aimpoint_T2",
                JWA_AccessoryBase.AccessoryType.Sight, "Aimpoint_T2",
                "Aimpoint Micro T-2 red dot housing", new[] { "Picatinny" }, false, 380,
                (root) => BuildSightAimpoint(root, mats));

            // Magnifier_G33
            count += CreateAccessory(path, "JWA_Accessory_Sight_Magnifier_G33",
                JWA_AccessoryBase.AccessoryType.Sight, "Magnifier_G33",
                "G33-style flip-to-side magnifier housing", new[] { "Picatinny" }, false, 480,
                (root) => BuildSightMagnifier(root, mats));

            // BUIS_Front
            count += CreateAccessory(path, "JWA_Accessory_Sight_BUIS_Front",
                JWA_AccessoryBase.AccessoryType.Sight, "BUIS_Front",
                "Backup front sight (folding)", new[] { "Picatinny" }, false, 180,
                (root) => BuildSightBUISFront(root, mats));

            // BUIS_Rear
            count += CreateAccessory(path, "JWA_Accessory_Sight_BUIS_Rear",
                JWA_AccessoryBase.AccessoryType.Sight, "BUIS_Rear",
                "Backup rear sight (folding)", new[] { "Picatinny" }, false, 180,
                (root) => BuildSightBUISRear(root, mats));

            return count;
        }

        static void BuildSightRMR(GameObject root, Dictionary<string, Material> mats, bool compact)
        {
            float scale = compact ? 0.8f : 1.0f;

            // Main housing
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.026f * scale, 0.018f * scale, 0.032f * scale);
            body.transform.localPosition = new Vector3(0, 0.009f * scale, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front lens
            var lens = CreatePrimitive(root, "FrontLens", PrimitiveType.Cube);
            lens.transform.localScale = new Vector3(0.02f * scale, 0.012f * scale, 0.002f);
            lens.transform.localPosition = new Vector3(0, 0.009f * scale, 0.017f * scale);
            lens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Rear lens
            var rearLens = CreatePrimitive(root, "RearLens", PrimitiveType.Cube);
            rearLens.transform.localScale = new Vector3(0.02f * scale, 0.012f * scale, 0.002f);
            rearLens.transform.localPosition = new Vector3(0, 0.009f * scale, -0.017f * scale);
            rearLens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Reticle (red dot)
            var reticle = CreatePrimitive(root, "Reticle", PrimitiveType.Cube);
            reticle.transform.localScale = new Vector3(0.002f * scale, 0.002f * scale, 0.001f);
            reticle.transform.localPosition = new Vector3(0, 0.009f * scale, 0);
            reticle.GetComponent<Renderer>().sharedMaterial = mats["ReticleRed"];

            // Brightness adjustment
            var adj = CreatePrimitive(root, "BrightnessAdj", PrimitiveType.Cylinder);
            adj.transform.localScale = new Vector3(0.005f * scale, 0.006f * scale, 0.005f * scale);
            adj.transform.localPosition = new Vector3(0.013f * scale, 0.009f * scale, -0.01f * scale);
            adj.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount base (Picatinny or RMR cut)
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.028f * scale, 0.008f, 0.03f * scale);
            mount.transform.localPosition = new Vector3(0, -0.004f, 0);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount screws (2 for RMR pattern)
            for (int i = 0; i < 2; i++)
            {
                var screw = CreatePrimitive(root, $"MountScrew_{i}", PrimitiveType.Cylinder);
                screw.transform.localScale = new Vector3(0.003f * scale, 0.006f, 0.003f * scale);
                screw.transform.localPosition = new Vector3(i == 0 ? -0.009f * scale : 0.009f * scale, -0.004f, 0);
                screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
                screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }
        }

        static void BuildSightRMRcc(GameObject root, Dictionary<string, Material> mats)
        {
            BuildSightRMR(root, mats, true);
        }

        static void BuildSightEOTech(GameObject root, Dictionary<string, Material> mats)
        {
            // Main rectangular housing
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.055f, 0.038f, 0.065f);
            body.transform.localPosition = new Vector3(0, 0.025f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front rectangular window
            var frontWin = CreatePrimitive(root, "FrontWindow", PrimitiveType.Cube);
            frontWin.transform.localScale = new Vector3(0.045f, 0.028f, 0.003f);
            frontWin.transform.localPosition = new Vector3(0, 0.025f, 0.034f);
            frontWin.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Rear window
            var rearWin = CreatePrimitive(root, "RearWindow", PrimitiveType.Cube);
            rearWin.transform.localScale = new Vector3(0.045f, 0.028f, 0.003f);
            rearWin.transform.localPosition = new Vector3(0, 0.025f, -0.034f);
            rearWin.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Holographic reticle (circle with dot)
            var reticleRing = CreatePrimitive(root, "ReticleRing", PrimitiveType.Cylinder);
            reticleRing.transform.localScale = new Vector3(0.018f, 0.001f, 0.018f);
            reticleRing.transform.localPosition = new Vector3(0, 0.025f, -0.005f);
            reticleRing.transform.localRotation = Quaternion.Euler(90, 0, 0);
            reticleRing.GetComponent<Renderer>().sharedMaterial = mats["ReticleGreen"];

            var reticleDot = CreatePrimitive(root, "ReticleDot", PrimitiveType.Cylinder);
            reticleDot.transform.localScale = new Vector3(0.0015f, 0.001f, 0.0015f);
            reticleDot.transform.localPosition = new Vector3(0, 0.025f, -0.005f);
            reticleDot.transform.localRotation = Quaternion.Euler(90, 0, 0);
            reticleDot.GetComponent<Renderer>().sharedMaterial = mats["ReticleGreen"];

            // Side buttons
            var btn1 = CreatePrimitive(root, "Button_Up", PrimitiveType.Cube);
            btn1.transform.localScale = new Vector3(0.008f, 0.006f, 0.006f);
            btn1.transform.localPosition = new Vector3(-0.029f, 0.035f, 0.01f);
            btn1.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            var btn2 = CreatePrimitive(root, "Button_Down", PrimitiveType.Cube);
            btn2.transform.localScale = new Vector3(0.008f, 0.006f, 0.006f);
            btn2.transform.localPosition = new Vector3(-0.029f, 0.015f, 0.01f);
            btn2.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Quick-detach mount
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.05f, 0.015f, 0.035f);
            mount.transform.localPosition = new Vector3(0, -0.005f, 0.002f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            var lever = CreatePrimitive(root, "QDLever", PrimitiveType.Cube);
            lever.transform.localScale = new Vector3(0.012f, 0.02f, 0.008f);
            lever.transform.localPosition = new Vector3(0.025f, 0.005f, 0.002f);
            lever.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildSightAimpoint(GameObject root, Dictionary<string, Material> mats)
        {
            // Main tube (Micro T-2 style)
            var tube = CreatePrimitive(root, "Tube", PrimitiveType.Cylinder);
            tube.transform.localScale = new Vector3(0.03f, 0.045f, 0.03f);
            tube.transform.localPosition = new Vector3(0, 0.0225f, 0);
            tube.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front lens
            var frontLens = CreatePrimitive(root, "FrontLens", PrimitiveType.Cylinder);
            frontLens.transform.localScale = new Vector3(0.028f, 0.002f, 0.028f);
            frontLens.transform.localPosition = new Vector3(0, 0.0455f, 0);
            frontLens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Rear lens
            var rearLens = CreatePrimitive(root, "RearLens", PrimitiveType.Cylinder);
            rearLens.transform.localScale = new Vector3(0.028f, 0.002f, 0.028f);
            rearLens.transform.localPosition = new Vector3(0, -0.0005f, 0);
            rearLens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Reticle
            var reticle = CreatePrimitive(root, "Reticle", PrimitiveType.Cylinder);
            reticle.transform.localScale = new Vector3(0.003f, 0.001f, 0.003f);
            reticle.transform.localPosition = new Vector3(0, 0.0225f, 0);
            reticle.GetComponent<Renderer>().sharedMaterial = mats["ReticleRed"];

            // Brightness dial
            var dial = CreatePrimitive(root, "BrightnessDial", PrimitiveType.Cylinder);
            dial.transform.localScale = new Vector3(0.012f, 0.006f, 0.012f);
            dial.transform.localPosition = new Vector3(0.018f, 0.0225f, 0);
            dial.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mount base (integrated Picatinny)
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.042f, 0.012f, 0.03f);
            mount.transform.localPosition = new Vector3(0, -0.01f, 0.002f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
            screw.transform.localPosition = new Vector3(0.014f, -0.01f, 0.002f);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildSightMagnifier(GameObject root, Dictionary<string, Material> mats)
        {
            // Main magnifier body
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.042f, 0.06f, 0.042f);
            body.transform.localPosition = new Vector3(0, 0.03f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front lens (larger)
            var frontLens = CreatePrimitive(root, "FrontLens", PrimitiveType.Cylinder);
            frontLens.transform.localScale = new Vector3(0.038f, 0.003f, 0.038f);
            frontLens.transform.localPosition = new Vector3(0, 0.061f, 0);
            frontLens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Rear lens
            var rearLens = CreatePrimitive(root, "RearLens", PrimitiveType.Cylinder);
            rearLens.transform.localScale = new Vector3(0.038f, 0.003f, 0.038f);
            rearLens.transform.localPosition = new Vector3(0, -0.001f, 0);
            rearLens.GetComponent<Renderer>().sharedMaterial = mats["SightGlass"];

            // Flip mount base
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.05f, 0.025f, 0.04f);
            mount.transform.localPosition = new Vector3(0, -0.02f, 0.015f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Flip hinge
            var hinge = CreatePrimitive(root, "FlipHinge", PrimitiveType.Cylinder);
            hinge.transform.localScale = new Vector3(0.008f, 0.03f, 0.008f);
            hinge.transform.localPosition = new Vector3(0.025f, -0.005f, 0.015f);
            hinge.transform.localRotation = Quaternion.Euler(0, 0, 90);
            hinge.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Lock lever
            var lockLever = CreatePrimitive(root, "LockLever", PrimitiveType.Cube);
            lockLever.transform.localScale = new Vector3(0.01f, 0.015f, 0.006f);
            lockLever.transform.localPosition = new Vector3(-0.025f, -0.005f, 0.015f);
            lockLever.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildSightBUISFront(GameObject root, Dictionary<string, Material> mats)
        {
            // Base
            var baseObj = CreatePrimitive(root, "Base", PrimitiveType.Cube);
            baseObj.transform.localScale = new Vector3(0.018f, 0.01f, 0.022f);
            baseObj.transform.localPosition = new Vector3(0, 0.005f, 0);
            baseObj.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Front sight post
            var post = CreatePrimitive(root, "SightPost", PrimitiveType.Cube);
            post.transform.localScale = new Vector3(0.003f, 0.025f, 0.003f);
            post.transform.localPosition = new Vector3(0, 0.0175f, 0);
            post.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Protective wings
            var wing1 = CreatePrimitive(root, "Wing_Left", PrimitiveType.Cube);
            wing1.transform.localScale = new Vector3(0.002f, 0.02f, 0.008f);
            wing1.transform.localPosition = new Vector3(-0.006f, 0.015f, 0);
            wing1.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            var wing2 = CreatePrimitive(root, "Wing_Right", PrimitiveType.Cube);
            wing2.transform.localScale = new Vector3(0.002f, 0.02f, 0.008f);
            wing2.transform.localPosition = new Vector3(0.006f, 0.015f, 0);
            wing2.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Folding hinge
            var hinge = CreatePrimitive(root, "Hinge", PrimitiveType.Cylinder);
            hinge.transform.localScale = new Vector3(0.004f, 0.008f, 0.004f);
            hinge.transform.localPosition = new Vector3(0, -0.001f, -0.011f);
            hinge.transform.localRotation = Quaternion.Euler(0, 0, 90);
            hinge.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Mount
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.02f, 0.008f, 0.015f);
            mount.transform.localPosition = new Vector3(0, -0.005f, -0.005f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildSightBUISRear(GameObject root, Dictionary<string, Material> mats)
        {
            // Base
            var baseObj = CreatePrimitive(root, "Base", PrimitiveType.Cube);
            baseObj.transform.localScale = new Vector3(0.022f, 0.01f, 0.022f);
            baseObj.transform.localPosition = new Vector3(0, 0.005f, 0);
            baseObj.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Rear aperture (peep sight)
            var aperture = CreatePrimitive(root, "Aperture", PrimitiveType.Cylinder);
            aperture.transform.localScale = new Vector3(0.006f, 0.003f, 0.006f);
            aperture.transform.localPosition = new Vector3(0, 0.02f, 0);
            aperture.transform.localRotation = Quaternion.Euler(90, 0, 0);
            aperture.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Aperture hole (smaller cylinder)
            var hole = CreatePrimitive(root, "ApertureHole", PrimitiveType.Cylinder);
            hole.transform.localScale = new Vector3(0.0025f, 0.004f, 0.0025f);
            hole.transform.localPosition = new Vector3(0, 0.02f, 0);
            hole.transform.localRotation = Quaternion.Euler(90, 0, 0);
            hole.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Windage drum
            var windage = CreatePrimitive(root, "WindageDrum", PrimitiveType.Cylinder);
            windage.transform.localScale = new Vector3(0.01f, 0.012f, 0.01f);
            windage.transform.localPosition = new Vector3(0.014f, 0.011f, 0);
            windage.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Folding hinge
            var hinge = CreatePrimitive(root, "Hinge", PrimitiveType.Cylinder);
            hinge.transform.localScale = new Vector3(0.004f, 0.008f, 0.004f);
            hinge.transform.localPosition = new Vector3(0, -0.001f, -0.011f);
            hinge.transform.localRotation = Quaternion.Euler(0, 0, 90);
            hinge.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Mount
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.024f, 0.008f, 0.015f);
            mount.transform.localPosition = new Vector3(0, -0.005f, -0.005f);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        // =====================================================================
        // MUZZLE DEVICES
        // =====================================================================

        static int CreateMuzzleDevices(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Pistol_Short suppressor
            count += CreateAccessory(path, "JWA_Accessory_Suppressor_Pistol_Short",
                JWA_AccessoryBase.AccessoryType.Suppressor, "Pistol_Short",
                "Short pistol suppressor outer tube", new[] { "ThreadedBarrel" }, false, 380,
                (root) => BuildSuppressor(root, mats, true));

            // Rifle_Full suppressor
            count += CreateAccessory(path, "JWA_Accessory_Suppressor_Rifle_Full",
                JWA_AccessoryBase.AccessoryType.Suppressor, "Rifle_Full",
                "Full-size rifle suppressor outer tube", new[] { "ThreadedBarrel" }, false, 580,
                (root) => BuildSuppressor(root, mats, false));

            // Compensator_3Port
            count += CreateAccessory(path, "JWA_Accessory_MuzzleBrake_Compensator_3Port",
                JWA_AccessoryBase.AccessoryType.MuzzleBrake, "Compensator_3Port",
                "3-port compensator", new[] { "ThreadedBarrel" }, false, 280,
                (root) => BuildCompensator(root, mats, 3));

            // Compensator_5Port
            count += CreateAccessory(path, "JWA_Accessory_MuzzleBrake_Compensator_5Port",
                JWA_AccessoryBase.AccessoryType.MuzzleBrake, "Compensator_5Port",
                "5-port compensator (TTI style)", new[] { "ThreadedBarrel" }, false, 380,
                (root) => BuildCompensator(root, mats, 5));

            // Flash_Hider_A2
            count += CreateAccessory(path, "JWA_Accessory_MuzzleBrake_Flash_Hider_A2",
                JWA_AccessoryBase.AccessoryType.MuzzleBrake, "Flash_Hider_A2",
                "A2 birdcage flash hider", new[] { "ThreadedBarrel" }, false, 280,
                (root) => BuildFlashHiderA2(root, mats));

            return count;
        }

        static void BuildSuppressor(GameObject root, Dictionary<string, Material> mats, bool pistol)
        {
            float length = pistol ? 0.065f : 0.18f;
            float dia = pistol ? 0.028f : 0.038f;

            // Main tube
            var tube = CreatePrimitive(root, "Tube", PrimitiveType.Cylinder);
            tube.transform.localScale = new Vector3(dia, length, dia);
            tube.transform.localPosition = new Vector3(0, length / 2, 0);
            tube.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Front end cap
            var frontCap = CreatePrimitive(root, "FrontCap", PrimitiveType.Cylinder);
            frontCap.transform.localScale = new Vector3(dia + 0.002f, 0.004f, dia + 0.002f);
            frontCap.transform.localPosition = new Vector3(0, length + 0.002f, 0);
            frontCap.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Rear mount section
            var mount = CreatePrimitive(root, "MountSection", PrimitiveType.Cylinder);
            mount.transform.localScale = new Vector3(dia + 0.004f, 0.015f, dia + 0.004f);
            mount.transform.localPosition = new Vector3(0, -0.0075f, 0);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Thread indicator
            var threads = CreatePrimitive(root, "Threads", PrimitiveType.Cube);
            threads.transform.localScale = new Vector3(dia + 0.002f, 0.001f, dia + 0.002f);
            threads.transform.localPosition = new Vector3(0, -0.015f, 0);
            threads.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildCompensator(GameObject root, Dictionary<string, Material> mats, int ports)
        {
            // Main body
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.032f, 0.045f, 0.032f);
            body.transform.localPosition = new Vector3(0, 0.0225f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Ports
            float portAngle = 360f / ports;
            for (int i = 0; i < ports; i++)
            {
                var port = CreatePrimitive(root, $"Port_{i}", PrimitiveType.Cylinder);
                port.transform.localScale = new Vector3(0.006f, 0.002f, 0.01f);
                float angle = i * portAngle * Mathf.Deg2Rad;
                port.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 0.018f,
                    0.0225f,
                    Mathf.Cos(angle) * 0.018f
                );
                port.transform.localRotation = Quaternion.Euler(0, i * portAngle, 90);
                port.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }

            // Top ports (gas venting up)
            for (int i = 0; i < ports; i++)
            {
                var topPort = CreatePrimitive(root, $"TopPort_{i}", PrimitiveType.Cylinder);
                topPort.transform.localScale = new Vector3(0.005f, 0.015f, 0.005f);
                float angle = i * portAngle * Mathf.Deg2Rad;
                topPort.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 0.012f,
                    0.04f,
                    Mathf.Cos(angle) * 0.012f
                );
                topPort.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }

            // Thread section
            var threads = CreatePrimitive(root, "Threads", PrimitiveType.Cylinder);
            threads.transform.localScale = new Vector3(0.028f, 0.01f, 0.028f);
            threads.transform.localPosition = new Vector3(0, -0.005f, 0);
            threads.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildFlashHiderA2(GameObject root, Dictionary<string, Material> mats)
        {
            // Main body
            var body = CreatePrimitive(root, "Body", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.026f, 0.05f, 0.026f);
            body.transform.localPosition = new Vector3(0, 0.025f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Birdcage prongs (5 prongs, no bottom)
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) continue; // Skip bottom (6 o'clock)

                var prong = CreatePrimitive(root, $"Prong_{i}", PrimitiveType.Cube);
                prong.transform.localScale = new Vector3(0.0025f, 0.035f, 0.004f);
                float angle = i * 72f * Mathf.Deg2Rad;
                prong.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 0.013f,
                    0.04f,
                    Mathf.Cos(angle) * 0.013f
                );
                prong.transform.localRotation = Quaternion.Euler(0, i * 72f, 0);
                prong.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }

            // Base ring
            var ring = CreatePrimitive(root, "BaseRing", PrimitiveType.Cylinder);
            ring.transform.localScale = new Vector3(0.028f, 0.006f, 0.028f);
            ring.transform.localPosition = new Vector3(0, 0.008f, 0);
            ring.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Thread section
            var threads = CreatePrimitive(root, "Threads", PrimitiveType.Cylinder);
            threads.transform.localScale = new Vector3(0.024f, 0.01f, 0.024f);
            threads.transform.localPosition = new Vector3(0, -0.005f, 0);
            threads.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        // =====================================================================
        // RAIL COVERS
        // =====================================================================

        static int CreateRailCovers(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Picatinny_2Slot
            count += CreateAccessory(path, "JWA_Accessory_RailCover_Picatinny_2Slot",
                JWA_AccessoryBase.AccessoryType.RailCover, "Picatinny_2Slot",
                "2-slot Picatinny rail cover", new[] { "Picatinny" }, false, 80,
                (root) => BuildRailCoverPicatinny(root, mats, 2));

            // Picatinny_4Slot
            count += CreateAccessory(path, "JWA_Accessory_RailCover_Picatinny_4Slot",
                JWA_AccessoryBase.AccessoryType.RailCover, "Picatinny_4Slot",
                "4-slot Picatinny rail cover", new[] { "Picatinny" }, false, 130,
                (root) => BuildRailCoverPicatinny(root, mats, 4));

            // MLOK_3Slot
            count += CreateAccessory(path, "JWA_Accessory_RailCover_MLOK_3Slot",
                JWA_AccessoryBase.AccessoryType.RailCover, "MLOK_3Slot",
                "3-slot M-LOK rail cover", new[] { "M-LOK" }, false, 80,
                (root) => BuildRailCoverMLOK(root, mats, 3));

            // MLOK_5Slot
            count += CreateAccessory(path, "JWA_Accessory_RailCover_MLOK_5Slot",
                JWA_AccessoryBase.AccessoryType.RailCover, "MLOK_5Slot",
                "5-slot M-LOK rail cover", new[] { "M-LOK" }, false, 130,
                (root) => BuildRailCoverMLOK(root, mats, 5));

            return count;
        }

        static void BuildRailCoverPicatinny(GameObject root, Dictionary<string, Material> mats, int slots)
        {
            float slotWidth = 0.00523f; // 5.23mm
            float length = slots * slotWidth * 2 + 0.005f;

            // Main cover
            var cover = CreatePrimitive(root, "Cover", PrimitiveType.Cube);
            cover.transform.localScale = new Vector3(0.022f, 0.008f, length);
            cover.transform.localPosition = new Vector3(0, 0.004f, 0);
            cover.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Cross slots (Picatinny pattern)
            for (int i = 0; i < slots; i++)
            {
                var slot = CreatePrimitive(root, $"Slot_{i}", PrimitiveType.Cube);
                slot.transform.localScale = new Vector3(0.018f, 0.002f, 0.004f);
                slot.transform.localPosition = new Vector3(0, 0.007f, -length/2 + 0.005f + i * slotWidth * 2);
                slot.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
            }

            // Locking tab
            var tab = CreatePrimitive(root, "LockTab", PrimitiveType.Cube);
            tab.transform.localScale = new Vector3(0.004f, 0.003f, 0.008f);
            tab.transform.localPosition = new Vector3(0.011f, 0.001f, 0);
            tab.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
        }

        static void BuildRailCoverMLOK(GameObject root, Dictionary<string, Material> mats, int slots)
        {
            float slotWidth = 0.00838f; // 8.38mm
            float length = slots * slotWidth + 0.005f;

            // Main cover
            var cover = CreatePrimitive(root, "Cover", PrimitiveType.Cube);
            cover.transform.localScale = new Vector3(0.022f, 0.007f, length);
            cover.transform.localPosition = new Vector3(0, 0.0035f, 0);
            cover.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // M-LOK slots (rectangular)
            for (int i = 0; i < slots; i++)
            {
                var slot = CreatePrimitive(root, $"MLOKSlot_{i}", PrimitiveType.Cube);
                slot.transform.localScale = new Vector3(0.006f, 0.002f, slotWidth * 0.8f);
                slot.transform.localPosition = new Vector3(0, 0.006f, -length/2 + slotWidth/2 + i * slotWidth);
                slot.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
            }

            // Locking tabs (front and rear)
            for (int i = 0; i < 2; i++)
            {
                var tab = CreatePrimitive(root, $"LockTab_{i}", PrimitiveType.Cube);
                tab.transform.localScale = new Vector3(0.004f, 0.003f, 0.006f);
                tab.transform.localPosition = new Vector3(0.011f, 0.0005f, i == 0 ? -length/2 + 0.003f : length/2 - 0.003f);
                tab.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];
            }
        }

        // =====================================================================
        // SLING MOUNTS
        // =====================================================================

        static int CreateSlingMounts(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // QD_Socket
            count += CreateAccessory(path, "JWA_Accessory_SlingMount_QD_Socket",
                JWA_AccessoryBase.AccessoryType.SlingMount, "QD_Socket",
                "Quick-detach sling socket", new[] { "Picatinny", "Plate" }, false, 180,
                (root) => BuildSlingQDSocket(root, mats));

            // Loop_Standard
            count += CreateAccessory(path, "JWA_Accessory_SlingMount_Loop_Standard",
                JWA_AccessoryBase.AccessoryType.SlingMount, "Loop_Standard",
                "Standard sling loop", new[] { "Stock", "Receiver" }, false, 130,
                (root) => BuildSlingLoop(root, mats));

            // ASH_Plate
            count += CreateAccessory(path, "JWA_Accessory_SlingMount_ASH_Plate",
                JWA_AccessoryBase.AccessoryType.SlingMount, "ASH_Plate",
                "ASH receiver plate mount", new[] { "Receiver" }, true, 230,
                (root) => BuildSlingASHPlate(root, mats));

            return count;
        }

        static void BuildSlingQDSocket(GameObject root, Dictionary<string, Material> mats)
        {
            // QD socket body
            var body = CreatePrimitive(root, "SocketBody", PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.022f, 0.028f, 0.022f);
            body.transform.localPosition = new Vector3(0, 0.014f, 0);
            body.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Internal spring cavity
            var cavity = CreatePrimitive(root, "SpringCavity", PrimitiveType.Cylinder);
            cavity.transform.localScale = new Vector3(0.014f, 0.024f, 0.014f);
            cavity.transform.localPosition = new Vector3(0, 0.014f, 0);
            cavity.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Locking ball detents (visual)
            for (int i = 0; i < 4; i++)
            {
                var detent = CreatePrimitive(root, $"Detent_{i}", PrimitiveType.Sphere);
                detent.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
                float angle = i * 90f * Mathf.Deg2Rad;
                detent.transform.localPosition = new Vector3(
                    Mathf.Sin(angle) * 0.0085f,
                    0.02f,
                    Mathf.Cos(angle) * 0.0085f
                );
                detent.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }

            // Mount base (Picatinny)
            var mount = CreatePrimitive(root, "MountBase", PrimitiveType.Cube);
            mount.transform.localScale = new Vector3(0.03f, 0.012f, 0.025f);
            mount.transform.localPosition = new Vector3(0, -0.006f, 0);
            mount.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            var screw = CreatePrimitive(root, "MountScrew", PrimitiveType.Cylinder);
            screw.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
            screw.transform.localPosition = new Vector3(0.01f, -0.006f, 0);
            screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
            screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        static void BuildSlingLoop(GameObject root, Dictionary<string, Material> mats)
        {
            // Loop ring
            var ring = CreatePrimitive(root, "LoopRing", PrimitiveType.Cylinder);
            ring.transform.localScale = new Vector3(0.025f, 0.005f, 0.025f);
            ring.transform.localPosition = new Vector3(0, 0.0125f, 0);
            ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
            ring.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Mount plate
            var plate = CreatePrimitive(root, "MountPlate", PrimitiveType.Cube);
            plate.transform.localScale = new Vector3(0.03f, 0.006f, 0.02f);
            plate.transform.localPosition = new Vector3(0, -0.003f, 0);
            plate.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Mount screws (2)
            for (int i = 0; i < 2; i++)
            {
                var screw = CreatePrimitive(root, $"MountScrew_{i}", PrimitiveType.Cylinder);
                screw.transform.localScale = new Vector3(0.004f, 0.008f, 0.004f);
                screw.transform.localPosition = new Vector3(i == 0 ? -0.01f : 0.01f, -0.003f, 0);
                screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
                screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }
        }

        static void BuildSlingASHPlate(GameObject root, Dictionary<string, Material> mats)
        {
            // ASH receiver plate (rectangular with QD socket)
            var plate = CreatePrimitive(root, "ReceiverPlate", PrimitiveType.Cube);
            plate.transform.localScale = new Vector3(0.055f, 0.006f, 0.045f);
            plate.transform.localPosition = new Vector3(0, 0.003f, 0);
            plate.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Integrated QD socket
            var socket = CreatePrimitive(root, "QDSocket", PrimitiveType.Cylinder);
            socket.transform.localScale = new Vector3(0.02f, 0.025f, 0.02f);
            socket.transform.localPosition = new Vector3(0, 0.0155f, 0);
            socket.GetComponent<Renderer>().sharedMaterial = mats["Steel"];

            // Socket cavity
            var cavity = CreatePrimitive(root, "Cavity", PrimitiveType.Cylinder);
            cavity.transform.localScale = new Vector3(0.012f, 0.02f, 0.012f);
            cavity.transform.localPosition = new Vector3(0, 0.0155f, 0);
            cavity.GetComponent<Renderer>().sharedMaterial = mats["Polymer"];

            // Mounting holes (4)
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    var hole = CreatePrimitive(root, $"MountHole_{x}_{z}", PrimitiveType.Cylinder);
                    hole.transform.localScale = new Vector3(0.005f, 0.008f, 0.005f);
                    hole.transform.localPosition = new Vector3(
                        (x == 0 ? -0.02f : 0.02f),
                        0.003f,
                        (z == 0 ? -0.015f : 0.015f)
                    );
                    hole.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    hole.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
                }
            }
        }

        // =====================================================================
        // MAGWELL & CHARGING HANDLES
        // =====================================================================

        static int CreateMagwellCharging(string path, Dictionary<string, Material> mats)
        {
            int count = 0;

            // Flared_Standard
            count += CreateAccessory(path, "JWA_Accessory_MagwellFlare_Flared_Standard",
                JWA_AccessoryBase.AccessoryType.MagwellFlare, "Flared_Standard",
                "Standard flared magwell", new[] { "Magwell" }, true, 280,
                (root) => BuildMagwellFlare(root, mats, false));

            // Flared_Extended
            count += CreateAccessory(path, "JWA_Accessory_MagwellFlare_Flared_Extended",
                JWA_AccessoryBase.AccessoryType.MagwellFlare, "Flared_Extended",
                "Extended flared magwell", new[] { "Magwell" }, true, 380,
                (root) => BuildMagwellFlare(root, mats, true));

            // Extended_Latch
            count += CreateAccessory(path, "JWA_Accessory_ChargingHandle_Extended_Latch",
                JWA_AccessoryBase.AccessoryType.ChargingHandle, "Extended_Latch",
                "Extended charging handle latch", new[] { "Receiver" }, true, 180,
                (root) => BuildChargingHandle(root, mats, false));

            // Ambidextrous
            count += CreateAccessory(path, "JWA_Accessory_ChargingHandle_Ambidextrous",
                JWA_AccessoryBase.AccessoryType.ChargingHandle, "Ambidextrous",
                "Ambidextrous charging handle", new[] { "Receiver" }, true, 280,
                (root) => BuildChargingHandle(root, mats, true));

            return count;
        }

        static void BuildMagwellFlare(GameObject root, Dictionary<string, Material> mats, bool extended)
        {
            float flareWidth = extended ? 0.055f : 0.045f;
            float height = extended ? 0.04f : 0.03f;

            // Main flare body
            var body = CreatePrimitive(root, "FlareBody", PrimitiveType.Cube);
            body.transform.localScale = new Vector3(flareWidth, height, 0.025f);
            body.transform.localPosition = new Vector3(0, height / 2 - 0.005f, 0);
            body.transform.localRotation = Quaternion.Euler(-10, 0, 0); // Slight flare angle
            body.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Front ramp
            var ramp = CreatePrimitive(root, "FrontRamp", PrimitiveType.Cube);
            ramp.transform.localScale = new Vector3(flareWidth - 0.005f, 0.015f, 0.01f);
            ramp.transform.localPosition = new Vector3(0, 0.01f, 0.017f);
            ramp.transform.localRotation = Quaternion.Euler(-25, 0, 0);
            ramp.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Rear blend
            var rear = CreatePrimitive(root, "RearBlend", PrimitiveType.Cube);
            rear.transform.localScale = new Vector3(flareWidth - 0.005f, 0.01f, 0.01f);
            rear.transform.localPosition = new Vector3(0, 0.01f, -0.017f);
            rear.transform.localRotation = Quaternion.Euler(10, 0, 0);
            rear.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Mounting screws (2)
            for (int i = 0; i < 2; i++)
            {
                var screw = CreatePrimitive(root, $"MountScrew_{i}", PrimitiveType.Cylinder);
                screw.transform.localScale = new Vector3(0.004f, 0.008f, 0.004f);
                screw.transform.localPosition = new Vector3(i == 0 ? -0.015f : 0.015f, height / 2, -0.01f);
                screw.transform.localRotation = Quaternion.Euler(90, 0, 0);
                screw.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }
        }

        static void BuildChargingHandle(GameObject root, Dictionary<string, Material> mats, bool ambi)
        {
            // Main latch
            var latch = CreatePrimitive(root, "Latch_Main", PrimitiveType.Cube);
            latch.transform.localScale = new Vector3(0.012f, 0.018f, 0.035f);
            latch.transform.localPosition = new Vector3(ambi ? 0.01f : 0.02f, 0.009f, 0);
            latch.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Extended tab
            var tab = CreatePrimitive(root, "ExtendedTab", PrimitiveType.Cube);
            tab.transform.localScale = new Vector3(0.006f, 0.008f, 0.045f);
            tab.transform.localPosition = new Vector3(ambi ? 0.01f : 0.02f, 0.018f, 0);
            tab.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

            // Ambidextrous: second latch on left
            if (ambi)
            {
                var latch2 = CreatePrimitive(root, "Latch_Left", PrimitiveType.Cube);
                latch2.transform.localScale = new Vector3(0.012f, 0.018f, 0.035f);
                latch2.transform.localPosition = new Vector3(-0.01f, 0.009f, 0);
                latch2.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

                var tab2 = CreatePrimitive(root, "ExtendedTab_Left", PrimitiveType.Cube);
                tab2.transform.localScale = new Vector3(0.006f, 0.008f, 0.045f);
                tab2.transform.localPosition = new Vector3(-0.01f, 0.018f, 0);
                tab2.GetComponent<Renderer>().sharedMaterial = mats["Aluminum"];

                // Connecting rod
                var rod = CreatePrimitive(root, "ConnectingRod", PrimitiveType.Cylinder);
                rod.transform.localScale = new Vector3(0.004f, 0.004f, 0.02f);
                rod.transform.localPosition = new Vector3(0, 0.009f, 0);
                rod.transform.localRotation = Quaternion.Euler(0, 90, 0);
                rod.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
            }

            // Roll pin
            var pin = CreatePrimitive(root, "RollPin", PrimitiveType.Cylinder);
            pin.transform.localScale = new Vector3(0.003f, 0.003f, 0.015f);
            pin.transform.localPosition = new Vector3(ambi ? 0.01f : 0.02f, 0.009f, -0.02f);
            pin.transform.localRotation = Quaternion.Euler(0, 90, 0);
            pin.GetComponent<Renderer>().sharedMaterial = mats["Steel"];
        }

        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        static int CreateAccessory(string path, string name, JWA_AccessoryBase.AccessoryType type, string variant,
            string description, string[] compatibleMounts, bool requiresGunsmith, int maxPolys,
            System.Action<GameObject> buildAction)
        {
            string prefabPath = $"{path}/{name}.prefab";

            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                Debug.Log($"[{name}] Already exists, skipping");
                return 0;
            }

            // Create root GameObject
            var root = new GameObject(name);

            // Add JWA_AccessoryBase component
            var accessory = root.AddComponent<JWA_AccessoryBase>();
            accessory.type = type;
            accessory.displayName = description;
            accessory.variant = variant;
            accessory.compatibleMounts = compatibleMounts;
            accessory.requiresGunsmith = requiresGunsmith;
            accessory.isCosmeticOnly = true;
            accessory.maxPolygonCount = maxPolys;

            // Create MountPoint (REQUIRED)
            var mountPoint = new GameObject("MountPoint").transform;
            mountPoint.SetParent(root.transform);
            mountPoint.localPosition = Vector3.zero;
            mountPoint.localRotation = Quaternion.identity;
            mountPoint.localScale = Vector3.one;
            accessory.mountPoint = mountPoint;

            // Create OffsetPoint (OPTIONAL)
            var offsetPoint = new GameObject("OffsetPoint").transform;
            offsetPoint.SetParent(root.transform);
            offsetPoint.localPosition = Vector3.zero;
            offsetPoint.localRotation = Quaternion.identity;
            offsetPoint.localScale = Vector3.one;
            accessory.offsetPoint = offsetPoint;

            // Create Model container
            var model = new GameObject("Model");
            model.transform.SetParent(root.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            // Build the accessory geometry
            buildAction(model);

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            // Clean up
            DestroyImmediate(root);

            Debug.Log($"[{name}] Created at {prefabPath}");
            return 1;
        }

        static GameObject CreatePrimitive(GameObject parent, string name, PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            // Remove collider (cosmetic only)
            var collider = go.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);

            return go;
        }

        // =====================================================================
        // CATALOG CREATION
        // =====================================================================

        [MenuItem("Tools/John Wick Arsenal/Create Accessory Catalog")]
        public static void CreateAccessoryCatalog()
        {
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string catalogPath = "Assets/Resources/JWA_AccessoryCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<JWA_AccessoryCatalog>(catalogPath);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<JWA_AccessoryCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }

            // Populate catalog with generated prefabs
            PopulateCatalog(catalog);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[JWA_AccessoryPrefabGenerator] Catalog created/updated at {catalogPath}");
            EditorUtility.DisplayDialog("Complete", "Accessory Catalog created and populated!", "OK");
        }

        static void PopulateCatalog(JWA_AccessoryCatalog catalog)
        {
            string basePath = "Assets/JohnWickArsenal/Prefabs/Accessories";

            // Helper to find and assign prefabs
            System.Action<JWA_AccessoryBase.AccessoryType, string, List<JWA_AccessoryCatalog.AccessoryEntry>> assignPrefabs =
                (type, subfolder, list) =>
                {
                    foreach (var entry in list)
                    {
                        string prefabPath = $"{basePath}/{subfolder}/JWA_Accessory_{type}_{entry.variant}.prefab";
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        if (prefab != null)
                        {
                            entry.prefab = prefab;
                        }
                        else
                        {
                            Debug.LogWarning($"[Catalog] Prefab not found: {prefabPath}");
                        }
                    }
                };

            // Flashlights
            assignPrefabs(JWA_AccessoryBase.AccessoryType.Flashlight, "Flashlights", catalog.flashlights);

            // Laser Modules
            assignPrefabs(JWA_AccessoryBase.AccessoryType.LaserModule, "LaserModules", catalog.laserModules);

            // Grips
            assignPrefabs(JWA_AccessoryBase.AccessoryType.Grip, "Grips", catalog.grips);

            // Sights
            assignPrefabs(JWA_AccessoryBase.AccessoryType.Sight, "Sights", catalog.sights);

            // Muzzle Devices (Suppressors)
            var suppressors = catalog.muzzleDevices.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.Suppressor);
            assignPrefabs(JWA_AccessoryBase.AccessoryType.Suppressor, "MuzzleDevices", suppressors);

            // Muzzle Devices (Muzzle Brakes)
            var muzzleBrakes = catalog.muzzleDevices.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.MuzzleBrake);
            assignPrefabs(JWA_AccessoryBase.AccessoryType.MuzzleBrake, "MuzzleDevices", muzzleBrakes);

            // Rail Covers
            assignPrefabs(JWA_AccessoryBase.AccessoryType.RailCover, "RailCovers", catalog.railCovers);

            // Sling Mounts
            assignPrefabs(JWA_AccessoryBase.AccessoryType.SlingMount, "SlingMounts", catalog.slingMounts);

            // Magwell & Charging Handles (Magwell Flares)
            var magwells = catalog.magwellCharging.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.MagwellFlare);
            assignPrefabs(JWA_AccessoryBase.AccessoryType.MagwellFlare, "MagwellCharging", magwells);

            // Magwell & Charging Handles (Charging Handles)
            var chargingHandles = catalog.magwellCharging.FindAll(e => e.type == JWA_AccessoryBase.AccessoryType.ChargingHandle);
            assignPrefabs(JWA_AccessoryBase.AccessoryType.ChargingHandle, "MagwellCharging", chargingHandles);
        }

        [MenuItem("Tools/John Wick Arsenal/Generate All (Prefabs + Catalog)")]
        public static void GenerateAllAndCatalog()
        {
            GenerateAllAccessories();
            CreateAccessoryCatalog();
        }
    }
}