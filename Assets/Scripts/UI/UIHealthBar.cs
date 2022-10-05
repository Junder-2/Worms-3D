using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIHealthBar : MonoBehaviour
    {
        [SerializeField] 
        private Image healthBarImage;

        [SerializeField] 
        private GameObject[] displayHat = new GameObject[4];
    
        private Material _healthUIMat;
    
        private static readonly int HealthValuesA = Shader.PropertyToID("_HealthValuesA");
        private static readonly int HealthValuesB = Shader.PropertyToID("_HealthValuesB");
    
        public void SetupHealth(byte wormAmount, int playerIndex)
        {
            Material UIMatCopy = healthBarImage.material;
        
            int assignedPreset = GameRules.PlayerAssignedPreset[playerIndex];

            Vector4 aValues = Vector4.zero;
            Vector4 bValues = Vector4.zero;

            for (int i = 0; i < wormAmount; i++)
            {
                int index = i % 4;

                if (i < 4)
                    aValues[index] = 1;
                else
                    bValues[index] = 1;

            }
        
            UIMatCopy.SetVector(HealthValuesA, aValues);
            UIMatCopy.SetVector(HealthValuesB, bValues);
            UIMatCopy.SetFloat("_MaximumHealth", wormAmount);
        
            UIMatCopy.SetColor("_Color", GameRules.PlayerUIColors[assignedPreset]);
            _healthUIMat = new Material(UIMatCopy);

            for (int i = 0; i < displayHat.Length; i++)
            {
                displayHat[i].SetActive(i == assignedPreset);
            }
        
            healthBarImage.material = _healthUIMat;
        }

        public void UpdateHealth(byte worm, float health)
        {
            int index = worm % 4;
        
            if (worm < 4)
            {
                Vector4 values = _healthUIMat.GetVector(HealthValuesA);
                values[index] = health;
                _healthUIMat.SetVector(HealthValuesA, values);
            }
            else
            {
                Vector4 values = _healthUIMat.GetVector(HealthValuesB);
                values[index] = health;
                _healthUIMat.SetVector(HealthValuesB, values);
            }
        }

        public void Display(bool value) => gameObject.SetActive(value);
    }
}
