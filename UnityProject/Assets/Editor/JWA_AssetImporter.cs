using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Automated asset import pipeline for John Wick Arsenal.
/// Handles model, texture, audio, and animation import settings optimized for Quest.
/// </summary>
public class JWA_AssetImporter : AssetPostprocessor
{
    private const string MODEL_PATH = "Assets/JohnWickArsenal/Models/";
    private const string TEXTURE_PATH = "Assets/JohnWickArsenal/Textures/";
    private const string AUDIO_PATH = "Assets/JohnWickArsenal/Audio/";
    private const string ANIMATION_PATH = "Assets/JohnWickArsenal/Animations/";

    // ========================================================================
    // MODEL IMPORT SETTINGS
    // ========================================================================

    private void OnPreprocessModel()
    {
        if (!assetPath.StartsWith(MODEL_PATH)) return;

        var importer = assetImporter as ModelImporter;
        if (importer == null) return;

        // General settings optimized for Quest
        importer.importVisibility = ModelImporterVisibility.Visible;
        importer.importBlendShapes = false;
        importer.importCameras = false;
        importer.importLights = false;

        // Scale
        importer.globalScale = 1f;
        importer.fileScale = 1f;
        importer.unitConversion = ModelImporterUnitConversion.ConvertToMeters;

        // Mesh compression
        importer.meshCompression = ModelImporterMeshCompression.Medium;

        // Optimize mesh
        importer.optimizeMeshOrder = true;
        importer.optimizeMeshPolygonOrder = true;

        // Read/Write disabled for performance (enable only if needed for runtime modification)
        importer.isReadable = false;

        // Tangents for normal mapping
        importer.importTangents = ModelImporterTangents.CalculateMikk;

        // UVs
        importer.importUvs = true;
        importer.secondaryUvs = true;
        importer.generateSecondaryUV = false;

        // Normals
        importer.importNormals = ModelImporterNormals.Calculate;
        importer.normalSmoothingAngle = 60f;
        importer.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;

        // Materials
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.materialImportMode = ModelImporterMaterialImportMode.DoNotImport;

        // Animation (if any)
        importer.importAnimation = false;

        // Rig
        importer.animationType = ModelImporterAnimationType.None;

        // Avatar (not needed for weapons)
        importer.importAnimations = false;

        // LOD (optional - only if model has LOD groups)
        // importer.lodType = ModelImporterLODType.None;

        Debug.Log($"[JWA AssetImporter] Configured model import: {assetPath}");
    }

    // ========================================================================
    // TEXTURE IMPORT SETTINGS
    // ========================================================================

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(TEXTURE_PATH)) return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        // Determine texture type from folder/name
        var textureType = DetectTextureType(assetPath);

        importer.textureType = textureType;
        importer.sRGBTexture = textureType == TextureImporterType.Default ||
                                textureType == TextureImporterType.Sprite ||
                                textureType == TextureImporterType.GUI;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = textureType != TextureImporterType.NormalMap;
        importer.streamingMipmaps = false;
        importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
        importer.mipmapFadeDistanceStart = 1;
        importer.mipmapFadeDistanceEnd = 3;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 0; // Disabled for Quest performance

        // Platform-specific settings for Android
        var androidSettings = new TextureImporterPlatformSettings
        {
            name = "Android",
            overridden = true,
            maxTextureSize = GetMaxTextureSize(textureType),
            resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
            textureCompression = TextureImporterCompression.ASTC_6x6,
            compressionQuality = 50,
            crunchedCompression = textureType != TextureImporterType.NormalMap,
            format = GetTextureFormat(textureType),
            allowAlphaSplitting = false
        };

        importer.SetPlatformTextureSettings(androidSettings);

        // Also set default (fallback)
        var defaultSettings = new TextureImporterPlatformSettings
        {
            name = "Standalone",
            overridden = true,
            maxTextureSize = 2048,
            textureCompression = TextureImporterCompression.Compressed,
            compressionQuality = 50
        };
        importer.SetPlatformTextureSettings(defaultSettings);

        Debug.Log($"[JWA AssetImporter] Configured texture import: {assetPath} as {textureType}");
    }

    private TextureImporterType DetectTextureType(string path)
    {
        string lowerPath = path.ToLower();

        if (lowerPath.Contains("_normal") || lowerPath.Contains("_n.") || lowerPath.Contains("normalmap"))
            return TextureImporterType.NormalMap;

        if (lowerPath.Contains("_orm") || lowerPath.Contains("_occlusion") ||
            lowerPath.Contains("_roughness") || lowerPath.Contains("_metallic") ||
            lowerPath.Contains("_ao") || lowerPath.Contains("packed"))
            return TextureImporterType.Default; // Packed maps

        if (lowerPath.Contains("_emissive") || lowerPath.Contains("_emission"))
            return TextureImporterType.Default;

        if (lowerPath.Contains("_mask") || lowerPath.Contains("_alpha"))
            return TextureImporterType.Mask;

        if (lowerPath.Contains("_detail"))
            return TextureImporterType.Default;

        if (lowerPath.Contains("ui") || lowerPath.Contains("icon"))
            return TextureImporterType.Sprite;

        // Default for albedo, etc.
        return TextureImporterType.Default;
    }

    private int GetMaxTextureSize(TextureImporterType type)
    {
        switch (type)
        {
            case TextureImporterType.NormalMap:
                return 1024; // Normal maps don't need high res
            case TextureImporterType.Sprite:
            case TextureImporterType.GUI:
                return 512;
            case TextureImporterType.Mask:
                return 512;
            default:
                return 2048; // Albedo, ORM, Emissive
        }
    }

    private TextureImporterFormat GetTextureFormat(TextureImporterType type)
    {
        // ASTC formats for Quest (Android)
        switch (type)
        {
            case TextureImporterType.NormalMap:
                return TextureImporterFormat.ASTC_6x6; // Normal maps need precision
            case TextureImporterType.Mask:
                return TextureImporterFormat.ASTC_6x6;
            default:
                return TextureImporterFormat.ASTC_6x6;
        }
    }

    // ========================================================================
    // AUDIO IMPORT SETTINGS
    // ========================================================================

    private void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith(AUDIO_PATH)) return;

        var importer = assetImporter as AudioImporter;
        if (importer == null) return;

        var settings = importer.defaultSampleSettings;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.75f; // Good quality/size balance for Quest
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        settings.loadType = AudioClipLoadType.CompressedInMemory;
        importer.defaultSampleSettings = settings;

        // Override for Android
        var androidSettings = importer.GetOverrideSampleSettings("Android");
        androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
        androidSettings.quality = 0.7f;
        androidSettings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        androidSettings.loadType = AudioClipLoadType.CompressedInMemory;
        importer.SetOverrideSampleSettings("Android", androidSettings);

        Debug.Log($"[JWA AssetImporter] Configured audio import: {assetPath}");
    }

    // ========================================================================
    // ANIMATION IMPORT SETTINGS (if needed)
    // ========================================================================

    private void OnPreprocessAnimation()
    {
        if (!assetPath.StartsWith(ANIMATION_PATH)) return;

        var importer = assetImporter as ModelImporter;
        if (importer == null) return;

        // Animation settings for weapon animations
        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.animationCompression = ModelImporterAnimationCompression.Optimal;
        importer.animationRotationError = 0.5f;
        importer.animationPositionError = 0.5f;
        importer.animationScaleError = 0.5f;

        Debug.Log($"[JWA AssetImporter] Configured animation import: {assetPath}");
    }

    // ========================================================================
    // MENU ITEMS
    // ========================================================================

    [MenuItem("John Wick Arsenal/Reimport All Assets", priority = 50)]
    public static void ReimportAllAssets()
    {
        string[] paths = {
            MODEL_PATH,
            TEXTURE_PATH,
            AUDIO_PATH,
            ANIMATION_PATH
        };

        foreach (var path in paths)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Reimport Complete", "All JWA assets have been reimported with optimized settings.", "OK");
    }

    [MenuItem("John Wick Arsenal/Verify Import Settings", priority = 51)]
    public static void VerifyImportSettings()
    {
        var issues = new List<string>();

        // Check models
        var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { MODEL_PATH });
        foreach (var guid in modelGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                if (importer.isReadable) issues.Add($"{path}: isReadable should be false for performance");
                if (importer.meshCompression == ModelImporterMeshCompression.Off) issues.Add($"{path}: meshCompression should be Medium/High");
            }
        }

        // Check textures
        var textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { TEXTURE_PATH });
        foreach (var guid in textureGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                var androidSettings = importer.GetPlatformTextureSettings("Android");
                if (!androidSettings.overridden) issues.Add($"{path}: Android settings not overridden");
                if (androidSettings.textureCompression != TextureImporterCompression.ASTC_6x6)
                    issues.Add($"{path}: Should use ASTC_6x6 compression for Quest");
                if (androidSettings.maxTextureSize > 2048)
                    issues.Add($"{path}: Max texture size too high for Quest");
            }
        }

        // Check audio
        var audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { AUDIO_PATH });
        foreach (var guid in audioGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                var settings = importer.defaultSampleSettings;
                if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
                    issues.Add($"{path}: Should use Vorbis compression");
            }
        }

        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog("Import Settings Verified", "✅ All assets have correct import settings for Quest!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Import Settings Issues",
                $"Found {issues.Count} issue(s):\n\n{string.Join("\n", issues.Take(20))}" +
                (issues.Count > 20 ? $"\n... and {issues.Count - 20} more" : ""),
                "OK");
        }
    }
}