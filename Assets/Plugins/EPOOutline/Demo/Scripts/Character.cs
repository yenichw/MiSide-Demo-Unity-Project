using UnityEngine;
using UnityEngine.AI;
#pragma warning disable CS0649

namespace EPOOutline.Demo
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private AudioSource walkSource;

        [SerializeField]
        private NavMeshAgent agent;

        [SerializeField]
        private Animator characterAnimator;

        private float initialWalkVolume = 0.0f;

        private Camera mainCamera;

        private void Start()
        {
            initialWalkVolume = walkSource.volume;
            mainCamera = Camera.main;
            agent.updateRotation = false;
        }

        private void Update()
        {
            var forward = mainCamera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = mainCamera.transform.right;
            right.y = 0;
            right.Normalize();

            var direction = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");

            if (direction.magnitude > 0.1f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * agent.angularSpeed);

            agent.velocity = direction.normalized * agent.speed;

            walkSource.volume = initialWalkVolume * (agent.velocity.magnitude / agent.speed);

            characterAnimator.SetBool("IsRunning", direction.magnitude > 0.1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            var collectable = other.GetComponent<ICollectable>();
            if (collectable == null)
                return;

            collectable.Collect(gameObject);
        }
    }
}