/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class Updater : EditorWindow
    {
        private const string PackageID = "Ultimate Editor Enhancer";
        private const string LastVersionKey = Prefs.Prefix + "LastVersion";
        private const string LastVersionCheckKey = Prefs.Prefix + "LastVersionCheck";
        private const string ChannelKey = Prefs.Prefix + "UpdateChannel";
        private const string InvoiceNumberKey = Prefs.Prefix + "InvoiceNumber";
        private const long TicksInHour = 36000000000;

        public static bool hasNewVersion = false;

        private static Channel channel = Channel.stable;
        private string invoiceNumber;
        private Vector2 scrollPosition;
        private List<Item> updates;
        private static string lastVersionID;
        private GUIContent helpContent;

        private void CheckNewVersions()
        {
            if (string.IsNullOrEmpty(invoiceNumber))
            {
                EditorUtility.DisplayDialog("Error", "Please enter the Invoice Number.", "OK");
                return;
            }

            SavePrefs();

            string updateKey = GetUpdateKey();
            GetUpdateList(updateKey);
        }

        public static void CheckNewVersionAvailable()
        {
            if (!IsNeedToSendRequest()) return;

            EditorPrefs.SetInt(LastVersionCheckKey, (int)(DateTime.Now.Ticks / TicksInHour));

            if (EditorPrefs.HasKey(ChannelKey)) channel = (Channel)EditorPrefs.GetInt(ChannelKey);
            else channel = Channel.stable;

            if (channel == Channel.stablePrevious) channel = Channel.stable;

            WebClient client = new WebClient();

            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            client.UploadDataCompleted += OnCheckUpdateComplete;
            client.UploadDataAsync(new Uri("https://infinity-code.com/products_update/getlastversion.php"), "POST", Encoding.UTF8.GetBytes("c=" + (int)channel + "&package=" + UnityWebRequest.EscapeURL(PackageID)));
        }

        private static bool CompareVersions()
        {
            double v1 = GetDoubleVersion(Version.version);
            double v2 = GetDoubleVersion(lastVersionID);
            return v1 < v2;
        }

        private static double GetDoubleVersion(string v)
        {
            string[] vs = v.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (vs[1].Length < 2) vs[1] = "0" + vs[1];
            if (vs[2].Length < 2) vs[2] = "0" + vs[2];
            if (vs[3].Length < 4)
            {
                vs[3] = "000" + vs[3];
                vs[3] = vs[3].Substring(vs[3].Length - 4, 4);
            }
            v = vs[0] + "." + vs[1] + vs[2] + vs[3];
            double result;
            if (!double.TryParse(v, NumberStyles.AllowDecimalPoint, Culture.numberFormat, out result)) result = 1;
            return result;
        }

        private string GetUpdateKey()
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string updateKey = client.UploadString("https://infinity-code.com/products_update/getupdatekey.php",
                "key=" + invoiceNumber + "&package=" + UnityWebRequest.EscapeURL(PackageID));

            return updateKey;
        }

        private void GetUpdateList(string updateKey)
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            string response;

            try
            {
                response = client.UploadString("https://infinity-code.com/products_update/checkupdates.php",
                    "k=" + UnityWebRequest.EscapeURL(updateKey) + "&v=" + Version.version + "&c=" + (int)channel);
            }
            catch (Exception exception)
            {
                Debug.Log(exception.Message);
                return;
            }

            XmlDocument document = new XmlDocument();
            document.LoadXml(response);

            XmlNode firstChild = document.FirstChild;
            updates = new List<Item>();

            foreach (XmlNode node in firstChild.ChildNodes) updates.Add(new Item(node));
        }

        private static bool IsNeedToSendRequest()
        {
            if (!Prefs.checkForUpdates) return false;
            if (EditorPrefs.HasKey(LastVersionKey))
            {
                lastVersionID = EditorPrefs.GetString(LastVersionKey);

                if (CompareVersions())
                {
                    hasNewVersion = false;
                    return true;
                }
            }

            if (EditorPrefs.HasKey(LastVersionCheckKey))
            {
                long lastVersionCheck = EditorPrefs.GetInt(LastVersionCheckKey) * TicksInHour;
                if (DateTime.Now.Ticks - lastVersionCheck < 24 * TicksInHour)
                {
                    return false;
                }
            }

            return true;
        }

        private static void OnCheckUpdateComplete(object sender, UploadDataCompletedEventArgs response)
        {
            if (response.Error != null)
            {
                Debug.Log(response.Error.Message);
                return;
            }

            string version = Encoding.UTF8.GetString(response.Result);

            try
            {
                string[] vars = version.Split('.');
                string[] vars2 = new string[4];
                while (vars[1].Length < 8) vars[1] += "0";
                vars2[0] = vars[0];
                vars2[1] = int.Parse(vars[1].Substring(0, 2)).ToString();
                vars2[2] = int.Parse(vars[1].Substring(2, 2)).ToString();
                vars2[3] = int.Parse(vars[1].Substring(4)).ToString();

                version = string.Join(".", vars2);
            }
            catch (Exception)
            {
                Debug.Log("Automatic check for Ultimate Editor Enhancer updates: Bad response.");
                return;
            }

            lastVersionID = version;

            hasNewVersion = CompareVersions();
            EditorApplication.update += SetLastVersion;
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey(InvoiceNumberKey)) invoiceNumber = EditorPrefs.GetString(InvoiceNumberKey);
            else invoiceNumber = "";

            if (EditorPrefs.HasKey(ChannelKey)) channel = (Channel)EditorPrefs.GetInt(ChannelKey);
            else channel = Channel.stable;

            helpContent = new GUIContent(Icons.help, "You can find out your Invoice Number in the email confirming the purchase, or page the user in Unity Asset Store.\nClick to go to the Unity Asset Store.");
        }

        private void OnDestroy()
        {
            SavePrefs();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            invoiceNumber = EditorGUILayout.TextField("Invoice Number:", invoiceNumber).Trim(' ');

            GUIStyle helpStyle = new GUIStyle();
            helpStyle.margin = new RectOffset(2, 2, 2, 2);

            if (helpContent != null && GUILayout.Button(helpContent, helpStyle, GUILayout.ExpandWidth(false)))
            {
                Process.Start("https://assetstore.unity.com/orders");
            }

            EditorGUILayout.EndHorizontal();

            channel = (Channel)EditorGUILayout.EnumPopup("Channel:", channel);
            GUILayout.Label("Current version: " + Version.version);

            if (GUILayout.Button("Check new versions")) CheckNewVersions();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (updates != null)
            {
                foreach (Item update in updates) update.Draw();
                if (updates.Count == 0) GUILayout.Label("No updates");
            }

            EditorGUILayout.EndScrollView();
        }

        [MenuItem(WindowsHelper.MenuPath + "Check Updates", false, MenuItemOrder.CheckUpdates)]
        public static void OpenWindow()
        {
            GetWindow<Updater>(false, "Ultimate Editor Enhancer Updater", true);
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(InvoiceNumberKey, invoiceNumber);
            EditorPrefs.SetInt(ChannelKey, (int)channel);
        }

        private static void SetLastVersion()
        {
            EditorPrefs.SetString(LastVersionKey, lastVersionID);
            EditorApplication.update -= SetLastVersion;
        }

        private static string Unescape(string str)
        {
            return Regex.Replace(str, @"&(\w+);", delegate (Match match)
            {
                string v = match.Groups[1].Value;
                if (v == "amp") return "&";
                if (v == "lt") return "<";
                if (v == "gt") return ">";
                if (v == "quot") return "\"";
                if (v == "apos") return "'";
                return match.Value;
            });
        }

        public class Item
        {
            private string version;
            private int type;
            private string changelog;
            private string download;
            private string date;

            private static GUIStyle _changelogStyle;
            private static GUIStyle _titleStyle;

            private static GUIStyle changelogStyle
            {
                get
                {
                    if (_changelogStyle == null) _changelogStyle = new GUIStyle(EditorStyles.label) {wordWrap = true};
                    return _changelogStyle;
                }
            }

            private static GUIStyle titleStyle
            {
                get
                {
                    if (_titleStyle == null) _titleStyle = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
                    return _titleStyle;
                }
            }

            public Item(XmlNode node)
            {
                version = node.SelectSingleNode("Version").InnerText;
                type = int.Parse(node.SelectSingleNode("Type").InnerText);
                changelog = Unescape(node.SelectSingleNode("ChangeLog").InnerText);
                download = node.SelectSingleNode("Download").InnerText;
                date = node.SelectSingleNode("Date").InnerText;

                string[] vars = version.Split('.');
                string[] vars2 = new string[4];
                vars2[0] = vars[0];
                vars2[1] = int.Parse(vars[1].Substring(0, 2)).ToString();
                vars2[2] = int.Parse(vars[1].Substring(2, 2)).ToString();
                vars2[3] = int.Parse(vars[1].Substring(4, 4)).ToString();
                version = string.Join(".", vars2);
            }

            public void Draw()
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Version: " + version + " (" + typeStr + "). " + date, titleStyle);

                GUILayout.Label(changelog, changelogStyle);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Download"))
                {
                    Process.Start("https://infinity-code.com/products_update/download.php?k=" + download);
                }

                if (GUILayout.Button("Copy download link", GUILayout.ExpandWidth(false)))
                {
                    EditorGUIUtility.systemCopyBuffer = "https://infinity-code.com/products_update/download.php?k=" + download;
                    EditorUtility.DisplayDialog("Success",
                        "Download link is copied to the clipboard.\nOpen a browser and paste the link into the address bar.",
                        "OK");
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            public string typeStr
            {
                get { return Enum.GetName(typeof(Channel), type); }
            }
        }

        public enum Channel
        {
            stable = 10,
            stablePrevious = 15,
            releaseCandidate = 20,
            beta = 30,
            alpha = 40,
            working = 50
        }
    }
}
