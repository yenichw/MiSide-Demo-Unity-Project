// based on the original game.Yen Chezky(yenichw)
using System;
using UnityEngine;
using UnityEditor.Rendering.Universal.Path2D.GUIFramework;

namespace UnityEditor.Rendering.Universal.Path2D
{
    internal interface IEditablePathView
    {
        IEditablePathController controller { get; set; }
        void Install(GUISystem guiSystem);
        void Uninstall(GUISystem guiSystem);
    }
}
