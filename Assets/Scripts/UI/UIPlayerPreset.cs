using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIPlayerPreset : MonoBehaviour
    {
        [SerializeField] private Image iconDisplay;

        public void SetIcon(Sprite icon) => iconDisplay.sprite = icon;

        public bool Active
        {
            get => _active;

            set
            {
                _active = value;
            
                gameObject.SetActive(_active);
            }
        }

        private bool _active;
    }
}
