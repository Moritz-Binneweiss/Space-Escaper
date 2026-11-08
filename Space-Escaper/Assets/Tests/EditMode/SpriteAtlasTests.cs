using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace SpaceEscaper.Tests
{
    public class SpriteAtlasTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiFolder = "Assets/UI/";

        // On Android a UI image outside an atlas stays uncompressed: ETC2 only packs sizes that are
        // multiples of 4, and most of the images are not. Each one is also a draw call of its own.
        [Test]
        public void EveryUiImageIsInAnAtlas()
        {
            Assert.That(EditorSettings.spritePackerMode, Is.EqualTo(SpritePackerMode.SpriteAtlasV2),
                "Sprite packing is not on Sprite Atlas V2, so the atlases are not packed in the Editor");

            HashSet<string> packedImages = GetPackedImages();
            List<string> loneImages = new List<string>();
            foreach (string image in GetUiImagesOfTheGame())
            {
                if (!packedImages.Contains(image))
                {
                    loneImages.Add(image);
                }
            }

            Assert.That(loneImages, Is.Empty);
        }

        // The game has no late binding, so an atlas left out of the build leaves its images blank.
        [Test]
        public void EveryAtlasComesWithTheBuild()
        {
            List<string> leftOutAtlases = new List<string>();
            foreach (string atlasPath in GetAtlasPaths())
            {
                if (!GetImporter(atlasPath).includeInBuild)
                {
                    leftOutAtlases.Add(atlasPath);
                }
            }

            Assert.That(leftOutAtlases, Is.Empty);
        }

        // A UI image shows the rectangle of its sprite in the atlas. A rotated sprite would show
        // turned, and with tight packing other images reach into the rectangle. A new atlas has both on.
        [Test]
        public void NoAtlasRotatesOrPacksTightly()
        {
            List<string> problems = new List<string>();
            foreach (string atlasPath in GetAtlasPaths())
            {
                SpriteAtlasPackingSettings packing = GetImporter(atlasPath).packingSettings;
                if (packing.enableRotation)
                {
                    problems.Add(atlasPath + " rotates");
                }

                if (packing.enableTightPacking)
                {
                    problems.Add(atlasPath + " packs tightly");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // As soon as one of its images shows, the whole atlas is in memory. An image the game no
        // longer uses only makes it bigger.
        [Test]
        public void AtlasesHoldOnlyImagesTheGameUses()
        {
            HashSet<string> usedImages = new HashSet<string>(GetUiImagesOfTheGame());
            List<string> unusedImages = new List<string>();
            foreach (string image in GetPackedImages())
            {
                if (!usedImages.Contains(image))
                {
                    unusedImages.Add(image);
                }
            }

            Assert.That(unusedImages, Is.Empty);
        }

        private static List<string> GetUiImagesOfTheGame()
        {
            List<string> images = new List<string>();
            foreach (string path in AssetDatabase.GetDependencies(k_ScenePath, true))
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (path.StartsWith(k_UiFolder) && importer != null
                    && importer.textureType == TextureImporterType.Sprite)
                {
                    images.Add(path);
                }
            }

            Assert.That(images, Is.Not.Empty, "No UI images in " + k_ScenePath);
            return images;
        }

        // An atlas takes images one by one or as a whole folder.
        private static HashSet<string> GetPackedImages()
        {
            HashSet<string> images = new HashSet<string>();
            foreach (string atlasPath in GetAtlasPaths())
            {
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                foreach (Object packable in atlas.GetPackables())
                {
                    string path = AssetDatabase.GetAssetPath(packable);
                    if (packable == null)
                    {
                        images.Add(atlasPath + ": missing image");
                    }
                    else if (AssetDatabase.IsValidFolder(path))
                    {
                        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { path }))
                        {
                            images.Add(AssetDatabase.GUIDToAssetPath(guid));
                        }
                    }
                    else
                    {
                        images.Add(path);
                    }
                }
            }

            return images;
        }

        private static List<string> GetAtlasPaths()
        {
            List<string> paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:SpriteAtlas", new[] { "Assets" }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }

        private static SpriteAtlasImporter GetImporter(string atlasPath)
        {
            SpriteAtlasImporter importer = AssetImporter.GetAtPath(atlasPath) as SpriteAtlasImporter;
            Assert.That(importer, Is.Not.Null, atlasPath + " is no Sprite Atlas V2");
            return importer;
        }
    }
}
