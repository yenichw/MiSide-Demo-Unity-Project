

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum MemoryType { MonoBehaviour, ScriptableObject, Component, Other };
    public enum CompareResult { New, Removed }

    public class MemoryInstanceType : IComparable<MemoryInstanceType>
    {
        public FastSortedHashSet<int, MemoryObject> instances = new FastSortedHashSet<int, MemoryObject>(128);
        public bool isSorted = false;
        public Type type;
        public MemoryType memoryType;

        public MemoryInstanceType(Type type)
        {
            this.type = type;

            if (type.IsSubclassOf(typeof(MonoBehaviour))) memoryType = MemoryType.MonoBehaviour;
            else if (type.IsSubclassOf(typeof(Component))) memoryType = MemoryType.Component;
            else if (type.IsSubclassOf(typeof(ScriptableObject))) memoryType = MemoryType.ScriptableObject;
            else memoryType = MemoryType.Other;
        }

        public void Reset()
        {
            instances.Clear();
            isSorted = false;
        }

        public int CompareTo(MemoryInstanceType other)
        {
            int count = instances.list.Count;
            int otherCount = other.instances.list.Count;

            if (count > otherCount) return -1;
            else if (count < otherCount) return 1;
            else return 0;
        }

        public void Sort()
        {
            instances.Sort();
            isSorted = true;
        }
    }

    public class MemoryObject : IComparable<MemoryObject>
    {
        public int instanceId;
        public HideFlags hideFlags;
        public bool isPrefab;
        public string name;
        public string nameAndTime;
        public CompareResult compareResult;

        public MemoryObject(string name, int instanceId, HideFlags hideFlags, bool isPrefab, CompareResult compareResult = CompareResult.New)
        {
            this.compareResult = compareResult;
            this.instanceId = instanceId;
            this.hideFlags = hideFlags;
            this.isPrefab = isPrefab;
            this.name = name;
            nameAndTime = name + Time.realtimeSinceStartup;
        }

        public MemoryObject(MemoryObject memoryObject, CompareResult compareResult = CompareResult.New)
        {
            this.compareResult = compareResult;
            instanceId = memoryObject.instanceId;
            hideFlags = memoryObject.hideFlags;
            isPrefab = memoryObject.isPrefab;
            name = memoryObject.name;
            nameAndTime = memoryObject.nameAndTime;
        }

        public int CompareTo(MemoryObject other)
        {
            return string.Compare(nameAndTime, other.nameAndTime);
        }
    }

    public class MemorySnapshot
    {
        public FastSortedDictionary<string, MemoryInstanceType> memoryTypesLookup = new FastSortedDictionary<string, MemoryInstanceType>(1024);
        public float tStamp;
        public GUIChangeBool selected = new GUIChangeBool(false);
        public CompareResult compareResult;
        public bool hasCleanedDifCompare;

        public void Reset()
        {
            memoryTypesLookup.Clear();
            hasCleanedDifCompare = false;
        }

        public void ScanMemory(CompareMode compareMode)
        {
            tStamp = Time.realtimeSinceStartup;

            var objects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();

            for (int i = 0; i < objects.Length; i++)
            {
                UnityEngine.Object obj = objects[i];
                Type type = obj.GetType();
                string typeName = type.Name;

                MemoryInstanceType memoryType;
                if (!memoryTypesLookup.lookup.TryGetValue(typeName, out memoryType))
                {
                    memoryType = new MemoryInstanceType(type);
                    memoryTypesLookup.Add(typeName, memoryType);
                }
                int instanceId = obj.GetInstanceID();
                memoryType.instances.Add(instanceId, new MemoryObject(obj.name, instanceId, obj.hideFlags, Helper.IsPrefab(obj)));
            }

            memoryTypesLookup.Sort(compareMode);
        }

        public void CleanDifSnapshot(CompareMode compareMode)
        {
            if (hasCleanedDifCompare) return;

            hasCleanedDifCompare = true;
            var memoryTypeList = memoryTypesLookup.list;

            for (int i = 0; i < memoryTypeList.Count; i++)
            {
                MemoryInstanceType memoryInstanceType = memoryTypeList.items[i].value;

                var instanceList = memoryInstanceType.instances.list;

                for (int j = 0; j < instanceList.Count; j++)
                {
                    MemoryObject memoryObject = instanceList.items[j];
                    CompareResult sCompareResult = memoryObject.compareResult;

                    for (int k = j + 1; k < instanceList.Count; k++)
                    {
                        MemoryObject comMemoryObject = instanceList.items[k];

                        if (comMemoryObject.compareResult == sCompareResult) continue;

                        if (comMemoryObject.name == memoryObject.name)
                        {
                            instanceList.RemoveAt(k);
                            instanceList.RemoveAt(j--);
                            break;
                        }
                    }
                }

                if (instanceList.Count == 0) memoryTypeList.RemoveAt(i--);
            }

            memoryTypesLookup.Sort(compareMode);
        }

        public static void CompareMemorySnapshots(MemorySnapshot oldSnapshot, MemorySnapshot newSnapshot, MemorySnapshot difSnapshot, CompareMode compareMode)
        {
            difSnapshot.Reset();

            CompareMemorySnapshots(oldSnapshot, newSnapshot, difSnapshot, CompareResult.Removed);
            CompareMemorySnapshots(newSnapshot, oldSnapshot, difSnapshot, CompareResult.New);

            difSnapshot.memoryTypesLookup.Sort(compareMode);
        }

        public static void CompareMemorySnapshots(MemorySnapshot s, MemorySnapshot d, MemorySnapshot difSnapshot, CompareResult compareResult)
        {
            var sMemoryTypesLookup = s.memoryTypesLookup;
            var dMemoryTypesLookup = d.memoryTypesLookup;

            var sList = sMemoryTypesLookup.list;

            // Compare s with d 
            for (int i = 0; i < sList.Count; i++)
            {
                ItemHolder<string, MemoryInstanceType> item = sList.items[i];
                MemoryInstanceType sMemoryInstanceType = item.value;
                MemoryInstanceType dMemoryInstanceType;
                MemoryInstanceType difMemoryInstanceType = null;
                string typeName = item.key;

                bool allSame = true;
                if (dMemoryTypesLookup.lookup.TryGetValue(typeName, out dMemoryInstanceType))
                {
                    // d Snapshot contains instance of item Type (could be all the same)
                    FastSortedHashSet<int, MemoryObject> sInstances = sMemoryInstanceType.instances;
                    HashSet<int> dInstancesLookup = dMemoryInstanceType.instances.lookup;

                    FastList<MemoryObject> sInstanceList = sInstances.list;

                    for (int j = 0; j < sInstanceList.Count; j++)
                    {
                        MemoryObject sMemoryObject = sInstanceList.items[j];

                        if (dInstancesLookup.Contains(sMemoryObject.instanceId)) continue;

                        // Add when doesn't contain
                        if (allSame)
                        {
                            allSame = false;
                            if (!difSnapshot.memoryTypesLookup.lookup.TryGetValue(typeName, out difMemoryInstanceType))
                            {
                                difMemoryInstanceType = new MemoryInstanceType(sMemoryInstanceType.type);
                                difSnapshot.memoryTypesLookup.Add(typeName, difMemoryInstanceType);
                                // DrawInfo info = GetObjectDrawInfo(difSnapshot.memoryLookup, typeName, true);
                            }
                        }

                        difMemoryInstanceType.instances.Add(sMemoryObject.instanceId, new MemoryObject(sMemoryObject, compareResult));
                    }
                }
                else
                {
                    // Add all instances 
                    difMemoryInstanceType = new MemoryInstanceType(sMemoryInstanceType.type);
                    difSnapshot.memoryTypesLookup.Add(typeName, difMemoryInstanceType);
                    // DrawInfo info = GetObjectDrawInfo(difSnapshot.memoryLookup, typeName, true);
                    var sInstanceList = sMemoryInstanceType.instances.list;

                    for (int j = 0; j < sInstanceList.Count; j++)
                    {
                        difMemoryInstanceType.instances.Add(sInstanceList.items[j].instanceId, new MemoryObject(sInstanceList.items[j], compareResult));
                    }
                }
            }
        }
    }
}