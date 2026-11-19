using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Comprehensive build validation for John Wick Arsenal mod.
/// Validates prefabs, settings, dependencies, and build output.
/// </summary>
public class JWA_BuildValidator : EditorWindow
{
    private const string MOD_NAME = "JohnWickArsenal";
    private const string RESOURCES_PATH = "Assets/JohnWickArsenal/Resources";
    private const string BUNDLE_OUTPUT = "AssetBundles/Android/johnwickarsenal";

    private Vector2 scrollPos;
    private ValidationResult lastResult;

    [MenuItem("John Wick Arsenal/Validate Build", priority = 30)]
    public static void ShowWindow()
    {
        var window = GetWindow<JWA_BuildValidator>("JWA Build Validator");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        DrawValidationCategories();
        DrawLastResult();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("John Wick Arsenal - Build Validator", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Comprehensive validation for B&S Nomad mod builds", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(5);
    }

    private void DrawValidationCategories()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Validation Categories", EditorStyles.boldLabel);

        if (GUILayout.Button("🔍 Run All Validations", GUILayout.Height(40)))
        {
            RunAllValidations();
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📦 Project Settings"))
        {
            ValidateProjectSettings();
        }
        if (GUILayout.Button("📁 Prefabs & Assets"))
        {
            ValidatePrefabsAndAssets();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔧 Components & Scripts"))
        {
            ValidateComponentsAndScripts();
        }
        if (GUILayout.Button("📱 Android/Quest Config"))
        {
            ValidateAndroidConfig();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📦 AssetBundle Output"))
        {
            ValidateAssetBundleOutput();
        }
        if (GUILayout.Button("📋 Mod Package"))
        {
            ValidateModPackage();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawLastResult()
    {
        if (lastResult == null) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Last Validation Result", EditorStyles.boldLabel);

        var color = lastResult.IsValid ? Color.green : Color.red;
        var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
        EditorGUILayout.LabelField(lastResult.IsValid ? "✅ ALL CHECKS PASSED" : "❌ VALIDATION FAILED", style);

        EditorGUILayout.LabelField($"Timestamp: {lastResult.Timestamp}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Duration: {lastResult.Duration.TotalSeconds:F1}s", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Checks: {lastResult.PassedChecks}/{lastResult.TotalChecks} passed", EditorStyles.miniLabel);

        if (lastResult.Issues.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Issues Found:", EditorStyles.boldLabel);

            foreach (var issue in lastResult.Issues)
            {
                var issueStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = issue.Severity == ValidationSeverity.Error ? Color.red : Color.yellow }
                };
                EditorGUILayout.LabelField($"  [{issue.Severity}] {issue.Category}: {issue.Message}", issueStyle);
                if (!string.IsNullOrEmpty(issue.FixHint))
                {
                    EditorGUILayout.LabelField($"    → {issue.FixHint}", EditorStyles.miniLabel);
                }
            }
        }

        if (lastResult.Warnings.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);
            foreach (var warn in lastResult.Warnings)
            {
                EditorGUILayout.LabelField($"  ⚠ {warn}", EditorStyles.miniLabel);
            }
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Export Report"))
        {
            ExportReport();
        }

        EditorGUILayout.EndVertical();
    }

    // ========================================================================
    // VALIDATION IMPLEMENTATION
    // ========================================================================

    private void RunAllValidations()
    {
        lastResult = new ValidationResult { Timestamp = DateTime.Now };
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            ValidateProjectSettings();
            ValidatePrefabsAndAssets();
            ValidateComponentsAndScripts();
            ValidateAndroidConfig();
            ValidateAssetBundleOutput();
            ValidateModPackage();

            lastResult.Duration = sw.Elapsed;
            lastResult.Finalize();

            if (lastResult.IsValid)
            {
                EditorUtility.DisplayDialog("Validation Passed", "All validation checks passed!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Failed",
                    $"Found {lastResult.Issues.Count} issue(s) and {lastResult.Warnings.Count} warning(s).\nSee details in validator window.", "OK");
            }
        }
        catch (Exception e)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "System", $"Validation crashed: {e.Message}", e.StackTrace);
            lastResult.Duration = sw.Elapsed;
            lastResult.Finalize();
        }

        Repaint();
    }

    private void ValidateProjectSettings()
    {
        lastResult.AddCategory("Project Settings");

        // Build target
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Project Settings",
                "Active build target is not Android",
                "Set build target to Android in Build Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // Scripting backend
        var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
        if (backend != ScriptingImplementation.IL2CPP)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Project Settings",
                $"Scripting backend is {backend}, required IL2CPP",
                "Set Scripting Backend to IL2CPP in Player Settings > Android > Other Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // API compatibility
        var apiLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android);
        if (apiLevel != ApiCompatibilityLevel.NET_Standard_2_1)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Project Settings",
                $"API compatibility level is {apiLevel}, required .NET Standard 2.1",
                "Set Api Compatibility Level to .NET Standard 2.1 in Player Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // Min SDK version
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Project Settings",
                $"Min SDK version is {PlayerSettings.Android.minSdkVersion}, recommended API 29+ for Quest",
                "Set Minimum API Level to 29 in Player Settings > Android > Other Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // Target SDK
        if (PlayerSettings.Android.targetSdkVersion < AndroidSdkVersions.AndroidApiLevel30)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Project Settings",
                $"Target SDK version is {PlayerSettings.Android.targetSdkVersion}, recommended API 30+",
                "Set Target API Level to 30+ in Player Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // Architecture
        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Project Settings",
                "ARM64 architecture not enabled",
                "Enable ARM64 in Player Settings > Android > Other Settings > Target Architectures");
        }
        else
        {
            lastResult.AddPass();
        }

        // Color space
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Project Settings",
                "Color space is not Linear",
                "Set Color Space to Linear in Player Settings > Android > Other Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // XR settings
        if (!PlayerSettings.virtualRealitySupported)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Project Settings",
                "Virtual Reality not supported",
                "Enable Virtual Reality Supported in Player Settings > XR Settings");
        }
        else
        {
            lastResult.AddPass();
        }

        // Stripping level
        var stripping = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android);
        if (stripping == ManagedStrippingLevel.Minimal)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Project Settings",
                "Managed stripping level is Minimal",
                "Consider Medium or High for smaller build size");
        }
    }

    private void ValidatePrefabsAndAssets()
    {
        lastResult.AddCategory("Prefabs & Assets");

        // Check Resources folder exists
        if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets",
                $"Resources folder not found: {RESOURCES_PATH}",
                "Run Project Bootstrap to create folder structure");
            return;
        }

        // Find all prefabs
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { RESOURCES_PATH });
        if (guids.Length == 0)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                "No prefabs found in Resources folder",
                "Use Prefab Setup Wizard to create weapon prefabs");
        }
        else
        {
            lastResult.AddPass();

            // Validate each prefab
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ValidatePrefab(path);
            }
        }

        // Check for models
        var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/JohnWickArsenal/Models" });
        if (modelGuids.Length == 0)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                "No models found in Assets/JohnWickArsenal/Models/",
                "Import weapon models (FBX/OBJ) to Models folder");
        }

        // Check for textures
        var textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets/JohnWickArsenal/Textures" });
        if (textureGuids.Length == 0)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                "No textures found in Assets/JohnWickArsenal/Textures/",
                "Import texture maps (Albedo, Normal, ORM, Emissive)");
        }

        // Check Addressables
        var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
        if (aaSettings == null)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                "Addressables not initialized",
                "Run Project Bootstrap > Setup Addressables");
        }
    }

    private void ValidatePrefab(string prefabPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets",
                $"Failed to load prefab: {prefabPath}", "");
            return;
        }

        var name = prefab.name;
        bool isThrowable = false;

        // Check required components via reflection
        var asm = Assembly.GetAssembly(typeof(Weapon));
        if (asm != null)
        {
            var weaponType = asm.GetType("BladeAndSorcery.Gameplay.Weapon");
            var itemType = asm.GetType("BladeAndSorcery.Gameplay.Item");
            var gunType = asm.GetType("BladeAndSorcery.Gameplay.Gun");
            var magazineType = asm.GetType("BladeAndSorcery.Gameplay.Magazine");
            var throwableType = asm.GetType("BladeAndSorcery.Gameplay.Throwable");

            if (itemType != null && prefab.GetComponent(itemType) != null)
                isThrowable = true;

            if (isThrowable)
            {
                if (prefab.GetComponent(itemType) == null)
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Item component");
                if (throwableType != null && prefab.GetComponent(throwableType) == null)
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Throwable component");
            }
            else
            {
                if (weaponType != null && prefab.GetComponent(weaponType) == null)
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Weapon component");
                if (gunType != null && prefab.GetComponent(gunType) == null)
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Gun component");
                if (magazineType != null && prefab.GetComponentInChildren(magazineType) == null)
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Magazine component");
            }
        }

        // Check required transforms
        string[] requiredTransforms = { "MuzzlePoint", "EjectionPort", "Grip_L", "Grip_R" };
        foreach (var tName in requiredTransforms)
        {
            if (prefab.transform.Find(tName) == null)
                lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing transform '{tName}'");
        }

        // Check for sights on rifles/shotguns
        if (name.Contains("rifle") || name.Contains("shotgun") || name.Contains("tr1") || name.Contains("benelli") || name.Contains("ksg"))
        {
            if (prefab.transform.Find("Sight_Front") == null)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Missing Sight_Front");
            if (prefab.transform.Find("Sight_Rear") == null)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Missing Sight_Rear");
        }

        // Check for Tip on throwables
        if (name.Contains("knife") || name.Contains("pencil"))
        {
            if (prefab.transform.Find("Tip") == null)
                lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Tip transform");
        }

        // Check colliders
        if (prefab.GetComponent<Collider>() == null)
            lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing root collider");
        else
        {
            var cols = prefab.GetComponentsInChildren<Collider>(true);
            bool hasConvex = cols.Any(c => c is MeshCollider mc && mc.convex);
            if (!hasConvex && cols.Length > 1)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: No convex mesh colliders on Model");
        }

        // Check rigidbody
        var rb = prefab.GetComponent<Rigidbody>();
        if (rb == null)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing Rigidbody");
        }
        else
        {
            if (rb.interpolation != RigidbodyInterpolation.Interpolate)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Rigidbody interpolation should be Interpolate");
            if (rb.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Rigidbody collision detection should be ContinuousDynamic");
            if (rb.mass <= 0)
                lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Rigidbody mass should be > 0");
        }

        // Check for missing scripts
        var components = prefab.GetComponentsInChildren<Component>(true);
        foreach (var comp in components)
        {
            if (comp == null)
            {
                lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing script on {GetGameObjectPath(comp?.gameObject)}");
            }
        }

        // Check triangle count
        int totalTris = 0;
        var renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r is MeshRenderer mr && mr.GetComponent<MeshFilter>() != null)
            {
                totalTris += mr.GetComponent<MeshFilter>().sharedMesh?.triangles.Length / 3 ?? 0;
            }
            else if (r is SkinnedMeshRenderer smr)
            {
                totalTris += smr.sharedMesh?.triangles.Length / 3 ?? 0;
            }
        }

        if (totalTris > 30000)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                $"{name}: High triangle count ({totalTris:N0}) - may impact Quest performance",
                "Consider LODs, retopology, or normal map baking");
        }
        else if (totalTris > 15000)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets",
                $"{name}: Moderate triangle count ({totalTris:N0}) - consider optimization");
        }

        // Check materials
        foreach (var r in renderers)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat == null)
                {
                    lastResult.AddIssue(ValidationSeverity.Error, "Prefabs & Assets", $"{name}: Missing material on {r.name}");
                }
                else if (mat.shader == null || mat.shader.name.Contains("Hidden"))
                {
                    lastResult.AddIssue(ValidationSeverity.Warning, "Prefabs & Assets", $"{name}: Material '{mat.name}' uses hidden/invalid shader");
                }
            }
        }
    }

    private void ValidateComponentsAndScripts()
    {
        lastResult.AddCategory("Components & Scripts");

        // Check for JohnWickArsenal assembly
        var asm = Assembly.GetExecutingAssembly();
        var jwaTypes = asm.GetTypes().Where(t => t.Namespace?.StartsWith("JohnWickArsenal") == true).ToArray();

        if (jwaTypes.Length == 0)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Components & Scripts",
                "JohnWickArsenal assembly not found or no types",
                "Ensure Source/JohnWickArsenal is linked to Assets/Scripts/JohnWickArsenal");
        }
        else
        {
            lastResult.AddPass();

            // Check for required behaviors
            var requiredBehaviors = new[]
            {
                "JohnWickPistolBehavior",
                "JohnWickShotgunBehavior",
                "JohnWickRevolverBehavior",
                "JohnWickRifleBehavior",
                "ThrowingKnifeBehavior",
                "PencilBehavior"
            };

            foreach (var behavior in requiredBehaviors)
            {
                var type = asm.GetType($"JohnWickArsenal.Behaviors.{behavior}");
                if (type == null)
                {
                    lastResult.AddIssue(ValidationSeverity.Error, "Components & Scripts",
                        $"Missing behavior: {behavior}",
                        $"Ensure {behavior}.cs exists in Source/JohnWickArsenal/Behaviors/");
                }
                else
                {
                    lastResult.AddPass();
                }
            }

            // Check for ModBehaviorRegistry
            var registryType = asm.GetType("JohnWickArsenal.Utils.ModBehaviorRegistry");
            if (registryType == null)
            {
                lastResult.AddIssue(ValidationSeverity.Warning, "Components & Scripts",
                    "ModBehaviorRegistry not found",
                    "Create Utils/ModBehaviorRegistry.cs");
            }
        }

        // Check asmdef
        var asmdefPath = "Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef";
        if (!File.Exists(asmdefPath))
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Components & Scripts",
                "Assembly definition not found",
                "Create JohnWickArsenal.asmdef in Assets/Scripts/JohnWickArsenal/");
        }
        else
        {
            lastResult.AddPass();
        }
    }

    private void ValidateAndroidConfig()
    {
        lastResult.AddCategory("Android/Quest Config");

        // Check Unity version
        if (!Application.unityVersion.StartsWith("2022.3"))
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Android/Quest Config",
                $"Unity version {Application.unityVersion} - recommended 2022.3 LTS",
                "Use Unity 2022.3 LTS for best Quest compatibility");
        }
        else
        {
            lastResult.AddPass();
        }

        // Check graphics API
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        if (!apis.Contains(UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3))
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Android/Quest Config",
                "OpenGL ES 3.2 not in Graphics APIs",
                "Add OpenGLES3 to Player Settings > Android > Other Settings > Graphics APIs");
        }
        else
        {
            lastResult.AddPass();
        }

        // Check Vulkan is not forced (Quest doesn't support it well)
        if (apis.Contains(UnityEngine.Rendering.GraphicsDeviceType.Vulkan))
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Android/Quest Config",
                "Vulkan in Graphics APIs - Quest prefers OpenGL ES 3.2",
                "Move OpenGLES3 above Vulkan or remove Vulkan");
        }

        // Check for Oculus XR package
        var pkgList = UnityEditor.PackageManager.Client.List(true);
        while (pkgList.Status == UnityEditor.PackageManager.StatusCode.InProgress) { }
        bool hasOculus = pkgList.Result?.Any(p => p.name == "com.unity.xr.oculus") == true;
        if (!hasOculus)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Android/Quest Config",
                "Oculus XR package not installed",
                "Install com.unity.xr.oculus via Package Manager or Project Bootstrap");
        }
        else
        {
            lastResult.AddPass();
        }

        // Check XR Management
        bool hasXRMgmt = pkgList.Result?.Any(p => p.name == "com.unity.xr.management") == true;
        if (!hasXRMgmt)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Android/Quest Config",
                "XR Management package not installed",
                "Install com.unity.xr.management via Package Manager");
        }

        // Check Input System
        bool hasInput = pkgList.Result?.Any(p => p.name == "com.unity.inputsystem") == true;
        if (!hasInput)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Android/Quest Config",
                "Input System package not installed",
                "Install com.unity.inputsystem for better Quest input handling");
        }

        // Check Addressables
        bool hasAddressables = pkgList.Result?.Any(p => p.name == "com.unity.addressables") == true;
        if (!hasAddressables)
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Android/Quest Config",
                "Addressables package not installed",
                "Install com.unity.addressables for asset management");
        }

        // Check URP
        bool hasURP = pkgList.Result?.Any(p => p.name == "com.unity.urp") == true;
        if (!hasURP)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "Android/Quest Config",
                "Universal Render Pipeline not installed",
                "Install com.unity.urp for Quest-optimized rendering");
        }
    }

    private void ValidateAssetBundleOutput()
    {
        lastResult.AddCategory("AssetBundle Output");

        string bundlePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BUNDLE_OUTPUT));

        if (!File.Exists(bundlePath))
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "AssetBundle Output",
                $"AssetBundle not found at {bundlePath}",
                "Run Build AssetBundle to generate");
            return;
        }

        lastResult.AddPass();

        var fileInfo = new FileInfo(bundlePath);
        lastResult.AddInfo("AssetBundle Output", $"Bundle size: {FormatBytes(fileInfo.Length)}");

        // Validate bundle
        try
        {
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                lastResult.AddIssue(ValidationSeverity.Error, "AssetBundle Output",
                    "Failed to load AssetBundle - corrupted or invalid format");
                return;
            }

            var assetNames = bundle.GetAllAssetNames();
            lastResult.AddInfo("AssetBundle Output", $"Assets in bundle: {assetNames.Length}");

            // Check for weapon prefabs
            int weaponCount = assetNames.Count(n => n.Contains("Weapon") || n.Contains("Gun") || n.Contains("Item"));
            lastResult.AddInfo("AssetBundle Output", $"Weapon/Item assets: {weaponCount}");

            // Check for materials
            int matCount = assetNames.Count(n => n.EndsWith(".mat"));
            lastResult.AddInfo("AssetBundle Output", $"Materials: {matCount}");

            // Check for shaders
            int shaderCount = assetNames.Count(n => n.EndsWith(".shader"));
            lastResult.AddInfo("AssetBundle Output", $"Shaders: {shaderCount}");

            bundle.Unload(false);
        }
        catch (Exception e)
        {
            lastResult.AddIssue(ValidationSeverity.Error, "AssetBundle Output",
                $"Bundle validation failed: {e.Message}", "");
        }
    }

    private void ValidateModPackage()
    {
        lastResult.AddCategory("Mod Package");

        string modRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        string modDir = Path.Combine(modRoot, MOD_NAME);

        if (!Directory.Exists(modDir))
        {
            lastResult.AddIssue(ValidationSeverity.Warning, "Mod Package",
                $"Mod directory not found: {modDir}",
                "Run full build to create mod package");
            return;
        }

        lastResult.AddPass();

        // Check required files
        string[] requiredFiles = { "mod.json", "icon.png", "JohnWickArsenal.dll", "johnwickarsenal.bundle", "README.md" };
        foreach (var file in requiredFiles)
        {
            string path = Path.Combine(modDir, file);
            if (File.Exists(path))
            {
                lastResult.AddPass();
                var fi = new FileInfo(path);
                lastResult.AddInfo("Mod Package", $"{file}: {FormatBytes(fi.Length)}");
            }
            else
            {
                lastResult.AddIssue(ValidationSeverity.Error, "Mod Package",
                    $"Missing required file: {file}", "");
            }
        }

        // Validate mod.json
        string modJsonPath = Path.Combine(modDir, "mod.json");
        if (File.Exists(modJsonPath))
        {
            try
            {
                var json = File.ReadAllText(modJsonPath);
                var parsed = JsonUtility.FromJson<ModJson>(json);
                if (parsed != null)
                {
                    lastResult.AddInfo("Mod Package", $"Mod ID: {parsed.id}, Version: {parsed.version}");
                    if (parsed.id != MOD_NAME)
                    {
                        lastResult.AddIssue(ValidationSeverity.Warning, "Mod Package",
                            $"Mod ID mismatch: expected '{MOD_NAME}', got '{parsed.id}'");
                    }
                }
            }
            catch
            {
                lastResult.AddIssue(ValidationSeverity.Error, "Mod Package",
                    "mod.json is invalid JSON", "");
            }
        }

        // Check for ZIP
        var zipFiles = Directory.GetFiles(modRoot, $"{MOD_NAME}_v*.zip");
        if (zipFiles.Length > 0)
        {
            var fi = new FileInfo(zipFiles[0]);
            lastResult.AddInfo("Mod Package", $"Distribution ZIP: {fi.Name} ({FormatBytes(fi.Length)})");
        }
    }

    // ========================================================================
    // HELPER CLASSES & METHODS
    // ========================================================================

    private class ValidationResult
    {
        public DateTime Timestamp;
        public TimeSpan Duration;
        public int TotalChecks = 0;
        public int PassedChecks = 0;
        public List<ValidationIssue> Issues = new List<ValidationIssue>();
        public List<string> Warnings = new List<string>();

        public bool IsValid => Issues.Count == 0;

        public void AddCategory(string name)
        {
            // Category tracking for report
        }

        public void AddPass()
        {
            TotalChecks++;
            PassedChecks++;
        }

        public void AddIssue(ValidationSeverity severity, string category, string message, string fixHint)
        {
            TotalChecks++;
            Issues.Add(new ValidationIssue
            {
                Severity = severity,
                Category = category,
                Message = message,
                FixHint = fixHint
            });
        }

        public void AddInfo(string category, string message)
        {
            // Info only
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public void Finalize()
        {
            // Finalize
        }
    }

    private class ValidationIssue
    {
        public ValidationSeverity Severity;
        public string Category;
        public string Message;
        public string FixHint;
    }

    private enum ValidationSeverity
    {
        Error,
        Warning
    }

    private class ModJson
    {
        public string id;
        public string version;
        public string name;
    }

    private string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "null";
        string path = obj.name;
        var current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:F1} {suffixes[i]}";
    }

    private void ExportReport()
    {
        if (lastResult == null) return;

        var path = Path.Combine(Application.dataPath, "..", "validation_report.txt");
        var sb = new StringBuilder();

        sb.AppendLine("JOHN WICK ARSENAL - VALIDATION REPORT");
        sb.AppendLine($"Generated: {lastResult.Timestamp}");
        sb.AppendLine($"Duration: {lastResult.Duration.TotalSeconds:F1}s");
        sb.AppendLine($"Passed: {lastResult.PassedChecks}/{lastResult.TotalChecks}");
        sb.AppendLine($"Issues: {lastResult.Issues.Count}");
        sb.AppendLine($"Warnings: {lastResult.Warnings.Count}");
        sb.AppendLine("");

        foreach (var issue in lastResult.Issues)
        {
            sb.AppendLine($"[{issue.Severity}] {issue.Category}: {issue.Message}");
            if (!string.IsNullOrEmpty(issue.FixHint))
                sb.AppendLine($"  Fix: {issue.FixHint}");
        }

        if (lastResult.Warnings.Count > 0)
        {
            sb.AppendLine("");
            sb.AppendLine("WARNINGS:");
            foreach (var w in lastResult.Warnings)
                sb.AppendLine($"  {w}");
        }

        File.WriteAllText(path, sb.ToString());
        EditorUtility.RevealInFinder(path);
    }
}