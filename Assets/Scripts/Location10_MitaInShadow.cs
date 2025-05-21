using UnityEngine;

public class Location10_MitaInShadow : MonoBehaviour
{
	public SkinnedMeshRenderer rend;

	public AudioSource audioHide;

	public AudioClip[] soundsHide;

	private Animator anim;

	public bool visibleWas;

	private Transform playerT;

	private bool hideAnimation;

	private void Start()
	{
		anim = GetComponent<Animator>();
		visibleWas = false;
		anim.enabled = false;
		playerT = GlobalTag.player.transform;
	}

	private void Update()
	{
		if (rend.isVisible)
		{
			if (!visibleWas)
			{
				visibleWas = true;
				ResetAnim();
			}
		}
		else if (visibleWas)
		{
			visibleWas = false;
			anim.enabled = false;
		}
		if (!hideAnimation && Vector3.Distance(base.transform.position, playerT.position) < 1.75f && Vector3.Dot(base.transform.forward, Vector3.Normalize(playerT.position - base.transform.position)) > 0.2f)
		{
			anim.SetTrigger("Hide");
			hideAnimation = true;
			if (audioHide != null)
			{
				audioHide.clip = soundsHide[Random.Range(0, soundsHide.Length)];
				audioHide.pitch = Random.Range(0.95f, 1.05f);
				audioHide.Play();
			}
		}
	}

	private void ResetAnim()
	{
		anim.ResetTrigger("Hide");
		hideAnimation = false;
		anim.enabled = true;
		anim.Play("Idle", 0, Random.Range(0f, 1f));
		if (anim.layerCount > 0 && Random.Range(0, 10) > 4)
		{
			anim.Play("Blink", 1, Random.Range(0f, 1f));
		}
	}
}
