using UnityEngine;

public class GlobalGameMethods : MonoBehaviour
{
	public static string ReturnRandomName()
	{
		string text = "";
		bool flag = false;
		int num = Random.Range(0, 1);
		if (Random.Range(0, 10) > 5)
		{
			flag = true;
		}
		for (int i = 0; i < Random.Range(3, 10); i++)
		{
			if (num < 0)
			{
				num = Random.Range(0, 1);
				flag = !flag;
			}
			num--;
			text = ((!flag) ? (text + SymGn()) : (text + SymG()));
			if (i == 0)
			{
				text = text.ToUpper();
			}
		}
		return text;
	}

	private static string SymG()
	{
		string result = "";
		int num = Random.Range(0, 6);
		if (num == 0)
		{
			result = "e";
		}
		if (num == 1)
		{
			result = "y";
		}
		if (num == 2)
		{
			result = "u";
		}
		if (num == 3)
		{
			result = "i";
		}
		if (num == 4)
		{
			result = "o";
		}
		if (num >= 5)
		{
			result = "a";
		}
		return result;
	}

	private static string SymGn()
	{
		string result = "";
		int num = Random.Range(0, 17);
		if (num == 0)
		{
			result = "r";
		}
		if (num == 1)
		{
			result = "t";
		}
		if (num == 2)
		{
			result = "p";
		}
		if (num == 3)
		{
			result = "s";
		}
		if (num == 4)
		{
			result = "d";
		}
		if (num == 5)
		{
			result = "f";
		}
		if (num == 6)
		{
			result = "g";
		}
		if (num == 7)
		{
			result = "h";
		}
		if (num == 8)
		{
			result = "j";
		}
		if (num == 9)
		{
			result = "k";
		}
		if (num == 10)
		{
			result = "l";
		}
		if (num == 11)
		{
			result = "z";
		}
		if (num == 12)
		{
			result = "v";
		}
		if (num == 13)
		{
			result = "c";
		}
		if (num == 14)
		{
			result = "b";
		}
		if (num == 15)
		{
			result = "n";
		}
		if (num >= 16)
		{
			result = "m";
		}
		return result;
	}
}
