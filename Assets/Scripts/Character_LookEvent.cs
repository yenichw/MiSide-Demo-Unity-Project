using System;
using UnityEngine;
using UnityEngine.Events;

public class Character_LookEvent : MonoBehaviour
{
	public Character_Look characterLook;

	public UnityEvent eventRotateBodyFinish;

	public UnityEvent eventRotateBodyHalfFinish;

	public float rotate;

	public Transform rotateTarget;

	public void Rotate()
	{
		if (rotateTarget != null)
		{
			characterLook.StartRotate(rotateTarget.position);
		}
		else
		{
			characterLook.StartRotate(characterLook.transform.position + new Vector3(Mathf.Cos((0f - rotate + 90f) * (MathF.PI / 180f)), 0f, Mathf.Sin((0f - rotate + 90f) * (MathF.PI / 180f))) / 2f);
		}
		characterLook.erbfReady = false;
		characterLook.erbhfReady = false;
		characterLook.eventRotateBodyFinish = eventRotateBodyFinish;
		characterLook.eventRotateBodyHalfFinish = eventRotateBodyHalfFinish;
	}
}
