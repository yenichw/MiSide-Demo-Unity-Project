using UnityEngine;

public class Time_ObjectOrderActive : MonoBehaviour
{
	public bool onStart = true;

	public GameObject[] objects;

	public float timeActiveOrder = 0.1f;

	[Header("MiSide")]
	public AudioSource audioFoot;

	public AudioClip[] soundsFoot;

	private int indexNextObject;

	private bool active;

	private float timeNext;

	private void Start()
	{
		if (onStart)
		{
			active = true;
		}
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (timeNext > 0f)
		{
			timeNext -= Time.deltaTime;
			if (timeNext <= 0f)
			{
				timeNext = 0f;
				objects[indexNextObject].SetActive(value: true);
				if (audioFoot != null)
				{
					audioFoot.transform.position = objects[indexNextObject].transform.position;
					audioFoot.clip = soundsFoot[Random.Range(0, soundsFoot.Length)];
					audioFoot.pitch = Random.Range(0.9f, 1.1f);
					audioFoot.Play();
				}
				indexNextObject++;
				if (indexNextObject > objects.Length - 1)
				{
					Object.Destroy(this);
				}
			}
		}
		if (timeNext == 0f)
		{
			timeNext = timeActiveOrder;
		}
	}
}
