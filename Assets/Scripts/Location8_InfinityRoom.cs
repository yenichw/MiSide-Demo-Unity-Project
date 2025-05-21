using UnityEngine;
using UnityEngine.Events;

public class Location8_InfinityRoom : MonoBehaviour
{
	private Transform playerT;

	private bool canNextIndex;

	[SerializeField]
	private int indexRoom;

	[Header("Комнаты")]
	public Loc8_Room[] roomsEvents;

	private bool teleportRandomMita;

	private bool teleportRandomMitaWindow;

	[Header("Мита")]
	public Location8_MitaBrokeLife mitaLife;

	public UnityEvent eventMitaWindow;

	[Header("Монстр")]
	public Location8_SlouchLife slouchLife;

	private void Start()
	{
		canNextIndex = true;
		playerT = GlobalTag.player.transform;
	}

	public void StartNextRoom()
	{
		playerT.position += new Vector3(-10f, 0f, 11.333333f);
		if (canNextIndex)
		{
			indexRoom++;
			for (int i = 0; i < roomsEvents.Length; i++)
			{
				if (indexRoom == roomsEvents[i].numberRoom)
				{
					roomsEvents[i].eventEnter.Invoke();
					break;
				}
			}
		}
		if (teleportRandomMita)
		{
			mitaLife.TeleportRandom();
		}
		if (teleportRandomMitaWindow)
		{
			MitaWindowTeleport();
		}
		if (slouchLife.gameObject.activeSelf)
		{
			slouchLife.SlouchBack();
		}
	}

	public void CanNext(bool _x)
	{
		canNextIndex = _x;
	}

	public void MitaTeleportActive(bool x)
	{
		teleportRandomMita = x;
		if (!x)
		{
			teleportRandomMitaWindow = true;
		}
	}

	public void MitaWindowTeleport()
	{
		mitaLife.StopWalk();
		eventMitaWindow.Invoke();
		mitaLife.transform.position = new Vector3(-9.056f, 0f, -14.818f);
		mitaLife.transform.rotation = Quaternion.Euler(0f, 328.77f, 0f);
	}
}
