using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Agens.Stickers
{
    [CustomEditor(typeof(StickerPack))]
    public class StickerPackEditor : Editor
    {

        public static readonly int[] ValidSizes = new int[3] {300, 408, 618};
        public static readonly int[] StickerPerRow = new int[3] {4, 3, 2};

        private const float FieldHeight = 18;
        private const float ButtonHeight = 22;
        private const float ButtonPadding = ButtonHeight + 4;
        private const float FieldPadding = FieldHeight + 2;
        private const float ImageSize = 122;
        private const float ImagePadding = 4;

        private MethodInfo repaintMethod;
        private object guiView;
        private GUIStyle boldLabelStyle;
        private GUIStyle labelStyle;
        private GUIStyle elementStyle;
        private GUIStyle settingsStyle;

        private SerializedProperty title;
        private SerializedProperty bundleId;
        //private SerializedProperty bundleVersion;
        //private SerializedProperty buildNumber;

        private SerializedProperty signing;
        private SerializedProperty automaticSigning;
        private SerializedProperty provisioningProfile;
        private SerializedProperty provisioningProfileSpecifier;


        private SerializedProperty backgroundColor;
        private SerializedProperty fillPercentage;
        private SerializedProperty filterMode;
        private SerializedProperty scaleMode;
        private SerializedProperty overrideIcon;

        private SerializedProperty[] iconProperties;
        private Texture2D[] iconTextures;
        private Vector2[] iconTextureSizes;
        private GUIContent[] iconTextureLabels;

        private SerializedProperty stickers;
        private ReorderableList list;

        private Vector2 previewScroll;
        private static readonly GUIContent PixelSize = new GUIContent("Pixel Size");
        private static readonly GUIContent SizeOnDisk = new GUIContent("Size on disk");
        private static readonly GUIContent AppStoreIcon = new GUIContent("App Store Icon");
        private static readonly GUIContent AppStoreIconSize = new GUIContent("1024 x 1024");
        private static readonly GUIContent LoadFromFolder = new GUIContent("Load from Folder");

        private readonly Dictionary<Sticker, long> diskSizes = new Dictionary<Sticker, long>();
        private int SelectedSection { get { return EditorPrefs.GetInt("StickerSettings.ShownSection", -1); } set{ EditorPrefs.SetInt("StickerSettings.ShownSection", value); }}
        private readonly AnimBool[] sectionAnimators = new AnimBool[2];
        private static bool textureChanged;

        private void OnEnable()
        {
            title = serializedObject.FindProperty("title");
            bundleId = serializedObject.FindProperty("bundleId");
            //bundleVersion = serializedObject.FindProperty("bundleVersion");
            //buildNumber = serializedObject.FindProperty("buildNumber");

            signing = serializedObject.FindProperty("Signing");
            automaticSigning = signing.FindPropertyRelative("AutomaticSigning");
            automaticSigningAnimated = new AnimBool(Repaint);
            provisioningProfile = signing.FindPropertyRelative("ProvisioningProfile");
            provisioningProfileSpecifier = signing.FindPropertyRelative("ProvisioningProfileSpecifier");

            var icons = serializedObject.FindProperty("Icons");
            var settings = icons.FindPropertyRelative("Settings");
            backgroundColor = settings.FindPropertyRelative("BackgroundColor");
            fillPercentage = settings.FindPropertyRelative("FillPercentage");
            filterMode = settings.FindPropertyRelative("FilterMode");
            scaleMode = settings.FindPropertyRelative("ScaleMode");

            overrideIcon = icons.FindPropertyRelative("Override");

            iconProperties = new[]
            {
                icons.FindPropertyRelative("appStore"),
                icons.FindPropertyRelative("messagesiPadPro2"),
                icons.FindPropertyRelative("messagesiPad2"),
                icons.FindPropertyRelative("messagesiPhone2"),
                icons.FindPropertyRelative("messagesiPhone3"),
                icons.FindPropertyRelative("messagesSmall2"),
                icons.FindPropertyRelative("messagesSmall3"),
                icons.FindPropertyRelative("messages2"),
                icons.FindPropertyRelative("messages3"),
                icons.FindPropertyRelative("iPhoneSettings2"),
                icons.FindPropertyRelative("iPhoneSettings3"),
                icons.FindPropertyRelative("iPadSettings2")
            };

            iconTextureLabels = new[]
            {
                new GUIContent("AppStore"),
                new GUIContent("Messages iPad Pro @2x"),
                new GUIContent("Messages iPad @2x"),
                new GUIContent("Messages iPhone @2x"),
                new GUIContent("Messages iPhone @3x"),
                new GUIContent("Messages Small @2x"),
                new GUIContent("Messages Small @3x"),
                new GUIContent("Messages @2x"),
                new GUIContent("Messages @3x"),
                new GUIContent("iPhone Settings @2x"),
                new GUIContent("iPhone Settings @3x"),
                new GUIContent("iPad Settings @2x")
            };

            stickers = serializedObject.FindProperty("Stickers");
            list = new ReorderableList(serializedObject, stickers, true, true, true, true);
            list.drawElementBackgroundCallback += DrawElementBackgroundCallback;
            list.showDefaultBackground = false;
            list.elementHeightCallback += ElementHeight;
            list.drawHeaderCallback += DrawHeaderCallback;
            list.drawElementCallback += DrawStickerElement;
            list.onAddDropdownCallback += OnAddDropdownCallback;
            list.onRemoveCallback += OnRemoveCallback;
            list.onReorderCallback += OnReorderCallback;
            for (int index = 0; index < sectionAnimators.Length; ++index)
            {
                sectionAnimators[index] = new AnimBool(SelectedSection == index, Repaint);
            }
        }

        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList orderList)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Image"), false, CreateSticker);
            menu.AddItem(new GUIContent("Add Sequence"), false, CreateStickerSequence);
            menu.AddItem(new GUIContent("Import from Folder"), false, ImportFromFolder);
            menu.ShowAsContext();
        }

        private void CreateStickerSequence()
        {
            OnAddCallback(true);
        }

        private void CreateSticker()
        {
            OnAddCallback(false);
        }

        private void DrawElementBackgroundCallback(Rect rect, int index, bool selected, bool isFocused)
        {
            rect.height = ElementHeight(index);

            GUI.Box(Rect.MinMaxRect(rect.xMin + 1f, rect.yMin, rect.xMax - 3f, rect.yMax), string.Empty);

            if (Event.current.type != EventType.Repaint)
                return;

            if (elementStyle == null)
            {
                elementStyle = GetStyle("RL Element");
            }

            elementStyle.Draw(rect, false, selected, selected, isFocused);
        }

        private void OnRemoveCallback(ReorderableList orderList)
        {
            orderList.serializedProperty.DeleteArrayElementAtIndex(orderList.index);
            UpdatedIndexes(orderList);
        }

        private void OnReorderCallback(ReorderableList orderList)
        {
            UpdatedIndexes(orderList);
        }

        private void UpdatedIndexes(ReorderableList orderList)
        {
            Debug.Log("Saving sticker list");
            diskSizes.Clear();
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
            var stickersToAdd = new List<Sticker>();
            var index = 0;

            for (int i = 0; i < orderList.serializedProperty.arraySize; i++)
            {
                var sticker = orderList.serializedProperty.GetArrayElementAtIndex(i);

                var stickerValue = sticker.objectReferenceValue as Sticker;
                if (stickerValue != null)
                {
                    var stickerToAdd = CreateInstance<Sticker>();
                    stickerToAdd.CopyFrom(stickerValue, index++);
                    stickersToAdd.Add(stickerToAdd);
                }
            }

            foreach (var asset in assets)
            {
                if (asset is Sticker)
                {
                    DestroyImmediate(asset, true);
                }
            }

            orderList.serializedProperty.arraySize = 0;

            foreach (var stickerToAdd in stickersToAdd)
            {
                AddObjectToAsset(orderList, stickerToAdd);
            }
        }

        private Sticker OnAddCallback(bool sequence)
        {
            Debug.Log("Adding asset to " + target, target);
            var stickerToAdd = CreateInstance<Sticker>();
            stickerToAdd.Index = stickers.arraySize;
            stickerToAdd.name = "Sticker #" + stickers.arraySize;
            stickerToAdd.Name = "Sticker #" + stickers.arraySize;
            stickerToAdd.Sequence = sequence;
            stickerToAdd.Frames = new List<Texture2D>(1);
            stickerToAdd.Frames.Add(null);
            AddObjectToAsset(list, stickerToAdd);
            return stickerToAdd;
        }

        private void ImportFromFolder()
        {
            var folder = EditorUtility.OpenFolderPanel("Import Stickers", EditorPrefs.GetString("Stickers.ImportFolder"), string.Empty);
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                EditorPrefs.SetString("Stickers.ImportFolder", folder);

                var files = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var sticker = OnAddCallback(false);
                    var texture = LoadAssetFromPath<Texture2D>(file);
                    if (texture != null)
                    {
                        sticker.Frames[0] = texture;
                        sticker.name = texture.name;
                        sticker.Name = texture.name;
                    }
                }

                var directories = Directory.GetDirectories(folder);
                foreach (var directory in directories)
                {
                    if (string.IsNullOrEmpty(directory))
                    {
                        continue;
                    }

                    var sticker = OnAddCallback(true);
                    sticker.Frames = new List<Texture2D>(1);

                    var dirInfo = new DirectoryInfo(directory);

                    sticker.name = dirInfo.Name;
                    sticker.Name = dirInfo.Name;

                    files = Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly);
                    for (var index = 0; index < files.Length; index++)
                    {
                        var file = files[index];
                        var texture = LoadAssetFromPath<Texture2D>(file);
                        if (texture != null)
                        {
                            sticker.Frames.Add(texture);
                        }
                    }
                }
            }
        }

        private void AddObjectToAsset(ReorderableList orderList, Sticker stickerToAdd)
        {
            AssetDatabase.AddObjectToAsset(stickerToAdd, target);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stickerToAdd));

            var index = orderList.serializedProperty.arraySize;
            orderList.serializedProperty.arraySize++;
            orderList.index = index;

            var prop = stickers.GetArrayElementAtIndex(index);
            prop.objectReferenceValue = stickerToAdd;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.DropShadowLabel(rect, "Image Assets");
        }

        private float ElementHeight(int index)
        {
            return ImageSize + ImagePadding + ImagePadding;
        }

        private bool BeginSettingsBox(int nr, GUIContent header)
        {
            var enabled = GUI.enabled;
            GUI.enabled = true;
            EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
            var rect = GUILayoutUtility.GetRect(20f, 18f);
            rect.x += 3f;
            rect.width += 6f;

            EditorGUI.BeginChangeCheck();
            bool toggled = GUI.Toggle(rect, SelectedSection == nr, header, settingsStyle);
            if (EditorGUI.EndChangeCheck())
            {
                SelectedSection = !toggled ? -1 : nr;
                GUIUtility.keyboardControl = 0;
            }
            sectionAnimators[nr].target = toggled;
            GUI.enabled = enabled;
            return EditorGUILayout.BeginFadeGroup(sectionAnimators[nr].faded);
        }

        private static GUIStyle GetStyle(string styleName)
        {
            var guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
                guiStyle = GUIStyle.none;
            }
            return guiStyle;
        }

        private void EndSettingsBox()
        {
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndVertical();
        }

        public override bool HasPreviewGUI()
        {
            return list.count > 0;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (r.width < 60 || r.height < 60) return;

            var firstSticker = stickers.GetArrayElementAtIndex(0);
            var firstStickerObject = new SerializedObject(firstSticker.objectReferenceValue);

            var frames = firstStickerObject.FindProperty("Frames");

            var firstFrame = frames.GetArrayElementAtIndex(0);

            var firstFrameTexture = firstFrame.objectReferenceValue as Texture2D;

            var size = new Vector2(firstFrameTexture != null ? firstFrameTexture.width : 0, firstFrameTexture != null ? firstFrameTexture.height : 0);

            var sizeIndex = 0;
            for (var index = 0; index < ValidSizes.Length; index++)
            {
                var validSize = ValidSizes[index];
                if (validSize <= size.x)
                {
                    sizeIndex = index;
                }
            }

            var stickersToDrawPerRow = StickerPerRow[sizeIndex];

            var viewRect = new Rect(r);
            viewRect.width -= 18;
            var pixelWidth = viewRect.width / stickersToDrawPerRow;
            var x = viewRect.xMin;
            var y = viewRect.yMin;
            var stickerCounter = 0;
            viewRect.height = Mathf.CeilToInt(stickers.arraySize / 3f) * pixelWidth;
            previewScroll = GUI.BeginScrollView(r, previewScroll, viewRect, false, true);

            var sequence = false;
            for (int i = 0; i < stickers.arraySize; i++)
            {
                var sticker = stickers.GetArrayElementAtIndex(i);
                firstStickerObject = new SerializedObject(sticker.objectReferenceValue);
                frames = firstStickerObject.FindProperty("Frames");
                var fps = firstStickerObject.FindProperty("Fps");

                GUI.Box(new Rect(x, y, pixelWidth, pixelWidth), GUIContent.none);
                var firstFrameRect = new Rect(x + 2, y + 2, pixelWidth - 4, pixelWidth - 4);
                var frameIndex = StickerEditor.AnimatedIndex(frames, fps);

                var texture = firstFrameTexture;
                if (texture == null)
                {
                    continue;
                }
                if (frameIndex < frames.arraySize)
                {
                    texture = frames.GetArrayElementAtIndex(frameIndex).objectReferenceValue as Texture2D ?? firstFrameTexture;
                }

                if (firstStickerObject.FindProperty("Sequence").boolValue)
                {
                    sequence = true;
                }
                EditorGUI.DrawTextureTransparent(firstFrameRect, texture);
                x += pixelWidth;

                stickerCounter++;

                if (stickerCounter >= stickersToDrawPerRow)
                {
                    x = r.xMin;
                    y += pixelWidth;
                    stickerCounter = 0;
                }
            }
            GUI.EndScrollView();

            if (sequence)
            {
                Repaint();
            }
        }

        private void DrawStickerElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            rect.width -= ImageSize + ImagePadding;
            var stickerAsset = stickers.GetArrayElementAtIndex(index);

            var sticker = new SerializedObject(stickerAsset.objectReferenceValue);

            var frames = sticker.FindProperty("Frames");
            var stickerName = sticker.FindProperty("Name");
            var sequence = sticker.FindProperty("Sequence");
            var fps = sticker.FindProperty("Fps");
            var reps = sticker.FindProperty("Repetitions");

            Profiler.BeginSample("Sticker Element #" + index, stickerAsset.objectReferenceValue);
            {

                var nameRect = new Rect(rect);
                nameRect.y += 1;
                nameRect.height = FieldHeight;

                EditorGUI.PropertyField(nameRect, stickerName);

                var sequenceRect = new Rect(rect);
                sequenceRect.y += ButtonHeight + 2;
                sequenceRect.height = ButtonHeight;
                sequenceRect.width -= 150;
                EditorGUI.PropertyField(sequenceRect, sequence);

                EditorGUI.BeginDisabledGroup(!sequence.boolValue);
                var buttonRect = new Rect(rect);
                buttonRect.y += ButtonHeight;
                buttonRect.xMin = EditorGUIUtility.labelWidth + 50;
                if (GUI.Button(buttonRect, LoadFromFolder))
                {
                    StickerEditor.AddStickerSequence(sequence, stickerName, fps, frames);
                }

                var fieldRect = new Rect(rect);
                fieldRect.y += FieldPadding + ButtonPadding;
                fieldRect.height = FieldHeight;
                EditorGUI.PropertyField(fieldRect, fps);
                fieldRect.y += FieldPadding;
                EditorGUI.PropertyField(fieldRect, reps);
                EditorGUI.EndDisabledGroup();

                if (frames.arraySize == 0)
                {
                    frames.InsertArrayElementAtIndex(0);
                }

                if (!sequence.boolValue && frames.arraySize > 1)
                {
                    frames.arraySize = 1;
                }

                fieldRect.y += FieldPadding;

                var firstFrameRect = new Rect(rect);
                firstFrameRect.height = ImageSize;
                firstFrameRect.x = firstFrameRect.xMax + 4;
                firstFrameRect.y += 1f;
                firstFrameRect.width = ImageSize;

                var firstFrame = frames.GetArrayElementAtIndex(0);

                var firstFrameTexture = firstFrame.objectReferenceValue as Texture2D;

                var size = new Vector2(firstFrameTexture != null ? firstFrameTexture.width : 0, firstFrameTexture != null ? firstFrameTexture.height : 0);

                Profiler.BeginSample("Size Help Box");
                {
                    DrawSizeHelpBox(fieldRect, size);
                }
                Profiler.EndSample();
                fieldRect.y += FieldPadding;

                Profiler.BeginSample("Disk Size Help Box");
                {
                    DrawDiskSizeHelpBox(fieldRect, stickerAsset);
                }
                Profiler.EndSample();
                var updateIndexes = false;

                Profiler.BeginSample("Draw texture");
                {
                    if (DrawTexture(sequence, firstFrameRect, frames, fps, firstFrameTexture, firstFrame, stickerName, sticker, ref updateIndexes)) return;
                }
                Profiler.EndSample();

                if (sticker.ApplyModifiedProperties() && updateIndexes)
                {
                    UpdatedIndexes(list);
                }
            }
            Profiler.EndSample();

            if (sequence.boolValue && frames.arraySize > 1)
            {
                RepaintView();
            }
        }

        private static bool DrawTexture(SerializedProperty sequence, Rect firstFrameRect, SerializedProperty frames, SerializedProperty fps, Texture2D firstFrameTexture, SerializedProperty firstFrame, SerializedProperty stickerName, SerializedObject sticker, ref bool updateIndexes)
        {
            if (sequence.boolValue)
            {
                GUI.Box(firstFrameRect, GUIContent.none);
                var frameIndex = StickerEditor.AnimatedIndex(frames, fps);

                var texture = firstFrameTexture;
                if (texture == null)
                {
                    return true;
                }
                if (frameIndex < frames.arraySize)
                {
                    texture = frames.GetArrayElementAtIndex(frameIndex).objectReferenceValue as Texture2D ?? firstFrameTexture;
                }
                EditorGUI.DrawTextureTransparent(firstFrameRect, texture);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var texture = EditorGUI.ObjectField(firstFrameRect, firstFrame.objectReferenceValue as Texture2D, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck())
                {
					firstFrame.objectReferenceValue = texture;
					if (texture != null) {
						stickerName.stringValue = texture.name;
						sticker.targetObject.name = texture.name;
						updateIndexes = true;
					}
                }
            }
            return false;
        }

        private void DrawDiskSizeHelpBox(Rect fieldRect, SerializedProperty stickerAsset)
        {
            var id = GUIUtility.GetControlID(SizeOnDisk, FocusType.Passive);
            var helpRect2 = EditorGUI.PrefixLabel(fieldRect, id, SizeOnDisk);
            var sizeOnDisk = GetFileSize(stickerAsset.objectReferenceValue as Sticker);
            if (sizeOnDisk <= 500000)
            {
                EditorGUI.HelpBox(helpRect2, GetFileSizeInKB(sizeOnDisk), MessageType.None);
            }
            else
            {
                EditorGUI.HelpBox(helpRect2, GetFileSizeInKB(sizeOnDisk) + " is too large", MessageType.Warning);
            }
        }

        private static void DrawSizeHelpBox(Rect fieldRect, Vector2 size)
        {
            var id = GUIUtility.GetControlID(PixelSize, FocusType.Passive);
            var helpRect = EditorGUI.PrefixLabel(fieldRect, id, PixelSize);
            if (ValidSizes.Any(si => si == size.x))
            {
                EditorGUI.HelpBox(helpRect, size.x + "x" + size.y, MessageType.None);
            }
            else
            {
                EditorGUI.HelpBox(helpRect, size.x + "x" + size.y + " is not valid", MessageType.Warning);
            }
        }

        private void RepaintView()
        {
            if (repaintMethod == null)
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
                var prop = type.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
                guiView = prop.GetValue(null, null);
                repaintMethod = guiView.GetType().GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
            }
            repaintMethod.Invoke(guiView, null);
        }

        public static string GetFileSizeInKB(long size)
        {
            if (size < 1000)
            {
                return size + "B";
            }
            else
            {
                return (size / 1000) + " KB";
            }
        }

        private long GetFileSize(Sticker sticker)
        {
            if (diskSizes.ContainsKey(sticker))
            {
                return diskSizes[sticker];
            }

            var size = CalculateFileSize(sticker);
            diskSizes.Add(sticker, size);
            return size;
        }

        public static long CalculateFileSize(Sticker sticker)
        {
            long size = 0;
            for (int index = 0; index < sticker.Frames.Count; index++)
            {
                var stickerTexture = sticker.Frames[index];
                var postPath = AssetDatabase.GetAssetPath(stickerTexture);
                var projectPath = Application.dataPath;
                var filePath = projectPath.Replace("Assets", string.Empty) + postPath;
                var info = new FileInfo(filePath);
				try{
                	size += info.Length;
				} catch(FileNotFoundException e) {
					Debug.LogWarning ("Filepath: " + filePath + " is not valid. Please check sticker is not null or exists on disk.");
					Debug.LogWarning ("Catching: " + e);
				}
            }
            return size;
        }

        public override void OnInspectorGUI()
        {
            if (settingsStyle == null)
            {
                settingsStyle = GetStyle("IN TitleText");
            }

            if (boldLabelStyle == null)
            {
                boldLabelStyle = GetStyle("boldLabel");
            }

            if (labelStyle == null)
            {
                labelStyle = GetStyle("label");
            }

            serializedObject.Update();
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.PropertyField(title);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Bundle Id");
#if UNITY_5_6_OR_NEWER
            var content = new GUIContent(PlayerSettings.applicationIdentifier + ".");
#else
            var content = new GUIContent(PlayerSettings.bundleIdentifier + ".");
#endif
            EditorGUILayout.LabelField(content.text, labelStyle, GUILayout.Width(labelStyle.CalcSize(content).x));
            EditorGUILayout.PropertyField(bundleId, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Bundle Version", PlayerSettings.bundleVersion);
            EditorGUILayout.LabelField("Build Number", PlayerSettings.iOS.buildNumber);

            EditorGUILayout.PropertyField(automaticSigning);
            automaticSigningAnimated.target = !automaticSigning.boolValue;

            if(EditorGUILayout.BeginFadeGroup(automaticSigningAnimated.faded))
            {
                EditorGUI.BeginDisabledGroup(automaticSigning.boolValue);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(provisioningProfile);
                EditorGUILayout.PropertyField(provisioningProfileSpecifier);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndFadeGroup();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            if (BeginSettingsBox(0, new GUIContent("Icons")))
            {
                DrawIcons();
            }
            EndSettingsBox();

            if (BeginSettingsBox(1, new GUIContent("Stickers")))
            {
                list.DoLayoutList();
            }
            EndSettingsBox();
            serializedObject.ApplyModifiedProperties();
        }

        private AnimBool automaticSigningAnimated;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        private void CreateIconTextures()
        {
            if (iconTextures == null || iconTextureSizes == null || textureChanged)
            {
                iconTextures = (target as StickerPack).Icons.Textures;
                iconTextureSizes = (target as StickerPack).Icons.Sizes;
                textureChanged = false;
            }
        }

        private void DrawIcons()
        {
            CreateIconTextures();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(AppStoreIcon, boldLabelStyle);
            EditorGUILayout.LabelField(AppStoreIconSize);
            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            iconProperties[0].objectReferenceValue = (Texture2D) EditorGUILayout.ObjectField(iconProperties[0].objectReferenceValue, typeof (Texture2D), false, GUILayout.Height(75) , GUILayout.Width(75));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(overrideIcon.boolValue);
            EditorGUILayout.PropertyField(backgroundColor);
            EditorGUILayout.PropertyField(filterMode);
            EditorGUILayout.PropertyField(scaleMode);
            EditorGUI.BeginDisabledGroup(scaleMode.enumValueIndex != (int)ScaleMode.ScaleToFit);
            EditorGUILayout.PropertyField(fillPercentage);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            var rect = GUILayoutUtility.GetRect(100 + ImagePadding, 100 + ImagePadding, 75, 75, GUILayout.ExpandWidth(false));
            rect.xMin += ImagePadding;
            CreateIconTextures();
            if (iconTextures.Length == 0 || iconTextures[0] == null)
            {
                GUI.Box(rect, GUIContent.none);
            }
            else
            {
                EditorGUI.DrawTextureTransparent(rect, iconTextures[0]);
            }
            EditorGUILayout.EndVertical();

            DrawOverride();

            for (int i = 1; i < iconProperties.Length; i++)
            {
                DrawIcons(i);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawOverride()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(overrideIcon);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                textureChanged = true;
            }
            EditorGUI.BeginDisabledGroup(!overrideIcon.boolValue);
            if (GUILayout.Button(LoadFromFolder, GUILayout.Width(155)))
            {
                LoadIconsFromFolder();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private T LoadAssetFromPath<T>(string path) where T : Object
        {
            var projectPath = Application.dataPath;
            var filePath = path.Replace(projectPath, "Assets");
            Debug.Log("loaded texture at " + filePath);
            var asset = AssetDatabase.LoadAssetAtPath<T>(filePath);
            return asset;
        }

        private void LoadIconsFromFolder()
        {
            var folder = EditorUtility.OpenFolderPanel("Select Sticker Icon Folder", string.Empty, string.Empty);
            var files = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly).ToList();
            files.Sort();

            overrideIcon.boolValue = true;

            var texturesFound = new List<Texture2D>();

            foreach (var file in files)
            {
                texturesFound.Add(LoadAssetFromPath<Texture2D>(file));
            }

            for (var i = 0; i < iconProperties.Length; i++)
            {
                iconProperties[i].objectReferenceValue = texturesFound.Find(t => t.width == (int) iconTextureSizes[i].x && t.height == (int) iconTextureSizes[i].y);
            }
        }

        private void DrawIcons(int i)
        {

            var size = iconTextureSizes.Length > i ? iconTextureSizes[i] : Vector2.zero;
            var label = iconTextureLabels.Length > i ? iconTextureLabels[i] : GUIContent.none;
            var icon = iconProperties.Length > i ? iconProperties[i] : null;
            var texture = iconTextures.Length > i ? iconTextures[i] : null;
            var rect = GUILayoutUtility.GetRect(size.x, Mathf.Max(size.y + 4, 84), GUILayout.ExpandWidth(true));

            GUI.Box(rect, string.Empty);
            var text = size.x + "x" + size.y;
            var labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, 20f);
            GUI.Label(labelRect, label, boldLabelStyle);
            labelRect.y += 20f;
            GUI.Label(labelRect, text);

            rect.x = rect.width - size.x + 15;
            rect.y += 2;
            rect.height = size.y;
            rect.width = size.x;

            var textureSize = Vector2.zero;
            if (overrideIcon.boolValue)
            {
                icon.objectReferenceValue = (Texture2D) EditorGUI.ObjectField(rect, icon.objectReferenceValue, typeof (Texture2D), false);
                if (icon.objectReferenceValue != null)
                {
                    var iconTexture = (Texture2D) icon.objectReferenceValue;
                    textureSize = new Vector2(iconTexture.width, iconTexture.height);
                }
            }
            else if(texture != null)
            {
                //var obj = (Texture2D) EditorGUI.ObjectField(rect, texture, typeof (Texture2D), false);
                EditorGUI.DrawTextureTransparent(rect, texture);
                if (rect.Contains(Event.current.mousePosition) && Event.current.clickCount == 1)
                {
                    Selection.activeObject = texture;
                }
                textureSize = new Vector2(texture.width, texture.height);
            }
            else
            {
                GUI.Box(rect, GUIContent.none);
            }

            if (textureSize != size && textureSize != Vector2.zero)
            {
                labelRect.x += 2f;
                labelRect.y += 20f;
                labelRect.height = 40;
                EditorGUI.HelpBox(labelRect, "Size is " + textureSize.x + "," + textureSize.y, MessageType.Warning);
            }
            GUILayout.Space(4);
        }
    }
}