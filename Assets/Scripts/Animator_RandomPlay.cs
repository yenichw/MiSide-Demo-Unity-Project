using UnityEngine;

public class Animator_RandomPlay : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Animator>().Play(0, 0, Random.Range(0f, 1f));
	}
}
