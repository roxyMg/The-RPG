// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;

// /// <summary>
// /// Applies safe pixel-art defaults to every PNG in this package.
// /// Standalone sprites are immediately usable. Atlas PNGs remain a single
// /// sprite until the user chooses Multiple and slices them on a 16x16 grid.
// /// </summary>
// public sealed class HarborPixelArtImporter : AssetPostprocessor
// {
//     private const string PackageRoot = "Assets/HarborVillageTileset_16px/";

//     private void OnPreprocessTexture()
//     {
//         if (!assetPath.StartsWith(PackageRoot) || !assetPath.EndsWith(".png"))
//         {
//             return;
//         }

//         var importer = (TextureImporter)assetImporter;
//         importer.textureType = TextureImporterType.Sprite;
//         importer.spriteImportMode = SpriteImportMode.Single;
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
