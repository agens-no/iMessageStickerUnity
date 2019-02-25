using System;
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
        private const string MenuItemPath = "Window/Sticker Pack";

        public const string StickerAssetName = "StickerPack.asset";
        private const string StickerAssetPath = "Assets/Editor Default Resources/" + StickerAssetName;
        private const string ExportName = "Unity-iPhone-Stickers";
        private static readonly string ExportPath = Application.dataPath + "/../Temp/Stickers/";

        [MenuItem(MenuItemPath)]
        public static void Configurate()
        {
            var sticker = EditorGUIUtility.Load(StickerAssetName) as StickerPack;
            if (sticker == null)
            {
                Debug.Log("Could not find Sticker Pack at " + StickerAssetName);
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


            if (directories.Length == 0)
            {
                LogError("Could not find any directories in the directory '" + pathToBuiltProject + "'");
                return;
            }

            var pbxProjFile = directories.FirstOrDefault(file => Path.GetExtension(file) == ".pbxproj" || Path.GetExtension(file) == ".xcodeproj");
            if (pbxProjFile == null)
            {
                var files = Directory.GetFiles(pathToBuiltProject);
                if (files.Length == 0)
                {
                    LogError("Could not find any files in the directory '" + pathToBuiltProject + "'");
                    return;
                }
                
                pbxProjFile = files.FirstOrDefault(file => Path.GetExtension(file) == ".pbxproj" || Path.GetExtension(file) == ".xcodeproj");
                if (pbxProjFile == null)
                {
                    LogError("Could not find the Xcode project in the directory '" + pathToBuiltProject + "'");
                    return;
                }
            }

            var pack = EditorGUIUtility.Load(StickerAssetName) as StickerPack;
            AddSticker(pack);

#if UNITY_5_6_OR_NEWER
            var extensionBundleId = PlayerSettings.applicationIdentifier + "." + pack.BundleId;
#else
            var extensionBundleId = PlayerSettings.bundleIdentifier + "." + pack.BundleId;
#endif

            PBXProject.AddStickerExtensionToXcodeProject(
                ExportPath + ExportName + "/",
                pathToBuiltProject + "/",
                ExportName,
                extensionBundleId,
                PlayerSettings.iOS.appleDeveloperTeamID,
                PlayerSettings.bundleVersion,
                PlayerSettings.iOS.buildNumber,
                GetTargetDeviceFamily(PlayerSettings.iOS.targetDevice),
                pack.Signing.AutomaticSigning,
                pack.Signing.ProvisioningProfile,
                pack.Signing.ProvisioningProfileSpecifier
            );
            Log("Added sticker extension named " + pack.Title);
        }

        private static void AddSticker(StickerPack pack)
        {
            var path = ExportPath + ExportName;

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
            var contents = CreateStickerPackContent(pack.Size);
            contents.WriteToFile(pathToContent);
           
            var pathToStickersListContent=pathToStickers+"/Contents.json";
            var stickerListContent=CreateStickerListContent(pack);
            Log("Writing sticker list content to "+pathToStickersListContent);
            stickerListContent.WriteToFile(pathToStickersListContent);

            var plist = CreatePList(pack.Title, PlayerSettings.bundleVersion, PlayerSettings.iOS.buildNumber);
            plist.WriteToFile(path + "/Info.plist");

            foreach (var sticker in pack.Stickers)
            {
                if (sticker.Frames.Count == 1)
                {
                    ExportSticker(pathToStickers, sticker, pathToProject);
                }
                else
                {
                    ExportStickerSequence(pathToStickers, sticker, pathToProject);
                }
            }
        }

        private static void ExportSticker(string pathToStickers, Sticker sticker, string pathToProject)
        {
            if (sticker == null || sticker.Frames[0] == null) return;

            var stickerTexture = sticker.Frames[0];

            var pathToSticker = pathToStickers + "/" + sticker.Name + ".sticker";

            if (Directory.Exists(pathToSticker))
            {
                Directory.Delete(pathToSticker, true);
            }

            Directory.CreateDirectory(pathToSticker);
            var unityAssetPath = pathToProject + "/" + AssetDatabase.GetAssetPath(stickerTexture);

            var newFileName = sticker.Name;
            var fileExtension = Path.GetExtension(unityAssetPath);


            var json = CreateStickerContent(newFileName + fileExtension);
            Log("writing " + pathToSticker + "/Contents.json");
            json.WriteToFile(pathToSticker + "/Contents.json");
            
            var xcodeAssetPath = pathToSticker + "/" + newFileName + fileExtension;

            var count = 0;
            while (File.Exists(xcodeAssetPath))
            {
                xcodeAssetPath = pathToSticker + "/" + newFileName + count.ToString() + fileExtension;
            }
            File.Copy(unityAssetPath, xcodeAssetPath);
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
            CreateStickerIconElement(icon.MessagesAppStoreIcon, images.AddDict());

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
        
        public static JsonDocument CreateStickerPackContent(StickerSize size)
        {
            var content = CreateContent();
            var properties = content.root.CreateDict("properties");
            properties.SetString("grid-size", Enum.GetName(typeof(StickerSize), size).ToLowerInvariant());
            
            return content;
        }

        public static JsonDocument CreateStickerListContent(StickerPack pack) {
            
            var content = CreateContent();                      
            var stickerList = content.root.CreateArray("stickers");
            foreach (var sticker in pack.Stickers)
            {
                stickerList.AddDict().SetString("filename", sticker.name + ".sticker");                
            }
             // Add info
            var info = content.root.CreateDict("info");
            info.SetInteger("version", 1);
            info.SetString("author", "xcode");
             // Add properties
            var properties = content.root.CreateDict("properties");
            properties.SetString("grid-size", Enum.GetName(typeof(StickerSize), pack.Size).ToLowerInvariant());
            return content;
        }

        public static JsonDocument CreateContent()
        {
            var content = new JsonDocument();

            var info = content.root.CreateDict("info");
            info.SetInteger("version", 1);
            info.SetString("author", "xcode");
            return content;
        }

        public static JsonDocument CreateStickerContent(string filename)
        {
            var content = CreateContent();

            var properties = content.root.CreateDict("properties");
            properties.SetString("filename", filename);
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