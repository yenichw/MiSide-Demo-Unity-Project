using UnityEngine;
using UnityEngine.Events;

public class TamagotchiGame_HelpTrash : MonoBehaviour
{
	public UnityEvent eventFinish;

	[Header("Корзинка")]
	public GameObject basket;

	public GameObject basketPointer;

	[Header("Мусор")]
	public AnimationCurveTransforms[] trashes;

	public AnimationCurve animationFly;

	public AnimationCurve animationBasket;

	public ParticleSystem particleTrash;

	[Header("Звуки")]
	public AudioSource audioTrash;

	public AudioClip[] soundsTrashDrop;

	public AudioSource audioClothTake;

	public AudioClip[] soundsTakeCloth;

	private GameObject trashTake;

	private bool ready;

	private float timeAnimationPointer;

	private void Update()
	{
		ready = true;
		timeAnimationPointer += Time.deltaTime;
		if (timeAnimationPointer > 1f)
		{
			timeAnimationPointer -= 1f;
		}
		for (int i = 0; i < trashes.Length; i++)
		{
			if (trashes[i].tObj[0] != null)
			{
				ready = false;
				trashes[i].tObj[1].position = trashes[i].tObj[0].position + new Vector3(0f, 0.5f + animationFly.Evaluate(timeAnimationPointer), 0f);
				if (trashes[i].tObj[0].gameObject == trashTake)
				{
					trashes[i].timeAnimation += Time.deltaTime;
					if (trashes[i].timeAnimation > 1f)
					{
						trashes[i].timeAnimation -= 1f;
					}
					trashes[i].tObj[0].position = Vector3.Lerp(trashes[i].tObj[0].position, trashes[i].positionStart + new Vector3(0f, 0.2f + animationFly.Evaluate(trashes[i].timeAnimation) / 4f, 0f), Time.deltaTime * 10f);
				}
				else
				{
					trashes[i].tObj[0].position = Vector3.Lerp(trashes[i].tObj[0].position, trashes[i].positionStart, Time.deltaTime * 10f);
				}
			}
			if (trashes[i].tObj[2] != null)
			{
				trashes[i].timeAnimation += Time.deltaTime * 1.5f;
				trashes[i].tObj[2].position = Vector3.Lerp(trashes[i].positionStart, trashes[i].positionFinish, trashes[i].timeAnimation) + new Vector3(0f, animationBasket.Evaluate(trashes[i].timeAnimation) * 2f, 0f);
				if (trashes[i].timeAnimation >= 1f)
				{
					particleTrash.Play();
					Object.Destroy(trashes[i].tObj[2].gameObject);
					basket.GetComponent<Animator>().SetTrigger("Trash");
					audioTrash.clip = soundsTrashDrop[Random.Range(0, soundsTrashDrop.Length)];
					audioTrash.pitch = Random.Range(0.95f, 1.05f);
					audioTrash.Play();
				}
				ready = false;
			}
		}
		if (ready)
		{
			eventFinish.Invoke();
			Object.Destroy(base.gameObject);
		}
	}

	public void ClickBasket()
	{
		if (trashTake != null)
		{
			for (int i = 0; i < trashes.Length; i++)
			{
				if (trashes[i].tObj[0] != null)
				{
					if (trashTake == trashes[i].tObj[0].gameObject)
					{
						trashes[i].timeAnimation = 0f;
						trashes[i].positionStart = trashes[i].tObj[0].position;
						trashes[i].positionFinish = basket.transform.position + new Vector3(0f, 0.4f, 0f);
						trashes[i].tObj[1].gameObject.SetActive(value: false);
						trashes[i].tObj[2] = trashes[i].tObj[0];
						trashes[i].tObj[0] = null;
					}
					else
					{
						trashes[i].tObj[1].gameObject.SetActive(value: true);
					}
				}
			}
			basketPointer.SetActive(value: false);
		}
		basket.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
	}

	public void TakeTrash(GameObject _trash)
	{
		if (_trash != trashTake)
		{
			basket.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
			for (int i = 0; i < trashes.Length; i++)
			{
				if (trashes[i].tObj[0] != null)
				{
					if (_trash == trashes[i].tObj[0].gameObject)
					{
						trashes[i].tObj[1].gameObject.SetActive(value: false);
						trashes[i].tObj[0].GetComponent<Trigger_MouseClick>().ActivationOutline(x: false);
					}
					else
					{
						trashes[i].tObj[1].gameObject.SetActive(value: true);
						trashes[i].tObj[0].GetComponent<Trigger_MouseClick>().ActivationOutline(x: true);
					}
				}
			}
			trashTake = _trash;
			audioClothTake.clip = soundsTakeCloth[Random.Range(0, soundsTakeCloth.Length)];
			audioClothTake.pitch = Random.Range(0.9f, 1.1f);
			audioClothTake.Play();
		}
		if (trashTake != null)
		{
			basketPointer.SetActive(value: true);
		}
		else
		{
			basketPointer.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		for (int i = 0; i < trashes.Length; i++)
		{
			trashes[i].tObj[0].GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
			trashes[i].tObj[1].gameObject.SetActive(value: true);
			trashes[i].positionStart = trashes[i].tObj[0].position;
		}
		basket.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		basket.GetComponent<Animator>().enabled = true;
	}
}
