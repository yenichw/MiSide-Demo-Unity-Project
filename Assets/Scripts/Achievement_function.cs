// based on the original game.Yen Chezky(yenichw)
using UnityEngine;

public class Achievement_function : MonoBehaviour
{
	public void AchievementComplete(int x)
	{
		GlobalTag.gameOptions.GetComponent<AchievementsController>().AchievementCompleted(x);
	}
}
