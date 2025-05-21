using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class Location6_MitaKiller : MonoBehaviour
{
	public PlayableGraph m_Graph;

	public Transform[] positionsAnimationKill;

	private float timeLose;

	private bool killNow;

	[Header("Kill")]
	public ObjectAnimationPlayer animationKill;

	public UnityEvent eventKill;

	public UnityEvent eventRestart;

	public UnityEvent eventStart;

	public Transform mita;

	public Player_Teleport tpRestart;

	[Header("Game")]
	public Trigger_Event[] triggers;

	public Trigger_Event[] triggersDanger;

	public WorldPlayer worldPlayer;

	private Transform player;

	private PlayableDirector cutScene;

	private bool playerExit;

	private AudioSource audioLook;

	private void Start()
	{
		cutScene = GetComponent<PlayableDirector>();
		player = GlobalTag.player.transform;
		audioLook = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (timeLose > 0f && !playerExit)
		{
			timeLose -= Time.deltaTime;
			if (timeLose <= 0f)
			{
				bool flag = false;
				for (int i = 0; i < triggersDanger.Length; i++)
				{
					if (triggersDanger[i].triggerActive)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Kill();
				}
				else
				{
					worldPlayer.CameraVignetteActive(_x: false);
				}
			}
			if ((double)audioLook.volume < 0.4)
			{
				audioLook.volume += Time.deltaTime;
				if (audioLook.volume > 0.4f)
				{
					audioLook.volume = 0.4f;
				}
			}
		}
		else if (audioLook.volume > 0f)
		{
			audioLook.volume -= Time.deltaTime;
			if (audioLook.volume < 0f)
			{
				audioLook.volume = 0f;
			}
		}
	}

	public void Kill()
	{
		if (playerExit || killNow)
		{
			return;
		}
		worldPlayer.CameraVignetteActive(_x: false);
		killNow = true;
		cutScene.Stop();
		for (int i = 0; i < triggersDanger.Length; i++)
		{
			triggersDanger[i].gameObject.SetActive(value: false);
		}
		float num = 1000f;
		int num2 = -1;
		for (int j = 0; j < positionsAnimationKill.Length; j++)
		{
			if (Vector3.Distance(player.position, positionsAnimationKill[j].position) < num)
			{
				num2 = j;
				num = Vector3.Distance(player.position, positionsAnimationKill[j].position);
			}
		}
		animationKill.transform.SetPositionAndRotation(positionsAnimationKill[num2].position, positionsAnimationKill[num2].rotation);
		mita.transform.SetPositionAndRotation(positionsAnimationKill[num2].position + positionsAnimationKill[num2].forward, positionsAnimationKill[num2].rotation * Quaternion.Euler(0f, 180f, 0f));
		worldPlayer.PlayerCameraNoise(3f);
		worldPlayer.CameraGlitch(_x: true);
		animationKill.AnimationPlay();
		worldPlayer.PlayerNeedSit(x: false);
		eventKill.Invoke();
	}

	public void RestartGame()
	{
		killNow = false;
		mita.position = Vector3.one * 100f;
		worldPlayer.CameraVignetteActive(_x: false);
		worldPlayer.CameraGlitch(_x: false);
		timeLose = 0f;
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].Restart();
			triggers[i].gameObject.SetActive(value: true);
		}
		for (int j = 0; j < triggersDanger.Length; j++)
		{
			triggersDanger[j].Restart();
			triggersDanger[j].gameObject.SetActive(value: false);
		}
		tpRestart.Click();
		eventRestart.Invoke();
	}

	public void PlayerLose()
	{
		if (timeLose <= 0f)
		{
			timeLose = 1.5f;
		}
		worldPlayer.CameraVignetteActive(_x: true);
	}

	public void StartAttack()
	{
		cutScene.Play();
		for (int i = 0; i < triggersDanger.Length; i++)
		{
			triggersDanger[i].gameObject.SetActive(value: true);
			triggersDanger[i].Restart();
		}
		mita.localPosition = Vector3.zero;
		mita.localRotation = Quaternion.Euler(0f, 0f, 0f);
		eventStart.Invoke();
	}

	public void PlayerExit()
	{
		playerExit = true;
		worldPlayer.CameraVignetteActive(_x: false);
	}
}
