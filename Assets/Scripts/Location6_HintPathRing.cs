using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Location6_HintPathRing : MonoBehaviour
{
	public GameObject objectExampleHint;

	public Transform[] targetsMove;

	public ParticleSystem particleRing;

	private Transform ringPlayer;

	private bool play;

	private int indexTarget;

	[HideInInspector]
	public List<GameObject> objectsHint;

	private float timeInstance;

	private void Start()
	{
		ringPlayer = GlobalTag.player.GetComponent<Player_Ring>().ring.transform;
		particleRing.transform.parent = GlobalTag.player.GetComponent<Player_Ring>().ring.transform;
		particleRing.transform.localPosition = Vector3.zero;
		particleRing.transform.localRotation = Quaternion.identity;
	}

	private void Update()
	{
		if (play)
		{
			timeInstance += Time.deltaTime;
			if (timeInstance > 1.5f)
			{
				timeInstance = 0f;
				particleRing.Play();
				GameObject gameObject = Object.Instantiate(objectExampleHint, objectExampleHint.transform.parent);
				gameObject.transform.position = new Vector3(ringPlayer.position.x, -3f, ringPlayer.position.z);
				gameObject.SetActive(value: true);
				gameObject.GetComponent<NavMeshAgent>().SetDestination(targetsMove[indexTarget].position);
				gameObject.GetComponent<NavMeshAgent>().baseOffset = 3f + ringPlayer.transform.position.y;
				objectsHint.Add(gameObject);
			}
		}
		if (objectsHint == null || objectsHint.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < objectsHint.Count; i++)
		{
			if (objectsHint[i].transform.localScale.x < 1f)
			{
				objectsHint[i].transform.localScale += Vector3.one * (Time.deltaTime * 2f);
				if (objectsHint[i].transform.localScale.x > 1f)
				{
					objectsHint[i].transform.localScale = Vector3.one;
				}
			}
			if (objectsHint[i].GetComponent<NavMeshAgent>().baseOffset > 0.25f)
			{
				objectsHint[i].GetComponent<NavMeshAgent>().baseOffset -= Time.deltaTime * 0.25f;
				if ((double)objectsHint[i].GetComponent<NavMeshAgent>().baseOffset < 0.25)
				{
					objectsHint[i].GetComponent<NavMeshAgent>().baseOffset = 0.25f;
				}
			}
			if (objectsHint[i].GetComponent<NavMeshAgent>().speed < 3f)
			{
				objectsHint[i].GetComponent<NavMeshAgent>().speed += Time.deltaTime;
				if (objectsHint[i].GetComponent<NavMeshAgent>().speed > 3f)
				{
					objectsHint[i].GetComponent<NavMeshAgent>().speed = 3f;
				}
			}
			if (objectsHint[i].GetComponent<NavMeshAgent>().enabled && (double)Vector3.Distance(objectsHint[i].transform.position, objectsHint[i].GetComponent<NavMeshAgent>().destination) < 0.5)
			{
				for (int j = 0; j < objectsHint[i].GetComponent<DataValues_Objects>().dataObjects.Length; j++)
				{
					objectsHint[i].GetComponent<DataValues_Objects>().dataObjects[j].GetComponent<ParticleSystem>().Stop();
				}
				objectsHint[i].GetComponent<NavMeshAgent>().enabled = false;
			}
			if (!objectsHint[i].GetComponent<NavMeshAgent>().enabled)
			{
				bool flag = true;
				for (int k = 0; k < objectsHint[i].GetComponent<DataValues_Objects>().dataObjects.Length; k++)
				{
					if (objectsHint[i].GetComponent<DataValues_Objects>().dataObjects[k].GetComponent<ParticleSystem>().isPlaying)
					{
						flag = false;
						break;
					}
				}
				if (objectsHint[i].GetComponent<AudioSource>().volume > 0f)
				{
					objectsHint[i].GetComponent<AudioSource>().volume -= Time.deltaTime;
					if (objectsHint[i].GetComponent<AudioSource>().volume < 0f)
					{
						objectsHint[i].GetComponent<AudioSource>().volume = 0f;
					}
				}
				if (flag && objectsHint[i].GetComponent<AudioSource>().volume == 0f)
				{
					GameObject obj = objectsHint[i];
					objectsHint.Remove(objectsHint[i]);
					Object.Destroy(obj);
				}
			}
			else if (objectsHint[i].GetComponent<AudioSource>().volume < 0.3f)
			{
				objectsHint[i].GetComponent<AudioSource>().volume += Time.deltaTime * 0.25f;
				if (objectsHint[i].GetComponent<AudioSource>().volume > 0.3f)
				{
					objectsHint[i].GetComponent<AudioSource>().volume = 0.3f;
				}
			}
		}
	}

	public void Play(bool x)
	{
		play = x;
	}

	public void NextTrigger()
	{
		indexTarget++;
		Play(x: true);
	}

	public void Restart()
	{
		for (int i = 0; i < objectsHint.Count; i++)
		{
			Object.Destroy(objectsHint[i]);
		}
		objectsHint.Clear();
		indexTarget = 0;
		play = false;
	}
}
