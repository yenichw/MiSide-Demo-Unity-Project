using UnityEngine;

public class TamagotchiGame_Sorting_Item : MonoBehaviour
{
	[HideInInspector]
	public int typeItem;

	[HideInInspector]
	public bool soundActive;

	private AudioSource audioSource;

	private AudioClip[] soundsDrop;

	private Rigidbody rb;

	private void Update()
	{
		if (base.transform.position.y < 9f)
		{
			base.transform.position = new Vector3(base.transform.position.x, 9.05f, base.transform.position.z);
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	public void RestartPosition(int _typeItem, Mesh _mesh, AudioClip[] _soundsDrop)
	{
		rb = GetComponent<Rigidbody>();
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		base.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360)));
		rb.isKinematic = true;
		typeItem = _typeItem;
		GetComponent<MeshFilter>().mesh = _mesh;
		GetComponent<Trigger_MouseClick>().ActivatedObject(x: true);
		audioSource = GetComponent<AudioSource>();
		soundsDrop = _soundsDrop;
		soundActive = true;
	}

	public void StartItem()
	{
		rb.isKinematic = false;
	}

	public void Take()
	{
		rb.isKinematic = true;
	}

	public void RigidbodyFreeze(bool x)
	{
		rb.isKinematic = x;
	}

	public void NoRight()
	{
		soundActive = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (soundActive)
		{
			soundActive = false;
			audioSource.clip = soundsDrop[Random.Range(0, soundsDrop.Length)];
			audioSource.pitch = Random.Range(0.95f, 1.05f);
			audioSource.Play();
		}
	}
}
