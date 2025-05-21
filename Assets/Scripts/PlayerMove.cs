using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMove : MonoBehaviour
{
	[Header("Игрок")]
	public float headOffsetWalkForward = 0.15f;

	public float headOffsetWalkBack = 0.05f;

	public float headOffsetWalkRight = 0.15f;

	public bool canRun;

	public bool needRun;

	public PhysicMaterial physMaterialStopLadder;

	public float intensityMouse = 1f;

	private PlayerPerson scrpp;

	[HideInInspector]
	public PlayerPersonIK scrppik;

	[Header("Скрипты")]
	public PlayerArmsHead scrpah;

	private float timeCameraFreeze;

	private bool faceWall;

	private float forwardHead;

	private float heightHead;

	private float rotX;

	private float rotY;

	private float clampRotYMin;

	private float clampRotYMax;

	private int timeCast;

	private AnimationCurve cameraOtherTargetLerp;

	private Transform cameraOtherTarget;

	private float timeCameraOtherTargetLerp;

	private Vector3 cameraOtherTargetWasPosition;

	private Quaternion cameraOtherTargetWasRotation;

	[Header("Голова")]
	public Transform mainCamera;

	public Transform head;

	public AnimationCurve animationHeadWalk;

	private float animationHeadWalkTime;

	public AnimationCurve animationHeadRotationX;

	private float animationHeadTorationXTime;

	public AnimationCurve animationHeadRotationY;

	private float animationHeadTorationYTime;

	public Transform headPerson;

	public LayerMask castHead;

	public LayerMask castMove;

	public LookAtIK ikHead;

	public AnimationCurve animationCameraZoom;

	private bool zoom;

	private float timeZoom;

	private float zoomWas;

	public Transform fixHead;

	private float cameraNoise;

	[HideInInspector]
	public bool dontMove;

	[HideInInspector]
	public bool stopMouseMove;

	private float speed;

	private float speedAnim;

	private int moveH;

	private int moveV;

	private Rigidbody rb;

	private bool hide;

	private bool ladder;

	private float ladderSpeed;

	private Vector3 velocityNormalized;

	private float moveSlow;

	private Vector3 velocityLocal;

	public bool canSit;

	private bool needSit;

	private int sit;

	private CapsuleCollider capsule;

	private float timeSitStart;

	private float timeSitStop;

	[HideInInspector]
	public bool animationHandRun;

	private float timeAnimationHandNow;

	private AnimationClip animationHandsLoop;

	private AnimationClip animationHandsStop;

	private UnityEvent eventAnimationHandStartLoop;

	private AnimatorOverrideController animArmsFaceOver;

	private bool typeAnimationArms;

	private bool animationArmsFace;

	private bool animationArmsRechangeItem;

	[HideInInspector]
	public string nameStatePlayNow;

	[HideInInspector]
	public ObjectAnimationPlayer scrAnimationNow;

	[HideInInspector]
	public bool animationRun;

	[HideInInspector]
	public bool keepItem;

	[HideInInspector]
	public bool animationFast;

	private float timeAnimationNow;

	[HideInInspector]
	public float timeAnimationAfter;

	[HideInInspector]
	public bool animationLoopRun;

	private AnimationClip animationLoop;

	private AnimationClip animationStop;

	private Transform animationPosition;

	private UnityEvent eventAnimationStartLoop;

	private bool typeAnimation;

	private Quaternion rotationPlayerSave;

	private Vector3 positionPlayerSave;

	[HideInInspector]
	public AnimatorOverrideController animOver;

	[HideInInspector]
	public float angleHeadRotateAnimation;

	private float rotXAnim;

	private float rotYAnim;

	private float animForward;

	private float animRight;

	private bool stopAnimationPositionHips;

	private Vector3 stopAPHStartPos;

	private Vector3 stopAPHStopPos;

	[Header("анимация")]
	public AnimationCurve animationBlend;

	private float animationBlendTime;

	public AnimationCurve animationBlendCamera;

	public Animator animPerson;

	public Animator animArmsFace;

	public Transform hipsPerson;

	private int timeAnimOtherFloat;

	private float animatorFloatOtherA;

	private float animatorFloatOtherB;

	private AudioSource audioRotation;

	private float volumeRotation;

	[Header("Звуки")]
	public ObjectFoot foot;

	private Transform lookTarget;

	private float lookTime;

	private UnityEvent lookEventEnd;

	private bool hideItem;

	[Header("Предметы")]
	public GameObject item;

	[Header("Информация")]
	[HideInInspector]
	public int blockInteractive;

	private RaycastHit hit;

	[HideInInspector]
	public GameObject objectCast;

	[HideInInspector]
	public GameObject objectCastInteractive;

	[HideInInspector]
	public GameObject objectCastHandInteractive;

	[HideInInspector]
	public float distanceCast;

	public void Awake()
	{
		if (GlobalTag.player == null)
		{
			GlobalTag.player = base.gameObject;
			GlobalTag.playerRightItem = base.transform.Find("Person").GetComponent<PlayerPersonIK>().rightItemFixPosition.gameObject;
			GlobalTag.playerLeftItem = base.transform.Find("Person").GetComponent<PlayerPersonIK>().leftItemFixPosition.gameObject;
		}
		if (GlobalTag.cameraPlayer == null)
		{
			GlobalTag.cameraPlayer = mainCamera.gameObject;
		}
	}

	public void StartComponent()
	{
		scrpp = base.transform.Find("Person").GetComponent<PlayerPerson>();
		scrppik = scrpp.GetComponent<PlayerPersonIK>();
		rb = GetComponent<Rigidbody>();
		moveSlow = 1f;
		rotX = base.transform.rotation.eulerAngles.y;
		rotY = head.localRotation.eulerAngles.x;
		timeCameraFreeze = 0.5f;
		heightHead = 1.7f;
		forwardHead = 0.1f;
		timeZoom = 1f;
		clampRotYMin = -85f;
		clampRotYMax = 89f;
		fixHead.SetPositionAndRotation(headPerson.position, headPerson.rotation);
		UpdateSettingsCamera();
		animOver = new AnimatorOverrideController(animPerson.runtimeAnimatorController);
		animPerson.runtimeAnimatorController = animOver;
		animArmsFaceOver = new AnimatorOverrideController(animArmsFace.runtimeAnimatorController);
		animArmsFace.runtimeAnimatorController = animArmsFaceOver;
		animationBlendTime = 1f;
		capsule = GetComponent<CapsuleCollider>();
		audioRotation = GetComponent<AudioSource>();
		if (GlobalGame.timeGameplay && GlobalAM.ExistsData("Continue"))
		{
			string[] array = GlobalAM.LoadData("Continue");
			GlobalGame.timeS = GlobalAM.StringToFloat(array[1]);
			GlobalGame.timeM = GlobalAM.StringToInt(array[2]);
			GlobalGame.timeH = GlobalAM.StringToInt(array[3]);
		}
		TeleportPlayer(base.transform.position);
		ResetCast();
	}

	private void Update()
	{
		if (!(Time.timeScale > 0f))
		{
			return;
		}
		if (!animationRun)
		{
			animationHeadTorationXTime += Time.deltaTime * 0.25f;
			if (animationHeadTorationXTime > 1f)
			{
				animationHeadTorationXTime -= 1f;
			}
			animationHeadTorationYTime += Time.deltaTime * 0.25f;
			if (animationHeadTorationYTime > 1f)
			{
				animationHeadTorationYTime -= 1f;
			}
			rotX += Random.Range(0f - cameraNoise, cameraNoise);
			rotY += Random.Range(0f - cameraNoise, cameraNoise);
			if (cameraNoise > 0f)
			{
				head.localRotation *= Quaternion.Euler(Random.Range(0f - cameraNoise, cameraNoise), Random.Range(0f - cameraNoise, cameraNoise), 0f);
				base.transform.rotation *= Quaternion.Euler(0f, Random.Range(0f - cameraNoise, cameraNoise), 0f);
			}
			if (sit == 0)
			{
				if (((canRun || GlobalGame.trailer) && Input.GetButton("Shift")) || needRun)
				{
					speedAnim = 2f;
					speed = 1.5f;
				}
				else
				{
					speedAnim = 1f;
					speed = 1f;
				}
				if (animPerson.GetBool("Sit"))
				{
					timeSitStop = 0.35f;
				}
				animPerson.SetBool("Sit", value: false);
			}
			else
			{
				speedAnim = 1f;
				speed = 0.75f;
				if (!animPerson.GetBool("Sit"))
				{
					timeSitStart = 0.35f;
				}
				animPerson.SetBool("Sit", value: true);
			}
			if (((canSit || GlobalGame.trailer) && Input.GetButton("Sit")) || needSit || timeSitStart > 0f)
			{
				sit = 2;
			}
			if (timeSitStop > 0f)
			{
				sit = 0;
			}
			if (sit > 0)
			{
				sit--;
				capsule.center = Vector3.Lerp(capsule.center, new Vector3(0f, 0.55f, 0.25f), Time.deltaTime * 5f);
				capsule.height = Mathf.Lerp(capsule.height, 0.9f, Time.deltaTime * 5f);
				capsule.radius = Mathf.Lerp(capsule.radius, 0.2f, Time.deltaTime * 5f);
				if (Physics.Raycast(base.transform.position + Vector3.up * 0.9f, Vector3.up, out hit, 0.2f, castMove))
				{
					sit = 3;
				}
				heightHead = Mathf.Lerp(heightHead, 0.85f, Time.deltaTime * 10f);
				foot.inensityVolume = 0.3f;
			}
			else
			{
				capsule.center = Vector3.Lerp(capsule.center, new Vector3(0f, 1.075f, 0f), Time.deltaTime * 5f);
				capsule.height = Mathf.Lerp(capsule.height, 1.85f, Time.deltaTime * 5f);
				capsule.radius = Mathf.Lerp(capsule.radius, 0.25f, Time.deltaTime * 5f);
				if ((moveH == 0 && moveV == 0) || !GlobalGame.headMove)
				{
					heightHead = Mathf.Lerp(heightHead, 1.7f, Time.deltaTime * 10f);
				}
				else
				{
					heightHead = Mathf.Lerp(heightHead, 1.65f, Time.deltaTime * 10f);
				}
				foot.inensityVolume = 1f;
			}
			if (!stopMouseMove && lookTime == 0f)
			{
				if (timeCameraFreeze == 0f)
				{
					rotX += Input.GetAxis("Mouse X") * (1f + GlobalGame.mouseSpeed) * intensityMouse;
				}
				if (rotX > 360f)
				{
					rotX -= 360f;
				}
				if (rotX < 0f)
				{
					rotX += 360f;
				}
				scrppik.handsInertia -= new Vector3(Input.GetAxis("Mouse X") * (1f + GlobalGame.mouseSpeed) / 50f, 0f);
				if (timeCameraFreeze == 0f)
				{
					rotY -= Input.GetAxis("Mouse Y") * (1f + GlobalGame.mouseSpeed) * intensityMouse;
				}
				if (rotY != -85f && rotY != 89f)
				{
					scrppik.handsInertia += new Vector3(0f, Input.GetAxis("Mouse Y") * (1f + GlobalGame.mouseSpeed) / 50f);
				}
				if (animationHandRun)
				{
					if (clampRotYMin < -75f)
					{
						clampRotYMin += Time.deltaTime * 20f;
						if (clampRotYMin > -75f)
						{
							clampRotYMin = -75f;
						}
					}
					if (clampRotYMax > 80f)
					{
						clampRotYMax -= Time.deltaTime * 20f;
						if (clampRotYMax < 80f)
						{
							clampRotYMax = 80f;
						}
					}
				}
				else
				{
					if (clampRotYMin > -85f)
					{
						clampRotYMin -= Time.deltaTime * 20f;
						if (clampRotYMin < -85f)
						{
							clampRotYMin = -85f;
						}
					}
					if (clampRotYMax < 89f)
					{
						clampRotYMax += Time.deltaTime * 20f;
						if (clampRotYMax > 89f)
						{
							clampRotYMax = 89f;
						}
					}
				}
				rotY = Mathf.Clamp(rotY, clampRotYMin, clampRotYMax);
				if (Input.GetAxis("Mouse Y") != 0f || Input.GetAxis("Mouse X") != 0f)
				{
					volumeRotation += Time.deltaTime * (Mathf.Abs(Input.GetAxis("Mouse Y")) + Mathf.Abs(Input.GetAxis("Mouse X"))) * intensityMouse;
					if ((double)volumeRotation > 0.15)
					{
						volumeRotation = 0.15f;
					}
				}
			}
			if (lookTime > 0f)
			{
				rotX = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(GlobalAM.DirectionFloor(base.transform.position, lookTarget.position), Vector3.up), Time.deltaTime * 8f).eulerAngles.y;
				rotY = Quaternion.Lerp(head.localRotation, Quaternion.LookRotation(lookTarget.position - head.position, Vector3.up), Time.deltaTime * 8f).eulerAngles.x;
				lookTime -= Time.deltaTime;
				if (lookTime <= 0f)
				{
					lookTime = 0f;
					lookTarget = null;
					lookEventEnd.Invoke();
				}
			}
			if (animationHeadWalkTime > 0f)
			{
				if (speed <= 1f)
				{
					animationHeadWalkTime += Time.deltaTime * 2f * speed;
				}
				else
				{
					animationHeadWalkTime += Time.deltaTime * 3f;
				}
				if (animationHeadWalkTime >= 1f)
				{
					animationHeadWalkTime = 0f;
					foot.FootStep();
				}
				if (GlobalGame.headMove)
				{
					head.localPosition = new Vector3(0f, heightHead + animationHeadWalk.Evaluate(animationHeadWalkTime) * 0.025f, forwardHead);
				}
				else
				{
					head.localPosition = new Vector3(0f, heightHead, forwardHead);
				}
			}
			else
			{
				head.localPosition = new Vector3(0f, heightHead, forwardHead);
			}
			moveH = 0;
			moveV = 0;
			if (!dontMove)
			{
				if (Input.GetAxis("Horizontal") > 0.5f)
				{
					moveH++;
				}
				if (Input.GetAxis("Horizontal") < -0.5f)
				{
					moveH--;
				}
				if (Input.GetAxis("Vertical") > 0.5f)
				{
					moveV++;
				}
				if (Input.GetAxis("Vertical") < -0.5f)
				{
					moveV--;
				}
			}
			if (moveH != 0 || moveV != 0)
			{
				if (Physics.Raycast(base.transform.position + Vector3.up * 0.3f, Vector3.Normalize(base.transform.forward * moveV + base.transform.right * moveH), out hit, 0.4f, castMove))
				{
					moveH = 0;
					moveV = 0;
				}
				if ((moveH != 0 || moveV != 0) && animationHeadWalkTime == 0f)
				{
					animationHeadWalkTime = 0.001f;
				}
			}
		}
		if (animationRun)
		{
			sit = 0;
			head.localPosition = Vector3.Lerp(head.localPosition, new Vector3(0f, heightHead, forwardHead), Time.deltaTime * 10f);
		}
		if (!animationRun || timeAnimationNow == -1f)
		{
			timeCast++;
			if (timeCast == 5)
			{
				timeCast = 0;
				if (Physics.SphereCast(mainCamera.position, 0.025f, mainCamera.forward, out hit, 50f, castHead))
				{
					objectCast = hit.collider.gameObject;
					distanceCast = hit.distance;
				}
				else
				{
					objectCast = null;
				}
				if (Physics.SphereCast(mainCamera.position, 0.025f, mainCamera.forward, out hit, 50f, castHead))
				{
					float num = 1f;
					if (hit.collider.GetComponent<ObjectInteractive>() != null)
					{
						num = hit.collider.GetComponent<ObjectInteractive>().distanceFloor;
					}
					if (GlobalAM.DistanceFloor(hit.point, base.transform.position) <= num)
					{
						objectCastInteractive = hit.collider.gameObject;
					}
					else
					{
						objectCastInteractive = null;
					}
				}
				else
				{
					objectCastInteractive = null;
				}
				if (!animationHandRun)
				{
					if (Physics.SphereCast(mainCamera.position, 0.025f, mainCamera.forward, out hit, 1f, castHead))
					{
						if ((double)GlobalAM.DistanceFloor(base.transform.position, hit.point) > 0.4)
						{
							if (hit.collider.GetComponent<ObjectInteractiveReqIK>() != null && Vector3.Dot(base.transform.forward, hit.collider.GetComponent<ObjectInteractiveReqIK>().GetDirection()) > hit.collider.GetComponent<ObjectInteractiveReqIK>().dotLimit)
							{
								objectCastHandInteractive = hit.collider.gameObject;
							}
							else
							{
								objectCastHandInteractive = null;
							}
						}
						else
						{
							objectCastHandInteractive = null;
						}
					}
					else
					{
						objectCastHandInteractive = null;
					}
				}
			}
			if (objectCast != null && objectCast.GetComponent<Trigger_Zoom>() != null && Vector3.Distance(objectCast.transform.position, mainCamera.position) < objectCast.GetComponent<Trigger_Zoom>().distance)
			{
				if (!zoom)
				{
					zoom = true;
					zoomWas = mainCamera.GetComponent<Camera>().fieldOfView;
					mainCamera.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
					timeZoom = 0f;
				}
				if (timeZoom < 1f)
				{
					timeZoom += Time.deltaTime * 2f;
					if (timeZoom > 1f)
					{
						timeZoom = 1f;
					}
					mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(zoomWas, objectCast.GetComponent<Trigger_Zoom>().zoom, animationCameraZoom.Evaluate(timeZoom));
					mainCamera.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
				}
			}
			else
			{
				if (zoom)
				{
					zoom = false;
					zoomWas = mainCamera.GetComponent<Camera>().fieldOfView;
					mainCamera.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
					timeZoom = 0f;
				}
				if (timeZoom < 1f)
				{
					timeZoom += Time.deltaTime * 2f;
					if (timeZoom > 1f)
					{
						timeZoom = 1f;
					}
					mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(zoomWas, GlobalGame.playerFov, animationCameraZoom.Evaluate(timeZoom));
					mainCamera.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
				}
			}
		}
		if (cameraNoise > 0f)
		{
			cameraNoise = Mathf.Lerp(cameraNoise, 0f, Time.deltaTime * 8f);
		}
		if (moveSlow > 1f)
		{
			moveSlow -= Time.deltaTime;
			if (moveSlow < 1f)
			{
				moveSlow = 1f;
			}
		}
		if (timeSitStart > 0f)
		{
			timeSitStart -= Time.deltaTime;
			if (timeSitStart < 0f)
			{
				timeSitStart = 0f;
			}
		}
		if (timeSitStop > 0f)
		{
			timeSitStop -= Time.deltaTime;
			if (timeSitStop < 0f)
			{
				timeSitStop = 0f;
			}
		}
		if (timeCameraFreeze > 0f)
		{
			timeCameraFreeze -= Time.deltaTime;
			if (timeCameraFreeze < 0f)
			{
				timeCameraFreeze = 0f;
			}
		}
		if (blockInteractive > 0)
		{
			blockInteractive--;
		}
		if (timeCameraOtherTargetLerp > 0f)
		{
			timeCameraOtherTargetLerp -= Time.deltaTime;
			if (timeCameraOtherTargetLerp <= 0f)
			{
				timeCameraOtherTargetLerp = 0f;
			}
			if (cameraOtherTarget != null)
			{
				mainCamera.transform.position = Vector3.Lerp(cameraOtherTargetWasPosition, cameraOtherTarget.position, cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
				mainCamera.transform.rotation = Quaternion.Lerp(cameraOtherTargetWasRotation, cameraOtherTarget.rotation, cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
			}
			else if (!animationRun)
			{
				mainCamera.transform.position = Vector3.Lerp(cameraOtherTargetWasPosition, Vector3.zero, cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
				mainCamera.transform.rotation = Quaternion.Lerp(cameraOtherTargetWasRotation, Quaternion.Euler(Vector3.zero), cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
			}
			else
			{
				mainCamera.transform.localPosition = Vector3.Lerp(cameraOtherTargetWasPosition, new Vector3(0f, 0.05f, 0f), cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
				mainCamera.transform.localRotation = Quaternion.Lerp(cameraOtherTargetWasRotation, Quaternion.Euler(Vector3.zero) * Quaternion.Euler(rotYAnim, rotXAnim, 0f), cameraOtherTargetLerp.Evaluate(1f - timeCameraOtherTargetLerp));
			}
		}
		Animation();
		if (volumeRotation > 0f)
		{
			volumeRotation -= Time.deltaTime * 2f;
			if (volumeRotation < 0f)
			{
				volumeRotation = 0f;
			}
		}
		audioRotation.volume = Mathf.Lerp(audioRotation.volume, volumeRotation / 2f, Time.deltaTime * 5f);
		if (!GlobalGame.timeGameplay)
		{
			return;
		}
		GlobalGame.timeS += Time.deltaTime;
		if (GlobalGame.timeS >= 60f)
		{
			GlobalGame.timeS -= 60f;
			GlobalGame.timeM++;
			if (GlobalGame.timeM == 60)
			{
				GlobalGame.timeM = 0;
				GlobalGame.timeH++;
			}
		}
	}

	private void LateUpdate()
	{
		fixHead.SetPositionAndRotation(headPerson.position, headPerson.rotation);
	}

	private void Animation()
	{
		if (timeAnimationAfter > 0f)
		{
			timeAnimationAfter -= Time.deltaTime;
			if (timeAnimationAfter <= 0f)
			{
				timeAnimationAfter = 0f;
			}
		}
		if (animationPosition != null)
		{
			rb.isKinematic = true;
			if (animationBlendTime < 1f)
			{
				animationBlendTime += Time.deltaTime * 4f;
				if (animationBlendTime >= 1f)
				{
					animationBlendTime = 1f;
					mainCamera.transform.parent = fixHead.transform;
					mainCamera.transform.localPosition = new Vector3(0f, 0.05f, 0f);
					mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero) * Quaternion.Euler(rotYAnim, rotXAnim, 0f);
				}
				else
				{
					mainCamera.transform.SetPositionAndRotation(Vector3.Lerp(head.position, fixHead.position + fixHead.up * 0.05f, animationBlend.Evaluate(animationBlendTime)), Quaternion.Lerp(head.rotation, fixHead.rotation, animationBlend.Evaluate(animationBlendTime)));
				}
			}
			else if (cameraOtherTarget == null && timeCameraOtherTargetLerp == 0f)
			{
				if (!GlobalGame.mouseSpeedLerp)
				{
					mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero) * Quaternion.Euler(rotYAnim, rotXAnim, 0f);
				}
				else
				{
					mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, Quaternion.Euler(Vector3.zero) * Quaternion.Euler(rotYAnim, rotXAnim, 0f), Time.deltaTime * 20f);
				}
			}
			base.transform.SetPositionAndRotation(Vector3.Lerp(positionPlayerSave, animationPosition.position, animationBlend.Evaluate(animationBlendTime)), Quaternion.Lerp(rotationPlayerSave, animationPosition.rotation, animationBlend.Evaluate(animationBlendTime)));
			if (timeAnimationNow != -1f)
			{
				timeAnimationNow -= Time.deltaTime;
				if (timeAnimationNow < 0.25f)
				{
					if (animationLoop == null)
					{
						AnimationStop();
					}
					else
					{
						animationLoopRun = true;
						timeAnimationAfter = 0.25f;
						if (!typeAnimation)
						{
							animOver["OtherAnimationA"] = animationLoop;
						}
						else
						{
							animOver["OtherAnimationB"] = animationLoop;
						}
						typeAnimation = !typeAnimation;
						timeAnimationNow = -1f;
						animPerson.SetBool("OtherAnimationType", typeAnimation);
						animPerson.SetTrigger("OtherAnimationNext");
						animationLoop = null;
						eventAnimationStartLoop.Invoke();
					}
				}
				rotXAnim = Mathf.Lerp(rotXAnim, 0f, Time.deltaTime * 5f);
				rotYAnim = Mathf.Lerp(rotYAnim, 0f, Time.deltaTime * 5f);
			}
			else
			{
				if (Time.timeScale > 0f)
				{
					rotXAnim += Input.GetAxis("Mouse X") * (1f + GlobalGame.mouseSpeed);
					rotYAnim -= Input.GetAxis("Mouse Y") * (1f + GlobalGame.mouseSpeed);
					rotXAnim = Mathf.Clamp(rotXAnim, 0f - angleHeadRotateAnimation, angleHeadRotateAnimation);
					rotYAnim = Mathf.Clamp(rotYAnim, (0f - angleHeadRotateAnimation) / 2f, angleHeadRotateAnimation / 2f);
				}
				if (scrAnimationNow.GetComponent<ObjectAnimationPlayerHold>() != null)
				{
					if (scrAnimationNow.GetComponent<ObjectAnimationPlayerHold>().animationTimeHold > 0f)
					{
						animPerson.SetBool("AnimationHold", value: true);
					}
					else
					{
						animPerson.SetBool("AnimationHold", value: false);
					}
					animPerson.SetFloat("HoldTime", scrAnimationNow.GetComponent<ObjectAnimationPlayerHold>().animationTimeHold);
				}
			}
			ikHead.solver.headWeight = Mathf.Lerp(ikHead.solver.headWeight, 0f, Time.deltaTime * 10f);
		}
		else
		{
			rb.isKinematic = false;
			if (animationBlendTime < 1f)
			{
				animationBlendTime += Time.deltaTime * 4f;
				if (animationBlendTime >= 1f)
				{
					AnimationStopFull();
					animationBlendTime = 1f;
					if (stopAnimationPositionHips)
					{
						base.transform.position = stopAPHStopPos;
					}
					base.transform.rotation = Quaternion.Euler(0f, rotX, 0f);
					mainCamera.transform.parent = head.transform;
				}
				else
				{
					mainCamera.rotation = Quaternion.Lerp(mainCamera.rotation, Quaternion.Euler(mainCamera.eulerAngles.x, mainCamera.eulerAngles.y, 0f), animationBlendCamera.Evaluate(animationBlendTime));
					base.transform.rotation = Quaternion.Lerp(rotationPlayerSave, Quaternion.Euler(0f, rotX, 0f), animationBlendCamera.Evaluate(animationBlendTime));
					if (stopAnimationPositionHips)
					{
						base.transform.position = Vector3.Lerp(stopAPHStartPos, stopAPHStopPos, animationBlend.Evaluate(animationBlendTime));
					}
				}
			}
			ikHead.solver.headWeight = Mathf.Lerp(ikHead.solver.headWeight, 1f, Time.deltaTime * 10f);
			if (animationBlendTime == 1f)
			{
				mainCamera.transform.SetLocalPositionAndRotation(Vector3.Lerp(mainCamera.localPosition, Vector3.zero, Time.deltaTime * 5f), Quaternion.Lerp(mainCamera.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 5f));
			}
		}
		if (timeAnimationHandNow > 0f)
		{
			timeAnimationHandNow -= Time.deltaTime;
			if ((double)timeAnimationHandNow < 0.5 && !animationArmsRechangeItem)
			{
				if (animationHandsLoop == null)
				{
					AnimationHandStop();
				}
				else
				{
					if (!animationArmsFace)
					{
						if (!typeAnimationArms)
						{
							animOver["ArmsOtherAnimationA"] = animationHandsLoop;
						}
						else
						{
							animOver["ArmsOtherAnimationB"] = animationHandsLoop;
						}
						typeAnimationArms = !typeAnimationArms;
						animPerson.SetBool("OtherAnimationArmsType", typeAnimationArms);
						animPerson.SetTrigger("OtherAnimationArmsNext");
					}
					else
					{
						if (!typeAnimationArms)
						{
							animArmsFaceOver["ArmsOtherAnimationA"] = animationHandsLoop;
						}
						else
						{
							animArmsFaceOver["ArmsOtherAnimationB"] = animationHandsLoop;
						}
						typeAnimationArms = !typeAnimationArms;
						animArmsFace.SetBool("OtherAnimationArmsType", typeAnimationArms);
						animArmsFace.SetTrigger("OtherAnimationArmsNext");
					}
					timeAnimationHandNow = -1f;
					animationHandsLoop = null;
					eventAnimationHandStartLoop.Invoke();
				}
			}
			if (timeAnimationHandNow < 0f && animationArmsRechangeItem)
			{
				animationArmsRechangeItem = false;
				timeAnimationHandNow = 0f;
			}
		}
		if ((timeAnimationHandNow > 0f || timeAnimationHandNow == -1f) && (!animationArmsFace || animationArmsRechangeItem))
		{
			animPerson.SetLayerWeight(1, Mathf.Lerp(animPerson.GetLayerWeight(1), 1f, Time.deltaTime * 5f));
		}
		else
		{
			animPerson.SetLayerWeight(1, Mathf.Lerp(animPerson.GetLayerWeight(1), 0f, Time.deltaTime * 5f));
		}
		animForward = Mathf.Lerp(animForward, (float)moveV * speedAnim, Time.deltaTime * 5f);
		animRight = Mathf.Lerp(animRight, (float)(-moveH) * speedAnim, Time.deltaTime * 5f);
		animPerson.SetFloat("Forward", animForward);
		animPerson.SetFloat("Right", animRight);
		if (sit == 0)
		{
			if (GlobalGame.headMove)
			{
				if (!faceWall)
				{
					if (moveV >= 0)
					{
						forwardHead = Mathf.Lerp(forwardHead, 0.1f + headOffsetWalkForward * Mathf.Clamp(Mathf.Abs(moveH) + moveV, 0f, 1f), Time.deltaTime * 5f);
					}
					else
					{
						forwardHead = Mathf.Lerp(forwardHead, 0.1f + Mathf.Clamp(Mathf.Abs(moveH) + -moveV, 0.75f, 2f) * 0.05f, Time.deltaTime * 5f);
					}
				}
				else
				{
					forwardHead = Mathf.Lerp(forwardHead, 0.1f, Time.deltaTime * 10f);
				}
			}
			else
			{
				forwardHead = Mathf.Lerp(forwardHead, 0.1f, Time.deltaTime * 10f);
			}
		}
		else if (GlobalGame.headMove)
		{
			forwardHead = Mathf.Lerp(forwardHead, 0.25f + headOffsetWalkForward * (0.4f + (float)Mathf.Abs(moveH) / 4f), Time.deltaTime * 10f);
		}
		else
		{
			forwardHead = Mathf.Lerp(forwardHead, 0.25f + headOffsetWalkForward * 0.4f, Time.deltaTime * 10f);
		}
		if (timeAnimOtherFloat == 0)
		{
			if (animatorFloatOtherA > 0f)
			{
				animatorFloatOtherA -= Time.deltaTime;
				if (animatorFloatOtherA < 0f)
				{
					animatorFloatOtherA = 0f;
				}
			}
			if (animatorFloatOtherB > 0f)
			{
				animatorFloatOtherB -= Time.deltaTime;
				if (animatorFloatOtherB < 0f)
				{
					animatorFloatOtherB = 0f;
				}
			}
			animPerson.SetFloat("OtherAnimationAFloat", animatorFloatOtherA);
			animPerson.SetFloat("OtherAnimationBFloat", animatorFloatOtherB);
		}
		else
		{
			timeAnimOtherFloat--;
		}
	}

	private void FixedUpdate()
	{
		if (!animationRun)
		{
			if (!GlobalGame.mouseSpeedLerp || lookTime > 0f)
			{
				head.localRotation = Quaternion.Euler(rotY + animationHeadRotationX.Evaluate(animationHeadTorationXTime), animationHeadRotationY.Evaluate(animationHeadTorationYTime), 0f);
				base.transform.rotation = Quaternion.Euler(0f, rotX, 0f);
			}
			else
			{
				head.localRotation = Quaternion.Lerp(head.localRotation, Quaternion.Euler(rotY + animationHeadRotationX.Evaluate(animationHeadTorationXTime), animationHeadRotationY.Evaluate(animationHeadTorationYTime), 0f), Time.deltaTime * 30f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, rotX, 0f), Time.deltaTime * 30f);
			}
		}
		if (Physics.Raycast(GetLocalPositionHead() + base.transform.forward * -0.2f, head.forward, out hit, 0.4f, castMove))
		{
			faceWall = true;
		}
		else
		{
			faceWall = false;
		}
		if (Physics.SphereCast(base.transform.position + Vector3.up * 0.3f, 0.2f, -Vector3.up, out hit, 0.6f, castMove))
		{
			if (hit.collider.GetComponent<ObjectMaterial>() != null && hit.collider.GetComponent<ObjectMaterial>().ladder > 0f)
			{
				ladder = true;
				GetComponent<SphereCollider>().material = physMaterialStopLadder;
				base.transform.position = new Vector3(base.transform.position.x, hit.point.y, base.transform.position.z);
				ladderSpeed = hit.collider.GetComponent<ObjectMaterial>().ladder;
			}
			else
			{
				ladder = false;
				GetComponent<SphereCollider>().material = null;
			}
		}
		else
		{
			ladder = false;
			GetComponent<SphereCollider>().material = null;
		}
		if (!ladder && (moveV != 0 || moveH != 0) && Physics.Raycast(base.transform.position + Vector3.up * 0.3f + new Vector3(velocityNormalized.x, 0f, velocityNormalized.z) * 0.3f, -Vector3.up, out hit, 0.3f, castMove) && !Physics.Raycast(base.transform.position + Vector3.up * 0.3f + new Vector3(velocityNormalized.x, 0f, velocityNormalized.z) * 0.3f, -Vector3.up, 0.2f, castMove))
		{
			base.transform.position = new Vector3(base.transform.position.x, hit.point.y, base.transform.position.z);
		}
		velocityNormalized = rb.velocity.normalized;
		if (!ladder)
		{
			rb.velocity += Vector3.Normalize(base.transform.forward * moveV + base.transform.right * moveH) * (Time.fixedDeltaTime * 30f) * speed / moveSlow;
		}
		else
		{
			rb.velocity = Vector3.Normalize(base.transform.forward * moveV + base.transform.right * moveH) * (Time.fixedDeltaTime * 30f) * speed * ladderSpeed / moveSlow;
		}
		velocityLocal = base.transform.InverseTransformDirection(rb.velocity);
	}

	private Vector3 GetLocalPositionHead()
	{
		return GlobalAM.TransformPivot(base.transform, new Vector3(0f, heightHead + animationHeadWalk.Evaluate(animationHeadWalkTime) * 0.025f, forwardHead));
	}

	public void AnimationPlay(AnimationClip _animationStart, AnimationClip _animationLoop, AnimationClip _animationStop, Transform _position, UnityEvent _eventAnimationStartLoop, List<UnityEvent> _eventsPlayer, bool _keepItem, ObjectAnimationPlayer _scranimation)
	{
		bool @bool = animPerson.GetBool("AnimationHold");
		if (animationPosition == null)
		{
			rotYAnim = 0f;
			rotXAnim = 0f;
		}
		animationLoopRun = false;
		if (_animationStart == null && _animationLoop != null)
		{
			_animationStart = _animationLoop;
			animationLoopRun = true;
		}
		angleHeadRotateAnimation = 0f;
		animationLoop = _animationLoop;
		animationStop = _animationStop;
		animationPosition = _position;
		keepItem = _keepItem;
		eventAnimationStartLoop = _eventAnimationStartLoop;
		if (!animationRun)
		{
			animationBlendTime = 0f;
		}
		scrpp.eventsPlayer = _eventsPlayer;
		scrAnimationNow = _scranimation;
		stopAnimationPositionHips = scrAnimationNow.stopPositionHips;
		if (stopAnimationPositionHips)
		{
			stopAPHStartPos = _position.position;
		}
		if (@bool)
		{
			animPerson.SetBool("AnimationHold", value: false);
		}
		if (scrAnimationNow.GetComponent<ObjectAnimationPlayerHold>() != null)
		{
			animOver["OtherAnimationHold"] = scrAnimationNow.GetComponent<ObjectAnimationPlayerHold>().animationHold;
		}
		animationRun = true;
		if (!typeAnimation)
		{
			animOver["OtherAnimationA"] = _animationStart;
			nameStatePlayNow = "AnimationOtherB";
		}
		else
		{
			animOver["OtherAnimationB"] = _animationStart;
			nameStatePlayNow = "AnimationOtherA";
		}
		typeAnimation = !typeAnimation;
		timeAnimationNow = _animationStart.length;
		rotationPlayerSave = base.transform.rotation;
		positionPlayerSave = base.transform.position;
		animPerson.SetBool("OtherAnimation", value: true);
		animPerson.SetBool("OtherAnimationType", typeAnimation);
		if (!@bool && !animationFast)
		{
			animPerson.SetTrigger("OtherAnimationNext");
		}
		ResetCast();
		moveH = 0;
		moveV = 0;
		rb.velocity = Vector3.zero;
		if (animationFast)
		{
			animPerson.ResetTrigger("OtherAnimationNext");
			animationFast = false;
			ikHead.solver.headWeight = 0f;
			rotYAnim = 0f;
			rotXAnim = 0f;
			if (typeAnimation)
			{
				animPerson.Play("AnimationOtherA", 0, 0f);
			}
			else
			{
				animPerson.Play("AnimationOtherB", 0, 0f);
			}
			base.transform.SetPositionAndRotation(animationPosition.position, animationPosition.rotation);
			animationBlendTime = 1f;
			scrppik.IkZero();
			mainCamera.transform.parent = fixHead.transform;
			mainCamera.transform.localPosition = new Vector3(0f, 0.05f, 0f);
			mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero) * Quaternion.Euler(rotYAnim, rotXAnim, 0f);
		}
		if (item != null && scrppik.objectInHand != null && scrppik.objectInHand.GetComponent<FlashLight>() != null)
		{
			scrppik.objectInHand.GetComponent<FlashLight>().TakeLightPlayer();
		}
	}

	public void AnimationPlayStop()
	{
		if (animationStop != null)
		{
			if (!typeAnimation)
			{
				animOver["OtherAnimationA"] = animationStop;
			}
			else
			{
				animOver["OtherAnimationB"] = animationStop;
			}
			typeAnimation = !typeAnimation;
			timeAnimationNow = animationStop.length;
			animPerson.SetBool("OtherAnimationType", typeAnimation);
			animPerson.SetTrigger("OtherAnimationNext");
		}
		else
		{
			AnimationStop();
		}
	}

	public void AnimationStop()
	{
		animationBlendTime = 0f;
		animationLoop = null;
		animationStop = null;
		animationLoopRun = false;
		rotX = mainCamera.eulerAngles.y;
		rotY = mainCamera.eulerAngles.x;
		if (rotY > 180f)
		{
			rotY -= 360f;
		}
		head.localRotation = Quaternion.Euler(rotY, 0f, 0f);
		rotationPlayerSave = base.transform.rotation;
		mainCamera.transform.parent = base.transform.parent;
		if (scrAnimationNow != null && scrAnimationNow.firstEventFinish && !scrAnimationNow.firstEventFinishReady)
		{
			scrAnimationNow.firstEventFinishReady = true;
			scrAnimationNow.eventFinish.Invoke();
		}
		animationPosition = null;
		if (stopAnimationPositionHips && Physics.Raycast(hipsPerson.position, -Vector3.up, out hit, 10f, castMove))
		{
			stopAPHStopPos = new Vector3(hipsPerson.position.x, hit.point.y, hipsPerson.position.z);
		}
		animPerson.SetBool("OtherAnimation", value: false);
	}

	public void AnimationStopFull()
	{
		if (scrAnimationNow != null && !scrAnimationNow.firstEventFinishReady)
		{
			scrAnimationNow.firstEventFinishReady = true;
			scrAnimationNow.eventFinish.Invoke();
		}
		scrAnimationNow = null;
		animationRun = false;
		typeAnimation = false;
		animationHeadWalkTime = 0f;
		animationHeadTorationXTime = 0f;
		animationHeadTorationYTime = 0f;
		rb.velocity = Vector3.zero;
		moveV = 0;
		moveH = 0;
	}

	public void AnimationFloatSet(bool ab, float _float)
	{
		if (ab)
		{
			animatorFloatOtherA = Mathf.Lerp(animatorFloatOtherA, _float, Time.deltaTime * 8f);
			animPerson.SetFloat("OtherAnimationAFloat", animatorFloatOtherA);
		}
		else
		{
			animatorFloatOtherB = Mathf.Lerp(animatorFloatOtherB, _float, Time.deltaTime * 8f);
			animPerson.SetFloat("OtherAnimationBFloat", animatorFloatOtherB);
		}
		timeAnimOtherFloat = 2;
	}

	public void Step()
	{
		foot.FootStep();
	}

	public void AnimationFastStop()
	{
		animPerson.Play("Move", -1, 0f);
		AnimationStop();
		AnimationStopFull();
		animationBlendTime = 1f;
		if (stopAnimationPositionHips)
		{
			base.transform.position = stopAPHStopPos;
		}
		base.transform.rotation = Quaternion.Euler(0f, rotX, 0f);
		mainCamera.transform.parent = head.transform;
	}

	public void AnimationHandPlay(AnimationClip _animationStart, AnimationClip _animationLoop, AnimationClip _animationStop, UnityEvent _eventAnimationStartLoop, List<UnityEvent> _eventsPlayer, bool _face, bool _keepItem)
	{
		animationHandRun = true;
		animationArmsFace = _face;
		animationHandsLoop = _animationLoop;
		animationHandsStop = _animationStop;
		keepItem = _keepItem;
		eventAnimationHandStartLoop = _eventAnimationStartLoop;
		if (!animationArmsFace)
		{
			scrpp.eventsPlayer = _eventsPlayer;
		}
		else
		{
			scrpah.eventsPlayer = _eventsPlayer;
			scrppik.AnimationHandsFace(x: true);
		}
		if (animationArmsFace)
		{
			if (!typeAnimationArms)
			{
				animArmsFaceOver["ArmsOtherAnimationA"] = _animationStart;
			}
			else
			{
				animArmsFaceOver["ArmsOtherAnimationB"] = _animationStart;
			}
		}
		else if (!typeAnimationArms)
		{
			animOver["ArmsOtherAnimationA"] = _animationStart;
		}
		else
		{
			animOver["ArmsOtherAnimationB"] = _animationStart;
		}
		typeAnimationArms = !typeAnimationArms;
		timeAnimationHandNow = _animationStart.length;
		if (animationArmsFace)
		{
			animArmsFace.SetBool("OtherAnimationArms", value: true);
			animArmsFace.SetBool("OtherAnimationArmsType", typeAnimationArms);
			animArmsFace.SetTrigger("OtherAnimationArmsNext");
		}
		else
		{
			animPerson.SetBool("OtherAnimationArms", value: true);
			animPerson.SetBool("OtherAnimationArmsType", typeAnimationArms);
			animPerson.SetTrigger("OtherAnimationArmsNext");
		}
		ResetCast();
	}

	public void AnimationHandPlayStop()
	{
		if (animationHandsStop != null)
		{
			if (animationArmsFace)
			{
				if (!typeAnimationArms)
				{
					animArmsFaceOver["ArmsOtherAnimationA"] = animationHandsStop;
				}
				else
				{
					animArmsFaceOver["ArmsOtherAnimationB"] = animationHandsStop;
				}
			}
			else if (!typeAnimationArms)
			{
				animOver["ArmsOtherAnimationA"] = animationHandsStop;
			}
			else
			{
				animOver["ArmsOtherAnimationB"] = animationHandsStop;
			}
			typeAnimationArms = !typeAnimationArms;
			timeAnimationHandNow = animationStop.length;
			if (animationArmsFace)
			{
				animArmsFace.SetBool("OtherAnimationArmsType", typeAnimationArms);
				animArmsFace.SetTrigger("OtherAnimationArmsNext");
			}
			else
			{
				animPerson.SetBool("OtherAnimationArmsType", typeAnimationArms);
				animPerson.SetTrigger("OtherAnimationArmsNext");
			}
			animationHandsStop = null;
		}
		else
		{
			AnimationHandStop();
		}
	}

	public void AnimationHandStop()
	{
		animationHandRun = false;
		scrppik.AnimationHandsFace(x: false);
		scrppik.UpdateItemInHand();
		timeAnimationHandNow = 0f;
		animationHandsLoop = null;
		animationHandsStop = null;
		animPerson.SetBool("OtherAnimationArms", value: false);
		animArmsFace.SetBool("OtherAnimationArms", value: false);
		if (item != null && !keepItem)
		{
			scrppik.TakeItem(item);
			scrppik.TakeItemInHand();
		}
		keepItem = false;
	}

	public void TeleportPlayer(Vector3 _position)
	{
		if (Physics.Raycast(_position + Vector3.up * 0.5f, -Vector3.up, out hit, 10f, castMove))
		{
			base.transform.position = hit.point;
			TeleportPlayerGeneral();
		}
	}

	public void TeleportPlayer(Vector3 _position, float _rotation)
	{
		if (Physics.Raycast(_position + Vector3.up * 0.5f, -Vector3.up, out hit, 10f, castMove))
		{
			base.transform.position = hit.point;
			rotX = _rotation;
			base.transform.rotation = Quaternion.Euler(0f, _rotation, 0f);
			rb.rotation = Quaternion.Euler(0f, _rotation, 0f);
			TeleportPlayerGeneral();
		}
		else
		{
			Debug.Log("Не удалось телепортироваться.");
		}
	}

	public void TeleportPlayer(Vector3 _position, float _rotation, float _rotationHead)
	{
		if (Physics.Raycast(_position + Vector3.up * 0.5f, -Vector3.up, out hit, 10f, castMove))
		{
			base.transform.position = hit.point;
			rotX = _rotation;
			base.transform.rotation = Quaternion.Euler(0f, _rotation, 0f);
			rb.rotation = Quaternion.Euler(0f, _rotation, 0f);
			rotY = _rotationHead;
			head.localRotation = Quaternion.Euler(_rotationHead, 0f, 0f);
			TeleportPlayerGeneral();
		}
		else
		{
			Debug.Log("Не удалось телепортироваться.");
		}
	}

	private void TeleportPlayerGeneral()
	{
		mainCamera.transform.parent = head.transform;
		mainCamera.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
		rb.velocity = GlobalAM.TransformPivotLocal(base.transform, velocityLocal);
	}

	public void Look(Transform _target, float _time, UnityEvent _event)
	{
		lookTime = _time;
		lookTarget = _target;
		lookEventEnd = _event;
	}

	public void Hide(bool x)
	{
		hide = x;
		if (hide)
		{
			ResetCast();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void SitHold()
	{
		sit = 3;
	}

	private void ResetCast()
	{
		distanceCast = 10f;
		timeCast = 0;
		objectCastInteractive = null;
		objectCast = null;
		objectCastHandInteractive = null;
	}

	public void HideBody(bool x)
	{
		animPerson.transform.Find("Arms").gameObject.SetActive(!x);
		animPerson.transform.Find("Clothes").gameObject.SetActive(!x);
		animPerson.transform.Find("HeadMirror").gameObject.SetActive(!x);
		animPerson.transform.Find("HairMirror").gameObject.SetActive(!x);
	}

	public void SitNeed(bool x)
	{
		needSit = x;
		if (!needSit && animationRun)
		{
			sit = 0;
			timeSitStart = 0f;
			timeSitStop = 0f;
		}
	}

	public void RunNeed(bool x)
	{
		needRun = x;
	}

	public void BlockInteractive()
	{
		blockInteractive = 3;
	}

	public void SlowTimeMove(float _intensity)
	{
		moveSlow = _intensity;
	}

	public void UpdateSettingsCamera()
	{
		if (!zoom)
		{
			mainCamera.GetComponent<Camera>().fieldOfView = GlobalGame.playerFov;
			mainCamera.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
		}
	}

	public void CameraNoise(float x)
	{
		cameraNoise = x;
	}

	public void CameraTargetOther(Transform target, AnimationCurve animationCurve)
	{
		timeCameraOtherTargetLerp = 1f;
		cameraOtherTargetLerp = animationCurve;
		if (target == null)
		{
			if (!animationRun)
			{
				mainCamera.transform.parent = head.transform;
			}
			else
			{
				mainCamera.transform.parent = fixHead.transform;
			}
			cameraOtherTargetWasPosition = mainCamera.transform.localPosition;
			cameraOtherTargetWasRotation = mainCamera.transform.localRotation;
			cameraOtherTarget = null;
		}
		else
		{
			cameraOtherTargetWasPosition = mainCamera.transform.position;
			cameraOtherTargetWasRotation = mainCamera.transform.rotation;
			mainCamera.transform.parent = target;
			cameraOtherTarget = target;
		}
	}

	public void TakeItem(GameObject _item)
	{
		if (_item != null)
		{
			if (!(item != _item))
			{
				return;
			}
			animationArmsFace = false;
			timeAnimationHandNow = 0.3f;
			animationArmsRechangeItem = true;
			hideItem = false;
			if (item != null)
			{
				if (item.GetComponent<ObjectItem>().mainHand.rightHand)
				{
					animPerson.SetTrigger("RightItemRechange");
				}
				else
				{
					animPerson.SetTrigger("LeftItemRechange");
				}
			}
			item = _item;
			scrppik.TakeItem(_item);
			if (item.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				animPerson.SetTrigger("RightItemRechange");
			}
			else
			{
				animPerson.SetTrigger("LeftItemRechange");
			}
		}
		else
		{
			RemoveItem();
		}
	}

	public void RemoveItem()
	{
		timeAnimationHandNow = 0.3f;
		animationArmsRechangeItem = true;
		if (item != null)
		{
			if (item.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				animPerson.SetTrigger("RightItemRechange");
			}
			else
			{
				animPerson.SetTrigger("LeftItemRechange");
			}
		}
		item = null;
		scrppik.RemoveItem();
	}

	public void HideItem(bool x)
	{
		if (hideItem == x)
		{
			return;
		}
		hideItem = x;
		if (!animationHandRun)
		{
			timeAnimationHandNow = 0.3f;
			animationArmsRechangeItem = true;
		}
		if (x)
		{
			if (!animationHandRun)
			{
				if (item.GetComponent<ObjectItem>().mainHand.rightHand)
				{
					animPerson.SetTrigger("RightItemRechange");
				}
				else
				{
					animPerson.SetTrigger("LeftItemRechange");
				}
			}
			scrppik.RemoveItem();
			if (animationHandRun)
			{
				scrppik.TakeItemInHand();
			}
			return;
		}
		scrppik.TakeItem(item);
		if (!animationHandRun)
		{
			if (item.GetComponent<ObjectItem>().mainHand.rightHand)
			{
				animPerson.SetTrigger("RightItemRechange");
			}
			else
			{
				animPerson.SetTrigger("LeftItemRechange");
			}
			if (animationHandRun && !keepItem)
			{
				scrppik.TakeItemInHand();
			}
		}
	}

	public Transform GetBoneRightItem()
	{
		return scrppik.handRightItem;
	}

	public Transform GetBoneLeftItem()
	{
		return scrppik.handLeftItem;
	}
}
