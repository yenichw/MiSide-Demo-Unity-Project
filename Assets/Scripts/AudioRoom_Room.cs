using UnityEngine;

public class AudioRoom_Room : MonoBehaviour
{
	public AudioRoom_Audio[] audioSources;

	private int timeCheck;

	private bool inRoom;

	private LayerMask layerPlayer;

	[Header("КОМНАТА")]
	public Vector3 sizeRoom;

	private void Start()
	{
		layerPlayer = GameObject.FindWithTag("GameController").GetComponent<GameController>().layerPlayer;
	}

	private void Update()
	{
		timeCheck++;
		if (timeCheck > 5)
		{
			timeCheck = 0;
			CheckInRoom();
		}
	}

	private void CheckInRoom()
	{
		if (Physics.OverlapBox(base.transform.position, sizeRoom, base.transform.rotation, layerPlayer).Length != 0)
		{
			if (!inRoom)
			{
				inRoom = true;
				for (int i = 0; i < audioSources.Length; i++)
				{
					audioSources[i].PlayerInRoom(x: true);
				}
			}
		}
		else if (inRoom)
		{
			inRoom = false;
			for (int j = 0; j < audioSources.Length; j++)
			{
				audioSources[j].PlayerInRoom(x: false);
			}
		}
	}
}
