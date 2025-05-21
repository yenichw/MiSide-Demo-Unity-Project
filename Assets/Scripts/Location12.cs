using UnityEngine;
using UnityEngine.Events;

public class Location12 : MonoBehaviour
{
	private Transform playerT;

	private float timeCanTP;

	[Header("Убийца Мита")]
	public MitaPerson scrMitaCreepy;

	public UnityEvent eventNearMita;

	public ObjectAnimationPlayer animationPlayerKill;

	public UnityEvent eventResetHunter;

	public Player_Teleport scrPlayerTeleportReset;

	[Header("Уродливая Мита")]
	public GameObject creepyFreak;

	public LayerMask layerWallFreak;

	public UnityEvent eventTPFreakStart;

	private int questCount;

	[Header("Вопросы Мите")]
	public UnityEvent eventQuest1;

	public UnityEvent eventQuest2;

	public UnityEvent eventQuest3;

	public UnityEvent eventQuestsFinish;

	public UnityEvent eventQuestsPreFinish;

	private void Start()
	{
		timeCanTP = -1f;
		playerT = GlobalTag.player.transform;
	}

	private void Update()
	{
		if (timeCanTP > 0f)
		{
			timeCanTP -= Time.deltaTime;
			if (timeCanTP <= 0f)
			{
				timeCanTP = 0f;
			}
		}
	}

	private void FixedUpdate()
	{
		if (timeCanTP == 0f && !Physics.CapsuleCast(playerT.position + Vector3.up * 0.25f, playerT.position + Vector3.up * 0.5f, 0.15f, -playerT.forward, 0.75f, layerWallFreak))
		{
			timeCanTP = -1f;
			playerT.GetComponent<PlayerMove>().dontMove = true;
			playerT.GetComponent<PlayerMove>().stopMouseMove = true;
			eventTPFreakStart.Invoke();
		}
	}

	public void CanTP()
	{
		timeCanTP = 1f;
	}

	public void FreakTP()
	{
		creepyFreak.gameObject.SetActive(value: true);
		creepyFreak.transform.SetPositionAndRotation(playerT.position - playerT.forward, playerT.rotation);
		playerT.GetComponent<PlayerMove>().dontMove = false;
		playerT.GetComponent<PlayerMove>().stopMouseMove = false;
	}

	public void Quest()
	{
		questCount++;
		if (questCount == 1)
		{
			eventQuest1.Invoke();
		}
		if (questCount == 2)
		{
			eventQuest2.Invoke();
		}
		if (questCount == 3)
		{
			eventQuest3.Invoke();
		}
	}

	public void QuestFinish()
	{
		if (questCount == 3)
		{
			eventQuestsFinish.Invoke();
		}
		else
		{
			eventQuestsPreFinish.Invoke();
		}
	}

	public void CreepyMitaStayAttack()
	{
		scrMitaCreepy.AiWalkToTargetTranform(playerT, eventNearMita);
	}

	public void CreepyMitaAttack()
	{
		animationPlayerKill.transform.SetPositionAndRotation(scrMitaCreepy.animMita.transform.position + scrMitaCreepy.animMita.transform.forward, Quaternion.LookRotation(scrMitaCreepy.animMita.transform.position - playerT.position, Vector3.up));
		animationPlayerKill.AnimationPlayFast();
	}

	public void ResetHunter()
	{
		eventResetHunter.Invoke();
		scrMitaCreepy.AiWalkToTargetTranform(playerT, eventNearMita);
		scrPlayerTeleportReset.Click();
	}

	public void StopHunter()
	{
		scrMitaCreepy.AiWalkToTargetTranform(playerT, null);
	}
}
