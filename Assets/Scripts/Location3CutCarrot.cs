using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Location3CutCarrot : MonoBehaviour
{
	public float[] timesCut;

	public GameObject[] partsCarrot;

	public AnimationClip animationLength;

	public UnityEvent eventFinish;

	[Header("Интерфейс")]
	public Image imgCase;

	public Image imgPointer;

	[Header("Звуки")]
	public AudioSource audioCut;

	public AudioClip[] soundsCut;

	private int indexCarrotFall;

	private int indexTimeCut;

	private float animationTime;

	private Animator animPlayer;

	private bool play;

	private bool keyDown;

	private string stateName;

	private float timeStart;

	private float animationTimeImage;

	private void Update()
	{
		if (play)
		{
			if (timeStart == 0f)
			{
				if (keyDown)
				{
					if (Input.GetAxis("Vertical") < -0.5f)
					{
						animationTime += Time.deltaTime;
						if (animationTime >= timesCut[indexTimeCut])
						{
							Cut();
						}
					}
				}
				else if (Input.GetAxis("Vertical") > 0.5f)
				{
					animationTime += Time.deltaTime;
					if (animationTime >= timesCut[indexTimeCut])
					{
						Cut();
					}
				}
				animPlayer.Play(stateName, 0, animationTime / animationLength.length);
				if (animationTimeImage < 1f)
				{
					animationTimeImage += Time.deltaTime * 4f;
					if (animationTimeImage > 1f)
					{
						animationTimeImage = 1f;
					}
					imgCase.color = new Color(1f, 1f, 1f, animationTimeImage);
					imgPointer.color = new Color(1f, 1f, 1f, animationTimeImage);
				}
				imgPointer.transform.localScale = Vector3.Lerp(imgPointer.transform.localScale, Vector3.one, Time.deltaTime * 8f);
			}
			else if (timeStart > 0f)
			{
				timeStart -= Time.deltaTime;
				if (timeStart <= 0f)
				{
					timeStart = 0f;
					animationTime = 0.18f;
					animPlayer.speed = 0f;
				}
			}
		}
		else
		{
			if (!(animationTimeImage > 0f))
			{
				return;
			}
			animationTimeImage -= Time.deltaTime * 4f;
			imgCase.color = new Color(1f, 1f, 1f, animationTimeImage);
			imgPointer.color = new Color(1f, 1f, 1f, animationTimeImage);
			if (!(animationTimeImage <= 0f))
			{
				return;
			}
			animationTimeImage = 0f;
			for (int i = 0; i < partsCarrot.Length; i++)
			{
				if (partsCarrot[i] != null)
				{
					Object.Destroy(partsCarrot[i].GetComponent<BoxCollider>());
					Object.Destroy(partsCarrot[i].GetComponent<Rigidbody>());
				}
			}
			Object.Destroy(this);
		}
	}

	private void Cut()
	{
		if (keyDown)
		{
			partsCarrot[indexCarrotFall].transform.parent = base.transform;
			partsCarrot[indexCarrotFall].GetComponent<Rigidbody>().isKinematic = false;
			indexCarrotFall++;
			audioCut.clip = soundsCut[Random.Range(0, soundsCut.Length)];
			audioCut.pitch = Random.Range(0.9f, 1.1f);
			audioCut.Play();
		}
		animationTime = timesCut[indexTimeCut];
		keyDown = !keyDown;
		indexTimeCut++;
		if (indexTimeCut == timesCut.Length - 1)
		{
			play = false;
			animPlayer.speed = 1f;
			eventFinish.Invoke();
		}
		imgPointer.transform.localScale = Vector3.one * 0.8f;
		if (keyDown)
		{
			imgPointer.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		}
		else
		{
			imgPointer.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
		}
	}

	public void CutStart()
	{
		animPlayer = GlobalTag.player.transform.Find("Person").GetComponent<Animator>();
		stateName = GlobalTag.player.GetComponent<PlayerMove>().nameStatePlayNow;
		play = true;
		keyDown = true;
		timeStart = 0.18f;
		imgCase.gameObject.SetActive(value: true);
		imgCase.color = new Color(1f, 1f, 1f, 0f);
		imgPointer.color = new Color(0.9f, 0.2f, 0.2f, 0f);
	}
}
