using EPOOutline;
using UnityEngine;

public class TamagotchiGame_Cartridge_Cartridge : MonoBehaviour
{
	public TamagotchiGame_Cartridge main;

	private int countBolt;

	[Header("Детали")]
	public GameObject bolt1;

	public GameObject bolt2;

	public GameObject bolt3;

	public GameObject bolt4;

	public GameObject up;

	public GameObject down;

	public GameObject plan;

	private bool clear;

	private float timeAnimation;

	private GameObject nextDetailOpen;

	private float timeAnimationReady;

	[Header("Анимация")]
	public AnimationCurve curveUp;

	public TamagotchiGame_CartridgeDetails[] details;

	public Transform pointerHint;

	public AnimationCurve animationPointerHint;

	private float timeAnimationPointer;

	private bool fall;

	private bool soundTrash;

	[Header("Audio")]
	public AudioClip[] soundsFall;

	private void Update()
	{
		if (timeAnimation > 0f)
		{
			timeAnimation -= Time.deltaTime;
			if (timeAnimation < 0f)
			{
				timeAnimation = 0f;
				if (nextDetailOpen != null)
				{
					nextDetailOpen.GetComponent<Trigger_MouseClick>().enabled = true;
				}
			}
		}
		if (timeAnimationReady > 0f)
		{
			timeAnimationReady -= Time.deltaTime;
			if (timeAnimationReady <= 0f)
			{
				timeAnimationReady = 0f;
				main.CasseteReady(base.gameObject);
			}
		}
		if (clear)
		{
			for (int i = 0; i < details.Length; i++)
			{
				if (details[i].timeStop > 0f)
				{
					details[i].timeStop -= Time.deltaTime;
					if (details[i].timeStop < 0f)
					{
						details[i].positionOrigin = details[i].objectDetail.position;
						details[i].timeStop = 0f;
						if (details[i].objectDetail.GetComponent<Animator>() != null)
						{
							details[i].objectDetail.GetComponent<Animator>().enabled = false;
						}
					}
					continue;
				}
				if (details[i].curveTimeMove > 0.5f && !soundTrash)
				{
					main.Trash();
					soundTrash = true;
				}
				if (details[i].curveTimeMove < 1f)
				{
					details[i].curveTimeMove += Time.deltaTime * 2f;
					if (details[i].curveTimeMove > 1f)
					{
						details[i].curveTimeMove = 1f;
					}
				}
				details[i].objectDetail.position = Vector3.Lerp(details[i].positionOrigin, new Vector3(-1f, 9.404f + curveUp.Evaluate(details[i].curveTimeMove), 0.22f), details[i].curveTimeMove);
			}
		}
		if (pointerHint.gameObject.activeSelf)
		{
			timeAnimationPointer += Time.deltaTime * 2f;
			if (timeAnimationPointer > 1f)
			{
				timeAnimationPointer -= 1f;
			}
			pointerHint.localPosition = new Vector3(0f, 0f, 0.04f + animationPointerHint.Evaluate(timeAnimationPointer) * 0.03f);
		}
	}

	public void PutBolt()
	{
		pointerHint.gameObject.SetActive(value: false);
		countBolt++;
		if (countBolt == 4)
		{
			timeAnimation = 0.9f;
			nextDetailOpen = up;
		}
	}

	public void PutUp()
	{
		if (timeAnimation == 0f)
		{
			timeAnimation = 0.5f;
			nextDetailOpen = plan;
		}
	}

	public void PutPlan()
	{
		if (timeAnimation == 0f)
		{
			timeAnimationReady = 0.2f;
			clear = true;
			for (int i = 0; i < details.Length; i++)
			{
				details[i].curveTimeMove = 0f;
				details[i].timeStop = 0.1f + (float)i * 0.15f;
			}
		}
	}

	public void Restart()
	{
		timeAnimationReady = 0f;
		timeAnimation = 0f;
		fall = false;
		clear = false;
		pointerHint.gameObject.SetActive(value: false);
		timeAnimationPointer = 0f;
		bolt1.GetComponent<Animator>().enabled = false;
		bolt1.GetComponent<Rigidbody>().isKinematic = true;
		bolt1.GetComponent<Collider>().enabled = false;
		bolt1.GetComponent<Trigger_MouseClick>().enabled = false;
		bolt1.GetComponent<Trigger_MouseClick>().Restart();
		bolt1.GetComponent<Outlinable>().enabled = false;
		bolt1.transform.localPosition = new Vector3(-0.1134f, -0.08385507f, 0.03024f);
		bolt1.transform.localRotation = Quaternion.Euler(Vector3.zero);
		bolt2.GetComponent<Animator>().enabled = false;
		bolt2.GetComponent<Rigidbody>().isKinematic = true;
		bolt2.GetComponent<Collider>().enabled = false;
		bolt2.GetComponent<Trigger_MouseClick>().enabled = false;
		bolt2.GetComponent<Trigger_MouseClick>().Restart();
		bolt2.GetComponent<Outlinable>().enabled = false;
		bolt2.transform.localPosition = new Vector3(-0.1134f, 0.08385507f, 0.03024f);
		bolt2.transform.localRotation = Quaternion.Euler(Vector3.zero);
		bolt3.GetComponent<Animator>().enabled = false;
		bolt3.GetComponent<Rigidbody>().isKinematic = true;
		bolt3.GetComponent<Collider>().enabled = false;
		bolt3.GetComponent<Trigger_MouseClick>().enabled = false;
		bolt3.GetComponent<Trigger_MouseClick>().Restart();
		bolt3.GetComponent<Outlinable>().enabled = false;
		bolt3.transform.localPosition = new Vector3(0.1134f, -0.08385507f, 0.03024f);
		bolt3.transform.localRotation = Quaternion.Euler(Vector3.zero);
		bolt4.GetComponent<Animator>().enabled = false;
		bolt4.GetComponent<Rigidbody>().isKinematic = true;
		bolt4.GetComponent<Collider>().enabled = false;
		bolt4.GetComponent<Trigger_MouseClick>().enabled = false;
		bolt4.GetComponent<Trigger_MouseClick>().Restart();
		bolt4.GetComponent<Outlinable>().enabled = false;
		bolt4.transform.localPosition = new Vector3(0.1134f, 0.08385507f, 0.03024f);
		bolt4.transform.localRotation = Quaternion.Euler(Vector3.zero);
		down.GetComponent<Rigidbody>().isKinematic = true;
		down.GetComponent<Collider>().enabled = false;
		down.transform.localPosition = Vector3.zero;
		down.transform.localRotation = Quaternion.Euler(Vector3.zero);
		up.GetComponent<Animator>().enabled = false;
		up.GetComponent<Rigidbody>().isKinematic = true;
		up.GetComponent<Collider>().enabled = false;
		up.GetComponent<Trigger_MouseClick>().enabled = false;
		up.GetComponent<Trigger_MouseClick>().Restart();
		up.GetComponent<Outlinable>().enabled = false;
		up.transform.localPosition = Vector3.zero;
		up.transform.localRotation = Quaternion.Euler(Vector3.zero);
		plan.GetComponent<Animator>().enabled = false;
		plan.GetComponent<Rigidbody>().isKinematic = true;
		plan.GetComponent<Collider>().enabled = false;
		plan.GetComponent<Trigger_MouseClick>().enabled = false;
		plan.GetComponent<Trigger_MouseClick>().Restart();
		plan.GetComponent<Outlinable>().enabled = false;
		plan.transform.localPosition = Vector3.zero;
		plan.transform.localRotation = Quaternion.Euler(Vector3.zero);
	}

	public void Take()
	{
		pointerHint.gameObject.SetActive(value: true);
		GetComponent<BoxCollider>().enabled = false;
		up.GetComponent<Collider>().enabled = true;
		down.GetComponent<Collider>().enabled = true;
		plan.GetComponent<Collider>().enabled = true;
		countBolt = 0;
		bolt1.GetComponent<Collider>().enabled = true;
		bolt1.GetComponent<Trigger_MouseClick>().enabled = true;
		bolt2.GetComponent<Collider>().enabled = true;
		bolt2.GetComponent<Trigger_MouseClick>().enabled = true;
		bolt3.GetComponent<Collider>().enabled = true;
		bolt3.GetComponent<Trigger_MouseClick>().enabled = true;
		bolt4.GetComponent<Collider>().enabled = true;
		bolt4.GetComponent<Trigger_MouseClick>().enabled = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!fall)
		{
			GetComponent<AudioSource>().clip = soundsFall[Random.Range(0, soundsFall.Length)];
			GetComponent<AudioSource>().pitch = Random.Range(0.95f, 1.05f);
			GetComponent<AudioSource>().Play();
			fall = true;
		}
	}
}
