using UnityEngine;

public class TamagotchiGame_Sorting : MonoBehaviour
{
	public Tamagotchi_AddMoney moneyAdd;

	public int energy;

	public GameObject box1;

	public GameObject box2;

	public GameObject box3;

	[Header("Предметы")]
	public Mesh meshItem1;

	public Mesh meshItem2;

	public Mesh meshItem3;

	public TamagotchiGame_Sorting_ItemAnimation[] items;

	[Header("Анимация")]
	public AnimationCurve animaitonItemDrop;

	[Header("Звуки")]
	public AudioSource audioBoxDropItem;

	public AudioClip[] soundsBoxDropItem;

	public AudioClip[] soundsBoxBottle;

	public AudioClip[] soundsBoxMetal;

	public AudioClip[] soundsBoxFood;

	private GameObject itemTake;

	private float timeStart;

	private float timeItemStart;

	private int itemIndexStart;

	private bool firstStart;

	private void Update()
	{
		if (timeStart > 0f)
		{
			timeStart -= Time.deltaTime;
		}
		else if (itemIndexStart < items.Length)
		{
			timeItemStart += Time.deltaTime;
			if (timeItemStart < 0.2f)
			{
				timeItemStart = 0f;
				items[itemIndexStart].item.GetComponent<TamagotchiGame_Sorting_Item>().StartItem();
				itemIndexStart++;
			}
		}
		if (itemTake != null)
		{
			itemTake.transform.position = Vector3.Lerp(itemTake.transform.position, new Vector3(10f, 9.5f, 0f), Time.deltaTime * 15f);
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (!items[i].animationPlay || !(items[i].timeAnimation < 1f))
			{
				continue;
			}
			items[i].timeAnimation += Time.deltaTime * 2.5f;
			if (items[i].timeAnimation > 1f)
			{
				items[i].timeAnimation = 1f;
				if (items[i].right || items[i].stopAnimaiton)
				{
					items[i].item.GetComponent<TamagotchiGame_Sorting_Item>().RigidbodyFreeze(x: false);
					items[i].animationPlay = false;
					if (items[i].right)
					{
						audioBoxDropItem.clip = soundsBoxDropItem[Random.Range(0, soundsBoxDropItem.Length)];
						audioBoxDropItem.pitch = Random.Range(0.95f, 1.05f);
						audioBoxDropItem.Play();
					}
				}
				else
				{
					items[i].timeAnimation = 0f;
					items[i].positionStart = items[i].positionEnd;
					items[i].positionEnd = new Vector3(10f, 9.3f, 0f);
					items[i].item.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
					items[i].stopAnimaiton = true;
					items[i].item.GetComponent<TamagotchiGame_Sorting_Item>().NoRight();
				}
			}
			items[i].item.transform.position = Vector3.Lerp(items[i].positionStart, items[i].positionEnd + Vector3.up * animaitonItemDrop.Evaluate(items[i].timeAnimation), items[i].timeAnimation);
		}
	}

	public void Restart()
	{
		timeStart = 0.5f;
		itemIndexStart = 0;
		timeItemStart = 0f;
		itemTake = null;
		if (!firstStart)
		{
			firstStart = true;
			for (int i = 1; i < items.Length; i++)
			{
				items[i].item = Object.Instantiate(items[0].item, items[0].item.transform.parent);
			}
		}
		for (int j = 0; j < items.Length; j++)
		{
			items[j].item.transform.localPosition = new Vector3(0f, 1f + (float)j * 0.3f, 0f);
			items[j].animationPlay = false;
			items[j].timeAnimation = 0f;
			items[j].right = false;
			items[j].stopAnimaiton = false;
			int num = Random.Range(0, 3);
			if (num == 0)
			{
				items[j].item.GetComponent<TamagotchiGame_Sorting_Item>().RestartPosition(0, meshItem1, soundsBoxBottle);
			}
			if (num == 1)
			{
				items[j].item.GetComponent<TamagotchiGame_Sorting_Item>().RestartPosition(1, meshItem2, soundsBoxMetal);
			}
			if (num == 2)
			{
				items[j].item.GetComponent<TamagotchiGame_Sorting_Item>().RestartPosition(2, meshItem3, soundsBoxFood);
			}
		}
		box1.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		box2.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		box3.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
	}

	public void TakeItem(GameObject _object)
	{
		itemTake = _object;
		itemTake.GetComponent<TamagotchiGame_Sorting_Item>().Take();
		box1.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		box2.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		box3.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		for (int i = 0; i < items.Length; i++)
		{
			items[i].item.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		}
	}

	public void DropItemBox(GameObject _obejct)
	{
		int num = 0;
		if (_obejct == box2)
		{
			num = 1;
		}
		if (_obejct == box3)
		{
			num = 2;
		}
		if (itemTake.GetComponent<TamagotchiGame_Sorting_Item>().typeItem == num)
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].item == itemTake)
				{
					items[i].right = true;
					items[i].positionStart = itemTake.transform.position;
					items[i].positionEnd = _obejct.transform.position + Vector3.up * 0.3f;
					items[i].timeAnimation = 0f;
					items[i].animationPlay = true;
					items[i].stopAnimaiton = false;
				}
				else
				{
					items[i].item.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
				}
			}
		}
		else
		{
			for (int j = 0; j < items.Length; j++)
			{
				if (items[j].item == itemTake)
				{
					items[j].positionStart = itemTake.transform.position;
					items[j].positionEnd = _obejct.transform.position + Vector3.up * 0.3f;
					items[j].timeAnimation = 0f;
					items[j].animationPlay = true;
					items[j].stopAnimaiton = false;
				}
				else
				{
					items[j].item.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
				}
			}
		}
		bool flag = true;
		for (int k = 0; k < items.Length; k++)
		{
			if (!items[k].right)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			moneyAdd.StartAddMoney(250, energy);
			for (int l = 0; l < items.Length; l++)
			{
				items[l].item.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			}
		}
		box1.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		box2.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		box3.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		itemTake = null;
	}
}
