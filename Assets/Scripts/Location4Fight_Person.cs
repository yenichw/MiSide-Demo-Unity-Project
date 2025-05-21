using UnityEngine;

public class Location4Fight_Person : MonoBehaviour
{
	public Location4Fight main;

	public bool side;

	public ParticleSystem getDamageParticle;

	public ParticleSystem fallParticle;

	public Transform enemy;

	[HideInInspector]
	public float timeAttack;

	[HideInInspector]
	public bool sit;

	private float sitF;

	private float moveF;

	private float gravityY;

	private int move;

	private float moveDamage;

	private float timeStopMove;

	private float timeDamage;

	private float timeJumping;

	[Header("Анимация")]
	public Animator anim;

	private BoxCollider box;

	[HideInInspector]
	public bool death;

	[Header("Настройки")]
	public int damage = 8;

	public float health = 100f;

	public Vector3 positionAttack;

	public LayerMask layerAttack;

	[Header("Движение")]
	public float jump = 10f;

	public float gravitySpeed = 10f;

	public float speed = 0.5f;

	[Header("звуки")]
	public AudioSource audioDeath;

	public AudioSource audioStep;

	public AudioClip[] soundsStep;

	public AudioSource audioMove;

	public AudioClip soundJump;

	public AudioClip soundFall;

	public AudioSource audioWing;

	public AudioClip soundWing;

	public AudioClip soundHit;

	public Audio_Data soundsGamepad;

	[Header("Дополнения")]
	public Transform shadow;

	private void Start()
	{
		box = GetComponent<BoxCollider>();
	}

	private void Update()
	{
		if (sit)
		{
			sitF = Mathf.Lerp(sitF, 1f, Time.deltaTime * 10f);
		}
		else
		{
			sitF = Mathf.Lerp(sitF, 0f, Time.deltaTime * 10f);
		}
		anim.SetFloat("Sit", sitF);
		if (base.transform.localPosition.y > 0f)
		{
			gravityY -= Time.deltaTime * gravitySpeed;
		}
		if (timeStopMove == 0f)
		{
			if (base.transform.localPosition.y == 0f)
			{
				if (!side)
				{
					anim.SetFloat("Forward", moveF);
				}
				else
				{
					anim.SetFloat("Forward", 0f - moveF);
				}
			}
			else
			{
				anim.SetFloat("Forward", 0f);
			}
			moveF = Mathf.Lerp(moveF, move, Time.deltaTime * 15f);
		}
		if (base.transform.localPosition.y == 0f)
		{
			base.transform.position += new Vector3(moveDamage + moveF * speed, gravityY, 0f) * Time.deltaTime;
		}
		else
		{
			base.transform.position += new Vector3(moveDamage + moveF * (speed * 2f), gravityY, 0f) * Time.deltaTime;
		}
		if (timeStopMove > 0f)
		{
			timeStopMove -= Time.deltaTime;
			if (timeStopMove < 0f)
			{
				timeStopMove = 0f;
			}
		}
		if (moveDamage > 0f)
		{
			moveDamage -= Time.deltaTime * 5f;
			if (moveDamage < 0f)
			{
				moveDamage = 0f;
			}
		}
		if (moveDamage < 0f)
		{
			moveDamage += Time.deltaTime * 5f;
			if (moveDamage > 0f)
			{
				moveDamage = 0f;
			}
		}
		if (timeAttack > 0f)
		{
			timeAttack -= Time.deltaTime;
			if (timeAttack < 0f)
			{
				timeAttack = 0f;
			}
		}
		if (timeDamage > 0f)
		{
			timeDamage -= Time.deltaTime;
			if (timeDamage < 0f)
			{
				timeDamage = 0f;
			}
		}
		if (timeJumping > 0f)
		{
			timeJumping -= Time.deltaTime;
			if (timeJumping < 0f)
			{
				timeJumping = 0f;
			}
		}
		if (!main.win && (double)base.transform.localPosition.y < 0.6 && (double)enemy.localPosition.y < 0.6)
		{
			if (!side && base.transform.localPosition.x >= enemy.localPosition.x - 0.8f)
			{
				base.transform.localPosition = new Vector3(enemy.localPosition.x - 0.81f, base.transform.localPosition.y, 0f);
			}
			if (side && base.transform.localPosition.x <= enemy.localPosition.x + 0.8f)
			{
				base.transform.localPosition = new Vector3(enemy.localPosition.x + 0.81f, base.transform.localPosition.y, 0f);
			}
		}
	}

	private void LateUpdate()
	{
		if (base.transform.localPosition.y < 0f)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, 0f, 0f);
			gravityY = 0f;
			if (!death)
			{
				anim.SetTrigger("Fall");
			}
			timeStopMove = 0.1f;
			moveF = 0f;
			anim.SetFloat("Forward", 0f);
			anim.ResetTrigger("KickSit");
			anim.ResetTrigger("Kick");
			fallParticle.Play();
			audioMove.clip = soundFall;
			audioMove.pitch = Random.Range(0.95f, 1.05f);
			audioMove.Play();
		}
		base.transform.localPosition = new Vector3(Mathf.Clamp(base.transform.localPosition.x, 0f - main.distanceWall, main.distanceWall), base.transform.localPosition.y, 0f);
		shadow.position = new Vector3(base.transform.position.x, 50f, 0f);
	}

	public void Kick()
	{
		if (base.transform.localPosition.y == 0f)
		{
			if (timeAttack == 0f && timeStopMove == 0f)
			{
				soundsGamepad.RandomPlayPitch();
			}
			if (sit)
			{
				anim.SetTrigger("KickSit");
			}
			else
			{
				anim.SetTrigger("Kick");
			}
			timeAttack = 1f;
			moveF = 0f;
			anim.SetFloat("Forward", 0f);
			timeStopMove = 0.3f;
		}
	}

	public void Jump()
	{
		if (!death && timeStopMove == 0f && (base.transform.localPosition.y == 0f || timeDamage > 0f))
		{
			if (timeJumping == 0f)
			{
				soundsGamepad.RandomPlayPitch();
			}
			timeJumping = 0.3f;
			anim.SetTrigger("Jump");
			gravityY = jump;
			anim.SetFloat("Forward", 0f);
			anim.ResetTrigger("KickSit");
			anim.ResetTrigger("Kick");
			audioMove.clip = soundJump;
			audioMove.pitch = Random.Range(0.95f, 1.05f);
			audioMove.Play();
		}
	}

	public void Sit(bool _x)
	{
		if (sit != _x)
		{
			sit = _x;
			soundsGamepad.RandomPlayPitch();
		}
		if (!sit)
		{
			box.size = new Vector3(1f, 1.5f, 1f);
			box.center = new Vector3(0f, 0.75f, 0f);
			positionAttack = new Vector3(positionAttack.x, 1.25f, 0f);
		}
		else
		{
			box.size = new Vector3(1f, 0.75f, 1f);
			box.center = new Vector3(0f, 0.4f, 0f);
			positionAttack = new Vector3(positionAttack.x, 0.25f, 0f);
		}
	}

	public void Move(int x)
	{
		if (!death)
		{
			if (move != x)
			{
				soundsGamepad.RandomPlayPitch();
			}
			move = x;
		}
	}

	public void Damage(int _damage)
	{
		if (death || enemy.GetComponent<Location4Fight_Person>().death)
		{
			return;
		}
		health -= _damage;
		if (health < 0f)
		{
			health = 0f;
		}
		if (health > 0f)
		{
			anim.SetTrigger("Damage");
			if (base.transform == main.playerEnemy.transform)
			{
				main.EnemyGetDamage();
			}
			timeDamage = 0.25f;
		}
		else
		{
			move = 0;
			anim.SetTrigger("Death");
			death = true;
			audioDeath.Play();
		}
		getDamageParticle.Play();
		if (timeJumping == 0f)
		{
			gravityY = 5f;
		}
		if (side)
		{
			moveDamage = 2f;
		}
		else
		{
			moveDamage = -2f;
		}
		main.UpdateInterface();
	}

	private void Attack(Vector3 _position)
	{
		Component[] array = Physics.OverlapBox(base.transform.position + positionAttack, new Vector3(1.25f, 0.25f, 0.25f), Quaternion.identity, layerAttack);
		Component[] array2 = array;
		if (array2 == null || array2.Length == 0)
		{
			return;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].GetComponent<Location4Fight_Person>() != null && array2[i].GetComponent<Location4Fight_Person>() != this)
			{
				array2[i].GetComponent<Location4Fight_Person>().Damage(damage);
				audioWing.clip = soundHit;
				audioWing.pitch = Random.Range(0.95f, 1.05f);
				audioWing.Play();
			}
		}
	}

	public void ReSide(bool x)
	{
		if (!death)
		{
			side = x;
			if (!side)
			{
				anim.transform.localScale = new Vector3(1f, 1f, -1f);
				positionAttack = new Vector3(Mathf.Abs(positionAttack.x), positionAttack.y, positionAttack.z);
			}
			else
			{
				anim.transform.localScale = new Vector3(1f, 1f, 1f);
				positionAttack = new Vector3(0f - Mathf.Abs(positionAttack.x), positionAttack.y, positionAttack.z);
			}
		}
	}

	public void ResetPerson()
	{
		timeStopMove = 0f;
		moveDamage = 0f;
		health = 100f;
		moveF = 0f;
		gravityY = 0f;
		death = false;
		anim.SetFloat("Forward", 0f);
		anim.ResetTrigger("Death");
		anim.ResetTrigger("Kick");
		anim.ResetTrigger("KickSit");
		anim.ResetTrigger("Fall");
		anim.ResetTrigger("Damage");
		anim.ResetTrigger("Jump");
		anim.Play("Move", -1, 0f);
	}

	public void AttackTrigger()
	{
		Attack(positionAttack);
	}

	public void AttackTriggerSit()
	{
		Attack(positionAttack - Vector3.up * 0.5f);
	}

	public void Step()
	{
		if (move != 0)
		{
			audioStep.clip = soundsStep[Random.Range(0, soundsStep.Length)];
			audioStep.pitch = Random.Range(0.95f, 1.05f);
			audioStep.Play();
		}
	}

	public void Wing()
	{
		audioWing.clip = soundWing;
		audioWing.pitch = Random.Range(0.95f, 1.05f);
		audioWing.Play();
	}
}
