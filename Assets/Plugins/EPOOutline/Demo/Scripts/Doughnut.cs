using System.Collections;
using UnityEngine;
#pragma warning disable CS0649
#pragma warning disable CS0414

namespace EPOOutline.Demo
{
    public class Doughnut : MonoBehaviour, ICollectable
    {
        [SerializeField]
        private float rotationSpeed = 30.0f;

        [SerializeField]
        private AudioClip eatSound;

        [SerializeField]
        private float moveAmplitude = 0.25f;

        [SerializeField]
        private float moveSpeed = 0.2f;

        private Outlinable outlinable;

        private Vector3 initialPosition;

        private float amplitudeShift = 0.0f;

        private bool isCollected = false;

        private void Start()
        {
            outlinable = GetComponent<Outlinable>();
            amplitudeShift = Random.Range(0.0f, 10.0f);
            initialPosition = transform.position;
        }

        private void Update()
        {
            if (!isCollected)
                transform.position = initialPosition + Vector3.up * Mathf.Sin(Time.time * moveSpeed + amplitudeShift);

            transform.Rotate(Vector3.up * rotationSpeed * Time.smoothDeltaTime, Space.World);
        }

        public void Collect(GameObject collector)
        {
            if (isCollected)
                return;

            isCollected = true;

            StartCoroutine(AnimateCollection(collector));
        }

        private IEnumerator AnimateCollection(GameObject collector)
        {
            AudioSource.PlayClipAtPoint(eatSound, transform.position, 10);

            var duration = 0.2f;
            var collectionRadius = 1.5f;
            var collectionAngle = Random.Range(0.0f, 360.0f);
            var timeLeft = duration;

            while (collector != null && timeLeft > 0.0f)
            {
                timeLeft -= Time.smoothDeltaTime;

                var collectionShift = Quaternion.Euler(0, collectionAngle, 0) * Vector3.right;

                var targetPosition = collector.transform.position + collectionShift + Vector3.up * 4.5f;

                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.smoothDeltaTime * 5.0f);

                collectionAngle += Time.smoothDeltaTime * 360.0f;
                collectionRadius = Mathf.MoveTowards(collectionRadius, 0.0f, Time.smoothDeltaTime * 3.5f);

                yield return null;
            }

            timeLeft = duration;

            var initialScale = transform.localScale;
            while (timeLeft >= 0.0f)
            {
                timeLeft -= Time.smoothDeltaTime;

                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, 1.0f - (timeLeft / duration));

                yield return null;
            }

            transform.localScale = Vector3.zero;

            Destroy(gameObject);
        }
    }
}