using System;
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
        
        public override string ToString()
        {
            return $"SerializableVector2:({x}, {y})";
        }
        
        //override comparison operators
        public static bool operator ==(SerializableVector2 v1, SerializableVector2 v2)
        {
            return Math.Abs(v1.x - v2.x) < 0.001f && Math.Abs(v1.y - v2.y) < 0.001f;
        }
        public static bool operator !=(SerializableVector2 v1, SerializableVector2 v2)
        {
            return !(v1 == v2);
        }
        
        //override the Object.Equals(object o) method
        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            return this == (SerializableVector2)o;
        }
        
        //override the Object.GetHashCode() method
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
    }
}