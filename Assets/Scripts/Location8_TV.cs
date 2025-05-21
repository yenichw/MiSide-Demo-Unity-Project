using UnityEngine;

public class Location8_TV : MonoBehaviour
{
	public Texture2D[] pictures;

	public Light lg;

	public AudioSource sound;

	[Header("Материалы")]
	public Material materialOff;

	public Material materialCat;

	public Material materialInterface;

	public MeshRenderer rendLight;

	private bool TVWork;

	private MeshRenderer rend;

	private int indexPicture;

	private float timeResetPicture;

	private bool showInterface;

	private void Start()
	{
		TVWork = true;
		rend = GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		if (!TVWork || showInterface)
		{
			return;
		}
		timeResetPicture += Time.deltaTime * 10f;
		if (timeResetPicture >= 1f)
		{
			timeResetPicture = 0f;
			indexPicture++;
			if (indexPicture > pictures.Length - 1)
			{
				indexPicture = 0;
			}
			rend.material.mainTexture = pictures[indexPicture];
			rend.material.SetTexture("_EmissionMap", pictures[pictures.Length - (indexPicture + 1)]);
		}
	}

	public void MakeRed(bool _x)
	{
		if (_x)
		{
			lg.color = new Color(1f, 0f, 0f);
			rend.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f));
			rend.material.SetColor("_Color", new Color(1f, 0f, 0f));
			rendLight.material.SetColor("_Color", new Color(1f, 0f, 0f));
		}
		else
		{
			lg.color = new Color(1f, 1f, 1f);
			rend.material.SetColor("_EmissionColor", new Color(1f, 1f, 1f));
			rend.material.SetColor("_Color", new Color(1f, 1f, 1f));
			rendLight.material.SetColor("_Color", new Color(1f, 1f, 1f));
		}
	}

	public void TvActive(bool _x)
	{
		TVWork = _x;
		lg.gameObject.SetActive(_x);
		sound.enabled = _x;
		if (!_x)
		{
			rend.material = materialOff;
		}
		else
		{
			rend.material = materialInterface;
		}
	}

	public void MaterialShowInterface()
	{
		showInterface = true;
		rend.material = materialInterface;
	}

	public void MaterialShowCat()
	{
		showInterface = false;
		rend.material = materialCat;
	}
}
