using UnityEngine;

namespace DebuggingEssentials
{
    public struct ScrollViewCullData
    {
        public float scrollWindowPosY;
        public float culledSpaceY;
        public float windowHeight;
        public float rectStartScrollY;
        public float updatedScrollViewY;

        public void Start(WindowSettings window)
        {
            scrollWindowPosY = 0;
            culledSpaceY = 0;
            windowHeight = window.rect.height;
            rectStartScrollY = window.rectStartScroll.y;
            updatedScrollViewY = window.updatedScrollView.y;
        }

        public void End()
        {
            if (culledSpaceY > 0) GUILayout.Space(culledSpaceY);
        }
    }
}