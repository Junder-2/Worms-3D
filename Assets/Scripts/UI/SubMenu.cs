using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class SubMenu : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelect;
        [SerializeField] private GameObject menuWindow;

        [SerializeField] private UnityEvent setup;

        public GameObject GetFirstSelect() => firstSelect;

        public GameObject GetWindow() => menuWindow;

        public void Setup() => setup.Invoke();
    }
}
