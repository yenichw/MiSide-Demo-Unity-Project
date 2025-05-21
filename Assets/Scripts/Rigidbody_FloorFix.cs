using UnityEngine;

public class Rigidbody_FloorFix : MonoBehaviour
{
	public float height;

	public float floorY;

	private float timeDontSound;

	private AudioSource au;

	[Header("Звуки")]
	public AudioClip[] soundsFall;

	[Header("Настройки")]
	public bool isKinematicAfter;

	public bool destroyColliderAfter;

	private void Start()
	{
		au = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (base.transform.position.y < floorY + height)
		{
			Fall();
		}
		if (timeDontSound > 0f)
		{
			timeDontSound -= Time.deltaTime;
			if (timeDontSound < 0f)
			{
				timeDontSound = 0f;
			}
		}
	}

	private void Fall()
	{
		base.transform.position = new Vector3(base.transform.position.x, floorY + height, base.transform.position.z);
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		timeDontSound = 0.75f;
		au.clip = soundsFall[Random.Range(0, soundsFall.Length)];
		au.pitch = Random.Range(0.95f, 1.05f);
		au.Play();
		if (isKinematicAfter)
		{
			Object.Destroy(GetComponent<Rigidbody>());
			Object.Destroy(this);
			if (GetComponent<Rigidbody_StartVelocity>() != null)
			{
				Object.Destroy(GetComponent<Rigidbody_StartVelocity>());
			}
		}
		if (destroyColliderAfter && GetComponent<Collider>() != null)
		{
			Object.Destroy(GetComponent<Collider>());
		}
	}
}
