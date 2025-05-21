using System.IO;
using UnityEngine;

public class Material_TextureLanguage : MonoBehaviour
{
	public string nameTexture;

	public string nameFolder;

	public string namePropertie;

	private void Start()
	{
		if (nameFolder != null && nameFolder != "")
		{
			if (Directory.Exists("Data/Languages/" + GlobalGame.Language + "/Textures") && Directory.Exists("Data/Languages/" + GlobalGame.Language + "/Textures/" + nameFolder) && File.Exists("Data/Languages/" + GlobalGame.Language + "/Textures/" + nameFolder + "/" + nameTexture + ".png"))
			{
				Texture2D texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(File.ReadAllBytes("Data/Languages/" + GlobalGame.Language + "/Textures/" + nameFolder + "/" + nameTexture + ".png"));
				texture2D.mipMapBias = 0f;
				texture2D.requestedMipmapLevel = 0;
				texture2D.filterMode = FilterMode.Bilinear;
				if (GetComponent<MeshRenderer>() != null)
				{
					if (namePropertie != null && namePropertie != "")
					{
						GetComponent<MeshRenderer>().material.SetTexture(namePropertie, texture2D);
					}
					else
					{
						GetComponent<MeshRenderer>().material.mainTexture = texture2D;
					}
				}
				if (GetComponent<SkinnedMeshRenderer>() != null)
				{
					if (namePropertie != null && namePropertie != "")
					{
						GetComponent<SkinnedMeshRenderer>().material.SetTexture(namePropertie, texture2D);
					}
					else
					{
						GetComponent<SkinnedMeshRenderer>().material.mainTexture = texture2D;
					}
				}
			}
			else
			{
				ConsoleMain.ConsolePrintWarning("Текстура не обнаружена.");
			}
		}
		else
		{
			World component = GameObject.FindWithTag("World").GetComponent<World>();
			if (component.texturesLoad)
			{
				if (GetComponent<MeshRenderer>() != null)
				{
					if (namePropertie != null && namePropertie != "")
					{
						GetComponent<MeshRenderer>().material.SetTexture(namePropertie, component.GetTexture2DLanguage(nameTexture));
					}
					else
					{
						GetComponent<MeshRenderer>().material.mainTexture = component.GetTexture2DLanguage(nameTexture);
					}
				}
				if (GetComponent<SkinnedMeshRenderer>() != null)
				{
					if (namePropertie != null && namePropertie != "")
					{
						GetComponent<SkinnedMeshRenderer>().material.SetTexture(namePropertie, component.GetTexture2DLanguage(nameTexture));
					}
					else
					{
						GetComponent<SkinnedMeshRenderer>().material.mainTexture = component.GetTexture2DLanguage(nameTexture);
					}
				}
			}
		}
		Object.Destroy(this);
	}
}
