// based on the original game.Yen Chezky(yenichw)
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    interface ICurvesOwnerInspectorWrapper
    {
        ICurvesOwner curvesOwner { get; }
        SerializedObject serializedPlayableAsset { get; }
        int lastCurveVersion { get; set; }
        double lastEvalTime { get; set; }

        double ToLocalTime(double time);
    }
}
