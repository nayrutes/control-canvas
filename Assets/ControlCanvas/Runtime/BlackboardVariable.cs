using System;

namespace ControlCanvas.Runtime
{
    [System.Serializable]
    public class BlackboardVariable<T>
    {
        public Type blackboardType;
        public string blackboardKey;

        public void SetBlackboardType(Type blackboardType)
        {
            this.blackboardType = blackboardType;
        }
        
        public void SetBlackboardKey(string blackboardKey)
        {
            this.blackboardKey = blackboardKey;
        }
        
        public T GetValue(IControlAgent agentContext)
        {
            IBlackboard blackboard = agentContext.GetBlackboard(blackboardType);
            if (blackboard == null)
            {
                return default;
            }

            if (typeof(T) == typeof(IObservable<object>))
            {
                return (T)BlackboardManager.GetValueOfProperty<IObservable<object>, object>(blackboardType, blackboardKey, blackboard);
            }

            return BlackboardManager.GetValueOfProperty<T>(blackboardType, blackboardKey, blackboard);
        }
    }
    
    // [System.Serializable]
    // public class BlackboardVariableObservable<T>
    // {
    //     public Type blackboardType;
    //     public string blackboardKey;
    //
    //     public void SetBlackboardType(Type blackboardType)
    //     {
    //         this.blackboardType = blackboardType;
    //     }
    //     
    //     public void SetBlackboardKey(string blackboardKey)
    //     {
    //         this.blackboardKey = blackboardKey;
    //     }
    //     
    //     public T GetValue(IControlAgent agentContext) 
    //     {
    //         IBlackboard blackboard = agentContext.GetBlackboard(blackboardType);
    //         if (blackboard == null)
    //         {
    //             return default;
    //         }
    //
    //         return BlackboardManager.GetValueOfProperty<T, object>(blackboardType, blackboardKey, blackboard);
    //     }
    // }
}