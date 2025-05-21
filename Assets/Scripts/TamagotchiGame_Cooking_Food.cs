using System;
using UnityEngine;

[Serializable]
public class TamagotchiGame_Cooking_Food
{
	public Transform tObj;

	public float timeAnimation;

	public Vector3 positionStart;

	public Vector3 positionFinish;

	public Vector3 rotation;

	[Header("Cut")]
	public GameObject[] cutObject;

	public Vector2[] cutPosition;

	[HideInInspector]
	public bool ready;
}
