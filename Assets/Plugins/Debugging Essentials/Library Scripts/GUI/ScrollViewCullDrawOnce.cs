using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    public class ScrollViewCullDrawOnce
    {
        public bool active = true;
        public bool recalc;

        FastList<float> elements1 = new FastList<float>();
        FastList<float> elements2 = new FastList<float>();

        Vector2 scrollView;

        bool eventLayout;
        bool eventRepaint;
        bool hasCalc;
        bool calcHeights;

        int startElementCull = 0;
        float spaceEnd;

        float scrollViewStartHeight;
        float scrollViewEndHeight;

        public void ResetAndRecalc()
        {
            recalc = true;
            scrollView.y = 0;
        }

        public void BeginScrollView(float heightOffset = 0)
        {
            Event currentEvent = Event.current;

            eventLayout = (currentEvent.type == EventType.Layout);
            eventRepaint = (currentEvent.type == EventType.Repaint);

            if (recalc && eventLayout)
            {
                hasCalc = false;
                recalc = false;
                elements1.FastClear();
                elements2.FastClear();
            }

            startElementCull = 0;
            calcHeights = (!hasCalc && eventRepaint);
            spaceEnd = 0;

            GUILayout.Space(0);
            float startHeight = GUILayoutUtility.GetLastRect().y;

            if (calcHeights) scrollViewStartHeight = startHeight;

            scrollView = GUILayout.BeginScrollView(scrollView);

            if (hasCalc && active)
            {
                int index = Helper.BinarySearch(elements2.items, elements2.Count, scrollView.y);
                float spaceStart = 0;

                float height = elements2.items[index];

                if (height < scrollView.y) spaceStart = height;
                else if (index > 0)
                {
                    height = elements2.items[index - 1];
                    if (height < scrollView.y) spaceStart = height;
                }

                if (spaceStart > 0) GUILayout.Space(spaceStart + 4);
            }
        }

        public void EndScrollView()
        {
            if (hasCalc && active)
            {
                float space = spaceEnd != 0 ? elements2.items[elements2.Count - 1] - spaceEnd : 0;
                GUILayout.Space(space);
            }

            GUILayout.EndScrollView();

            if (calcHeights) hasCalc = true;

            GUILayout.Space(0);
            float endHeight = GUILayoutUtility.GetLastRect().y;

            if (calcHeights) scrollViewEndHeight = endHeight;
        }

        public bool StartCull()
        {
            if (!(hasCalc && active)) return true;

            if (startElementCull >= elements1.items.Length) return true;

            float yMin = elements1.items[startElementCull];
            float yMax = elements2.items[startElementCull++];

            if (yMax < scrollView.y && active) return false;
            else if (yMin > (scrollViewEndHeight - scrollViewStartHeight) + scrollView.y && active)
            {
                if (spaceEnd == 0) spaceEnd = yMin;
                return false;
            }

            return true;
        }

        public void EndCull()
        {
            if (!calcHeights) return;
            Rect rect = GUILayoutUtility.GetLastRect();
            elements1.Add(rect.yMin);
            elements2.Add(rect.yMax);
        }
    }
}