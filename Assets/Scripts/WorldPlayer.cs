using UnityEngine;

public class WorldPlayer : MonoBehaviour
{
	public bool ringOnPlayer;

	public bool scarOnPlayer;

	[HideInInspector]
	public float speedLerpFlashLight = 5f;

	[Header("Фонарик")]
	public float flashLightIntensity = 0.7f;

	public float flashLightSpotAngle = 40f;

	public float flashLightRange = 30f;

	public float flashLightRangeSphere = 2f;

	public float flashLightIntensitySphere = 0.25f;

	private Camera cameraPlayer;

	private float timeAnimationFOV;

	private float needFOV;

	private float wasFOV;

	private Color cameraColorWas;

	private Color cameraColorNeed;

	private float cameraColorTime;

	[Header("Камера")]
	public AnimationCurve cameraAnimationTarget;

	public AnimationCurve cameraAnimationFOV;

	public float speedAnimationFOV;

	public Color[] colorCamera;

	public float speedColorCamera = 1f;

	private float timeColorLine;

	private Color colorLineNeed;

	private Color colorLine;

	private Color colorLineWas;

	private Material materialLine;

	[Header("Линии мира")]
	public float speed = 1f;

	public int colorStart = -1;

	private GameController scrgc;

	private PlayerMove scrpm;

	private void Start()
	{
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		cameraPlayer = GlobalTag.cameraPlayer.GetComponent<Camera>();
		if (ringOnPlayer)
		{
			GlobalTag.player.GetComponent<Player_Ring>().ring.SetActive(value: true);
		}
		timeColorLine = 1f;
		materialLine = (Resources.Load("DataGame") as GameObject).GetComponent<DataMaterialShaders>().materialLine;
		if (colorStart > -1)
		{
			materialLine.SetColor("_OutlineColor", (Resources.Load("DataGame") as GameObject).GetComponent<DataMaterialShaders>().colorsLine[colorStart]);
		}
		timeAnimationFOV = 1f;
		cameraColorTime = 1f;
	}

	private void Update()
	{
		if (timeColorLine < 1f)
		{
			timeColorLine += Time.deltaTime * speed;
			if (timeColorLine > 1f)
			{
				timeColorLine = 1f;
			}
			colorLine = Color.Lerp(colorLineWas, colorLineNeed, timeColorLine);
			materialLine.SetColor("_OutlineColor", colorLine);
		}
		if (timeAnimationFOV < 1f)
		{
			timeAnimationFOV += Time.deltaTime * speedAnimationFOV;
			if (timeAnimationFOV > 1f)
			{
				timeAnimationFOV = 1f;
			}
			cameraPlayer.fieldOfView = Mathf.Lerp(wasFOV, needFOV, cameraAnimationFOV.Evaluate(timeAnimationFOV));
			cameraPlayer.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
		}
		if (cameraColorTime < 1f)
		{
			cameraColorTime += Time.deltaTime * speedColorCamera;
			if (cameraColorTime > 1f)
			{
				cameraColorTime = 1f;
			}
			cameraPlayer.backgroundColor = Color.Lerp(cameraColorWas, cameraColorNeed, cameraColorTime);
		}
	}

	public void LineColor(int _indexColor)
	{
		colorLineWas = materialLine.GetColor("_OutlineColor");
		colorLineNeed = (Resources.Load("DataGame") as GameObject).GetComponent<DataMaterialShaders>().colorsLine[_indexColor];
		timeColorLine = 0f;
	}

	public void LineColorFast(int _indexColor)
	{
		if (materialLine == null)
		{
			materialLine = (Resources.Load("DataGame") as GameObject).GetComponent<DataMaterialShaders>().materialLine;
		}
		materialLine.SetColor("_OutlineColor", (Resources.Load("DataGame") as GameObject).GetComponent<DataMaterialShaders>().colorsLine[_indexColor]);
		timeColorLine = 1f;
	}

	public void AddKeyItem(GameObject _item)
	{
		scrgc.AddKeyItem(_item);
	}

	public void RemoveKeyItem(GameObject _item)
	{
		scrgc.RemoveKeyItem(_item);
	}

	public void TakeHandItem(GameObject _item)
	{
		scrpm.TakeItem(_item);
	}

	public void RemoveHandItem()
	{
		scrpm.RemoveItem();
	}

	public void UpdateCamera()
	{
		if (cameraPlayer == null)
		{
			cameraPlayer = GlobalTag.cameraPlayer.GetComponent<Camera>();
		}
		cameraPlayer.GetComponent<PlayerCameraEffects>().UpdateCameraPerson();
	}

	public void CameraGlitch(bool _x)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().fxGlitch.enabled = _x;
	}

	public void CameraGausianBlur(bool _x)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().fxGaussian.enabled = _x;
	}

	public void CameraVignetteActive(bool _x)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().FastVegnetteActive(_x);
	}

	public void CameraNoise(float _intensity)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectNoise(_intensity);
	}

	public void CameraFish(bool _x)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectFishEye(_x);
	}

	public void CameraDatamosh(bool x)
	{
		GlobalTag.cameraPlayer.GetComponent<PlayerCameraEffects>().EffectDatamosh(x);
	}

	public void CameraAnimationFOV(float x)
	{
		timeAnimationFOV = 0f;
		wasFOV = cameraPlayer.fieldOfView;
		needFOV = x;
	}

	public void ScreenBlackTime(float _time)
	{
		GlobalTag.gameOptions.transform.Find("Interface/BlackScreen").GetComponent<BlackScreen>().BlackScreenBlackTime(_time);
	}

	public void PlayerBlinkPlay(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().animPerson.GetComponent<PlayerPerson>().BlinkPlay(x);
	}

	public void PlayerFullHide(bool x)
	{
		GlobalTag.player.SetActive(x);
	}

	public void HideBodyPlayer(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().HideBody(x);
	}

	public void HideCrosshair(bool x)
	{
		GlobalTag.gameController.GetComponent<GameController>().HideCrosshair(x, _cursor: false);
	}

	public void PlayerAnimationStop()
	{
		GlobalTag.player.GetComponent<PlayerMove>().AnimationStop();
	}

	public void PlayerAnimationFastStop()
	{
		GlobalTag.player.GetComponent<PlayerMove>().AnimationFastStop();
	}

	public void PlayerCameraNoise(float _x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().CameraNoise(_x);
	}

	public void PlayerDontMove(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().dontMove = x;
	}

	public void PlayerDontMouseMove(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().stopMouseMove = x;
	}

	public void RingActive(bool x)
	{
		GlobalTag.player.GetComponent<Player_Ring>().ring.SetActive(x);
	}

	public void ScarActive(bool x)
	{
	}

	public void PlayerNeedSit(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().SitNeed(x);
	}

	public void PlayerNeedRun(bool x)
	{
		GlobalTag.player.GetComponent<PlayerMove>().RunNeed(x);
	}

	public void ShowMouse(bool x)
	{
		GlobalTag.gameController.GetComponent<GameController>().ShowCursor(x);
	}

	public void CameraLerpOtherTarget(Transform target)
	{
		scrpm.CameraTargetOther(target, cameraAnimationTarget);
	}

	public void CameraLerpOtherTargetStop()
	{
		scrpm.CameraTargetOther(null, cameraAnimationTarget);
	}

	public void CameraLerpColor(int x)
	{
		cameraColorTime = 0f;
		cameraColorWas = cameraPlayer.backgroundColor;
		cameraColorNeed = colorCamera[x];
	}

	public void FlashLightRechange(FlashLight_World _component)
	{
		if (_component.speedLerp != 0f)
		{
			speedLerpFlashLight = _component.speedLerp;
		}
		flashLightIntensity = _component.flashLightIntensity;
		flashLightSpotAngle = _component.flashLightSpotAngle;
		flashLightRange = _component.flashLightRange;
		flashLightRangeSphere = _component.flashLightRangeSphere;
		flashLightIntensitySphere = _component.flashLightIntensitySphere;
	}
}
