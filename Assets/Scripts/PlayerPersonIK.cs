using RootMotion.FinalIK;
using UnityEngine;

public class PlayerPersonIK : MonoBehaviour
{
	private PlayerMove scrpm;

	private FullBodyBipedIK scrfbbik;

	private RaycastHit hit;

	[Header("Transforms")]
	public Transform headPlayer;

	public Transform pocketRight;

	public Transform pocketLeft;

	[HideInInspector]
	public bool freeRightHand;

	[Header("Hands player")]
	public Transform handRightIK;

	public Transform handLeftIK;

	public GenericPoser armRightPoser;

	public GenericPoser armLeftPoser;

	public Transform rightElbowPlayer;

	public Transform leftElbowPlayer;

	public Transform handRightItem;

	public Transform handLeftItem;

	public Transform rightItemFixPosition;

	public Transform leftItemFixPosition;

	public Transform rightWristFixPosition;

	public Transform leftWristFixPosition;

	public LookAtIK lookHandRight;

	public LookAtIK lookHandLeft;

	[Header("Hands animation")]
	public Transform armRightHand;

	public Transform armLeftHand;

	public Transform rightElbowAnimationArms;

	public Transform leftElbowAnimationArms;

	private bool animationArms;

	private float animationIKWeightRightHandTime;

	private float animationIKWeightLeftHandTime;

	[Header("Animation")]
	public AnimationCurve animationIKWeight;

	public AnimationCurve animationIKFingers;

	private float timeAnimationIKFingers;

	public AnimationCurve animationIKPocketTake;

	private float timeAnimationIKPocketTake;

	private float timeAnimationIKMoveTowardsRight;

	private float timeAnimationIKMoveTowardsLeft;

	private Vector3 animationIKMoveTowardsRightPos;

	private Vector3 animationIKMoveTowardsLeftPos;

	private Quaternion animationIKMoveTowardsRightRot;

	private Quaternion animationIKMoveTowardsLeftRot;

	private IK_HandTrigger handTrigger;

	private GameObject itemTakePocket;

	private float timeTakePocket;

	private ObjectInspection inspection;

	[HideInInspector]
	public Vector3 inspectionHand;

	[Header("Inspection")]
	public Transform forwardInspection;

	private GameObject item;

	private GameObject itemInHand;

	[HideInInspector]
	public GameObject objectInHand;

	[HideInInspector]
	public bool itemDown;

	private float handsrandomTime;

	private Vector3 handsPositionRandom;

	private Vector3 handsRotationRandom;

	private Vector3 handsPositionRandomSave;

	private Vector3 handsRotationRandomSave;

	[HideInInspector]
	public Vector3 handsInertia;

	private bool otherControl;

	private AudioSource audioSource;

	[Header("Звуки")]
	public AudioClip[] soundsTakeItem;

	private bool rightHandUse;

	private bool leftHandUse;

	private bool needHideItem;

	private bool weightLimbRightOff;

	private bool weightLimbLeftOff;

	private Vector3 positionRightHand;

	private Quaternion rotationRightHand;

	private Vector3 positionLeftHand;

	private Quaternion rotationLeftHand;

	private void Start()
	{
		scrpm = base.transform.parent.GetComponent<PlayerMove>();
		scrfbbik = GetComponent<FullBodyBipedIK>();
		armRightPoser.weight = 0f;
		armLeftPoser.weight = 0f;
		armRightPoser.enabled = false;
		armLeftPoser.enabled = false;
		audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (otherControl)
		{
			return;
		}
		freeRightHand = true;
		rightHandUse = false;
		leftHandUse = false;
		needHideItem = false;
		weightLimbRightOff = false;
		weightLimbLeftOff = false;
		positionRightHand = handRightIK.position;
		rotationRightHand = handRightIK.rotation;
		positionLeftHand = handLeftIK.position;
		rotationLeftHand = handLeftIK.rotation;
		handsInertia = Vector3.Lerp(handsInertia, Vector3.zero, Time.deltaTime * 5f);
		handsInertia = GlobalAM.Vector3Clamp2D(handsInertia, -0.05f, 0.05f);
		if (scrpm.objectCastHandInteractive != null)
		{
			if (handTrigger == null)
			{
				if (scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>() != null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>().active)
				{
					if (itemInHand == null)
					{
						handTrigger = scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>();
					}
					else
					{
						needHideItem = true;
					}
				}
				if (scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>() != null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>().main.active)
				{
					if (itemInHand == null)
					{
						handTrigger = scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>().main;
					}
					else
					{
						needHideItem = true;
					}
				}
				if (handTrigger != null)
				{
					if (handTrigger.handPoseRight != null)
					{
						HandPoseSmoothApply(handTrigger.handPoseRight, _positionApply: true, _rotationApply: true, _hold: true);
					}
					if (handTrigger.handPoseLeft != null)
					{
						HandPoseSmoothApply(handTrigger.handPoseLeft, _positionApply: true, _rotationApply: true, _hold: true);
					}
				}
			}
			else
			{
				if (itemInHand != null)
				{
					handTrigger = null;
				}
				if ((scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>() != null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>().active && handTrigger != scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>()) || (scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>() != null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>().main.active && handTrigger != scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>().main))
				{
					handTrigger = null;
				}
				if (scrpm.objectCastHandInteractive.GetComponent<IK_HandTrigger>() == null && scrpm.objectCastHandInteractive.GetComponent<IK_HandTriggerAddon>() == null)
				{
					handTrigger = null;
				}
				if (handRightIK.GetComponent<PlayerHandIK>().handPoseUse != null && handRightIK.GetComponent<PlayerHandIK>().handPoseUse.rightHand)
				{
					if (!handRightIK.GetComponent<PlayerHandIK>().holdPose)
					{
						HandPoseSmoothApply(handTrigger.handPoseRight, _positionApply: true, _rotationApply: true, _hold: true);
					}
				}
				else if (!handLeftIK.GetComponent<PlayerHandIK>().holdPose)
				{
					HandPoseSmoothApply(handTrigger.handPoseLeft, _positionApply: true, _rotationApply: true, _hold: true);
				}
			}
		}
		else
		{
			handTrigger = null;
		}
		if (handTrigger != null)
		{
			if ((handTrigger.handToWallRight || handTrigger.handToWallLeft) && Physics.Raycast(scrpm.head.position, scrpm.head.forward, out hit, 50f, scrpm.castHead))
			{
				if (handTrigger.handPoseRight != null)
				{
					if (handTrigger.handPivotRight != null)
					{
						if (handTrigger.handFaceRight)
						{
							handTrigger.handPivotRight.transform.position = GlobalAM.TransformPivot(headPlayer, handTrigger.handFacePositionRight);
						}
						else
						{
							handTrigger.handPivotRight.transform.position = hit.point - scrpm.head.forward * handTrigger.distanceHandToWallRight;
						}
					}
					else if (handTrigger.handFaceRight)
					{
						handTrigger.handPoseRight.transform.position = GlobalAM.TransformPivot(headPlayer, handTrigger.handFacePositionRight);
					}
					else
					{
						handTrigger.handPoseRight.transform.position = hit.point - scrpm.head.forward * handTrigger.distanceHandToWallRight;
					}
				}
				if (handTrigger.handPoseLeft != null)
				{
					if (handTrigger.handPivotLeft != null)
					{
						if (handTrigger.handFaceLeft)
						{
							handTrigger.handPivotLeft.transform.position = GlobalAM.TransformPivot(headPlayer, handTrigger.handFacePositionLeft);
						}
						else
						{
							handTrigger.handPivotLeft.transform.position = hit.point - scrpm.head.forward * handTrigger.distanceHandToWallLeft;
						}
					}
					else if (handTrigger.handFaceLeft)
					{
						handTrigger.handPoseLeft.transform.position = GlobalAM.TransformPivot(headPlayer, handTrigger.handFacePositionLeft);
					}
					else
					{
						handTrigger.handPoseLeft.transform.position = hit.point - scrpm.head.forward * handTrigger.distanceHandToWallLeft;
					}
				}
			}
			if (handTrigger.handPoseRight != null)
			{
				positionRightHand = handTrigger.handPoseRight.transform.position;
				rotationRightHand = handTrigger.handPoseRight.transform.rotation;
				freeRightHand = false;
				rightHandUse = true;
			}
			if (handTrigger.handPoseLeft != null)
			{
				positionLeftHand = handTrigger.handPoseLeft.transform.position;
				rotationLeftHand = handTrigger.handPoseLeft.transform.rotation;
				leftHandUse = true;
			}
		}
		if (itemInHand != item)
		{
			if (itemInHand != null)
			{
				if (itemInHand.GetComponent<ObjectItem>().mainHand.GetComponent<PlayerHandIK_Prefab>().rightHand)
				{
					rightHandUse = false;
				}
				else
				{
					leftHandUse = false;
				}
				if (itemInHand.GetComponent<ObjectItem>().secondaryHand != null && itemInHand.GetComponent<ObjectItem>().secondaryHand.GetComponent<PlayerHandIK_Prefab>().rightHand)
				{
					rightHandUse = false;
				}
				else
				{
					leftHandUse = false;
				}
			}
			if (item != null)
			{
				if (item.GetComponent<ObjectItem>().mainHand.GetComponent<PlayerHandIK_Prefab>().rightHand)
				{
					rightHandUse = false;
				}
				else
				{
					leftHandUse = false;
				}
				if (item.GetComponent<ObjectItem>().secondaryHand != null && item.GetComponent<ObjectItem>().secondaryHand.GetComponent<PlayerHandIK_Prefab>().rightHand)
				{
					rightHandUse = false;
				}
				else
				{
					leftHandUse = false;
				}
			}
		}
		else if (itemInHand != null)
		{
			float num = Vector3.Dot(headPlayer.forward, Vector3.Normalize(base.transform.position - headPlayer.position));
			num = ((!(num > 0f)) ? (num / itemInHand.GetComponent<ObjectItem>().shareUp) : (num / itemInHand.GetComponent<ObjectItem>().shareDown));
			if (itemInHand.GetComponent<ObjectItem>().castBack != 0f && scrpm.objectCast != null && scrpm.distanceCast < itemInHand.GetComponent<ObjectItem>().castBackDistance)
			{
				num -= itemInHand.GetComponent<ObjectItem>().castBack;
			}
			if (itemInHand.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				positionRightHand = GlobalAM.TransformPivot(headPlayer, itemInHand.GetComponent<ObjectItem>().positionFace + handsInertia) + headPlayer.forward * (1f * num);
				rotationRightHand = headPlayer.rotation * Quaternion.Euler(itemInHand.GetComponent<ObjectItem>().rotationFace);
				rightHandUse = true;
				freeRightHand = false;
			}
			else
			{
				positionLeftHand = GlobalAM.TransformPivot(headPlayer, itemInHand.GetComponent<ObjectItem>().positionFace + handsInertia) + headPlayer.forward * (1f * num);
				rotationLeftHand = headPlayer.rotation * Quaternion.Euler(itemInHand.GetComponent<ObjectItem>().rotationFace);
				leftHandUse = true;
			}
			if (scrpm.distanceCast < itemInHand.GetComponent<ObjectItem>().minDistanceCastHide || (scrpm.objectCast != null && scrpm.objectCast.GetComponent<Trigger_DontShot>() != null))
			{
				if (itemInHand.GetComponent<ObjectItem>().mainHand.rightHand)
				{
					weightLimbRightOff = true;
				}
				if (!itemInHand.GetComponent<ObjectItem>().mainHand.rightHand)
				{
					weightLimbLeftOff = true;
				}
				itemDown = true;
			}
			else
			{
				itemDown = false;
			}
			if (rightHandUse)
			{
				rightElbowPlayer.transform.localPosition = Vector3.Lerp(rightElbowPlayer.transform.localPosition, itemInHand.GetComponent<ObjectItem>().positionRightElbow, Time.deltaTime * 5f);
			}
			if (leftHandUse)
			{
				leftElbowPlayer.transform.localPosition = Vector3.Lerp(leftElbowPlayer.transform.localPosition, itemInHand.GetComponent<ObjectItem>().positionLeftElbow, Time.deltaTime * 5f);
			}
		}
		if (itemTakePocket != null)
		{
			if (timeTakePocket < 1f)
			{
				timeTakePocket += Time.deltaTime * 3f;
				if (freeRightHand)
				{
					positionRightHand = itemTakePocket.GetComponent<ObjectInteractiveItemTake>().handTake.transform.position + Vector3.up * 0.1f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake) + base.transform.right * 0.2f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake);
					rotationRightHand = itemTakePocket.GetComponent<ObjectInteractiveItemTake>().handTake.transform.rotation;
					rightHandUse = true;
					if (timeTakePocket >= 1f)
					{
						itemTakePocket.GetComponent<ObjectInteractiveItemTake>().TakeInHand(handRightItem.parent);
						AnimationMoveTowardsHand(_rightHand: true);
						audioSource.clip = soundsTakeItem[Random.Range(0, soundsTakeItem.Length)];
						audioSource.pitch = Random.Range(0.95f, 1.05f);
						audioSource.Play();
					}
				}
				else
				{
					positionLeftHand = itemTakePocket.GetComponent<ObjectInteractiveItemTake>().handTake.transform.position + Vector3.up * 0.1f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake) + base.transform.right * -0.2f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake);
					rotationLeftHand = itemTakePocket.GetComponent<ObjectInteractiveItemTake>().handTake.transform.rotation;
					leftHandUse = true;
					if (timeTakePocket >= 1f)
					{
						itemTakePocket.GetComponent<ObjectInteractiveItemTake>().TakeInHand(handLeftItem.parent);
						AnimationMoveTowardsHand(_rightHand: false);
						audioSource.clip = soundsTakeItem[Random.Range(0, soundsTakeItem.Length)];
						audioSource.pitch = Random.Range(0.95f, 1.05f);
						audioSource.Play();
					}
				}
			}
			if (timeTakePocket >= 1f)
			{
				timeTakePocket += Time.deltaTime * 3f;
				if (freeRightHand)
				{
					rightHandUse = true;
					positionRightHand = pocketRight.position + Vector3.up * 0.4f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake) + base.transform.right * 0.2f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake);
					rotationRightHand = pocketRight.rotation * Quaternion.Euler(new Vector3(75f, 0f, -100f) * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake));
				}
				else
				{
					leftHandUse = true;
					positionLeftHand = pocketLeft.position + Vector3.up * 0.4f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake) + base.transform.right * -0.2f * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake);
					rotationLeftHand = pocketLeft.rotation * Quaternion.Euler(new Vector3(0f, 75f, 100f) * animationIKPocketTake.Evaluate(timeAnimationIKPocketTake));
				}
				if (timeTakePocket >= 2f)
				{
					TakeItemPocketEnd();
					AnimationMoveTowardsHandStop();
				}
			}
		}
		if (scrpm.item != null)
		{
			bool flag = false;
			if (needHideItem)
			{
				flag = true;
			}
			if (handTrigger != null)
			{
				flag = true;
			}
			if (scrpm.animationRun || scrpm.animationHandRun)
			{
				flag = true;
				if (scrpm.keepItem)
				{
					flag = false;
				}
			}
			if (flag)
			{
				needHideItem = true;
				if (itemInHand == scrpm.item)
				{
					scrpm.HideItem(x: true);
				}
			}
		}
		if (scrpm.item != null && itemInHand == null && !needHideItem)
		{
			bool flag2 = false;
			if (!needHideItem)
			{
				flag2 = true;
			}
			if (!scrpm.animationRun && !scrpm.animationHandRun)
			{
				flag2 = true;
			}
			if (flag2)
			{
				scrpm.HideItem(x: false);
			}
		}
		if (scrpm.animationRun || animationArms || scrpm.animationHandRun)
		{
			rightHandUse = false;
			leftHandUse = false;
		}
		if (rightHandUse || leftHandUse)
		{
			handsrandomTime -= Time.deltaTime;
			if ((double)handsrandomTime < 0.5)
			{
				handsPositionRandomSave = Vector3.Lerp(handsPositionRandomSave, new Vector3(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f)), Time.deltaTime * 10f);
				handsRotationRandomSave = Vector3.Lerp(handsRotationRandomSave, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), Time.deltaTime * 10f);
			}
			if (handsrandomTime < 0f)
			{
				handsrandomTime = Random.Range(0.2f, 1.5f);
			}
			handsPositionRandom = Vector3.Lerp(handsPositionRandom, handsPositionRandomSave, Time.deltaTime * 1f);
			handsRotationRandom = Vector3.Lerp(handsRotationRandom, handsRotationRandomSave, Time.deltaTime * 1f);
		}
		if (rightHandUse)
		{
			if (!weightLimbRightOff)
			{
				if (animationIKWeightRightHandTime < 1f)
				{
					animationIKWeightRightHandTime += Time.deltaTime * 3f;
					if (animationIKWeightRightHandTime > 1f)
					{
						animationIKWeightRightHandTime = 1f;
					}
				}
			}
			else if (animationIKWeightRightHandTime > 0f)
			{
				animationIKWeightRightHandTime -= Time.deltaTime * 3f;
				if (animationIKWeightRightHandTime < 0f)
				{
					animationIKWeightRightHandTime = 0f;
				}
			}
			if (timeAnimationIKMoveTowardsRight == 0f)
			{
				handRightIK.position = Vector3.Lerp(handRightIK.position, positionRightHand + handsPositionRandom, Time.deltaTime * 10f);
				handRightIK.rotation = Quaternion.Lerp(handRightIK.rotation, rotationRightHand * Quaternion.Euler(handsRotationRandom), Time.deltaTime * 10f);
			}
			else
			{
				handRightIK.position = Vector3.MoveTowards(animationIKMoveTowardsRightPos, positionRightHand, animationIKWeight.Evaluate(timeAnimationIKMoveTowardsRight));
				handRightIK.rotation = Quaternion.Slerp(animationIKMoveTowardsRightRot, rotationRightHand, animationIKWeight.Evaluate(timeAnimationIKMoveTowardsRight));
			}
			scrfbbik.solver.rightArmMapping.weight = animationIKWeight.Evaluate(animationIKWeightRightHandTime);
		}
		else
		{
			if (animationIKWeightRightHandTime > 0f)
			{
				animationIKWeightRightHandTime -= Time.deltaTime * 3f;
				if (animationIKWeightRightHandTime < 0f)
				{
					animationIKWeightRightHandTime = 0f;
				}
			}
			if (handRightIK.GetComponent<PlayerHandIK>().active && ((itemInHand != null && !itemInHand.GetComponent<ObjectItem>().mainHand.rightHand) || itemInHand == null))
			{
				handRightIK.GetComponent<PlayerHandIK>().Deactive();
			}
			if (!animationArms)
			{
				handRightIK.localPosition = Vector3.Lerp(handRightIK.localPosition, new Vector3(0.25f, 0.9f, 0f), Time.deltaTime * 10f);
				handRightIK.localRotation = Quaternion.Slerp(handRightIK.localRotation, Quaternion.Euler(-180f, -50f, 0f), Time.deltaTime * 10f);
				scrfbbik.solver.rightArmMapping.weight = animationIKWeight.Evaluate(animationIKWeightRightHandTime);
			}
		}
		if (leftHandUse)
		{
			if (!weightLimbLeftOff)
			{
				if (animationIKWeightLeftHandTime < 1f)
				{
					animationIKWeightLeftHandTime += Time.deltaTime * 3f;
					if (animationIKWeightLeftHandTime > 1f)
					{
						animationIKWeightLeftHandTime = 1f;
					}
				}
			}
			else if (animationIKWeightLeftHandTime > 0f)
			{
				animationIKWeightLeftHandTime -= Time.deltaTime * 3f;
				if (animationIKWeightLeftHandTime < 0f)
				{
					animationIKWeightLeftHandTime = 0f;
				}
			}
			if (timeAnimationIKMoveTowardsLeft == 0f)
			{
				handLeftIK.position = Vector3.Lerp(handLeftIK.position, positionLeftHand + handsPositionRandom, Time.deltaTime * 10f);
				handLeftIK.rotation = Quaternion.Lerp(handLeftIK.rotation, rotationLeftHand * Quaternion.Euler(handsRotationRandom), Time.deltaTime * 10f);
			}
			else
			{
				handLeftIK.position = Vector3.MoveTowards(animationIKMoveTowardsLeftPos, positionLeftHand, animationIKWeight.Evaluate(timeAnimationIKMoveTowardsLeft));
				handLeftIK.rotation = Quaternion.RotateTowards(animationIKMoveTowardsLeftRot, rotationLeftHand, Quaternion.Angle(animationIKMoveTowardsLeftRot, rotationLeftHand) * animationIKWeight.Evaluate(timeAnimationIKMoveTowardsLeft));
			}
			scrfbbik.solver.leftArmMapping.weight = animationIKWeight.Evaluate(animationIKWeightLeftHandTime);
		}
		else
		{
			if (animationIKWeightLeftHandTime > 0f)
			{
				animationIKWeightLeftHandTime -= Time.deltaTime * 3f;
				if (animationIKWeightLeftHandTime < 0f)
				{
					animationIKWeightLeftHandTime = 0f;
				}
			}
			if (handLeftIK.GetComponent<PlayerHandIK>().active && ((itemInHand != null && itemInHand.GetComponent<ObjectItem>().mainHand.rightHand) || itemInHand == null))
			{
				handLeftIK.GetComponent<PlayerHandIK>().Deactive();
			}
			if (!animationArms)
			{
				handLeftIK.localPosition = Vector3.Lerp(handLeftIK.localPosition, new Vector3(-0.25f, 0.9f, 0f), Time.deltaTime * 10f);
				handLeftIK.localRotation = Quaternion.Lerp(handLeftIK.localRotation, Quaternion.Euler(-180f, 50f, 0f), Time.deltaTime * 10f);
				scrfbbik.solver.leftArmMapping.weight = animationIKWeight.Evaluate(animationIKWeightLeftHandTime);
			}
		}
		if (!rightHandUse)
		{
			rightElbowPlayer.transform.localPosition = Vector3.Lerp(rightElbowPlayer.transform.localPosition, new Vector3(0.3f, 0.5f, -0.3f), Time.deltaTime * 5f);
		}
		if (!leftHandUse)
		{
			leftElbowPlayer.transform.localPosition = Vector3.Lerp(leftElbowPlayer.transform.localPosition, new Vector3(-0.3f, 0.5f, -0.3f), Time.deltaTime * 5f);
		}
		if (inspection != null)
		{
			if (!inspection.variantNow)
			{
				forwardInspection.transform.rotation = Quaternion.Euler(inspection.variantA.rotationWas + inspection.variantA.rotationNow);
				if (inspection.variantA.rightHand)
				{
					forwardInspection.transform.position = rightWristFixPosition.position;
					if (!lookHandRight.enabled)
					{
						lookHandRight.enabled = true;
					}
					if (lookHandRight.solver.headWeight < 1f)
					{
						lookHandRight.solver.headWeight += Time.deltaTime * 5f;
						if (lookHandRight.solver.headWeight > 1f)
						{
							lookHandRight.solver.headWeight = 1f;
						}
					}
				}
				else
				{
					forwardInspection.transform.position = leftWristFixPosition.position;
					if (!lookHandLeft.enabled)
					{
						lookHandLeft.enabled = true;
					}
					if (lookHandLeft.solver.headWeight < 1f)
					{
						lookHandLeft.solver.headWeight += Time.deltaTime * 5f;
						if (lookHandLeft.solver.headWeight > 1f)
						{
							lookHandLeft.solver.headWeight = 1f;
						}
					}
				}
			}
			else
			{
				forwardInspection.transform.rotation = Quaternion.Euler(inspection.variantB.rotationWas + inspection.variantB.rotationNow);
				if (inspection.variantB.rightHand)
				{
					forwardInspection.transform.position = rightWristFixPosition.position;
					if (!lookHandRight.enabled)
					{
						lookHandRight.enabled = true;
					}
					if (lookHandRight.solver.headWeight < 1f)
					{
						lookHandRight.solver.headWeight += Time.deltaTime * 5f;
						if (lookHandRight.solver.headWeight > 1f)
						{
							lookHandRight.solver.headWeight = 1f;
						}
					}
				}
				else
				{
					forwardInspection.transform.position = leftWristFixPosition.position;
					if (!lookHandLeft.enabled)
					{
						lookHandLeft.enabled = true;
					}
					if (lookHandLeft.solver.headWeight < 1f)
					{
						lookHandLeft.solver.headWeight += Time.deltaTime * 5f;
						if (lookHandLeft.solver.headWeight > 1f)
						{
							lookHandLeft.solver.headWeight = 1f;
						}
					}
				}
			}
		}
		else
		{
			if (lookHandRight.enabled)
			{
				lookHandRight.solver.headWeight -= Time.deltaTime * 5f;
				if (lookHandRight.solver.headWeight <= 0f)
				{
					lookHandRight.solver.headWeight = 0f;
					lookHandRight.enabled = false;
				}
			}
			if (lookHandLeft.enabled)
			{
				lookHandLeft.solver.headWeight -= Time.deltaTime * 5f;
				if (lookHandLeft.solver.headWeight <= 0f)
				{
					lookHandLeft.solver.headWeight = 0f;
					lookHandLeft.enabled = false;
				}
			}
		}
		if (timeAnimationIKPocketTake > 0f)
		{
			timeAnimationIKPocketTake += Time.deltaTime * 1.5f;
			if (timeAnimationIKPocketTake >= 1f)
			{
				timeAnimationIKPocketTake = 0f;
			}
		}
		if (timeAnimationIKMoveTowardsRight > 0f)
		{
			timeAnimationIKMoveTowardsRight += Time.deltaTime * 3f;
			if (timeAnimationIKMoveTowardsRight > 1f)
			{
				timeAnimationIKMoveTowardsRight = 1f;
			}
		}
		if (timeAnimationIKMoveTowardsLeft > 0f)
		{
			timeAnimationIKMoveTowardsLeft += Time.deltaTime * 3f;
			if (timeAnimationIKMoveTowardsLeft > 1f)
			{
				timeAnimationIKMoveTowardsLeft = 1f;
			}
		}
		if (animationArms)
		{
			if (timeAnimationIKFingers < 1f)
			{
				timeAnimationIKFingers += Time.deltaTime * 3f;
				if (timeAnimationIKFingers > 1f)
				{
					timeAnimationIKFingers = 1f;
				}
				armLeftPoser.weight = animationIKFingers.Evaluate(timeAnimationIKFingers);
				armRightPoser.weight = animationIKFingers.Evaluate(timeAnimationIKFingers);
			}
			if (scrfbbik.solver.leftArmMapping.weight < 1f)
			{
				scrfbbik.solver.leftArmMapping.weight += Time.deltaTime * 3f;
				if (scrfbbik.solver.leftArmMapping.weight >= 1f)
				{
					scrfbbik.solver.leftArmMapping.weight = 1f;
				}
			}
			if (scrfbbik.solver.rightArmMapping.weight < 1f)
			{
				scrfbbik.solver.rightArmMapping.weight += Time.deltaTime * 3f;
				if (scrfbbik.solver.rightArmMapping.weight >= 1f)
				{
					scrfbbik.solver.rightArmMapping.weight = 1f;
				}
			}
			animationIKWeightLeftHandTime = 1f;
			animationIKWeightRightHandTime = 1f;
			handRightIK.localPosition = Vector3.Lerp(handRightIK.localPosition, Vector3.zero, Time.deltaTime * 10f);
			handRightIK.localRotation = Quaternion.Slerp(handRightIK.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 10f);
			handLeftIK.localPosition = Vector3.Lerp(handLeftIK.localPosition, Vector3.zero, Time.deltaTime * 10f);
			handLeftIK.localRotation = Quaternion.Slerp(handLeftIK.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 10f);
		}
		else if (timeAnimationIKFingers > 0f)
		{
			timeAnimationIKFingers -= Time.deltaTime * 3f;
			if (timeAnimationIKFingers <= 0f)
			{
				timeAnimationIKFingers = 0f;
				armRightPoser.weight = 0f;
				armLeftPoser.weight = 0f;
				armRightPoser.enabled = false;
				armLeftPoser.enabled = false;
			}
			else
			{
				armLeftPoser.weight = animationIKFingers.Evaluate(timeAnimationIKFingers);
				armRightPoser.weight = animationIKFingers.Evaluate(timeAnimationIKFingers);
			}
		}
	}

	private void LateUpdate()
	{
		rightItemFixPosition.position = handRightItem.transform.position;
		leftItemFixPosition.position = handLeftItem.transform.position;
		rightItemFixPosition.rotation = handRightItem.transform.rotation;
		leftItemFixPosition.rotation = handLeftItem.transform.rotation;
		rightWristFixPosition.position = handRightItem.parent.transform.position;
		leftWristFixPosition.position = handLeftItem.parent.transform.position;
		rightWristFixPosition.rotation = handRightItem.parent.transform.rotation;
		leftWristFixPosition.rotation = handLeftItem.parent.transform.rotation;
	}

	private void AnimationMoveTowardsHand(bool _rightHand)
	{
		if (_rightHand)
		{
			animationIKMoveTowardsRightPos = handRightIK.position;
			animationIKMoveTowardsRightRot = handRightIK.rotation;
			timeAnimationIKMoveTowardsRight = 0.0001f;
		}
		else
		{
			animationIKMoveTowardsLeftPos = handLeftIK.position;
			animationIKMoveTowardsLeftRot = handLeftIK.rotation;
			timeAnimationIKMoveTowardsLeft = 0.0001f;
		}
	}

	private void AnimationMoveTowardsHandStop()
	{
		timeAnimationIKMoveTowardsRight = 0f;
		timeAnimationIKMoveTowardsLeft = 0f;
	}

	public void UpdateItemInHand()
	{
		if (itemInHand != null)
		{
			if (item.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				handRightIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(item.GetComponent<ObjectItem>().mainHand, _position: false, _rotation: true, _hold: false);
			}
			else
			{
				handLeftIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(item.GetComponent<ObjectItem>().mainHand, _position: false, _rotation: true, _hold: false);
			}
		}
	}

	public void IkZero()
	{
		animationIKWeightLeftHandTime = 0f;
		animationIKWeightRightHandTime = 0f;
		scrfbbik.solver.leftArmMapping.weight = 0f;
		scrfbbik.solver.rightArmMapping.weight = 0f;
	}

	private void HandPoseSmoothApply(PlayerHandIK_Prefab _pose, bool _positionApply, bool _rotationApply, bool _hold)
	{
		if (_pose.rightHand)
		{
			handRightIK.GetComponent<PlayerHandIK>().HandPoseSmoothApply(_pose, _positionApply, _rotationApply, _hold);
		}
		else
		{
			handLeftIK.GetComponent<PlayerHandIK>().HandPoseSmoothApply(_pose, _positionApply, _rotationApply, _hold);
		}
	}

	public void HandPoseSharplyApply(PlayerHandIK_Prefab _pose, bool _limbHand, bool _hold)
	{
		if (_pose.rightHand)
		{
			handRightIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(_pose, _position: true, _rotation: true, _hold);
			if (_limbHand)
			{
				scrfbbik.solver.rightArmMapping.weight = 1f;
				animationIKWeightRightHandTime = 1f;
				handRightIK.position = _pose.transform.position;
				handRightIK.rotation = _pose.transform.rotation;
			}
		}
		else
		{
			handLeftIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(_pose, _position: true, _rotation: true, _hold);
			if (_limbHand)
			{
				scrfbbik.solver.leftArmMapping.weight = 1f;
				animationIKWeightLeftHandTime = 1f;
				handLeftIK.position = _pose.transform.position;
				handLeftIK.rotation = _pose.transform.rotation;
			}
		}
	}

	public void TakeItem(GameObject _item)
	{
		item = _item;
	}

	public void RemoveItem()
	{
		if (item != null)
		{
			rightItemFixPosition.GetComponent<AudioSource>().clip = item.GetComponent<ObjectItem>().soundRemove;
			rightItemFixPosition.GetComponent<AudioSource>().Play();
			item = null;
		}
	}

	public void TakeItemInHand()
	{
		if (!(itemInHand != item))
		{
			return;
		}
		if (objectInHand != null && !scrpm.keepItem)
		{
			Object.Destroy(objectInHand);
		}
		if (item != null)
		{
			if (item.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				objectInHand = Object.Instantiate(item.GetComponent<ObjectItem>().itemInHand, handRightItem);
				handRightIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(item.GetComponent<ObjectItem>().mainHand, _position: false, _rotation: true, _hold: true);
			}
			else
			{
				objectInHand = Object.Instantiate(item.GetComponent<ObjectItem>().itemInHand, handLeftItem);
				handLeftIK.GetComponent<PlayerHandIK>().HandPoseSharplyApply(item.GetComponent<ObjectItem>().mainHand, _position: false, _rotation: true, _hold: true);
			}
			objectInHand.transform.localPosition = item.GetComponent<ObjectItem>().positionItemInHand;
			objectInHand.transform.localRotation = item.GetComponent<ObjectItem>().rotationItemInHand;
		}
		itemInHand = item;
		if (item != null)
		{
			rightItemFixPosition.GetComponent<AudioSource>().clip = item.GetComponent<ObjectItem>().soundTake;
			rightItemFixPosition.GetComponent<AudioSource>().Play();
		}
	}

	public void TakeItemPocket(PlayerHandIK_Prefab _hand, GameObject _object)
	{
		timeAnimationIKPocketTake = 0.0001f;
		timeTakePocket = 0f;
		itemTakePocket = _object;
		HandPoseSmoothApply(_hand, _positionApply: false, _rotationApply: false, _hold: true);
		AnimationMoveTowardsHand(_hand.rightHand);
	}

	private void TakeItemPocketEnd()
	{
		handRightIK.GetComponent<PlayerHandIK>().HoldOff();
		handLeftIK.GetComponent<PlayerHandIK>().HoldOff();
		itemTakePocket.GetComponent<ObjectInteractiveItemTake>().TakeDestroy();
		itemTakePocket = null;
	}

	public void StartInpection(ObjectInspection _inspection)
	{
		inspection = _inspection;
	}

	public void StopInspection()
	{
		inspection = null;
	}

	public void AnimationHandsFace(bool x)
	{
		animationArms = x;
		if (!animationArms)
		{
			handRightIK.transform.parent = base.transform.parent;
			handLeftIK.transform.parent = base.transform.parent;
			rightElbowPlayer.transform.position = scrfbbik.solver.rightArmChain.bendConstraint.bendGoal.position;
			leftElbowPlayer.transform.position = scrfbbik.solver.leftArmChain.bendConstraint.bendGoal.position;
			scrfbbik.solver.rightArmChain.bendConstraint.bendGoal = rightElbowPlayer;
			scrfbbik.solver.leftArmChain.bendConstraint.bendGoal = leftElbowPlayer;
			handRightIK.GetComponent<PlayerHandIK>().Deactive();
			handLeftIK.GetComponent<PlayerHandIK>().Deactive();
		}
		else
		{
			armRightPoser.enabled = true;
			armLeftPoser.enabled = true;
			handRightIK.transform.parent = armRightHand;
			handLeftIK.transform.parent = armLeftHand;
			scrfbbik.solver.rightArmChain.bendConstraint.bendGoal = rightElbowAnimationArms;
			scrfbbik.solver.leftArmChain.bendConstraint.bendGoal = leftElbowAnimationArms;
		}
	}

	public void ActiveOtherControl(bool x)
	{
		otherControl = x;
		if (!otherControl)
		{
			scrfbbik.solver.rightHandEffector.target = handRightIK;
			scrfbbik.solver.leftHandEffector.target = handLeftIK;
		}
	}
}
