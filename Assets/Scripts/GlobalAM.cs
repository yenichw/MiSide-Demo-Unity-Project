using System;
using System.IO;
using UnityEngine;

public class GlobalAM : MonoBehaviour
{
	public static void SaveFile(string path, string text)
	{
		using StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.WriteLine(text);
	}

	public static void SaveData(string path, string text)
	{
		if (!Directory.Exists(Application.persistentDataPath + "/Save"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/Save");
		}
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/Save/" + path);
		streamWriter.WriteLine(text);
	}

	public static string[] LoadData(string path)
	{
		string[] result = new string[0];
		if (Directory.Exists(Application.persistentDataPath + "/Save") && File.Exists(Application.persistentDataPath + "/Save/" + path))
		{
			result = File.ReadAllLines(Application.persistentDataPath + "/Save/" + path);
		}
		return result;
	}

	public static int LoadDataInt(string path, int indexString)
	{
		return StringToInt(LoadData(path)[indexString]);
	}

	public static bool ExistsData(string path)
	{
		if (!Directory.Exists(Application.persistentDataPath + "/Save"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/Save");
		}
		return File.Exists(Application.persistentDataPath + "/Save/" + path);
	}

	public static void DeleteData(string path)
	{
		if (ExistsData(path))
		{
			File.Delete(Application.persistentDataPath + "/Save/" + path);
		}
	}

	public static float DistanceFloor(Vector3 position1, Vector3 position2)
	{
		return Vector2.Distance(new Vector2(position1.x, position1.z), new Vector2(position2.x, position2.z));
	}

	public static Vector3 DirectionFloor(Vector3 positionStart, Vector3 positionEnd)
	{
		return Vector3.Normalize(new Vector3(positionEnd.x, 0f, positionEnd.z) - new Vector3(positionStart.x, 0f, positionStart.z));
	}

	public static Vector3 Direction(Vector3 positionStart, Vector3 positionEnd)
	{
		return Vector3.Normalize(positionEnd - positionStart);
	}

	public static Vector3 VectorFloor(Vector3 vector3)
	{
		return new Vector3(vector3.x, 0f, vector3.z);
	}

	public static Vector3 NormalizeFloor(Vector3 vector3)
	{
		return Vector3.Normalize(new Vector3(vector3.x, 0f, vector3.z));
	}

	public static Vector2 Vector2Normalize(Vector2 vector2)
	{
		Vector3 vector3 = Vector3.Normalize(new Vector3(vector2.x, 0f, vector2.y));
		return new Vector2(vector3.x, vector3.z);
	}

	public static Vector3 TransformPivot(Transform _transform, Vector3 _vector3)
	{
		return _transform.position + _transform.right * _vector3.x + _transform.up * _vector3.y + _transform.forward * _vector3.z;
	}

	public static Vector3 TransformPivotLocal(Transform _transform, Vector3 _vector3)
	{
		return _transform.right * _vector3.x + _transform.up * _vector3.y + _transform.forward * _vector3.z;
	}

	public static Vector3 TransformDirection(Transform _transform, Vector3 _vector3)
	{
		return _transform.right * _vector3.x + _transform.up * _vector3.y + _transform.forward * _vector3.z;
	}

	public static Vector2 Vector2Clamp(Vector2 _vector2, float _min, float _max)
	{
		if (_vector2.x > _max)
		{
			_vector2 = new Vector2(_max, _vector2.y);
		}
		if (_vector2.x < _min)
		{
			_vector2 = new Vector2(_min, _vector2.y);
		}
		if (_vector2.y > _max)
		{
			_vector2 = new Vector2(_vector2.x, _max);
		}
		if (_vector2.y < _min)
		{
			_vector2 = new Vector2(_vector2.x, _min);
		}
		return _vector2;
	}

	public static Vector3 Vector3Clamp2D(Vector2 _vector2, float _min, float _max)
	{
		if (_vector2.x > _max)
		{
			_vector2 = new Vector3(_max, _vector2.y, 0f);
		}
		if (_vector2.x < _min)
		{
			_vector2 = new Vector3(_min, _vector2.y, 0f);
		}
		if (_vector2.y > _max)
		{
			_vector2 = new Vector3(_vector2.x, _max, 0f);
		}
		if (_vector2.y < _min)
		{
			_vector2 = new Vector3(_vector2.x, _min, 0f);
		}
		return _vector2;
	}

	public static Vector3 Vector3Random(float _min, float _max)
	{
		return new Vector3(UnityEngine.Random.Range(_min, _max), UnityEngine.Random.Range(_min, _max), UnityEngine.Random.Range(_min, _max));
	}

	public static GameObject[] OverlapLine(Vector3 pointStart, Vector3 pointEnd, LayerMask layers)
	{
		RaycastHit[] array = Physics.RaycastAll(pointStart, pointEnd - pointStart, Vector3.Distance(pointStart, pointEnd), layers);
		GameObject[] array2 = new GameObject[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i].collider.gameObject;
		}
		return array2;
	}

	public static void DestroyColliders(GameObject _object)
	{
		if (_object.GetComponent<BoxCollider>() != null)
		{
			UnityEngine.Object.Destroy(_object.GetComponent<BoxCollider>());
		}
		if (_object.GetComponent<MeshCollider>() != null)
		{
			UnityEngine.Object.Destroy(_object.GetComponent<MeshCollider>());
		}
		if (_object.GetComponent<SphereCollider>() != null)
		{
			UnityEngine.Object.Destroy(_object.GetComponent<SphereCollider>());
		}
		if (_object.GetComponent<CapsuleCollider>() != null)
		{
			UnityEngine.Object.Destroy(_object.GetComponent<CapsuleCollider>());
		}
	}

	public static float RotationFloor(Vector3 _pointStart, Vector3 _pointEnd)
	{
		return Quaternion.LookRotation(_pointEnd - _pointStart, Vector3.up).eulerAngles.y;
	}

	public static int StringToInt(string x)
	{
		return Convert.ToInt32(x);
	}

	public static float StringToFloat(string x)
	{
		return Convert.ToSingle(x);
	}

	public static float DoubleToFloat(double x)
	{
		return (float)x;
	}

	public static float FloatRound(float x, int count)
	{
		return (float)Math.Round(x, count);
	}

	public static int ChanceInt()
	{
		int num = 100 - (UnityEngine.Random.Range(0, 21) + UnityEngine.Random.Range(0, 21) + UnityEngine.Random.Range(0, 21) + UnityEngine.Random.Range(0, 21) + UnityEngine.Random.Range(0, 21));
		if (num >= 50)
		{
			num -= 50;
			num *= 2;
		}
		if (num <= 50)
		{
			num = (50 - num) * 2;
		}
		return num;
	}

	public static float ChanceFloat()
	{
		float num = 100f - (UnityEngine.Random.Range(0f, 20f) + UnityEngine.Random.Range(0f, 20f) + UnityEngine.Random.Range(0f, 20f) + UnityEngine.Random.Range(0f, 20f) + UnityEngine.Random.Range(0f, 20f));
		if (num >= 50f)
		{
			num -= 50f;
			num *= 2f;
		}
		if (num <= 50f)
		{
			num = (50f - num) * 2f;
		}
		return (float)Math.Round(num, 2);
	}

	public static void DrawBox(Transform transformTarget, Vector3 offset, Vector3 size, Color colorDraw)
	{
		Vector3 vector = offset;
		Vector3 vector2 = size;
		Vector3 position = new Vector3(vector.x - vector2.x, vector.y + vector2.y, vector.z - vector2.z);
		Vector3 position2 = new Vector3(vector.x + vector2.x, vector.y + vector2.y, vector.z - vector2.z);
		Vector3 position3 = new Vector3(vector.x - vector2.x, vector.y - vector2.y, vector.z - vector2.z);
		Vector3 position4 = new Vector3(vector.x + vector2.x, vector.y - vector2.y, vector.z - vector2.z);
		Vector3 position5 = new Vector3(vector.x - vector2.x, vector.y + vector2.y, vector.z + vector2.z);
		Vector3 position6 = new Vector3(vector.x + vector2.x, vector.y + vector2.y, vector.z + vector2.z);
		Vector3 position7 = new Vector3(vector.x - vector2.x, vector.y - vector2.y, vector.z + vector2.z);
		Vector3 position8 = new Vector3(vector.x + vector2.x, vector.y - vector2.y, vector.z + vector2.z);
		position = transformTarget.TransformPoint(position);
		position2 = transformTarget.TransformPoint(position2);
		position3 = transformTarget.TransformPoint(position3);
		position4 = transformTarget.TransformPoint(position4);
		position5 = transformTarget.TransformPoint(position5);
		position6 = transformTarget.TransformPoint(position6);
		position7 = transformTarget.TransformPoint(position7);
		position8 = transformTarget.TransformPoint(position8);
		Debug.DrawLine(position, position2, colorDraw);
		Debug.DrawLine(position2, position4, colorDraw);
		Debug.DrawLine(position4, position3, colorDraw);
		Debug.DrawLine(position3, position, colorDraw);
		Debug.DrawLine(position5, position6, colorDraw);
		Debug.DrawLine(position6, position8, colorDraw);
		Debug.DrawLine(position8, position7, colorDraw);
		Debug.DrawLine(position7, position5, colorDraw);
		Debug.DrawLine(position, position5, colorDraw);
		Debug.DrawLine(position2, position6, colorDraw);
		Debug.DrawLine(position4, position8, colorDraw);
		Debug.DrawLine(position3, position7, colorDraw);
	}

	public static void DrawCircle(Vector3 position, Vector3 direction, float radius, Color colorDraw)
	{
	}
}
