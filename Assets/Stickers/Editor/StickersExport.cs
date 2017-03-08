using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.iOS.Xcode.Stickers;
using Application = UnityEngine.Application;

namespace Agens.Stickers
{
    public static class StickersExport
    {
        private const string MenuItemPath = "Window/Stickers/";

        private const string StickerAssetName = "StickerPack";
        private const string StickerAssetPath = "Assets/Stickers/Resources/"+StickerAssetName+".asset";
        private static readonly string ExportPath = Application.dataPath + "/../Temp/Stickers/";

        [MenuItem(MenuItemPath + "Configurate    ")]
        public static void Configurate()
        {
            var sticker = Resources.Load<StickerPack>(StickerAssetName);
            if (sticker == null)
            {
                sticker = ScriptableObject.CreateInstance<StickerPack>();
                sticker.Title = PlayerSettings.productName + " Stickers";
                sticker.BundleId = "stickers";
                sticker.Signing = new SigningSettings();
                sticker.Signing.AutomaticSigning = PlayerSettings.iOS.appleEnableAutomaticSigning;
                sticker.Signing.ProvisioningProfile = PlayerSettings.iOS.iOSManualProvisioningProfileID;
                var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (StickerAssetPath);
                Log("Creating StickerPack asset at " + assetPathAndName);
                AssetDatabase.CreateAsset(sticker, assetPathAndName);
            }

            Selection.activeObject = sticker;
        }

        [MenuItem(MenuItemPath + "Add to Xcode")]
        public static void WriteToProject()
        {
            var pathToBuiltProject = EditorUtility.OpenFolderPanel("Select Xcode folder", "/../", "Build");

            WriteToProject(pathToBuiltProject);
        }

        private static void LogError(string error)
        {
            Debug.LogError("Sticker Plugin Error: " + error);
        }

        private static void Log(string message)
        {
            Debug.Log("Sticker Plugin: " + message);
        }

        public static void WriteToProject(string pathToBuiltProject)
        {
            var exists = Directory.Exists(pathToBuiltProject);
            if (!exists)
            {
                LogError("Could not find directory '" + pathToBuiltProject + "'");
                return;
            }

            var directories = Directory.GetDirectories(pathToBuiltProject);

            var files = Directory.GetFiles(pathToBuiltProject);
            if (files.Length == 0)
            {
                LogError("Could not find any files in the directory '" + pathToBuiltProject + "'");
                return;
            }

            if (directories.Length == 0)
            {
                LogError("Could not find any directories in the directory '" + pathToBuiltProject + "'");
                return;
            }

            var pbxProjFile = directories.FirstOrDefault(file => Path.GetExtension(file) == ".pbxproj" || Path.GetExtension(file) == ".xcodeproj");
            if (pbxProjFile == null)
            {
                pbxProjFile = files.FirstOrDefault(file => Path.GetExtension(file) == ".pbxproj" || Path.GetExtension(file) == ".xcodeproj");
                if (pbxProjFile == null)
                {
                    LogError("Could not find the Xcode project in the directory '" + pathToBuiltProject + "'");
                    return;
                }
            }

            var pack = Resources.Load<StickerPack>(StickerAssetName);
            AddSticker(pack);
            var name = pack.Title;

            PBXProject.AddStickerExtensionToXcodeProject(
                ExportPath + name + "/",
                pathToBuiltProject + "/",
                name,
                pack.BundleId,
                PlayerSettings.iOS.appleDeveloperTeamID,
                PlayerSettings.bundleVersion,
                PlayerSettings.iOS.buildNumber,
                GetTargetDeviceFamily(PlayerSettings.iOS.targetDevice),
                pack.Signing.AutomaticSigning ? null : pack.Signing.ProvisioningProfile,
                pack.Signing.AutomaticSigning ? null : pack.Signing.ProvisioningProfileSpecifier
            );
            Log("Added sticker extension named " + name);
        }

        private static void AddSticker(StickerPack pack)
        {
            var name = pack.Title;

            var path = ExportPath + name;

            ExportIcons(pack, path);

            ExportStickers(pack, path);
        }

        private static void ExportStickers(StickerPack pack, string path)
        {
            var pathToProject = Application.dataPath.Replace("/Assets", string.Empty);
            var pathToStickers = path + "/Stickers.xcassets/Sticker Pack.stickerpack";
            if (!Directory.Exists(pathToStickers))
            {
                Log("Creating " + pathToStickers);
                Directory.CreateDirectory(pathToStickers);
            }

            var pathToContent = path + "/Stickers.xcassets/Contents.json";
            var contents = CreateContent();
            contents.WriteToFile(pathToContent);

            var plist = CreatePList(pack.Title, PlayerSettings.bundleVersion, PlayerSettings.iOS.buildNumber);
            plist.WriteToFile(path + "/Info.plist");

            foreach (var sticker in pack.Stickers)
            {
                if (sticker.Frames.Count == 1)
                {
                    ExportSticker(pathToStickers, sticker.Frames[0], pathToProject);
                }
                else
                {
                    ExportStickerSequence(pathToStickers, sticker, pathToProject);
                }
            }
        }

        private static void ExportSticker(string pathToStickers, Texture2D sticker, string pathToProject)
        {
			if (sticker == null) return;

            var pathToSticker = pathToStickers + "/" + sticker.name + ".sticker";

            if (Directory.Exists(pathToSticker))
            {
                Directory.Delete(pathToSticker, true);
            }

            Directory.CreateDirectory(pathToSticker);

            var json = CreateStickerContent(sticker);
            Log("writing " + pathToSticker + "/Contents.json");
            json.WriteToFile(pathToSticker + "/Contents.json");
            var oldPath = pathToProject + "/" + AssetDatabase.GetAssetPath(sticker);
            var fileName = pathToSticker + "/" + sticker.name + ".png";

            var count = 0;
            while (File.Exists(fileName))
            {
                fileName = pathToSticker + "/" + sticker.name + count + ".png";
            }
            File.Copy(oldPath, fileName);
        }

        private static void ExportStickerSequence(string pathToStickers, Sticker stickerSequence, string pathToProject)
        {
            var pathToSticker = pathToStickers + "/" + stickerSequence.Name + ".stickersequence";
            if (Directory.Exists(pathToSticker))
            {
                Directory.Delete(pathToSticker, true);
            }

            Directory.CreateDirectory(pathToSticker);

            var json = CreateStickerSequenceContent(stickerSequence);
            Log("writing " + pathToSticker + "/Contents.json");
            json.WriteToFile(pathToSticker + "/Contents.json");

            foreach (var frame in stickerSequence.Frames)
            {
                var oldPath = pathToProject + "/" + AssetDatabase.GetAssetPath(frame);

                var fileName = pathToSticker + "/" + frame.name + ".png";
                File.Copy(oldPath, fileName);
            }
        }

        private static void ExportIcons(StickerPack pack, string path)
        {
            var pathToAppIcons = path + "/Stickers.xcassets/iMessage App Icon.stickersiconset";
            if (!Directory.Exists(pathToAppIcons))
            {
                Log("Creating " + pathToAppIcons);
                Directory.CreateDirectory(pathToAppIcons);
            }

            var iconContent = CreateIconContent(pack.Icons);
            iconContent.WriteToFile(pathToAppIcons + "/Contents.json");
            var icons = pack.Icons.Textures;
            foreach (var icon in icons)
            {
                var fileName = pathToAppIcons + "/" + icon.name + ".png";
                Log("Copying " + icon.name + " to " + fileName);
                File.WriteAllBytes(fileName, icon.EncodeToPNG());
            }
        }

        private static JsonDocument CreateIconContent(StickerPackIcon icon)
        {
            var document = CreateContent();
            var images = document.root.CreateArray("images");

            CreateStickerIconElement(icon.IPhoneSettings2Icon, images.AddDict());
            CreateStickerIconElement(icon.IPhoneSettings3Icon, images.AddDict());

            CreateStickerIconElement(icon.MessagesiPhone2Icon, images.AddDict());
            CreateStickerIconElement(icon.MessagesiPhone3Icon, images.AddDict());

            CreateStickerIconElement(icon.IPadSettings2Icon, images.AddDict());
            CreateStickerIconElement(icon.MessagesIpad2Icon, images.AddDict());
            CreateStickerIconElement(icon.MessagesIpadPro2Icon, images.AddDict());

            CreateStickerIconElement(icon.MessagesSmall2Icon, images.AddDict());
            CreateStickerIconElement(icon.MessagesSmall3Icon, images.AddDict());

            CreateStickerIconElement(icon.Messages2Icon, images.AddDict());
            CreateStickerIconElement(icon.Messages3Icon, images.AddDict());

            CreateStickerIconElement(icon.AppStoreIcon, images.AddDict());

            return document;
        }

        private static void CreateStickerIconElement(StickerIcon icon, JsonElementDict dict)
        {
            dict.SetString("size", string.Format("{0}x{1}", icon.size.x, icon.size.y));
            dict.SetString("idiom", icon.GetIdiom());
            dict.SetString("filename", icon.filename);
            dict.SetString("scale", icon.GetScale());
            if (!string.IsNullOrEmpty(icon.platform))
            {
                dict.SetString("platform", icon.platform);
            }

        }

        public static JsonDocument CreateContent()
        {
            var content = new JsonDocument();

            var info = content.root.CreateDict("info");
            info.SetInteger("version", 1);
            info.SetString("author", "xcode");
            return content;
        }

        public static JsonDocument CreateStickerContent(Texture2D file)
        {
            var content = CreateContent();

            var properties = content.root.CreateDict("properties");
            properties.SetString("filename", file.name + ".png");
            return content;
        }
        public static JsonDocument CreateStickerSequenceContent(Sticker stickerSequence)
        {
            var content = CreateContent();

            var properties = content.root.CreateDict("properties");
            properties.SetInteger("duration", stickerSequence.Fps);
            properties.SetString("duration-type", "fps");
            properties.SetInteger("repetitions", stickerSequence.Repetitions);

            var frames = content.root.CreateArray("frames");
            foreach (var file in stickerSequence.Frames)
            {
                frames.AddDict().SetString("filename", file.name + ".png");
            }
            return content;
        }

        private static string GetTargetDeviceFamily ( iOSTargetDevice targetDevice ) {
            if ( targetDevice == iOSTargetDevice.iPhoneOnly )
                return "1";
            if ( targetDevice == iOSTargetDevice.iPadOnly )
                return "2";
            return "1,2"; // universal
        }

        private static PlistDocument CreatePList(string name, string versionString = "1.0", string buildVersion = "1")
        {
            var list = new PlistDocument();
            var dict = list.root;
            dict.SetString("CFBundleDevelopmentRegion",
                "en");
            dict.SetString("CFBundleDisplayName",
                name);
            dict.SetString("CFBundleExecutable",
                "$(EXECUTABLE_NAME)");
            dict.SetString("CFBundleIdentifier",
                "$(PRODUCT_BUNDLE_IDENTIFIER)");
            dict.SetString("CFBundleInfoDictionaryVersion",
                "6.0");
            dict.SetString("CFBundleName",
                "$(PRODUCT_BUNDLE_IDENTIFIER)");
            dict.SetString("CFBundleName",
                "$(PRODUCT_NAME)");
            dict.SetString("CFBundlePackageType",
                "XPC!");
            dict.SetString("CFBundleShortVersionString",
                versionString);
            dict.SetString("CFBundleVersion", buildVersion);

            var extension = dict.CreateDict("NSExtension");
            extension.SetString("NSExtensionPointIdentifier",
                "com.apple.message-payload-provider");
            extension.SetString("NSExtensionPrincipalClass",
                "StickerBrowserViewController");
            return list;
        }
    }
}