using UnityEngine;

public class DataValues_AnimationClip : MonoBehaviour
{
	public AnimationClip[] animations;

	public AnimationClip GetAnimation(string _nameAnimation)
	{
		AnimationClip result = null;
		for (int i = 0; i < animations.Length; i++)
		{
			if (animations[i].name == _nameAnimation)
			{
				result = animations[i];
				break;
			}
		}
		return result;
	}

	public static AnimationClip AnimationStringFind(string nameAnimationClip, int numberData)
	{
		DataValues_AnimationClip component = (Resources.Load("Data/AnimationsPlayer " + numberData) as GameObject).GetComponent<DataValues_AnimationClip>();
		AnimationClip result = null;
		for (int i = 0; i < component.animations.Length; i++)
		{
			if (component.animations[i].name == nameAnimationClip)
			{
				result = component.animations[i];
				break;
			}
		}
		return result;
	}
}
