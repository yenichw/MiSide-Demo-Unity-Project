using System;
using UnityEngine;

[Serializable]
public class GamesCore_Main_PositionSphere
{
	public bool isCore;

	public bool isFinish;

	public Vector2 position;

	public float rotation;

	public int indexJoinUp;

	public int indexJoinLeft;

	public int indexJoinRight;

	public int indexJoinDown;

	public string version;

	[Space(20f)]
	public GameObject sphere;
}
