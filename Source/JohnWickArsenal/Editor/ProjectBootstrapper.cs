using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// One-click Unity project setup for John Wick Arsenal mod development.
    /// Configures project settings, installs packages, sets up folder structure.
    /// </summary>
    public class ProjectBootstrapper : EditorWindow
    {
        private Vector2 scrollPos;
        private bool installXRPackages = true;
        private bool installAddressables = true;
        private bool installInputSystem = true;
        private bool setupProjectSettings = true;
        private bool createFolderStructure = true;
        private bool copySourceCode = true;
        private bool setupBuildPipeline = true;
        private string unityVersion = "2022.3.20f1";

        [MenuItem("John Wick Arsenal/Bootstrap Unity Project", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectBootstrapper>("JWA Project Bootstrap");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(10);
            DrawHeader();

            EditorGUILayout.Space(10);
            DrawPackageSelection();

            EditorGUILayout.Space(10);
            DrawSetupOptions();

            EditorGUILayout.Space(10);
            DrawBootstrapAction();

            EditorGUILayout.Space(10);
            DrawStatus();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("John Wick Arsenal - Unity Project Bootstrap", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Target Unity Version: {unityVersion}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("This will configure your project for B&S Nomad mod development", EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawPackageSelection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Packages to Install", EditorStyles.boldLabel);

            installXRPackages = EditorGUILayout.ToggleLeft("XR Management + Oculus (Meta)", installXRPackages);
            EditorGUILayout.LabelField("  Required for Quest/Standalone builds", EditorStyles.miniLabel);

            installAddressables = EditorGUILayout.ToggleLeft("Addressables", installAddressables);
            EditorGUILayout.LabelField("  For asset management (optional)", EditorStyles.miniLabel);

            installInputSystem = EditorGUILayout.ToggleLeft("Input System", installInputSystem);
            EditorGUILayout.LabelField("  New input system (recommended)", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawSetupOptions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Project Configuration", EditorStyles.boldLabel);

            setupProjectSettings = EditorGUILayout.ToggleLeft("Configure Project Settings", setupProjectSettings);
            EditorGUILayout.LabelField("  Graphics API, Scripting Backend, Player Settings", EditorStyles.miniLabel);

            createFolderStructure = EditorGUILayout.ToggleLeft("Create Folder Structure", createFolderStructure);
            EditorGUILayout.LabelField("  Assets/JohnWickArsenal/{Models,Animations,Audio,Materials,Prefabs,Resources,Editor}", EditorStyles.miniLabel);

            copySourceCode = EditorGUILayout.ToggleLeft("Link Source Code", copySourceCode);
            EditorGUILayout.LabelField("  Symlink Source/JohnWickArsenal to Assets/Scripts/JohnWickArsenal", EditorStyles.miniLabel);

            setupBuildPipeline = EditorGUILayout.ToggleLeft("Setup Build Pipeline", setupBuildPipeline);
            EditorGUILayout.LabelField("  Configure BuildAssetBundles.cs for this project", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawBootstrapAction()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Action", EditorStyles.boldLabel);

            GUI.enabled = !EditorApplication.isCompiling;

            if (GUILayout.Button("🚀 Bootstrap Project", GUILayout.Height(50)))
            {
                BootstrapProject();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📁 Open Project Folder"))
            {
                EditorUtility.RevealInFinder(Application.dataPath);
            }
            if (GUILayout.Button("⚙️ Open Project Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatus()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Current Project Status", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Unity Version: {Application.unityVersion}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Platform: {EditorUserBuildSettings.activeBuildTarget}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Graphics API: {PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget)[0]}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Scripting Backend: {PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.activeBuildTarget)}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"API Compatibility: {PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.activeBuildTarget)}", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void BootstrapProject()
        {
            try
            {
                int step = 0;
                int totalSteps = 0;
                if (installXRPackages) totalSteps++;
                if (installAddressables) totalSteps++;
                if (installInputSystem) totalSteps++;
                if (setupProjectSettings) totalSteps++;
                if (createFolderStructure) totalSteps++;
                if (copySourceCode) totalSteps++;
                if (setupBuildPipeline) totalSteps++;

                if (installXRPackages) InstallXRPackages(++step, totalSteps);
                if (installAddressables) InstallAddressables(++step, totalSteps);
                if (installInputSystem) InstallInputSystem(++step, totalSteps);
                if (setupProjectSettings) ConfigureProjectSettings(++step, totalSteps);
                if (createFolderStructure) CreateFolderStructure(++step, totalSteps);
                if (copySourceCode) LinkSourceCode(++step, totalSteps);
                if (setupBuildPipeline) SetupBuildPipeline(++step, totalSteps);

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Bootstrap Complete",
                    "John Wick Arsenal project has been configured!\n\n" +
                    "Next steps:\n" +
                    "1. Import your weapon models to Assets/JohnWickArsenal/Models/\n" +
                    "2. Use 'Prefab Setup Wizard' to create prefabs\n" +
                    "3. Use 'Build AssetBundle' to build the bundle\n" +
                    "4. Run build.sh for full pipeline",
                    "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA Bootstrap] Failed: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Bootstrap Failed", $"Error: {e.Message}", "OK");
            }
        }

        private void InstallXRPackages(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Installing XR Packages...", (float)step / total);

            AddPackage("com.unity.xr.management@4.5.0");
            AddPackage("com.unity.xr.oculus@4.5.0");

            // Wait for completion
            while (Client.GetPackageList().Status == StatusCode.InProgress) { }
        }

        private void InstallAddressables(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Installing Addressables...", (float)step / total);
            AddPackage("com.unity.addressables@1.21.0");
            while (Client.GetPackageList().Status == StatusCode.InProgress) { }
        }

        private void InstallInputSystem(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Installing Input System...", (float)step / total);
            AddPackage("com.unity.inputsystem@1.7.0");
            while (Client.GetPackageList().Status == StatusCode.InProgress) { }

            // Enable new input system
            PlayerSettings.useLegacyInput = false;
        }

        private void AddPackage(string packageSpec)
        {
            var request = Client.Add(packageSpec);
            while (request.Status == StatusCode.InProgress) { }
            if (request.Status == StatusCode.Failure)
            {
                Debug.LogWarning($"[JWA Bootstrap] Package install failed: {request.Error.message}");
            }
            else
            {
                Debug.Log($"[JWA Bootstrap] Installed: {packageSpec}");
            }
        }

        private void ConfigureProjectSettings(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Configuring Project Settings...", (float)step / total);

            // Target Android
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);

            // Player Settings for Quest/Standalone
            PlayerSettings.companyName = "JohnWickArsenal";
            ProductName = "JohnWickArsenal";
            PlayerSettings.applicationIdentifier = "com.johnwickarsenal.mod";

            // Graphics - OpenGL ES 3.2 for Quest
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.gpuSkinning = true;

            // Scripting - IL2CPP for Quest
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_1);
            PlayerSettings.targetArchitectures = AndroidArchitecture.ARM64;

            // Minimum API level
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29; // Quest 1/2

            // Rendering - URP
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.mobileMTRendering = true;

            // Optimization
            PlayerSettings.stripUnusedMeshComponents = true;
            PlayerSettings.vertexChannelCompressionMask = VertexChannelCompressionMask.All;

            // XR Settings
            PlayerSettings.virtualRealitySupported = true;

            // Keystore (use debug for development)
            PlayerSettings.Android.useCustomKeystore = false;

            Debug.Log("[JWA Bootstrap] Project settings configured for Android/Quest");
        }

        private void CreateFolderStructure(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Creating Folder Structure...", (float)step / total);

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
                "Textures/UI"
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
            Debug.Log("[JWA Bootstrap] Folder structure created");
        }

        private void LinkSourceCode(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Linking Source Code...", (float)step / total);

            string sourcePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Source", "JohnWickArsenal"));
            string targetPath = "Assets/Scripts/JohnWickArsenal";

            if (Directory.Exists(sourcePath))
            {
                // Create directory if needed
                if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scripts");
                }

                // On Windows, use junction; on Mac/Linux, use symlink
                // For simplicity, just copy (or user can symlink manually)
                string destPath = Path.Combine(Application.dataPath, "Scripts", "JohnWickArsenal");

                if (!Directory.Exists(destPath))
                {
                    CopyDirectory(sourcePath, destPath);
                    Debug.Log("[JWA Bootstrap] Source code copied to Assets/Scripts/JohnWickArsenal");
                }
                else
                {
                    Debug.Log("[JWA Bootstrap] Source code already exists at destination");
                }
            }
            else
            {
                Debug.LogWarning("[JWA Bootstrap] Source path not found: " + sourcePath);
                Debug.LogWarning("Expected at: /home/wolfwolfwolf8586/JohnWickArsenal/Source/JohnWickArsenal");
            }

            AssetDatabase.Refresh();
        }

        private void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (string file in Directory.GetFiles(source))
            {
                string destFile = Path.Combine(dest, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            foreach (string dir in Directory.GetDirectories(source))
            {
                CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
            }
        }

        private void SetupBuildPipeline(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Setting Up Build Pipeline...", (float)step / total);

            // The BuildAssetBundles.cs and PrefabSetupWizard.cs are already in the Editor folder
            // Just verify they compile
            AssetDatabase.Refresh();

            // Force script recompilation
            AssetDatabase.ImportAsset("Assets/Scripts/JohnWickArsenal/Editor/BuildAssetBundles.cs", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/Scripts/JohnWickArsenal/Editor/PrefabSetupWizard.cs", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/Scripts/JohnWickArsenal/Editor/ProjectBootstrapper.cs", ImportAssetOptions.ForceUpdate);

            Debug.Log("[JWA Bootstrap] Build pipeline scripts ready");
        }
    }
}