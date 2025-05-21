using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/GameObject/Destroy")]
public class GameObject_Destroy : MonoBehaviour
{
	public UnityEvent eventDestroy;

	public UnityEvent eventOnDestroy;

	public float timeDestroy;

	private bool des;

	private void Update()
	{
		if (timeDestroy > 0f)
		{
			timeDestroy -= Time.deltaTime;
			if (timeDestroy <= 0f)
			{
				destroy();
			}
		}
	}

	public void destroy()
	{
		if (!des)
		{
			des = true;
			eventDestroy.Invoke();
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		des = true;
		eventOnDestroy.Invoke();
	}
}
