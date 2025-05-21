using UnityEngine;
using UnityEngine.UI;

public class UI_DistanceAlpha : MonoBehaviour
{
	public float distance = 1f;

	public float speed = 3f;

	public Image[] images;

	public Text[] textes;

	public bool floor = true;

	public Transform transformDistance;

	private Transform targetDistance;

	private float colorAlpha;

	private void Start()
	{
		if (transformDistance == null)
		{
			transformDistance = base.transform;
		}
		targetDistance = GameObject.FindWithTag("Player").transform;
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
		if (floor)
		{
			if (GlobalAM.DistanceFloor(transformDistance.position, targetDistance.position) < distance)
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
		else if (Vector3.Distance(transformDistance.position, targetDistance.position) < distance)
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
				for (int m = 0; m < images.Length; m++)
				{
					images[m].color = new Color(images[m].color.r, images[m].color.g, images[m].color.b, colorAlpha);
				}
			}
			if (textes != null && textes.Length != 0)
			{
				for (int n = 0; n < textes.Length; n++)
				{
					textes[n].color = new Color(textes[n].color.r, textes[n].color.g, textes[n].color.b, colorAlpha);
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
			}
			if (images != null && images.Length != 0)
			{
				for (int num = 0; num < images.Length; num++)
				{
					images[num].color = new Color(images[num].color.r, images[num].color.g, images[num].color.b, colorAlpha);
				}
			}
			if (textes != null && textes.Length != 0)
			{
				for (int num2 = 0; num2 < textes.Length; num2++)
				{
					textes[num2].color = new Color(textes[num2].color.r, textes[num2].color.g, textes[num2].color.b, colorAlpha);
				}
			}
		}
	}
}
