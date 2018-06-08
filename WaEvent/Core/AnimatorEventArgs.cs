using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace WaEvent.Core
{
    [Serializable]
    public class AnimatorEvents : List<AnimatorEventArgs>
    {
        public AnimatorEvents() { }

        public AnimatorEvents(IEnumerable<AnimatorEventArgs> args)
        {
            AddRange(args);
        }
    }

    public enum AnimatorEventType
    {
        Float,
        Int,
        String,
        Object
    }

    [Serializable]
    public class AnimatorEventArgs
    {
        public float NormalizedTime;
        public string Name;
        public AnimatorEventType Type;

        public float FloatParm;
        public int IntParm;
        public string StringParm;
        public Object ObjectParm;

        public override string ToString()
        {
            return string.Format("Name: {0} @{1:F3} - Type:{2}", Name, NormalizedTime, Type);
        }
    }
}