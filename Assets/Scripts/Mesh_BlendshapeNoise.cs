using UnityEngine;

public class Mesh_BlendshapeNoise : MonoBehaviour
{
	private SkinnedMeshRenderer mesh;

	public int[] blendshapes;

	[Range(0f, 101f)]
	public float min;

	[Range(0f, 101f)]
	public float max;

	public bool active;

	[Header("Игрок")]
	public string playerMesh;

	private float animationTimeMax;

	private float animationTimeMin;

	private float animationMax;

	private float animationMin;

	private float animationWasMax;

	private float animationWasMin;

	[Header("Анимация")]
	public AnimationCurve animationLerp;

	public float speed = 1f;

	private void Start()
	{
		animationTimeMax = 1f;
		animationTimeMin = 1f;
		if (playerMesh != null && playerMesh != "")
		{
			mesh = GlobalTag.player.transform.Find("Person/" + playerMesh).GetComponent<SkinnedMeshRenderer>();
		}
		else
		{
			mesh = GetComponent<SkinnedMeshRenderer>();
		}
	}

	private void Update()
	{
		if (!active || !(Time.timeScale > 0f))
		{
			return;
		}
		for (int i = 0; i < blendshapes.Length; i++)
		{
			mesh.SetBlendShapeWeight(blendshapes[i], Random.Range(min, max));
		}
		if (animationTimeMin < 1f)
		{
			animationTimeMin += Time.deltaTime * speed;
			if (animationTimeMin > 1f)
			{
				animationTimeMin = 1f;
			}
			min = Mathf.Lerp(animationWasMin, animationMin, animationLerp.Evaluate(animationTimeMin));
		}
		if (animationTimeMax < 1f)
		{
			animationTimeMax += Time.deltaTime * speed;
			if (animationTimeMax > 1f)
			{
				animationTimeMax = 1f;
			}
			max = Mathf.Lerp(animationWasMax, animationMax, animationLerp.Evaluate(animationTimeMax));
		}
	}

	public void Activation(bool _x)
	{
		active = _x;
	}

	public void DeactiveZero()
	{
		for (int i = 0; i < blendshapes.Length; i++)
		{
			mesh.SetBlendShapeWeight(blendshapes[i], 0f);
		}
	}

	public void AnimationMaxPlay(float _max)
	{
		animationTimeMax = 0f;
		animationWasMax = max;
		animationMax = _max;
	}

	public void AnimationMinPlay(float _min)
	{
		animationTimeMin = 0f;
		animationWasMin = min;
		animationMin = _min;
	}
}
