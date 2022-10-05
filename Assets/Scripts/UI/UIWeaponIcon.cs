using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIWeaponIcon : MonoBehaviour
    {
        [SerializeField] private GameObject selector;

        [SerializeField] private Image image;

        [SerializeField] private TextMeshProUGUI amountText;

        private void Start()
        {
            SetSelector(false);
        }

        public void SetSelector(bool state)
        {
            selector.SetActive(state);
        }

        private void SetAvailability(bool state)
        {
            image.color = state ? Color.white : new Color(1, 1, 1, .25f);
        }

        public void SetAmount(int amount)
        {
            amountText.text = amount < 0 ? "" : amount.ToString();

            SetAvailability(amount != 0);
        }
    }
}
