using UnityEngine;

public class Achievement_function : MonoBehaviour
{
	public void AchievementComplete(int x)
	{
		GlobalTag.gameOptions.GetComponent<AchievementsController>().AchievementCompleted(x);
	}
}
