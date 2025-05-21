using UnityEngine;

public class Player_CreateFace : MonoBehaviour
{
	public GameObjectInstantiate[] objects;

	public Transform parent;

	public bool objectsActive;

	public void Create(int _index)
	{
		if (!objectsActive)
		{
			Object.Instantiate(objects[_index].objectGame, GlobalAM.TransformPivot(GlobalTag.cameraPlayer.transform, objects[_index].position), GlobalTag.cameraPlayer.transform.rotation * Quaternion.Euler(objects[_index].rotation)).transform.parent = parent;
			return;
		}
		objects[_index].objectGame.SetActive(value: true);
		objects[_index].objectGame.transform.parent = parent;
		objects[_index].objectGame.transform.position = GlobalAM.TransformPivot(GlobalTag.cameraPlayer.transform, objects[_index].position);
		objects[_index].objectGame.transform.rotation = GlobalTag.cameraPlayer.transform.rotation * Quaternion.Euler(objects[_index].rotation);
	}

	public void Activation(int _index)
	{
		objects[_index].objectGame.SetActive(value: true);
		objects[_index].objectGame.transform.position = GlobalAM.TransformPivot(GlobalTag.cameraPlayer.transform, objects[_index].position);
		objects[_index].objectGame.transform.rotation = GlobalTag.cameraPlayer.transform.rotation * Quaternion.Euler(objects[_index].rotation);
	}
}
