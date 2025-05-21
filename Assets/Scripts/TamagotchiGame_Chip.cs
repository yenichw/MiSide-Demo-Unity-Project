using EPOOutline;
using UnityEngine;

public class TamagotchiGame_Chip : MonoBehaviour
{
	public TamagotchiGame_Chip_Case[] cases;

	public GameObject plitaHorizontal;

	public GameObject plitaVertical;

	public GameObject plitaAngle1;

	public GameObject plitaAngle2;

	public GameObject plitaAngle3;

	public GameObject plitaAngle4;

	public GameObject pointStart;

	public GameObject pointFinish;

	public AnimationCurve animationPlitaInCase;

	public AnimationCurveTransform[] objectsDestroy;

	public AnimationCurve animationDestroyUp;

	public Tamagotchi_AddMoney moneyAdd;

	public int energy;

	public LayerMask plita;

	[Header("Звуки")]
	public AudioClip soundPlitaPut;

	public AudioSource plitaUse;

	public AudioClip[] soundsPlitaUse;

	public AudioSource audioPlitaChange;

	public AudioClip[] soundsPlitaChange;

	private GameObject plitaChange;

	private RaycastHit hit;

	private int timeCheckReady;

	private void Update()
	{
		if (plitaChange != plitaHorizontal)
		{
			plitaHorizontal.transform.localPosition = Vector3.Lerp(plitaHorizontal.transform.localPosition, new Vector3(1.25f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaHorizontal.transform.localPosition = Vector3.Lerp(plitaHorizontal.transform.localPosition, new Vector3(1.25f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		if (plitaChange != plitaVertical)
		{
			plitaVertical.transform.localPosition = Vector3.Lerp(plitaVertical.transform.localPosition, new Vector3(0.75f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaVertical.transform.localPosition = Vector3.Lerp(plitaVertical.transform.localPosition, new Vector3(0.75f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		if (plitaChange != plitaAngle1)
		{
			plitaAngle1.transform.localPosition = Vector3.Lerp(plitaAngle1.transform.localPosition, new Vector3(0.25f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaAngle1.transform.localPosition = Vector3.Lerp(plitaAngle1.transform.localPosition, new Vector3(0.25f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		if (plitaChange != plitaAngle2)
		{
			plitaAngle2.transform.localPosition = Vector3.Lerp(plitaAngle2.transform.localPosition, new Vector3(-0.25f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaAngle2.transform.localPosition = Vector3.Lerp(plitaAngle2.transform.localPosition, new Vector3(-0.25f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		if (plitaChange != plitaAngle3)
		{
			plitaAngle3.transform.localPosition = Vector3.Lerp(plitaAngle3.transform.localPosition, new Vector3(-0.75f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaAngle3.transform.localPosition = Vector3.Lerp(plitaAngle3.transform.localPosition, new Vector3(-0.75f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		if (plitaChange != plitaAngle4)
		{
			plitaAngle4.transform.localPosition = Vector3.Lerp(plitaAngle4.transform.localPosition, new Vector3(-1.25f, -0.12f, 1.4f), Time.deltaTime * 10f);
		}
		else
		{
			plitaAngle4.transform.localPosition = Vector3.Lerp(plitaAngle4.transform.localPosition, new Vector3(-1.25f, 0f, 1.4f), Time.deltaTime * 10f);
		}
		for (int i = 0; i < cases.Length; i++)
		{
			if (cases[i].objectPlita != null && cases[i].timeAnimation < 1f)
			{
				cases[i].timeAnimation += Time.deltaTime * 2f;
				if ((double)cases[i].timeAnimation > 0.55 && !cases[i].soundPut)
				{
					cases[i].soundPut = true;
					plitaUse.volume = 0.5f;
					plitaUse.clip = soundPlitaPut;
					plitaUse.pitch = Random.Range(0.95f, 1.05f);
					plitaUse.Play();
				}
				if (cases[i].timeAnimation > 1f)
				{
					cases[i].timeAnimation = 1f;
				}
				cases[i].objectPlita.transform.localPosition = new Vector3(0f, 0f, animationPlitaInCase.Evaluate(cases[i].timeAnimation) * 3f);
			}
		}
		for (int j = 0; j < objectsDestroy.Length; j++)
		{
			if (objectsDestroy[j].tObj != null)
			{
				objectsDestroy[j].timeAnimation += Time.deltaTime * 2f;
				if (objectsDestroy[j].timeAnimation > 1f)
				{
					Object.Destroy(objectsDestroy[j].tObj.gameObject);
				}
				else
				{
					objectsDestroy[j].tObj.position = Vector3.Lerp(objectsDestroy[j].positionStart, objectsDestroy[j].positionFinish + Vector3.up * animationDestroyUp.Evaluate(objectsDestroy[j].timeAnimation) * 0.4f, objectsDestroy[j].timeAnimation);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (timeCheckReady <= 0)
		{
			return;
		}
		timeCheckReady--;
		if (timeCheckReady == 0)
		{
			GameObject gameObject = CheckCast(pointStart.transform.position + new Vector3(0.4f, 0f, 0f));
			if (gameObject != null && gameObject.GetComponent<TamagotchiGame_Chip_Plita>() != null)
			{
				gameObject.GetComponent<TamagotchiGame_Chip_Plita>().TouchLeft();
			}
		}
	}

	public void Restart()
	{
		for (int i = 0; i < cases.Length; i++)
		{
			cases[i].obejctCase.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			if (cases[i].objectPlita != null)
			{
				Object.Destroy(cases[i].objectPlita);
			}
		}
		plitaChange = null;
		plitaHorizontal.transform.localPosition = new Vector3(1.25f, 0f, 1.4f);
		plitaVertical.transform.localPosition = new Vector3(0.75f, 0f, 1.4f);
		plitaAngle1.transform.localPosition = new Vector3(0.25f, 0f, 1.4f);
		plitaAngle2.transform.localPosition = new Vector3(-0.25f, 0f, 1.4f);
		plitaAngle3.transform.localPosition = new Vector3(-0.75f, 0f, 1.4f);
		plitaAngle4.transform.localPosition = new Vector3(-1.25f, 0f, 1.4f);
		plitaHorizontal.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		plitaVertical.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		plitaAngle1.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		plitaAngle2.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		plitaAngle3.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		plitaAngle4.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		int num = Random.Range(0, 3);
		if (num == 0)
		{
			pointStart.transform.localPosition = new Vector3(-0.981992f, 0.06895618f, 0.655f);
		}
		if (num == 1)
		{
			pointStart.transform.localPosition = new Vector3(-0.981992f, 0.06895618f, 0.2185f);
		}
		if (num == 2)
		{
			pointStart.transform.localPosition = new Vector3(-0.981992f, 0.06895618f, -0.2185f);
		}
		if (num == 3)
		{
			pointStart.transform.localPosition = new Vector3(-0.981992f, 0.06895618f, -0.655f);
		}
		int num2 = Random.Range(0, 3);
		if (num2 == 0)
		{
			pointFinish.transform.localPosition = new Vector3(0.981992f, 0.06895618f, 0.655f);
		}
		if (num2 == 1)
		{
			pointFinish.transform.localPosition = new Vector3(0.981992f, 0.06895618f, 0.2185f);
		}
		if (num2 == 2)
		{
			pointFinish.transform.localPosition = new Vector3(0.981992f, 0.06895618f, -0.2185f);
		}
		if (num2 == 3)
		{
			pointFinish.transform.localPosition = new Vector3(0.981992f, 0.06895618f, -0.655f);
		}
		for (int j = 0; j < objectsDestroy.Length; j++)
		{
			if (objectsDestroy[j].tObj != null)
			{
				objectsDestroy[j].timeAnimation = 0f;
				Object.Destroy(objectsDestroy[j].tObj.gameObject);
			}
		}
	}

	public void ChangePlita(GameObject _plita)
	{
		plitaChange = _plita;
		audioPlitaChange.clip = soundsPlitaChange[Random.Range(0, soundsPlitaChange.Length)];
		audioPlitaChange.pitch = Random.Range(0.95f, 1.05f);
		audioPlitaChange.Play();
		for (int i = 0; i < cases.Length; i++)
		{
			cases[i].obejctCase.GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		}
	}

	public void ChangeCase(GameObject _case)
	{
		for (int i = 0; i < cases.Length; i++)
		{
			if (!(cases[i].obejctCase == _case))
			{
				continue;
			}
			if (cases[i].objectPlita != null)
			{
				for (int j = 0; j < objectsDestroy.Length; j++)
				{
					if (objectsDestroy[j].tObj == null)
					{
						objectsDestroy[j].tObj = cases[i].objectPlita.transform;
						objectsDestroy[j].tObj.gameObject.layer = 2;
						objectsDestroy[j].timeAnimation = 0f;
						objectsDestroy[j].positionStart = objectsDestroy[j].tObj.position;
						objectsDestroy[j].positionFinish = objectsDestroy[j].tObj.position + new Vector3(0f, 0f, 2.5f);
						break;
					}
				}
			}
			plitaUse.volume = 0.3f;
			plitaUse.clip = soundsPlitaUse[Random.Range(0, soundsPlitaUse.Length)];
			plitaUse.pitch = Random.Range(0.95f, 1.05f);
			plitaUse.Play();
			cases[i].objectPlita = Object.Instantiate(plitaChange, cases[i].obejctCase.transform);
			cases[i].objectPlita.layer = 6;
			cases[i].objectPlita.transform.localPosition = new Vector3(0f, 0f, 3f);
			cases[i].objectPlita.transform.rotation = plitaChange.transform.rotation;
			cases[i].timeAnimation = 0f;
			cases[i].soundPut = false;
			Object.Destroy(cases[i].objectPlita.GetComponent<Trigger_MouseClick>());
			Object.Destroy(cases[i].objectPlita.GetComponent<Outlinable>());
			break;
		}
		timeCheckReady = 2;
	}

	public GameObject CheckCast(Vector3 _positionCheck)
	{
		GameObject gameObject = null;
		if (Physics.Raycast(_positionCheck + Vector3.up * 100f, -Vector3.up, out hit, 120f, plita))
		{
			gameObject = hit.collider.gameObject;
		}
		if (gameObject == pointFinish)
		{
			moneyAdd.StartAddMoney(200, energy);
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i].obejctCase.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			}
			plitaHorizontal.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			plitaVertical.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			plitaAngle1.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			plitaAngle2.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			plitaAngle3.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
			plitaAngle4.GetComponent<Trigger_MouseClick>().ActivatedObject(x: false);
		}
		return gameObject;
	}
}
