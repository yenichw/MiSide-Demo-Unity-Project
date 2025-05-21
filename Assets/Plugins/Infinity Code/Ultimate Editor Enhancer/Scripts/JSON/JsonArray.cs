/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InfinityCode.UltimateEditorEnhancer.JSON
{
    /// <summary>
    /// The wrapper for an array of JSON elements.
    /// </summary>
    public class JsonArray : JsonItem
    {
        private List<JsonItem> _items;
        private int _count;

        public List<JsonItem> items
        {
            get { return _items; }
        }

        /// <summary>
        /// Count elements
        /// </summary>
        public int count
        {
            get { return _count; }
        }

        public override JsonItem this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) return null;
                return _items[index];
            }
        }


        public override JsonItem this[string key]
        {
            get { return Get(key); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonArray()
        {
            _items = new List<JsonItem>();
        }

        /// <summary>
        /// Adds an element to the array.
        /// </summary>
        /// <param name="item">Element</param>
        public void Add(JsonItem item)
        {
            _items.Add(item);
            _count++;
        }

        /// <summary>
        /// Adds an elements to the array.
        /// </summary>
        /// <param name="collection">Array of elements</param>
        public void AddRange(JsonArray collection)
        {
            if (collection == null) return;
            _items.AddRange(collection._items);
            _count += collection._count;
        }

        public void AddRange(JsonItem collection)
        {
            AddRange(collection as JsonArray);
        }

        public JsonObject CreateObject()
        {
            JsonObject obj = new JsonObject();
            Add(obj);
            return obj;
        }

        public override object Deserialize(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (_count == 0) return null;

            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                Array v = Array.CreateInstance(elementType, _count);
                if (_items[0] is JsonObject)
                {
                    IEnumerable<MemberInfo> members = Reflection.GetMembers(elementType, bindingFlags);
                    for (int i = 0; i < _count; i++)
                    {
                        JsonItem child = _items[i];
                        object item = (child as JsonObject).Deserialize(elementType, members, bindingFlags);
                        v.SetValue(item, i);
                    }
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        JsonItem child = _items[i];
                        object item = child.Deserialize(elementType, bindingFlags);
                        v.SetValue(item, i);
                    }
                }

                return v;
            }

            if (Reflection.IsGenericType(type))
            {
                Type listType = Reflection.GetGenericArguments(type)[0];
                object v = Activator.CreateInstance(type);

                if (_items[0] is JsonObject)
                {
                    IEnumerable<MemberInfo> members = Reflection.GetMembers(listType, BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < _count; i++)
                    {
                        JsonItem child = _items[i];
                        object item = (child as JsonObject).Deserialize(listType, members);
                        try
                        {
                            MethodInfo methodInfo = Reflection.GetMethod(type, "Add");
                            if (methodInfo != null) methodInfo.Invoke(v, new[] { item });
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _count; i++)
                    {
                        JsonItem child = _items[i];
                        object item = child.Deserialize(listType);
                        try
                        {
                            MethodInfo methodInfo = Reflection.GetMethod(type, "Add");
                            if (methodInfo != null) methodInfo.Invoke(v, new[] { item });
                        }
                        catch
                        {
                        }
                    }
                }

                return v;
            }


            return null;
        }

        private JsonItem Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (key.StartsWith("//"))
            {
                string k = key.Substring(2);
                if (string.IsNullOrEmpty(k) || k.StartsWith("//")) return null;
                return GetAll(k);
            }

            return GetThis(key);
        }

        private JsonItem GetThis(string key)
        {
            int kindex;

            if (key.Contains("/"))
            {
                int index = key.IndexOf("/");
                string k = key.Substring(0, index);
                string nextPart = key.Substring(index + 1);

                if (k == "*")
                {
                    JsonArray arr = new JsonArray();
                    for (int i = 0; i < _count; i++)
                    {
                        JsonItem item = _items[i][nextPart];
                        if (item != null) arr.Add(item);
                    }

                    return arr;
                }

                if (int.TryParse(k, out kindex))
                {
                    if (kindex < 0 || kindex >= _count) return null;
                    JsonItem item = _items[kindex];
                    return item[nextPart];
                }
            }

            if (key == "*") return this;
            if (int.TryParse(key, out kindex)) return this[kindex];
            return null;
        }


        public override JsonItem GetAll(string k)
        {
            JsonItem item = GetThis(k);
            JsonArray arr = null;
            if (item != null)
            {
                arr = new JsonArray();
                arr.Add(item);
            }

            for (int i = 0; i < _count; i++)
            {
                item = _items[i];
                JsonArray subArr = item.GetAll(k) as JsonArray;
                if (subArr != null)
                {
                    if (arr == null) arr = new JsonArray();
                    arr.AddRange(subArr);
                }
            }

            return arr;
        }

        public override IEnumerator<JsonItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Parse a string that contains an array
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Instance</returns>
        public static JsonArray ParseArray(string json)
        {
            return Json.Parse(json) as JsonArray;
        }

        public override void ToJSON(StringBuilder b)
        {
            b.Append("[");
            for (int i = 0; i < _count; i++)
            {
                if (i != 0) b.Append(",");
                _items[i].ToJSON(b);
            }

            b.Append("]");
        }

        public override object Value(Type type)
        {
            if (Reflection.IsValueType(type)) return Activator.CreateInstance(type);
            return null;

        }
    }
}