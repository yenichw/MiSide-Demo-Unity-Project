using UnityEngine;

public class ObjectInHand : MonoBehaviour
{
	public ObjectInHandPosition[] positions;

	private PlayerMove scrpm;

	private Vector3 positionOrigin;

	private Quaternion rotationOrigin;

	private bool dropOrigin;

	private Transform mainParent;

	[Header("Информация")]
	public int indexPosition;

	private void Start()
	{
		scrpm = GameObject.FindWithTag("Player").GetComponent<PlayerMove>();
		positionOrigin = base.transform.localPosition;
		rotationOrigin = base.transform.localRotation;
		mainParent = base.transform.parent;
		indexPosition = -1;
	}

	private void Update()
	{
		if (dropOrigin)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, positionOrigin, Time.deltaTime * 5f);
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, rotationOrigin, Time.deltaTime * 5f);
		}
		else if (indexPosition >= 0)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, positions[indexPosition].position, Time.deltaTime * 5f);
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.Euler(positions[indexPosition].rotation), Time.deltaTime * 5f);
		}
	}

	public void DropOrigin()
	{
		base.transform.parent = mainParent;
		dropOrigin = true;
	}

	public void TakeInHand(int _index)
	{
		dropOrigin = false;
		indexPosition = _index;
		if (positions[_index].rightHand)
		{
			base.transform.parent = scrpm.scrppik.rightItemFixPosition;
		}
		else
		{
			base.transform.parent = scrpm.scrppik.leftItemFixPosition;
		}
	}
}
