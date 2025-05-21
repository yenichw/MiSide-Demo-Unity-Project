using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_LookOnCamera : MonoBehaviour
{
	private float colorAlpha;

	[Header("Изображения")]
	public Image[] images;

	public Text[] textes;

	public float speed = 1f;

	private Transform cameraT;

	[Header("3D")]
	public float size = 1f;

	public float distanceSize;

	private bool destroyActive;

	private bool hideMehotd;

	[Header("Видимость")]
	public float distanceHide;

	public bool hide;

	public bool hideDistance;

	public UnityEvent eventDestroy;

	private void OnEnable()
	{
		cameraT = GlobalTag.cameraPlayer.transform;
		colorAlpha = 0f;
		if (images != null && images.Length != 0)
		{
			for (int i = 0; i < images.Length; i++)
			{
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, 0f);
			}
		}
		if (textes != null && textes.Length != 0)
		{
			for (int j = 0; j < textes.Length; j++)
			{
				textes[j].color = new Color(textes[j].color.r, textes[j].color.g, textes[j].color.b, 0f);
			}
		}
	}

	private void Update()
	{
		if (distanceSize == 0f)
		{
			base.transform.localScale = Vector3.one * (size / 800f * Vector3.Distance(base.transform.position, cameraT.position));
		}
		else if (Vector3.Distance(base.transform.position, cameraT.position) > distanceSize)
		{
			base.transform.localScale = Vector3.one * (size / 800f * Vector3.Distance(base.transform.position, cameraT.position));
		}
		else
		{
			base.transform.localScale = Vector3.one * (size / 800f * distanceSize);
		}
		base.transform.rotation = Quaternion.LookRotation(base.transform.position - cameraT.position);
		if (distanceHide > 0f && !destroyActive)
		{
			if (Vector3.Distance(base.transform.position, cameraT.position) > distanceHide)
			{
				hideDistance = true;
			}
			else
			{
				hideDistance = false;
			}
		}
		if (!destroyActive && !hideDistance && !hide && !hideMehotd)
		{
			if (!(colorAlpha < 1f))
			{
				return;
			}
			colorAlpha += Time.deltaTime * speed;
			if (colorAlpha >= 1f)
			{
				colorAlpha = 1f;
			}
			if (images != null && images.Length != 0)
			{
				for (int i = 0; i < images.Length; i++)
				{
					images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, colorAlpha);
				}
			}
			if (textes != null && textes.Length != 0)
			{
				for (int j = 0; j < textes.Length; j++)
				{
					textes[j].color = new Color(textes[j].color.r, textes[j].color.g, textes[j].color.b, colorAlpha);
				}
			}
		}
		else
		{
			if (!(colorAlpha > 0f))
			{
				return;
			}
			colorAlpha -= Time.deltaTime * speed;
			if (colorAlpha <= 0f)
			{
				colorAlpha = 0f;
				if (destroyActive)
				{
					eventDestroy.Invoke();
					Object.Destroy(base.gameObject);
				}
			}
			if (images != null && images.Length != 0)
			{
				for (int k = 0; k < images.Length; k++)
				{
					images[k].color = new Color(images[k].color.r, images[k].color.g, images[k].color.b, colorAlpha);
				}
			}
			if (textes != null && textes.Length != 0)
			{
				for (int l = 0; l < textes.Length; l++)
				{
					textes[l].color = new Color(textes[l].color.r, textes[l].color.g, textes[l].color.b, colorAlpha);
				}
			}
		}
	}

	public void SmoothDestroy()
	{
		destroyActive = true;
	}

	public void Hide(bool _hide)
	{
		hideMehotd = _hide;
		hide = _hide;
	}
}
