using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

public class Location11_Lift : MonoBehaviour
{
	public UnityEvent eventFinishA;

	public UnityEvent eventStartB;

	public UnityEvent eventFinishB;

	public Transform positionAnimationPlayer;

	private bool turretUse;

	[HideInInspector]
	public List<Location11_TurretTrailShot> trailShot;

	[Header("Турель")]
	public Transform targetTurret;

	public Transform turretVertical;

	public Transform turretHorizontal;

	public Transform pointShot;

	public LayerMask layerEnemy;

	public GameObject exampleShot;

	public GameObject particleHit;

	private bool readyTurret;

	private bool readyAttack;

	private float timeCreateEnemy;

	private int checkEnemys;

	[HideInInspector]
	public List<GameObject> enemys;

	[Header("Враги")]
	public GameObject enemyExample;

	private FullBodyBipedIK scrfbbik;

	private Transform elbowLeftPlayer;

	private Transform elbowRightPlayer;

	[Header("IK игрока")]
	public Transform pivotHandRight;

	public Transform pivotHandLeft;

	private PlayerMove scrpm;

	private Transform cameraPlayer;

	private Transform playerT;

	private int typePath;

	private float speed;

	private float intensityControlTurret;

	private void Start()
	{
		scrpm = GlobalTag.player.GetComponent<PlayerMove>();
		playerT = GlobalTag.player.transform;
		cameraPlayer = GlobalTag.cameraPlayer.transform;
	}

	private void Update()
	{
		if (typePath == 1)
		{
			if (base.transform.localPosition.y > 83f)
			{
				base.transform.localPosition += new Vector3(0f, speed * Time.deltaTime, 0f);
			}
			else
			{
				if ((double)speed < -0.1)
				{
					speed += Time.deltaTime * 2.25f;
					if ((double)speed > -0.1)
					{
						speed = -0.1f;
					}
					base.transform.localPosition += new Vector3(0f, speed * Time.deltaTime, 0f);
				}
				if (base.transform.localPosition.y < 80.86f)
				{
					base.transform.localPosition = new Vector3(base.transform.localPosition.x, 80.86f, base.transform.localPosition.z);
					eventFinishA.Invoke();
					typePath = 2;
				}
			}
		}
		if (typePath == 3)
		{
			checkEnemys++;
			if (checkEnemys == 10)
			{
				checkEnemys = 0;
				if (enemys != null && enemys.Count > 0)
				{
					for (int i = 0; i < enemys.Count; i++)
					{
						if (enemys[i] == null)
						{
							enemys.RemoveAt(i);
							i = 0;
						}
					}
				}
			}
			if ((double)base.transform.localPosition.y < 331.8)
			{
				if (speed < 3f)
				{
					speed += Time.deltaTime;
					if (speed > 3f)
					{
						speed = 3f;
					}
				}
				base.transform.localPosition += new Vector3(0f, speed * Time.deltaTime, 0f);
			}
			else
			{
				if ((double)speed > 0.1)
				{
					speed -= Time.deltaTime * 2.25f;
					if ((double)speed < 0.1)
					{
						speed = 0.1f;
					}
				}
				base.transform.localPosition += new Vector3(0f, speed * Time.deltaTime, 0f);
				if (base.transform.localPosition.y > 333.8f)
				{
					base.transform.localPosition = new Vector3(base.transform.localPosition.x, 333.8f, base.transform.localPosition.z);
					eventFinishB.Invoke();
					typePath = 4;
				}
			}
			if (base.transform.localPosition.y < 130f)
			{
				timeCreateEnemy += Time.deltaTime;
				if (timeCreateEnemy > 5f)
				{
					CreateEnemy();
				}
			}
			else if (!readyAttack && (enemys == null || enemys.Count == 0))
			{
				readyAttack = true;
				turretUse = false;
			}
		}
		if (turretUse)
		{
			scrfbbik.solver.rightArmMapping.weight = Mathf.Lerp(scrfbbik.solver.rightArmMapping.weight, 1f, Time.deltaTime * 5f);
			scrfbbik.solver.leftArmMapping.weight = Mathf.Lerp(scrfbbik.solver.leftArmMapping.weight, 1f, Time.deltaTime * 5f);
			elbowLeftPlayer.transform.localPosition = Vector3.Lerp(elbowLeftPlayer.transform.localPosition, new Vector3(-1.7f, 0.5f, -0.3f), Time.deltaTime * 5f);
			elbowRightPlayer.transform.localPosition = Vector3.Lerp(elbowRightPlayer.transform.localPosition, new Vector3(-1.1f, 0.5f, -0.3f), Time.deltaTime * 5f);
			if (scrfbbik.solver.rightArmMapping.weight > 0.95f)
			{
				targetTurret.position = Vector3.Lerp(targetTurret.position, cameraPlayer.position + cameraPlayer.forward * 10f, Time.deltaTime * 10f);
				if (targetTurret.localScale.x < 0.25f)
				{
					targetTurret.localScale += Vector3.one * Time.deltaTime;
					if (targetTurret.localScale.x > 0.25f)
					{
						targetTurret.localScale = Vector3.one * 0.25f;
					}
				}
				if (intensityControlTurret < 1f)
				{
					intensityControlTurret += Time.deltaTime;
					if (intensityControlTurret > 1f)
					{
						intensityControlTurret = 1f;
					}
				}
				Quaternion quaternion = Quaternion.LookRotation(targetTurret.position - turretHorizontal.transform.position, Vector3.up);
				turretHorizontal.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 0f, quaternion.eulerAngles.y - 90f), intensityControlTurret);
				quaternion = Quaternion.LookRotation(targetTurret.position - turretVertical.transform.position, Vector3.right);
				turretVertical.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f - quaternion.eulerAngles.x, 0f, 0f), intensityControlTurret);
				turretVertical.localPosition = Vector3.Lerp(turretVertical.localPosition, Vector3.zero, Time.deltaTime * 8f);
				if (Time.timeScale > 0f && Input.GetMouseButtonDown(0))
				{
					Shot();
				}
			}
		}
		else if (readyAttack && !readyTurret && typePath > 2)
		{
			if (intensityControlTurret > 0f)
			{
				intensityControlTurret -= Time.deltaTime;
				if (intensityControlTurret < 0f)
				{
					intensityControlTurret = 0f;
				}
			}
			Quaternion quaternion2 = Quaternion.LookRotation(targetTurret.position - turretHorizontal.transform.position, Vector3.up);
			turretHorizontal.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 0f, quaternion2.eulerAngles.y - 90f), intensityControlTurret);
			quaternion2 = Quaternion.LookRotation(targetTurret.position - turretVertical.transform.position, Vector3.right);
			turretVertical.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f - quaternion2.eulerAngles.x, 0f, 0f), intensityControlTurret);
			targetTurret.localScale = Vector3.one * 0.25f * intensityControlTurret;
			if (intensityControlTurret == 0f)
			{
				scrfbbik.solver.rightArmMapping.weight = Mathf.Lerp(scrfbbik.solver.rightArmMapping.weight, 0f, Time.deltaTime * 5f);
				scrfbbik.solver.leftArmMapping.weight = Mathf.Lerp(scrfbbik.solver.leftArmMapping.weight, 0f, Time.deltaTime * 5f);
				elbowLeftPlayer.transform.localPosition = Vector3.Lerp(elbowLeftPlayer.transform.localPosition, new Vector3(-1.7f, 0.5f, -0.3f), Time.deltaTime * 5f);
				elbowRightPlayer.transform.localPosition = Vector3.Lerp(elbowRightPlayer.transform.localPosition, new Vector3(-1.1f, 0.5f, -0.3f), Time.deltaTime * 5f);
				if (scrfbbik.solver.rightArmMapping.weight < 0.05f)
				{
					GlobalTag.player.GetComponent<PlayerMove>().scrppik.ActiveOtherControl(x: false);
					positionAnimationPlayer.GetComponent<ObjectAnimationPlayer>().AnimationStop();
					readyTurret = true;
				}
			}
		}
		if (trailShot == null || trailShot.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < trailShot.Count; j++)
		{
			if (trailShot[j].objectShot != null)
			{
				trailShot[j].objectShot.GetComponent<LineRenderer>().SetPosition(0, Vector3.MoveTowards(trailShot[j].objectShot.GetComponent<LineRenderer>().GetPosition(0), trailShot[j].objectShot.GetComponent<LineRenderer>().GetPosition(1), Time.deltaTime * 100f));
				if (Vector3.Distance(trailShot[j].objectShot.GetComponent<LineRenderer>().GetPosition(0), trailShot[j].objectShot.GetComponent<LineRenderer>().GetPosition(1)) < 0.5f)
				{
					Object.Destroy(trailShot[j].objectShot);
				}
			}
		}
	}

	private void LateUpdate()
	{
		if (typePath == 3 && base.transform.localPosition.y < 333.8f)
		{
			if (!scrpm.animationRun)
			{
				playerT.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
			}
			else
			{
				playerT.position = positionAnimationPlayer.position;
			}
		}
	}

	public void StartLift()
	{
		if (typePath == 2)
		{
			typePath = 3;
		}
		if (typePath == 0)
		{
			timeCreateEnemy = -10f;
			speed = -3f;
			typePath = 1;
		}
	}

	private void CreateEnemy()
	{
		timeCreateEnemy = 0f;
		GameObject gameObject = Object.Instantiate(enemyExample, enemyExample.transform.parent);
		gameObject.transform.position = base.transform.position + new Vector3(25f, Random.Range(2, 5), Random.Range(-5, 5));
		gameObject.SetActive(value: true);
		enemys.Add(gameObject);
	}

	public void TurretUse(bool x)
	{
		turretUse = x;
		if (x)
		{
			targetTurret.gameObject.SetActive(value: true);
			targetTurret.localScale = Vector3.zero;
			scrfbbik = GlobalTag.player.transform.Find("Person").GetComponent<FullBodyBipedIK>();
			GlobalTag.player.GetComponent<PlayerMove>().scrppik.ActiveOtherControl(x: true);
			scrfbbik.solver.rightHandEffector.target = pivotHandRight;
			scrfbbik.solver.leftHandEffector.target = pivotHandLeft;
			elbowLeftPlayer = GlobalTag.player.transform.Find("Elbow Left");
			elbowRightPlayer = GlobalTag.player.transform.Find("Elbow Right");
		}
	}

	private void Shot()
	{
		pointShot.GetComponent<ParticleSystem>().Play();
		turretVertical.localPosition = new Vector3(0f, Random.Range(-0.01f, -0.02f), Random.Range(-0.015f, 0.02f));
		Vector3 vector = pointShot.transform.position + GlobalAM.Direction(pointShot.transform.position, targetTurret.position) * 25f;
		if (Physics.Raycast(pointShot.transform.position, GlobalAM.Direction(pointShot.transform.position, targetTurret.position) * 25f, out var hitInfo, 50f, layerEnemy))
		{
			vector = pointShot.transform.position + GlobalAM.Direction(pointShot.transform.position, targetTurret.position) * hitInfo.distance;
			if (hitInfo.collider.name == "B")
			{
				hitInfo.collider.transform.parent.GetComponent<Location11_LiftEnemy>().DamageBody();
			}
			if (hitInfo.collider.name == "H")
			{
				hitInfo.collider.transform.parent.GetComponent<Location11_LiftEnemy>().DamageHead();
			}
			GameObject obj = Object.Instantiate(particleHit, particleHit.transform.parent);
			obj.transform.position = pointShot.transform.position + GlobalAM.Direction(pointShot.transform.position, targetTurret.position) * hitInfo.distance;
			obj.SetActive(value: true);
		}
		Location11_TurretTrailShot location11_TurretTrailShot = new Location11_TurretTrailShot
		{
			objectShot = Object.Instantiate(exampleShot, exampleShot.transform.parent),
			shotEnd = vector
		};
		location11_TurretTrailShot.objectShot.GetComponent<LineRenderer>().SetPosition(0, pointShot.transform.position);
		location11_TurretTrailShot.objectShot.GetComponent<LineRenderer>().SetPosition(1, vector);
		location11_TurretTrailShot.objectShot.SetActive(value: true);
		trailShot.Add(location11_TurretTrailShot);
		for (int i = 0; i < trailShot.Count; i++)
		{
			if (trailShot[i].objectShot == null)
			{
				trailShot.RemoveAt(i);
				i = 0;
			}
		}
	}

	public void Restart()
	{
		if (enemys == null || enemys.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < enemys.Count; i++)
		{
			if (enemys[i] != null)
			{
				Object.Destroy(enemys[i]);
			}
		}
		enemys.Clear();
	}
}
