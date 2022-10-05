using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UITurnTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
    
        public void SetTime(int value)
        {
            timeText.text = value.ToString();
            _time = value;
        }

        private bool _countTime;
        private float _time;

        public void StartTime(bool state) => _countTime = state;

        private void Update()
        {
            if(!_countTime)
                return;

            _time -= Time.deltaTime;
        
            timeText.text = Mathf.CeilToInt(Math.Max(0, _time)).ToString();

            if (_time < 0)
                _countTime = false;
        }
    }
}
