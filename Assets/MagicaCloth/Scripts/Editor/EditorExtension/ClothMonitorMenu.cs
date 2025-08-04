// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// MagicaCloth?????????
    /// </summary>
    public class ClothMonitorMenu : EditorWindow
    {
        public static ClothMonitorMenu Monitor { get; set; }

        [SerializeField]
        private ClothMonitorUI ui = new ClothMonitorUI();

        //=========================================================================================
        [MenuItem("Tools/Magica Cloth/Cloth Monitor", false)]
        public static void InitWindow()
        {
            GetWindow<ClothMonitorMenu>();
        }

        //=========================================================================================
        public ClothMonitorUI UI
        {
            get
            {
                return ui;
            }
        }

        //=========================================================================================
        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnUpdate;
            ui.Enable();
            Monitor = this; // ???????
        }

        private void OnDisable()
        {
            Monitor = null; // ???????
            EditorApplication.update -= OnUpdate;
            ui.Disable();
        }

        private void OnDestroy()
        {
            ui.Destroy();
        }

        private void OnGUI()
        {
            ui.OnGUI();
        }

        void OnUpdate()
        {
            if (EditorApplication.isPlaying == false)
                return;

            if ((Time.frameCount % 30) == 0)
                Repaint();
        }

        //=========================================================================================
        void Init()
        {
            this.titleContent.text = "Cloth Monitor";

            ui.Init(this);
        }
    }
}
