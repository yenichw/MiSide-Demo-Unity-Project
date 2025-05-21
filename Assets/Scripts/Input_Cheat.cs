using UnityEngine;

public class Input_Cheat : MonoBehaviour
{
	public CheatInput[] cheats;

	private void Update()
	{
		if (!Input.anyKeyDown)
		{
			return;
		}
		for (int i = 0; i < cheats.Length; i++)
		{
			if (Input.inputString.ToLower() == cheats[i].inputText)
			{
				if (cheats[i].oneTime && !cheats[i].oneTimeReady)
				{
					cheats[i].eventCheat.Invoke();
					cheats[i].oneTimeReady = true;
				}
				else
				{
					cheats[i].eventCheat.Invoke();
				}
			}
		}
	}
}
