using UnityEngine;
using UnityEngine.EventSystems;
#pragma warning disable CS0649

namespace EPOOutline.Demo
{
    public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private AudioClip interactionSound;

        [SerializeField]
        private bool affectOutlinable = true;

        private Outlinable outlinable;

        private void Start()
        {
            if (!affectOutlinable)
                return;

            outlinable = GetComponent<Outlinable>();
            outlinable.enabled = false;

            outlinable.FrontParameters.FillPass.SetFloat("_PublicAngle", 35.0f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!affectOutlinable)
                return;

            outlinable.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!affectOutlinable)
                return;

            outlinable.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            AudioSource.PlayClipAtPoint(interactionSound, transform.position, 1.0f);
        }
    }
}