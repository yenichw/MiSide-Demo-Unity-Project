using System.Collections;
using UnityEngine;
#pragma warning disable CS0649

#if EPO_DOTWEEN
using DG.Tweening;
#endif

namespace EPOOutline.Demo
{
    public class BubbleSpot : MonoBehaviour
    {
        [SerializeField]
        private Transform trackPosition;

        [SerializeField]
        private Vector3 trackShift;

        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        private Transform bubble;

        [SerializeField]
        private bool visibleFromBegining = false;

        [SerializeField]
        private float showDelay = 0.0f;

        [SerializeField]
        private float showDuration = 5.0f;

        [SerializeField]
        private bool once;

        private bool wasShown = false;

        private int playersInside = 0;

        private IEnumerator Start()
        {
            Hide(0.0f);

            if (!visibleFromBegining)
                yield break;

            yield return new WaitForSeconds(showDelay);

            Show();

            yield return new WaitForSeconds(showDuration);

            Hide();
        }

        private void Reset()
        {
            targetCamera = Camera.main;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<Character>())
                return;

            if (playersInside++ == 0)
                Show();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.GetComponent<Character>())
                return;

            if (--playersInside == 0)
                Hide();
        }

        private void Show()
        {
            if (wasShown && once)
                return;

            wasShown = true;
            Show(0.5f);
        }

        private void Hide()
        {
            Hide(0.15f);
        }

        private void Hide(float duration)
        {
#if EPO_DOTWEEN
            bubble.transform.DOKill(true);
            bubble.transform.DOScale(Vector3.zero, duration);
#else
            bubble.gameObject.SetActive(false);
#endif
        }

        private void Show(float duration)
        {
#if EPO_DOTWEEN
            bubble.transform.DOKill(true);
            bubble.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutElastic, 0.00001f);
#else
            bubble.gameObject.SetActive(true);
#endif
        }

        private void Update()
        {
            if (trackPosition)
                transform.position = trackPosition.position + trackShift;

            bubble.transform.position = targetCamera.WorldToScreenPoint(transform.position);
        }
    }
}