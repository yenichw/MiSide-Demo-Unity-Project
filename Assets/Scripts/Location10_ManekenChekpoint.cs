using UnityEngine;

public class Location10_ManekenChekpoint : MonoBehaviour
{
	public ManekenData[] manekens;

	public int indexSavePoint;

	public Player_Teleport[] savePositionPlayer;

	public void ManekenAttack(GameObject _maneken)
	{
		for (int i = 0; i < manekens.Length; i++)
		{
			if (manekens[i].maneken != _maneken)
			{
				manekens[i].maneken.GetComponent<Mob_Maneken>().Stop(x: true);
			}
		}
	}

	public void PlayerDeath()
	{
		savePositionPlayer[indexSavePoint].Click();
		for (int i = 0; i < manekens.Length; i++)
		{
			if (manekens[i].positionStart[indexSavePoint].x != 1000f)
			{
				manekens[i].maneken.GetComponent<Mob_Maneken>().ResetManeken();
				manekens[i].maneken.transform.SetPositionAndRotation(manekens[i].positionStart[indexSavePoint], Quaternion.Euler(0f, manekens[i].rotationStart[indexSavePoint], 0f));
			}
		}
	}

	public void SavePoint(int _save)
	{
		indexSavePoint = _save;
	}

	public void StopManekens()
	{
		for (int i = 0; i < manekens.Length; i++)
		{
			manekens[i].maneken.GetComponent<Mob_Maneken>().Stop(x: true);
		}
	}
}
