using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Automatic project configuration for John Wick Arsenal.
/// Runs on Unity startup and after package installation to ensure correct setup.
/// </summary>
[InitializeOnLoad]
public class JWA_AutoConfiguration
{
    private const string MOD_NAME = "JohnWickArsenal";
    private const string URP_ASSET_PATH = "Assets/JohnWickArsenal/URP/JohnWickArsenal_URP.asset";
    private const string URP_RENDERER_PATH = "Assets/JohnWickArsenal/URP/ForwardRenderer.asset";

    static JWA_AutoConfiguration()
    {
        EditorApplication.delayCall += RunAutoConfiguration;
    }

    private static void RunAutoConfiguration()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunAutoConfiguration;
            return;
        }

        try
        {
            bool madeChanges = false;

            // 1. Ensure build target is Android
            madeChanges |= EnsureAndroidBuildTarget();

            // 2. Ensure URP asset exists and is assigned
            madeChanges |= EnsureURPAsset();

            // 3. Ensure required packages are installed
            madeChanges |= EnsurePackagesInstalled();

            // 4. Ensure Addressables initialized
            madeChanges |= EnsureAddressablesInitialized();

            // 5. Ensure folder structure exists
            madeChanges |= EnsureFolderStructure();

            // 6. Ensure scripting backend settings
            madeChanges |= EnsurePlayerSettings();

            // 7. Ensure Quality settings for Quest
            madeChanges |= EnsureQualitySettings();

            // 8. Ensure Graphics APIs
            madeChanges |= EnsureGraphicsAPIs();

            // 9. Ensure XR settings
            madeChanges |= EnsureXRSettings();

            if (madeChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[JWA AutoConfig] Configuration complete. Project ready for {MOD_NAME} development.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JWA AutoConfig] Configuration failed: {e.Message}\n{e.StackTrace}");
        }
    }

    private static bool EnsureAndroidBuildTarget()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("[JWA AutoConfig] Switching build target to Android...");
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
            return true;
        }
        return false;
    }

    private static bool EnsureURPAsset()
    {
        bool changed = false;

        // Create URP directory if needed
        string urpDir = Path.GetDirectoryName(URP_ASSET_PATH);
        if (!AssetDatabase.IsValidFolder(urpDir))
        {
            Directory.CreateDirectory(urpDir);
            AssetDatabase.Refresh();
        }

        // Check if URP asset exists
        var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(URP_ASSET_PATH);
        if (urpAsset == null)
        {
            Debug.Log("[JWA AutoConfig] Creating URP Asset for Quest...");
            urpAsset = CreateURPAsset();
            changed = true;
        }

        // Assign as default render pipeline
        if (GraphicsSettings.defaultRenderPipeline != urpAsset)
        {
            Debug.Log("[JWA AutoConfig] Assigning URP Asset as default render pipeline...");
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            changed = true;
        }

        // Verify quality tier settings use URP
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            var tierSettings = GraphicsSettings.GetRenderPipelineSettingsForTier((GraphicsTier)i);
            if (tierSettings == null || tierSettings.renderPipelineAsset != urpAsset)
            {
                GraphicsSettings.SetRenderPipelineSettingsForTier((GraphicsTier)i, new TierSettings
                {
                    renderingPath = RenderingPath.Forward,
                    renderPipelineAsset = urpAsset,
                    maxPixelLights = i == 0 ? 0 : (i == 1 ? 1 : 2)
                });
                changed = true;
            }
        }

        return changed;
    }

    private static UniversalRenderPipelineAsset CreateURPAsset()
    {
        var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
        urpAsset.name = "JohnWickArsenal_URP";

        // Configure for Quest/Quest 2 performance
        urpAsset.renderPipelineSettings = new UniversalRenderPipelineSettings
        {
            // Rendering
            renderPipelineShaderVariantStripping = ShaderVariantStripping.Default,
            srpBatcher = true,
            useTextureArrays = true,

            // Lighting (minimal for Quest)
            mainLightRenderingMode = LightRenderingMode.PerPixel,
            additionalLightsRenderingMode = LightRenderingMode.Disabled,
            maxAdditionalLightsCount = 0,
            maxVisibleAdditionalLights = 0,
            reflectionProbeBoxProjection = false,
            reflectionProbeBlending = false,

            // Shadows (disabled for performance)
            shadowType = ShadowType.SoftShadows,
            shadowCascadeCount = 1,
            shadowDistance = 0f,
            shadowDepthBias = 1f,
            shadowNormalBias = 1f,
            softShadowsSupported = false,

            // Post-processing (disabled for Quest)
            postProcessingEnabled = false,
            postProcessingFeatureSet = PostProcessingFeatureSet.PostProcessingV2,

            // Camera
            cameraStackingEnabled = true,

            // Terrain
            terrainHolesFeatureEnabled = false,

            // Decals
            decalsEnabled = false,

            // Motion vectors
            motionVectorsDisabled = true,

            // Ray tracing (disabled)
            rayTracingSupported = false,

            // Shader variant limits
            maxShaderVariantCount = 128
        };

        // Create Forward Renderer
        var rendererData = ScriptableObject.CreateInstance<ForwardRendererData>();
        rendererData.name = "ForwardRenderer_Quest";

        // Configure renderer for Quest
        var renderer = new UniversalRenderer(rendererData);
        rendererData.SetRendererFeatures(new System.Collections.Generic.List<ScriptableRendererFeature>());

        // Set default renderer
        urpAsset.scriptableRendererDataList = new[] { rendererData };

        AssetDatabase.CreateAsset(urpAsset, URP_ASSET_PATH);
        AssetDatabase.CreateAsset(rendererData, URP_RENDERER_PATH);
        AssetDatabase.AddObjectToAsset(rendererData, urpAsset);

        Debug.Log($"[JWA AutoConfig] Created URP Asset at {URP_ASSET_PATH}");
        return urpAsset;
    }

    private static bool EnsurePackagesInstalled()
    {
        // This is informational - packages should be installed via Package Manager or Bootstrap
        // We just verify and warn if missing
        var requiredPackages = new[]
        {
            "com.unity.xr.management",
            "com.unity.xr.oculus",
            "com.unity.inputsystem",
            "com.unity.addressables",
            "com.unity.urp",
            "com.unity.mobile.android-logcat"
        };

        bool hasMissing = false;
        var listRequest = Client.List(true);
        while (listRequest.Status == StatusCode.InProgress) { }

        if (listRequest.Result != null)
        {
            var installed = listRequest.Result.Select(p => p.name).ToHashSet();
            foreach (var pkg in requiredPackages)
            {
                if (!installed.Contains(pkg))
                {
                    Debug.LogWarning($"[JWA AutoConfig] Missing package: {pkg}. Run 'John Wick Arsenal > Project Bootstrap' to install.");
                    hasMissing = true;
                }
            }
        }

        return hasMissing;
    }

    private static bool EnsureAddressablesInitialized()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.Log("[JWA AutoConfig] Initializing Addressables...");
            settings = AddressableAssetSettings.Create(
                Path.Combine(Application.dataPath, "AddressableAssets"),
                "AddressableAssetSettings",
                true,
                true
            );
            return true;
        }
        return false;
    }

    private static bool EnsureFolderStructure()
    {
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
            "AddressableAssets",
            "URP"
        };

        bool created = false;
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
                        created = true;
                    }
                    currentPath = nextPath;
                }
            }
        }

        if (created)
        {
            AssetDatabase.Refresh();
            Debug.Log("[JWA AutoConfig] Folder structure created/verified");
        }
        return created;
    }

    private static bool EnsurePlayerSettings()
    {
        bool changed = false;

        // Scripting backend
        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            changed = true;
        }

        // API compatibility
        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android) != ApiCompatibilityLevel.NET_Standard_2_1)
        {
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_1);
            changed = true;
        }

        // Architecture
        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            changed = true;
        }

        // SDK versions
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            changed = true;
        }

        if (PlayerSettings.Android.targetSdkVersion < AndroidSdkVersions.AndroidApiLevel30)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
            changed = true;
        }

        // Color space
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            changed = true;
        }

        // Stripping
        if (PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) != ManagedStrippingLevel.Medium)
        {
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            changed = true;
        }

        // XR
        if (!PlayerSettings.virtualRealitySupported)
        {
            PlayerSettings.virtualRealitySupported = true;
            changed = true;
        }

        // Product info
        if (PlayerSettings.productName != MOD_NAME)
        {
            PlayerSettings.productName = MOD_NAME;
            changed = true;
        }

        if (PlayerSettings.companyName != MOD_NAME)
        {
            PlayerSettings.companyName = MOD_NAME;
            changed = true;
        }

        if (PlayerSettings.applicationIdentifier != "com.johnwickarsenal.mod")
        {
            PlayerSettings.applicationIdentifier = "com.johnwickarsenal.mod";
            changed = true;
        }

        return changed;
    }

    private static bool EnsureQualitySettings()
    {
        bool changed = false;

        // Disable shadows and expensive features for all quality levels
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, true);

            if (QualitySettings.shadows != ShadowQuality.Disable)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                changed = true;
            }
            if (QualitySettings.shadowResolution != ShadowResolution.Low)
            {
                QualitySettings.shadowResolution = ShadowResolution.Low;
                changed = true;
            }
            if (QualitySettings.shadowDistance != 0)
            {
                QualitySettings.shadowDistance = 0;
                changed = true;
            }
            if (QualitySettings.pixelLightCount != 0)
            {
                QualitySettings.pixelLightCount = 0;
                changed = true;
            }
            if (QualitySettings.anisotropicFiltering != AnisotropicFiltering.Disable)
            {
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                changed = true;
            }
        }

        // Set to lowest quality
        QualitySettings.SetQualityLevel(0, true);

        return changed;
    }

    private static bool EnsureGraphicsAPIs()
    {
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        bool hasGLES3 = apis.Contains(GraphicsDeviceType.OpenGLES3);

        if (!hasGLES3)
        {
            Debug.Log("[JWA AutoConfig] Adding OpenGL ES 3.2 to Graphics APIs...");
            var newApis = new[] { GraphicsDeviceType.OpenGLES3 };
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, newApis);
            return true;
        }

        // Ensure GLES3 is first (preferred)
        if (apis[0] != GraphicsDeviceType.OpenGLES3)
        {
            var newApis = new[] { GraphicsDeviceType.OpenGLES3 };
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, newApis);
            return true;
        }

        return false;
    }

    private static bool EnsureXRSettings()
    {
        // This is handled by XR Management package
        // We just verify it's enabled in PlayerSettings
        if (!PlayerSettings.virtualRealitySupported)
        {
            PlayerSettings.virtualRealitySupported = true;
            return true;
        }
        return false;
    }

    // ========================================================================
    // MENU ITEMS FOR MANUAL TRIGGER
    // ========================================================================

    [MenuItem("John Wick Arsenal/Auto-Configure Project", priority = 1)]
    public static void ManualConfigure()
    {
        Debug.Log("[JWA AutoConfig] Manual configuration triggered...");
        RunAutoConfiguration();
        EditorUtility.DisplayDialog("Auto-Configuration",
            "Project configuration complete!\n\n" +
            "If any packages were missing, run 'Project Bootstrap' to install them.\n\n" +
            "Next steps:\n" +
            "1. Import weapon models to Assets/JohnWickArsenal/Models/\n" +
            "2. Use 'Prefab Setup Wizard' to create prefabs\n" +
            "3. Use 'Build AssetBundle' to build the Android bundle", "OK");
    }

    [MenuItem("John Wick Arsenal/Verify Configuration", priority = 2)]
    public static void VerifyConfiguration()
    {
        var issues = new System.Collections.Generic.List<string>();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            issues.Add("Build target is not Android");

        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            issues.Add("Scripting backend is not IL2CPP");

        if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android) != ApiCompatibilityLevel.NET_Standard_2_1)
            issues.Add("API compatibility level is not .NET Standard 2.1");

        if (GraphicsSettings.defaultRenderPipeline == null)
            issues.Add("No URP asset assigned");

        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
            issues.Add("ARM64 not enabled");

        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        if (!apis.Contains(GraphicsDeviceType.OpenGLES3))
            issues.Add("OpenGL ES 3.2 not in Graphics APIs");

        if (!PlayerSettings.virtualRealitySupported)
            issues.Add("Virtual Reality not supported");

        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog("Configuration Verified", "✅ All checks passed!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Configuration Issues",
                $"Found {issues.Count} issue(s):\n\n{string.Join("\n", issues)}",
                "OK");
        }
    }
}