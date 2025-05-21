using UnityEngine;

public class ConsoleCommandsGame : MonoBehaviour
{
	public static string Help()
	{
		return "<color=#02E8E0>Game [MiSide] ----------------------------------------</color>\ncheat = активировать читы\n";
	}

	public static bool Command(string code)
	{
		bool result = false;
		if (code == "cheat")
		{
			result = true;
			GlobalGame.cheat = !GlobalGame.cheat;
			ConsoleMain.ConsolePrintCheat("Cheat = " + GlobalGame.cheat);
			if (GlobalGame.cheat)
			{
				ConsoleMain.ConsolePrintCheat("Чтобы пропускать ремонт, нажмите [N]");
			}
		}
		return result;
	}
}
