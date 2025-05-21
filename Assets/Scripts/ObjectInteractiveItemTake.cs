using UnityEngine;
using UnityEngine.Events;

public class ObjectInteractiveItemTake : MonoBehaviour
{
	[Header("Общее")]
	public Transform pivotHand;

	[Header("Right")]
	public PlayerHandIK_Prefab rightHand;

	public Quaternion rotationRightHand;

	public Vector3 positionRightHand;

	public Quaternion[] rotationsRightFingers;

	[Header("Left")]
	public PlayerHandIK_Prefab leftHand;

	public Quaternion rotationLeftHand;

	public Vector3 positionLeftHand;

	public Quaternion[] rotationsLeftFingers;

	[Header("События")]
	public UnityEvent eventTake;

	private bool lerpPositionInHand;

	private Vector3 positionInHand;

	private Quaternion rotationInHand;

	private Transform player;

	[HideInInspector]
	public PlayerHandIK_Prefab handTake;

	private void Start()
	{
		player = GlobalTag.player.transform;
		if (pivotHand == null)
		{
			if (rightHand != null)
			{
				Object.Destroy(rightHand.gameObject);
			}
			if (leftHand != null)
			{
				Object.Destroy(leftHand.gameObject);
			}
		}
		if (GetComponent<ObjectInteractive>() != null)
		{
			GetComponent<ObjectInteractive>().eventClick.AddListener(Take);
		}
	}

	private void Update()
	{
		if (lerpPositionInHand)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, positionInHand, Time.deltaTime * 30f);
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, rotationInHand, Time.deltaTime * 30f);
		}
		if (pivotHand != null)
		{
			pivotHand.rotation = Quaternion.LookRotation(player.position - base.transform.position, Vector3.up);
			pivotHand.rotation = Quaternion.Euler(new Vector3(-90f, pivotHand.eulerAngles.y, 0f));
		}
	}

	public void Take()
	{
		PlayerPersonIK component = GameObject.FindWithTag("Player").transform.Find("Person").GetComponent<PlayerPersonIK>();
		bool freeRightHand = component.freeRightHand;
		PlayerHandIK_Prefab playerHandIK_Prefab = null;
		if (freeRightHand)
		{
			if (pivotHand == null)
			{
				GameObject obj = Object.Instantiate(Resources.Load("Hands/Hand Right") as GameObject, base.transform);
				obj.transform.localPosition = positionRightHand;
				obj.transform.localRotation = rotationRightHand;
				playerHandIK_Prefab = obj.GetComponent<PlayerHandIK_Prefab>();
				for (int i = 0; i < playerHandIK_Prefab.transforms.Length; i++)
				{
					playerHandIK_Prefab.transforms[i].localRotation = rotationsRightFingers[i];
				}
			}
			else
			{
				playerHandIK_Prefab = rightHand;
			}
			playerHandIK_Prefab.Start();
			GetPositionInHand(playerHandIK_Prefab.gameObject);
		}
		if (!freeRightHand)
		{
			if (pivotHand == null)
			{
				GameObject gameObject = Object.Instantiate(Resources.Load("Hands/Hand Left") as GameObject, base.transform);
				gameObject.transform.localPosition = positionLeftHand;
				gameObject.transform.localRotation = rotationLeftHand;
				playerHandIK_Prefab = gameObject.GetComponent<PlayerHandIK_Prefab>();
				for (int j = 0; j < playerHandIK_Prefab.transforms.Length; j++)
				{
					playerHandIK_Prefab.transforms[j].localRotation = rotationsLeftFingers[j];
				}
				playerHandIK_Prefab.Start();
				GetPositionInHand(gameObject);
			}
			else
			{
				playerHandIK_Prefab = leftHand;
			}
			playerHandIK_Prefab.Start();
			GetPositionInHand(playerHandIK_Prefab.gameObject);
		}
		handTake = playerHandIK_Prefab;
		component.TakeItemPocket(playerHandIK_Prefab, base.gameObject);
		GlobalAM.DestroyColliders(base.gameObject);
	}

	public void TakeInHand(Transform _parent)
	{
		base.transform.parent = _parent;
		lerpPositionInHand = true;
	}

	public void TakeDestroy()
	{
		eventTake.Invoke();
		Object.Destroy(base.gameObject);
	}

	private void GetPositionInHand(GameObject _objectHand)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = _objectHand.transform;
		gameObject.transform.position = base.transform.position;
		gameObject.transform.rotation = base.transform.rotation;
		positionInHand = gameObject.transform.localPosition;
		rotationInHand = gameObject.transform.localRotation;
		Object.Destroy(gameObject);
	}
}
