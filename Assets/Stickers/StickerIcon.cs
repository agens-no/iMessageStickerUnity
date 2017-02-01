using System;
using UnityEngine;

namespace Agens.Stickers
{
    public class StickerIcon
    {
        public Vector2 size;
        public Idiom idiom;
        public string filename;
        public Scale scale;
        public string platform;

        public StickerIcon(Texture2D texture, int width, int height, Idiom idiom, Scale scale = Scale.Double, string platform = null)
        {
            size = new Vector2(width, height);
            filename = texture.name + ".png";
            this.idiom = idiom;
            this.scale = scale;
            this.platform = platform;
        }

        public enum Idiom
        {
            Iphone,
            Ipad,
            Universal,
            IosMarketing
        }

        public enum Scale
        {
            Original = 1,
            Double = 2,
            Triple = 3
        }

        public string GetIdiom()
        {
            switch (idiom)
            {
                case Idiom.IosMarketing:
                    return "ios-marketing";
                default:
                    return Enum.GetName(typeof(Idiom), idiom).ToLower();
            }
        }

        public string GetScale()
        {
            return (int) scale + "x";
        }
    }
}