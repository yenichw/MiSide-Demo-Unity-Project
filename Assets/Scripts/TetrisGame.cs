using System.Collections.Generic;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

public class TetrisGame : MonoBehaviour
{
	private float positionDefeatRandom;

	[Header("Интерфейс")]
	public Text textScore;

	public RectTransform textDefeat;

	public GameObject game;

	public GameObject defeat;

	private bool play;

	private float timeNextSpeed;

	private int score;

	private float speed;

	private Vector2 move;

	private float timeUpdateArray;

	private int timeCreateMoney;

	private int timeCreateDecoration;

	private int timeCreateEnemy;

	[Header("Игровые объекты")]
	public RectTransform background;

	public GameObject player;

	public GameObject enemy;

	public GameObject money;

	public GameObject decoration;

	public Sprite[] spritesDecoration;

	[Header("Звуки")]
	public AudioSource audioMoney;

	public AudioSource audioRemove;

	private float timeAutoRestart;

	private bool autoPlay;

	[Space(20f)]
	public List<GameObject> obejctsGameMove;

	public List<GameObject> obejctsEnemy;

	public List<GameObject> obejctsMoney;

	private void Update()
	{
		if (play)
		{
			if (Input.GetAxis("Horizontal") > 0f && move != new Vector2(-1f, 0f))
			{
				autoPlay = false;
				audioRemove.pitch = Random.Range(0.95f, 1.05f);
				audioRemove.Play();
				move = new Vector2(-1f, 0f);
				player.GetComponent<UIFlip>().horizontal = false;
			}
			if (Input.GetAxis("Horizontal") < 0f && move != new Vector2(1f, 0f))
			{
				autoPlay = false;
				audioRemove.pitch = Random.Range(0.95f, 1.05f);
				audioRemove.Play();
				move = new Vector2(1f, 0f);
				player.GetComponent<UIFlip>().horizontal = true;
			}
			if (Input.GetAxis("Vertical") > 0f && move != new Vector2(0f, -1f))
			{
				autoPlay = false;
				audioRemove.pitch = Random.Range(0.95f, 1.05f);
				audioRemove.Play();
				move = new Vector2(0f, -1f);
			}
			if (Input.GetAxis("Vertical") < 0f && move != new Vector2(0f, 1f))
			{
				autoPlay = false;
				audioRemove.pitch = Random.Range(0.95f, 1.05f);
				audioRemove.Play();
				move = new Vector2(0f, 1f);
			}
			if (autoPlay && obejctsMoney != null && obejctsMoney.Count > 0)
			{
				for (int i = 0; i < obejctsMoney.Count; i++)
				{
					if (!(obejctsMoney[i] != null))
					{
						continue;
					}
					Vector2 anchoredPosition = obejctsMoney[i].GetComponent<RectTransform>().anchoredPosition;
					if (anchoredPosition.x < 10f && anchoredPosition.x > -10f)
					{
						if (anchoredPosition.y > 0f && move != new Vector2(0f, -1f))
						{
							audioRemove.pitch = Random.Range(0.95f, 1.05f);
							audioRemove.Play();
							move = new Vector2(0f, -1f);
						}
						if (anchoredPosition.y < 0f && move != new Vector2(0f, 1f))
						{
							audioRemove.pitch = Random.Range(0.95f, 1.05f);
							audioRemove.Play();
							move = new Vector2(0f, 1f);
						}
						break;
					}
					if (anchoredPosition.x > 0f && move != new Vector2(-1f, 0f))
					{
						audioRemove.pitch = Random.Range(0.95f, 1.05f);
						audioRemove.Play();
						move = new Vector2(-1f, 0f);
						player.GetComponent<UIFlip>().horizontal = false;
					}
					if (anchoredPosition.x < 0f && move != new Vector2(1f, 0f))
					{
						audioRemove.pitch = Random.Range(0.95f, 1.05f);
						audioRemove.Play();
						move = new Vector2(1f, 0f);
						player.GetComponent<UIFlip>().horizontal = true;
					}
					break;
				}
			}
			timeNextSpeed += Time.deltaTime;
			if (timeNextSpeed > 1f)
			{
				timeNextSpeed = 0f;
				speed += 1f;
				timeCreateMoney++;
				if (timeCreateMoney == 3)
				{
					timeCreateMoney = 0;
					GameObject gameObject = Object.Instantiate(money, money.transform.parent);
					gameObject.SetActive(value: true);
					obejctsGameMove.Add(gameObject);
					obejctsMoney.Add(gameObject);
					if (move == new Vector2(1f, 0f))
					{
						gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(-1f, 0f))
					{
						gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(0f, 1f))
					{
						gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), -200f);
					}
					if (move == new Vector2(0f, -1f))
					{
						gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), 200f);
					}
				}
				timeCreateDecoration++;
				if (timeCreateDecoration == 1)
				{
					timeCreateDecoration = 0;
					GameObject gameObject2 = Object.Instantiate(decoration, decoration.transform.parent);
					gameObject2.GetComponent<Image>().sprite = spritesDecoration[Random.Range(0, spritesDecoration.Length)];
					gameObject2.GetComponent<RectTransform>().sizeDelta = gameObject2.GetComponent<Image>().sprite.rect.size;
					if (Random.Range(0, 2) == 0)
					{
						gameObject2.GetComponent<UIFlip>().horizontal = true;
					}
					gameObject2.SetActive(value: true);
					obejctsGameMove.Add(gameObject2);
					if (move == new Vector2(1f, 0f))
					{
						gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(-1f, 0f))
					{
						gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(0f, 1f))
					{
						gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), -200f);
					}
					if (move == new Vector2(0f, -1f))
					{
						gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), 200f);
					}
				}
				timeCreateEnemy++;
				if (timeCreateEnemy == 5)
				{
					timeCreateEnemy = 0;
					GameObject gameObject3 = Object.Instantiate(enemy, enemy.transform.parent);
					gameObject3.SetActive(value: true);
					obejctsGameMove.Add(gameObject3);
					obejctsEnemy.Add(gameObject3);
					gameObject3.GetComponent<TetrisFrog>().StartComponent(score * 2);
					if (move == new Vector2(1f, 0f))
					{
						gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(-1f, 0f))
					{
						gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(200f, 32 * Random.Range(-3, 4));
					}
					if (move == new Vector2(0f, 1f))
					{
						gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), -200f);
					}
					if (move == new Vector2(0f, -1f))
					{
						gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(-3, 4), 200f);
					}
				}
			}
			Vector2 vector = move * (speed * Time.deltaTime);
			background.anchoredPosition += vector;
			if (background.anchoredPosition.x > 256f)
			{
				background.anchoredPosition -= new Vector2(256f, 0f);
			}
			if (background.anchoredPosition.x < -256f)
			{
				background.anchoredPosition += new Vector2(256f, 0f);
			}
			if (background.anchoredPosition.y > 256f)
			{
				background.anchoredPosition -= new Vector2(0f, 256f);
			}
			if (background.anchoredPosition.y < -256f)
			{
				background.anchoredPosition += new Vector2(0f, 256f);
			}
			if (obejctsGameMove != null && obejctsGameMove.Count > 0)
			{
				for (int j = 0; j < obejctsGameMove.Count; j++)
				{
					if (obejctsGameMove[j] != null)
					{
						obejctsGameMove[j].GetComponent<RectTransform>().anchoredPosition += vector;
						if (obejctsGameMove[j].GetComponent<RectTransform>().anchoredPosition.x < -256f)
						{
							Object.Destroy(obejctsGameMove[j]);
						}
						if (obejctsGameMove[j].GetComponent<RectTransform>().anchoredPosition.x > 512f)
						{
							Object.Destroy(obejctsGameMove[j]);
						}
						if (obejctsGameMove[j].GetComponent<RectTransform>().anchoredPosition.y < -256f)
						{
							Object.Destroy(obejctsGameMove[j]);
						}
						if (obejctsGameMove[j].GetComponent<RectTransform>().anchoredPosition.y > 512f)
						{
							Object.Destroy(obejctsGameMove[j]);
						}
					}
				}
			}
			if (obejctsMoney != null && obejctsMoney.Count > 0)
			{
				for (int k = 0; k < obejctsMoney.Count; k++)
				{
					if (obejctsMoney[k] != null)
					{
						Vector2 anchoredPosition2 = obejctsMoney[k].GetComponent<RectTransform>().anchoredPosition;
						if (anchoredPosition2.x < 10f && anchoredPosition2.x > -10f && anchoredPosition2.y < 10f && anchoredPosition2.y > -10f)
						{
							Object.Destroy(obejctsMoney[k]);
							ScoreAdd(1);
						}
					}
				}
			}
			if (obejctsEnemy != null && obejctsEnemy.Count > 0)
			{
				for (int l = 0; l < obejctsEnemy.Count; l++)
				{
					if (obejctsEnemy[l] != null)
					{
						Vector2 anchoredPosition3 = obejctsEnemy[l].GetComponent<RectTransform>().anchoredPosition;
						if (anchoredPosition3.x > 0f)
						{
							obejctsEnemy[l].GetComponent<TetrisFrog>().Flip(x: true);
						}
						else
						{
							obejctsEnemy[l].GetComponent<TetrisFrog>().Flip(x: false);
						}
						if (anchoredPosition3.x < 16f && anchoredPosition3.x > -16f && anchoredPosition3.y < 10f && anchoredPosition3.y > -10f)
						{
							Death();
						}
					}
				}
			}
			timeUpdateArray += Time.deltaTime;
			if (timeUpdateArray > 5f)
			{
				timeUpdateArray = 0f;
				UpdateArray();
			}
		}
		textScore.color = Color.Lerp(textScore.color, new Color(0f, 1f, 0f), Time.deltaTime * 6f);
		textScore.transform.localScale = Vector3.Lerp(textScore.transform.localScale, Vector3.one * 0.5f, Time.deltaTime * 6f);
		player.GetComponent<Image>().color = Color.Lerp(player.GetComponent<Image>().color, new Color(0.6f, 1f, 0.1f), Time.deltaTime * 6f);
		if (play)
		{
			return;
		}
		textDefeat.anchoredPosition = new Vector2(0f + Random.Range(0f - positionDefeatRandom, positionDefeatRandom), -60f + Random.Range(0f - positionDefeatRandom, positionDefeatRandom));
		positionDefeatRandom = Mathf.Lerp(positionDefeatRandom, 0f, Time.deltaTime * 5f);
		if (Input.GetButtonDown("Interactive") || Input.GetButtonDown("Submit"))
		{
			RestartGame();
		}
		if (autoPlay)
		{
			timeAutoRestart += Time.deltaTime;
			if ((double)timeAutoRestart > 1.5)
			{
				timeAutoRestart = 0f;
				RestartGame();
			}
		}
	}

	[ContextMenu("Рестарт")]
	private void ScoreAdd(int x)
	{
		score += x;
		textScore.text = "SCORE: " + score;
		if (x > 0)
		{
			textScore.color = new Color(1f, 1f, 1f);
			textScore.transform.localScale = Vector3.one * 0.6f;
			player.GetComponent<Image>().color = new Color(1f, 1f, 1f);
			audioMoney.pitch = Random.Range(0.95f, 1.05f);
			audioMoney.Play();
		}
	}

	private void RestartGame()
	{
		play = true;
		speed = 20f;
		timeNextSpeed = 0f;
		score = 0;
		timeCreateMoney = 0;
		score = 0;
		ScoreAdd(0);
		game.SetActive(value: true);
		defeat.SetActive(value: false);
		int num = Random.Range(0, 4);
		if (num == 0)
		{
			move = new Vector2(1f, 0f);
			player.GetComponent<UIFlip>().horizontal = true;
		}
		if (num == 1)
		{
			move = new Vector2(-1f, 0f);
			player.GetComponent<UIFlip>().horizontal = false;
		}
		if (num == 2)
		{
			move = new Vector2(0f, 1f);
		}
		if (num == 3)
		{
			move = new Vector2(0f, -1f);
		}
		background.anchoredPosition = Vector2.zero;
		for (int i = 0; i < Random.Range(6, 13); i++)
		{
			GameObject gameObject = Object.Instantiate(decoration, decoration.transform.parent);
			gameObject.GetComponent<Image>().sprite = spritesDecoration[Random.Range(0, spritesDecoration.Length)];
			gameObject.GetComponent<RectTransform>().sizeDelta = gameObject.GetComponent<Image>().sprite.rect.size;
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(32 * Random.Range(0, 9), 32 * Random.Range(0, 9));
			if (Random.Range(0, 2) == 0)
			{
				gameObject.GetComponent<UIFlip>().horizontal = true;
			}
			gameObject.SetActive(value: true);
			obejctsGameMove.Add(gameObject);
		}
	}

	private void ClearObejcts()
	{
		if (obejctsGameMove != null && obejctsGameMove.Count > 0)
		{
			for (int i = 0; i < obejctsGameMove.Count; i++)
			{
				if (obejctsGameMove[i] != null)
				{
					Object.Destroy(obejctsGameMove[i]);
				}
			}
		}
		obejctsMoney.Clear();
		obejctsEnemy.Clear();
		obejctsGameMove.Clear();
	}

	private void UpdateArray()
	{
		if (obejctsGameMove != null && obejctsGameMove.Count > 0)
		{
			for (int i = 0; i < obejctsGameMove.Count; i++)
			{
				if (obejctsGameMove[i] == null)
				{
					obejctsGameMove.RemoveAt(i);
					i = 0;
				}
			}
		}
		if (obejctsEnemy != null && obejctsEnemy.Count > 0)
		{
			for (int j = 0; j < obejctsEnemy.Count; j++)
			{
				if (obejctsEnemy[j] == null)
				{
					obejctsEnemy.RemoveAt(j);
					j = 0;
				}
			}
		}
		if (obejctsMoney == null || obejctsMoney.Count <= 0)
		{
			return;
		}
		for (int k = 0; k < obejctsMoney.Count; k++)
		{
			if (obejctsMoney[k] == null)
			{
				obejctsMoney.RemoveAt(k);
				k = 0;
			}
		}
	}

	private void Death()
	{
		ClearObejcts();
		play = false;
		game.SetActive(value: false);
		defeat.SetActive(value: true);
		positionDefeatRandom = 10f;
	}

	public void StartRestart()
	{
		autoPlay = true;
		enemy.SetActive(value: false);
		money.SetActive(value: false);
		decoration.SetActive(value: false);
		ClearObejcts();
		RestartGame();
	}
}
