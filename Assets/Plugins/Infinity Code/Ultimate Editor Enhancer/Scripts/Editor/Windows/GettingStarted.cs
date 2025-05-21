/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class GettingStarted : EditorWindow
    {
        private static Slide activeSlide;
        private static List<Slide> activeSlides;
        private static GUIContent[] contents;
        private static string filterText;
        private static Slide first;
        private static List<Slide> flatSlides;
        private static string folder;
        private static Slide last;
        private static bool needReload = true;
        private static bool resetSelection;
        private static bool showByGroup = true;
        private static List<Slide> slides;
        private static int totalSlides;
        private static GSTreeView treeView;
        private static GSTreeView treeView2;
        private static TreeViewState treeViewState;
        private static TreeViewState treeViewState2;
        private static List<Slide> versionSlides;

        private void DrawActiveSlide()
        {
            Event e = Event.current;

            Rect buttonsRect = new Rect(position.width - 35, 5, 30, 20);

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.RightArrow)
                {
                    SetSlide(activeSlide.next);
                }
                else if (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.LeftArrow)
                {
                    SetSlide(activeSlide.prev);
                }

                UpdateTitle();
                Repaint();
            }
            else if (e.type == EventType.MouseUp && !buttonsRect.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    SetSlide(activeSlide.next);
                }
                else if (e.button == 1)
                {
                    SetSlide(activeSlide.prev);
                }

                UpdateTitle();
                Repaint();
            }

            if (activeSlide.texture != null) GUI.DrawTexture(new Rect(302, 2, position.width - 304, position.height - 4), activeSlide.texture);
            if (contents == null) contents = new[] { new GUIContent("?", "Open Documentation") };
            int ti = GUI.Toolbar(buttonsRect, -1, contents);
            if (ti != -1) Links.OpenDocumentation(activeSlide.help);
        }

        private static bool DrawFilterTextField()
        {
            GUILayout.BeginArea(new Rect(2, 2, 300, 16));
            GUI.SetNextControlName("UEEGettingStartedSearchTextField");
            EditorGUI.BeginChangeCheck();
            filterText = GUILayoutUtils.ToolbarSearchField(filterText);
            bool changed = EditorGUI.EndChangeCheck();

            if (resetSelection && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("UEEGettingStartedSearchTextField");
                resetSelection = false;
            }

            GUILayout.EndArea();

            return changed;
        }

        private void DrawTableOfContent()
        {
            bool filterChanged = DrawFilterTextField();

            if (filterChanged || activeSlides == null)
            {
                if (!string.IsNullOrEmpty(filterText))
                {
                    string pattern = SearchableItem.GetPattern(filterText);
                    activeSlides = flatSlides.Where(p => p.Match(pattern)).ToList();
                }
                else activeSlides = slides;

                needReload = true;
            }
            
            showByGroup = GUI.Toggle(new Rect(0, 18, 150, 16), showByGroup, "By Group", EditorStyles.toolbarButton);
            showByGroup = !GUI.Toggle(new Rect(150, 18, 150, 16), !showByGroup, "By Version", EditorStyles.toolbarButton);

            if (treeView == null)
            {
                treeViewState = new TreeViewState();
                treeView = new GSTreeView(treeViewState, false);
                treeView.ExpandAll();
                needReload = false;
            }

            if (needReload) treeView.Reload();

            Rect rect = new Rect(0, 36, 300, position.height - 36);
            
            if (showByGroup) treeView.OnGUI(rect);
            else
            {
                if (treeView2 == null)
                {
                    treeViewState2 = new TreeViewState();
                    treeView2 = new GSTreeView(treeViewState2, true);
                    treeView2.ExpandAll();
                }
                
                treeView2.OnGUI(rect);
            }
        }

        private void InitSlides(List<Slide> _slides, Slide parent, ref int index, ref Slide prev)
        {
            for (int i = 0; i < _slides.Count; i++)
            {
                Slide slide = _slides[i];
                slide.parent = parent;
                if (parent != null)
                {
                    if (slide.updated == 0) slide.updated = parent.updated;
                    if (slide.added == 0) slide.added = parent.added;
                }

                if (!string.IsNullOrEmpty(slide.image))
                {
                    slide.prev = prev;
                    slide.index = ++index;
                    if (prev == null) activeSlide = slide;
                    else prev.next = slide;
                    prev = slide;
                }

                slide.id = flatSlides.Count;
                flatSlides.Add(slide);

                if (slide.slides != null) InitSlides(slide.slides, slide, ref index, ref prev);
            }
        }

        private void OnDisable()
        {
            if (slides != null)
            {
                foreach (Slide slide in slides) slide.Dispose();
                slides = null;
            }

            activeSlide = null;
            activeSlides = null;
            first = null;
            flatSlides = null;
            last = null;
            slides = null;
            treeView = null;
            treeViewState = null;
            treeView2 = null;
            treeViewState2 = null;
        }

        private void OnEnable()
        {
            folder = Resources.assetFolder + "Textures/Getting Started/";
            string content = File.ReadAllText(folder + "_Content.json", Encoding.UTF8);

            slides = Json.Deserialize<List<Slide>>(content);

            Slide prev = null;
            totalSlides = 0;
            flatSlides = new List<Slide>();
            InitSlides(slides, null, ref totalSlides, ref prev);

            versionSlides = new List<Slide>();
            Slide group = null;
            float version = 0;
            
            StringBuilder builder = StaticStringBuilder.Start();

            foreach (Slide slide in flatSlides.Where(s => s.added > 0).OrderByDescending(s => s.updated).ThenBy(s => s.title))
            {
                if (slide.updated != version)
                {
                    builder.Clear();
                    builder.Append((int)slide.updated);
                    builder.Append(".");
                    builder.Append(Math.Round(slide.updated * 100) % 100);
                    
                    group = new Slide();
                    group.slides = new List<Slide>();
                    group.title = "Version " + builder;
                    group.updated = slide.updated;
                    group.index = group.id = flatSlides.Count + versionSlides.Count + 1; 
                    versionSlides.Add(group);
                    version = slide.updated;
                }
                
                group.slides.Add(slide.Copy());
            }

            flatSlides = flatSlides.ToList();

            last = prev;
            first = activeSlide;

            first.prev = last;
            last.next = first;

            minSize = new Vector2(904, 454);
            maxSize = new Vector2(904, 454);
            showByGroup = true;

            UpdateTitle();
            SetSlide(activeSlide);
        }

        public void OnGUI()
        {
            DrawTableOfContent();
            DrawActiveSlide();
        }

        [MenuItem(WindowsHelper.MenuPath + "Getting Started", false, MenuItemOrder.GettingStarted)]
        public static void OpenWindow()
        {
            GettingStarted wnd = GetWindow<GettingStarted>(true, "Getting Started", true);
            SetSlide(activeSlide); 
            wnd.UpdateTitle();
        }

        private static void SetSlide(Slide slide)
        {
            if (string.IsNullOrEmpty(slide.image))
            {
                if (slide.slides == null) return;
                
                bool success = false;
                for (int i = 0; i < slide.slides.Count; i++)
                {
                    Slide s = slide.slides[i];
                    if (string.IsNullOrEmpty(s.image)) continue;

                    slide = s;
                    success = true;
                    break;
                }

                if (!success) return;
            }
            activeSlide = slide;
            if (treeView != null)
            {
                try
                {
                    treeView.SetSelection(new List<int>{ slide.id });
                    treeView.FrameItem(slide.id);
                }
                catch
                {
                }
            }
            
            if (treeView2 != null)
            {
                try
                {
                    treeView2.SetSelection(new List<int>{ slide.id });
                    treeView2.FrameItem(slide.id);
                }
                catch
                {
                }
            }
        }

        private void UpdateTitle()
        {
            titleContent = new GUIContent("Getting Started. Frame " + activeSlide.index + " / " + totalSlides + " (click to continue)");
        }

        public class Slide : SearchableItem
        {
            public string title;
            public string image;
            public string help;
            public List<Slide> slides;
            public int index;
            public float added;
            public float updated;

            public Slide next;
            public Slide parent;
            public Slide prev;
            private Texture2D _texture;
            public int id;

            public Texture2D texture
            {
                get
                {
                    if (_texture == null)
                    {
                        _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + image);
                    }

                    return _texture;
                }
            }

            public Slide Copy()
            {
                Slide slide = new Slide
                {
                    title = title,
                    image = image,
                    help = help,
                    index = index,
                    added = added,
                    updated = updated,
                    next = next,
                    parent = parent,
                    prev = prev,
                    id = id
                };
                return slide;
            }

            public void Dispose()
            {
                if (slides != null)
                {
                    foreach (Slide slide in slides)
                    {
                        slide.Dispose();
                    }
                }

                slides = null;
                next = null;
                prev = null;
                _texture = null;
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return title;
            }
        }

        internal class GSTreeView : TreeView
        {
            private List<TreeViewItem> allItems;
            private bool useVersionSlides; 

            public GSTreeView(TreeViewState state, bool useVersionSlides) : base(state)
            {
                this.useVersionSlides = useVersionSlides;
                useScrollView = true;
                showBorder = true;
                showAlternatingRowBackgrounds = true;
                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                TreeViewItem root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
                allItems = new List<TreeViewItem>();

                BuildTree(useVersionSlides? versionSlides: activeSlides, 0);

                SetupParentsAndChildrenFromDepths(root, allItems);

                return root;
            }

            private void BuildTree(List<Slide> slides, int depth)
            {
                for (int i = 0; i < slides.Count; i++)
                {
                    Slide slide = slides[i];
                    GSTreeViewItem item = new GSTreeViewItem
                    {
                        id = slide.id, 
                        depth = depth, 
                        displayName = slide.title,
                        slide = slide
                    };
                    allItems.Add(item);
                    if (slide.slides != null && slide.slides.Count > 0) BuildTree(slide.slides, depth + 1);
                }
            }

            protected override void SingleClickedItem(int id)
            {
                TreeViewItem item = FindItem(id, rootItem);
                SetSlide((item as GSTreeViewItem).slide);
            }
        }

        public class GSTreeViewItem : TreeViewItem
        {
            public Slide slide;
        }

        internal class GSItem : SearchableItem
        {
            public string name;
            public GameObject target;

            public GSItem(GameObject target)
            {
                this.target = target;
                name = target.name;
            }

            public void Dispose()
            {
                target = null;
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return name;
            }
        }
    }
}