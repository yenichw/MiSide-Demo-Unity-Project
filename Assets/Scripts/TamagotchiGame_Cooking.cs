using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TamagotchiGame_Cooking : MonoBehaviour
{
	public UnityEvent eventReady;

	public TamagotchiGame_Cooking_Food[] food;

	public AnimationCurve animationTake;

	public RectTransform rectCut;

	public Slider sliderCut;

	public Audio_Data audioCut;

	public AudioSource audioDone;

	private GameObject foodTakeNow;

	private int indexCut;

	private bool ready;

	private void OnEnable()
	{
		rectCut.gameObject.SetActive(value: false);
	}

	private void LateUpdate()
	{
		if (ready)
		{
			return;
		}
		if (foodTakeNow != null)
		{
			for (int i = 0; i < food.Length; i++)
			{
				if (!(food[i].tObj != null) || !(food[i].tObj.gameObject == foodTakeNow))
				{
					continue;
				}
				if (food[i].timeAnimation < 1f)
				{
					food[i].timeAnimation += Time.deltaTime * 2f;
					if (food[i].timeAnimation >= 1f)
					{
						food[i].timeAnimation = 1f;
						rectCut.gameObject.SetActive(value: true);
						sliderCut.GetComponent<RectTransform>().anchoredPosition = food[i].cutPosition[0];
						sliderCut.value = 0f;
						indexCut = 0;
					}
					food[i].tObj.transform.position = Vector3.Lerp(food[i].positionStart, food[i].positionFinish, food[i].timeAnimation) + new Vector3(0f, animationTake.Evaluate(food[i].timeAnimation) / 3f, 0f);
				}
				food[i].tObj.transform.rotation = Quaternion.Lerp(food[i].tObj.transform.rotation, Quaternion.Euler(food[i].rotation), Time.deltaTime * 10f);
			}
		}
		for (int j = 0; j < food.Length; j++)
		{
			if (food[j].ready && food[j].tObj != null && food[j].timeAnimation < 1f)
			{
				food[j].timeAnimation += Time.deltaTime;
				if (food[j].timeAnimation >= 1f)
				{
					Object.Destroy(food[j].tObj.gameObject);
				}
				food[j].tObj.transform.position = Vector3.Lerp(food[j].positionStart, food[j].positionFinish, food[j].timeAnimation) + new Vector3(0f, animationTake.Evaluate(food[j].timeAnimation) / 3f, 0f);
			}
		}
		if ((double)sliderCut.GetComponent<UI_SliderNeedMove>().value >= 0.98 && !sliderCut.GetComponent<UI_SliderNeedMove>().back)
		{
			Cut();
			sliderCut.GetComponent<UI_SliderNeedMove>().ResetMove();
		}
	}

	public void Ready()
	{
		ready = true;
		eventReady.Invoke();
	}

	public void TakeFood(GameObject _food)
	{
		for (int i = 0; i < food.Length; i++)
		{
			if (food[i].tObj != null)
			{
				food[i].tObj.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
				if (food[i].tObj.gameObject == _food)
				{
					foodTakeNow = _food;
					food[i].positionStart = food[i].tObj.position;
				}
			}
		}
		GetComponent<Tamagotchi_MiniGame>().SetLerpCameraPosition(new Vector3(20f, 12.5f, 2.5f));
	}

	public void Cut()
	{
		audioCut.RandomPlayPitch();
		for (int i = 0; i < food.Length; i++)
		{
			if (!(food[i].tObj != null) || !(food[i].tObj.gameObject == foodTakeNow))
			{
				continue;
			}
			if (indexCut < food[i].cutObject.Length - 1)
			{
				food[i].cutObject[indexCut].GetComponent<Rigidbody>().isKinematic = false;
				food[i].cutObject[indexCut].GetComponent<Rigidbody>().velocity = new Vector3(-2f, 0f, 0f);
				indexCut++;
				sliderCut.GetComponent<RectTransform>().anchoredPosition = food[i].cutPosition[indexCut];
				break;
			}
			rectCut.gameObject.SetActive(value: false);
			foodTakeNow = null;
			food[i].ready = true;
			food[i].timeAnimation = 0f;
			food[i].positionStart = food[i].tObj.position;
			food[i].positionFinish = food[i].tObj.position - new Vector3(3f, 0f, 0f);
			bool flag = true;
			for (int j = 0; j < food.Length; j++)
			{
				if (!food[j].ready)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Ready();
			}
			if (flag)
			{
				break;
			}
			for (int k = 0; k < food.Length; k++)
			{
				if (!food[k].ready)
				{
					food[k].tObj.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
				}
			}
			GetComponent<Tamagotchi_MiniGame>().SetLerpCameraPosition(new Vector3(20f, 12.5f, 0.8f));
			audioDone.Play();
			break;
		}
	}
}
