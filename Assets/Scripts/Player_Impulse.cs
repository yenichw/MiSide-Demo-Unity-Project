using UnityEngine;

public class Player_Impulse : MonoBehaviour
{
	public Vector3 impulse;

	public ForceMode impulseMode = ForceMode.Impulse;

	public bool local;

	public void PlayerImpulse()
	{
		if (!local)
		{
			GlobalTag.player.GetComponent<Rigidbody>().AddForce(impulse, impulseMode);
		}
		else
		{
			GlobalTag.player.GetComponent<Rigidbody>().AddForce(GlobalAM.TransformDirection(GlobalTag.player.transform, impulse), impulseMode);
		}
	}
}
