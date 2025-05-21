using UnityEngine;

public class GameObject_Create : MonoBehaviour
{
	public GameObject objectCreate;

	public GameObject targetPosition;

	[Space(7f)]
	public Vector3 positionTargetForward;

	public Vector3 rotationTargetForward;

	[Header("Settings")]
	public string findTargetPlayer;

	public float timeReCreate = -1f;

	private float timeReCreateNow;

	public GameObject objectCreateNow;

	public bool useObjectCreateFromNow;

	private void Start()
	{
		timeReCreateNow = timeReCreate;
		if (useObjectCreateFromNow)
		{
			objectCreate = Object.Instantiate(objectCreateNow, objectCreateNow.transform.parent);
			objectCreate.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (timeReCreate >= 0f && objectCreateNow == null)
		{
			timeReCreateNow -= Time.deltaTime;
			if (timeReCreateNow <= 0f)
			{
				timeReCreateNow = timeReCreate;
				CreateObject();
			}
		}
	}

	public void CreateObject()
	{
		if (findTargetPlayer != null && findTargetPlayer != "")
		{
			targetPosition = GameObject.FindWithTag("Player").transform.Find(findTargetPlayer).gameObject;
		}
		objectCreateNow = Object.Instantiate(objectCreate);
		objectCreateNow.SetActive(value: true);
		if (useObjectCreateFromNow)
		{
			objectCreateNow.transform.parent = objectCreate.transform.parent;
			objectCreateNow.transform.rotation = objectCreate.transform.rotation;
			objectCreateNow.transform.position = objectCreate.transform.position;
		}
		else if (targetPosition != null)
		{
			objectCreateNow.transform.position = targetPosition.transform.position;
			objectCreateNow.transform.rotation = targetPosition.transform.rotation;
			objectCreateNow.transform.position += GlobalAM.TransformPivot(targetPosition.transform, positionTargetForward);
			objectCreateNow.transform.rotation *= Quaternion.Euler(rotationTargetForward);
		}
		else
		{
			objectCreateNow.transform.position = GlobalAM.TransformPivot(base.transform, positionTargetForward);
			objectCreateNow.transform.rotation = base.transform.rotation * Quaternion.Euler(rotationTargetForward);
		}
	}

	public void DestroyCreateObject()
	{
		Object.Destroy(objectCreateNow);
	}
}
