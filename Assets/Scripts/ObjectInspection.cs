using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ObjectAnimationPlayer))]
public class ObjectInspection : MonoBehaviour
{
	public GameObject objectInspection;

	public AnimationClip animationStart;

	public ObjectInspectionVariant variantA;

	public ObjectInspectionVariant variantB;

	public UnityEvent eventsStopInspection;

	private bool activeNow;

	[HideInInspector]
	public bool variantNow;

	private bool stopedNow;

	private float timeAnimationNext;

	private ObjectAnimationPlayer scroap;

	private PlayerPerson scrpp;

	private PlayerPersonIK scrppik;

	private GameController scrgc;

	private void Start()
	{
		scroap = GetComponent<ObjectAnimationPlayer>();
		scrgc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		scrpp = GameObject.FindWithTag("Player").transform.Find("Person").GetComponent<PlayerPerson>();
		scrppik = scrpp.GetComponent<PlayerPersonIK>();
	}

	private void Update()
	{
		if (timeAnimationNext > 0f)
		{
			timeAnimationNext -= Time.deltaTime;
			if (timeAnimationNext <= 0f)
			{
				timeAnimationNext = 0f;
				if (!variantNow)
				{
					if (variantA.rightHand)
					{
						variantA.rotationWas = scrppik.rightWristFixPosition.eulerAngles;
					}
					else
					{
						variantA.rotationWas = scrppik.leftWristFixPosition.eulerAngles;
					}
					variantA.rotationNow = Vector3.zero;
				}
				else
				{
					if (variantB.rightHand)
					{
						variantB.rotationWas = scrppik.rightWristFixPosition.eulerAngles;
					}
					else
					{
						variantB.rotationWas = scrppik.leftWristFixPosition.eulerAngles;
					}
					variantB.rotationNow = Vector3.zero;
				}
				if (stopedNow)
				{
					stopedNow = false;
					eventsStopInspection.Invoke();
				}
				else
				{
					scrppik.StartInpection(this);
				}
			}
		}
		if (timeAnimationNext != 0f || !activeNow)
		{
			return;
		}
		if (Input.GetButtonDown("Interactive"))
		{
			StopInspection();
		}
		if (variantB.animationLoop != null && Input.GetButtonDown("Cancel"))
		{
			RotateStart();
		}
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (!variantNow)
		{
			variantA.rotationNow += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
			if (variantA.rotationNow.x > variantA.rotationMax.x)
			{
				variantA.rotationNow = new Vector3(variantA.rotationMax.x, variantA.rotationNow.y, 0f);
			}
			if (variantA.rotationNow.x < variantA.rotationMin.x)
			{
				variantA.rotationNow = new Vector3(variantA.rotationMin.x, variantA.rotationNow.y, 0f);
			}
			if (variantA.rotationNow.y > variantA.rotationMax.y)
			{
				variantA.rotationNow = new Vector3(variantA.rotationNow.x, variantA.rotationMax.y, 0f);
			}
			if (variantA.rotationNow.y < variantA.rotationMin.y)
			{
				variantA.rotationNow = new Vector3(variantA.rotationNow.x, variantA.rotationMin.y, 0f);
			}
		}
		else
		{
			variantB.rotationNow += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
			if (variantB.rotationNow.x > variantB.rotationMax.x)
			{
				variantB.rotationNow = new Vector3(variantB.rotationMax.x, variantB.rotationNow.y, 0f);
			}
			if (variantB.rotationNow.x < variantB.rotationMin.x)
			{
				variantB.rotationNow = new Vector3(variantB.rotationMin.x, variantB.rotationNow.y, 0f);
			}
			if (variantB.rotationNow.y > variantB.rotationMax.y)
			{
				variantB.rotationNow = new Vector3(variantB.rotationNow.x, variantB.rotationMax.y, 0f);
			}
			if (variantB.rotationNow.y < variantB.rotationMin.y)
			{
				variantB.rotationNow = new Vector3(variantB.rotationNow.x, variantB.rotationMin.y, 0f);
			}
		}
	}

	public void StartIspection()
	{
		scrppik.StopInspection();
		scroap.animationStart = animationStart;
		scroap.animationLoop = variantA.animationLoop;
		scroap.animationStop = variantA.animationStop;
		scroap.AnimationPlay();
		timeAnimationNext = animationStart.length;
		variantNow = false;
		activeNow = true;
		stopedNow = false;
		scrpp.objectInspection = this;
		scroap.animationStart = variantA.animationRechange;
	}

	public void StopInspection()
	{
		scroap.AnimationStop();
		stopedNow = true;
		if (!variantNow)
		{
			timeAnimationNext = variantA.animationStop.length;
		}
		else
		{
			timeAnimationNext = variantB.animationStop.length;
		}
		scrppik.StopInspection();
	}

	public void RotateStart()
	{
		scrppik.StopInspection();
		variantNow = !variantNow;
		if (!variantNow)
		{
			scroap.animationStart = variantB.animationRechange;
			timeAnimationNext = variantA.animationRechange.length;
			scroap.animationLoop = variantA.animationLoop;
			scroap.animationStop = variantA.animationStop;
		}
		else
		{
			scroap.animationStart = variantA.animationRechange;
			timeAnimationNext = variantB.animationRechange.length;
			scroap.animationLoop = variantB.animationLoop;
			scroap.animationStop = variantB.animationStop;
		}
		scroap.AnimationPlay();
	}

	public void TakeObject(int _index)
	{
		objectInspection.GetComponent<ObjectInHand>().TakeInHand(_index);
	}

	public void DropObject()
	{
		objectInspection.GetComponent<ObjectInHand>().DropOrigin();
	}
}
