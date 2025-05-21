using UnityEngine;
using UnityEngine.Events;

public class Mesh_VisibleEvents : MonoBehaviour
{
	public Renderer rend;

	public UnityEvent eventVisibleEnter;

	public UnityEvent eventVisibleExit;

	public bool oneTime;

	public bool destroyEnter;

	public float timeVisible;

	private bool enterUse;

	private bool exitUse;

	private void Start()
	{
		if (rend == null && GetComponent<MeshRenderer>() != null)
		{
			rend = GetComponent<MeshRenderer>();
		}
	}

	private void Update()
	{
		if (rend.isVisible)
		{
			if (timeVisible == 0f)
			{
				if (oneTime)
				{
					if (!enterUse)
					{
						enterUse = true;
						eventVisibleEnter.Invoke();
					}
				}
				else
				{
					eventVisibleEnter.Invoke();
				}
				if (destroyEnter)
				{
					Object.Destroy(base.gameObject);
				}
			}
			else
			{
				timeVisible -= Time.deltaTime;
				if (timeVisible < 0f)
				{
					timeVisible = 0f;
				}
			}
		}
		else if (oneTime)
		{
			if (!exitUse)
			{
				exitUse = true;
				eventVisibleExit.Invoke();
			}
		}
		else
		{
			eventVisibleExit.Invoke();
		}
	}
}
