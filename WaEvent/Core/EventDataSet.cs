using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WaEvent.Core
{
    [CreateAssetMenu(menuName = "WaEvent/New Event Data")]
    public class EventDataSet : ScriptableObject
    {
        public List<EventData> Datas = new List<EventData>();
    }

    [Serializable]
    public class EventData
    {
        public string Name = "New Event Data";

        public Object TargetController;

        public int Layer;
        public int StateNameHash;
        public List<AnimatorEventArgs> Events;

        public EventData()
        {
            TargetController = null;
            Layer = -1;
            StateNameHash = -1;
            Events = new List<AnimatorEventArgs>();
        }

        public EventData(EventData other)
        {
            TargetController = other.TargetController;
            Layer = other.Layer;
            StateNameHash = other.StateNameHash;
            Events = new List<AnimatorEventArgs>(other.Events);
        }
    }
}