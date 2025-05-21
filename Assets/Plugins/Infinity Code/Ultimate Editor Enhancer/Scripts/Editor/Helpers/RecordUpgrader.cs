/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using InfinityCode.UltimateEditorEnhancer.InspectorTools;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.ProjectTools;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.SceneTools.QuickAccessActions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class RecordUpgrader
    {
        private const int CurrentUpgradeID = 6;

        static RecordUpgrader()
        {
            int upgradeID = LocalSettings.upgradeID;
            if (upgradeID < 1) InitDefaultQuickAccessItems();
            if (upgradeID < 2)
            {
                ReplaceSaveQuickAccessItem();
                TryInsertOpenAction();
            }

            if (upgradeID < 3)
            {
                TryAddHeaderRules();
            }

            if (upgradeID < 4)
            {
                InitDefaultEmptyInspectorItems();
                UpdateQuickAccessWelcomeSize();
            }

            if (upgradeID < 5)
            { 
                InitDefaultProjectIcons();
            }
            
            if (upgradeID < 6)
            {
                TryInsertNoteManager();
            }

            ReferenceManager.Save();
            LocalSettings.upgradeID = CurrentUpgradeID;
        }

        private static EmptyInspector.Item CreateUEEEmptyInspectorItem(string title)
        {
            return new EmptyInspector.Item
            {
                title = title,
                menuPath = WindowsHelper.MenuPath + title
            };
        }

        public static void InitDefaultEmptyInspectorItems()
        {
            List<EmptyInspector.Group> groups = ReferenceManager.emptyInspectorItems;
            groups.Clear();

            EmptyInspector.Group packageGroup = new EmptyInspector.Group("Packages", new List<EmptyInspector.Item>
            {
                new EmptyInspector.Item
                {
                    menuPath = "Assets/Import Package/Custom Package...",
                    title = "Import Custom Package"
                }
            });

            groups.AddRange(new []
            {
                // UEE Items
                new EmptyInspector.Group("Ultimate Editor Enhancer", new List<EmptyInspector.Item>
                {
                    CreateUEEEmptyInspectorItem("Bookmarks"),
                    CreateUEEEmptyInspectorItem("Distance Tool"),
                    CreateUEEEmptyInspectorItem("View Gallery"),
                    CreateUEEEmptyInspectorItem("Documentation"),
                    CreateUEEEmptyInspectorItem("Settings"),
                }),

                // Settings
                new EmptyInspector.Group("Settings", new List<EmptyInspector.Item>
                {
                    new EmptyInspector.Item
                    {
                        menuPath = "Edit/Project Settings...",
                        title = "Project Settings"
                    },
                    new EmptyInspector.Item
                    {
                        menuPath = "Edit/Preferences...",
                        title = "Preferences"
                    },
                    new EmptyInspector.Item
                    {
#if !UNITY_EDITOR_OSX
                        menuPath = "Edit/Shortcuts...",
#else
                        menuPath = "Unity/Shortcuts...",
#endif
                        title = "Shortcuts"
                    },
                }),


                // Packages
                packageGroup
            });
            
            EmptyInspector.Group lastGroup = packageGroup;

            foreach (string submenu in Unsupported.GetSubmenus("Window"))
            {
                if (!ValidateEmptyInspectorMenuItem(submenu)) continue;

                string[] parts = submenu.Split('/');
                string firstPart = parts[1];

                if (parts.Length == 2)
                {
                    if (firstPart == "Package Manager" || firstPart == "Asset Store" || firstPart == "My Assets")
                    {
                        lastGroup = groups.FirstOrDefault(g => g.title == "Packages");
                        if (lastGroup == null)
                        {
                            lastGroup = new EmptyInspector.Group("Packages");
                            groups.Add(lastGroup);
                        }
                    }
                    else
                    {
                        lastGroup = new EmptyInspector.Group(firstPart);
                        groups.Add(lastGroup);
                    }
                    
                    lastGroup.Add(new EmptyInspector.Item
                    {
                        menuPath = submenu,
                        title = firstPart
                    });
                }
                else if (parts.Length == 3)
                {
                    if (lastGroup.title != firstPart)
                    {
                        lastGroup = new EmptyInspector.Group(firstPart);
                        groups.Add(lastGroup);
                    }

                    lastGroup.Add(new EmptyInspector.Item
                    {
                        menuPath = submenu,
                        title = parts[2]
                    });
                }
            }
        }

        public static void InitDefaultQuickAccessItems()
        {
            List<QuickAccessItem> items = ReferenceManager.quickAccessItems;
            if (items.Count > 0) return;

            QuickAccessItem open = new QuickAccessItem(QuickAccessItemType.action)
            {
                settings = new[] { typeof(OpenAction).FullName },
                tooltip = "Open Scene",
                expanded = false
            };

            QuickAccessItem save = new QuickAccessItem(QuickAccessItemType.action)
            {
                settings = new[] { typeof(SaveAction).FullName },
                tooltip = "Save Scenes",
                expanded = false
            };

            QuickAccessItem hierarchy = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { SceneHierarchyWindowRef.type.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Hierarchy2.png",
                tooltip = "Hierarchy",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem project = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { ProjectBrowserRef.type.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Project.png",
                tooltip = "Project",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem inspector = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { InspectorWindowRef.type.AssemblyQualifiedName },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Inspector.png",
                tooltip = "Inspector",
                visibleRules = SceneViewVisibleRules.onMaximized,
                expanded = false
            };

            QuickAccessItem bookmarks = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.Windows.Bookmarks, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Star-White.png",
                tooltip = "Bookmarks",
                expanded = false
            };
            
            QuickAccessItem notes = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.NoteManager, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Note-Empty.png",
                tooltip = "Note Manager",
                expanded = false
            };

            QuickAccessItem viewGallery = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.Windows.ViewGallery, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "d_ViewToolOrbit",
                tooltip = "View Gallery",
                expanded = false
            };

            QuickAccessItem distanceTool = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.Windows.DistanceTool, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Rule.png",
                tooltip = "Distance Tool",
                expanded = false
            };

            QuickAccessItem quickAccessSettings = new QuickAccessItem(QuickAccessItemType.settings)
            {
                settings = new[] { "Project/Ultimate Editor Enhancer/Scene View/Quick Access Bar" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "d_Settings",
                tooltip = "Edit Items",
                expanded = false
            };

            QuickAccessItem info = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.Windows.Welcome, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.editorIconContent,
                iconSettings = "_Help",
                tooltip = "Info",
                expanded = false,
                useCustomWindowSize = true,
                customWindowSize = new Vector2(500, 300)
            };

            items.Add(open);
            items.Add(save);
            items.Add(hierarchy);
            items.Add(project);
            items.Add(inspector);
            items.Add(bookmarks);
            items.Add(notes);
            items.Add(viewGallery);
            items.Add(distanceTool);
            items.Add(new QuickAccessItem(QuickAccessItemType.flexibleSpace));
            items.Add(quickAccessSettings);
            items.Add(info);
        }

        private static void InitDefaultProjectIcons()
        {
            if (ReferenceManager.projectFolderIcons.Count > 0) return;

            string path = Resources.assetFolder + "LocalResources/DefaultItems/DefaultProjectIcons.json";
            if (!File.Exists(path)) return;

            try
            {
                string content = File.ReadAllText(path, Encoding.UTF8);
                JsonItem json = Json.Parse(content);
                List<ProjectFolderRule> items = json["project-icons"].Deserialize<List<ProjectFolderRule>>();
                ReferenceManager.projectFolderIcons.Clear();
                ReferenceManager.projectFolderIcons.AddRange(items);
            }
            catch
            {
            }
        }

        private static void ReplaceSaveQuickAccessItem()
        {
            List<QuickAccessItem> items = ReferenceManager.quickAccessItems;
            if (items.Count == 0) return;

            foreach (QuickAccessItem item in items)
            {
                if (item.type == QuickAccessItemType.menuItem && item.settings[0] == "File/Save")
                {
                    item.type = QuickAccessItemType.action;
                    item.settings[0] = typeof(SaveAction).FullName;
                    TitleAttribute titleAttribute = typeof(SaveAction).GetCustomAttribute<TitleAttribute>();
                    item.tooltip = titleAttribute != null ? titleAttribute.displayName : "Save Scenes";
                    item.icon = QuickAccessItemIcon.editorIconContent;
                }
            }
        }

        private static void TryAddHeaderRules()
        {
            if (ReferenceManager.headerRules.Count != 0) return;

            ReferenceManager.headerRules.Add(new HeaderRule
            {
                condition = HeaderCondition.nameStarts,
                value = "--",
                trimChars = "-=",
                backgroundColor = Color.gray,
                textColor = Color.white,
                textAlign = TextAlignment.Center,
                textStyle = FontStyle.Bold
            });
        }

        private static void TryInsertNoteManager()
        {
            List<QuickAccessItem> items = ReferenceManager.quickAccessItems;
            if (items.Count < 1) return;

            if (items.Any(item => item.type == QuickAccessItemType.window && 
                                  item.settings[0].StartsWith("InfinityCode.UltimateEditorEnhancer.NoteManager")))
            {
                return;
            }

            QuickAccessItem bookmarks = items.FirstOrDefault(i => i.type == QuickAccessItemType.window &&
                i.settings[0].StartsWith("InfinityCode.UltimateEditorEnhancer.Windows.Bookmarks")); 
            
            int index = items.Count;
            if (bookmarks != null) index = items.IndexOf(bookmarks) + 1;

            QuickAccessItem notes = new QuickAccessItem(QuickAccessItemType.window)
            {
                settings = new[] { "InfinityCode.UltimateEditorEnhancer.NoteManager, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                icon = QuickAccessItemIcon.texture,
                iconSettings = Resources.iconsFolder + "Note-Empty.png",
                tooltip = "Note Manager",
                expanded = false
            };
            
            items.Insert(index, notes);
        }

        private static void TryInsertOpenAction()
        {
            List<QuickAccessItem> items = ReferenceManager.quickAccessItems;
            if (items.Count < 1) return;

            QuickAccessItem item = items[0];
            if (item.type != QuickAccessItemType.action || item.settings[0] != typeof(SaveAction).FullName) return;

            QuickAccessItem open = new QuickAccessItem(QuickAccessItemType.action)
            {
                settings = new[] { typeof(OpenAction).FullName },
                tooltip = "Open Scene",
                expanded = false
            };

            items.Insert(0, open);
        }

        private static void UpdateQuickAccessWelcomeSize()
        {
            QuickAccessItem item = ReferenceManager.quickAccessItems.FirstOrDefault(i => i.type == QuickAccessItemType.window &&
                i.settings.Length > 0 &&
                i.settings[0] == "InfinityCode.UltimateEditorEnhancer.Windows.Welcome, UltimateEditorEnhancer-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (item != null)
            {
                if (!item.useCustomWindowSize)
                {
                    item.useCustomWindowSize = true;
                    item.customWindowSize = new Vector2(500, 300);
                } 
            }
        }

        private static bool ValidateEmptyInspectorMenuItem(string submenu)
        {
            string upper = Culture.textInfo.ToUpper(submenu);
            //Debug.Log(upper);
            //if (!upper.StartsWith("WINDOW/")) return false;
            if (upper.StartsWith("WINDOW/PANELS/")) return false;
            if (upper.StartsWith("WINDOW/LAYOUTS/")) return false;
            if (upper == "WINDOW/NEXT WINDOW") return false;
            if (upper == "WINDOW/PREVIOUS WINDOW") return false;
            //if (upper == "WINDOW/PACKAGE MANAGER") return false;
            return true;
        }
    }
}