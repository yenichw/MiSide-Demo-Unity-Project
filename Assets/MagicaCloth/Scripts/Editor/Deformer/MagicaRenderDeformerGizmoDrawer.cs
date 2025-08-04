// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;

namespace MagicaCloth
{
    /// <summary>
    /// MagicaRenderDeformer??????
    /// </summary>
    public class MagicaRenderDeformerGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
        static void DrawGizmo(MagicaRenderDeformer scr, GizmoType gizmoType)
        {
            bool selected = (gizmoType & GizmoType.Selected) != 0 || (ClothMonitorMenu.Monitor != null && ClothMonitorMenu.Monitor.UI.AlwaysDeformerShow);

            if (PointSelector.EditEnable)
                return;
            if (ClothMonitorMenu.Monitor == null)
                return;
            if (ClothMonitorMenu.Monitor.UI.DrawDeformer == false)
                return;

            if (selected == false)
                return;


            // ??????????
            if (scr.VerifyData() != Define.Error.None)
                return;

            // ?????????
            DeformerGizmoDrawer.DrawDeformerGizmo(scr, scr);
        }
    }
}
