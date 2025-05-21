using UnityEngine;

public class SceneLoading_Preloading : MonoBehaviour
{
	private UI_Colors scruicolors;

	private float timeDelete;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		scruicolors = GetComponent<UI_Colors>();
	}

	private void Update()
	{
		if (timeDelete > 0f)
		{
			timeDelete -= Time.unscaledDeltaTime;
			if (timeDelete < 0f)
			{
				timeDelete = 0f;
				scruicolors.DestroyHide();
			}
		}
	}

	public void LoadingReady(float _timeLoading)
	{
		timeDelete = _timeLoading;
	}
}
