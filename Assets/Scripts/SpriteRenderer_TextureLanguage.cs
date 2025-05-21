using UnityEngine;

public class SpriteRenderer_TextureLanguage : MonoBehaviour
{
	public string nameTexture;

	private void Start()
	{
		World component = GlobalTag.world.GetComponent<World>();
		if (component.texturesLoad)
		{
			Texture2D texture2DLanguage = component.GetTexture2DLanguage(nameTexture);
			GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture2DLanguage, new Rect(0f, 0f, texture2DLanguage.width, texture2DLanguage.height), new Vector2(0.5f, 0.5f), 100f);
		}
		Object.Destroy(this);
	}
}
