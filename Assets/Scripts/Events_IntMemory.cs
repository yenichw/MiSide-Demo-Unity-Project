using System;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Functions/Event/Event memory")]
public class Events_IntMemory : MonoBehaviour
{
	[Serializable]
	public class SerializableIntMemory
	{
		public int ifInt;

		public UnityEvent _event;
	}

	public SerializableIntMemory[] _memory;

	[Header("Information")]
	public int _int;

	public void Add()
	{
		_int++;
		FoundEvent();
	}

	public void Remove()
	{
		_int--;
		FoundEvent();
	}

	private void FoundEvent()
	{
		for (int i = 0; i < _memory.Length; i++)
		{
			if (_int == _memory[i].ifInt)
			{
				_memory[i]._event.Invoke();
			}
		}
	}
}
