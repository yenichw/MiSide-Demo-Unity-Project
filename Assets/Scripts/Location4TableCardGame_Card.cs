using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Location4TableCardGame_Card : MonoBehaviour
{
	public Transform mainCanvas;

	public Text textHeart;

	public Text textDamage;

	public Text textShield;

	private GameObject doubleCanvas;

	private float timeDestroy;

	private MeshRenderer rend;

	private Color clr;

	[Header("Информация")]
	public List<Image> ui_images;

	public List<Color> ui_imagesColor;

	public List<Text> ui_text;

	public List<Color> ui_textColor;

	public void StartComponent(int _heart, int _damage, int _shield)
	{
		textHeart.text = _heart.ToString() ?? "";
		textDamage.text = _damage.ToString() ?? "";
		textShield.text = _shield.ToString() ?? "";
		doubleCanvas = Object.Instantiate(mainCanvas.gameObject, mainCanvas.parent);
		doubleCanvas.GetComponent<RectTransform>().localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
		rend = GetComponent<MeshRenderer>();
		clr = rend.material.GetColor("_Color");
		Component[] componentsInChildren = base.transform.GetComponentsInChildren<Text>();
		Component[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			ui_text.Add(array[i].GetComponent<Text>());
			if (array[i].GetComponent<UI_ColorsSave>() == null)
			{
				ui_textColor.Add(array[i].GetComponent<Text>().color);
			}
			else
			{
				ui_textColor.Add(array[i].GetComponent<UI_ColorsSave>().GetColor());
			}
		}
		componentsInChildren = base.transform.GetComponentsInChildren<Image>();
		array = componentsInChildren;
		_ = new Image[array.Length];
		_ = new Color[array.Length];
		for (int j = 0; j < array.Length; j++)
		{
			ui_images.Add(array[j].GetComponent<Image>());
			if (array[j].GetComponent<UI_ColorsSave>() == null)
			{
				ui_imagesColor.Add(array[j].GetComponent<Image>().color);
			}
			else
			{
				ui_imagesColor.Add(array[j].GetComponent<UI_ColorsSave>().GetColor());
			}
		}
	}

	private void Update()
	{
		if (!(timeDestroy > 0f))
		{
			return;
		}
		timeDestroy -= Time.deltaTime;
		if (timeDestroy <= 0f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		clr = new Color(clr.r, clr.g, clr.b, timeDestroy);
		rend.material.SetColor("_Color", clr);
		for (int i = 0; i < ui_text.Count; i++)
		{
			ui_text[i].color = new Color(ui_textColor[i].r, ui_textColor[i].g, ui_textColor[i].b, timeDestroy);
		}
		for (int j = 0; j < ui_images.Count; j++)
		{
			ui_images[j].color = new Color(ui_imagesColor[j].r, ui_imagesColor[j].g, ui_imagesColor[j].b, timeDestroy);
		}
	}

	public void DestroyAlpha()
	{
		timeDestroy = 1f;
		rend.material.SetInt("_SrcBlend", 5);
		rend.material.SetInt("_DstBlend", 10);
		rend.material.SetInt("_ZWrite", 0);
		rend.material.DisableKeyword("_ALPHATEST_ON");
		rend.material.EnableKeyword("_ALPHABLEND_ON");
		rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		rend.material.renderQueue = 3000;
	}
}
