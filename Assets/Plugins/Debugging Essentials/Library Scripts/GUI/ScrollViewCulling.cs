using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum DrawResult { DontDraw, Draw, DrawHeader, DrawHeaderAndRemoveLastHeader }

    public class CullGroup
    {
        static FastList<CullList> cullLists = new FastList<CullList>();
        public CullList cullList;
        double startHeight = 0;

        public static void ResetStatic()
        {
            cullLists.Clear();
        }

        public CullGroup(int capacity)
        {
            cullList = new CullList(capacity);
        }

        public void CalcDraw(bool reset, FastList<CullList> _cullLists)
        {
            if (reset)
            {
                startHeight = 0;
                cullList.cullItems.Clear();
            }

            cullLists.Clear();

            for (int i = 0; i < _cullLists.Count; i++)
            {
                CullList cullList = _cullLists.items[i];
                if (reset) cullList.currentCalcCullItem = 0;
                if (cullList.currentCalcCullItem < cullList.cullItems.Count) cullLists.Add(cullList);
            }

            // Debug.Log("CullLists " + cullLists.Count + " reset " + reset);

            while (cullLists.Count > 0)
            {
                int idMax = int.MaxValue;
                int nextItemIndex = -1;

                for (int i = 0; i < cullLists.Count; i++)
                {
                    CullList cullList = cullLists.items[i];

                    int id = cullList.cullItems.items[cullList.currentCalcCullItem].id;
                    if (id < idMax)
                    {
                        idMax = id;
                        nextItemIndex = i;
                    }
                }

                if (!cullLists.items[nextItemIndex].Draw(cullList, ref startHeight))
                {
                    cullLists.RemoveAt(nextItemIndex);
                }
            }

            if (reset && cullList.cullItems.Count > 0 && (cullList.cullItems.items[cullList.cullItems.Count - 1]).isHeader)
            {
                cullList.cullItems.RemoveLast();
            }
        }
    }

    public class CullList
    {
        public SortedFastList<CullItem> cullItems;
        public int startIndex;
        public int endIndex;
        public int currentCalcCullItem;
        public GUIChangeBool show = new GUIChangeBool(true);
        public float tStamp = 0;
        public DrawResult lastDrawResult;
        public int lastAddedIndex;

        public CullList(int capacity)
        {
            cullItems = new SortedFastList<CullItem>(capacity);
        }

        public bool Draw(CullList currentCullItems, ref double startHeight)
        {
            CullItem cullItem = cullItems.items[currentCalcCullItem++];

            DrawResult drawResult = cullItem.DoDraw();

            if (drawResult != DrawResult.DontDraw)
            {
                SortedFastList<CullItem> cullItems = currentCullItems.cullItems;

                cullItem.Draw(ref startHeight);
                cullItems.Add(cullItem);

                if (drawResult == DrawResult.DrawHeaderAndRemoveLastHeader)
                {
                    double height = cullItems.items[lastAddedIndex].endHeight - cullItems.items[lastAddedIndex].startHeight; ;
                    startHeight -= height;
                    cullItems.RemoveAt(lastAddedIndex);

                    for (int i = lastAddedIndex; i < cullItems.Count; i++)
                    {
                        cullItems.items[i].startHeight -= height;
                        cullItems.items[i].endHeight -= height;
                    }
                }

                if (drawResult == DrawResult.DrawHeader || drawResult == DrawResult.DrawHeaderAndRemoveLastHeader) lastAddedIndex = cullItems.Count - 1;
                currentCullItems.lastDrawResult = drawResult;
            }

            if (currentCalcCullItem >= cullItems.Count) return false;
            else return true;
        }

        public void Cull(Double2 heights)
        {
            if (cullItems.Count == 0) return;

            startIndex = Helper.BinarySearch(cullItems.items, cullItems.Count, heights.x);
            if (cullItems.items[startIndex].endHeight <= heights.x) startIndex++;

            if (startIndex >= cullItems.Count) startIndex = cullItems.Count - 1;

            endIndex = Helper.BinarySearch(cullItems.items, cullItems.Count, heights.y);

            if (endIndex + 1 < cullItems.Count)
            {
                if (cullItems.items[endIndex + 1].startHeight < heights.y) endIndex++;
            }
        }
    }

    public class CullItem : IComparable<double>
    {
        public bool isHeader;
        public int id;

        public double startHeight;
        public double endHeight;

        public CullItem(int id)
        {
            this.id = id;
        }

        public void Draw(ref double startHeight)
        {
            this.startHeight = startHeight;
            startHeight += CalcHeight();
            endHeight = startHeight;
        }

        public int CompareTo(double compareValue)
        {
            if (endHeight > compareValue) return 1;
            else if (endHeight < compareValue) return -1;
            else return 0;
        }

        public virtual DrawResult DoDraw()
        {
            return DrawResult.Draw;
        }

        public virtual float CalcHeight()
        {
            return -1;
        }
    }

    public struct Double2
    {
        public double x;
        public double y;

        public Double2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}