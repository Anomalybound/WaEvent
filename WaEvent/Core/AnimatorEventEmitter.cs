using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace WaEvent.Core
{
    [Serializable]
    public class PureEventWrapper : UnityEvent<string> { }

    [Serializable]
    public class ObjectEventWrapper : UnityEvent<string, Object> { }

    [Serializable]
    public class StringEventWrapper : UnityEvent<string, string> { }

    [Serializable]
    public class FloatEventWrapper : UnityEvent<string, float> { }

    [Serializable]
    public class IntEventWrapper : UnityEvent<string, int> { }

    [Serializable]
    public class AnimatorEventWarpper : UnityEvent<AnimatorEventArgs> { }

    [RequireComponent(typeof(Animator))]
    public class AnimatorEventEmitter : MonoBehaviour
    {
        public EventDataSet DataSet;

        public AnimatorEventWarpper OnTriggerEvent;
        public PureEventWrapper OnTriggerPureEvent;

        public IntEventWrapper OnTriggerIntEvent;
        public FloatEventWrapper OnTriggerFloatEvent;
        public StringEventWrapper OnTriggerStringEvent;
        public ObjectEventWrapper OnTriggerObjectEvent;

        private RuntimeAnimatorController Controller
        {
            get { return _animator.runtimeAnimatorController; }
        }

        private int ControllerId
        {
            get { return Controller.GetInstanceID(); }
        }

        private Animator _animator;
        private readonly Dictionary<int, int> _stateIndexes = new Dictionary<int, int>();
        private readonly Dictionary<int, float> _stateNormalizedTime = new Dictionary<int, float>();

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            for (var i = 0; i < DataSet.Datas.Count; i++)
            {
                var data = DataSet.Datas[i];
                if (data.TargetController == null) { continue; }

                if (ControllerId != data.TargetController.GetInstanceID()) { continue; }

                foreach (var pair in data.Configs)
                {
                    var index = pair.Key;
                    var currentState = _animator.GetCurrentAnimatorStateInfo(index.Layer);

                    if (!_stateIndexes.ContainsKey(index.Layer))
                    {
                        _stateIndexes.Add(index.Layer, currentState.fullPathHash);
                        _stateNormalizedTime[currentState.fullPathHash] = currentState.normalizedTime % 1f;
                    }
                    else
                    {
                        if (_stateIndexes[index.Layer] != currentState.fullPathHash)
                        {
                            // TODO if transition point too close to the event, event may be ignored.
                            _stateNormalizedTime[currentState.fullPathHash] = currentState.normalizedTime % 1f;
                            _stateIndexes[index.Layer] = currentState.fullPathHash;
                        }
                    }

                    if (index.StateNameHash == currentState.shortNameHash)
                    {
                        var currentNormalizedTime = currentState.normalizedTime % 1f;
                        var lastNormalizedTime = _stateNormalizedTime[currentState.fullPathHash];

                        var events = pair.Value;

                        for (var j = 0; j < events.Count; j++)
                        {
                            var arg = events[j];

                            if (lastNormalizedTime < arg.NormalizedTime &&
                                arg.NormalizedTime <= currentNormalizedTime)
                            {
                                if (OnTriggerEvent != null) { OnTriggerEvent.Invoke(arg); }

                                switch (arg.Type)
                                {
                                    case AnimatorEventType.Float:
                                        if (OnTriggerFloatEvent != null)
                                        {
                                            OnTriggerFloatEvent.Invoke(arg.Name, arg.FloatParm);
                                        }

                                        break;
                                    case AnimatorEventType.Int:
                                        if (OnTriggerIntEvent != null)
                                        {
                                            OnTriggerIntEvent.Invoke(arg.Name, arg.IntParm);
                                        }

                                        break;
                                    case AnimatorEventType.String:
                                        if (OnTriggerStringEvent != null)
                                        {
                                            OnTriggerStringEvent.Invoke(arg.Name, arg.StringParm);
                                        }

                                        break;
                                    case AnimatorEventType.Object:
                                        if (OnTriggerObjectEvent != null)
                                        {
                                            OnTriggerObjectEvent.Invoke(arg.Name, arg.ObjectParm);
                                        }

                                        break;
                                    case AnimatorEventType.Pure:
                                        if (OnTriggerPureEvent != null) { OnTriggerPureEvent.Invoke(arg.Name); }

                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }

                        _stateNormalizedTime[currentState.fullPathHash] = currentNormalizedTime;
                    }
                }
            }
        }
    }
}