using System;
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
        private WorldInfo worldInfo;
        private void Start()
        {
            worldInfo = FindObjectOfType<WorldInfo>();
        }

        //[ExecuteAlways]
        private void Update()
        {
            if (manual)
            {
                Fraction = fractionManual;
                return;
            }
            
            Fraction = (Fraction + Time.deltaTime / duration) % 1f;

            
            if (worldInfo != null)
            {
                worldInfo.Night.Value = Fraction < 0.25f || Fraction > 0.75f;
                worldInfo.Day.Value = Fraction > 0.25f && Fraction < 0.75f;
                worldInfo.Dawn.Value = Fraction > 0.25f && Fraction < 0.5f;
                worldInfo.Dusk.Value = Fraction > 0.5f && Fraction < 0.75f;
            }
        }
    }
}