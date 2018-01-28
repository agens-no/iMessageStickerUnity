using System.IO;
using Agens;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace Agens.Stickers
{
    public class StickerTest
    {
        [Test]
        public void Title()
        {
            var pack = EditorGUIUtility.Load(StickersExport.StickerAssetName) as StickerPack;
            Assert.IsNotNull(pack, "Sticker Pack object is missing");
#if UNITY_5_6_OR_NEWER
            Assert.IsNotNull(pack.Title, "Sticker Pack does not have a title");
#else
            Assert.IsNotNullOrEmpty(pack.Title, "Sticker Pack does not have a title");
#endif
        }

        [Test]
        public void Stickers()
        {
            var pack = EditorGUIUtility.Load(StickersExport.StickerAssetName) as StickerPack;

            for (int index = 0; index < pack.Stickers.Count; index++)
            {
                var sticker = pack.Stickers[index];
                AssertSequence(sticker, index);
            }
        }

        private static void AssertSequence(Sticker sticker, int i)
        {
            Assert.IsNotNull(sticker.Frames[0], "Sticker #" + i + " is null");
            var pixelSize = sticker.Frames[0].width;
            for (int index = 0; index < sticker.Frames.Count; index++)
            {
                var stickerTexture = sticker.Frames[index];
                Assert.IsNotNull(stickerTexture, "Sticker Asset #" + index + " in Sequence #" + i + " is null");
                Assert.Contains(stickerTexture.width, StickerPackEditor.ValidSizes, "Sticker " + stickerTexture.name + " in Sequence #" + i + " is not a valid size (" + stickerTexture.width + ")");
                Assert.AreEqual(pixelSize, stickerTexture.width, stickerTexture.height, "Sticker " + stickerTexture.name + " in Sequence #" + i + " is not the same size as the rest");
            }

            Assert.LessOrEqual(StickerPackEditor.CalculateFileSize(sticker), 500000, "Sticker " + sticker.Name + " is larger than the allowed 500KB");
        }
    }
}