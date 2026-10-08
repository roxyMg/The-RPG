// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;

// /// <summary>
// /// Applies safe pixel-art import settings to PNG files inside
// /// Assets/HarborTileset_Batch01.
// /// </summary>
// public sealed class HarborPixelArtImporter : AssetPostprocessor
// {
//     private const string PackPath = "Assets/HarborTileset_Batch01/";

//     private void OnPreprocessTexture()
//     {
//         if (!assetPath.StartsWith(PackPath) || !assetPath.EndsWith(".png"))
//             return;

//         TextureImporter importer = (TextureImporter)assetImporter;
//         importer.textureType = TextureImporterType.Sprite;
//         importer.spriteImportMode = assetPath.Contains("/Sprites/Atlas/")
//             ? SpriteImportMode.Multiple
//             : SpriteImportMode.Single;
//         importer.spritePixelsPerUnit = 16f;
//         importer.spriteMeshType = SpriteMeshType.FullRect;
//         importer.mipmapEnabled = false;
//         importer.alphaIsTransparency = true;
//         importer.filterMode = FilterMode.Point;
//         importer.wrapMode = TextureWrapMode.Clamp;
//         importer.npotScale = TextureImporterNPOTScale.None;
//         importer.textureCompression = TextureImporterCompression.Uncompressed;
//     }
// }
// #endif

