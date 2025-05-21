using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsController : MonoBehaviour
{
	public bool disableSteam;

	public CGameID m_GameID;

	public AppId_t appID;

	[HideInInspector]
	public DataAchievementsValues[] dataAchievements;

	public void StartComponent()
	{
		dataAchievements = (Resources.Load("Data/Achievements") as GameObject).GetComponent<DataAchievements>().dataAchievements;
		for (int i = 0; i < dataAchievements.Length; i++)
		{
			if (GlobalAM.ExistsData("Achi " + dataAchievements[i].steamAchievement))
			{
				dataAchievements[i].intNow = GlobalAM.StringToInt(GlobalAM.LoadData("Achi " + dataAchievements[i].steamAchievement)[0]);
			}
		}
		if (!disableSteam && SteamManager.Initialized)
		{
			appID = SteamUtils.GetAppID();
			m_GameID = new CGameID(SteamUtils.GetAppID());
		}
	}

	public int ProgressAchievement()
	{
		float num = 0f;
		if (!GlobalGame.demo)
		{
			for (int i = 0; i < dataAchievements.Length; i++)
			{
				if (dataAchievements[i].intNow == dataAchievements[i].intMax)
				{
					num += 1f;
				}
			}
			return Mathf.FloorToInt(num / (float)dataAchievements.Length * 100f);
		}
		int num2 = 0;
		for (int j = 0; j < dataAchievements.Length; j++)
		{
			if (dataAchievements[j].demo)
			{
				num2++;
				if (dataAchievements[j].intNow == dataAchievements[j].intMax)
				{
					num += 1f;
				}
			}
		}
		return Mathf.FloorToInt(num / (float)num2 * 100f);
	}

	public void AchievementCompleted(int x)
	{
		if (dataAchievements[x].intNow == dataAchievements[x].intMax)
		{
			return;
		}
		if (dataAchievements[x].intNow < dataAchievements[x].intMax)
		{
			dataAchievements[x].intNow++;
			if (dataAchievements[x].intNow == dataAchievements[x].intMax)
			{
				dataAchievements[x].intNow = dataAchievements[x].intMax;
				GameObject obj = Object.Instantiate(Resources.Load("Data/InterfaceAchievement") as GameObject);
				obj.transform.Find("Frame/FrameIcon/Icon").GetComponent<Image>().sprite = dataAchievements[x].icon;
				obj.transform.Find("Frame/Text").GetComponent<Text>().text = File.ReadAllLines("Data/Languages/" + GlobalGame.Language + "/Achievements.txt")[x];
			}
		}
		if (!disableSteam && SteamManager.Initialized)
		{
			SteamUserStats.SetAchievement(dataAchievements[x].steamAchievement);
			SteamUserStats.StoreStats();
		}
		GlobalAM.SaveData("Achi " + dataAchievements[x].steamAchievement, dataAchievements[x].intNow.ToString() ?? "");
	}
}
