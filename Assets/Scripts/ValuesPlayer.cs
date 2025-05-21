using UnityEngine;

public class ValuesPlayer : MonoBehaviour
{
	public ValuePlayerInt[] valueInt;

	public int GetValueInt(string _name)
	{
		int result = 0;
		for (int i = 0; i < valueInt.Length; i++)
		{
			if (_name == valueInt[i].name)
			{
				result = valueInt[i].score;
				break;
			}
		}
		return result;
	}

	public void SetValueInt(string _name, int _int)
	{
		for (int i = 0; i < valueInt.Length; i++)
		{
			if (_name == valueInt[i].name)
			{
				valueInt[i].score = _int;
				break;
			}
		}
	}

	public void AddValueInt(string _name, int _int)
	{
		for (int i = 0; i < valueInt.Length; i++)
		{
			if (_name == valueInt[i].name)
			{
				valueInt[i].score += _int;
				break;
			}
		}
	}
}
