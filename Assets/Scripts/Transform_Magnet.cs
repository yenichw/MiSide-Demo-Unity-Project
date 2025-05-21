using UnityEngine;

[AddComponentMenu("Functions/Transform/Magnet")]
public class Transform_Magnet : MonoBehaviour
{
	public bool active = true;

	[Header("Объекты")]
	public string magnetPlayer;

	public Transform transformTarget;

	public Transform transformMagnet;

	[Header("Позиции")]
	public float speed = 5f;

	public bool local = true;

	public bool localMagnet;

	public Vector3 position;

	public Vector3 rotation;

	[Header("Шум")]
	public bool noiseActive;

	public float noiseTimeMin;

	public float noiseTimeMax;

	private float noiseTimeNow;

	public float noiseMin;

	public float noiseMax;

	private float noise;

	private int sharply;

	private bool timeParent;

	private Vector3 positionOrigin;

	private Quaternion rotationOrigin;

	private Transform reMagnetAndParent;

	private bool fs;

	private void Start()
	{
		if (fs)
		{
			return;
		}
		fs = true;
		if (transformMagnet == null)
		{
			transformMagnet = base.transform;
		}
		noiseTimeNow = Random.Range(noiseTimeMin, noiseTimeMax);
		if (magnetPlayer == null || !(magnetPlayer != ""))
		{
			return;
		}
		bool flag = false;
		if (magnetPlayer == "Right item")
		{
			flag = true;
			transformMagnet = GlobalTag.player.transform.Find("RightItem FixPosition");
		}
		if (magnetPlayer == "Left item")
		{
			flag = true;
			transformMagnet = GlobalTag.player.transform.Find("LeftItem FixPosition");
		}
		if (flag)
		{
			return;
		}
		Component[] componentsInChildren = GlobalTag.player.transform.Find("Person").GetComponentsInChildren(typeof(Transform), includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name == magnetPlayer)
			{
				transformMagnet = componentsInChildren[i].transform;
				break;
			}
		}
	}

	private void LateUpdate()
	{
		if (!(transformMagnet != null) || !(transformTarget != null))
		{
			return;
		}
		if (active)
		{
			if (!local)
			{
				if (speed >= 0f)
				{
					if (!localMagnet)
					{
						transformTarget.SetPositionAndRotation(Vector3.Lerp(transformTarget.position, position, Time.deltaTime * speed), Quaternion.Lerp(transformTarget.rotation, Quaternion.Euler(rotation), Time.deltaTime * speed));
					}
					else
					{
						transformTarget.SetPositionAndRotation(Vector3.Lerp(transformTarget.position, GlobalAM.TransformPivot(transformMagnet, position), Time.deltaTime * speed), Quaternion.Lerp(transformTarget.rotation, transformMagnet.rotation * Quaternion.Euler(rotation), Time.deltaTime * speed));
					}
				}
				else
				{
					transformTarget.SetPositionAndRotation(position, Quaternion.Euler(rotation));
				}
			}
			else if (speed >= 0f)
			{
				if (!localMagnet)
				{
					transformTarget.localPosition = Vector3.Lerp(transformTarget.localPosition, position, Time.deltaTime * speed);
					transformTarget.localRotation = Quaternion.Lerp(transformTarget.localRotation, Quaternion.Euler(rotation), Time.deltaTime * speed);
				}
				else
				{
					transformTarget.localPosition = Vector3.Lerp(transformTarget.localPosition, GlobalAM.TransformPivot(transformMagnet, position), Time.deltaTime * speed);
					transformTarget.localRotation = Quaternion.Lerp(transformTarget.localRotation, transformMagnet.rotation * Quaternion.Euler(rotation), Time.deltaTime * speed);
				}
			}
			else if (!localMagnet)
			{
				transformTarget.localPosition = position;
				transformTarget.localRotation = Quaternion.Euler(rotation);
			}
			else
			{
				transformTarget.localPosition = GlobalAM.TransformPivot(transformMagnet, position);
				transformTarget.localRotation = transformMagnet.rotation * Quaternion.Euler(rotation);
			}
			if (sharply > 0)
			{
				sharply--;
				if (!local)
				{
					transformTarget.position = position;
					transformTarget.rotation = Quaternion.Euler(rotation);
				}
				else
				{
					transformTarget.localPosition = position;
					transformTarget.localRotation = Quaternion.Euler(rotation);
				}
			}
		}
		if (noiseActive)
		{
			noiseTimeNow -= Time.deltaTime;
			if (noiseTimeNow <= 0f)
			{
				noiseTimeNow = Random.Range(noiseTimeMin, noiseTimeMax);
				noise = Random.Range(noiseMin, noiseMax);
				transformTarget.position += new Vector3(Random.Range(0f - noise, noise), Random.Range(0f - noise, noise), Random.Range(0f - noise, noise));
			}
		}
		if (reMagnetAndParent != null)
		{
			ReMagnetAndParent(reMagnetAndParent);
			reMagnetAndParent = null;
		}
		if (timeParent)
		{
			timeParent = false;
			transformTarget.SetPositionAndRotation(positionOrigin, rotationOrigin);
		}
	}

	public void PassComponent(Transform_Magnet obj)
	{
		obj.local = local;
		obj.speed = speed;
		obj.position = position;
		obj.rotation = rotation;
	}

	public void CopyComponent(Transform_Magnet obj)
	{
		transformTarget = obj.transformTarget;
		transformMagnet = obj.transformMagnet;
		speed = obj.speed;
		local = obj.local;
		localMagnet = obj.localMagnet;
		position = obj.position;
		rotation = obj.rotation;
		noiseActive = obj.noiseActive;
		noiseTimeMin = obj.noiseTimeMin;
		noiseTimeMax = obj.noiseTimeMax;
		noiseMin = obj.noiseMin;
		noiseMax = obj.noiseMax;
	}

	public void ReTarget(Transform x)
	{
		transformTarget = x;
	}

	public void ReMagnetAndParent(Transform x)
	{
		if (!fs)
		{
			Start();
		}
		base.gameObject.SetActive(value: true);
		active = true;
		positionOrigin = transformTarget.position;
		rotationOrigin = transformTarget.rotation;
		transformMagnet = x;
		transformTarget.parent = x;
		timeParent = true;
	}

	public void ReMagnetAndParentSharply(Transform x)
	{
		ReMagnetAndParent(x);
		SharplyPosition();
	}

	public void ReMagnerAndParentPlayerRI()
	{
		ReMagnetAndParent(GlobalTag.playerRightItem.transform);
	}

	public void ReMagnerAndParentPlayerLI()
	{
		ReMagnetAndParent(GlobalTag.playerLeftItem.transform);
	}

	public void Activation(bool x)
	{
		active = x;
	}

	public void ActivationParent()
	{
		if (!fs)
		{
			Start();
		}
		base.gameObject.SetActive(value: true);
		positionOrigin = transformTarget.position;
		rotationOrigin = transformTarget.rotation;
		active = true;
		transformTarget.parent = transformMagnet;
		timeParent = true;
	}

	public void SharplyPosition()
	{
		sharply = 2;
		if (!local)
		{
			transformTarget.SetPositionAndRotation(position, Quaternion.Euler(rotation));
			return;
		}
		transformTarget.localPosition = position;
		transformTarget.localRotation = Quaternion.Euler(rotation);
	}

	[ContextMenu("Активировать и мгновенно оказаться на позиции")]
	public void ActivationSharplyParent()
	{
		if (!fs)
		{
			Start();
		}
		base.gameObject.SetActive(value: true);
		positionOrigin = transformTarget.position;
		rotationOrigin = transformTarget.rotation;
		active = true;
		transformTarget.parent = transformMagnet;
		SharplyPosition();
	}

	public void AnimatorReMagnetAndParent(Transform x)
	{
		reMagnetAndParent = x;
	}
}
