using UnityEngine;

[RequireComponent(typeof(LifeObject))]
public class Mob_Cockroach : MonoBehaviour
{
	private Transform player;

	[Header("Layers")]
	public LayerMask layerWalls;

	private RaycastHit hit;

	private float timeMoveRandom;

	private float timeMoveForward;

	private float timeRotationRandom;

	private float timeRotation;

	private float speed;

	private bool smoothDestroy;

	private AudioSource audioSource;

	[Header("Settings")]
	public float timeDestroySmooth;

	private void Start()
	{
		player = GameObject.FindWithTag("Player").transform;
		speed = 1f;
		timeMoveRandom = Random.Range(0f, 15f);
		timeRotationRandom = Random.Range(0f, 0.5f);
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, -base.transform.up, out hit, 1f, layerWalls))
		{
			base.transform.position = hit.point + base.transform.up * 0.005f;
		}
		audioSource = GetComponent<AudioSource>();
		audioSource.time = Random.Range(0f, 1f);
	}

	private void Update()
	{
		if (timeMoveForward == 0f)
		{
			timeMoveRandom -= Time.deltaTime;
		}
		if (timeMoveRandom < 0f)
		{
			timeMoveRandom = Random.Range(0f, 15f);
			timeMoveForward = Random.Range(1f, 3f);
		}
		if (timeMoveForward > 0f)
		{
			timeMoveForward -= Time.deltaTime;
			if (timeMoveForward < 0f)
			{
				timeMoveForward = 0f;
				timeRotation = 0f;
			}
			base.transform.position += base.transform.forward * (Time.deltaTime / 4f * speed);
			if (Physics.Raycast(base.transform.position, base.transform.forward, out hit, Time.deltaTime * speed, layerWalls))
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			}
			if (!Physics.Raycast(base.transform.position + base.transform.forward * 0.02f, -base.transform.up, 0.01f, layerWalls) && Physics.Raycast(base.transform.position + base.transform.forward * 0.02f, -base.transform.up - base.transform.forward, out hit, 1f, layerWalls))
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
				base.transform.position = hit.point + base.transform.up * 0.005f;
			}
			if (Physics.Raycast(base.transform.position, -base.transform.up, out hit, 1f, layerWalls))
			{
				base.transform.position = hit.point + base.transform.up * 0.005f;
			}
			timeRotationRandom -= Time.deltaTime;
			if (timeRotationRandom <= 0f)
			{
				timeRotationRandom = Random.Range(0f, 0.5f);
				timeRotation = Random.Range(-3f, 3f);
			}
			if (timeRotation > 0f)
			{
				timeRotation -= Time.deltaTime;
				if (timeRotation < 0f)
				{
					timeRotation = 0f;
				}
				base.transform.rotation *= Quaternion.Euler(0f, Time.deltaTime * 180f, 0f);
			}
			if (timeRotation < 0f)
			{
				timeRotation += Time.deltaTime;
				if (timeRotation > 0f)
				{
					timeRotation = 0f;
				}
				base.transform.rotation *= Quaternion.Euler(0f, Time.deltaTime * -180f, 0f);
			}
		}
		if ((double)GlobalAM.DistanceFloor(base.transform.position, player.position) < 1.5)
		{
			timeMoveRandom = 1f;
			timeMoveForward = 2f;
			speed = 3f;
		}
		else
		{
			speed = 1f;
		}
		if (smoothDestroy)
		{
			audioSource.volume -= Time.deltaTime * 0.5f;
			base.transform.localScale -= Vector3.one * (Time.deltaTime * 0.5f);
			if (base.transform.localScale.x <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		if (timeDestroySmooth > 0f)
		{
			timeDestroySmooth -= Time.deltaTime;
			if (timeDestroySmooth <= 0f)
			{
				timeDestroySmooth = 0f;
				SmoothDestroy();
			}
		}
	}

	public void SmoothDestroy()
	{
		smoothDestroy = true;
	}
}
