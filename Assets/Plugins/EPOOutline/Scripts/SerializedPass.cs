using System.Collections.Generic;
using UnityEngine;

namespace EPOOutline
{
    /// <summary>
    /// Holds an information about how pass should be rendered.
    /// </summary>
    [System.Serializable]
    public class SerializedPass : ISerializationCallbackReceiver
    {
        /// <summary>
        /// The type of shader property.
        /// </summary>
        public enum PropertyType
        {
            /// <summary>
            /// Color.
            /// </summary>
            Color = 0,
            /// <summary>
            /// Vector.
            /// </summary>
            Vector = 1,
            /// <summary>
            /// Float.
            /// </summary>
            Float = 2,
            /// <summary>
            /// Float but limited with a range.
            /// </summary>
            Range = 3,
            /// <summary>
            /// Texture.
            /// </summary>
            TexEnv = 4
        }

        [System.Serializable]
        private class SerializedPropertyKeyValuePair
        {
            [SerializeField]
            public string PropertyName;

            [SerializeField]
            public SerializedPassProperty Property;
        }

        [System.Serializable]
        private class SerializedPassProperty
        {
#pragma warning disable CS0649
            [SerializeField]
            public Color ColorValue;

            [SerializeField]
            public float FloatValue;

            [SerializeField]
            public Vector4 VectorValue;

            [SerializeField]
            public PropertyType PropertyType;
#pragma warning restore CS0649
        }

        [SerializeField]
        private Shader shader;

        /// <summary>
        /// The <see cref="UnityEngine.Shader"/> that will be used for rendering the pass.
        /// </summary>
        public Shader Shader
        {
            get => shader;

            set
            {
                propertiesIsDirty = true;
                shader = value;
            }
        }

        [SerializeField]
        private List<SerializedPropertyKeyValuePair> serializedProperties = new List<SerializedPropertyKeyValuePair>();

        private Dictionary<int,     SerializedPassProperty> propertiesById      = new Dictionary<int, SerializedPassProperty>();
        private Dictionary<string,  SerializedPassProperty> propertiesByName    = new Dictionary<string, SerializedPassProperty>();

        private Material material;

        private bool propertiesIsDirty = false;

        internal Material Material
        {
            get
            {
                if (shader == null)
                    return null;

                if (material == null || material.shader != shader)
                {
                    if (material != null)
                        GameObject.DestroyImmediate(material);

                    material = new Material(shader);
                }

                if (!propertiesIsDirty)
                    return material;

                foreach (var property in propertiesById)
                {
                    switch (property.Value.PropertyType)
                    {
                        case PropertyType.Color:
                            material.SetColor(property.Key, property.Value.ColorValue);
                            break;
                        case PropertyType.Vector:
                            material.SetVector(property.Key, property.Value.VectorValue);
                            break;
                        case PropertyType.Float:
                            material.SetFloat(property.Key, property.Value.FloatValue);
                            break;
                        case PropertyType.Range:
                            material.SetFloat(property.Key, property.Value.FloatValue);
                            break;
                        case PropertyType.TexEnv:
                            break;
                    }
                }

                propertiesIsDirty = false;

                return material;
            }
        }

        /// <summary>
        /// Checks if the pass has the property.
        /// </summary>
        /// <param name="name">The property name to check</param>
        /// <returns>Returns true if it has the property.</returns>
        public bool HasProperty(string name)
        {
            return propertiesByName.ContainsKey(name);
        }

        /// <summary>
        /// Checks if the pass has the property.
        /// </summary>
        /// <param name="hash">The property hash to check</param>
        /// <returns>Returns true if it has the property.</returns>
        public bool HasProperty(int hash)
        {
            return propertiesById.ContainsKey(hash);
        }

        /// <summary>
        /// Returns a vector from the pass.
        /// </summary>
        /// <param name="name">The name of the vector.</param>
        /// <returns>The vector result.</returns>
        public Vector4 GetVector(string name)
        {
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                Debug.LogError("The property " + name + " doesn't exist");
                return Vector4.zero;
            }

            if (result.PropertyType == PropertyType.Vector)
                return result.VectorValue;
            
            Debug.LogError("The property " + name + " is not a vector property");
            return Vector4.zero;
        }

        /// <summary>
        /// Returns a vector from the pass.
        /// </summary>
        /// <param name="hash">The hash of the vector.</param>
        /// <returns>The vector result.</returns>
        public Vector4 GetVector(int hash)
        {
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogError("The property " + hash + " doesn't exist");
                return Vector4.zero;
            }

            if (result.PropertyType == PropertyType.Vector)
                return result.VectorValue;
            
            Debug.LogError("The property " + hash + " is not a vector property");
            return Vector4.zero;
        }

        /// <summary>
        /// Sets vector to the pass.
        /// </summary>
        /// <param name="name">The name of the vector.</param>
        /// <param name="value">The vector value to be set.</param>
        public void SetVector(string name, Vector4 value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                result = new SerializedPassProperty();
                result.PropertyType = PropertyType.Vector;
                propertiesByName.Add(name, result);
                propertiesById.Add(Shader.PropertyToID(name), result);
            }

            if (result.PropertyType != PropertyType.Vector)
            {
                Debug.LogError("The property " + name + " is not a vector property");
                return;
            }

            result.VectorValue = value;
        }

        /// <summary>
        /// Sets vector to the pass.
        /// </summary>
        /// <param name="hash">The hash of the vector.</param>
        /// <param name="value">The vector value to be set.</param>
        public void SetVector(int hash, Vector4 value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogWarning("The property " + hash + " doesn't exist. Use string overload to create one.");
                return;
            }

            if (result.PropertyType != PropertyType.Vector)
            {
                Debug.LogError("The property " + hash + " is not a vector property");
                return;
            }

            result.VectorValue = value;
        }

        /// <summary>
        /// Returns a float from the pass.
        /// </summary>
        /// <param name="name">The name of the float.</param>
        /// <returns>The float value from the pass.</returns>
        public float GetFloat(string name)
        {
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                Debug.LogError("The property " + name + " doesn't exist");
                return 0.0f;
            }

            if (result.PropertyType == PropertyType.Float || result.PropertyType == PropertyType.Range)
                return result.FloatValue;
            
            Debug.LogError("The property " + name + " is not a float property");
            return 0.0f;
        }

        /// <summary>
        /// Returns a float from the pass.
        /// </summary>
        /// <param name="hash">The hash of the float.</param>
        /// <returns>The float value from the pass.</returns>
        public float GetFloat(int hash)
        {
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogError("The property " + hash + " is doesn't exist");
                return 0.0f;
            }

            if (result.PropertyType == PropertyType.Float || result.PropertyType == PropertyType.Range)
                return result.FloatValue;
            
            Debug.LogError("The property " + hash + " is not a float property");
            return 0.0f;
        }

        /// <summary>
        /// Sets a float to the pass.
        /// </summary>
        /// <param name="name">The name of the float.</param>
        /// <param name="value">The value to be set.</param>
        public void SetFloat(string name, float value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                result = new SerializedPassProperty();
                result.PropertyType = PropertyType.Float;
                propertiesByName.Add(name, result);
                propertiesById.Add(Shader.PropertyToID(name), result);
            }

            if (result.PropertyType != PropertyType.Float && result.PropertyType != PropertyType.Range)
            {
                Debug.LogError("The property " + name + " is not a float property");
                return;
            }

            result.FloatValue = value;
        }

        /// <summary>
        /// Sets a float to the pass.
        /// </summary>
        /// <param name="hash">The hash of the float.</param>
        /// <param name="value">The value to be set.</param>
        public void SetFloat(int hash, float value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogError("The property " + hash + " doesn't exist. Use string overload to create one.");
                return;
            }

            if (result.PropertyType != PropertyType.Float)
            {
                Debug.LogError("The property " + hash + " is not a float property");
                return;
            }

            result.FloatValue = value;
        }

        /// <summary>
        /// Returns a color from the pass.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <returns>The color value from the pass.</returns>
        public Color GetColor(string name)
        {
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                Debug.LogError("The property " + name + " doesn't exist");
                return Color.black;
            }

            if (result.PropertyType == PropertyType.Color)
                return result.ColorValue;
            else
            {
                Debug.LogError("The property " + name + " is not a color property");
                return Color.black;
            }
        }
        
        /// <summary>
        /// Returns a color from the pass.
        /// </summary>
        /// <param name="hash">The hash of the color.</param>
        /// <returns>The color value from the pass.</returns>
        public Color GetColor(int hash)
        {
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogError("The property " + hash + " doesn't exist");
                return Color.black;
            }

            if (result.PropertyType == PropertyType.Color)
                return result.ColorValue;
            else
                return Color.black;
        }

        /// <summary>
        /// Sets a color to the pass.
        /// </summary>
        /// <param name="name">The name of the color</param>
        /// <param name="value">The value to be set.</param>
        public void SetColor(string name, Color value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesByName.TryGetValue(name, out result))
            {
                result = new SerializedPassProperty();
                result.PropertyType = PropertyType.Color;
                propertiesByName.Add(name, result);
                propertiesById.Add(Shader.PropertyToID(name), result);
            }

            if (result.PropertyType != PropertyType.Color)
            {
                Debug.LogError("The property " + name + " is not a color property.");
                return;
            }

            result.ColorValue = value;
        }

        /// <summary>
        /// Sets a color to the pass.
        /// </summary>
        /// <param name="hash">The hash of the color</param>
        /// <param name="value">The value to be set.</param>
        public void SetColor(int hash, Color value)
        {
            propertiesIsDirty = true;
            SerializedPassProperty result = null;
            if (!propertiesById.TryGetValue(hash, out result))
            {
                Debug.LogError("The property " + hash + " doesn't exist. Use string overload to create one.");
                return;
            }

            if (result.PropertyType != PropertyType.Color)
            {
                Debug.LogError("The property " + hash + " is not a color property");
                return;
            }

            result.ColorValue = value;
        }

        public void OnBeforeSerialize()
        {
            serializedProperties.Clear();

            foreach (var property in propertiesByName)
            {
                var pair = new SerializedPropertyKeyValuePair();
                pair.Property = property.Value;
                pair.PropertyName = property.Key;

                serializedProperties.Add(pair);
            }
        }

        public void OnAfterDeserialize()
        {
            propertiesIsDirty = true;
            propertiesById.Clear();
            propertiesByName.Clear();
            foreach (var serialized in serializedProperties)
            {
                if (propertiesByName.ContainsKey(serialized.PropertyName))
                    continue;

                propertiesById.Add(Shader.PropertyToID(serialized.PropertyName), serialized.Property);
                propertiesByName.Add(serialized.PropertyName, serialized.Property);
            }
        }
    }
}