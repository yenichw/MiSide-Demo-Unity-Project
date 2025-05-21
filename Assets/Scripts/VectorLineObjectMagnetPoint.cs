using System;
using UnityEngine;

[Serializable]
public class VectorLineObjectMagnetPoint
{
	public int countPoints;

	public Vector3 positionStart;

	public Vector3 positionEnd;

	[Space(10f)]
	public Vector3 rotationObject;
}
