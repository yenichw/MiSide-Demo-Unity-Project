using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MitaPerson : MonoBehaviour
{
	public Animator animMita;

	public NavMeshAgent nma;

	public Character_Look lookLife;

	private float alphaFaceLayer;

	private int indexTextureFaceLayer;

	private int indexTextureFaceLayerNow;

	[Header("Лицевая маска")]
	public SkinnedMeshRenderer faceLayer;

	public int startLayerFace;

	[Header("Hands")]
	public Transform boneRightItem;

	public Transform boneLeftItem;

	public Transform fixedRightItem;

	public Transform fixedLeftItem;

	private Transform magnetTarget;

	private bool canRotate;

	private AnimationClip animIdleAfter;

	private Transform targetMoveTo;

	private UnityEvent eventFinish;

	private float timeRotate;

	private Transform tTarget;

	private float nmaSpeed;

	[Header("AI")]
	public float distanceTargetMoveFinish = 0.25f;

	private bool faceEmotion;

	[Header("Эмоция")]
	public string startEmotion;

	private void Start()
	{
		nmaSpeed = nma.speed / 8f;
		if (faceLayer != null)
		{
			if (startLayerFace == 0)
			{
				faceLayer.gameObject.SetActive(value: false);
				alphaFaceLayer = -1f;
				faceLayer.material.SetFloat("_AlphaMod", -1f);
			}
			else
			{
				alphaFaceLayer = 0f;
				faceLayer.material.SetFloat("_AlphaMod", 0f);
				faceLayer.gameObject.SetActive(value: true);
				indexTextureFaceLayerNow = startLayerFace;
				indexTextureFaceLayer = startLayerFace;
				faceLayer.material.SetTexture("_MainTex", (Resources.Load("Mita/FaceLayer") as GameObject).GetComponent<DataValues_Texture>().textures2d[indexTextureFaceLayer]);
			}
		}
		if (startEmotion != null && startEmotion != "")
		{
			FaceEmotion(startEmotion);
		}
	}

	private void Update()
	{
		if (!faceEmotion)
		{
			if (animMita.layerCount > 2 && animMita.GetLayerWeight(2) > 0f)
			{
				animMita.SetLayerWeight(2, animMita.GetLayerWeight(2) - Time.deltaTime * 3f);
				if (animMita.GetLayerWeight(2) < 0f)
				{
					animMita.SetLayerWeight(2, 0f);
				}
			}
		}
		else if (animMita.layerCount > 2 && animMita.GetLayerWeight(2) < 1f)
		{
			animMita.SetLayerWeight(2, animMita.GetLayerWeight(2) + Time.deltaTime * 3f);
			if (animMita.GetLayerWeight(2) > 1f)
			{
				animMita.SetLayerWeight(2, 1f);
			}
		}
		if (nma.enabled)
		{
			if (tTarget != null)
			{
				nma.SetDestination(tTarget.position);
			}
			if (Vector3.Distance(nma.transform.position, nma.destination) < distanceTargetMoveFinish)
			{
				lookLife.ActivationRotateBody(canRotate);
				nma.enabled = false;
				if (eventFinish != null)
				{
					eventFinish.Invoke();
				}
				if (animIdleAfter != null)
				{
					ConsoleMain.ConsolePrintGame("Idle after walk");
					animMita.GetComponent<Animator_FunctionsOverride>().AnimationIdlePlayAfterWalk(animIdleAfter);
					animIdleAfter = null;
				}
				else
				{
					animMita.SetBool("Walk", value: false);
					animMita.SetTrigger("NextLerp");
				}
				if (targetMoveTo != null)
				{
					if (targetMoveTo.GetComponent<MitaAIMovePoint>() != null && targetMoveTo.GetComponent<MitaAIMovePoint>().magnetAfter)
					{
						MagnetToTarget(targetMoveTo);
					}
					targetMoveTo = null;
				}
			}
			else if (nma.path.corners != null && nma.path.corners.Length >= 2)
			{
				nma.transform.position = Vector3.MoveTowards(nma.transform.position, nma.path.corners[1], Time.deltaTime * nmaSpeed);
				if (GlobalAM.DirectionFloor(nma.transform.position, nma.path.corners[1]) != Vector3.zero)
				{
					nma.transform.rotation = Quaternion.Lerp(nma.transform.rotation, Quaternion.LookRotation(GlobalAM.DirectionFloor(nma.transform.position, nma.path.corners[1]), Vector3.up), Time.deltaTime * 10f);
				}
			}
		}
		if (timeRotate > 0f)
		{
			timeRotate -= Time.deltaTime;
			if (timeRotate <= 0f)
			{
				timeRotate = 0f;
				AiWalkToTarget(targetMoveTo);
			}
		}
		if (magnetTarget != null)
		{
			animMita.transform.position = Vector3.Lerp(animMita.transform.position, magnetTarget.position, Time.deltaTime * 5f);
			animMita.transform.rotation = Quaternion.Lerp(animMita.transform.rotation, magnetTarget.rotation, Time.deltaTime * 5f);
		}
		if (!(faceLayer != null))
		{
			return;
		}
		if (indexTextureFaceLayerNow != indexTextureFaceLayer)
		{
			alphaFaceLayer -= Time.deltaTime * 3f;
			if (alphaFaceLayer <= -1f)
			{
				alphaFaceLayer = -1f;
				indexTextureFaceLayerNow = indexTextureFaceLayer;
				if (indexTextureFaceLayer != 0)
				{
					faceLayer.gameObject.SetActive(value: true);
					faceLayer.material.SetTexture("_MainTex", (Resources.Load("Mita/FaceLayer") as GameObject).GetComponent<DataValues_Texture>().textures2d[indexTextureFaceLayer]);
				}
				else
				{
					faceLayer.gameObject.SetActive(value: false);
				}
			}
			faceLayer.material.SetFloat("_AlphaMod", alphaFaceLayer);
		}
		else if (indexTextureFaceLayerNow != 0 && alphaFaceLayer < 0f)
		{
			alphaFaceLayer += Time.deltaTime * 3f;
			if (alphaFaceLayer > 0f)
			{
				alphaFaceLayer = 0f;
			}
			faceLayer.material.SetFloat("_AlphaMod", alphaFaceLayer);
		}
	}

	private void LateUpdate()
	{
		if (fixedRightItem != null)
		{
			fixedRightItem.SetPositionAndRotation(boneRightItem.position, boneRightItem.rotation);
		}
		if (fixedLeftItem != null)
		{
			fixedLeftItem.SetPositionAndRotation(boneLeftItem.position, boneLeftItem.rotation);
		}
	}

	public void MagnetToTarget(Transform _target)
	{
		magnetTarget = _target;
	}

	public void MagnetOff()
	{
		magnetTarget = null;
	}

	public void AiWalkToTarget(Transform _target, UnityEvent _eventFinish)
	{
		animIdleAfter = null;
		eventFinish = _eventFinish;
		AiWalkToTarget(_target);
	}

	public void AiWalkToTarget(Transform _target, UnityEvent _eventFinish, AnimationClip _animIdleAfter)
	{
		animIdleAfter = _animIdleAfter;
		eventFinish = _eventFinish;
		AiWalkToTarget(_target);
	}

	public void AiWalkToTargetTranform(Transform _target, UnityEvent _eventFinish)
	{
		nma.enabled = true;
		nma.SetDestination(_target.position);
		tTarget = _target;
		eventFinish = _eventFinish;
		animMita.SetBool("Walk", value: true);
		animIdleAfter = null;
	}

	private void AiWalkToTarget(Transform _target)
	{
		if (!nma.enabled)
		{
			canRotate = lookLife.canRotateBody;
			lookLife.ActivationRotateBody(x: false);
		}
		if (_target != null)
		{
			targetMoveTo = _target;
			tTarget = null;
			nma.enabled = true;
			nma.SetDestination(_target.position);
			animMita.SetBool("Walk", value: true);
		}
	}

	public void AiWalkToTargetRotate(Transform _target, UnityEvent _eventFinish)
	{
		animIdleAfter = null;
		eventFinish = _eventFinish;
		targetMoveTo = _target;
		lookLife.IKBodyEnable(x: false);
		lookLife.ActivationRotateBody(x: true);
		lookLife.RotateOnTarget(_target);
		timeRotate = 0.75f;
	}

	public void AiSpeedNavigation(float x)
	{
		nma.speed = x;
	}

	public void AiShraplyStop()
	{
		nma.enabled = false;
		if (animMita.GetBool("Walk"))
		{
			animMita.SetTrigger("NextLerp");
		}
		animMita.SetBool("Walk", value: false);
		targetMoveTo = null;
	}

	public void FaceLayer(int _index)
	{
		indexTextureFaceLayer = _index;
	}

	public void FaceLayerFast(int _index)
	{
		if (startLayerFace == _index)
		{
			faceLayer.gameObject.SetActive(value: false);
			indexTextureFaceLayerNow = 0;
			indexTextureFaceLayer = 0;
			alphaFaceLayer = -1f;
			faceLayer.material.SetFloat("_AlphaMod", -1f);
		}
		else
		{
			faceLayer.gameObject.SetActive(value: true);
			indexTextureFaceLayerNow = _index;
			indexTextureFaceLayer = _index;
			faceLayer.material.SetTexture("_MainTex", (Resources.Load("Mita/FaceLayer") as GameObject).GetComponent<DataValues_Texture>().textures2d[_index]);
			alphaFaceLayer = 0f;
			faceLayer.material.SetFloat("_AlphaMod", 0f);
		}
	}

	public void FaceEmotion(string _name)
	{
		_name = _name.ToLower();
		faceEmotion = true;
		if (_name == "smile")
		{
			animMita.SetInteger("FaceEmotion", 0);
		}
		if (_name == "quest")
		{
			animMita.SetInteger("FaceEmotion", 1);
		}
		if (_name == "smileteeth")
		{
			animMita.SetInteger("FaceEmotion", 2);
		}
		if (_name == "sad")
		{
			animMita.SetInteger("FaceEmotion", 3);
		}
		if (_name == "emptiness")
		{
			animMita.SetInteger("FaceEmotion", 4);
		}
		if (_name == "angry")
		{
			animMita.SetInteger("FaceEmotion", 5);
		}
		if (_name == "smilestrange")
		{
			animMita.SetInteger("FaceEmotion", 6);
		}
		if (_name == "shy")
		{
			animMita.SetInteger("FaceEmotion", 7);
		}
		if (_name == "smileobvi")
		{
			animMita.SetInteger("FaceEmotion", 8);
		}
		if (_name == "smiletonque")
		{
			animMita.SetInteger("FaceEmotion", 9);
		}
		if (_name == "smilecringe")
		{
			animMita.SetInteger("FaceEmotion", 10);
		}
		if (_name == "surprise")
		{
			animMita.SetInteger("FaceEmotion", 11);
		}
		animMita.SetTrigger("FaceEmotionNext");
	}

	public void FaceEmotionFast(string _name)
	{
		faceEmotion = true;
		animMita.Play(_name, 2, 0f);
		animMita.SetLayerWeight(2, 1f);
	}

	public void FaceEmotionOff()
	{
		faceEmotion = false;
	}

	public void FaceEmotionFastOff()
	{
		faceEmotion = false;
		animMita.Play("None", 2, 0f);
		animMita.SetLayerWeight(2, 0f);
	}
}
