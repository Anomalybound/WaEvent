using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace WaEvent.Core
{
    [Serializable]
    public class AnimatorEventWarpper : UnityEvent<AnimatorEventArgs> { }

    [RequireComponent(typeof(Animator))]
    public class AnimatorEventEmitter : MonoBehaviour
    {
        public EventDataSet DataSet;

        public AnimatorEventWarpper OnTriggerEvent;

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
                            }
                        }

                        _stateNormalizedTime[currentState.fullPathHash] = currentNormalizedTime;
                    }
                }
            }
        }
    }
}