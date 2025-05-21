using UnityEngine;
using UnityEngine.Events;

public class ChibiRoomLogic_Main : MonoBehaviour
{
	[Header("Общее")]
	public GameObject chibiMita;

	[Header("Room")]
	public Mob_ChibiMita_Animation animationNopeKey;

	private bool cartHold;

	[Header("Цех")]
	public Transform pointCart;

	public Transform pivotCart;

	public AnimationClip animationMitaWalkCart;

	public AnimationClip animationMitaWalk;

	public UnityEvent eventWalkToCart;

	public Mob_ChibiMita_Animation animationDropCart;

	public GameObject[] irons;

	private void Start()
	{
	}

	private void Update()
	{
		if (cartHold)
		{
			pivotCart.transform.position = GlobalAM.TransformPivot(chibiMita.transform, new Vector3(0f, 0f, 1.75f) * 0.1f);
			pivotCart.transform.rotation = chibiMita.transform.rotation * Quaternion.Euler(-90f, 0f, 90f);
		}
	}

	public void NopeKey()
	{
		animationNopeKey.transform.position = chibiMita.transform.position;
		animationNopeKey.AnimationPlay();
	}

	public void GoCart()
	{
		chibiMita.GetComponent<Mob_ChibiMita>().GoWalk(pointCart.position, eventWalkToCart);
	}

	public void CartTake()
	{
		cartHold = true;
		pivotCart.gameObject.SetActive(value: true);
		chibiMita.GetComponent<Mob_ChibiMita>().ReAnimationMove(animationMitaWalkCart);
		chibiMita.transform.SetPositionAndRotation(pointCart.position, pointCart.rotation);
		chibiMita.GetComponent<Mob_ChibiMita>().AnimatorReset();
		animationDropCart.AnimationMovePlay();
		pivotCart.transform.position = GlobalAM.TransformPivot(chibiMita.transform, new Vector3(0f, 0f, 1.75f) * 0.1f);
		pivotCart.transform.rotation = chibiMita.transform.rotation * Quaternion.Euler(-90f, 0f, 90f);
	}

	public void CartWasDrop()
	{
		chibiMita.GetComponent<Mob_ChibiMita>().ReAnimationMove(animationMitaWalk);
		cartHold = false;
	}

	public void IronDestroy()
	{
		for (int i = 0; i < irons.Length; i++)
		{
			Object.Destroy(irons[i]);
		}
	}
}
