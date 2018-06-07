using System;
using System.Collections.Generic;
using UnityEngine;
using WaEvent.Util;
using Object = UnityEngine.Object;

namespace WaEvent.Core
{
    [CreateAssetMenu(menuName = "WaEvent/New Event Data")]
    public class EventDataSet : ScriptableObject
    {
        public List<EventData> Datas = new List<EventData>();
    }

    [Serializable]
    public struct StateIndex
    {
        public int Layer;
        public int StateNameHash;

        public StateIndex(int layer, int stateNameHash)
        {
            Layer = layer;
            StateNameHash = stateNameHash;
        }

        public override bool Equals(object obj)
        {
            var other = (StateIndex) obj;
            return Equals(other);
        }

        public bool Equals(StateIndex other)
        {
            return Layer == other.Layer && StateNameHash == other.StateNameHash;
        }

        public override int GetHashCode()
        {
            unchecked { return (Layer * 397) ^ StateNameHash; }
        }
    }

    [Serializable]
    public class StateIndexDictioanry : SerializableDictionary<StateIndex, AnimatorEvents> { }

    [Serializable]
    public class EventData
    {
        public string Name = "New Event Data";

        public Object TargetController;
        public StateIndexDictioanry Configs;

        public EventData()
        {
            TargetController = null;
            Configs = new StateIndexDictioanry();
        }
    }
}