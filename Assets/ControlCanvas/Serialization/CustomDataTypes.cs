﻿using System;
using UnityEngine;

namespace ControlCanvas.Serialization
{
    // Serializable version of Vector2
    [Serializable]
    public struct SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        // public Vector2 ToVector2()
        // {
        //     return new Vector2(x, y);
        // }
        public static implicit operator Vector2(SerializableVector2 v)
        {
            return new Vector2(v.x, v.y);
        }
        public static implicit operator SerializableVector2(Vector2 v)
        {
            return new SerializableVector2(v.x, v.y);
        }
    }
}