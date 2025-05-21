using UnityEngine;

public class Dialogue_Data3DText : MonoBehaviour
{
	public Dialogue_3DText[] dialogues;

	public void StopDialouesNext()
	{
		for (int i = 0; i < dialogues.Length; i++)
		{
			if (dialogues[i] != null)
			{
				dialogues[i].StopNextDialogue();
			}
		}
	}

	public void StopFastDialouesNext()
	{
		for (int i = 0; i < dialogues.Length; i++)
		{
			if (dialogues[i] != null)
			{
				dialogues[i].StopFastNextDialogue();
			}
		}
	}
}
