using UnityEngine;
using UnityEngine.Events;

public class Location3WalkToToilet : MonoBehaviour
{
	public UnityEvent eventFinish;

	public Animator_FunctionsOverride mitaScript;

	public float distance = 10f;

	public Transform positionPlayerAnimation;

	public AnimationClip animationMitaWalk;

	public AnimationClip animationMitaIdle;

	public AnimationClip animationPlayerWalk;

	public AnimationClip animationPlayerIdle;

	[Header("Animation")]
	public AnimationCurve animationPosZ;

	public AnimationCurve animationRotY;

	public float walkDistance;

	[Header("Doors")]
	public GameObject doorKitchen;

	public GameObject doorMain;

	public float distanceOpenMain;

	private bool doorMainOpen;

	public GameObject doorToilet;

	public float distanceOpenToilet;

	private bool doorToiletOpen;

	public AnimationClip animaitonOpen;

	private Vector3 positionPlayerStart;

	private float speedWalkNow;

	private float timeWalk;

	private PlayerMove playerScript;

	private float timeDatamosh;

	private void Start()
	{
		playerScript = GlobalTag.player.GetComponent<PlayerMove>();
		playerScript.animOver["OtherAnimationB"] = animationPlayerIdle;
		playerScript.animOver["OtherAnimationBF"] = animationPlayerWalk;
		positionPlayerStart = playerScript.transform.position;
		mitaScript.animOver["SimpleB"] = animationMitaIdle;
		mitaScript.animOver["SimpleBF"] = animationMitaWalk;
		doorKitchen.GetComponent<ObjectDoor>().AnimationPlay(animaitonOpen);
		timeDatamosh = Random.Range(1.5f, 3f);
	}

	private void LateUpdate()
	{
		if (Input.GetButton("Up"))
		{
			timeWalk = 0.5f;
		}
		if (timeWalk > 0f)
		{
			timeWalk -= Time.deltaTime;
			if (timeWalk < 0f)
			{
				timeWalk = 0f;
			}
			speedWalkNow = Mathf.Lerp(speedWalkNow, 1f, Time.deltaTime * 8f);
			playerScript.AnimationFloatSet(ab: false, 1f);
			mitaScript.AnimationFloatSet(ab: false, 1f);
		}
		else
		{
			speedWalkNow = Mathf.Lerp(speedWalkNow, 0f, Time.deltaTime * 5f);
		}
		walkDistance += speedWalkNow * Time.deltaTime / distance;
		if (walkDistance > 1f)
		{
			walkDistance = 1f;
		}
		positionPlayerAnimation.SetPositionAndRotation(Vector3.Lerp(positionPlayerStart, new Vector3(-12.21f, 0f, 0.398f), walkDistance) + new Vector3(0f, 0f, animationPosZ.Evaluate(walkDistance)), Quaternion.Euler(new Vector3(0f, -90f + animationRotY.Evaluate(walkDistance) * 45f, 0f)));
		playerScript.transform.SetPositionAndRotation(positionPlayerAnimation.position, positionPlayerAnimation.rotation);
		mitaScript.transform.SetPositionAndRotation(positionPlayerAnimation.position, positionPlayerAnimation.rotation);
		if (walkDistance >= 1f)
		{
			doorToilet.GetComponent<ObjectDoor>().AnimationStop();
			eventFinish.Invoke();
			Object.Destroy(base.gameObject);
		}
		if (walkDistance > 0.15f)
		{
			doorKitchen.GetComponent<ObjectDoor>().AnimationStop();
		}
		if (doorMain != null)
		{
			if (walkDistance > distanceOpenMain && !doorMainOpen)
			{
				doorMainOpen = true;
				doorMain.GetComponent<ObjectDoor>().AnimationPlay(animaitonOpen);
			}
			if (walkDistance > distanceOpenMain + 0.15f)
			{
				doorMain.GetComponent<ObjectDoor>().AnimationStop();
				doorMain = null;
			}
		}
		if (walkDistance > distanceOpenToilet && !doorToiletOpen)
		{
			doorToiletOpen = true;
			doorToilet.GetComponent<ObjectDoor>().AnimationPlay(animaitonOpen);
		}
		timeDatamosh -= Time.deltaTime;
		if (timeDatamosh < 0f)
		{
			if (!GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().datamosh.enabled)
			{
				GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectDatamosh(x: true);
				timeDatamosh = Random.Range(0.3f, 0.5f);
			}
			else
			{
				GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectDatamosh(x: false);
				timeDatamosh = Random.Range(1.5f, 3f);
			}
		}
	}
}
