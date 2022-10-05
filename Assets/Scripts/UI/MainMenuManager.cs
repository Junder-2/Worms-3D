using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private SubMenu[] menus;

        [SerializeField] private EventSystem eventSystem;

        private void Start()
        {
            SetupMenus();
            SetMenu(0);
        }

        public void PlayMenuMusic()
        {
            AudioManager.Instance.PlayMusic((int)AudioSet.MusicID.menu);
        }

        private void SetupMenus()
        {
            foreach (var t in menus)
            {
                t.Setup();
            }
        }

        private int menuIndex = 0;
        private void SetMenu(int index)
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (index == i)
                {
                    menus[i].GetWindow().SetActive(true);
                    eventSystem.SetSelectedGameObject(menus[i].GetFirstSelect());
                    eventSystem.UpdateModules();
                }
                else
                {
                    menus[i].GetWindow().SetActive(false);
                }
            }

            menuIndex = index;
        }

        public void SwitchMenu(int index)
        {
            if(menuIndex - index > 0)
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.uiClickB);
            else 
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.uiClickA);
        
            SetMenu(index);
        }

        public void OnQuitButton()
        {
            AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.uiClickB);
            Application.Quit();
        }

        public void SwitchScenes(int index)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(index);
        }
    }
}
