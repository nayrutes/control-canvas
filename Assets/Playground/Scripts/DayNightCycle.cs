using UnityEngine;

namespace Playground.Scripts
{
    [ExecuteAlways]
    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private float duration = 60f;
        
        [SerializeField]
        private bool manual = false;

        [SerializeField, Range(0,1)]
        private float fractionManual = 0.5f;

        public float Fraction { get; private set; } = 0.5f;
        
        //[ExecuteAlways]
        private void Update()
        {
            if (manual)
            {
                Fraction = fractionManual;
                return;
            }
            
            Fraction = (Fraction + Time.deltaTime / duration) % 1f;
        }
    }
}