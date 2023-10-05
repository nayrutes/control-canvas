using System;
using UniRx;
using UnityEngine;

namespace ControlCanvas.Runtime
{
    public class GenericDecision : IDecision
    {
        public VariableType variableType1;
        public ValueType valueType1;
        public bool bool1;
        public int int1;
        public float float1;
        public string string1;
        public Type blackboardType1;
        public string blackboardKey1;
        
        public VariableType variableType2;
        public ValueType valueType2;
        public bool bool2;
        public int int2;
        public float float2;
        public string string2;
        public Type blackboardType2;
        public string blackboardKey2;
        
        public DecisionType decisionType;
        
        public bool Decide(IControlAgent agentContext)
        {
            return IsSameType(out Type vType) && Compare(vType, agentContext.GetBlackboard(blackboardType1), agentContext.GetBlackboard(blackboardType2));
            

            return false;
        }

        private bool Compare(Type vType, IBlackboard blackboard1, IBlackboard blackboard2)
        {
            object variable1 = GetValue(variableType1, valueType1, bool1, int1, float1, string1, blackboardType1, blackboardKey1, blackboard1);
            object variable2 = GetValue(variableType2, valueType2, bool2, int2, float2, string2, blackboardType2, blackboardKey2, blackboard2);
            
            switch (decisionType)
            {
                case DecisionType.Equal:
                    return Equals(variable1, variable2);
                    break;
                case DecisionType.NotEqual:
                    return !Equals(variable1, variable2);
                    break;
                case DecisionType.GreaterThan:
                    if(vType == typeof(int))
                        return (int)variable1 > (int)variable2;
                    if(vType == typeof(float))
                        return (float)variable1 > (float)variable2;
                    break;
                case DecisionType.LessThan:
                    if(vType == typeof(int))
                        return (int)variable1 < (int)variable2;
                    if(vType == typeof(float))
                        return (float)variable1 < (float)variable2;
                    break;
                case DecisionType.GreaterThanOrEqual:
                    if(vType == typeof(int))
                        return (int)variable1 >= (int)variable2;
                    if(vType == typeof(float))
                        return (float)variable1 >= (float)variable2;
                    break;
                case DecisionType.LessThanOrEqual:
                    if(vType == typeof(int))
                        return (int)variable1 <= (int)variable2;
                    if(vType == typeof(float))
                        return (float)variable1 <= (float)variable2;
                    break;
                case DecisionType.And:
                    if(vType == typeof(bool))
                        return (bool)variable1 && (bool)variable2;
                    break;
                case DecisionType.Or:
                    if(vType == typeof(bool))
                        return (bool)variable1 || (bool)variable2;
                    break;
                // case DecisionType.Not:
                //     if(vType == typeof(bool))
                //         return !(bool)variable1;
                //     break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Debug.LogError($"Comparision not implemented for: {decisionType} with types: {vType}");
            return false;
        }

        private bool IsSameType(out Type type1)
        {
            type1 = GetValueType(variableType1, valueType1, blackboardType1, blackboardKey1);
            Type type2 = GetValueType(variableType2, valueType2, blackboardType2, blackboardKey2);
            bool same = type1 == type2;
            if (!same)
            {
                Debug.LogError($"Types are not the same: {type1} and {type2}");
            }
            return same;
        }

        private Type GetValueType(VariableType variableType, ValueType valueType, Type blackboardType, string blackboardKey)
        {
            switch (variableType)
            {
                case VariableType.Constant:
                    return ValueTypeToType(valueType);
                    break;
                case VariableType.Reference:
                    return BlackboardManager.GetTypeOfProperty(blackboardType, blackboardKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, null);
            }
        }

        private Type ValueTypeToType(ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.Bool:
                    return typeof(bool);
                    break;
                case ValueType.Int:
                    return typeof(int);
                    break;
                case ValueType.Float:
                    return typeof(float);
                    break;
                case ValueType.String:
                    return typeof(string);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
            }
        }

        private object GetValue(VariableType variableType, ValueType valueType, bool boolValue, int intValue, float floatValue, string stringValue, Type blackboardType, string blackboardKey, IBlackboard blackboardInstance)
        {
            switch (variableType)
            {
                case VariableType.Constant:
                    switch (valueType)
                    {
                        case ValueType.Bool:
                            return boolValue;
                        case ValueType.Int:
                            return intValue;
                        case ValueType.Float:
                            return floatValue;
                        case ValueType.String:
                            return stringValue;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
                    }
                case VariableType.Reference:
                    object value = BlackboardManager.GetValueOfProperty(blackboardType, blackboardKey, blackboardInstance);
                    return value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, null);
            }
        }
        
    }
    
    public enum DecisionType
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        And,
        Or,
        //Not
    }

    public enum VariableType
    {
        Constant,
        Reference
    }
    
    public enum ValueType
    {
        Bool,
        Int,
        Float,
        String,
        //Vector2,
        //Vector3,
        //Vector4,
        //Quaternion,
        //Color,
        //Object
    }
    
    
}