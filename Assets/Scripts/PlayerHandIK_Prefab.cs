using UnityEngine;

public class PlayerHandIK_Prefab : MonoBehaviour
{
	[Header("ИНФОРМАЦИЯ")]
	public Transform[] transforms;

	public bool rightHand;

	public GameObject mesh;

	public void Start()
	{
		if (mesh != null)
		{
			Object.Destroy(mesh);
		}
	}
}
