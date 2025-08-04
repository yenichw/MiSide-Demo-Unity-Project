// based on the original game.Yen Chezky(yenichw)
using System;

namespace UnityEditor.Timeline
{
    interface ISelectable : ILayerable
    {
        void Select();
        bool IsSelected();
        void Deselect();
        bool CanSelect(UnityEngine.Event evt);
    }
}
