using UnityEngine;

public class LineRenderer_AnimationStyle : MonoBehaviour
{
	private LineRenderer lr;

	private float timeAnimaitonNow;

	public bool play = true;

	public float speed = 1f;

	public float width = 0.1f;

	public AnimationCurve widthCurveValue;

	[Header("Audio")]
	public Transform audioT;

	private void Start()
	{
		lr = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		if (timeAnimaitonNow < 1f)
		{
			timeAnimaitonNow += Time.deltaTime * speed;
			if (timeAnimaitonNow >= 1f && play)
			{
				timeAnimaitonNow = 0f;
			}
			AnimationCurve widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(timeAnimaitonNow - width, 0f), new Keyframe(timeAnimaitonNow, widthCurveValue.Evaluate(timeAnimaitonNow)), new Keyframe(timeAnimaitonNow + width, 0f), new Keyframe(1f, 0f));
			lr.widthCurve = widthCurve;
		}
		if (play && audioT != null)
		{
			audioT.position = Vector3.Lerp(lr.GetPosition(0), lr.GetPosition(lr.positionCount - 1), timeAnimaitonNow);
		}
	}

	public void Active(bool x)
	{
		play = x;
		if (timeAnimaitonNow >= 1f)
		{
			timeAnimaitonNow = 0f;
		}
	}

	private void OnEnable()
	{
		lr = GetComponent<LineRenderer>();
		timeAnimaitonNow = 0f;
		AnimationCurve widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
		lr.widthCurve = widthCurve;
	}
}
