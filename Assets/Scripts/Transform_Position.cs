using UnityEngine;

public class Transform_Position : MonoBehaviour
{
	public Transform myParent;

	public TransformPoint[] positions;

	public float speedLerpLine = 1f;

	public AnimationCurve animationLerp;

	private int lerpLineIndex;

	private bool lerpLine;

	private float timeLerpLine;

	private Vector3 positionWas;

	private Quaternion rotationWas;

	private Transform parentOrigin;

	private bool f;

	private void Update()
	{
		if (lerpLine && timeLerpLine < 1f)
		{
			timeLerpLine += Time.deltaTime * speedLerpLine;
			if (timeLerpLine >= 1f)
			{
				timeLerpLine = 1f;
				lerpLine = false;
			}
			base.transform.localPosition = Vector3.Lerp(positionWas, positions[lerpLineIndex].position, animationLerp.Evaluate(timeLerpLine));
			base.transform.localRotation = Quaternion.Lerp(rotationWas, Quaternion.Euler(positions[lerpLineIndex].rotation), animationLerp.Evaluate(timeLerpLine));
		}
	}

	private void StartComponent()
	{
		f = true;
		if (myParent == null)
		{
			parentOrigin = base.transform.parent;
		}
		else
		{
			parentOrigin = myParent.parent;
		}
	}

	public void SetPositionAndRotation(int _index)
	{
		if (!f)
		{
			StartComponent();
		}
		base.transform.localPosition = positions[_index].position;
		base.transform.localRotation = Quaternion.Euler(positions[_index].rotation);
	}

	public void SetParentZero(Transform _transform)
	{
		SetParent(_transform);
		myParent.localPosition = Vector3.zero;
		myParent.localRotation = Quaternion.Euler(Vector3.zero);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.Euler(Vector3.zero);
		if (base.transform.parent != null && base.transform.parent.GetComponent<MitaPerson>() != null)
		{
			base.transform.parent.GetComponent<MitaPerson>().lookLife.StopRotate();
		}
	}

	public void SetParent(Transform _transform)
	{
		if (!f)
		{
			StartComponent();
		}
		if (myParent == null)
		{
			base.transform.parent = _transform;
			return;
		}
		myParent.parent = _transform;
		myParent.localPosition = Vector3.zero;
	}

	public void ResetParent()
	{
		if (!f)
		{
			StartComponent();
		}
		SetParent(parentOrigin);
	}

	public void ResetParentAndDeactive()
	{
		if (!f)
		{
			StartComponent();
		}
		SetParent(parentOrigin);
		if (myParent == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			myParent.gameObject.SetActive(value: false);
		}
	}

	public void LerpLinePositionAndRotation(int _index)
	{
		lerpLineIndex = _index;
		positionWas = base.transform.localPosition;
		rotationWas = base.transform.localRotation;
		timeLerpLine = 0f;
		lerpLine = true;
	}

	public void LerpLineSpeed(float x)
	{
		speedLerpLine = x;
	}

	public void ResetParentAll()
	{
		if (myParent != null)
		{
			base.transform.SetParent(myParent);
		}
		SetParentZero(parentOrigin);
	}
}
