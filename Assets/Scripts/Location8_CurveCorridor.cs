using UnityEngine;

public class Location8_CurveCorridor : MonoBehaviour
{
	public Vector3 pointStart;

	public Vector3 pointFinish;

	public AnimationCurve animationCruve;

	private Animator anim;

	private float distanceMax;

	private float lastDistancePlayerPoint;

	private bool play;

	private Transform playerT;

	private AudioSource au;

	private bool destroyMe;

	private void Start()
	{
		playerT = GlobalTag.player.transform;
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (play)
		{
			if (Vector3.Distance(playerT.position, pointFinish) < lastDistancePlayerPoint)
			{
				lastDistancePlayerPoint = Vector3.Distance(playerT.position, pointFinish);
				anim.SetFloat("Curve", animationCruve.Evaluate(1f - lastDistancePlayerPoint / distanceMax));
				if (lastDistancePlayerPoint < 2f)
				{
					anim.SetFloat("Curve", 1f);
					play = false;
					anim.enabled = false;
					destroyMe = true;
				}
				if ((double)au.volume < 0.25)
				{
					au.volume += Time.deltaTime * 0.3f;
					if ((double)au.volume > 0.25)
					{
						au.volume = 0.25f;
					}
				}
			}
			else if (au.volume > 0f)
			{
				au.volume -= Time.deltaTime * 0.5f;
				if (au.volume < 0f)
				{
					au.volume = 0f;
				}
			}
		}
		else if (Vector3.Distance(playerT.position, pointStart) < 2f)
		{
			Play();
		}
		if (destroyMe)
		{
			au.volume -= Time.deltaTime * 0.1f;
			if (au.volume <= 0f)
			{
				Object.Destroy(this);
			}
		}
	}

	public void Play()
	{
		play = true;
		anim = GetComponent<Animator>();
		distanceMax = Vector3.Distance(playerT.position, pointFinish) - 2f;
		lastDistancePlayerPoint = distanceMax;
	}
}
