using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// Keeps the custom map illustrations imported as compact UI sprites.
    /// </summary>
    public static class StationBadgeArtworkImporter
    {
        private const string ArtworkFolder = "Assets/Resources/Artwork/StationBadges";

        [MenuItem("Suricatus/Artwork/Importar emblemas da caça QR")]
        public static void Configure()
        {
            ConfigureSprites();
            EditorUtility.DisplayDialog("Artes da caça QR",
                "Os novos emblemas foram importados como sprites para a interface.", "OK");
        }

        /// <summary>Batch entry point used before WebGL builds.</summary>
        public static void ConfigureForBuild()
        {
            ConfigureSprites();
            EditorApplication.Exit(0);
        }

        private static void ConfigureSprites()
        {
            if (!Directory.Exists(ArtworkFolder))
            {
                Debug.LogError($"[StationBadgeArtworkImporter] Pasta não encontrada: {ArtworkFolder}");
                return;
            }

            foreach (var absolutePath in Directory.GetFiles(ArtworkFolder, "*.png"))
            {
                var path = absolutePath.Replace('\\', '/');
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.compressionQuality = 100;
                importer.maxTextureSize = 512;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[StationBadgeArtworkImporter] Emblemas importados como sprites.");
        }
    }
}
