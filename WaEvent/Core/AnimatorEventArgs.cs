using System;
using Object = UnityEngine.Object;

namespace WaEvent.Core
{
    [Serializable]
    public class AnimatorEventArgs
    {
        public float NormalizedTime;
        public string Name;

        public float FloatParm;
        public int IntParm;
        public string StringParm;
        public Object ObjectParm;

        public override string ToString()
        {
            return string.Format("Name: {0} @{1:F3}", Name, NormalizedTime);
        }
    }
}