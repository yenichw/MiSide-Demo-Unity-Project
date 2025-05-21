using UnityEngine;

public class Location14_PlayerQuestAnimation : MonoBehaviour
{
	public Location14_PlayerQuest player;

	public string nameTriggerAnimation;

	public void Play()
	{
		player.AnimationPlay(base.transform, nameTriggerAnimation);
	}
}
