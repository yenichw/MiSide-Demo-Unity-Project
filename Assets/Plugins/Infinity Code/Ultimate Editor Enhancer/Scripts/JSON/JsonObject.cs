/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.JSON
{
    /// <summary>
    /// The wrapper for JSON dictonary.
    /// </summary>
    public class JsonObject : JsonItem
    {
        private Dictionary<string, JsonItem> _table;

        /// <summary>
        /// Dictionary of items
        /// </summary>
        public Dictionary<string, JsonItem> table
        {
            get { return _table; }
        }

        public override JsonItem this[string key]
        {
            get { return Get(key); }
        }

        public override JsonItem this[int index]
        {
            get
            {
                if (index < 0) return null;

                int i = 0;
                foreach (KeyValuePair<string, JsonItem> pair in _table)
                {
                    if (i == index) return pair.Value;
                    i++;
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonObject()
        {
            _table = new Dictionary<string, JsonItem>();
        }

        /// <summary>
        /// Adds element to the dictionary
        /// </summary>
        /// <param name="name">Key</param>
        /// <param name="value">Value</param>
        public void Add(string name, JsonItem value)
        {
            _table[name] = value;
        }

        public void Add(string name, object value)
        {
            if (value is string || value is bool || value is int || value is long || value is short || value is float || value is double) _table[name] = new JsonValue(value);
            else if (value is UnityEngine.Object)
            {
                _table[name] = new JsonValue((value as UnityEngine.Object).GetInstanceID());
            }
            else _table[name] = Json.Serialize(value, BindingFlags.Instance | BindingFlags.Public);
        }

        public void Add(string name, object value, JsonValue.ValueType valueType)
        {
            _table[name] = new JsonValue(value, valueType);
        }

        public override JsonItem AppendObject(object obj)
        {
            Combine(Json.Serialize(obj));
            return this;
        }

        /// <summary>
        /// Combines two JSON Object.
        /// </summary>
        /// <param name="other">Other JSON Object</param>
        /// <param name="overwriteExistingValues">Overwrite the existing values?</param>
        public void Combine(JsonItem other, bool overwriteExistingValues = false)
        {
            JsonObject otherObj = other as JsonObject;
            if (otherObj == null) throw new Exception("Only JsonObject is allowed to be combined.");
            Dictionary<string, JsonItem> otherDict = otherObj.table;
            foreach (KeyValuePair<string, JsonItem> pair in otherDict)
            {
                if (overwriteExistingValues || !_table.ContainsKey(pair.Key)) _table[pair.Key] = pair.Value;
            }
        }

        public bool Contains(string key)
        {
            return _table.ContainsKey(key);
        }

        public JsonArray CreateArray(string name)
        {
            JsonArray array = new JsonArray();
            Add(name, array);
            return array;
        }

        public JsonObject CreateObject(string name)
        {
            JsonObject obj = new JsonObject();
            Add(name, obj);
            return obj;
        }

        public override object Deserialize(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            IEnumerable<MemberInfo> members = Reflection.GetMembers(type, bindingFlags);
            return Deserialize(type, members, bindingFlags);
        }

        /// <summary>
        /// Deserializes current element
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="members">Members of variable</param>
        /// <returns>Object</returns>
        public object Deserialize(Type type, IEnumerable<MemberInfo> members, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            object v = Activator.CreateInstance(type);
            DeserializeObject(v, members, bindingFlags);
            return v;
        }

        public void DeserializeObject(object obj, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            IEnumerable<MemberInfo> members = Reflection.GetMembers(obj.GetType(), bindingFlags);
            DeserializeObject(obj, members);
        }

        public void DeserializeObject(object obj, IEnumerable<MemberInfo> members, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            foreach (MemberInfo member in members)
            {
#if !NETFX_CORE
                MemberTypes memberType = member.MemberType;
                if (memberType != MemberTypes.Field && memberType != MemberTypes.Property) continue;
#else
                MemberTypes memberType;
                if (member is PropertyInfo) memberType = MemberTypes.Property;
                else if (member is FieldInfo) memberType = MemberTypes.Field;
                else continue;
#endif

                if (memberType == MemberTypes.Property && !((PropertyInfo) member).CanWrite) continue;
                JsonItem item;

#if !NETFX_CORE
                object[] attributes = member.GetCustomAttributes(typeof(Json.AliasAttribute), true);
                Json.AliasAttribute alias = attributes.Length > 0 ? attributes[0] as Json.AliasAttribute : null;
#else
                IEnumerable<Attribute> attributes = member.GetCustomAttributes(typeof(Json.AliasAttribute), true);
                Json.AliasAttribute alias = null;
                foreach (Attribute a in attributes)
                {
                    alias = a as Json.AliasAttribute;
                    break;
                }
#endif
                if (alias == null || !alias.ignoreFieldName)
                {
                    if (_table.TryGetValue(member.Name, out item))
                    {
                        Type t = memberType == MemberTypes.Field ? ((FieldInfo) member).FieldType : ((PropertyInfo) member).PropertyType;
                        if (memberType == MemberTypes.Field) ((FieldInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags));
                        else ((PropertyInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags), null);
                        continue;
                    }
                }

                if (alias != null)
                {
                    for (int j = 0; j < alias.aliases.Length; j++)
                    {
                        if (_table.TryGetValue(alias.aliases[j], out item))
                        {
                            Type t = memberType == MemberTypes.Field ? ((FieldInfo) member).FieldType : ((PropertyInfo) member).PropertyType;
                            if (memberType == MemberTypes.Field) ((FieldInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags));
                            else ((PropertyInfo) member).SetValue(obj, item.Deserialize(t, bindingFlags), null);
                            break;
                        }
                    }
                }
            }
        }

        private JsonItem Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (key.Length > 2 && key[0] == '/' && key[1] == '/')
            {
                string k = key.Substring(2);
                if (string.IsNullOrEmpty(k) || k.StartsWith("//")) return null;
                return GetAll(k);
            }

            return GetThis(key);
        }

        private JsonItem GetThis(string key)
        {
            JsonItem item;
            int index = -1;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '/')
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                string k = key.Substring(0, index);
                if (!string.IsNullOrEmpty(k))
                {
                    if (_table.TryGetValue(k, out item))
                    {
                        string nextPart = key.Substring(index + 1);
                        return item[nextPart];
                    }
                }

                return null;
            }

            if (_table.TryGetValue(key, out item)) return item;
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

            var enumerator = _table.GetEnumerator();
            while (enumerator.MoveNext())
            {
                item = enumerator.Current.Value;
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
            return _table.Values.GetEnumerator();
        }

        /// <summary>
        /// Parse a string that contains JSON dictonary
        /// </summary>
        /// <param name="json">String that contains JSON dictonary</param>
        /// <returns>Instance</returns>
        public static JsonObject ParseObject(string json)
        {
            return Json.Parse(json) as JsonObject;
        }

        public JsonItem Remove(string key)
        {
            JsonItem item;
            if (_table.TryGetValue(key, out item))
            {
                _table.Remove(key);
                return item;
            }

            return null;
        }

        public override void ToJSON(StringBuilder b)
        {
            b.Append("{");
            bool hasChildren = false;
            foreach (KeyValuePair<string, JsonItem> pair in _table)
            {
                b.Append("\"").Append(pair.Key).Append("\"").Append(":");
                pair.Value.ToJSON(b);
                b.Append(",");
                hasChildren = true;
            }

            if (hasChildren) b.Remove(b.Length - 1, 1);
            b.Append("}");
        }

        public override object Value(Type type)
        {
            if (Reflection.IsValueType(type))
            {
                object obj = Activator.CreateInstance(type);
                DeserializeObject(obj);
                return obj;
            }
            return Deserialize(type);
        }
    }
}