/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class Links
    {
        public const string AssetStore = "https://assetstore.unity.com/packages/tools/utilities/ultimate-editor-enhancer-141831";
        public const string Changelog = "https://infinity-code.com/products_update/get-changelog.php?asset=Ultimate%20Editor%20Enhancer&from=1.0";
        public const string Discord = "https://discord.gg/2XRWwPgZK4";
        public const string Documentation = "https://infinity-code.com/documentation/ultimate-editor-enhancer.html";
        public const string Forum = "https://forum.infinity-code.com";
        public const string Homepage = "https://infinity-code.com/assets/ultimate-editor-enhancer";
        public const string Reviews = AssetStore + "/reviews";
        public const string Support = "mailto:support@infinity-code.com?subject=Ultimate%20Editor%20Enhancer";
        public const string Youtube = "https://www.youtube.com/playlist?list=PL2QU1uhBMew_mR83EYhex5q3uZaMTwg1S";
        private const string Aid = "?aid=1100liByC";

        public static void Open(string url)
        {
            Application.OpenURL(url);
        }

        public static void OpenAssetStore()
        {
            Open(AssetStore + Aid);
        }

        public static void OpenChangelog()
        {
            Open(Changelog);
        }

        public static void OpenDiscord()
        {
            Open(Discord);
        }

        [MenuItem(WindowsHelper.MenuPath + "Documentation", false, MenuItemOrder.Documentation)]
        public static void OpenDocumentation()
        {
            OpenDocumentation(null);
        }

        public static void OpenDocumentation(string anchor)
        {
            string url = Documentation;
            if (!string.IsNullOrEmpty(anchor)) url += "#" + anchor;
            Open(url);
        }

        public static void OpenForum()
        {
            Open(Forum);
        }

        public static void OpenHomepage()
        {
            Open(Homepage);
        }

        public static void OpenLocalDocumentation()
        {
            string url = Resources.assetFolder + "Documentation/Content/Documentation-Content.html";
            Application.OpenURL(url);
        }

        public static void OpenReviews()
        {
            Open(Reviews + Aid);
        }

        public static void OpenSupport()
        {
            Open(Support);
        }

        public static void OpenYouTube()
        {
            Open(Youtube);
        }
    }
}