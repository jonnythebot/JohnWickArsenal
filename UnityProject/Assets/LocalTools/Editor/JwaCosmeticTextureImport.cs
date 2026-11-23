using System;
using System.IO;
using UnityEditor;

namespace JohnWickArsenal.LocalTools
{
    internal sealed class JwaCosmeticTextureImport : AssetPostprocessor
    {
        private const string ModelRoot = "Assets/JohnWickArsenal/Models/";
        private const int SizeLimit = 1024;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ModelRoot + "Cosmetics/", StringComparison.Ordinal)
                && assetPath != ModelRoot + "Suits/ClassicBlackSuit_Albedo.png"
                && assetPath != ModelRoot + "Suits/ClassicBlackSuit_Bump.png")
                return;

            var importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = SizeLimit;
            string name = Path.GetFileNameWithoutExtension(assetPath);
            if (name.EndsWith("_Normal", StringComparison.Ordinal))
                importer.textureType = TextureImporterType.NormalMap;
            if (name.EndsWith("_Bump", StringComparison.Ordinal))
                importer.sRGBTexture = false;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = SizeLimit;
            android.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(android);
        }
    }
}
