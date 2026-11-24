using UnityEditor;
using UnityEngine;

namespace JohnWickArsenal.Editor
{
    /// <summary>
    /// Cloud Build configuration for cosmetic-only builds.
    /// This script configures the project for Unity Cloud Build cosmetic-only builds.
    /// </summary>
    public static class JWA_CloudBuildConfig
    {
        // Cloud Build pre-export method
        public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            // Add cosmetic-only define symbol for cloud build
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!defines.Contains("JWA_COSMETIC_ONLY"))
            {
                if (!string.IsNullOrEmpty(defines))
                    defines += ";";
                defines += "JWA_COSMETIC_ONLY";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
                Debug.Log("[JWA_CloudBuildConfig] Added JWA_COSMETIC_ONLY define for cloud build");
            }

            // Set Android build settings for cosmetic preview
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.Android, AndroidArchitecture.ARM64);
            
            // Disable development build for preview
            EditorUserBuildSettings.development = false;
            
            Debug.Log("[JWA_CloudBuildConfig] Cloud build configuration applied for cosmetic preview");
        }

        // Cloud Build post-export method
        public static void PostExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            Debug.Log($"[JWA_CloudBuildConfig] Cloud build completed: {manifest.GetValue<string>("buildNumber")}");
        }
    }
}
