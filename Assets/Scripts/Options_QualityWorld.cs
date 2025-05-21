using UnityEngine;

public class Options_QualityWorld : MonoBehaviour
{
	public GameObject[] objectLevel1;

	public GameObject[] objectLevel2;

	private void Start()
	{
		StartSeetings();
	}

	public void StartSeetings()
	{
		if (objectLevel1 != null && objectLevel1.Length != 0)
		{
			if (GlobalGame.qualityWorld >= 1)
			{
				for (int i = 0; i < objectLevel1.Length; i++)
				{
					objectLevel1[i].SetActive(value: true);
				}
			}
			else
			{
				for (int j = 0; j < objectLevel1.Length; j++)
				{
					if (objectLevel1[j] != null)
					{
						Object.Destroy(objectLevel1[j]);
					}
				}
			}
		}
		if (objectLevel2 == null || objectLevel2.Length == 0)
		{
			return;
		}
		if (GlobalGame.qualityWorld >= 2)
		{
			for (int k = 0; k < objectLevel2.Length; k++)
			{
				objectLevel2[k].SetActive(value: true);
			}
			return;
		}
		for (int l = 0; l < objectLevel2.Length; l++)
		{
			if (objectLevel2[l] != null)
			{
				Object.Destroy(objectLevel2[l]);
			}
		}
	}
}
