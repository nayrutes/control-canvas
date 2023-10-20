using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Playground.Scripts
{
    [RequireComponent(typeof(Light2D))]
    public class LanternLight : MonoBehaviour
    {
        [SerializeField]
        private Light2D light2D;

        private float _defaultOuterRadius;
        [SerializeField]
        private float outerRadiusVariation = 0.1f;

        private float _defaultIntensity;
        [SerializeField]
        private float intensityVariation = 0.1f;

        private float _noiseOffset;
        
        [SerializeField]
        private bool startLightOn = true;
        private void Start()
        {
            light2D = GetComponent<Light2D>();
            _defaultOuterRadius = light2D.pointLightOuterRadius;
            _defaultIntensity = light2D.intensity;
            _noiseOffset = UnityEngine.Random.Range(0f, 100f);
            if (startLightOn)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }

        private void Update()
        {
            //let the light flicker smoothed over time
            light2D.pointLightOuterRadius = Mathf.Lerp(_defaultOuterRadius - outerRadiusVariation, _defaultOuterRadius + outerRadiusVariation, Mathf.PerlinNoise(Time.time, _noiseOffset));
            light2D.intensity = Mathf.Lerp(_defaultIntensity - intensityVariation, _defaultIntensity + intensityVariation, Mathf.PerlinNoise(Time.time, _noiseOffset));
        }
        
        public void TurnOff()
        {
            light2D.enabled = false;
        }
        
        public void TurnOn()
        {
            light2D.enabled = true;
        }
    }
}