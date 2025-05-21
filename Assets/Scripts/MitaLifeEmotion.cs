using UnityEngine;

public class MitaLifeEmotion : MonoBehaviour
{
	private float timeDontActive;

	[Header("Поведение")]
	public bool active = true;

	private float timeShy;

	private int levelShy;

	private int indexDialogueShy;

	public GameObject[] dialogueShy;

	private bool randomEmotion;

	private float timeRanomEmotion;

	private bool lookFullPlayer;

	private float timeLookFullPlayer;

	private int orderLookFullPlayer;

	private bool lookHead;

	[Header("Взгляд игрока")]
	public LayerMask layerPart;

	public SphereCollider colliderHead;

	private PlayerMove scrpm;

	private GameController scrgc;

	private Transform cameraPlayer;

	private MitaPerson mita;

	private void Start()
	{
		cameraPlayer = GlobalTag.cameraPlayer.transform;
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		scrgc = GlobalTag.gameController.GetComponent<GameController>();
		mita = GetComponent<MitaPerson>();
		timeLookFullPlayer = Random.Range(25f, 60f);
	}

	private void Update()
	{
		if (active && timeDontActive == 0f)
		{
			if (lookHead && orderLookFullPlayer == 0)
			{
				timeShy += Time.deltaTime;
				if (levelShy == 0 && timeShy > 8f)
				{
					timeShy = 0f;
					levelShy = 1;
					mita.FaceEmotion("smile");
					mita.FaceLayer(1);
					mita.lookLife.Blink();
					if (indexDialogueShy < dialogueShy.Length)
					{
						dialogueShy[indexDialogueShy].SetActive(value: true);
					}
					indexDialogueShy++;
				}
				if (levelShy == 1 && (double)timeShy > 0.5)
				{
					timeShy = 0f;
					levelShy = 2;
					mita.FaceEmotion("shy");
					mita.lookLife.lookObject = null;
					mita.lookLife.LookRandom();
					mita.lookLife.EyesLookOffsetRandom(0.1f);
					mita.lookLife.Blink();
				}
				if (levelShy == 2 && timeShy > 1f)
				{
					timeShy = 0f;
					levelShy = 3;
					mita.lookLife.LookRandom();
					mita.lookLife.EyesLookOffsetRandom(0.15f);
					mita.lookLife.Blink();
				}
				if (levelShy == 3 && timeShy > 4f)
				{
					timeShy = -0.2f;
					levelShy = 2;
					mita.lookLife.EyesLookOnPlayer(Random.Range(0.1f, 0.25f));
					mita.lookLife.Blink();
				}
			}
			else if (levelShy > 0)
			{
				timeShy += Time.deltaTime;
				if (timeShy > 4f)
				{
					mita.FaceEmotion("smile");
					mita.FaceLayer(0);
					mita.lookLife.LookOnPlayer();
					levelShy = 0;
					timeShy = 0f;
				}
			}
			if (!lookHead)
			{
				timeRanomEmotion += Time.deltaTime;
				if (timeRanomEmotion > 8f)
				{
					if (!randomEmotion)
					{
						timeRanomEmotion = Random.Range(-1f, 6f);
						int num = Random.Range(0, 2);
						if (num == 0)
						{
							mita.FaceEmotion("quest");
						}
						if (num == 1)
						{
							mita.FaceEmotion("emptiness");
						}
						if (num == 2)
						{
							mita.FaceEmotion("sad");
						}
						randomEmotion = true;
					}
					else
					{
						timeRanomEmotion = Random.Range(-25f, -8f);
						mita.FaceEmotion("smile");
						randomEmotion = false;
					}
				}
			}
			if (!lookFullPlayer && timeLookFullPlayer > 0f)
			{
				timeLookFullPlayer -= Time.deltaTime;
				if (timeLookFullPlayer < 0f)
				{
					bool flag = false;
					if (orderLookFullPlayer == 0 && mita.lookLife.lookObject == GlobalTag.cameraPlayer.transform)
					{
						timeLookFullPlayer = 0.5f;
						orderLookFullPlayer = 1;
						mita.lookLife.LookOnObjectSmooth(GlobalTag.player.transform.Find("Person/Armature/Hips/Right leg/Right knee"));
						flag = true;
					}
					if (!flag && orderLookFullPlayer == 1)
					{
						timeLookFullPlayer = 1f;
						orderLookFullPlayer = 2;
						mita.lookLife.LookOnObjectSmooth(GlobalTag.player.transform.Find("Person/Armature/Hips"));
						flag = true;
					}
					if (!flag && orderLookFullPlayer == 2)
					{
						timeLookFullPlayer = 1f;
						orderLookFullPlayer = 3;
						mita.lookLife.LookOnObjectSmooth(GlobalTag.player.transform.Find("Person/Armature/Hips/Spine/Chest"));
						flag = true;
					}
					if (!flag && orderLookFullPlayer == 3)
					{
						timeLookFullPlayer = 1f;
						orderLookFullPlayer = 4;
						mita.lookLife.LookOnObjectSmooth(GlobalTag.player.transform.Find("Person/Armature/Hips/Spine/Chest/Neck2"));
						flag = true;
					}
					if (!flag && orderLookFullPlayer == 4)
					{
						orderLookFullPlayer = 0;
						lookFullPlayer = true;
						mita.lookLife.LookOnPlayer();
					}
				}
			}
		}
		if (timeDontActive > 0f)
		{
			timeDontActive -= Time.deltaTime;
			if (timeDontActive < 0f)
			{
				timeDontActive = 0f;
			}
		}
		if (scrgc.timePrintWas > 0f)
		{
			TimeDontActive(5f);
		}
		if (mita.nma.enabled)
		{
			TimeDontActive(3f);
		}
		if (scrpm.animationRun)
		{
			TimeDontActive(1f);
		}
		if (scrpm.dontMove)
		{
			TimeDontActive(1f);
		}
	}

	private void FixedUpdate()
	{
		if (Physics.Raycast(cameraPlayer.position, cameraPlayer.forward, out var hitInfo, 3f, layerPart))
		{
			if (hitInfo.collider == colliderHead)
			{
				lookHead = true;
			}
			else
			{
				lookHead = false;
			}
			return;
		}
		lookHead = false;
		if (levelShy == 0)
		{
			timeShy = 0f;
		}
	}

	public void Activation(bool x)
	{
		active = x;
		if (!x)
		{
			StopEmotion();
		}
	}

	private void StopEmotion()
	{
		timeShy = 0f;
		if (levelShy > 0)
		{
			levelShy = 0;
			mita.FaceEmotion("smile");
			mita.FaceLayer(0);
			mita.lookLife.LookOnPlayer();
		}
		if (orderLookFullPlayer > 0)
		{
			lookFullPlayer = true;
			mita.lookLife.LookOnPlayer();
			orderLookFullPlayer = 0;
		}
		timeRanomEmotion = Random.Range(-25f, -8f);
		if (randomEmotion)
		{
			mita.FaceEmotion("smile");
			randomEmotion = false;
		}
	}

	private void TimeDontActive(float _x)
	{
		if (timeDontActive == 0f)
		{
			StopEmotion();
		}
		if (_x > timeDontActive)
		{
			timeDontActive = _x;
		}
	}
}
