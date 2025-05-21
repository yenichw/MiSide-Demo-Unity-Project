using UnityEngine;

public class Location3DialogueMita : MonoBehaviour
{
	public Transform dialogueChanger;

	public UI_LookOnCamera iconDialogue;

	public GameObject[] objectsDialogues;

	private bool first;

	public void Activation(bool x)
	{
		if (!first)
		{
			first = true;
			dialogueChanger.localPosition = new Vector3(0f, 1.5f, 0.175f);
			dialogueChanger.localRotation = Quaternion.Euler(5f, 180f, 0f);
			for (int i = 0; i < objectsDialogues.Length; i++)
			{
				if (objectsDialogues[i] != null)
				{
					objectsDialogues[i].transform.localPosition = new Vector3(0f, 0.25f, -0.25f);
				}
			}
		}
		iconDialogue.Hide(!x);
	}
}
