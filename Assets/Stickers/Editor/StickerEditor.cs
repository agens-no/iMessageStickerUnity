using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Agens.Stickers
{
    [CustomEditor(typeof(Sticker))]
    public class StickerEditor : UnityEditor.Editor
    {
        private SerializedProperty Frames;
        private SerializedProperty Sequence;
        private SerializedProperty Name;
        private SerializedProperty Fps;
        private SerializedProperty Repetitions;

        private static GUIContent[] s_PlayIcons = new GUIContent[2];

        private List<UnityEditor.Editor> textureEditors;

        private MethodInfo RepaintMethod;
        private object GUIView;

        private UnityEditor.Editor currentTextureEditor
        {
            get
            {
                if (textureEditors == null)
                {
                    CreateTextureEditor();
                }

                if (!playing)
                {
                    return textureEditors[0];
                }

                var index = AnimatedIndex(Frames, Fps);

                if (index < textureEditors.Count)
                {
                    return textureEditors[index];
                }
                return null;
            }
        }

        public static int AnimatedIndex(SerializedProperty frames, SerializedProperty fps)
        {
            var length = frames.arraySize / (float) fps.intValue;
            var time = (float)Wrap(EditorApplication.timeSinceStartup, 0, length);
            var normalized = Mathf.InverseLerp(0, length, time);
            var frameIndex = Mathf.FloorToInt(normalized * frames.arraySize);
            return frameIndex;
        }

        private static double Wrap(double number, double min, double max)
        {
            return ((number - min) % (max - min)) + min;
        }

        private void OnEnable()
        {

            Frames = serializedObject.FindProperty("Frames");
            Sequence = serializedObject.FindProperty("Sequence");
            Name = serializedObject.FindProperty("Name");
            Fps = serializedObject.FindProperty("Fps");
            Repetitions = serializedObject.FindProperty("Repetitions");

            s_PlayIcons[0] = EditorGUIUtility.IconContent("preAudioPlayOff", "Play");
            s_PlayIcons[1] = EditorGUIUtility.IconContent("preAudioPlayOn", "Stop");
        }

        public static void AddStickerSequence(SerializedProperty sequence, SerializedProperty name, SerializedProperty fps, SerializedProperty frames)
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select Sticker Sequence", string.Empty, new string[] {"Image", "png,gif,jpg,jpeg" });
            var folder = Path.GetDirectoryName(path);
            //var folder = EditorUtility.OpenFolderPanel("Select Sticker Sequence", string.Empty, string.Empty);
            Debug.Log("path: " + path + " folder: " + folder);
            var files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(StickerEditorUtility.HasValidFileExtension).ToList();
            files.Sort();

            sequence.boolValue = true;
            var dir = new DirectoryInfo(folder);
            name.stringValue = dir.Name;
            fps.intValue = 15;

            frames.arraySize = files.Count;
            for (int index = 0; index < files.Count; index++)
            {
                var file = files[index];
                var projectPath = Application.dataPath;
                var filePath = file.Replace(projectPath, "Assets");
                Debug.Log("loaded texture at " + filePath);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                var prop = frames.GetArrayElementAtIndex(index);
                prop.objectReferenceValue = asset;
            }
        }

        private static bool playing;

        public override void OnPreviewSettings()
        {
#if UNITY_5_4_OR_NEWER
            using (new EditorGUI.DisabledScope(!Sequence.boolValue))
#else
            EditorGUI.BeginDisabledGroup(!Sequence.boolValue);
#endif
            {
                playing = CycleButton(!playing ? 0 : 1, s_PlayIcons, "preButton") != 0;
            }
#if !UNITY_5_4_OR_NEWER
            EditorGUI.EndDisabledGroup();
#endif
            if (textureEditors == null)
            {
                CreateTextureEditor();
            }

            if (currentTextureEditor != null)
            {
#if UNITY_5_4_OR_NEWER
                using (new EditorGUI.DisabledScope(playing))
#else
                EditorGUI.BeginDisabledGroup(playing);
#endif
                {
                    currentTextureEditor.OnPreviewSettings();
                }
#if !UNITY_5_4_OR_NEWER
                EditorGUI.EndDisabledGroup();
#endif
            }
        }

        static int CycleButton(int selected, GUIContent[] options, GUIStyle style)
        {
            if (GUILayout.Button(options[selected], style))
            {
                ++selected;
                if (selected >= options.Length)
                    selected = 0;
            }
            return selected;
        }

        public override bool HasPreviewGUI()
        {
            var sticker = serializedObject;
            var frames = sticker.FindProperty("Frames");
            return frames.arraySize > 0;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (textureEditors == null || textureEditors.Count == 0)
            {
                CreateTextureEditor();
            }

            if (currentTextureEditor != null)
            {
                currentTextureEditor.OnInteractivePreviewGUI(r, background);
            }

            if (playing && Sequence.boolValue && Frames.arraySize > 1)
            {
                if (RepaintMethod == null)
                {
                    var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
                    var prop = type.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
                    GUIView = prop.GetValue(null, null);
                    RepaintMethod = GUIView.GetType().GetMethod("Repaint", BindingFlags.Public | BindingFlags.Instance);
                }

                RepaintMethod.Invoke(GUIView, null);
            }
        }

        public override string GetInfoString()
        {

            if (textureEditors == null || textureEditors.Count == 0)
            {
                CreateTextureEditor();
            }

            if (currentTextureEditor != null)
            {
                return currentTextureEditor.GetInfoString();
            }

            return string.Empty;
        }

        private void CreateTextureEditor()
        {
            var sticker = serializedObject;
            var frames = sticker.FindProperty("Frames");
            textureEditors = new List<UnityEditor.Editor>(frames.arraySize);
            for (int i = 0; i < frames.arraySize; i++)
            {
                var firstFrame = frames.GetArrayElementAtIndex(i);
                var texture = firstFrame.objectReferenceValue as Texture2D;
                if (texture != null)
                {
                    textureEditors.Add(CreateEditor(texture));
                }
                else
                {
                    textureEditors.Add(null);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(Name);
            EditorGUI.EndDisabledGroup();

            var rect = GUILayoutUtility.GetRect(new GUIContent(Sequence.displayName, Sequence.tooltip), GUIStyle.none, GUILayout.Height(20));

            var sequenceRect = new Rect(rect);
            sequenceRect.width = EditorGUIUtility.labelWidth + 20f;
            EditorGUI.PropertyField(sequenceRect, Sequence);
#if UNITY_5_4_OR_NEWER
            using (new EditorGUI.DisabledScope(playing))
#else
            EditorGUI.BeginDisabledGroup(playing);
#endif
            {
                rect.xMin = sequenceRect.xMax;
                if (GUI.Button(rect, "Load from Folder"))
                {
                    AddStickerSequence(Sequence, Name, Fps, Frames);
                }

                EditorGUILayout.PropertyField(Fps);
                EditorGUILayout.PropertyField(Repetitions);
            }
#if !UNITY_5_4_OR_NEWER
            EditorGUI.EndDisabledGroup();
#endif

            if (Frames.arraySize == 0)
            {
                Frames.InsertArrayElementAtIndex(0);
            }

            if (!Sequence.boolValue && Frames.arraySize > 1)
            {
                Frames.arraySize = 1;
            }

            EditorGUI.BeginChangeCheck();
            DrawFrame();
            if (EditorGUI.EndChangeCheck())
            {
                CreateTextureEditor();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFrame()
        {
            if (Sequence.boolValue)
            {
                EditorGUILayout.PropertyField(Frames, true);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(Frames.GetArrayElementAtIndex(0), new GUIContent("Frame"));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (Sequence.boolValue != true)
                    {
                        var propertyPath = AssetDatabase.GetAssetPath(Frames.GetArrayElementAtIndex(0).objectReferenceInstanceIDValue);
                        if (StickerEditorUtility.IsAnimatedTexture(propertyPath))
                        {
                            Sequence.boolValue = true;
                        }
                    }
                }
            }
        }
    }
}