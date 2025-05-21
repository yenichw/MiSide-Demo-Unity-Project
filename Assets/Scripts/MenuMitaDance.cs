using UnityEngine;

public class MenuMitaDance : MonoBehaviour
{
	public AudioSource audioMusicMain;

	public Transform mitaT;

	public float[] spectrum = new float[128];

	[Header("Dance")]
	public float intensityDance = 1f;

	public int indexDance;

	public float speedLerp;

	private bool playDance;

	[HideInInspector]
	public float lerpJump;

	private void Update()
	{
		if (playDance)
		{
			audioMusicMain.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
			lerpJump = Mathf.Lerp(lerpJump, spectrum[indexDance] * intensityDance, Time.deltaTime * speedLerp);
			mitaT.transform.position = Vector3.Lerp(mitaT.transform.position, new Vector3(0f, lerpJump, 0f), Time.deltaTime * speedLerp);
		}
	}

	public void PlayDance()
	{
		playDance = true;
	}
}
