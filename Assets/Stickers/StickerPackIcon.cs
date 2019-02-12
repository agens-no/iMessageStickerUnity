using System;
using UnityEngine;

namespace Agens.Stickers
{
    [Serializable]
    public class StickerPackIcon
    {
        [Serializable]
        public class IconExportSettings
        {
            public Color BackgroundColor = Color.white;

            [Range(0f,100f)]
            public int FillPercentage = 100;
            public FilterMode FilterMode = FilterMode.Trilinear;
            public ScaleMode ScaleMode = ScaleMode.ScaleToFit;
        }

        public StickerIcon[] Icons
        {
            get
            {
                return new []
                {
                    AppStoreIcon,
                    MessagesAppStoreIcon,

                    MessagesIpadPro2Icon,
                    MessagesIpad2Icon,
                    MessagesiPhone2Icon,
                    MessagesiPhone3Icon,

                    MessagesSmall2Icon,
                    MessagesSmall3Icon,
                    Messages2Icon,
                    Messages3Icon,

                    IPhoneSettings2Icon,
                    IPhoneSettings3Icon,
                    IPadSettings2Icon
                };
            }
        }

        public Texture2D[] Textures
        {
            get
            {
                return new []
                {
                    AppStore,
                    MessagesAppStore,

                    MessagesIpadPro2,
                    MessagesIpad2,
                    MessagesiPhone2,
                    MessagesiPhone3,
                    MessagesSmall2,
                    MessagesSmall3,
                    Messages2,
                    Messages3,
                    IPhoneSettings2,
                    IPhoneSettings3,
                    IPadSettings2
                };
            }
        }

        public Vector2[] Sizes
        {
            get
            {
                return new []
                {
                    new Vector2(1024,1024),
                    new Vector2(1024,768),

                    new Vector2(148,110),
                    new Vector2(134,100),
                    new Vector2(120,90),
                    new Vector2(180,135),

                    new Vector2(54,40),
                    new Vector2(81,60),
                    new Vector2(64,48),
                    new Vector2(96,72),

                    new Vector2(58,58),
                    new Vector2(87,87),
                    new Vector2(58,58),
                };
            }
        }

        #region AppStore

        [Header("1024 x 1024 px")]
        [SerializeField]
        private Texture2D appStore;

        [Header("1024 x 768 px")]
        [SerializeField]
        private Texture2D messagesAppStore;

        /// <summary>
        /// 1024,768
        /// </summary>
        public Texture2D AppStore
        {
            get
            {
                if (Override)
                {
                    return appStore;
                }
                return GetDefaultTexture(1024, 1024);
            }
        }


        public Texture2D MessagesAppStore
        {
            get
            {
                if (Override)
                {
                    return messagesAppStore;
                }
                return GetDefaultTexture(1024, 768);
            }
        }

        public StickerIcon AppStoreIcon
        {
            get
            {
                var texture = AppStore;
                if (texture != null)
                {
                    return new StickerIcon(texture, 1024, 1024, StickerIcon.Idiom.IosMarketing, StickerIcon.Scale.Original);
                }
                return null;
            }
        }

        public StickerIcon MessagesAppStoreIcon
        {
            get
            {
                var texture = MessagesAppStore;
                if(texture != null)
                {
                    return new StickerIcon(texture, 1024, 768, StickerIcon.Idiom.IosMarketing, StickerIcon.Scale.Original, "ios");
                }
                return null;
            }
        }

        #endregion

        public IconExportSettings Settings;
        public bool Override;

        #region MessagesIpadPro2
        [Header("148 x 110 px")]
        [SerializeField]
        private Texture2D messagesiPadPro2;
        /// <summary>
        /// 148,110
        /// </summary>
        public Texture2D MessagesIpadPro2
        {
            get
            {
                if (Override)
                {
                    return messagesiPadPro2;
                }
                return GetDefaultTexture(148, 110);
            }
        }

        public StickerIcon MessagesIpadPro2Icon
        {
            get
            {
                var texture = MessagesIpadPro2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 74, 55, StickerIcon.Idiom.Ipad);
                }
                return null;
            }
        }
        #endregion

        #region MessagesIpad2
        [Header("134 x 100 px")]
        [SerializeField]
        private  Texture2D messagesiPad2;

        /// <summary>
        /// 134,100
        /// </summary>
        public Texture2D MessagesIpad2
        {
            get
            {
                if (Override)
                {
                    return messagesiPad2;
                }
                return GetDefaultTexture(134, 100);
            }
        }

        public StickerIcon MessagesIpad2Icon
        {
            get
            {
                var texture = MessagesIpad2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 67, 50, StickerIcon.Idiom.Ipad);
                }
                return null;
            }
        }
        #endregion

        #region MessagesiPhone2
        [Header("120 x 90 px")]
        [SerializeField]
        private  Texture2D messagesiPhone2;

        /// <summary>
        /// 120,90
        /// </summary>
        public Texture2D MessagesiPhone2
        {
            get
            {
                if (Override)
                {
                    return messagesiPhone2;
                }
                return GetDefaultTexture(120, 90);
            }
        }

        public StickerIcon MessagesiPhone2Icon
        {
            get
            {
                var texture = MessagesiPhone2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 60, 45, StickerIcon.Idiom.Iphone);
                }
                return null;
            }
        }
        #endregion

        #region MessagesiPhone3
        [Header("180 x 135 px")]
        [SerializeField]
        private Texture2D messagesiPhone3;

        /// <summary>
        /// 180,135
        /// </summary>
        public Texture2D MessagesiPhone3
        {
            get
            {
                if (Override)
                {
                    return messagesiPhone3;
                }
                return GetDefaultTexture(180, 135);
            }
        }

        /// <summary>
        /// 60,45
        /// </summary>
        public StickerIcon MessagesiPhone3Icon
        {
            get
            {
                var texture = MessagesiPhone3;
                if (texture != null)
                {
                    return new StickerIcon(texture, 60, 45, StickerIcon.Idiom.Iphone, StickerIcon.Scale.Triple);
                }
                return null;
            }
        }
        #endregion

        #region MessagesSmall2

        [Header("54 x 40 px")]
        [SerializeField]
        private  Texture2D messagesSmall2;

        /// <summary>
        /// 54,40
        /// </summary>
        public Texture2D MessagesSmall2
        {
            get
            {
                if (Override)
                {
                    return messagesSmall2;
                }
                return GetDefaultTexture(54, 40);
            }
        }

        public StickerIcon MessagesSmall2Icon
        {
            get
            {
                var texture = MessagesSmall2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 27, 20, StickerIcon.Idiom.Universal, StickerIcon.Scale.Double, "ios");
                }
                return null;
            }
        }
        #endregion

        #region MessagesSmall3

        [Header("81 x 60 px")]
        [SerializeField]
        private  Texture2D messagesSmall3;

        /// <summary>
        /// 81,60
        /// </summary>
        public Texture2D MessagesSmall3
        {
            get
            {
                if (Override)
                {
                    return messagesSmall3;
                }
                return GetDefaultTexture(81, 60);
            }
        }

        public StickerIcon MessagesSmall3Icon
        {
            get
            {
                var texture = MessagesSmall3;
                if (texture != null)
                {
                    return new StickerIcon(texture, 27, 20, StickerIcon.Idiom.Universal, StickerIcon.Scale.Triple, "ios");
                }
                return null;
            }
        }
        #endregion

        #region Messages2

        [Header("64 x 48 px")]
        [SerializeField]
        private  Texture2D messages2;

        /// <summary>
        /// 64,48
        /// </summary>
        public Texture2D Messages2
        {
            get
            {
                if (Override)
                {
                    return messages2;
                }
                return GetDefaultTexture(64, 48);
            }
        }

        public StickerIcon Messages2Icon
        {
            get
            {
                var texture = Messages2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 32, 24, StickerIcon.Idiom.Universal, StickerIcon.Scale.Double, "ios");
                }
                return null;
            }
        }

        #endregion

        #region Messages3

        [Header("96 x 72 px")]
        [SerializeField]
        private  Texture2D messages3;

        /// <summary>
        /// 96,72
        /// </summary>
        public Texture2D Messages3
        {
            get
            {
                if (Override)
                {
                    return messages3;
                }
                return GetDefaultTexture(96, 72);
            }
        }

        public StickerIcon Messages3Icon
        {
            get
            {
                var texture = Messages3;
                if (texture != null)
                {
                    return new StickerIcon(texture, 32, 24, StickerIcon.Idiom.Universal, StickerIcon.Scale.Triple, "ios");
                }
                return null;
            }
        }

        #endregion

        #region IPhoneSettings2
        [Header("58 x 58 px")]
        [SerializeField]
        private  Texture2D iPhoneSettings2;
        /// <summary>
        /// 58,58
        /// </summary>
        public Texture2D IPhoneSettings2
        {
            get
            {
                if (Override)
                {
                    return iPhoneSettings2;
                }
                return GetDefaultTexture(58, 58);
            }
        }

        public StickerIcon IPhoneSettings2Icon
        {
            get
            {
                var texture = IPhoneSettings2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 29, 29, StickerIcon.Idiom.Iphone);
                }
                return null;
            }
        }
        #endregion

        #region IPhoneSettings3

        [Header("87 x 87 px")]
        [SerializeField]
        private  Texture2D iPhoneSettings3;

        /// <summary>
        /// 87,87
        /// </summary>
        public Texture2D IPhoneSettings3
        {
            get
            {
                if (Override)
                {
                    return iPhoneSettings3;
                }
                return GetDefaultTexture(87, 87);
            }
        }

        public StickerIcon IPhoneSettings3Icon
        {
            get
            {
                var texture = IPhoneSettings3;
                if (texture != null)
                {
                    return new StickerIcon(texture, 29, 29, StickerIcon.Idiom.Iphone, StickerIcon.Scale.Triple);
                }
                return null;
            }
        }
        #endregion

        #region IPadSettings2

        [Header("58 x 58 px")]
        [SerializeField]
        private  Texture2D iPadSettings2;

        /// <summary>
        /// 58,58
        /// </summary>
        public Texture2D IPadSettings2
        {
            get
            {
                if (Override)
                {
                    return iPadSettings2;
                }
                return GetDefaultTexture(58, 58);
            }
        }

        public StickerIcon IPadSettings2Icon
        {
            get
            {
                var texture = IPadSettings2;
                if (texture != null)
                {
                    return new StickerIcon(texture, 29, 29, StickerIcon.Idiom.Ipad);
                }
                return null;
            }
        }
        #endregion

        public Texture2D GetDefaultTexture(int width, int height)
        {
            if (appStore == null)
            {
                return null;
            }

            var scaled = TextureScale.ScaledResized(appStore, width, height, Settings.BackgroundColor, Settings.FillPercentage / 100f, Settings.FilterMode, Settings.ScaleMode);
            if (scaled != null)
            {
                scaled.name = width + "x" + height;
            }
            return scaled;
        }
    }
}