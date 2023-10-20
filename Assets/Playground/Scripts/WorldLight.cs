using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Playground.Scripts
{
    [RequireComponent(typeof(Light2D))]
    [ExecuteAlways]
    public class WorldLight : MonoBehaviour
    {
        [Range(0,1)]
        public float fraction = 0.5f;
        [SerializeField]
        private DayNightCycle dayNightCycle;
        [SerializeField]
        private Gradient gradient;
        [SerializeField]
        private Light2D light2D;

        private void Update()
        {
            if(dayNightCycle != null)
            {
                fraction = dayNightCycle.Fraction;
            }
            if (gradient == null)
            {
                Debug.LogWarning("No gradient set for world light");
                return;
            }
            if(light2D == null)
            {
                Debug.LogWarning("No light set for world light");
                return;
            }
            light2D.color = gradient.Evaluate(fraction);
        }
    }
}