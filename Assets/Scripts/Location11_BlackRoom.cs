using UnityEngine;

public class Location11_BlackRoom : MonoBehaviour
{
	public GameObject roomExample;

	public GameObject[] destroysRepeatStart;

	public Transform doorRepeat;

	public ObjectDoor doorA;

	public ObjectDoor doorB;

	public Transform mitaBlack;

	private GameObject mitaBlackCopy;

	private Transform[] transformsCopyMita;

	private Transform[] transformsMita;

	private float timeDoorClose;

	private GameObject roomRepeat;

	private Transform playerT;

	private bool startRepeat;

	private void Start()
	{
		playerT = GlobalTag.player.transform;
	}

	private void Update()
	{
		if (playerT.position.x < -5f && !startRepeat)
		{
			StartRepeat();
		}
		if (!startRepeat)
		{
			return;
		}
		if (playerT.position.x < -3.5f)
		{
			roomRepeat.transform.position = new Vector3(-2.98f, 0f, -0.49f);
			doorRepeat.localPosition = new Vector3(-11.125f, 0f, 0.49f);
		}
		else
		{
			roomRepeat.transform.position = new Vector3(10.98f, 0f, -0.49f);
			doorRepeat.localPosition = new Vector3(-4.145f, 0f, 0.49f);
		}
		if (doorA.open || doorB.open)
		{
			timeDoorClose = 0.5f;
			if (!roomRepeat.activeSelf)
			{
				roomRepeat.SetActive(value: true);
				doorRepeat.gameObject.SetActive(value: true);
			}
			RepeatUpdate();
		}
		else if (timeDoorClose > 0f)
		{
			timeDoorClose -= Time.deltaTime;
			if (timeDoorClose <= 0f)
			{
				roomRepeat.SetActive(value: false);
				doorRepeat.gameObject.SetActive(value: false);
				timeDoorClose = 0f;
				if (playerT.transform.position.x > -0.14f)
				{
					playerT.transform.position -= new Vector3(6.98f, 0f, 0f);
				}
				if (playerT.transform.position.x < -7.125f)
				{
					playerT.transform.position += new Vector3(6.98f, 0f, 0f);
				}
			}
		}
		if (timeDoorClose > 0f)
		{
			RepeatUpdate();
		}
	}

	private void RepeatUpdate()
	{
		mitaBlackCopy.transform.SetLocalPositionAndRotation(mitaBlack.localPosition, mitaBlack.localRotation);
		mitaBlackCopy.GetComponent<Transform_GetAllTransforms>().CopyTransformPos(mitaBlack.GetComponent<Transform_GetAllTransforms>());
	}

	private void StartRepeat()
	{
		startRepeat = true;
		roomRepeat = Object.Instantiate(roomExample, roomExample.transform.parent);
		roomRepeat.transform.position = new Vector3(-2.97f, 0f, -0.49f);
		roomRepeat.SetActive(value: false);
		doorRepeat.gameObject.SetActive(value: false);
		doorRepeat.parent = roomRepeat.transform;
		doorRepeat.localPosition = new Vector3(-11.145f, 0f, 0.49f);
		for (int i = 0; i < destroysRepeatStart.Length; i++)
		{
			if (destroysRepeatStart[i] != null)
			{
				Object.Destroy(destroysRepeatStart[i]);
			}
		}
		mitaBlack.parent = roomExample.transform;
		mitaBlack.GetComponent<Transform_GetAllTransforms>().Scan();
		mitaBlackCopy = Object.Instantiate(mitaBlack.gameObject, roomRepeat.transform);
		mitaBlackCopy.transform.SetLocalPositionAndRotation(mitaBlack.localPosition, mitaBlack.localRotation);
		Object.Destroy(mitaBlackCopy.GetComponent<Animator>());
	}
}
