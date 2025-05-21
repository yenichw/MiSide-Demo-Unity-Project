using UnityEngine;

public class Location14_PlayerMobileMove : MonoBehaviour
{
	[Header("Игрок")]
	public Transform head;

	public float speedPhysic = 100f;

	private bool destroySmooth;

	private bool lightActive;

	[Header("Дополнение игрока")]
	public Light lightPlayer;

	private bool play;

	private int hMove;

	private int vMove;

	private Rigidbody rb;

	private float rotHead;

	private float rotBody;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		lightActive = true;
	}

	private void Update()
	{
		if (play)
		{
			hMove = 0;
			vMove = 0;
			if ((double)Input.GetAxis("Horizontal") > 0.5)
			{
				hMove++;
			}
			if ((double)Input.GetAxis("Horizontal") < -0.5)
			{
				hMove--;
			}
			if ((double)Input.GetAxis("Vertical") > 0.5)
			{
				vMove++;
			}
			if ((double)Input.GetAxis("Vertical") < -0.5)
			{
				vMove--;
			}
			rotBody += Input.GetAxis("Mouse X") * (1f + GlobalGame.mouseSpeed);
			if (rotBody > 360f)
			{
				rotBody -= 360f;
			}
			if (rotBody < 0f)
			{
				rotBody += 360f;
			}
			rotHead -= Input.GetAxis("Mouse Y") * (1f + GlobalGame.mouseSpeed);
			rotHead = Mathf.Clamp(rotHead, -89f, 89f);
		}
		if (lightActive)
		{
			if (lightPlayer.intensity < 1f)
			{
				lightPlayer.intensity += Time.deltaTime * 2f;
				if (lightPlayer.intensity > 1f)
				{
					lightPlayer.intensity = 1f;
				}
			}
		}
		else
		{
			if (!(lightPlayer.intensity > 0f))
			{
				return;
			}
			lightPlayer.intensity -= Time.deltaTime * 2f;
			if (lightPlayer.intensity <= 0f)
			{
				lightPlayer.intensity = 0f;
				if (destroySmooth)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		rb.velocity = (base.transform.forward * vMove + base.transform.right * hMove) * speedPhysic * Time.fixedDeltaTime;
		if (!GlobalGame.mouseSpeedLerp)
		{
			head.localRotation = Quaternion.Euler(rotHead, 0f, 0f);
			base.transform.rotation = Quaternion.Euler(0f, rotBody, 0f);
		}
		else
		{
			head.localRotation = Quaternion.Lerp(head.localRotation, Quaternion.Euler(rotHead, 0f, 0f), Time.deltaTime * 30f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, rotBody, 0f), Time.deltaTime * 30f);
		}
	}

	public void Play()
	{
		play = true;
		rb.isKinematic = false;
		base.gameObject.layer = 0;
		GetComponent<CapsuleCollider>().isTrigger = false;
	}

	public void Stop()
	{
		play = false;
		hMove = 0;
		vMove = 0;
	}

	public void DestroySmooth()
	{
		lightActive = false;
		destroySmooth = true;
	}

	public void LightActivation(bool x)
	{
		lightActive = x;
	}
}
