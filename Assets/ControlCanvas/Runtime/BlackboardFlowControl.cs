using System.Collections.Generic;
using ControlCanvas.Serialization;

namespace ControlCanvas.Runtime
{
    public class BlackboardFlowControl
    {
        private Dictionary<IControl, object> _blackboard = new ();
        
        public bool TryGet<T>(IControl control, out T val)
        {
            val = default(T);
            if (!_blackboard.ContainsKey(control))
            {
                return false;
            }
            val = (T)_blackboard[control];
            return true;
        }

        public void Set<T>(IControl control, T val)
        {
            _blackboard[control] = val;
        }
        
        public T SetIfNeededWithFunctionAndGet<T>(IControl control, System.Func<T> func)
        {
            if (!TryGet(control, out T val))
            {
                Set(control, func());
            }
            return val;
        }

        public T Get<T>(IControl control, T defaultValue)
        {
            if (!TryGet(control, out T val))
            {
                return defaultValue;
            }
            return val;
        }
    }
}