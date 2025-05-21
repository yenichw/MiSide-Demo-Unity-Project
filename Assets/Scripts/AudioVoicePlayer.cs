using UnityEngine;

public class AudioVoicePlayer : MonoBehaviour
{
	private void Start()
	{
		ResetVoice();
	}

	public void ResetVoice()
	{
		GetComponent<DataValues_Sounds>().sounds[0] = (Resources.Load("DataVoicePlayer") as GameObject).GetComponent<Audio_Data>().sounds[GlobalGame.voicePlayer];
	}
}
