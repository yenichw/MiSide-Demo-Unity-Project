using UnityEngine;

public class AnimatorState_Bool : StateMachineBehaviour
{
	public string[] onExitBoolOff;

	public string[] onExitBoolOn;

	public string[] onEnterBoolOff;

	public string[] onEnterBoolOn;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < onEnterBoolOff.Length; i++)
		{
			animator.SetBool(onEnterBoolOff[i], value: false);
		}
		for (int j = 0; j < onEnterBoolOn.Length; j++)
		{
			animator.SetBool(onEnterBoolOn[j], value: true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < onExitBoolOff.Length; i++)
		{
			animator.SetBool(onExitBoolOff[i], value: false);
		}
		for (int j = 0; j < onExitBoolOn.Length; j++)
		{
			animator.SetBool(onExitBoolOn[j], value: true);
		}
	}
}
