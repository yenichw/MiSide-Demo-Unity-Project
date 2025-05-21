using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class AudioRoom_Audio : MonoBehaviour
{
	public float minimcalClamp;

	private float cut;

	private AudioLowPassFilter passFilter;

	private bool inRoom;

	private LayerMask layerWall;

	private Transform headPlayer;

	private bool lineCastPlayer;

	private int timeCheck;

	private void Start()
	{
		passFilter = GetComponent<AudioLowPassFilter>();
		layerWall = GameObject.FindWithTag("GameController").GetComponent<GameController>().layerPhysicSound;
		headPlayer = GameObject.FindWithTag("Player").GetComponent<PlayerMove>().head;
		FastCastSound();
	}

	private void Update()
	{
		timeCheck++;
		if (timeCheck > 5)
		{
			CheckCast();
		}
		if (!inRoom)
		{
			if (!lineCastPlayer)
			{
				cut = 100f;
			}
			else
			{
				cut = 10000f;
			}
		}
		if (inRoom)
		{
			if (!lineCastPlayer)
			{
				cut = 2000f;
			}
			else
			{
				cut = 10000f;
			}
		}
		if (cut < minimcalClamp)
		{
			cut = minimcalClamp;
		}
		passFilter.cutoffFrequency = Mathf.Lerp(passFilter.cutoffFrequency, cut, Time.deltaTime * 10f);
	}

	public void PlayerInRoom(bool x)
	{
		inRoom = x;
		CheckCast();
	}

	private void CheckCast()
	{
		timeCheck = 0;
		if (inRoom)
		{
			if (Physics.Linecast(headPlayer.position + GlobalAM.Direction(base.transform.position, headPlayer.position) * 0.2f, base.transform.position, layerWall))
			{
				lineCastPlayer = false;
			}
			else
			{
				lineCastPlayer = true;
			}
		}
		else if (Physics.Linecast(headPlayer.position, base.transform.position, layerWall))
		{
			lineCastPlayer = false;
		}
		else
		{
			lineCastPlayer = true;
		}
	}

	private void FastCastSound()
	{
		CheckCast();
		if (!inRoom)
		{
			if (!lineCastPlayer)
			{
				cut = 100f;
			}
			else
			{
				cut = 10000f;
			}
		}
		if (inRoom)
		{
			if (!lineCastPlayer)
			{
				cut = 2000f;
			}
			else
			{
				cut = 10000f;
			}
		}
		if (cut < minimcalClamp)
		{
			cut = minimcalClamp;
		}
		passFilter.cutoffFrequency = cut;
	}
}
