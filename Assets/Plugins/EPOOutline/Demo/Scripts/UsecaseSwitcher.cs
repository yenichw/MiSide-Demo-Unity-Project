using UnityEngine;

namespace EPOOutline.Demo
{
    public class UsecaseSwitcher : MonoBehaviour
    {
        private Transform currentSelected;

        private void Start()
        {
            for (var index = 0; index < transform.childCount; index++)
                transform.GetChild(index).gameObject.SetActive(index == 0);

            currentSelected = transform.GetChild(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                var currentIndex = currentSelected.GetSiblingIndex();
                transform.GetChild(currentIndex).gameObject.SetActive(false);
                currentIndex++;
                currentSelected = transform.GetChild(currentIndex % transform.childCount);
                currentSelected.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                var currentIndex = currentSelected.GetSiblingIndex();
                transform.GetChild(currentIndex).gameObject.SetActive(false);
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = transform.childCount - 1;

                currentSelected = transform.GetChild(currentIndex);
                currentSelected.gameObject.SetActive(true);
            }
        }
    }
}