using UnityEngine;

public class Animator_Unit : MonoBehaviour
{
	private Animator anim;

	private float footTime;

	public string floatNameFootSpeed = "SpeedForward";

	public GameObject footLeft;

	public GameObject footRight;

	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (footTime > 0f)
		{
			footTime -= Time.deltaTime;
			if (footTime < 0f)
			{
				footTime = 0f;
			}
		}
	}

	public void FootStep(int _right)
	{
		if (((floatNameFootSpeed != "" && (double)anim.GetFloat(floatNameFootSpeed) > 0.05) || floatNameFootSpeed == "") && footTime == 0f)
		{
			footTime = 0.2f;
			if (_right == 0)
			{
				footLeft.GetComponent<ObjectFoot>().FootStep();
			}
			if (_right == 1)
			{
				footRight.GetComponent<ObjectFoot>().FootStep();
			}
		}
	}

	public void Foot(int _right)
	{
		FootStep(_right);
	}
}
