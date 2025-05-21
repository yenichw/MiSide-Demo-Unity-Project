/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class Icons
    {
        private static Texture _addComponent;
        private static Texture _addFolder;
        private static Texture _align;
        private static Texture _alignDark;
        private static Texture _anchor;
        private static Texture _arrowDown;
        private static Texture _arrowRight;
        private static Texture _arrowUp;
        private static Texture _blueBullet;
        private static Texture _bounds;
        private static Texture _boundsDark;
        private static Texture _closeBlack;
        private static Texture _closeWhite;
        private static Texture _closeWindows;
        private static Texture _collapse;
        private static Texture _collection;
        private static Texture _copy;
        private static Texture _createObject;
        private static Texture _customPivotRotationTool;
        private static Texture _customPivotRotationToolActive;
        private static Texture _debug;
        private static Texture _debugOn;
        private static Texture _duplicate;
        private static Texture _duplicateTool;
        private static Texture _duplicateToolActive;
        private static Texture _expand;
        private static Texture _focus;
        private static Texture _focusToolbar;
        private static Texture _folder;
        private static Texture _folderEmpty;
        private static Texture _folderOpen;
        private static Texture _grayBullet;
        private static Texture _grid;
        private static Texture _gridPro;
        private static Texture _group;
        private static Texture _help;
        private static Texture _hierarchy;
        private static Texture _history;
        private static Texture _maximize;
        private static Texture _minimize;
        private static Texture _moveUpDown;
        private static Texture _note;
        private static Texture _noteEmpty;
        private static Texture _open;
        private static Texture _openNewBlack;
        private static Texture _openNewWhite;
        private static Texture _pin;
        private static Texture _pivotTool;
        private static Texture _pivotToolActive;
        private static Texture _playScene;
        private static Texture _proGridsWhite;
        private static Texture _replace;
        private static Texture _save;
        private static Texture _saveActive;
        private static Texture _search;
        private static Texture _settings;
        private static Texture _starBlack;
        private static Texture _starWhite;
        private static Texture _starYellow;
        private static Texture _timer;
        private static Texture _upDown;
        private static Texture _updateAvailable;
        private static Texture _updateAvailable2;
        private static Texture _windows;
        private static Texture _winBtnWinMax;

        public static Texture addComponent
        {
            get
            {
                if (_addComponent == null) _addComponent = ResourcesCache.GetIcon("Add-Component");
                return _addComponent;
            }
        }

        public static Texture addFolder
        {
            get
            {
                if (_addFolder == null) _addFolder = ResourcesCache.GetIcon("Add-Folder");
                return _addFolder;
            }
        }

        public static Texture align
        {
            get
            {
                if (_align == null) _align = ResourcesCache.GetIcon("Align");
                return _align;
            }
        }

        public static Texture alignDark
        {
            get
            {
                if (_alignDark == null) _alignDark = ResourcesCache.GetIcon("Align-Dark");
                return _alignDark;
            }
        }

        public static Texture anchor
        {
            get
            {
                if (_anchor == null) _anchor = ResourcesCache.GetIcon("Anchor");
                return _anchor;
            }
        }

        public static Texture arrowDown
        {
            get
            {
                if (_arrowDown == null) _arrowDown = ResourcesCache.GetIcon("Arrow-Down");
                return _arrowDown;
            }
        }

        public static Texture arrowRight
        {
            get
            {
                if (_arrowRight == null) _arrowRight = ResourcesCache.GetIcon("Arrow-Right");
                return _arrowRight;
            }
        }

        public static Texture arrowUp
        {
            get
            {
                if (_arrowUp == null) _arrowUp = ResourcesCache.GetIcon("Arrow-Up");
                return _arrowUp;
            }
        }

        public static Texture blueBullet
        {
            get
            {
                if (_blueBullet == null) _blueBullet = ResourcesCache.GetIcon("Blue-Bullet");
                return _blueBullet;
            }
        }

        public static Texture bounds
        {
            get
            {
                if (_bounds == null) _bounds = ResourcesCache.GetIcon("Bounds");
                return _bounds;
            }
        }

        public static Texture boundsDark
        {
            get
            {
                if (_boundsDark == null) _boundsDark = ResourcesCache.GetIcon("Bounds-Dark");
                return _boundsDark;
            }
        }

        public static Texture closeBlack
        {
            get
            {
                if (_closeBlack == null) _closeBlack = ResourcesCache.GetIcon("Close-Black");
                return _closeBlack;
            }
        }

        public static Texture closeWhite
        {
            get
            {
                if (_closeWhite == null) _closeWhite = ResourcesCache.GetIcon("Close-White");
                return _closeWhite;
            }
        }

        public static Texture closeWindows
        {
            get
            {
                if (_closeWindows == null) _closeWindows = ResourcesCache.GetIcon("Close-Windows");
                return _closeWindows;
            }
        }

        public static Texture collapse
        {
            get
            {
                if (_collapse == null) _collapse = ResourcesCache.GetIcon(Styles.isProSkin ? "Collapse-White" : "Collapse-Black");
                return _collapse;
            }
        }

        public static Texture collection
        {
            get
            {
                if (_collection == null) _collection = ResourcesCache.GetIcon("Collection");
                return _collection;

            }
        }
        
        public static Texture copy
        {
            get
            {
                if (_copy == null) _copy = ResourcesCache.GetIcon(Styles.isProSkin? "CopyPro": "Copy");
                return _copy;
            }
        }

        public static Texture createObject
        {
            get
            {
                if (_createObject == null) _createObject = ResourcesCache.GetIcon("Create-Object");
                return _createObject;
            }
        }
        
        public static Texture customPivotRotationTool
        {
            get
            {
                if (_customPivotRotationTool == null) _customPivotRotationTool = ResourcesCache.GetIcon(Styles.isProSkin ? "Custom-Pivot-Rotation-Tool-Active" : "Custom-Pivot-Rotation-Tool");
                return _customPivotRotationTool;
            }
        }

        public static Texture customPivotRotationToolActive
        {
            get
            {
                if (_customPivotRotationToolActive == null) _customPivotRotationToolActive = ResourcesCache.GetIcon("Custom-Pivot-Rotation-Tool-Active");
                return _customPivotRotationToolActive;
            }
        }

        public static Texture debug
        {
            get
            {
                if (_debug == null) _debug = ResourcesCache.GetIcon(Styles.isProSkin ? "DebugPro" : "Debug");
                return _debug;
            }
        }

        public static Texture debugOn
        {
            get
            {
                if (_debugOn == null) _debugOn = ResourcesCache.GetIcon("DebugOn");
                return _debugOn;
            }
        }

        public static Texture duplicate
        {
            get
            {
                if (_duplicate == null) _duplicate = ResourcesCache.GetIcon("Duplicate");
                return _duplicate;
            }
        }

        public static Texture duplicateTool
        {
            get
            {
                if (_duplicateTool == null) _duplicateTool = ResourcesCache.GetIcon(Styles.isProSkin ? "DuplicateToolPro" : "DuplicateTool");
                return _duplicateTool;
            }
        }

        public static Texture duplicateToolActive
        {
            get
            {
                if (_duplicateToolActive == null) _duplicateToolActive = ResourcesCache.GetIcon(Styles.isProSkin ? "Duplicate" : "DuplicateToolPro");
                return _duplicateToolActive;
            }
        }

        public static Texture expand
        {
            get
            {
                if (_expand == null) _expand = ResourcesCache.GetIcon(Styles.isProSkin ? "Expand-White" : "Expand-Black");
                return _expand;
            }
        }

        public static Texture focus
        {
            get
            {
                if (_focus == null) _focus = ResourcesCache.GetIcon("Focus");
                return _focus;
            }
        }

        public static Texture focusToolbar
        {
            get
            {
                if (_focusToolbar == null) _focusToolbar = ResourcesCache.GetIcon(Styles.isProSkin ? "FocusToolbarPro" : "FocusToolbar");
                return _focusToolbar;
            }
        }

        public static Texture folder
        {
            get
            {
                if (_folder == null) _folder = ResourcesCache.GetIcon("Folder");
                return _folder;
            }
        }

        public static Texture folderEmpty
        {
            get
            {
                if (_folderEmpty == null) _folderEmpty = ResourcesCache.GetIcon("FolderEmpty");
                return _folderEmpty;
            }
        }

        public static Texture folderOpen
        {
            get
            {
                if (_folderOpen == null) _folderOpen = ResourcesCache.GetIcon("FolderOpened");
                return _folderOpen;
            }
        }

        public static Texture grayBullet
        {
            get
            {
                if (_grayBullet == null) _grayBullet = ResourcesCache.GetIcon("Gray-Bullet");
                return _grayBullet;
            }
        }

        public static Texture grid
        {
            get
            {
                if (_grid == null) _grid = ResourcesCache.GetIcon("Grid");
                return _grid;
            }
        }

        public static Texture gridPro
        {
            get
            {
                if (_gridPro == null) _gridPro = ResourcesCache.GetIcon("Grid-Pro");
                return _gridPro;
            }
        }

        public static Texture group
        {
            get
            {
                if (_group == null) _group = ResourcesCache.GetIcon("Group");
                return _group;
            }
        }

        public static Texture help
        {
            get
            {
                if (_help == null) _help = ResourcesCache.GetIcon("HelpIcon");
                return _help;
            }
        }

        public static Texture hierarchy
        {
            get
            {
                if (_hierarchy == null) _hierarchy = ResourcesCache.GetIcon("Hierarchy");
                return _hierarchy;
            }
        }

        public static Texture history
        {
            get
            {
                if (_history == null) _history = ResourcesCache.GetIcon("History");
                return _history;
            }
        }

        public static Texture maximize
        {
            get
            {
                if (_maximize == null) _maximize = ResourcesCache.GetIcon("Maximize");
                return _maximize;
            }
        }

        public static Texture minimize
        {
            get
            {
                if (_minimize == null) _minimize = ResourcesCache.GetIcon("Minimize");
                return _minimize;
            }
        }

        public static Texture moveUpDown
        {
            get
            {
                if (_moveUpDown == null) _moveUpDown = ResourcesCache.GetIcon(Styles.isProSkin? "Move-UpDown-White": "Move-UpDown-Dark");
                return _moveUpDown;
            }
        }

        public static Texture note
        {
            get
            {
                if (_note == null) _note = ResourcesCache.GetIcon("Note");
                return _note;
            }
        }

        public static Texture noteEmpty
        {
            get
            {
                if (_noteEmpty == null) _noteEmpty = ResourcesCache.GetIcon("Note-Empty");
                return _noteEmpty;
            }
        }

        public static Texture open {
            get
            {
                if (_open == null) _open = ResourcesCache.GetIcon("Open");
                return _open;
            }
        }

        public static Texture openNewBlack
        {
            get
            {
                if (_openNewBlack == null) _openNewBlack = ResourcesCache.GetIcon("OpenNew-Black");
                return _openNewBlack;
            }
        }

        public static Texture openNewWhite
        {
            get
            {
                if (_openNewWhite == null) _openNewWhite = ResourcesCache.GetIcon("OpenNew-White");
                return _openNewWhite;
            }
        }

        public static Texture pin
        {
            get
            {
                if (_pin == null) _pin = ResourcesCache.GetIcon("Pin");
                return _pin;
            }
        }

        public static Texture pivotTool
        {
            get
            {
                if (_pivotTool == null) _pivotTool = ResourcesCache.GetIcon(Styles.isProSkin ? "PivotToolActive" : "PivotTool");
                return _pivotTool;
            }
        }

        public static Texture pivotToolActive
        {
            get
            {
                if (_pivotToolActive == null) _pivotToolActive = ResourcesCache.GetIcon("PivotToolActive");
                return _pivotToolActive;
            }
        }

        public static Texture playScene
        {
            get
            {
                if (_playScene == null) _playScene = ResourcesCache.GetIcon("PlayScene");
                return _playScene;
            }
        }

        public static Texture proGridsWhite
        {
            get
            {
                if (_proGridsWhite == null) _proGridsWhite = ResourcesCache.GetIcon("ProGrids-White");
                return _proGridsWhite;
            }
        }

        public static Texture replace
        {
            get
            {
                if (_replace == null) _replace = ResourcesCache.GetIcon("Replace");
                return _replace;
            }
        }

        public static Texture save
        {
            get
            {
                if (_save == null) _save = ResourcesCache.GetIcon("Save");
                return _save;
            }
        }

        public static Texture saveActive
        {
            get
            {
                if (_saveActive == null) _saveActive = ResourcesCache.GetIcon("Save-Active");
                return _saveActive;
            }
        }

        public static Texture search
        {
            get
            {
                if (_search == null) _search = ResourcesCache.GetIcon("Search");
                return _search;
            }
        }

        public static Texture settings
        {
            get
            {
                if (_settings == null) _settings = ResourcesCache.GetIcon("Settings");
                return _settings;
            }
        }

        public static Texture starBlack
        {
            get
            {
                if (_starBlack == null) _starBlack = ResourcesCache.GetIcon("Star-Black");
                return _starBlack;
            }
        }

        public static Texture starWhite
        {
            get
            {
                if (_starWhite == null) _starWhite = ResourcesCache.GetIcon("Star-White");
                return _starWhite;
            }
        }

        public static Texture starYellow
        {
            get
            {
                if (_starYellow == null) _starYellow = ResourcesCache.GetIcon("Star-Yellow");
                return _starYellow;
            }
        }

        public static Texture timer
        {
            get
            {
                if (_timer == null) _timer = ResourcesCache.GetIcon(Styles.isProSkin? "Timer" : "Timer-Black");
                return _timer;
            }
        }

        public static Texture upDown
        {
            get
            {
                if (_upDown == null) _upDown = ResourcesCache.GetIcon(Styles.isProSkin ? "Up-Down-White": "Up-Down-Black");
                return _upDown;
            }
        }

        public static Texture updateAvailable
        {
            get
            {
                if (_updateAvailable == null) _updateAvailable = ResourcesCache.GetIcon("Update-Available");
                return _updateAvailable;
            }
        }

        public static Texture updateAvailable2
        {
            get
            {
                if (_updateAvailable2 == null) _updateAvailable2 = ResourcesCache.GetIcon("Update-Available-2");
                return _updateAvailable2;
            }
        }

        public static Texture windows
        {
            get
            {
                if (_windows == null) _windows = ResourcesCache.GetIcon(Styles.isProSkin? "Windows": "Windows-Black");
                return _windows;
            }
        }

        public static Texture winBtnWinMax
        {
            get
            {
                if (_winBtnWinMax == null) _winBtnWinMax = ResourcesCache.GetIcon("winbtn_win_max");
                return _winBtnWinMax;
            }
        }
    }
}