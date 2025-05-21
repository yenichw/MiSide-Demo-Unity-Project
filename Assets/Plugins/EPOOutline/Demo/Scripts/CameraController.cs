using UnityEngine;
#pragma warning disable CS0649

namespace EPOOutline.Demo
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Vector3 shift;

        [SerializeField]
        private float moveSpeed = 2.0f;

        [SerializeField]
        private Transform target;

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, target.position + shift, Time.deltaTime * moveSpeed);
        }
    }
}