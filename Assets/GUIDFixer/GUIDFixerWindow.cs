using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Kamgam.GF
{
    public enum Command { Search, Replace }

    public class GUIDFixerWindow : EditorWindow
    {

        public static GUIStyle ErrorGroupDetailsButtonBoxStyle;
        public static Color WarningTextColor = new Color(0.9f, 0.2f, 0.2f);

        private static Color _DefaultBackgroundColor;
        public static Color DefaultBackgroundColor
        {
            get
            {
                if (_DefaultBackgroundColor.a == 0)
                {
                    try
                    {
                        var method = typeof(EditorGUIUtility).GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                        _DefaultBackgroundColor = (Color)method.Invoke(null, null);
                    }
                    catch
                    {
                        // fallback if reflection fails
                        _DefaultBackgroundColor = new Color32(56, 56, 56, 255);
                    }
                }
                return _DefaultBackgroundColor;
            }
        }

        // Input
        protected string guidToSearchFor;
        protected string guidToReplace;
        protected string guidReplacement;
        protected Command command;

        // State
        protected Vector2 resultsScrollPos = Vector2.zero;
        protected int isSearching;
        protected int isReplacing;
        protected List<string> searchedPaths;
        protected bool scanSelected;
        protected string assetPathOfGUIDToReplace;

        [MenuItem("Window/GUID Fixer")]
        static GUIDFixerWindow openWindow()
        {
            GUIDFixerWindow window = (GUIDFixerWindow)EditorWindow.GetWindow(typeof(GUIDFixerWindow));
            window.titleContent = new GUIContent("GUID Fixer");
            window.Initialize();
            window.Show();
            return window;
        }

        public static GUIDFixerWindow GetOrOpen()
        {
            if (!HasOpenInstances<GUIDFixerWindow>())
            {
                var window = openWindow();
                window.Focus();
                return window;
            }
            else
            {
                var window = GetWindow<GUIDFixerWindow>();
                window.Focus();
                return window;
            }
        }

        public void OnEnable()
        {
            Initialize();
            guidToSearchFor = "";
            scanSelected = false;
            isSearching = 0;
            isReplacing = 0;
        }

        public void Initialize()
        {
            ErrorGroupDetailsButtonBoxStyle = BackgroundStyle.Get(DefaultBackgroundColor);

            if (!isDocked())
            {
                if (position.width < 300 || position.height < 80)
                {
                    const int width = 450;
                    const int height = 100;
                    var x = Screen.currentResolution.width / 2 - width;
                    var y = Screen.currentResolution.height / 2 - height;
                    position = new Rect(x, y, width, height);
                }
            }
        }

        public void SetData(Command command, string guid)
        {
            this.command = command;

            switch (command)
            {
                case Command.Search:
                    if (guid != null)
                    {
                        guidToSearchFor = guid;
                    }
                    break;

                case Command.Replace:
                    if (guid != null)
                    {
                        guidReplacement = null;
                        guidToReplace = guid;
                        assetPathOfGUIDToReplace = AssetDatabase.GUIDToAssetPath(guidToReplace);
                    }
                    else
                    {
                        guidReplacement = null;
                        guidToReplace = "";
                        assetPathOfGUIDToReplace = null;
                    }
                    break;

                default:
                    break;
            }
        }

        protected bool isDocked()
        {
#if UNITY_2020_1_OR_NEWER
            return docked;
#else
            return true;
#endif
        }

        void OnGUI()
        {
            // Title bar with buttons
            var settings = GUIDFixerSettings.GetOrCreateSettings();

            GUILayout.BeginHorizontal();
            DrawLabel(commandToString(command), bold: true, wordwrap: false);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version " + GUIDFixerSettings.Version + " ");
            if (DrawButton(" Manual ", icon: "_Help"))
            {
                GUIDFixer.OpenManual();
            }
            if (DrawButton(" Settings ", icon: "_Popup"))
            {
                GUIDFixer.OpenSettings();
            }
            GUILayout.EndHorizontal();


            // Enter guid field & Settings
            if (command == Command.Search)
                DrawSearch();
            else
                DrawReplace();
        }


        string commandToString(Command command)
        {
            switch (command)
            {
                case Command.Search:
                    return "Search";

                case Command.Replace:
                    return "Replace";
            }

            return null;
        }

        #region Draw search
        private void DrawSearch()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.enabled = isSearching == 0;
            // GUID
            guidToSearchFor = EditorGUILayout.TextField(new GUIContent("GUID:", "Enter the GUID which you wish to scan for."), guidToSearchFor);
            // sacn only selected checkbox
            GUILayout.BeginHorizontal();
            scanSelected = EditorGUILayout.Toggle(new GUIContent("Scan selected:", "Should only the selected files and directories be scanned for the GUID?"), scanSelected);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            // Start scan button
            if (GUILayout.Button(isSearching == 1 ? "Searching .." : "Search"))
            {
                isSearching = 3;
                EditorApplication.update -= startSearchingForGUID;
                EditorApplication.update += startSearchingForGUID;
            }
            GUI.enabled = true;

            GUILayout.Space(5);

            // List of search results
            if (isSearching == 0 && searchedPaths != null && searchedPaths.Count > 0)
            {
                DrawLabel("Search Results (" + searchedPaths.Count + ")", bold: true);
                DrawSearchResults();
            }
            else
            {
                DrawLabel("Search Results", bold: true);
                DrawLabel("No results.");
            }
        }

        private void DrawSearchResults()
        {
            resultsScrollPos = GUILayout.BeginScrollView(resultsScrollPos);

            foreach (var path in searchedPaths)
            {
                GUILayout.BeginHorizontal();

                bool isMeta = path.EndsWith(".meta");
                string displayPath = path;
                displayPath = displayPath.Replace("/", " / ");
                if(isMeta)
                    displayPath = displayPath.Replace(".meta", "");
                Color textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
                displayPath = WrapInRichTextColor(displayPath, textColor);
                if (isMeta)
                {
                    textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
                    displayPath += WrapInRichTextColor("  (.meta)", textColor);
                }
                DrawLabel(displayPath);

                if (DrawButton(" Goto ", icon: "Animation.Play", options: GUILayout.Width(70)))
                {
                    string assetPath = path;
                    if (isMeta)
                        assetPath = path.Replace(".meta", "");
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    EditorGUIUtility.PingObject(obj);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);
        }

        void startSearchingForGUID()
        {
            // Give the window time to refresh
            if (isSearching > 1)
                isSearching--;

            if (isSearching == 1)
            {
                EditorApplication.update -= startSearchingForGUID;
                string[] guidsToSearchFor = guidToSearchFor.Split(',');
                if (scanSelected)
                {
                    searchedPaths = GUIDUtils.SearchForGUIDInSelected(guidsToSearchFor);
                }
                else
                {
                    searchedPaths = GUIDUtils.SearchForGUID(guidsToSearchFor);
                }
                isSearching = 0;
            }
        }
        #endregion

        #region Draw Replace
        private void DrawReplace()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.enabled = isReplacing == 0;

            bool isValid = GUIDUtils.IsValidGUID(guidToReplace);
            bool assetFound = !string.IsNullOrEmpty(assetPathOfGUIDToReplace);
            DrawLabel(
                (assetFound && isValid) ? assetPathOfGUIDToReplace : $"No asset found for {guidToReplace}.",
                bold: true,
                color: (assetFound && isValid) ? GUI.skin.label.normal.textColor : new Color(1f, 0.3f, 0.3f));

            // OLD GUID
            var oldStyle = new GUIStyle(GUI.skin.textField);
            if (!isValid)
            {
                oldStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                oldStyle.active.textColor = new Color(1f, 0.3f, 0.3f);
                oldStyle.focused.textColor = new Color(1f, 0.3f, 0.3f);
                oldStyle.hover.textColor = new Color(1f, 0.3f, 0.3f);
            }
            else
            {
                oldStyle.normal.textColor = Color.grey;
                oldStyle.hover.textColor = Color.grey;
            }
            string previousGUID = guidToReplace;
            guidToReplace = EditorGUILayout.TextField(new GUIContent("Old GUID:", "Enter the GUID which you wish to relace."), guidToReplace, oldStyle);
            if (previousGUID != guidToReplace && GUIDUtils.IsValidGUID(guidToReplace))
            {
                assetPathOfGUIDToReplace = AssetDatabase.GUIDToAssetPath(guidToReplace);
            }

            // NEW GUID
            isValid = GUIDUtils.IsValidGUID(guidReplacement);
            var newStyle = new GUIStyle(GUI.skin.textField);
            if (!isValid)
            {
                newStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
                newStyle.active.textColor = new Color(1f, 0.3f, 0.3f);
                newStyle.focused.textColor = new Color(1f, 0.3f, 0.3f);
                newStyle.hover.textColor = new Color(1f, 0.3f, 0.3f);
            }
            guidReplacement = EditorGUILayout.TextField(new GUIContent("NEW GUID:", "Enter the NEW GUID."), guidReplacement, newStyle);
            GUILayout.EndVertical();

            // Start button
            GUI.enabled = GUI.enabled 
                           && GUIDUtils.IsValidGUID(guidToReplace) 
                           && GUIDUtils.IsValidGUID(guidReplacement) 
                           && !string.IsNullOrEmpty(assetPathOfGUIDToReplace);
            if (GUILayout.Button(isReplacing == 1 ? "Replacing .." : "Replace"))
            {
                isReplacing = 3;
                EditorApplication.update -= startReplacingGUID;
                EditorApplication.update += startReplacingGUID;
            }
            GUI.enabled = true;
        }

        void startReplacingGUID()
        {
            // Give the window time to refresh
            if (isReplacing > 1)
                isReplacing--;

            if (isReplacing == 1)
            {
                // Check if new guid does not yet exist
                var newAssetPath = AssetDatabase.GUIDToAssetPath(guidReplacement);
                if (!string.IsNullOrEmpty(newAssetPath) && System.IO.File.Exists(newAssetPath))
                {
                    isReplacing = 0;
                    EditorUtility.DisplayDialog(
                        "The new GUID is already in use!",
                        $"The new GUID is used by '{newAssetPath}'.\n\nPlease enter another new GUID.",
                        "Okay");
                    return;
                }

                // Check if old guid asset exists.
                var assetPath = AssetDatabase.GUIDToAssetPath(guidToReplace);
                if(string.IsNullOrEmpty(assetPath))
                {
                    isReplacing = 0;
                    EditorUtility.DisplayDialog(
                        "No asset for this GUID",
                        $"No asset with the GUID: {guidToReplace} has been found. Therefore no replacement can be made.\n\nPlease check your GUID.",
                        "Okay");
                    return;
                }
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                GUIDUtils.Regenerate(new Object[] {asset}, guidReplacement);
                isReplacing = 0;
            }
        }
        #endregion

        #region Utilities

        public static class BackgroundStyle
        {
            private static GUIStyle style = new GUIStyle();
            private static Texture2D texture;

            public static GUIStyle Get(Color color)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);

                texture.SetPixel(0, 0, color);
                texture.Apply();
                style.normal.background = texture;

                return style;
            }
        }

        public static void DrawLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true, Texture icon = null, params GUILayoutOption[] options)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;

            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;
            style.imagePosition = ImagePosition.ImageLeft;

            var content = new GUIContent();
            content.text = text;
            if (icon != null)
            {
                GUILayout.Space(16);
                var position = GUILayoutUtility.GetRect(content, style);
                GUI.DrawTexture(new Rect(position.x - 16, position.y, 16, 16), icon);
                GUI.Label(position, content, style);
            }
            else
            {
                GUILayout.Label(text, style, options);
            }
        }

        public static void DrawSelectableLabel(string text, Color? color = null, bool bold = false, bool wordwrap = true, bool richText = true)
        {
            if (!color.HasValue)
                color = GUI.skin.label.normal.textColor;

            var style = new GUIStyle(GUI.skin.label);
            if (bold)
                style.fontStyle = FontStyle.Bold;
            style.normal.textColor = color.Value;
            style.wordWrap = wordwrap;
            style.richText = richText;

            var content = new GUIContent(text);
            var position = GUILayoutUtility.GetRect(content, style);
            EditorGUI.SelectableLabel(position, text, style);
        }

        public static bool DrawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
        {
            GUIContent content;

            // icon
            if (!string.IsNullOrEmpty(icon))
                content = EditorGUIUtility.IconContent(icon);
            else
                content = new GUIContent();

            // text
            content.text = text;

            // tooltip
            if (!string.IsNullOrEmpty(tooltip))
                content.tooltip = tooltip;

            return GUILayout.Button(content, options);
        }

        public static void BeginHorizontalIndent(int indentAmount = 10, bool beginVerticalInside = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(indentAmount);
            if (beginVerticalInside)
                GUILayout.BeginVertical();
        }

        public static void EndHorizontalIndent(float indentAmount = 10, bool begunVerticalInside = true, bool bothSides = false)
        {
            if (begunVerticalInside)
                GUILayout.EndVertical();
            if (bothSides)
                GUILayout.Space(indentAmount);
            GUILayout.EndHorizontal();
        }

        public static string WrapInRichTextColor(string text, Color color)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            return "<color=#" + hexColor + ">" + text + "</color>";
        }

        #endregion
    }
}
