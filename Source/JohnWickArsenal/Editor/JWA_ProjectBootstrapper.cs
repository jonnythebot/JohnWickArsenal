using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// One-click Unity project setup for John Wick Arsenal mod development.
/// Configures project settings, installs packages, sets up folder structure, and validates configuration.
/// </summary>
public class JWA_ProjectBootstrapper : EditorWindow
{
    // Configuration
    private const string MOD_NAME = "JohnWickArsenal";
    private const string UNITY_VERSION = "2022.3";

    // Package requirements
    private static readonly string[] RequiredPackages = new[]
    {
        "com.unity.xr.management@4.5.0",
        "com.unity.xr.oculus@4.5.0",
        "com.unity.inputsystem@1.7.0",
        "com.unity.addressables@1.21.0",
        "com.unity.urp@14.0.8",
        "com.unity.mobile.android-logcat@1.1.0"
    };

    // Settings
    private bool installPackages = true;
    private bool configurePlayerSettings = true;
    private bool configureGraphicsSettings = true;
    private bool createFolderStructure = true;
    private bool setupAddressables = true;
    private bool validateAfterSetup = true;

    // State
    private Vector2 scrollPos;
    private List<string> setupLog = new List<string>();
    private bool isRunning = false;
    private int currentStep = 0;
    private int totalSteps = 0;

    [MenuItem("John Wick Arsenal/Project Bootstrap", priority = 0)]
    public static void ShowWindow()
    {
        var window = GetWindow<JWA_ProjectBootstrapper>("JWA Project Bootstrap");
        window.minSize = new Vector2(550, 700);
        window.Show();
    }

    private void OnEnable()
    {
        setupLog.Clear();
        Log("John Wick Arsenal Project Bootstrap");
        Log($"Target Unity Version: {UNITY_VERSION}");
        Log($"Current Unity Version: {Application.unityVersion}");
        Log("");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        DrawPackageSelection();
        DrawConfigurationOptions();
        DrawActionButtons();
        DrawLog();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("John Wick Arsenal - Project Bootstrap", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Automated project configuration for B&S Nomad mod development", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(5);

        // Version warning
        if (!Application.unityVersion.StartsWith("2022.3"))
        {
            EditorGUILayout.HelpBox($"⚠ Unity version mismatch: Expected 2022.3.x, running {Application.unityVersion}. Some features may not work correctly.", MessageType.Warning);
        }

        EditorGUILayout.Space(5);
    }

    private void DrawPackageSelection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Package Installation", EditorStyles.boldLabel);

        installPackages = EditorGUILayout.ToggleLeft("Install required packages via Package Manager", installPackages);
        if (installPackages)
        {
            EditorGUI.indentLevel++;
            foreach (var pkg in RequiredPackages)
            {
                EditorGUILayout.LabelField($"  • {pkg}", EditorStyles.miniLabel);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawConfigurationOptions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Project Configuration", EditorStyles.boldLabel);

        configurePlayerSettings = EditorGUILayout.ToggleLeft("Configure Player Settings (Android/Quest)", configurePlayerSettings);
        configureGraphicsSettings = EditorGUILayout.ToggleLeft("Configure Graphics (URP, OpenGL ES 3.2)", configureGraphicsSettings);
        createFolderStructure = EditorGUILayout.ToggleLeft("Create standard folder structure", createFolderStructure);
        setupAddressables = EditorGUILayout.ToggleLeft("Initialize Addressables system", setupAddressables);
        validateAfterSetup = EditorGUILayout.ToggleLeft("Validate configuration after setup", validateAfterSetup);

        EditorGUILayout.EndVertical();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GUI.enabled = !isRunning && !EditorApplication.isCompiling;

        if (GUILayout.Button(isRunning ? "⏳ Running..." : "🚀 Run Full Bootstrap", GUILayout.Height(45)))
        {
            RunBootstrap();
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📦 Install Packages Only"))
        {
            InstallPackagesOnly();
        }
        if (GUILayout.Button("⚙️ Configure Settings Only"))
        {
            ConfigureSettingsOnly();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📁 Create Folders Only"))
        {
            CreateFoldersOnly();
        }
        if (GUILayout.Button("✅ Validate Current Setup"))
        {
            ValidateSetup();
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📂 Open Project Folder"))
        {
            EditorUtility.RevealInFinder(Application.dataPath);
        }
        if (GUILayout.Button("🔧 Open Player Settings"))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawLog()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Setup Log", EditorStyles.boldLabel);

        var logStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 10,
            wordWrap = true,
            richText = true
        };

        var logText = string.Join("\n", setupLog);
        EditorGUILayout.TextArea(logText, logStyle, GUILayout.MinHeight(200), GUILayout.ExpandHeight(true));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Log"))
        {
            setupLog.Clear();
            Log("Log cleared");
        }
        if (GUILayout.Button("Copy to Clipboard"))
        {
            EditorGUIUtility.systemCopyBuffer = logText;
            Log("Log copied to clipboard");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        setupLog.Add($"[{timestamp}] {message}");
        Repaint();
    }

    private void RunBootstrap()
    {
        isRunning = true;
        currentStep = 0;
        totalSteps = 0;

        if (installPackages) totalSteps++;
        if (configurePlayerSettings) totalSteps++;
        if (configureGraphicsSettings) totalSteps++;
        if (createFolderStructure) totalSteps++;
        if (setupAddressables) totalSteps++;
        if (validateAfterSetup) totalSteps++;

        EditorApplication.delayCall += () => RunBootstrapAsync();
    }

    private async void RunBootstrapAsync()
    {
        try
        {
            if (installPackages)
            {
                currentStep++;
                await InstallPackagesAsync(currentStep, totalSteps);
            }

            if (configurePlayerSettings)
            {
                currentStep++;
                ConfigurePlayerSettings(currentStep, totalSteps);
            }

            if (configureGraphicsSettings)
            {
                currentStep++;
                ConfigureGraphicsSettings(currentStep, totalSteps);
            }

            if (createFolderStructure)
            {
                currentStep++;
                CreateFolderStructure(currentStep, totalSteps);
            }

            if (setupAddressables)
            {
                currentStep++;
                SetupAddressables(currentStep, totalSteps);
            }

            if (validateAfterSetup)
            {
                currentStep++;
                ValidateSetup(currentStep, totalSteps);
            }

            Log("");
            Log("✅ <b>Bootstrap completed successfully!</b>");
            Log("");
            Log("Next steps:");
            Log("  1. Import weapon models to Assets/JohnWickArsenal/Models/");
            Log("  2. Use 'Prefab Setup Wizard' to create prefabs");
            Log("  3. Use 'Build AssetBundle' to build the Android bundle");
            Log("  4. Run build.sh for full automated pipeline");

            EditorUtility.DisplayDialog("Bootstrap Complete",
                "John Wick Arsenal project has been configured!\n\n" +
                "Next steps:\n" +
                "1. Import weapon models to Assets/JohnWickArsenal/Models/\n" +
                "2. Use 'Prefab Setup Wizard' to create prefabs\n" +
                "3. Use 'Build AssetBundle' to build the bundle\n" +
                "4. Run build.sh for full pipeline",
                "OK");
        }
        catch (Exception e)
        {
            Log($"❌ <color=red>Bootstrap failed: {e.Message}</color>");
            Log($"<color=red>{e.StackTrace}</color>");
            EditorUtility.DisplayDialog("Bootstrap Failed", $"Error: {e.Message}", "OK");
        }
        finally
        {
            isRunning = false;
            Repaint();
        }
    }

    private void InstallPackagesOnly()
    {
        isRunning = true;
        EditorApplication.delayCall += async () =>
        {
            try
            {
                await InstallPackagesAsync(1, 1);
                Log("✅ Package installation complete");
            }
            catch (Exception e)
            {
                Log($"❌ Package installation failed: {e.Message}");
            }
            finally
            {
                isRunning = false;
                Repaint();
            }
        };
    }

    private void ConfigureSettingsOnly()
    {
        ConfigurePlayerSettings(1, 2);
        ConfigureGraphicsSettings(2, 2);
        Log("✅ Settings configuration complete");
    }

    private void CreateFoldersOnly()
    {
        CreateFolderStructure(1, 1);
        Log("✅ Folder structure created");
    }

    private void ValidateSetup(int step = 1, int total = 1)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Validating configuration...", (float)step / total);

        var issues = new List<string>();

        // Check packages
        foreach (var pkg in RequiredPackages)
        {
            var name = pkg.Split('@')[0];
            if (!PackageManager.PackageInfo.FindForAssetPath("Assets").name.Contains(name))
            {
                var list = Client.List(true);
                while (list.Status == StatusCode.InProgress) { }
                if (list.Result != null && !list.Result.Any(p => p.name == name))
                {
                    issues.Add($"Missing package: {name}");
                }
            }
        }

        // Check Player Settings
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            issues.Add("Build target is not Android");

        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            issues.Add("Scripting backend is not IL2CPP");

        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android) != ApiCompatibilityLevel.NET_Standard_2_1)
            issues.Add("API compatibility level is not .NET Standard 2.1");

        // Check Graphics
        var tierSettings = GraphicsSettings.GetRenderPipelineSettingsForTier(GraphicsTier.Tier1);
        if (tierSettings == null)
            issues.Add("URP not configured for Tier 1");

        // Check Addressables
        if (setupAddressables)
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                issues.Add("Addressables not initialized");
        }

        EditorUtility.ClearProgressBar();

        if (issues.Count == 0)
        {
            Log("✅ All validation checks passed");
            EditorUtility.DisplayDialog("Validation Passed", "All configuration checks passed!", "OK");
        }
        else
        {
            Log($"⚠ Found {issues.Count} issue(s):");
            foreach (var issue in issues)
                Log($"  • {issue}");
            EditorUtility.DisplayDialog("Validation Issues", $"Found {issues.Count} issue(s):\n\n{string.Join("\n", issues)}", "OK");
        }
    }

    // ========================================================================
    // IMPLEMENTATION METHODS
    // ========================================================================

    private async System.Threading.Tasks.Task InstallPackagesAsync(int step, int total)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Installing packages...", (float)step / total);

        foreach (var pkg in RequiredPackages)
        {
            var name = pkg.Split('@')[0];
            Log($"Installing {name}...");

            var request = Client.Add(pkg);
            while (request.Status == StatusCode.InProgress)
                await System.Threading.Tasks.Task.Delay(100);

            if (request.Status == StatusCode.Success)
            {
                Log($"  ✅ {name} installed");
            }
            else
            {
                Log($"  ⚠ {name}: {request.Error?.message ?? "Failed"}");
            }
        }

        // Wait for resolution
        var resolve = Client.Resolve();
        while (resolve.Status == StatusCode.InProgress)
            await System.Threading.Tasks.Task.Delay(100);

        EditorUtility.ClearProgressBar();
    }

    private void ConfigurePlayerSettings(int step, int total)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Configuring Player Settings...", (float)step / total);

        // Switch to Android
        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);

        // Basic identification
        PlayerSettings.companyName = "JohnWickArsenal";
        PlayerSettings.productName = "JohnWickArsenal";
        PlayerSettings.applicationIdentifier = "com.johnwickarsenal.mod";

        // Scripting
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_1);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);

        // Architecture
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Minimum API level (Quest 1/2 compatible)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        // Rendering
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.mobileMTRendering = true;
        PlayerSettings.gpuSkinning = true;

        // Optimization
        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.vertexChannelCompressionMask = VertexChannelCompressionMask.All;

        // XR
        PlayerSettings.virtualRealitySupported = true;

        // Keystore (debug for development)
        PlayerSettings.Android.useCustomKeystore = false;

        // Icon
        if (File.Exists(Path.Combine(Application.dataPath, "..", "icon.png")))
        {
            var iconPath = Path.Combine(Application.dataPath, "..", "icon.png");
            var iconImporter = AssetImporter.GetAtPath("icon.png") as TextureImporter;
            if (iconImporter != null)
            {
                iconImporter.textureType = TextureImporterType.Default;
                iconImporter.SaveAndReimport();
            }
        }

        Log("  ✅ Player Settings configured for Android/Quest");
        EditorUtility.ClearProgressBar();
    }

    private void ConfigureGraphicsSettings(int step, int total)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Configuring Graphics Settings...", (float)step / total);

        // Ensure URP is the active render pipeline
        var urpAsset = GraphicsSettings.defaultRenderPipeline;
        if (urpAsset == null)
        {
            Log("  ⚠ No URP asset assigned - create one in Project Settings > Graphics");
        }
        else
        {
            Log($"  ✅ URP Asset assigned: {urpAsset.name}");
        }

        // Quality settings for Quest
        var qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, true);
            // Disable expensive features for mobile
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowDistance = 0;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }

        // Set default to lowest quality for performance
        QualitySettings.SetQualityLevel(0, true);

        // Graphics tier settings
        GraphicsSettings.tierSettings = new[]
        {
            new TierSettings { renderingPath = RenderingPath.Forward, maxPixelLights = 0 },
            new TierSettings { renderingPath = RenderingPath.Forward, maxPixelLights = 1 },
            new TierSettings { renderingPath = RenderingPath.Forward, maxPixelLights = 2 }
        };

        Log("  ✅ Graphics settings optimized for Quest");
        EditorUtility.ClearProgressBar();
    }

    private void CreateFolderStructure(int step, int total)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Creating folder structure...", (float)step / total);

        string basePath = "Assets/JohnWickArsenal";
        string[] folders = new[]
        {
            "Models/Pistols",
            "Models/Shotguns",
            "Models/Revolvers",
            "Models/Rifles",
            "Models/Throwables",
            "Animations",
            "Audio/Weapons",
            "Audio/Throwables",
            "Audio/Voice",
            "Materials/Weapons",
            "Materials/Effects",
            "Prefabs/Weapons",
            "Prefabs/Throwables",
            "Resources/Weapons/Pistols",
            "Resources/Weapons/Shotguns",
            "Resources/Weapons/Revolvers",
            "Resources/Weapons/Rifles",
            "Resources/Weapons/Throwables",
            "Editor",
            "Shaders",
            "Textures/Weapons",
            "Textures/Effects",
            "Textures/UI",
            "AddressableAssets"
        };

        foreach (string folder in folders)
        {
            string fullPath = Path.Combine(basePath, folder);
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                var parts = folder.Split('/');
                string currentPath = basePath;
                foreach (var part in parts)
                {
                    string nextPath = Path.Combine(currentPath, part);
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, part);
                    }
                    currentPath = nextPath;
                }
            }
        }

        AssetDatabase.Refresh();
        Log("  ✅ Folder structure created");
        EditorUtility.ClearProgressBar();
    }

    private void SetupAddressables(int step, int total)
    {
        EditorUtility.DisplayProgressBar("Bootstrap", "Setting up Addressables...", (float)step / total);

        try
        {
            // Initialize Addressables settings
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                settings = AddressableAssetSettings.Create(
                    Path.Combine(Application.dataPath, "AddressableAssets"),
                    "AddressableAssetSettings",
                    true,
                    true
                );
                Log("  ✅ Addressables settings created");
            }
            else
            {
                Log("  ✅ Addressables already initialized");
            }

            // Create default group for weapons
            var group = settings.FindGroup("Weapons");
            if (group == null)
            {
                group = settings.CreateGroup("Weapons", false, false, true, settings.DefaultGroup.Schemas);
                Log("  ✅ 'Weapons' group created");
            }

            // Create group for throwables
            group = settings.FindGroup("Throwables");
            if (group == null)
            {
                group = settings.CreateGroup("Throwables", false, false, true, settings.DefaultGroup.Schemas);
                Log("  ✅ 'Throwables' group created");
            }

            // Set build path
            settings.BuildPath = "AssetBundles/[BuildTarget]";
            settings.LoadPath = "AssetBundles/[BuildTarget]";

            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Log($"  ⚠ Addressables setup: {e.Message}");
        }

        EditorUtility.ClearProgressBar();
    }
}