using UnityEngine;

public class Location12_LongNeck : MonoBehaviour
{
	public Transform targetFollow;

	public Transform target;

	public Transform[] neck;

	public Transform neckStart;

	public AnimationCurve curveNeck;

	public LayerMask layerWall;

	public Vector3[] neckOrigin;

	private Vector3 positionHeadWas;

	[Header("Звуки")]
	public AudioSource audioNeck;

	private RaycastHit hit;

	private Vector3 targetRandomMove;

	private float resetRandom;

	private void Start()
	{
		targetFollow = GlobalTag.cameraPlayer.transform;
		for (int i = 0; i < neck.Length; i++)
		{
			neckOrigin[i] = neck[i].position;
		}
		positionHeadWas = neck[21].position;
	}

	private void Update()
	{
		for (int i = 0; i < neck.Length; i++)
		{
			neck[i].position = Vector3.Lerp(neckOrigin[i], target.position, (float)i / (float)neck.Length) + Vector3.up * curveNeck.Evaluate((float)i / (float)neck.Length) * Mathf.Clamp(Vector3.Distance(neckStart.position, target.position) - 0.25f, 0f, 1f);
			if (i > 0 && i < neck.Length)
			{
				neck[i - 1].rotation = Quaternion.LookRotation(neck[i].position - neck[i - 1].position, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
			}
		}
		neck[neck.Length - 1].rotation = target.rotation;
		targetRandomMove += GlobalAM.Vector3Random(-0.1f, 0.1f) * Time.deltaTime;
		resetRandom += Time.deltaTime;
		if (resetRandom > 1f)
		{
			resetRandom = 0f;
			targetRandomMove = Vector3.zero;
		}
		audioNeck.volume = Mathf.Lerp(audioNeck.volume, Vector3.Distance(positionHeadWas, neck[21].position), Time.deltaTime * 5f);
		positionHeadWas = Vector3.Lerp(positionHeadWas, neck[21].position, Time.deltaTime * 5f);
	}

	private void FixedUpdate()
	{
		if (!Physics.Raycast(neckOrigin[neck.Length - 1], targetFollow.position - neckOrigin[neck.Length - 1], out hit, Vector3.Distance(neckOrigin[neck.Length - 1], targetFollow.position), layerWall))
		{
			if (Vector3.Distance(neckOrigin[neck.Length - 1], targetFollow.position) < 3f)
			{
				target.position = Vector3.Lerp(target.position, Vector3.MoveTowards(targetFollow.position, neckOrigin[neck.Length - 1], 0.35f) + targetRandomMove, Time.fixedDeltaTime * 10f);
			}
			else
			{
				target.position = Vector3.Lerp(target.position, Vector3.MoveTowards(neckOrigin[neck.Length - 1], targetFollow.position, 3f) + targetRandomMove, Time.fixedDeltaTime * 10f);
			}
		}
		else
		{
			target.position = Vector3.Lerp(target.position, Vector3.MoveTowards(hit.point, neckOrigin[neck.Length - 1], 0.35f) + targetRandomMove, Time.fixedDeltaTime * 10f);
		}
		target.rotation = Quaternion.Lerp(target.rotation, Quaternion.LookRotation(targetFollow.position - target.position, Vector3.up), Time.fixedDeltaTime * 8f);
	}
}
