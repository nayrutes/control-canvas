using System;
using Playground.Scripts.AI;
using UnityEngine;

namespace Playground.Scripts
{
    public class Hero : MonoBehaviour
    {
        [SerializeField]
        private WorldInfo _worldInfo;
        private void Start()
        {
            GetComponent<Character2DAgent>().AddBlackboard(_worldInfo);
        }
    }
}