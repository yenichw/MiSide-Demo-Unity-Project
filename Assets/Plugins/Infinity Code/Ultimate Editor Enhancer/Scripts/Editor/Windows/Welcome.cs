/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class Welcome : EditorWindow
    {
        private const string ShowAtStartupPrefs = Prefs.Prefix + "ShowWelcomeScreen";

        public static Func<bool> OpenAtStartupValidate;
        public static Action OnInitLate;

        private string copyright = "Infinity Code " + DateTime.Now.Year;
        private static bool showAtStartup = true;
        private Vector2 scrollPosition;
        private static bool inited;
        public static GUIStyle headerStyle;
        private static Texture2D discordTexture;
        private static Texture2D docTexture;
        private static Texture2D gettingStartedTexture;
        private static Texture2D forumTexture;
        private static Texture2D proFeaturesTexture;
        private static Texture2D rateTexture;
        private static Texture2D settingsTexture;
        private static Texture2D shortcutsTexture;
        private static Texture2D supportTexture;
        private static Texture2D teamTexture;
        public static Texture2D updateTexture;
        private static Texture2D urlTexture;
        private static Texture2D videoTexture;
        private static GUIStyle copyrightStyle;
        private static Welcome wnd;

        static Welcome()
        {
            EditorApplication.update -= GetShowAtStartup;
            EditorApplication.update += GetShowAtStartup;
        }

        public static bool DrawButton(Texture2D texture, string title, string body = "", int space = 10)
        {
            try
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(34);
                GUILayout.Box(texture, GUIStyle.none, GUILayout.MaxWidth(48), GUILayout.MaxHeight(48));
                GUILayout.Space(10);

                GUILayout.BeginVertical();
                GUILayout.Space(1);
                GUILayout.Label(title, EditorStyles.boldLabel);
                GUILayout.Label(body, EditorStyles.wordWrappedLabel);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                Rect rect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

                bool returnValue = Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);

                GUILayout.Space(space);

                return returnValue;
            }
            catch
            {

            }

            return false;
        }

        private void DrawContent()
        {
            GUI.Box(new Rect(0, 0, 500, 58), "v" + Version.version, headerStyle);
            GUILayoutUtility.GetRect(position.width, 58);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);

            if (DrawButton(gettingStartedTexture, "Getting Started", "A quick tour of the main features of Ultimate Editor Enhancer"))
            {
                GettingStarted.OpenWindow();
            }
            
            if (DrawButton(teamTexture, "Team Work", "If you are working in a team, please see the guidelines."))
            {
                Links.OpenDocumentation("team-work");
            }

            if (DrawButton(shortcutsTexture, "Shortcuts", "Explore all Ultimate Editor Enhancer shortcuts"))
            {
                Shortcuts.OpenWindow();
            }

            if (DrawButton(settingsTexture, "Settings", "Customize Ultimate Editor Enhancer to fit your workflow perfectly"))
            {
                SettingsService.OpenProjectSettings("Project/Ultimate Editor Enhancer");
            }

            if (DrawButton(urlTexture, "Product Page", "Visit the official asset page"))
            {
                Links.OpenHomepage();
            }

            if (DrawButton(docTexture, "Documentation", "Online version of the documentation"))
            {
                Links.OpenDocumentation();
            }

            if (DrawButton(supportTexture, "Support", "If you have any problems feel free to contact us"))
            {
                Links.OpenSupport();
            }
            
            if (DrawButton(discordTexture, "Discord", "Join our Discord server"))
            {
                Links.OpenDiscord();
            }

            if (DrawButton(forumTexture, "Forum", "Official forum of Ultimate Editor Enhancer"))
            {
                Links.OpenForum();
            }

            if (DrawButton(videoTexture, "Videos", "Check out new videos about asset"))
            {
                Links.OpenYouTube();
            }

            if (DrawButton(rateTexture, "Rate and Review", "Share your impression about the asset"))
            {
                Links.OpenReviews();
            }

            if (DrawButton(updateTexture, "Check Updates", "Perhaps a new version is already waiting for you. Check it!"))
            {
                Updater.OpenWindow();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField(copyright, copyrightStyle);
        }

        private static void GetShowAtStartup()
        {
            EditorApplication.update -= GetShowAtStartup;
            showAtStartup = EditorPrefs.GetBool(ShowAtStartupPrefs, true);

            if (showAtStartup)
            {
                EditorApplication.update -= OpenAtStartup;
                EditorApplication.update += OpenAtStartup;
            }
        }

        private void Init()
        {
            headerStyle = new GUIStyle();
            headerStyle.normal.background = Resources.Load<Texture2D>("Textures/Welcome/Logo.png");
            headerStyle.padding = new RectOffset(390, 0, 25, 0);
            headerStyle.normal.textColor = Color.black;
            headerStyle.margin = new RectOffset(0, 0, 0, 0);

            copyrightStyle = new GUIStyle();
            copyrightStyle.alignment = TextAnchor.MiddleRight;
            copyrightStyle.normal.textColor = Styles.isProSkin ? Color.white : Color.black;

            gettingStartedTexture = Resources.Load<Texture2D>("Textures/Welcome/Getting Started.png");
            docTexture = Resources.Load<Texture2D>("Textures/Welcome/Docs.png");
            forumTexture = Resources.Load<Texture2D>("Textures/Welcome/Forum.png");
            discordTexture = Resources.Load<Texture2D>("Textures/Welcome/Discord.png");
            proFeaturesTexture = Resources.Load<Texture2D>("Textures/Welcome/Pro Features.png");
            rateTexture = Resources.Load<Texture2D>("Textures/Welcome/Rate.png");
            settingsTexture = Resources.Load<Texture2D>("Textures/Welcome/Settings.png");
            shortcutsTexture = Resources.Load<Texture2D>("Textures/Welcome/Shortcuts.png");
            supportTexture = Resources.Load<Texture2D>("Textures/Welcome/Support.png");
            teamTexture = Resources.Load<Texture2D>("Textures/Welcome/Team.png");
            updateTexture = Resources.Load<Texture2D>("Textures/Welcome/Update.png");
            urlTexture = Resources.Load<Texture2D>("Textures/Welcome/URL.png");
            videoTexture = Resources.Load<Texture2D>("Textures/Welcome/Video.png");

            if (OnInitLate != null) OnInitLate();

            inited = true;
        }

        private void OnDestroy()
        {
            wnd = null;
            EditorPrefs.SetBool(ShowAtStartupPrefs, false);

            Resources.Unload(gettingStartedTexture);
            Resources.Unload(docTexture);
            Resources.Unload(forumTexture);
            Resources.Unload(proFeaturesTexture);
            Resources.Unload(settingsTexture);
            Resources.Unload(shortcutsTexture);
            Resources.Unload(supportTexture);
            Resources.Unload(teamTexture);
            Resources.Unload(updateTexture);
            Resources.Unload(urlTexture);
            Resources.Unload(videoTexture);
        }

        private void OnEnable()
        {
            wnd = this;
            wnd.minSize = new Vector2(500, 300);
            wnd.maxSize = new Vector2(500, 300);
        }

        private void OnGUI()
        {
            if (!inited) Init();

            try
            {
                DrawContent();
            }
            catch
            {

            }
        }

        private static void OpenAtStartup()
        {
            EditorApplication.update -= OpenAtStartup;
            if (OpenAtStartupValidate != null && !OpenAtStartupValidate()) return;
            OpenWindow();
        }

        [MenuItem(WindowsHelper.MenuPath + "Welcome", false, MenuItemOrder.Welcome)]
        public static void OpenWindow()
        {
            if (wnd != null) return;

            wnd = GetWindow<Welcome>(true, "Welcome to Ultimate Editor Enhancer", true);
            wnd.maxSize = wnd.minSize = new Vector2(500, 440);
            wnd.Focus();
        }

        [MenuItem(WindowsHelper.MenuPath + "Rate and Review", false, MenuItemOrder.RateAndReview)]
        public static void RateAndReview()
        {
            Links.OpenReviews();
        }
    }
}
