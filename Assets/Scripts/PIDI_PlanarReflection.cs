using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class PIDI_PlanarReflection : MonoBehaviour
{
	public struct ReflectionCamera
	{
		public Camera reflector;

		public Camera source;

		public PIDI_PlanarReflection owner;

		public ReflectionCamera(Camera v_ref, Camera v_source, PIDI_PlanarReflection v_owner)
		{
			reflector = v_ref;
			source = v_source;
			owner = v_owner;
		}
	}

	private static ReflectionCamera[] v_reflectionPool = new ReflectionCamera[0];

	private List<ReflectionCamera> v_reflectionCameras = new List<ReflectionCamera>();

	public RenderingPath v_renderingPath = RenderingPath.Forward;

	public RenderTexture v_staticTexture;

	public bool b_displayGizmos;

	public bool b_updateInEditMode = true;

	public bool b_useFixedUptade;

	public bool b_ignoreSkybox;

	public Color v_backdropColor = Color.blue;

	public int v_timesPerSecond = 30;

	public bool b_useGlobalSettings = true;

	public static int v_maxResolution = 4096;

	public static int v_forcedQualityDowngrade = 0;

	public bool b_useExplicitCameras;

	public bool b_useExplicitNormal;

	public Vector3 v_explicitNormal = new Vector3(0f, 0f, 1f);

	public Shader depthShader;

	public bool b_forcePower2 = true;

	public bool b_useScreenResolution;

	public bool b_useDynamicResolution;

	public int v_dynRes;

	public float v_resMultiplier = 1f;

	public Vector2 v_minMaxDistance = new Vector2(1f, 15f);

	public bool b_useReflectionsPool = true;

	public int v_poolSize = 1;

	public float v_disableOnDistance = -1f;

	public Vector2 v_resolution = new Vector2(1024f, 1024f);

	private Vector2 v_oldRes;

	public int v_pixelLights = -1;

	public LayerMask v_reflectLayers = -1;

	public bool b_simplifyLandscapes = true;

	public int v_agressiveness = 2;

	public float v_shadowDistance = 25f;

	public float v_clippingOffset;

	public float v_nearClipModifier;

	public float v_farClipModifier = 100f;

	public bool b_safeClipping = true;

	public bool b_realOblique = true;

	public Vector2 v_mirrorSize = new Vector2(0f, 0f);

	public RenderTexture v_oldRend;

	public RenderTexture v_oldDepth;

	private float v_nextUpdate;

	private Vector3 v_surfaceNormal;

	private static bool b_isRendering;

	private RenderTexture v_refTexture;

	private float v_oldDistance;

	private float v_distance;

	private Camera v_reflectionCam;

	private Camera v_srcCamera;

	private bool b_oldUsePool;

	private bool b_willRender;

	public Mesh v_defaultMesh;

	public Material v_defaultMat;

	[SerializeField]
	private Material v_matInstance;

	public void OnEnable()
	{
		GenerateReflectionsPool(0);
		if ((bool)GetComponent<Renderer>())
		{
			GetComponent<Renderer>().enabled = true;
		}
	}

	public void Update()
	{
		if (Application.isPlaying && b_useFixedUptade)
		{
			if (Time.realtimeSinceStartup > v_nextUpdate && b_willRender)
			{
				b_willRender = false;
				v_nextUpdate = Time.realtimeSinceStartup + 1f / (float)v_timesPerSecond;
			}
			else if (Time.realtimeSinceStartup > v_nextUpdate)
			{
				b_willRender = true;
			}
		}
	}

	public void OnWillRenderObject()
	{
		b_realOblique = b_safeClipping;
		if (v_oldRend != v_staticTexture)
		{
			v_refTexture = null;
			v_oldRend = v_staticTexture;
		}
		if (((bool)GetComponent<PIDI_ForceUpdate>() || !GetComponent<MeshFilter>() || !GetComponent<MeshFilter>().sharedMesh) && !v_staticTexture)
		{
			return;
		}
		if (!Application.isPlaying && !b_updateInEditMode)
		{
			ClearReflectors();
		}
		else
		{
			if (Application.isPlaying && b_useFixedUptade && !b_willRender)
			{
				return;
			}
			if (!base.enabled || !GetComponent<Renderer>())
			{
				ClearReflectors();
			}
			else
			{
				if (b_isRendering)
				{
					return;
				}
				v_srcCamera = Camera.current;
				if (!v_srcCamera)
				{
					ClearReflectors();
					return;
				}
				Quaternion rotation = v_srcCamera.transform.rotation;
				if (v_srcCamera.transform.eulerAngles.x == 0f || v_srcCamera.transform.eulerAngles.z == 0f)
				{
					v_srcCamera.transform.Rotate(0.0001f, 0f, 0.0001f);
				}
				if (b_useExplicitCameras)
				{
					if ((bool)v_srcCamera.transform.Find("ExplicitCamera"))
					{
						v_srcCamera = v_srcCamera.transform.Find("ExplicitCamera").GetComponent<Camera>();
					}
					else if (Camera.current.cameraType != CameraType.SceneView)
					{
						return;
					}
				}
				if (base.transform.InverseTransformPoint(v_srcCamera.transform.position).z < 0f && !b_useExplicitNormal)
				{
					return;
				}
				v_distance = Vector3.Distance(v_srcCamera.transform.position, base.transform.position);
				if (b_useScreenResolution)
				{
					v_resolution = new Vector2((float)Screen.width * v_resMultiplier, (float)Screen.height * v_resMultiplier);
					if (b_useDynamicResolution)
					{
						if (v_distance < v_minMaxDistance.x)
						{
							v_resolution *= 1f;
						}
						else if (v_disableOnDistance > v_minMaxDistance.y)
						{
							v_resolution *= 0.25f;
						}
						else
						{
							v_resolution *= 0.5f;
						}
					}
					else
					{
						v_resolution *= Mathf.Pow(0.5f, v_dynRes);
					}
				}
				v_surfaceNormal = (b_useExplicitNormal ? v_explicitNormal : base.transform.forward);
				Vector3 position = base.transform.position;
				float num = Vector3.Dot(v_srcCamera.transform.forward, v_surfaceNormal);
				if (v_disableOnDistance > 0f && v_distance > v_disableOnDistance)
				{
					return;
				}
				b_isRendering = true;
				if (b_oldUsePool != b_useReflectionsPool)
				{
					GenerateReflectionsPool(0);
					ClearReflectors();
					b_oldUsePool = b_useReflectionsPool;
				}
				if (b_useReflectionsPool && v_reflectionPool.Length < v_poolSize)
				{
					GenerateReflectionsPool(v_poolSize);
				}
				if (Mathf.Abs(num) > 1f)
				{
					num %= 1f;
				}
				float farClipPlane = v_srcCamera.farClipPlane;
				float nearClipPlane = v_srcCamera.nearClipPlane;
				Vector2 vector = v_mirrorSize;
				if (!b_safeClipping)
				{
					vector *= 0f;
				}
				float[] array = new float[5];
				Vector3 inNormal = -v_srcCamera.transform.forward;
				new Plane(-v_surfaceNormal, v_srcCamera.transform.position).Raycast(new Ray(position, v_surfaceNormal), out array[0]);
				array[0] = Mathf.Abs(array[0]);
				Plane plane = new Plane(inNormal, v_srcCamera.transform.position);
				plane.Raycast(new Ray(base.transform.TransformPoint(vector.x * -0.5f, 0f, 0f), v_surfaceNormal), out array[1]);
				plane.Raycast(new Ray(base.transform.TransformPoint(vector.x * 0.5f, 0f, 0f), v_surfaceNormal), out array[2]);
				plane.Raycast(new Ray(base.transform.TransformPoint(0f, vector.y * 0.5f, 0f), v_surfaceNormal), out array[3]);
				plane.Raycast(new Ray(base.transform.TransformPoint(0f, vector.y * -0.5f, 0f), v_surfaceNormal), out array[4]);
				float num2 = 0f;
				if (!b_safeClipping)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = Mathf.Abs(array[i]);
					}
					num2 = Mathf.Min(array);
				}
				else
				{
					num2 = Mathf.Abs((num >= 0f) ? Mathf.Min(array) : Mathf.Max(array));
				}
				v_srcCamera.nearClipPlane = ((Mathf.Abs(num) > 0f) ? ((num2 + v_nearClipModifier) * Mathf.Abs(num)) : (num2 + v_nearClipModifier));
				v_srcCamera.farClipPlane = v_farClipModifier + v_srcCamera.nearClipPlane;
				GetReflectionCamera(v_srcCamera, out v_reflectionCam);
				if (v_reflectionCam == null)
				{
					b_isRendering = false;
					return;
				}
				SynchCameras(v_srcCamera, v_reflectionCam);
				float w = 0f - Vector3.Dot(v_surfaceNormal, position) - v_clippingOffset;
				Vector4 v_refPlane = new Vector4(v_surfaceNormal.x, v_surfaceNormal.y, v_surfaceNormal.z, w);
				Matrix4x4 v_refMatrix = Matrix4x4.zero;
				CalculateReflectionMatrix(ref v_refMatrix, v_refPlane);
				v_reflectionCam.transform.position = v_refMatrix.MultiplyPoint(v_srcCamera.transform.position);
				v_reflectionCam.worldToCameraMatrix = v_srcCamera.worldToCameraMatrix * v_refMatrix;
				if (b_safeClipping && b_realOblique)
				{
					v_reflectionCam.projectionMatrix = v_srcCamera.CalculateObliqueMatrix(CameraSpacePlane(v_reflectionCam, base.transform.position, v_surfaceNormal, 1f));
					v_reflectionCam.nearClipPlane += v_nearClipModifier;
					v_reflectionCam.farClipPlane = v_reflectionCam.nearClipPlane + v_farClipModifier;
				}
				else
				{
					v_reflectionCam.projectionMatrix = v_srcCamera.projectionMatrix;
				}
				v_reflectionCam.cullingMask = -17 & v_reflectLayers.value;
				v_reflectionCam.renderingPath = RenderingPath.Forward;
				float shadowDistance = QualitySettings.shadowDistance;
				if (v_shadowDistance > -1f)
				{
					QualitySettings.shadowDistance = v_shadowDistance;
				}
				int pixelLightCount = QualitySettings.pixelLightCount;
				if (v_pixelLights > -1)
				{
					QualitySettings.pixelLightCount = v_pixelLights;
				}
				GL.invertCulling = true;
				v_reflectionCam.targetTexture = v_refTexture;
				v_reflectionCam.useOcclusionCulling = false;
				v_reflectionCam.depthTextureMode = DepthTextureMode.None;
				v_reflectionCam.Render();
				bool flag = false;
				Material[] sharedMaterials = GetComponent<Renderer>().sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (material != null && material.HasProperty("_ReflectionDepth"))
					{
						flag = true;
					}
				}
				if ((bool)v_oldDepth)
				{
					RenderTexture.ReleaseTemporary(v_oldDepth);
					v_oldDepth = null;
				}
				if (!v_oldDepth && flag)
				{
					v_oldDepth = RenderTexture.GetTemporary((int)v_resolution.x, (int)v_resolution.y, 0, RenderTextureFormat.ARGB32);
					v_reflectionCam.targetTexture = v_oldDepth;
					v_reflectionCam.clearFlags = CameraClearFlags.Color;
					v_reflectionCam.backgroundColor = Color.green;
					if ((bool)depthShader)
					{
						Shader.SetGlobalVector("_DepthPlaneOrigin", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z));
						Shader.SetGlobalVector("_DepthPlaneNormal", new Vector4(0f - v_surfaceNormal.x, 0f - v_surfaceNormal.y, 0f - v_surfaceNormal.z));
						v_reflectionCam.RenderWithShader(depthShader, "");
					}
				}
				v_reflectionCam.transform.position = v_srcCamera.transform.position;
				v_reflectionCam.transform.eulerAngles = new Vector3(0f, v_srcCamera.transform.eulerAngles.y, v_srcCamera.transform.eulerAngles.z);
				v_reflectionCam.enabled = false;
				QualitySettings.shadowDistance = shadowDistance;
				QualitySettings.pixelLightCount = pixelLightCount;
				v_srcCamera.farClipPlane = farClipPlane;
				v_srcCamera.nearClipPlane = nearClipPlane;
				GL.invertCulling = false;
				Material[] sharedMaterials2 = GetComponent<Renderer>().sharedMaterials;
				if (sharedMaterials2.Length != 0 && GetComponent<Renderer>().sharedMaterial != null)
				{
					Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f);
					Vector3 lossyScale = base.transform.lossyScale;
					Matrix4x4 matrix4x2 = base.transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z));
					matrix4x2 = matrix4x * v_srcCamera.projectionMatrix * v_srcCamera.worldToCameraMatrix * matrix4x2;
					sharedMaterials = sharedMaterials2;
					foreach (Material material2 in sharedMaterials)
					{
						if (material2 != null && material2.HasProperty("_ReflectionTex"))
						{
							material2.SetMatrix("_ProjMatrix", matrix4x2);
							material2.SetTexture("_ReflectionTex", v_refTexture);
							material2.SetTexture("_ReflectionDepth", v_oldDepth);
							if (material2.HasProperty("_ChromaKeyColor"))
							{
								material2.SetColor("_ChromaKeyColor", b_ignoreSkybox ? v_backdropColor : Color.clear);
							}
						}
					}
				}
				v_srcCamera.transform.rotation = rotation;
				b_isRendering = false;
				if (b_useReflectionsPool)
				{
					ReleaseCamera();
				}
			}
		}
	}

	public void OnWillRenderObject(Camera withCamera)
	{
		b_realOblique = b_safeClipping;
		if (v_oldRend != v_staticTexture)
		{
			v_refTexture = null;
			v_oldRend = v_staticTexture;
		}
		if ((!GetComponent<MeshFilter>() || !GetComponent<MeshFilter>().sharedMesh) && !v_staticTexture)
		{
			return;
		}
		if (!Application.isPlaying && !b_updateInEditMode)
		{
			ClearReflectors();
		}
		else
		{
			if (Application.isPlaying && b_useFixedUptade && !b_willRender)
			{
				return;
			}
			if (!base.enabled || !GetComponent<Renderer>())
			{
				ClearReflectors();
			}
			else
			{
				if (b_isRendering)
				{
					return;
				}
				v_srcCamera = withCamera;
				if (!v_srcCamera)
				{
					ClearReflectors();
					return;
				}
				Quaternion rotation = v_srcCamera.transform.rotation;
				if (v_srcCamera.transform.eulerAngles.x == 0f || v_srcCamera.transform.eulerAngles.z == 0f)
				{
					v_srcCamera.transform.Rotate(0.0001f, 0f, 0.0001f);
				}
				v_distance = Vector3.Distance(v_srcCamera.transform.position, base.transform.position);
				if (b_useScreenResolution)
				{
					v_resolution = new Vector2((float)Screen.width * v_resMultiplier, (float)Screen.height * v_resMultiplier);
					if (b_useDynamicResolution)
					{
						if (v_distance < v_minMaxDistance.x)
						{
							v_resolution *= 1f;
						}
						else if (v_disableOnDistance > v_minMaxDistance.y)
						{
							v_resolution *= 0.25f;
						}
						else
						{
							v_resolution *= 0.5f;
						}
					}
					else
					{
						v_resolution *= Mathf.Pow(0.5f, v_dynRes);
					}
				}
				v_surfaceNormal = (b_useExplicitNormal ? v_explicitNormal : base.transform.forward);
				Vector3 position = base.transform.position;
				float num = Vector3.Dot(v_srcCamera.transform.forward, v_surfaceNormal);
				if ((v_disableOnDistance > 0f && v_distance > v_disableOnDistance) || Vector3.Dot(v_srcCamera.transform.forward, v_surfaceNormal) > 0.1f)
				{
					return;
				}
				b_isRendering = true;
				if (b_oldUsePool != b_useReflectionsPool)
				{
					GenerateReflectionsPool(0);
					ClearReflectors();
					b_oldUsePool = b_useReflectionsPool;
				}
				if (b_useReflectionsPool && v_reflectionPool.Length < v_poolSize)
				{
					GenerateReflectionsPool(v_poolSize);
				}
				if (Mathf.Abs(num) > 1f)
				{
					num %= 1f;
				}
				float farClipPlane = v_srcCamera.farClipPlane;
				float nearClipPlane = v_srcCamera.nearClipPlane;
				Vector2 vector = v_mirrorSize;
				if (!b_safeClipping)
				{
					vector *= 0f;
				}
				float[] array = new float[5];
				Vector3 inNormal = -v_srcCamera.transform.forward;
				new Plane(-v_surfaceNormal, v_srcCamera.transform.position).Raycast(new Ray(position, v_surfaceNormal), out array[0]);
				array[0] = Mathf.Abs(array[0]);
				Plane plane = new Plane(inNormal, v_srcCamera.transform.position);
				plane.Raycast(new Ray(base.transform.TransformPoint(vector.x * -0.5f, 0f, 0f), v_surfaceNormal), out array[1]);
				plane.Raycast(new Ray(base.transform.TransformPoint(vector.x * 0.5f, 0f, 0f), v_surfaceNormal), out array[2]);
				plane.Raycast(new Ray(base.transform.TransformPoint(0f, vector.y * 0.5f, 0f), v_surfaceNormal), out array[3]);
				plane.Raycast(new Ray(base.transform.TransformPoint(0f, vector.y * -0.5f, 0f), v_surfaceNormal), out array[4]);
				float num2 = 0f;
				if (!b_safeClipping)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = Mathf.Abs(array[i]);
					}
					num2 = Mathf.Min(array);
				}
				else
				{
					num2 = Mathf.Abs((num >= 0f) ? Mathf.Min(array) : Mathf.Max(array));
				}
				v_srcCamera.nearClipPlane = ((Mathf.Abs(num) > 0f) ? ((num2 + v_nearClipModifier) * Mathf.Abs(num)) : (num2 + v_nearClipModifier));
				v_srcCamera.farClipPlane = v_farClipModifier + v_srcCamera.nearClipPlane;
				GetReflectionCamera(v_srcCamera, out v_reflectionCam);
				if (v_reflectionCam == null)
				{
					b_isRendering = false;
					return;
				}
				SynchCameras(v_srcCamera, v_reflectionCam);
				float w = 0f - Vector3.Dot(v_surfaceNormal, position) - v_clippingOffset;
				Vector4 v_refPlane = new Vector4(v_surfaceNormal.x, v_surfaceNormal.y, v_surfaceNormal.z, w);
				Matrix4x4 v_refMatrix = Matrix4x4.zero;
				CalculateReflectionMatrix(ref v_refMatrix, v_refPlane);
				v_reflectionCam.transform.position = v_refMatrix.MultiplyPoint(v_srcCamera.transform.position);
				v_reflectionCam.worldToCameraMatrix = v_srcCamera.worldToCameraMatrix * v_refMatrix;
				if (b_safeClipping && b_realOblique)
				{
					v_reflectionCam.projectionMatrix = v_srcCamera.CalculateObliqueMatrix(CameraSpacePlane(v_reflectionCam, base.transform.position, v_surfaceNormal, 1f));
					v_reflectionCam.nearClipPlane += v_nearClipModifier;
					v_reflectionCam.farClipPlane = v_reflectionCam.nearClipPlane + v_farClipModifier;
				}
				else
				{
					v_reflectionCam.projectionMatrix = v_srcCamera.projectionMatrix;
				}
				v_reflectionCam.cullingMask = -17 & v_reflectLayers.value;
				v_reflectionCam.renderingPath = v_renderingPath;
				float shadowDistance = QualitySettings.shadowDistance;
				if (v_shadowDistance > -1f)
				{
					QualitySettings.shadowDistance = v_shadowDistance;
				}
				int pixelLightCount = QualitySettings.pixelLightCount;
				if (v_pixelLights > -1)
				{
					QualitySettings.pixelLightCount = v_pixelLights;
				}
				GL.invertCulling = true;
				v_reflectionCam.targetTexture = v_refTexture;
				v_reflectionCam.useOcclusionCulling = false;
				v_reflectionCam.Render();
				bool flag = false;
				Material[] sharedMaterials = GetComponent<Renderer>().sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (material != null && material.HasProperty("_ReflectionDepth"))
					{
						flag = true;
					}
				}
				if ((bool)v_oldDepth)
				{
					RenderTexture.ReleaseTemporary(v_oldDepth);
					v_oldDepth = null;
				}
				if (!v_oldDepth && flag)
				{
					v_oldDepth = RenderTexture.GetTemporary((int)v_resolution.x, (int)v_resolution.y);
					v_reflectionCam.targetTexture = v_oldDepth;
					if ((bool)depthShader)
					{
						Shader.SetGlobalVector("_DepthPlaneOrigin", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z));
						Shader.SetGlobalVector("_DepthPlaneNormal", new Vector4(0f - v_surfaceNormal.x, 0f - v_surfaceNormal.y, 0f - v_surfaceNormal.z));
						v_reflectionCam.RenderWithShader(depthShader, "");
					}
				}
				v_reflectionCam.transform.position = v_srcCamera.transform.position;
				v_reflectionCam.transform.eulerAngles = new Vector3(0f, v_srcCamera.transform.eulerAngles.y, v_srcCamera.transform.eulerAngles.z);
				v_reflectionCam.enabled = false;
				QualitySettings.shadowDistance = shadowDistance;
				QualitySettings.pixelLightCount = pixelLightCount;
				v_srcCamera.farClipPlane = farClipPlane;
				v_srcCamera.nearClipPlane = nearClipPlane;
				GL.invertCulling = false;
				Material[] sharedMaterials2 = GetComponent<Renderer>().sharedMaterials;
				if (sharedMaterials2.Length != 0 && GetComponent<Renderer>().sharedMaterial != null)
				{
					Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.one * 0.5f, Quaternion.identity, Vector3.one * 0.5f);
					Vector3 lossyScale = base.transform.lossyScale;
					Matrix4x4 matrix4x2 = base.transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z));
					matrix4x2 = matrix4x * v_srcCamera.projectionMatrix * v_srcCamera.worldToCameraMatrix * matrix4x2;
					sharedMaterials = sharedMaterials2;
					foreach (Material material2 in sharedMaterials)
					{
						if (material2 != null && material2.HasProperty("_ReflectionTex"))
						{
							material2.SetMatrix("_ProjMatrix", matrix4x2);
							material2.SetTexture("_ReflectionTex", v_refTexture);
							if (material2.HasProperty("_ChromaKeyColor"))
							{
								material2.SetColor("_ChromaKeyColor", b_ignoreSkybox ? v_backdropColor : Color.clear);
							}
						}
					}
				}
				v_srcCamera.transform.rotation = rotation;
				b_isRendering = false;
				if (b_useReflectionsPool)
				{
					ReleaseCamera();
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (b_displayGizmos)
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, b_useExplicitNormal ? Quaternion.FromToRotation(base.transform.up, v_explicitNormal) : base.transform.rotation, Vector3.one);
			Gizmos.color = new Color(0f, 0f, 1f, 0.35f);
			Gizmos.DrawCube(Vector3.zero, new Vector3(v_mirrorSize.x, v_mirrorSize.y, 0.08f));
			Gizmos.color = Color.cyan;
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.DrawRay(new Ray(base.transform.position, b_useExplicitNormal ? v_explicitNormal : base.transform.forward));
		}
	}

	public static void GenerateReflectionsPool(int v_size)
	{
		ReflectionCamera[] array = v_reflectionPool;
		for (int i = 0; i < array.Length; i++)
		{
			ReflectionCamera reflectionCamera = array[i];
			if (reflectionCamera.reflector != null)
			{
				UnityEngine.Object.DestroyImmediate(reflectionCamera.reflector.GetComponent<FlareLayer>());
				UnityEngine.Object.DestroyImmediate(reflectionCamera.reflector.gameObject);
			}
		}
		v_reflectionPool = new ReflectionCamera[v_size];
		for (int j = 0; j < v_size; j++)
		{
			v_reflectionPool[j] = default(ReflectionCamera);
			if ((bool)GameObject.Find("Pooled Reflector " + j))
			{
				v_reflectionPool[j].reflector = GameObject.Find("Pooled Reflector " + j).GetComponent<Camera>();
				continue;
			}
			GameObject gameObject = new GameObject("Pooled Reflector " + j, typeof(Camera), typeof(Skybox));
			v_reflectionPool[j].reflector = gameObject.GetComponent<Camera>();
			v_reflectionPool[j].reflector.enabled = false;
			if (!v_reflectionPool[j].reflector.GetComponent<FlareLayer>())
			{
				v_reflectionPool[j].reflector.gameObject.AddComponent<FlareLayer>();
			}
			v_reflectionPool[j].reflector.transform.position = Vector3.zero;
			v_reflectionPool[j].reflector.transform.rotation = Quaternion.identity;
			v_reflectionPool[j].reflector.gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void GetReflectionCamera(Camera v_source, out Camera v_reflected)
	{
		v_reflected = null;
		if (b_useReflectionsPool)
		{
			for (int i = 0; i < v_reflectionPool.Length; i++)
			{
				if (v_reflectionPool[i].owner == null)
				{
					v_reflectionPool[i].owner = this;
					v_reflected = v_reflectionPool[i].reflector;
					v_reflectionPool[i].source = v_source;
				}
			}
		}
		else
		{
			foreach (ReflectionCamera v_reflectionCamera in v_reflectionCameras)
			{
				if (v_reflectionCamera.source == v_source)
				{
					v_reflected = v_reflectionCamera.reflector;
					v_reflected.enabled = false;
				}
			}
			if (v_reflected == null)
			{
				GameObject gameObject = new GameObject("RefCamera " + GetInstanceID() + " from " + v_source.GetInstanceID(), typeof(Camera), typeof(Skybox));
				v_reflected = gameObject.GetComponent<Camera>();
				v_reflected.enabled = false;
				if (!v_reflected.gameObject.GetComponent<FlareLayer>())
				{
					v_reflected.gameObject.AddComponent<FlareLayer>();
				}
				v_reflected.transform.position = Vector3.zero;
				v_reflected.transform.rotation = Quaternion.Euler(0.0001f, 0f, 0.001f);
				v_reflected.gameObject.hideFlags = HideFlags.HideAndDontSave;
				v_reflectionCameras.Add(new ReflectionCamera(v_reflected, v_source, null));
			}
		}
		v_reflected.orthographic = false;
		Vector2 vector = v_resolution;
		if (b_useGlobalSettings)
		{
			vector.x = (int)(vector.x / Mathf.Pow(2f, Mathf.Clamp(v_forcedQualityDowngrade, 0, 3)));
			vector.x = (int)Mathf.Clamp(vector.x, 16f, v_maxResolution);
			vector.y = (int)(vector.y / Mathf.Pow(2f, Mathf.Clamp(v_forcedQualityDowngrade, 0, 3)));
			vector.y = (int)Mathf.Clamp(vector.y, 16f, v_maxResolution);
		}
		if ((bool)GetComponent<MeshFilter>() && (bool)GetComponent<MeshFilter>().sharedMesh && !v_staticTexture && (!v_refTexture || v_oldRes != vector))
		{
			if ((bool)v_refTexture)
			{
				UnityEngine.Object.DestroyImmediate(v_refTexture);
			}
			v_refTexture = new RenderTexture((int)vector.x, (int)vector.y, 16);
			v_refTexture.name = "RenderTextureFrom " + GetInstanceID();
			v_refTexture.isPowerOfTwo = true;
			v_refTexture.hideFlags = HideFlags.DontSave;
			v_oldRes = vector;
		}
		else if ((bool)v_staticTexture)
		{
			v_refTexture = v_staticTexture;
		}
	}

	public void ReleaseCamera()
	{
		for (int i = 0; i < v_reflectionPool.Length; i++)
		{
			if (v_reflectionPool[i].owner == this)
			{
				v_reflectionPool[i].owner = null;
			}
		}
	}

	private void SynchCameras(Camera v_source, Camera v_reflected)
	{
		if (v_reflected == null)
		{
			return;
		}
		v_reflected.clearFlags = (b_ignoreSkybox ? CameraClearFlags.Color : v_source.clearFlags);
		v_reflected.backgroundColor = (b_ignoreSkybox ? v_backdropColor : v_source.backgroundColor);
		if (v_source.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox component = v_source.GetComponent<Skybox>();
			Skybox component2 = v_reflected.GetComponent<Skybox>();
			if (!component || !component.material)
			{
				component2.enabled = false;
			}
			else
			{
				component2.material = component.material;
				component2.enabled = true;
			}
		}
		v_reflected.nearClipPlane = v_source.nearClipPlane;
		v_reflected.farClipPlane = v_source.farClipPlane;
		v_reflected.orthographic = v_source.orthographic;
		v_reflected.orthographicSize = v_source.orthographicSize;
		v_reflected.fieldOfView = v_source.fieldOfView;
		v_reflected.aspect = v_source.aspect;
	}

	private void CalculateReflectionMatrix(ref Matrix4x4 v_refMatrix, Vector4 v_refPlane)
	{
		v_refMatrix.m00 = 1f - 2f * v_refPlane[0] * v_refPlane[0];
		v_refMatrix.m01 = -2f * v_refPlane[0] * v_refPlane[1];
		v_refMatrix.m02 = -2f * v_refPlane[0] * v_refPlane[2];
		v_refMatrix.m03 = -2f * v_refPlane[3] * v_refPlane[0];
		v_refMatrix.m10 = -2f * v_refPlane[1] * v_refPlane[0];
		v_refMatrix.m11 = 1f - 2f * v_refPlane[1] * v_refPlane[1];
		v_refMatrix.m12 = -2f * v_refPlane[1] * v_refPlane[2];
		v_refMatrix.m13 = -2f * v_refPlane[3] * v_refPlane[1];
		v_refMatrix.m20 = -2f * v_refPlane[2] * v_refPlane[0];
		v_refMatrix.m21 = -2f * v_refPlane[2] * v_refPlane[1];
		v_refMatrix.m22 = 1f - 2f * v_refPlane[2] * v_refPlane[2];
		v_refMatrix.m23 = -2f * v_refPlane[3] * v_refPlane[2];
		v_refMatrix.m30 = 0f;
		v_refMatrix.m31 = 0f;
		v_refMatrix.m32 = 0f;
		v_refMatrix.m33 = 1f;
	}

	private Vector4 CameraSpacePlane(Camera v_cam, Vector3 v_pos, Vector3 v_normal, float sideSign)
	{
		Vector3 point = v_pos + v_normal * v_clippingOffset;
		Matrix4x4 worldToCameraMatrix = v_cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(v_normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	public void ClearReflectors()
	{
		if ((bool)v_oldDepth)
		{
			RenderTexture.ReleaseTemporary(v_oldDepth);
			v_oldDepth = null;
		}
		if ((bool)v_refTexture && v_refTexture != v_staticTexture)
		{
			UnityEngine.Object.DestroyImmediate(v_refTexture);
		}
		if (b_useReflectionsPool)
		{
			v_reflectionPool = new ReflectionCamera[0];
			foreach (ReflectionCamera v_reflectionCamera in v_reflectionCameras)
			{
				if (v_reflectionCamera.reflector != null)
				{
					UnityEngine.Object.DestroyImmediate(v_reflectionCamera.reflector.gameObject);
				}
			}
			v_reflectionCameras.Clear();
			return;
		}
		foreach (ReflectionCamera v_reflectionCamera2 in v_reflectionCameras)
		{
			if (v_reflectionCamera2.reflector != null)
			{
				UnityEngine.Object.DestroyImmediate(v_reflectionCamera2.reflector.gameObject);
			}
		}
		v_reflectionCameras.Clear();
	}

	public void OnDisable()
	{
		ClearReflectors();
		GC.Collect();
	}

	public bool IsObjectReflectionVisible(Transform targetObject, Camera fromCamera)
	{
		if ((bool)targetObject && (bool)fromCamera)
		{
			float w = 0f - Vector3.Dot(v_surfaceNormal, base.transform.position) - v_clippingOffset;
			Vector4 v_refPlane = new Vector4(v_surfaceNormal.x, v_surfaceNormal.y, v_surfaceNormal.z, w);
			Matrix4x4 v_refMatrix = Matrix4x4.zero;
			CalculateReflectionMatrix(ref v_refMatrix, v_refPlane);
			Vector3 vector = v_refMatrix.MultiplyPoint(targetObject.position);
			Ray ray = new Ray(fromCamera.transform.position, (vector - fromCamera.transform.position).normalized);
			float enter = 0f;
			if (new Plane(v_surfaceNormal, base.transform.position).Raycast(ray, out enter) && (bool)GetComponent<MeshRenderer>() && GetComponent<MeshRenderer>().bounds.Contains(ray.GetPoint(enter)))
			{
				return true;
			}
		}
		return false;
	}
}
