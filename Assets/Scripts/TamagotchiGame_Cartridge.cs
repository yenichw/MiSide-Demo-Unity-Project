using EPOOutline;
using UnityEngine;

public class TamagotchiGame_Cartridge : MonoBehaviour
{
	public GameObject cartridge1;

	public GameObject cartridge2;

	public GameObject cartridge3;

	public Tamagotchi_AddMoney moneyAdd;

	public int energy;

	public AudioSource audioTrash;

	private bool cassette1Ready;

	private bool cassette2Ready;

	private bool cassette3Ready;

	private GameObject takeCassette;

	private Vector2 rotation;

	private Vector2 rotationTime;

	private void Start()
	{
		RestartGame();
	}

	private void Update()
	{
		if (!(takeCassette != null))
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			rotationTime = Vector2.zero;
		}
		if (Input.GetMouseButton(0))
		{
			if ((double)Vector2.Distance(Vector2.zero, rotationTime) > 0.5)
			{
				rotation -= new Vector2(Input.GetAxis("Mouse Y"), 0f - Input.GetAxis("Mouse X")) * 3f;
			}
			else
			{
				rotationTime += new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
			}
		}
		takeCassette.transform.rotation = Quaternion.Lerp(takeCassette.transform.rotation, Quaternion.Euler(new Vector3(rotation.x, 0f, rotation.y)), Time.deltaTime * 15f);
		takeCassette.transform.position = Vector3.Lerp(takeCassette.transform.position, new Vector3(0f, 11.5f, 1f), Time.deltaTime * 15f);
	}

	public void RestartGame()
	{
		takeCassette = null;
		cassette1Ready = false;
		cartridge1.SetActive(value: false);
		cartridge1.transform.localPosition = new Vector3(0f, 1.25f, -1.5f);
		cartridge1.transform.localRotation = Quaternion.Euler(90f, 180 + Random.Range(-70, 70), 0f);
		cartridge1.GetComponent<BoxCollider>().enabled = true;
		cartridge1.GetComponent<Rigidbody>().isKinematic = false;
		cartridge1.GetComponent<TamagotchiGame_Cartridge_Cartridge>().Restart();
		cassette2Ready = false;
		cartridge2.SetActive(value: false);
		cartridge2.transform.localPosition = new Vector3(0f, 1.1f, -1.5f);
		cartridge2.transform.localRotation = Quaternion.Euler(90f, 180 + Random.Range(-70, 70), 0f);
		cartridge2.GetComponent<BoxCollider>().enabled = true;
		cartridge2.GetComponent<Rigidbody>().isKinematic = false;
		cartridge2.GetComponent<TamagotchiGame_Cartridge_Cartridge>().Restart();
		cassette3Ready = false;
		cartridge3.SetActive(value: false);
		cartridge3.transform.localPosition = new Vector3(0f, 0.95f, -1.5f);
		cartridge3.transform.localRotation = Quaternion.Euler(90f, 180 + Random.Range(-70, 70), 0f);
		cartridge3.GetComponent<BoxCollider>().enabled = true;
		cartridge3.GetComponent<Rigidbody>().isKinematic = false;
		cartridge3.GetComponent<TamagotchiGame_Cartridge_Cartridge>().Restart();
	}

	public void TakeCassette(GameObject _object)
	{
		takeCassette = _object;
		cartridge1.GetComponent<Outlinable>().enabled = false;
		cartridge2.GetComponent<Outlinable>().enabled = false;
		cartridge3.GetComponent<Outlinable>().enabled = false;
		cartridge1.GetComponent<Trigger_MouseClick>().enabled = false;
		cartridge2.GetComponent<Trigger_MouseClick>().enabled = false;
		cartridge3.GetComponent<Trigger_MouseClick>().enabled = false;
		takeCassette.GetComponent<Rigidbody>().isKinematic = true;
		takeCassette.GetComponent<BoxCollider>().enabled = false;
		takeCassette.GetComponent<TamagotchiGame_Cartridge_Cartridge>().Take();
		GetComponent<Tamagotchi_MiniGame>().SetLerpCameraSize(0.5f);
		rotation = new Vector2(80f, 150f);
	}

	public void CasseteReady(GameObject _object)
	{
		if (_object == cartridge1)
		{
			cassette1Ready = true;
		}
		if (_object == cartridge2)
		{
			cassette2Ready = true;
		}
		if (_object == cartridge3)
		{
			cassette3Ready = true;
		}
		if (cassette1Ready && cassette2Ready && cassette3Ready)
		{
			moneyAdd.StartAddMoney(150, energy);
		}
		if (!cassette1Ready)
		{
			cartridge1.GetComponent<Trigger_MouseClick>().enabled = true;
		}
		if (!cassette2Ready)
		{
			cartridge2.GetComponent<Trigger_MouseClick>().enabled = true;
		}
		if (!cassette3Ready)
		{
			cartridge3.GetComponent<Trigger_MouseClick>().enabled = true;
		}
		takeCassette = null;
		GetComponent<Tamagotchi_MiniGame>().SetLerpCameraSize(1.5f);
	}

	public void SwitchPut()
	{
		cartridge1.SetActive(value: true);
		cartridge2.SetActive(value: true);
		cartridge3.SetActive(value: true);
		cartridge1.GetComponent<Trigger_MouseClick>().enabled = true;
		cartridge2.GetComponent<Trigger_MouseClick>().enabled = true;
		cartridge3.GetComponent<Trigger_MouseClick>().enabled = true;
	}

	public void Trash()
	{
		audioTrash.Play();
	}
}
