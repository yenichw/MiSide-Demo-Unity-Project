using UnityEngine;
using UnityEngine.Events;

public class Location4ChangeSide : MonoBehaviour
{
	public Transform CaseNo;

	public Transform CaseYes;

	public UnityEvent eventYes;

	public UnityEvent eventNo;

	public Transform lockYesT;

	private bool lockYes;

	private bool click;

	private Vector3 positionOriginNo;

	private Vector3 positionOriginNoNoise;

	private float timeUpdateNoNoise;

	private void Start()
	{
		positionOriginNo = CaseNo.position;
		CaseNo.gameObject.SetActive(value: true);
		CaseYes.gameObject.SetActive(value: true);
		lockYesT.localScale = Vector3.zero;
		lockYes = true;
	}

	private void Update()
	{
		if (CaseNo != null)
		{
			CaseNo.transform.position = Vector3.Lerp(positionOriginNo, positionOriginNo + positionOriginNoNoise, Time.deltaTime * 8f);
			timeUpdateNoNoise += Time.deltaTime;
			if ((double)timeUpdateNoNoise > 0.05)
			{
				timeUpdateNoNoise = 0f;
				positionOriginNoNoise = new Vector3(Random.Range(-0.06f, 0.06f), Random.Range(-0.06f, 0.06f), Random.Range(-0.06f, 0.06f));
			}
		}
		if (lockYes && lockYesT != null)
		{
			if (click)
			{
				lockYesT.transform.localScale = Vector3.Lerp(lockYesT.localScale, Vector3.zero, Time.deltaTime * 5f);
			}
			else
			{
				lockYesT.transform.localScale = Vector3.Lerp(lockYesT.localScale, Vector3.one, Time.deltaTime * 5f);
			}
		}
	}

	public void ChangeYes()
	{
		if (!lockYes)
		{
			eventYes.Invoke();
			Click();
		}
	}

	public void ChangeNo()
	{
		eventNo.Invoke();
		Click();
	}

	private void Click()
	{
		CaseNo.GetComponent<Interface_KeyHint_Key>().SmoothDestroy();
		CaseYes.GetComponent<Interface_KeyHint_Key>().SmoothDestroy();
		click = true;
	}
}
