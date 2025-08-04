// based on the original game.Yen Chezky(yenichw)
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    interface IRowGUI
    {
        TrackAsset asset { get; }
        Rect boundingRect { get; }
        bool locked { get; }
        bool showMarkers { get; }
        bool muted { get; }

        Rect ToWindowSpace(Rect treeViewRect);
    }
}
