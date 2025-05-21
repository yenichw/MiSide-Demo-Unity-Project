using UnityEngine;
using UnityEngine.Serialization;

public class OutlinesPostprocessed : MonoBehaviour
{
	[FormerlySerializedAs("postprocessMaterial")]
	[SerializeField]
	public Material PostprocessMaterial;

	private Camera cam;

	private void Start()
	{
		cam = GetComponent<Camera>();
		cam.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, PostprocessMaterial);
	}
}
