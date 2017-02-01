using System.Collections.Generic;
using UnityEngine;

namespace Agens.Stickers
{
    [CreateAssetMenu(fileName = "StickerPack", menuName = "Sticker Pack")]
    public class StickerPack : ScriptableObject
    {

        [Tooltip("The Display Name of the Sticker Pack")]
        [SerializeField]
        private string title;

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(title))
                {
                    Debug.LogWarning("Title is missing from Sticker Pack", this);
                    return name;
                }
                return title;
            }
            set { title = value; }
        }

        [Tooltip("Bundle identifier postfix. This will come after the parents app bundle identifier.")]
        [SerializeField]
        private string bundleId;

        public string BundleId
        {
            get
            {
                if (string.IsNullOrEmpty(bundleId))
                {
                    Debug.LogWarning("Bundle Id is missing from Sticker Pack", this);
                    return "stickers";
                }
                return bundleId;
            }

            set { bundleId = value; }
        }

        [Tooltip("Bundle version")]
        [SerializeField]
        private string bundleVersion;

        public string BundleVersion
        {
            get
            {
                if (string.IsNullOrEmpty(bundleVersion))
                {
                    Debug.LogWarning("Bundle Version is missing from Sticker Pack", this);
                    return "1.0.0";
                }
                return bundleVersion;
            }

            set { bundleVersion = value; }
        }


        [Tooltip("Build version")]
        [SerializeField]
        private string buildNumber;

        public string BuildNumber
        {
            get
            {
                if (string.IsNullOrEmpty(buildNumber))
                {
                    Debug.LogWarning("Build Version is missing from Sticker Pack", this);
                    return "1";
                }
                return buildNumber;
            }

            set { buildNumber = value; }
        }

        public SigningSettings Signing;


        public StickerPackIcon Icons;
        public List<Sticker> Stickers;
    }
}