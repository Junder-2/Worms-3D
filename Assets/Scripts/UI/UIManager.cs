using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [SerializeField] 
        private UIHealthBar[] healthBar;

        [SerializeField] private UIWeaponIcon[] weaponIcon;

        [SerializeField] private UITurnTimer turnTimer;

        [SerializeField]
        private UIWinDisplay winDisplay;

        [SerializeField] private MainMenuManager menuManager;

        private InputsActions _inputs;
    
        private void Awake()
        {
            Instance = this;

            _inputs = new InputsActions();
        
            _inputs.UI.Options.started += _ => PauseGame(!_paused);
        
            PauseGame(false);
        }

        private void OnDisable() => _inputs.UI.Disable();

        private void OnEnable() => _inputs.UI.Enable();

        public void SetUpPlayersHealth(byte playerAmount, byte wormAmount)
        {
            for (int i = 0; i < healthBar.Length; i++)
            {
                if (i < playerAmount)
                {
                    healthBar[i].SetupHealth(wormAmount, i);
                    healthBar[i].Display(true);
                }
                else
                    healthBar[i].Display(false);
            }
        }

        public void UpdatePlayerHealth(byte player, byte worm, float health)
        {
            healthBar[player].UpdateHealth(worm, health);
        }

        public void InstanceWeaponUI(int[] amount, int selected)
        {
            for (int i = 0; i < weaponIcon.Length; i++)
            {
                if(selected >= 0 && selected == i)weaponIcon[i].SetSelector(true);
                else weaponIcon[i].SetSelector(false);
                weaponIcon[i].SetAmount(amount[i]);
            }
        }

        public void UpdateWeaponUI(int selected, int amount)
        {
            foreach (var t in weaponIcon)
            {
                t.SetSelector(false);
            }

            if (selected < 0) return;
            weaponIcon[selected].SetSelector(true);
            weaponIcon[selected].SetAmount(amount);
        }

        public void StartTimerUI(float time, bool start)
        {
            turnTimer.SetTime((int)time);
            turnTimer.StartTime(start);
        }

        private bool _paused;

        private void PauseGame(bool state)
        {
            if (state)
            {
                menuManager.gameObject.SetActive(true);
                menuManager.SwitchMenu(0);

                Time.timeScale = 0;
            }
            else
            {
                menuManager.gameObject.SetActive(false);

                Time.timeScale = 1;
            }

            _paused = state;
        }

        public void SetWinText(byte playerIndex) => winDisplay.SetWinner(playerIndex);
    }
}
