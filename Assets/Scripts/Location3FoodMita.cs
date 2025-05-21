using UnityEngine;

public class Location3FoodMita : MonoBehaviour
{
	[Header("Food")]
	public GameObject[] foods;

	private int indexFood;

	[Header("Sous")]
	public Vector3[] sousPositions;

	public LineRenderer lineSous;

	public Transform sousPack;

	private float timeSous;

	private int countSousActive;

	private void Update()
	{
		if (foods[indexFood].transform.localScale.x < 1f)
		{
			foods[indexFood].transform.localScale += Vector3.one * (Time.deltaTime * 5f);
			if (foods[indexFood].transform.localScale.x > 1f)
			{
				foods[indexFood].transform.localScale = Vector3.one;
			}
		}
		if (!(lineSous != null))
		{
			return;
		}
		if (timeSous > 0f && countSousActive < sousPositions.Length)
		{
			timeSous -= Time.deltaTime;
			if (timeSous <= 0f)
			{
				timeSous = 0.135f;
				countSousActive++;
			}
		}
		for (int i = 0; i < sousPositions.Length; i++)
		{
			if (countSousActive > i)
			{
				lineSous.SetPosition(i, Vector3.Lerp(lineSous.GetPosition(i), sousPositions[i], Time.deltaTime * 8f));
			}
			else
			{
				lineSous.SetPosition(i, GlobalAM.TransformPivot(sousPack, new Vector3(0.0358f, 0.0004f, -0.0304f)));
			}
		}
	}

	public void TakeFood()
	{
		indexFood = Random.Range(0, foods.Length);
		foods[indexFood].gameObject.SetActive(value: true);
		foods[indexFood].transform.localScale = Vector3.zero;
	}

	public void EatFood()
	{
		for (int i = 0; i < foods.Length; i++)
		{
			foods[i].SetActive(value: false);
		}
	}

	public void StartSous()
	{
		timeSous = 0.135f;
		lineSous.enabled = true;
	}
}
