using UnityEngine;
using UnityEngine.Events;

public class Location4Fight : MonoBehaviour
{
	public bool win;

	public bool play;

	public bool playEnemy;

	private bool sidePlayer;

	[Header("Игроки")]
	public Location4Fight_Person playerMain;

	public Location4Fight_Person playerEnemy;

	[Header("Интерфейс")]
	public RectTransform lineHealthPlayer;

	public RectTransform lineHealthEnemy;

	public GameObject[] healthPointPlayer;

	public GameObject[] healthPointEnemy;

	public GameObject triggerZoom;

	public GameObject KO;

	[Header("Настройки")]
	public float distanceWall = 3f;

	private int winG;

	private int loseG;

	[Header("Сюжет")]
	public GameObject[] objectsLose;

	public GameObject[] objectsWin;

	public UnityEvent eventReadyGame;

	[Range(0f, 6f)]
	public int enemyComplexity = 1;

	private float timeStartEnemy;

	private int moveJump;

	private float timeDontJump;

	private float timeDontMove;

	private float timeDontAttack;

	private float timeDontSit;

	private float timeDamage;

	private void Start()
	{
	}

	private void Update()
	{
		if (!play)
		{
			return;
		}
		if (Input.GetButton("Left"))
		{
			playerMain.Move(-1);
		}
		if (Input.GetButton("Right"))
		{
			playerMain.Move(1);
		}
		if (!Input.GetButton("Left") && !Input.GetButton("Right"))
		{
			playerMain.Move(0);
		}
		if (Input.GetButtonDown("Down"))
		{
			playerMain.Sit(_x: true);
		}
		if (Input.GetButtonUp("Down"))
		{
			playerMain.Sit(_x: false);
		}
		if (Input.GetButtonDown("Up"))
		{
			playerMain.Jump();
		}
		if (Input.GetButtonDown("Interactive"))
		{
			playerMain.Kick();
		}
		if (sidePlayer)
		{
			if (playerMain.transform.localPosition.x < playerEnemy.transform.localPosition.x)
			{
				sidePlayer = false;
				playerMain.ReSide(x: false);
				playerEnemy.ReSide(x: true);
			}
		}
		else if (playerMain.transform.localPosition.x > playerEnemy.transform.localPosition.x)
		{
			sidePlayer = true;
			playerMain.ReSide(x: true);
			playerEnemy.ReSide(x: false);
		}
		if (timeStartEnemy > 0f)
		{
			timeStartEnemy -= Time.deltaTime;
			if (timeStartEnemy < 0f)
			{
				playEnemy = true;
			}
		}
		if (playEnemy)
		{
			if ((double)Vector3.Distance(playerMain.transform.position, playerEnemy.transform.position) > 1.75 && timeDontMove == 0f)
			{
				if (playerEnemy.transform.localPosition.y == 0f)
				{
					if (playerEnemy.side)
					{
						playerEnemy.Move(-1);
					}
					else
					{
						playerEnemy.Move(1);
					}
				}
				if (playerEnemy.transform.localPosition.y > 0f)
				{
					playerEnemy.Move(moveJump);
				}
			}
			else
			{
				playerEnemy.Move(0);
				if (timeDontAttack == 0f && !playerMain.death)
				{
					playerEnemy.Kick();
					if (timeDontMove == 0f)
					{
						timeDontMove = Random.Range(0f, 0.5f) * (float)enemyComplexity;
					}
					if (timeDontAttack == 0f)
					{
						timeDontAttack = Random.Range(0f, 1.5f) * (float)enemyComplexity;
					}
				}
			}
			if (timeDontSit == 0f)
			{
				if (playerMain.sit)
				{
					if (!playerEnemy.sit)
					{
						playerEnemy.Sit(_x: true);
						if (timeDontSit == 0f)
						{
							timeDontSit = Random.Range(0f, 1f) * (float)enemyComplexity;
						}
					}
				}
				else if (playerEnemy.sit)
				{
					playerEnemy.Sit(_x: false);
					if (timeDontSit == 0f)
					{
						timeDontSit = Random.Range(0f, 1f) * (float)enemyComplexity;
					}
				}
				if (playerMain.timeAttack > 0f && !playerMain.sit)
				{
					playerEnemy.Sit(_x: true);
					if (timeDontSit == 0f)
					{
						timeDontSit = Random.Range(0f, 1f) * (float)enemyComplexity;
					}
				}
			}
			if (timeDontJump == 0f && playerMain.timeAttack > 0f && playerEnemy.transform.localPosition.y == 0f && !playerMain.sit)
			{
				timeDontJump = Random.Range(0.25f, 1f) * ((float)enemyComplexity + 0.2f);
				playerEnemy.Jump();
				if (playerEnemy.side)
				{
					moveJump = -1;
				}
				else
				{
					moveJump = 1;
				}
			}
		}
		if (timeDontJump > 0f)
		{
			timeDontJump -= Time.deltaTime;
			if (timeDontJump < 0f)
			{
				timeDontJump = 0f;
			}
		}
		if (timeDontMove > 0f)
		{
			timeDontMove -= Time.deltaTime;
			if (timeDontMove < 0f)
			{
				timeDontMove = 0f;
			}
		}
		if (timeDontSit > 0f)
		{
			timeDontSit -= Time.deltaTime;
			if (timeDontSit < 0f)
			{
				timeDontSit = 0f;
			}
		}
		if (timeDontAttack > 0f)
		{
			timeDontAttack -= Time.deltaTime;
			if (timeDontAttack < 0f)
			{
				timeDontAttack = 0f;
			}
		}
		if (timeDamage > 0f)
		{
			timeDamage -= Time.deltaTime * 0.4f;
			if (timeDamage < 0f)
			{
				timeDamage = 0f;
			}
		}
	}

	public void Complexity(int _x)
	{
		enemyComplexity = _x;
	}

	public void EnemyGetDamage()
	{
		timeDamage += 1f;
		if (enemyComplexity == 1)
		{
			timeDamage += 1f;
		}
		if (enemyComplexity == 0)
		{
			timeDamage += 1f;
		}
		if (timeDamage > 3f)
		{
			timeDamage = 0f;
			playerEnemy.Jump();
			if (playerEnemy.side)
			{
				moveJump = -1;
			}
			else
			{
				moveJump = 1;
			}
		}
	}

	public void UpdateInterface()
	{
		lineHealthPlayer.sizeDelta = new Vector2(playerMain.health * 0.295f, 3.5f);
		lineHealthEnemy.sizeDelta = new Vector2(playerEnemy.health * 0.295f, 3.5f);
		if (playerMain.health == 0f)
		{
			Win(_x: false);
		}
		if (playerEnemy.health == 0f)
		{
			Win(_x: true);
		}
	}

	public void Win(bool _x)
	{
		if (!win)
		{
			KO.gameObject.SetActive(value: true);
			playerEnemy.Move(0);
			playEnemy = false;
			if (!_x)
			{
				objectsLose[loseG].SetActive(value: true);
				healthPointEnemy[loseG].SetActive(value: false);
				loseG++;
			}
			if (_x)
			{
				objectsWin[winG].SetActive(value: true);
				healthPointPlayer[winG].SetActive(value: false);
				winG++;
			}
			if (loseG == 3 || winG == 3)
			{
				eventReadyGame.Invoke();
			}
			win = true;
			triggerZoom.GetComponent<BoxCollider>().enabled = false;
		}
	}

	[ContextMenu("Перезапустить игру")]
	public void ResetGame()
	{
		KO.gameObject.SetActive(value: false);
		win = false;
		play = true;
		playerMain.ResetPerson();
		playerEnemy.ResetPerson();
		playerMain.transform.localPosition = new Vector3(-3.5f, 0f, 0f);
		playerEnemy.transform.localPosition = new Vector3(3.5f, 0f, 0f);
		playerMain.ReSide(x: false);
		playerEnemy.ReSide(x: true);
		playerMain.Sit(_x: false);
		playerEnemy.Sit(_x: false);
		UpdateInterface();
		timeStartEnemy = Random.Range(0.8f, 2f);
		triggerZoom.GetComponent<BoxCollider>().enabled = true;
		if (winG == 2)
		{
			playerEnemy.damage = 12;
		}
	}

	public void GamePlay(bool x)
	{
		play = x;
		if (x)
		{
			timeStartEnemy = Random.Range(0.8f, 2f);
			triggerZoom.GetComponent<BoxCollider>().enabled = true;
		}
	}
}
