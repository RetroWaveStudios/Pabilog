using UnityEngine;
using UnityEditor;
public class CustomSpriteImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        // Apply only to sprites
        if (importer.textureType == TextureImporterType.Sprite)
        {
            // Different rules for UI vs character packs
            if (assetPath.Contains("Character Pack 1"))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = 2048; // allow bigger size for characters
            }
            else
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point; // good for pixel art
                importer.textureCompression = TextureImporterCompression.Uncompressed; // keep UI sharp
                importer.maxTextureSize = 1024; // don’t shrink UI too much
            }

            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;

            // Keep sprite scaling sane
            importer.spritePixelsPerUnit = 1400;
        }
    }
}