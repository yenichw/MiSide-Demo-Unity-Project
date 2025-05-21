using System;
using UnityEngine;

[Serializable]
public class DataAchievementsValues
{
	public Sprite icon;

	[HideInInspector]
	public int intNow;

	public int intMax;

	public string steamAchievement;

	public bool demo;
}
