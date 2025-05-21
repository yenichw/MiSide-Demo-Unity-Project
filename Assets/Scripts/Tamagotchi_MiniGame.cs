using UnityEngine;
using UnityEngine.Events;

public class Tamagotchi_MiniGame : MonoBehaviour
{
	public UnityEvent eventRestartGame;

	public UnityEvent eventStopGame;

	public Vector3 positionCamera;

	public Vector3 rotationCamera;

	public float size;

	public bool orthographic = true;

	public bool dontExit;

	private Camera cmr;

	private void Update()
	{
		if (orthographic)
		{
			cmr.orthographicSize = Mathf.Lerp(cmr.orthographicSize, size, Time.deltaTime * 10f);
		}
		else
		{
			cmr.fieldOfView = Mathf.Lerp(cmr.fieldOfView, size, Time.deltaTime * 10f);
		}
		cmr.transform.position = Vector3.Lerp(cmr.transform.position, positionCamera, Time.deltaTime * 10f);
	}

	public void StartGame(Transform _camera)
	{
		base.gameObject.SetActive(value: true);
		_camera.transform.SetPositionAndRotation(positionCamera, Quaternion.Euler(rotationCamera));
		_camera.GetComponent<Camera>().orthographic = orthographic;
		if (orthographic)
		{
			_camera.GetComponent<Camera>().orthographicSize = size;
		}
		else
		{
			_camera.GetComponent<Camera>().fieldOfView = size;
		}
		cmr = _camera.GetComponent<Camera>();
		eventRestartGame.Invoke();
	}

	public void StopGame()
	{
		eventStopGame.Invoke();
		base.gameObject.SetActive(value: false);
	}

	public void SetLerpCameraSize(float _size)
	{
		size = _size;
	}

	public void SetLerpCameraPosition(Vector3 _position)
	{
		positionCamera = _position;
	}
}
