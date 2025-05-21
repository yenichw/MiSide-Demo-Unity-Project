using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    public enum WindowType { Hierarchy, Inspector, Console };

    // [CreateAssetMenu(fileName = "Window_Data", menuName = "ScriptableObjects/DebuggingEssentials/Window_Data", order = 1)]
    public class SO_BaseWindow : ScriptableObject
    {
        public Color color = Color.white;
        public float backgroundAlpha = 1;
        public float boxAlpha = 1;
    }

    public class SO_Window : SO_BaseWindow
    {
        public WindowSettings hierarchyWindow;
        public WindowSettings inspectorWindow;

        public bool showTooltip = true;
        public float selectionSphereRadius = 0.2f;

        public IntInputField searchAssemblyPerFrame = new IntInputField(4000);
        public IntInputField searchGameObjectsPerFrame = new IntInputField(250);
        public IntInputField searchInspectorPerFrame = new IntInputField(2000);
        public GUIChangeBool showChildCount = new GUIChangeBool(true);

        public Texture texToggleOn;
        public Texture texArrowFolded;
        public Texture texArrowUnfolded;
        public Texture texDot;
        public Texture texDot2;
        public Texture texScene;
        public Texture texAssembly;
        public Texture texMemory;
        public Texture texHierarchy;
        public Texture texInspector;
        public Texture texGameObject;
        public Texture texSearch;
        public Texture texSettings;
        public Texture texPause;
        public Texture texCamera;
        public Texture texCameraFollow;
        public Texture texHelp;

        public Texture texPrefab;
        public Texture texSave;
        public Texture texLoad;
        public Texture texEdit;

        public Texture texAlphabeticSort;
        public Texture texCountSort;

        public Texture texVisible;
        public Texture texInvisible;

        public Texture texDestroyed;

        public GUISkin skin;
        public GUISkin sourceSkin;

        public ComponentIcons componentIcons;
    }

    [Serializable]
    public struct ComponentIcons
    {
        public Texture gameObjectIcon;
        public Texture transformIcon;
        public Texture cameraIcon;
        public Texture csScriptIcon;

        public Texture audioListenerIcon;
        public Texture audioSourceIcon;

        public Texture rigidbodyIcon;
        public Texture boxColliderIcon;
        public Texture capsuleColliderIcon;
        public Texture meshColliderIcon;
        public Texture sphereColliderIcon;
        public Texture terrainColliderIcon;
        public Texture wheelColliderIcon;

        public Texture meshFilterIcon;
        public Texture meshRendererIcon;

        public Texture pointLightIcon;
        public Texture directionalLightIcon;
        public Texture spotLightIcon;
        public Texture areaLightIcon;

        public Texture fontIcon;
        public Texture lightingDataAssetIcon;
        public Texture lightProbesIcon;
        public Texture materialIcon;
        public Texture meshIcon;
        public Texture renderTextureIcon;
        public Texture texture2DIcon;
        public Texture textureImporterIcon;
        public Texture scriptableObjectIcon;
        public Texture shaderIcon;
        public Texture shaderVariantCollectionIcon;
        public Texture computeShaderIcon;
        public Texture editorSettingsIcon;
        public Texture cubemapIcon;
        public Texture animationIcon;
        public Texture flareLayerIcon;
        public Texture guiLayerIcon;

        public Texture lightProbeProxyVolumeIcon;
        public Texture lightProbeGroupIcon;

        public Texture GetIcon(object obj)
        {
            if (obj is GameObject) return gameObjectIcon;
            else if (obj is Transform) return transformIcon;
            else if (obj is Camera) return cameraIcon;
            else if (obj is AudioListener) return audioListenerIcon;
            else if (obj is AudioSource) return audioSourceIcon;
            else if (obj is Rigidbody) return rigidbodyIcon;
            else if (obj is BoxCollider) return boxColliderIcon;
            else if (obj is CapsuleCollider) return capsuleColliderIcon;
            else if (obj is MeshCollider) return meshColliderIcon;
            else if (obj is SphereCollider) return sphereColliderIcon;
            else if (obj is TerrainCollider) return terrainColliderIcon;
            else if (obj is WheelCollider) return wheelColliderIcon;
            else if (obj is MeshFilter) return meshFilterIcon;
            else if (obj is MeshRenderer) return meshRendererIcon;
            else if (obj is MonoBehaviour) return csScriptIcon;

            else if (obj is Font) return fontIcon;
            else if (obj is LightProbes) return lightProbeGroupIcon;
            else if (obj is LightProbeProxyVolume) return lightProbeProxyVolumeIcon;
            else if (obj is LightProbeGroup) return lightProbeGroupIcon;
            else if (obj is Material) return materialIcon;
            else if (obj is Mesh) return meshIcon;
            else if (obj is RenderTexture) return renderTextureIcon;
            else if (obj is Texture2D) return texture2DIcon;
            else if (obj is ScriptableObject) return scriptableObjectIcon;
            else if (obj is Shader) return shaderIcon;
            else if (obj is ShaderVariantCollection) return shaderVariantCollectionIcon;
            else if (obj is ComputeShader) return computeShaderIcon;
            else if (obj is Cubemap) return cubemapIcon;
            else if (obj is Animation) return animationIcon;
            else if (obj is FlareLayer) return flareLayerIcon;
            // else if (obj is GUILayer) return guiLayerIcon;
#if UNITY_EDITOR
            else if (obj is UnityEditor.EditorSettings) return editorSettingsIcon;
            else if (obj is UnityEditor.LightingDataAsset) return lightingDataAssetIcon;
            else if (obj is UnityEditor.TextureImporter) return textureImporterIcon;
#endif
            else if (obj is Light)
            {
                Light light = (Light)obj;

                if (light.type == LightType.Directional) return directionalLightIcon;
                else if (light.type == LightType.Point) return pointLightIcon;
                else if (light.type == LightType.Spot) return spotLightIcon;
                else if (light.type == LightType.Area) return areaLightIcon;
            }
            return null;
        }

        public Texture GetIcon(Type type)
        {
            if (type == typeof(GameObject)) return gameObjectIcon;
            else if (type == typeof(Transform)) return transformIcon;
            else if (type == typeof(Camera)) return cameraIcon;
            else if (type == typeof(AudioListener)) return audioListenerIcon;
            else if (type == typeof(AudioSource)) return audioSourceIcon;
            else if (type == typeof(Rigidbody)) return rigidbodyIcon;
            else if (type == typeof(BoxCollider)) return boxColliderIcon;
            else if (type == typeof(CapsuleCollider)) return capsuleColliderIcon;
            else if (type == typeof(MeshCollider)) return meshColliderIcon;
            else if (type == typeof(SphereCollider)) return sphereColliderIcon;
            else if (type == typeof(TerrainCollider)) return terrainColliderIcon;
            else if (type == typeof(WheelCollider)) return wheelColliderIcon;
            else if (type == typeof(MeshFilter)) return meshFilterIcon;
            else if (type == typeof(MeshRenderer)) return meshRendererIcon;
            
            else if (type == typeof(Font)) return fontIcon;
            else if (type == typeof(LightProbes)) return lightProbeGroupIcon;
            else if (type == typeof(LightProbeProxyVolume)) return lightProbeProxyVolumeIcon;
            else if (type == typeof(LightProbeGroup)) return lightProbeGroupIcon;
            else if (type == typeof(Material)) return materialIcon;
            else if (type == typeof(Mesh)) return meshIcon;
            else if (type == typeof(RenderTexture)) return renderTextureIcon;
            else if (type == typeof(Texture2D)) return texture2DIcon;
            else if (type == typeof(Shader)) return shaderIcon;
            else if (type == typeof(ShaderVariantCollection)) return shaderVariantCollectionIcon;
            else if (type == typeof(ComputeShader)) return computeShaderIcon;
            else if (type == typeof(Cubemap)) return cubemapIcon;
            else if (type == typeof(Animation)) return animationIcon;
            else if (type == typeof(FlareLayer)) return flareLayerIcon;
            // else if (type == typeof(GUILayer)) return guiLayerIcon;
#if UNITY_EDITOR
            else if (type == typeof(UnityEditor.EditorSettings)) return editorSettingsIcon;
            else if (type == typeof(UnityEditor.LightingDataAsset)) return lightingDataAssetIcon;
            else if (type == typeof(UnityEditor.TextureImporter)) return textureImporterIcon;
#endif
            else if (type.IsSubclassOf(typeof(ScriptableObject))) return scriptableObjectIcon;
            else if (type.IsSubclassOf(typeof(MonoBehaviour))) return csScriptIcon;
            else if (type == typeof(Light))
            {
                return pointLightIcon;
                //Light light = (Light)type;

                // if (light.type == LightType.Directional) return directionalLightIcon;
                // else if (light.type == LightType.Point) return pointLightIcon;
                // else if (light.type == LightType.Spot) return spotLightIcon;
                // else if (light.type == LightType.Area) return areaLightIcon;
            }
            return null;
        }
    }

    [Serializable]
    public class WindowSettings
    {
        public List<Search> searchList;
        public bool showSearchNonFound = true;
        public bool search;
        public Vector2 position;
        public Rect rect;
        public GUIChangeBool isDocked = new GUIChangeBool(false);
        public bool isMinimized;

        [NonSerialized] public float newScrollViewY = -1;
        [NonSerialized] public Vector2 scrollView;
        [NonSerialized] public Vector2 updatedScrollView;
        [NonSerialized] public int drag;
        [NonSerialized] public Rect rectStartScroll;
        [NonSerialized] public float scrollWindowPosY;
        [NonSerialized] public float culledSpaceY;

        public void Update(float minWidth, float minHeight)
        {
            CheckMinMaxSize(minWidth, minHeight);
            CalcRectPosition();
        }

        public void UpdateScrollView()
        {
            if (newScrollViewY != 0)
            {
                scrollView.y = newScrollViewY;
                newScrollViewY = 0;
            }

            updatedScrollView = scrollView;
        }

        public void SetScrollViewToEnd(ScrollViewCullData cull)
        {
            scrollView.y = cull.scrollWindowPosY;
        }

        public bool IsScrollViewAtEnd(ScrollViewCullData cull) { return scrollView.y == cull.scrollWindowPosY; }

        public void CheckMinMaxSize(float minWidth, float minHeight)
        {
            if (rect.width < minWidth) rect.width = minWidth;
            else if (rect.width > Screen.width) rect.width = Screen.width;

            if (rect.height < minHeight) rect.height = minHeight;
            else if (rect.height > Screen.height) rect.height = Screen.height;

            position.x = Mathf.Clamp(position.x, 0.035f - (rect.width / Screen.width), 0.995f);
            position.y = Mathf.Clamp(position.y, 0, 0.995f);
        }

        public void CalcRectPosition()
        {
            rect.x = position.x * Screen.width;
            rect.y = position.y * Screen.height;
        }

        public bool ContainsMousePos(Vector2 mousePos)
        {
            if (isMinimized) return new Rect(rect.x, rect.y, rect.width, 30).Contains(mousePos);
            else return rect.Contains(mousePos);
        }
    }
}