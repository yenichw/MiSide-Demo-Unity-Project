/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InfinityCode.UltimateEditorEnhancer.JSON
{
    /// <summary>
    /// The base class of JSON elements.
    /// </summary>
    public abstract class JsonItem : IEnumerable<JsonItem>
    {
        /// <summary>
        /// Get the element by index
        /// </summary>
        /// <param name="index">Index of element</param>
        /// <returns>Element</returns>
        public abstract JsonItem this[int index] { get; }

        /// <summary>
        /// Get the element by key.\n
        /// Supports XPath like selectors:\n
        /// ["key"] - get element by key.\n
        /// ["key1/key2"] - get element key2, which is a child of the element key1.\n
        /// ["key/N"] - where N is number. Get array element by index N, which is a child of the element key1.\n
        /// ["key/*"] - get all array elements, which is a child of the element key1.\n
        /// ["//key"] - get all elements with the key on the first or the deeper levels of the current element. \n
        /// </summary>
        /// <param name="key">Element key</param>
        /// <returns>Element</returns>
        public abstract JsonItem this[string key] { get; }

        /// <summary>
        /// Serializes the object and adds to the current json node.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Current json node</returns>
        public virtual JsonItem AppendObject(object obj)
        {
            throw new Exception("AppendObject is only allowed for JsonObject.");
        }

        /// <summary>
        /// Returns the value of the child element, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <param name="childName">Child element key</param>
        /// <returns>Value</returns>
        public T ChildValue<T>(string childName)
        {
            JsonItem el = this[childName];
            if (el == null) return default(T);
            return el.Value<T>();
        }

        /// <summary>
        /// Deserializes current element
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Object</returns>
        public T Deserialize<T>(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            return (T) Deserialize(typeof(T), bindingFlags);
        }

        /// <summary>
        /// Deserializes current element
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Object</returns>
        public abstract object Deserialize(Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Get all elements with the key on the first or the deeper levels of the current element.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Elements</returns>
        public abstract JsonItem GetAll(string key);

        /// <summary>
        /// Converts the current and the child elements to JSON string.
        /// </summary>
        /// <param name="b">StringBuilder instance</param>
        public abstract void ToJSON(StringBuilder b);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<JsonItem> GetEnumerator()
        {
            return null;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            ToJSON(b);
            return b.ToString();
        }

        /// <summary>
        /// Returns the value of the element, converted to the specified type.
        /// </summary>
        /// <param name="type">Type of variable</param>
        /// <returns>Value</returns>
        public abstract object Value(Type type);

        /// <summary>
        /// Returns the value of the element, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <returns>Value</returns>
        public virtual T Value<T>()
        {
            return (T) Value(typeof(T));
        }

        /// <summary>
        /// Returns the value of the child element, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <param name="childName">Child element key</param>
        /// <returns>Value</returns>
        public T Value<T>(string childName)
        {
            return ChildValue<T>(childName);
        }

        /// <summary>
        /// Returns the value of the element, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <returns>Value</returns>
        public T V<T>()
        {
            return Value<T>();
        }

        /// <summary>
        /// Returns the value of the child element, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">Type of variable</typeparam>
        /// <param name="childName">Child element key</param>
        /// <returns>Value</returns>
        public T V<T>(string childName)
        {
            return ChildValue<T>(childName);
        }
    }
}