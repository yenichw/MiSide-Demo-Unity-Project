using UnityEngine;

public class Transform_SimpleCopyPosition : MonoBehaviour
{
	[Header("Копирует позицию и вращение (LateUpdate)")]
	public Transform copyPosition;

	public string playerTransform;

	private void Start()
	{
		if (playerTransform == null || !(playerTransform != "") || playerTransform == null || !(playerTransform != ""))
		{
			return;
		}
		bool flag = false;
		if (playerTransform == "Right item")
		{
			flag = true;
			copyPosition = GlobalTag.player.transform.Find("RightItem FixPosition");
		}
		if (playerTransform == "Left item")
		{
			flag = true;
			copyPosition = GlobalTag.player.transform.Find("LeftItem FixPosition");
		}
		if (flag)
		{
			return;
		}
		Component[] componentsInChildren = GlobalTag.player.transform.Find("Person").GetComponentsInChildren(typeof(Transform), includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name == playerTransform)
			{
				copyPosition = componentsInChildren[i].transform;
				break;
			}
		}
	}

	private void LateUpdate()
	{
		base.transform.SetPositionAndRotation(copyPosition.position, copyPosition.rotation);
	}
}
