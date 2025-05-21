using RootMotion.FinalIK;
using UnityEngine;

public class PlayerHandIK : MonoBehaviour
{
	[HideInInspector]
	public bool active;

	public AnimationCurve animationPoserHand;

	public bool rightHand;

	[HideInInspector]
	public bool holdPose;

	private float timeRotationFinger;

	[Header("Poser fingers")]
	public Vector3[] rotationsFingersWas;

	public Vector3[] rotationsFingers;

	[Header("ИНФОРМАЦИЯ")]
	public GenericPoser handPoser;

	public Transform[] transformsHand;

	public PlayerHandIK_Prefab handPoseUse;

	private void Update()
	{
		if (active)
		{
			if (holdPose)
			{
				if (handPoser.weight < 1f)
				{
					handPoser.weight += Time.deltaTime * 2f;
					if (handPoser.weight > 1f)
					{
						handPoser.weight = 1f;
					}
				}
			}
			else if (handPoser.weight > 0f)
			{
				handPoser.weight -= Time.deltaTime * 2f;
				if (handPoser.weight < 0f)
				{
					handPoser.weight = 0f;
				}
			}
			if (timeRotationFinger < 1f)
			{
				timeRotationFinger += Time.deltaTime * 3f;
				if (timeRotationFinger > 1f)
				{
					timeRotationFinger = 1f;
				}
				for (int i = 0; i < transformsHand.Length; i++)
				{
					transformsHand[i].localRotation = Quaternion.Lerp(Quaternion.Euler(rotationsFingersWas[i]), Quaternion.Euler(rotationsFingers[i]), animationPoserHand.Evaluate(timeRotationFinger));
				}
			}
		}
		else if (handPoser.weight > 0f)
		{
			handPoser.weight -= Time.deltaTime * 3f;
			if (handPoser.weight < 0f)
			{
				handPoser.weight = 0f;
			}
		}
	}

	public void GetHand(PlayerHandIK_Prefab _hand, bool _position, bool _rotation)
	{
		handPoseUse = _hand;
		active = true;
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		base.transform.position = _hand.transform.position;
		base.transform.rotation = _hand.transform.rotation;
		for (int i = 0; i < transformsHand.Length; i++)
		{
			rotationsFingersWas[i] = transformsHand[i].localRotation.eulerAngles;
			rotationsFingers[i] = _hand.transforms[i].localRotation.eulerAngles;
		}
		if (!_position)
		{
			base.transform.position = position;
		}
		if (!_rotation)
		{
			base.transform.rotation = rotation;
		}
		timeRotationFinger = 0f;
	}

	public void HandPoseSmoothApply(PlayerHandIK_Prefab _hand, bool _position, bool _rotation, bool _hold)
	{
		GetHand(_hand, _position, _rotation);
		holdPose = _hold;
	}

	public void HandPoseSharplyApply(PlayerHandIK_Prefab _hand, bool _position, bool _rotation, bool _hold)
	{
		GetHand(_hand, _position, _rotation);
		handPoser.weight = 1f;
		holdPose = _hold;
		timeRotationFinger = 1f;
		for (int i = 0; i < transformsHand.Length; i++)
		{
			transformsHand[i].transform.localRotation = _hand.transforms[i].localRotation;
		}
	}

	public void Deactive()
	{
		active = false;
	}

	public void HoldOff()
	{
		holdPose = false;
	}
}
