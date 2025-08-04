// based on the original game.Yen Chezky(yenichw)
namespace UnityEditor.Timeline
{
    enum PlacementValidity
    {
        Valid,
        InvalidContains,
        InvalidIsWithin,
        InvalidStartsInBlend,
        InvalidEndsInBlend,
        InvalidContainsBlend,
        InvalidOverlapWithNonBlendableClip
    }
}
