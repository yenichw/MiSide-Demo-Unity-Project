using UnityEngine;

public class ChibiRoomLogic_LineAnimKey : MonoBehaviour
{
	public LineRenderer line;

	public Vector3[] positionsAnimaiton;

	public float speed;

	[Header("Setup")]
	public Vector3[] positionsAnimaitonNow;

	public int[] indexAnimationNow;

	public float[] indexAnimationNowTime;

	public int countPlay;

	private void Start()
	{
		RestartAnimation();
	}

	private void Update()
	{
		for (int i = 0; i < countPlay + 1; i++)
		{
			indexAnimationNowTime[i] += Time.deltaTime * speed;
			positionsAnimaitonNow[i] = Vector3.Lerp(positionsAnimaiton[indexAnimationNow[i]], positionsAnimaiton[indexAnimationNow[i] + 1], indexAnimationNowTime[i]);
			if (indexAnimationNowTime[countPlay] >= 1f && countPlay < positionsAnimaiton.Length - 1)
			{
				countPlay++;
			}
			if (indexAnimationNowTime[i] >= 1f && indexAnimationNow[i] < positionsAnimaiton.Length - 2)
			{
				indexAnimationNowTime[i] = 0f;
				indexAnimationNow[i]++;
			}
			line.SetPosition(i, positionsAnimaitonNow[i]);
		}
	}

	private void RestartAnimation()
	{
		countPlay = 0;
		for (int i = 0; i < positionsAnimaiton.Length; i++)
		{
			indexAnimationNow[i] = 0;
			positionsAnimaitonNow[i] = positionsAnimaiton[0];
			indexAnimationNowTime[i] = 0f;
			line.SetPosition(i, positionsAnimaiton[0]);
		}
	}
}
