using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPerson : MonoBehaviour
{
	[HideInInspector]
	public List<UnityEvent> eventsPlayer;

	[HideInInspector]
	public ObjectInspection objectInspection;

	private Animator anim;

	private bool blinkPlay;

	private float timeBlink;

	private float blinkTimeAnimation;

	private float timeEyesTarget;

	[Header("Глаза")]
	public AnimationCurve animationBlink;

	public SkinnedMeshRenderer meshHead;

	public Transform ikEyesTarget;

	private Color setColor;

	private float timeReColor;

	[Header("Цвет outline")]
	public SkinnedMeshRenderer[] meshes;

	private void Start()
	{
		anim = GetComponent<Animator>();
		blinkPlay = true;
		timeBlink = Random.Range(0.3f, 10f);
		timeEyesTarget = Random.Range(0.3f, 1f);
		blinkTimeAnimation = 1f;
		ikEyesTarget.localPosition = new Vector3(0f, 0f, 5f);
	}

	private void Update()
	{
		if (meshHead.isVisible)
		{
			if (blinkPlay)
			{
				timeBlink -= Time.deltaTime;
				if (timeBlink < 0f)
				{
					timeBlink = 0f;
					timeBlink = Random.Range(0.2f, 8f);
					blinkTimeAnimation = 0f;
				}
			}
			if (blinkTimeAnimation < 1f)
			{
				blinkTimeAnimation += Time.deltaTime * 8f;
				if (blinkTimeAnimation > 1f)
				{
					blinkTimeAnimation = 1f;
				}
				meshHead.SetBlendShapeWeight(0, animationBlink.Evaluate(blinkTimeAnimation) * 100f);
			}
			timeEyesTarget -= Time.deltaTime;
			if (timeEyesTarget < 0f)
			{
				timeEyesTarget = Random.Range(0.3f, 1f);
				if (Random.Range(0, 20) < 7)
				{
					ikEyesTarget.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), 5f);
				}
				else
				{
					ikEyesTarget.localPosition = new Vector3(0f, 0f, 5f);
				}
			}
		}
		if (timeReColor > 0f)
		{
			timeReColor -= Time.deltaTime;
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].material.SetColor("_LineColor", Color.Lerp(meshes[i].material.GetColor("_LineColor"), setColor, Time.deltaTime * 5f));
			}
		}
	}

	public void BlinkPlay(bool x)
	{
		blinkPlay = x;
	}

	public void EventKey(int _x)
	{
		eventsPlayer[_x].Invoke();
		ConsoleMain.ConsolePrintGame("PlayerPerson.NewEvent(" + _x + ")");
	}

	public void NewEvent(int _x)
	{
		eventsPlayer[_x].Invoke();
		ConsoleMain.ConsolePrintGame("PlayerPerson.NewEvent(" + _x + ")");
	}

	public void TakeObj(int _x)
	{
		objectInspection.TakeObject(_x);
	}

	public void DropObj()
	{
		objectInspection.DropObject();
	}

	public void StepMove()
	{
		if (Mathf.Abs(anim.GetFloat("Forward")) > 0.1f || Mathf.Abs(anim.GetFloat("Right")) > 0.1f)
		{
			base.transform.parent.GetComponent<PlayerMove>().Step();
		}
	}

	public void Step()
	{
		base.transform.parent.GetComponent<PlayerMove>().Step();
	}

	public void SetColorOutline(Color _color)
	{
		setColor = _color;
		timeReColor = 3f;
	}
}
