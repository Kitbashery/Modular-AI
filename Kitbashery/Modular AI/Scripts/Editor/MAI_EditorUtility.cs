using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Kitbashery.AI
{
    /// <summary>
    /// Utility class for drawing commonly used editor GUI elements for modular AI's custom inspectors.
    /// </summary>
    public static class MAI_EditorUtility
    {
        public static GUIStyle centeredBoldHelpBox = new GUIStyle(EditorStyles.helpBox) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        public static GUIStyle wrappedMiniLabel = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 10 };
        public static GUIStyle miniLabel = new GUIStyle(GUI.skin.label) { clipping = TextClipping.Overflow, fontSize = 10 };
        public static GUIStyle centeredMiniLabel = new GUIStyle(GUI.skin.label) { clipping = TextClipping.Overflow, fontSize = 10, alignment = TextAnchor.MiddleCenter };
        public static GUIStyle upperLeftMiniLabel = new GUIStyle(GUI.skin.label) { clipping = TextClipping.Overflow, fontSize = 10, alignment = TextAnchor.UpperLeft };
        public static GUIStyle centeredLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        public static GUIStyle centeredBoldLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        public static GUIStyle middleLeftBoldLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        public static GUIStyle lowerLeftBoldLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerLeft, fontStyle = FontStyle.Bold };
        public static GUIStyle clippingBoldLabel = new GUIStyle(GUI.skin.label) { clipping = TextClipping.Overflow, fontStyle = FontStyle.Bold };
        public static GUIStyle rightAlignedLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
        public static GUIStyle richText = new GUIStyle(GUI.skin.label) { richText = true };
        public static GUILayoutOption[] horizontalLine = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) };
        public static GUILayoutOption[] thickHorizontalLine = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(3) };

        /// <summary>
        /// Draws a bold title with a help button that toggles a help box. 
        /// Useage example: myBool = DrawHelpTitleToggle(myBool, "title", "message");
        /// </summary>
        /// <param name="toggle">Boolean to pass in and return.</param>
        /// <param name="title">Text for the bold title.</param>
        /// <param name="text">Text for the help box to display.</param>
        /// <returns>toggle</returns>
        public static bool DrawHelpTitleToggle(bool toggle, string title, string text)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), GUIStyle.none, GUILayout.Width(20))) { toggle = !toggle; }
            EditorGUILayout.EndHorizontal();
            if (toggle == true)
            {
                EditorGUILayout.HelpBox(text, MessageType.Info);
            }
            GUILayout.Box("", thickHorizontalLine);

            return toggle;
        }

        public static int DrawCompactPopup(string label, int value, string[] options)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(75));
            value = EditorGUILayout.Popup(value, options);
            EditorGUILayout.EndHorizontal();
            return value;
        }
        public static bool DrawFoldout(bool value, string label)
        {
            bool _value;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _value = EditorGUILayout.Toggle(value, EditorStyles.foldout);

            EditorGUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();
            rect.x += 20;
            rect.width -= 20;

            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
            return _value;
        }

        public static void DrawComponentOptions(Component component)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Menu"), EditorStyles.helpBox, GUILayout.Width(24), GUILayout.Height(24)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Move Component Up"), false, MoveComponentUp, component);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Move Component Down"), false, MoveComponentDown, component);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Copy Component"), false, CopyComponentValues, component);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Paste Component Values"), false, PasteComponentValues, component);
                menu.ShowAsContext();
            }
        }

        static void MoveComponentUp(object component)
        {
            ComponentUtility.MoveComponentUp((Component)component);
        }

        static void MoveComponentDown(object component)
        {
            ComponentUtility.MoveComponentDown((Component)component);
        }

        static void CopyComponentValues(object component)
        {
            ComponentUtility.CopyComponent((Component)component);
        }

        static void PasteComponentValues(object component)
        {
            ComponentUtility.PasteComponentValues((Component)component);
        }
    }
}