using UnityEngine;
using UnityEngine.Events;

public class Location34_Communication : MonoBehaviour
{
	public UnityEvent eventStartAddon;

	private float checkNearPosition;

	private Transform playerT;

	private float timeStopNextCheck;

	private bool eventWalkReadyAddonActive;

	private UnityEvent eventWalkReadyAddon;

	private string roomPlayer;

	private bool mitaNeedToPoint;

	private float timeRotateToPlayer;

	private float timeFinishPoint;

	[Header("Точки для Миты")]
	public bool mitaCanWalk;

	public Location34_PositionForMita[] positionsForMita;

	public MitaPerson mita;

	public UnityEvent eventWalkStart;

	public UnityEvent eventWalkReady;

	public AnimationClip mitaAnimationIdle;

	public AnimationClip mitaAnimationWalk;

	public CapsuleCollider capsulePlaceMita;

	private bool interactiveNow;

	[Header("Взаимодействия")]
	public bool onStartActiveAddon;

	public GameObject[] addonInteractive;

	public GameObject[] addonOnStartActive;

	public UnityEvent eventStartInteractive;

	public UnityEvent eventStopInteractive;

	private int indexSwitchAnimation;

	private float timeRandomSwitchAnimation;

	private float timeSwitchAnimation;

	private int playTypeOhterAnimation;

	[Header("Другие анимации")]
	public Animator_FunctionsOverride mitaAnimator;

	public Location34_OtherAnimationWalk[] otherAnimation;

	public AnimationClip[] animationsBehaviour;

	public GameObject[] objectInHandsMita;

	[Header("Настройки")]
	public bool mitaFixPosition = true;

	private bool play;

	private void Start()
	{
		ActivationAddon(onStartActiveAddon);
		playerT = GlobalTag.player.transform;
		if (addonOnStartActive == null || addonOnStartActive.Length == 0)
		{
			return;
		}
		for (int i = 0; i < addonOnStartActive.Length; i++)
		{
			if (addonOnStartActive[i] != null)
			{
				addonOnStartActive[i].GetComponent<ObjectInteractive>().Activation(x: true);
				addonOnStartActive[i].transform.Find("Canvas").GetComponent<UI_LookOnCamera>().Hide(_hide: false);
			}
		}
	}

	private void Update()
	{
		if (mitaCanWalk && !mitaNeedToPoint)
		{
			if (timeStopNextCheck == 0f)
			{
				checkNearPosition += Time.deltaTime;
				if (checkNearPosition > 1f)
				{
					CheckNearPosition();
				}
			}
			else
			{
				timeStopNextCheck -= Time.deltaTime;
				if (timeStopNextCheck < 0f)
				{
					timeStopNextCheck = 0f;
				}
			}
			if (timeRandomSwitchAnimation != -100f && !mita.nma.enabled)
			{
				timeRandomSwitchAnimation += Time.deltaTime;
				if (timeRandomSwitchAnimation > 5f)
				{
					if (indexSwitchAnimation == 0)
					{
						if (Random.Range(0, 3) == 0)
						{
							SwitchOtherAnimation(Random.Range(1, otherAnimation.Length));
							timeRandomSwitchAnimation = Random.Range(-5f, -1f);
						}
						else
						{
							timeRandomSwitchAnimation = Random.Range(-10f, -3f);
							PlayBehaviourAnimation(Random.Range(0, animationsBehaviour.Length));
						}
					}
					else
					{
						timeRandomSwitchAnimation = Random.Range(-10f, -3f);
						SwitchOtherAnimation(0);
					}
				}
			}
		}
		if (Vector3.Distance(mitaAnimator.transform.position, capsulePlaceMita.transform.position) < 1f && capsulePlaceMita.radius < 0.15f)
		{
			if (!capsulePlaceMita.enabled)
			{
				capsulePlaceMita.enabled = true;
			}
			capsulePlaceMita.radius += Time.deltaTime * 0.4f;
			if (capsulePlaceMita.radius > 0.15f)
			{
				capsulePlaceMita.radius = 0.15f;
			}
		}
		if (timeFinishPoint == 0f && !mitaCanWalk && !mita.nma.enabled && indexSwitchAnimation != 0 && timeSwitchAnimation == 0f)
		{
			SwitchOtherAnimation(0);
		}
		if (timeSwitchAnimation > 0f)
		{
			timeSwitchAnimation -= Time.deltaTime;
			if (timeSwitchAnimation <= 0f)
			{
				timeSwitchAnimation = 0f;
				if (playTypeOhterAnimation == 0)
				{
					mitaAnimator.AnimationClipSimpleNext(otherAnimation[indexSwitchAnimation].animationIdle);
					mitaAnimator.AnimationClipWalk(otherAnimation[indexSwitchAnimation].animationWalk);
					if (indexSwitchAnimation == 0)
					{
						for (int i = 0; i < objectInHandsMita.Length; i++)
						{
							if (objectInHandsMita[i] != null)
							{
								objectInHandsMita[i].SetActive(value: false);
							}
						}
					}
				}
				if (playTypeOhterAnimation == 1)
				{
					mita.lookLife.Activation(x: true);
					mitaAnimator.AnimationClipSimpleNext(otherAnimation[0].animationIdle);
					playTypeOhterAnimation = 0;
				}
			}
		}
		if (eventWalkReadyAddonActive && !mita.nma.enabled && timeFinishPoint == 0f && timeSwitchAnimation == 0f)
		{
			eventWalkReadyAddonActive = false;
			eventWalkReadyAddon.Invoke();
			eventWalkReadyAddon = null;
		}
		if (mitaNeedToPoint && !mita.nma.enabled)
		{
			mitaNeedToPoint = false;
		}
		if (timeRotateToPlayer > 0f)
		{
			timeRotateToPlayer -= Time.deltaTime;
			if (timeRotateToPlayer <= 0f)
			{
				timeRotateToPlayer = 0f;
				mita.lookLife.LookRotatePriorityOnPlayer();
			}
		}
		if (timeFinishPoint > 0f)
		{
			timeFinishPoint -= Time.deltaTime;
			if (timeFinishPoint < 0f)
			{
				timeFinishPoint = 0f;
			}
		}
	}

	public void MitaStopPoint()
	{
		mita.lookLife.IKBodyEnable(x: true);
		timeStopNextCheck = Random.Range(5f, 10f);
		timeRotateToPlayer = 0.3f;
		timeFinishPoint = 0.4f;
	}

	public void SwitchAnimationClassic()
	{
		SwitchOtherAnimation(0);
		timeRandomSwitchAnimation = -100f;
	}

	public void ActivationOtherAnimation(bool x)
	{
		if (x)
		{
			timeRandomSwitchAnimation = Random.Range(-30f, -10f);
		}
		else
		{
			timeRandomSwitchAnimation = -100f;
		}
	}

	private void MitaGoPoint(Vector3 _point)
	{
		capsulePlaceMita.gameObject.SetActive(value: true);
		capsulePlaceMita.transform.position = _point;
		capsulePlaceMita.radius = 0f;
		capsulePlaceMita.enabled = false;
	}

	private void CheckNearPosition()
	{
		checkNearPosition = 0f;
		int num = 0;
		float num2 = 1000f;
		for (int i = 0; i < positionsForMita.Length; i++)
		{
			if (roomPlayer == positionsForMita[i].room && Vector3.Distance(positionsForMita[i].target.position, playerT.position) > 0.3f && Vector3.Distance(positionsForMita[i].target.position, playerT.position) < num2)
			{
				num = i;
				num2 = Vector3.Distance(positionsForMita[i].target.position, playerT.position);
			}
		}
		if (Vector3.Distance(positionsForMita[num].target.position, mita.animMita.transform.position) > 1f)
		{
			mita.lookLife.IKBodyEnable(x: false);
			mita.AiWalkToTarget(positionsForMita[num].target, eventWalkReady, otherAnimation[indexSwitchAnimation].animationIdle);
			MitaGoPoint(positionsForMita[num].target.position);
			mita.lookLife.LookOnPlayer();
			eventWalkStart.Invoke();
		}
	}

	private void SwitchOtherAnimation(int _index)
	{
		if (indexSwitchAnimation == _index)
		{
			return;
		}
		if (_index != 0)
		{
			indexSwitchAnimation = _index;
			timeSwitchAnimation = otherAnimation[indexSwitchAnimation].animationSwitchMe.length - 0.2f;
			mitaAnimator.AnimationClipSimpleNext(otherAnimation[indexSwitchAnimation].animationSwitchMe);
			if (otherAnimation[indexSwitchAnimation].offHeadIK)
			{
				mita.lookLife.offIKHead = true;
			}
		}
		else
		{
			timeSwitchAnimation = otherAnimation[indexSwitchAnimation].animationSwitchBack.length - 0.2f;
			mitaAnimator.AnimationClipSimpleNext(otherAnimation[indexSwitchAnimation].animationSwitchBack);
			indexSwitchAnimation = 0;
			mita.lookLife.offIKHead = false;
		}
		timeStopNextCheck += timeSwitchAnimation + 0.4f;
	}

	private void PlayBehaviourAnimation(int _index)
	{
		mita.lookLife.Activation(x: false);
		playTypeOhterAnimation = 1;
		timeSwitchAnimation = animationsBehaviour[_index].length - 0.2f;
		mitaAnimator.AnimationClipSimpleNext(animationsBehaviour[_index]);
	}

	public void DeactiveObjectsAddonAnimationMita()
	{
		for (int i = 0; i < objectInHandsMita.Length; i++)
		{
			if (objectInHandsMita[i] != null)
			{
				objectInHandsMita[i].SetActive(value: false);
			}
		}
	}

	public void PlayerInRoom(string _r)
	{
		roomPlayer = _r;
		timeStopNextCheck = 0.5f;
	}

	public void ActivationAddon(bool x)
	{
		for (int i = 0; i < addonInteractive.Length; i++)
		{
			if (addonInteractive[i] != null)
			{
				addonInteractive[i].SetActive(value: true);
				addonInteractive[i].GetComponent<ObjectInteractive>().Activation(x);
				addonInteractive[i].transform.Find("Canvas").GetComponent<UI_LookOnCamera>().Hide(!x);
			}
		}
	}

	public void ActivationCanWalk(bool x)
	{
		mitaCanWalk = x;
	}

	public void StartAddon()
	{
		play = true;
		eventStartAddon.Invoke();
		if (!interactiveNow)
		{
			ActivationAddon(x: true);
		}
		ActivationCanWalk(x: true);
		if (mitaFixPosition)
		{
			mita.animMita.GetComponent<CapsuleCollider>().enabled = true;
			mita.animMita.GetComponent<Animator_FunctionsOverride>().ResetOrder();
			mita.animMita.GetComponent<Animator_FunctionsOverride>().AnimationClipWalk(mitaAnimationWalk);
			mita.animMita.GetComponent<Animator_FunctionsOverride>().ReAnimationClip("SimpleA", mitaAnimationIdle);
			mita.animMita.GetComponent<Animator_FunctionsOverride>().ReAnimationClip("SimpleB", mitaAnimationIdle);
			mita.lookLife.PriorityLookOnPlayer();
			mita.lookLife.ForwardReTransform(mita.animMita.transform);
			mita.lookLife.ActivationRotateBody(x: true);
			mita.lookLife.IKBodyEnable(x: true);
			mita.MagnetOff();
		}
		CheckNearPosition();
	}

	public void StopAddon()
	{
		ActivationAddon(x: false);
		ActivationCanWalk(x: false);
	}

	public void InteractiveActive(bool x)
	{
		interactiveNow = x;
		if (play)
		{
			ActivationAddon(!x);
			ActivationCanWalk(!x);
			if (x)
			{
				CheckNearPosition();
			}
		}
		else
		{
			for (int i = 0; i < addonOnStartActive.Length; i++)
			{
				if (addonOnStartActive[i] != null)
				{
					addonOnStartActive[i].SetActive(value: true);
					addonOnStartActive[i].GetComponent<ObjectInteractive>().Activation(!x);
					addonOnStartActive[i].transform.Find("Canvas").GetComponent<UI_LookOnCamera>().Hide(x);
				}
			}
		}
		if (x)
		{
			eventStartInteractive.Invoke();
			ConsoleMain.ConsolePrintGame("Interactive Communication: true.");
		}
		else
		{
			eventStopInteractive.Invoke();
			ConsoleMain.ConsolePrintGame("Interactive Communication: false.");
		}
	}

	public void InteractiveActiveWithoutCheckPosition(bool x)
	{
		if (play)
		{
			ActivationAddon(!x);
			ActivationCanWalk(!x);
		}
		else
		{
			for (int i = 0; i < addonOnStartActive.Length; i++)
			{
				if (addonOnStartActive[i] != null)
				{
					addonOnStartActive[i].SetActive(value: true);
					addonOnStartActive[i].GetComponent<ObjectInteractive>().Activation(!x);
					addonOnStartActive[i].transform.Find("Canvas").GetComponent<UI_LookOnCamera>().Hide(x);
				}
			}
		}
		if (x)
		{
			eventStartInteractive.Invoke();
			ConsoleMain.ConsolePrintGame("Interactive Communication: true.");
		}
		else
		{
			eventStopInteractive.Invoke();
			ConsoleMain.ConsolePrintGame("Interactive Communication: false.");
		}
	}

	public void MitaWalkToPoint(Transform _position)
	{
		if (Vector3.Distance(_position.position, mita.animMita.transform.position) > 1f)
		{
			mitaNeedToPoint = true;
			mita.AiWalkToTarget(_position, eventWalkReady, otherAnimation[indexSwitchAnimation].animationIdle);
			MitaGoPoint(_position.position);
			mita.lookLife.LookOnPlayer();
			eventWalkStart.Invoke();
		}
	}

	public void TakeEventWhenReadyWalk(Events_Data _event)
	{
		eventWalkReadyAddonActive = true;
		eventWalkReadyAddon = _event._event[0];
	}

	public void TimeAddOtherAnimation(Time_Events _timeEvent)
	{
		if (indexSwitchAnimation != 0)
		{
			for (int i = 0; i < _timeEvent.EventsOnTime.Length; i++)
			{
				_timeEvent.EventsOnTime[i].time += otherAnimation[indexSwitchAnimation].animationSwitchBack.length + 0.2f;
			}
			_timeEvent.YieldRestart();
		}
		else
		{
			timeSwitchAnimation = 0f;
			_timeEvent.EventsOnTime[0]._event.Invoke();
		}
	}
}
