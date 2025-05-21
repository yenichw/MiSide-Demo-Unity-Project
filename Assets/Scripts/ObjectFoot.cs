using UnityEngine;

public class ObjectFoot : MonoBehaviour
{
	public float inensityVolume = 1f;

	public LayerMask layerDefault;

	private TypeMaterial typeMaterialNow;

	public bool footOnlyStone;

	public FootMaterial footGround;

	public FootMaterial footSand;

	public FootMaterial footGrass;

	public FootMaterial footWood;

	public FootMaterial footIron;

	public FootMaterial footMarble;

	public FootMaterial footSnow;

	public FootMaterial footCarpet;

	public FootMaterial footWater;

	public FootMaterial footDeepWater;

	private int timeFootMark;

	[Header("Footmark")]
	public GameObject bloodMark;

	public Sprite spriteMark;

	public float sizeMark;

	public float forwardMark = 0.1f;

	private bool foot;

	private bool footRun;

	private GameObject objectLastCreate;

	private Vector3 objectLastCreatePosition;

	private int lastIntStep;

	private void LateUpdate()
	{
		if (foot)
		{
			FootStepLateUpdate();
			foot = false;
			footRun = false;
		}
		if (objectLastCreate != null)
		{
			objectLastCreate.transform.position = objectLastCreatePosition;
		}
	}

	private void FootStepLateUpdate()
	{
		AudioClip clip = null;
		GameObject gameObject = null;
		float num = 0.15f;
		if (Physics.Raycast(base.transform.parent.position + Vector3.up * 0.5f, -Vector3.up, out var hitInfo, 1f, layerDefault))
		{
			objectLastCreatePosition = hitInfo.point + Vector3.up * 0.02f;
			if (hitInfo.collider.GetComponent<ObjectMaterial>() != null)
			{
				typeMaterialNow = hitInfo.collider.GetComponent<ObjectMaterial>().typeMaterial;
				hitInfo.collider.GetComponent<ObjectMaterial>().FootStep();
				if (hitInfo.collider.GetComponent<ObjectMaterial>().blood && bloodMark != null)
				{
					timeFootMark = 10;
				}
			}
			else
			{
				typeMaterialNow = TypeMaterial.grass;
				Debug.Log("Не назначен материал! (" + hitInfo.collider.name + ")");
			}
		}
		else
		{
			objectLastCreatePosition = base.transform.position;
			typeMaterialNow = TypeMaterial.grass;
		}
		if (GlobalGame.qualityWorld >= 1 && timeFootMark > 0 && Physics.Raycast(base.transform.parent.position + Vector3.up * 0.5f + base.transform.forward * forwardMark, -Vector3.up, out hitInfo, 1f, layerDefault))
		{
			timeFootMark--;
			GameObject obj = Object.Instantiate(bloodMark);
			obj.transform.position = hitInfo.point + Vector3.up * 0.01f;
			obj.transform.rotation = base.transform.rotation;
			obj.transform.rotation = Quaternion.Euler(90f, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
			obj.GetComponent<SpriteRenderer>().sprite = spriteMark;
			obj.GetComponent<SpriteRenderer_SmoothDestroy>().alphaStartIntensity = (float)timeFootMark / 9f;
			obj.transform.localScale = Vector3.one * sizeMark;
		}
		if (typeMaterialNow == TypeMaterial.grass)
		{
			clip = footGrass.soundsFoot[RandomCorrect(0, footGrass.soundsFoot.Length)];
			num = footGrass.volume;
			gameObject = (footRun ? footGrass.particleFootRun : footGrass.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.water)
		{
			clip = footWater.soundsFoot[RandomCorrect(0, footWater.soundsFoot.Length)];
			num = footWater.volume;
			gameObject = (footRun ? footWater.particleFootRun : footWater.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.ground)
		{
			clip = footGround.soundsFoot[RandomCorrect(0, footGround.soundsFoot.Length)];
			num = footGround.volume;
			gameObject = (footRun ? footGround.particleFootRun : footGround.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.sand)
		{
			clip = footSand.soundsFoot[RandomCorrect(0, footSand.soundsFoot.Length)];
			num = footSand.volume;
			gameObject = (footRun ? footSand.particleFootRun : footSand.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.wood)
		{
			clip = footWood.soundsFoot[RandomCorrect(0, footWood.soundsFoot.Length)];
			num = footWood.volume;
			gameObject = (footRun ? footWood.particleFootRun : footWood.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.iron)
		{
			clip = footIron.soundsFoot[RandomCorrect(0, footIron.soundsFoot.Length)];
			num = footIron.volume;
			gameObject = (footRun ? footIron.particleFootRun : footIron.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.deepWater)
		{
			clip = footDeepWater.soundsFoot[RandomCorrect(0, footDeepWater.soundsFoot.Length)];
			num = footDeepWater.volume;
			gameObject = (footRun ? footDeepWater.particleFootRun : footDeepWater.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.carpet)
		{
			clip = footCarpet.soundsFoot[RandomCorrect(0, footCarpet.soundsFoot.Length)];
			num = footCarpet.volume;
			gameObject = (footRun ? footCarpet.particleFootRun : footCarpet.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.snow)
		{
			clip = footSnow.soundsFoot[RandomCorrect(0, footSnow.soundsFoot.Length)];
			num = footSnow.volume;
			gameObject = (footRun ? footSnow.particleFootRun : footSnow.particleFoot);
		}
		if (typeMaterialNow == TypeMaterial.marble)
		{
			clip = footMarble.soundsFoot[RandomCorrect(0, footMarble.soundsFoot.Length)];
			num = footMarble.volume;
			gameObject = (footRun ? footMarble.particleFootRun : footMarble.particleFoot);
		}
		GetComponent<AudioSource>().volume = num * inensityVolume;
		GetComponent<AudioSource>().clip = clip;
		GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
		GetComponent<AudioSource>().Play();
		if (gameObject != null && GlobalGame.qualityWorld >= 1)
		{
			objectLastCreate = Object.Instantiate(gameObject);
			objectLastCreate.transform.position = base.transform.position;
			objectLastCreate.transform.rotation = base.transform.rotation;
		}
	}

	public void FootStep()
	{
		if (!footOnlyStone)
		{
			foot = true;
			return;
		}
		GetComponent<AudioSource>().volume = footGrass.volume;
		GetComponent<AudioSource>().clip = footGrass.soundsFoot[Random.Range(0, footGrass.soundsFoot.Length)];
		GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
		GetComponent<AudioSource>().Play();
		if (GlobalGame.qualityWorld >= 1 && Physics.Raycast(base.transform.parent.position + Vector3.up * 0.5f, -Vector3.up, out var hitInfo, 1f, layerDefault) && hitInfo.collider.GetComponent<ObjectMaterial>() != null)
		{
			if (timeFootMark > 0)
			{
				timeFootMark--;
				GameObject obj = Object.Instantiate(bloodMark);
				obj.transform.position = hitInfo.point + Vector3.up * 0.01f;
				obj.transform.rotation = base.transform.rotation;
				obj.transform.rotation = Quaternion.Euler(90f, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
				obj.GetComponent<SpriteRenderer>().sprite = spriteMark;
				obj.GetComponent<SpriteRenderer_SmoothDestroy>().alphaStartIntensity = (float)timeFootMark / 9f;
				obj.transform.localScale = Vector3.one * sizeMark;
			}
			if (hitInfo.collider.GetComponent<ObjectMaterial>().blood && bloodMark != null)
			{
				timeFootMark = 10;
			}
		}
	}

	public void FootStepRun()
	{
		foot = true;
		footRun = true;
	}

	public int RandomCorrect(int min, int max)
	{
		max--;
		int num = Random.Range(min, max + 1);
		if (num == lastIntStep)
		{
			num++;
			if (num >= max)
			{
				num = min;
			}
			if (num == min)
			{
				num = max;
			}
		}
		lastIntStep = num;
		return num;
	}
}
