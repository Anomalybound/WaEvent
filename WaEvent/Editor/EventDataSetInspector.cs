using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WaEvent;
using WaEvent.Core;
using WaEvent.Editor.Util;
using Object = UnityEngine.Object;

[CustomEditor(typeof(EventDataSet))]
public class EventDataSetInspector : Editor
{
    private EventDataSet _dataSet;

    private EventData _selectedData;
    private AnimatorEventArgs _selectedEventArg;
    private string _selectedName;
    private string _selectedEventName;
    private int _selectedLayer = -1;
    private int _selectedState = -1;
    private float _normalizedSetting;

    #region Preview Variables

    private AvatarPreviewWrapper _avatarPreview;
    private AnimatorController controller;
    private AnimatorStateMachine stateMachine;
    private AnimatorState state;
    private Motion previewedMotion;
    private bool controllerIsDitry;
    private bool PrevIKOnFeet;

    #endregion

    private List<Action> _delayedActions = new List<Action>();

    private void OnEnable()
    {
        _dataSet = target as EventDataSet;
        ResetAll();
    }

    private void ResetAll()
    {
        _selectedData = null;
        _selectedEventArg = null;
        previewedMotion = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (GUILayout.Button("Add New Data"))
        {
            _dataSet.Datas.Add(new EventData {Name = "Event Data " + _dataSet.Datas.Count});
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("0. Select Event Data Preset", EditorStyles.centeredGreyMiniLabel);
        for (var i = 0; i < _dataSet.Datas.Count; i++)
        {
            var data = _dataSet.Datas[i];
            using (new EditorGUILayout.HorizontalScope())
            {
                if (data == _selectedData) { GUI.backgroundColor = Color.green; }

                if (GUILayout.Button(data.Name))
                {
                    ResetAll();
                    _selectedData = data;
                    _selectedName = data.Name;
                }

                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("DEL", GUILayout.Width(45)))
                {
                    _delayedActions.Add(() => { _dataSet.Datas.Remove(data); });
                    if (_selectedData == data) { _selectedData = null; }
                }
            }
        }

        EditorGUILayout.EndVertical();

        if (_selectedData != null) { DrawEventData(_selectedData); }

        // Remove Event Datas
        for (var i = _delayedActions.Count - 1; i >= 0; i--) { _delayedActions[i].Invoke(); }

        _delayedActions.Clear();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEventData(EventData data)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Display Name: ", GUILayout.MaxWidth(100));
            _selectedName = EditorGUILayout.TextField(_selectedName);
            if (GUILayout.Button("Rename", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                data.Name = _selectedName;
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.LabelField("Target Controller: ", GUILayout.MaxWidth(100));
                data.TargetController = EditorGUILayout.ObjectField(data.TargetController,
                    typeof(AnimatorController), false) as AnimatorController;

                if (check.changed) { }
            }
        }

        if (data.TargetController != null)
        {
            // Draw Layers
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. Select Layer", EditorStyles.centeredGreyMiniLabel);

            var animatorController = (AnimatorController) data.TargetController;
            for (var i = 0; i < animatorController.layers.Length; i++)
            {
                var layer = animatorController.layers[i];

                if (i == _selectedLayer) { GUI.backgroundColor = Color.green; }

                if (GUILayout.Button(i + " - " + layer.name)) { _selectedLayer = i; }

                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndVertical();

            // Draw States
            if (_selectedLayer != -1)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("2. Select State", EditorStyles.centeredGreyMiniLabel);

                var stateMachine = animatorController.layers[_selectedLayer].stateMachine;
                for (var i = 0; i < stateMachine.states.Length; i++)
                {
                    var state = stateMachine.states[i];

                    if (state.state.nameHash == _selectedState) { GUI.backgroundColor = Color.green; }

                    if (GUILayout.Button(state.state.name))
                    {
                        SetupPreview(state.state.motion);
                        previewedMotion = state.state.motion;
                        _selectedState = state.state.nameHash;
//                        Debug.Log(_selectedState);
                        _selectedEventArg = null;
                    }

                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
            }

            // Draw Events
            if (_selectedState != -1)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                var index = new StateIndex(_selectedLayer, _selectedState);
                if (!data.Configs.ContainsKey(index)) { data.Configs.Add(index, new AnimatorEvents()); }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Normalized", GUILayout.Width(100));

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        _normalizedSetting = EditorGUILayout.Slider(_normalizedSetting, 0, 1);
                        if (check.changed)
                        {
                            if (_avatarPreview != null)
                            {
                                _avatarPreview.Animator.Play(0, 0, _normalizedSetting);
                                _avatarPreview.timeControl.normalizedTime = _normalizedSetting;
                            }

                            if (_selectedEventArg != null)
                            {
                                _selectedEventArg.NormalizedTime = _normalizedSetting;
                                data.Configs[index] =
                                    new AnimatorEvents(data.Configs[index].OrderBy(x => x.NormalizedTime));
                            }
                        }
                    }

                    if (GUILayout.Button("Add Event", EditorStyles.toolbarButton))
                    {
                        var countNum = data.Configs[index].Count;

                        var newEventArg = new AnimatorEventArgs
                        {
                            Name = "New Event " + countNum,
                            NormalizedTime = _normalizedSetting
                        };

                        data.Configs[index].Add(newEventArg);
                        data.Configs[index] = new AnimatorEvents(data.Configs[index].OrderBy(x => x.NormalizedTime));

                        _selectedEventArg = newEventArg;
                    }
                }

                EditorGUILayout.LabelField("3. Edit Events", EditorStyles.centeredGreyMiniLabel);

                for (var i = 0; i < data.Configs[index].Count; i++)
                {
                    var evt = data.Configs[index][i];
                    var normalizedTime = evt.NormalizedTime.ToString("F3");

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (_selectedEventArg == evt) { GUI.backgroundColor = Color.green; }

                        if (GUILayout.Button(normalizedTime + " - " + evt.Name))
                        {
                            if (_selectedEventArg == evt)
                            {
                                _selectedEventArg = null;
                                continue;
                            }

                            _selectedEventArg = evt;
                            _selectedEventName = evt.Name;
                            _normalizedSetting = evt.NormalizedTime;

                            if (_avatarPreview != null)
                            {
                                _avatarPreview.Animator.Play(0, 0, evt.NormalizedTime);
                                _avatarPreview.timeControl.normalizedTime = evt.NormalizedTime;
                            }
                        }

                        GUI.backgroundColor = Color.white;

                        if (GUILayout.Button("DEL", GUILayout.Width(45)))
                        {
                            _delayedActions.Add(() => data.Configs[index].Remove(evt));
                            if (_selectedEventArg == evt) { _selectedEventArg = null; }
                        }
                    }
                }

                if (_selectedEventArg != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Display Name: ", GUILayout.MaxWidth(100));
                        _selectedEventName = EditorGUILayout.TextField(_selectedEventName);
                        if (GUILayout.Button("Rename", EditorStyles.toolbarButton, GUILayout.Width(60)))
                        {
                            _selectedEventArg.Name = _selectedEventName;
                        }
                    }

                    _selectedEventArg.Type =
                        (AnimatorEventType) EditorGUILayout.EnumPopup("Event Type", _selectedEventArg.Type);

                    switch (_selectedEventArg.Type)
                    {
                        case AnimatorEventType.Float:
                            _selectedEventArg.FloatParm =
                                EditorGUILayout.FloatField("Float Parameter", _selectedEventArg.FloatParm);
                            break;
                        case AnimatorEventType.Int:
                            _selectedEventArg.IntParm =
                                EditorGUILayout.IntField("Int Parameter", _selectedEventArg.IntParm);
                            break;
                        case AnimatorEventType.String:
                            _selectedEventArg.StringParm =
                                EditorGUILayout.TextField("String Parameter", _selectedEventArg.StringParm);
                            break;
                        case AnimatorEventType.Object:
                            _selectedEventArg.ObjectParm = EditorGUILayout.ObjectField("Object Parameter",
                                _selectedEventArg.ObjectParm, typeof(Object), false);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndVertical();
    }

    #region Previews

    public override bool HasPreviewGUI()
    {
        return _avatarPreview != null;
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        if (_avatarPreview == null || previewedMotion == null)
            return;

        UpdateAvatarState();
        _avatarPreview.DoAvatarPreview(r, background);
    }

    public override void OnPreviewSettings()
    {
        if (_avatarPreview != null) { _avatarPreview.DoPreviewSettings(); }
    }

    private void SetupPreview(Motion stateMotion)
    {
        if (previewedMotion == stateMotion)
            return;

        previewedMotion = stateMotion;

        ClearStateMachine();

        if (_avatarPreview == null)
        {
            _avatarPreview = new AvatarPreviewWrapper(null, previewedMotion)
            {
                OnAvatarChangeFunc = OnPreviewAvatarChanged
            };
            PrevIKOnFeet = _avatarPreview.IKOnFeet;
        }

        if (stateMotion != null) { CreateStateMachine(); }

        Repaint();
    }

    private void ClearStateMachine()
    {
        if (_avatarPreview != null && _avatarPreview.Animator != null)
        {
            AnimatorController.SetAnimatorController(_avatarPreview.Animator, null);
        }

        if (controller != null) { controller.RemoveOnAnimatorControllerDirtyCallback(this.ControllerDitry); }

        DestroyImmediate(controller);
        //Object.DestroyImmediate(this.stateMachine);
        DestroyImmediate(state);
        stateMachine = null;
        controller = null;
        state = null;
    }

    private void CreateStateMachine()
    {
        if (_avatarPreview == null || _avatarPreview.Animator == null)
            return;

        if (controller == null)
        {
            controller = new AnimatorController();
            controller.SetPushUndo(false);
            controller.AddLayer("preview");
            stateMachine = controller.layers[0].stateMachine;
            stateMachine.SetPushUndo(false);

            CreateParameters();
            state = stateMachine.AddState("preview");
            state.SetPushUndo(false);
            state.motion = previewedMotion;
            state.iKOnFeet = _avatarPreview.IKOnFeet;
            state.hideFlags = HideFlags.HideAndDontSave;
            controller.hideFlags = HideFlags.HideAndDontSave;
            stateMachine.hideFlags = HideFlags.HideAndDontSave;

            AnimatorController.SetAnimatorController(_avatarPreview.Animator, controller);
            controller.AppendOnAnimatorControllerDirtyCallback(ControllerDitry);
            controllerIsDitry = false;
        }

        if (AnimatorControllerExtension.GetEffectiveAnimatorController(_avatarPreview.Animator) != controller)
        {
            AnimatorController.SetAnimatorController(_avatarPreview.Animator, controller);
        }

        _avatarPreview.timeControl.normalizedTime = 0;
    }

    private void CreateParameters()
    {
        var blendTree = previewedMotion as BlendTree;
        if (blendTree != null)
        {
            for (var j = 0; j < blendTree.GetRecursiveBlendParamCount(); j++)
            {
                controller.AddParameter(blendTree.GetRecursiveBlendParam(j), AnimatorControllerParameterType.Float);
            }
        }
    }

    private void ControllerDitry()
    {
        controllerIsDitry = true;
    }

    private void UpdateAvatarState()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        if (_avatarPreview.PreviewObject == null || controllerIsDitry)
        {
            _avatarPreview.ResetPreviewInstance();

            if (_avatarPreview.PreviewObject != null) { ResetStateMachine(); }
        }

        var animator = _avatarPreview.Animator;

        if (animator != null)
        {
            if (PrevIKOnFeet != _avatarPreview.IKOnFeet)
            {
                PrevIKOnFeet = _avatarPreview.IKOnFeet;
                var rootPosition = _avatarPreview.Animator.rootPosition;
                var rootRotation = _avatarPreview.Animator.rootRotation;

                ResetStateMachine();

                _avatarPreview.Animator.UpdateWrapper(_avatarPreview.timeControl.currentTime);
                _avatarPreview.Animator.UpdateWrapper(0f);
                _avatarPreview.Animator.rootPosition = rootPosition;
                _avatarPreview.Animator.rootRotation = rootRotation;
            }

            if (_avatarPreview.Animator != null)
            {
                var blendTree = previewedMotion as BlendTree;

                if (blendTree != null)
                {
                    for (var i = 0; i < blendTree.GetRecursiveBlendParamCount(); i++)
                    {
                        var recurvieBlendParameter = blendTree.GetRecursiveBlendParam(i);
                        var inputBlendValue = blendTree.GetInputBlendVal(recurvieBlendParameter);
                        _avatarPreview.Animator.SetFloat(recurvieBlendParameter, inputBlendValue);
                    }
                }
            }

            _avatarPreview.timeControl.loop = true;

            var length = 1f;
            var currentTime = 0f;
            if (animator.layerCount > 0)
            {
                var currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
                length = currentAnimatorStateInfo.length;
                currentTime = currentAnimatorStateInfo.normalizedTime;
            }

            _avatarPreview.timeControl.startTime = 0f;
            _avatarPreview.timeControl.stopTime = length;
            _avatarPreview.timeControl.Update();

            var num3 = _avatarPreview.timeControl.deltaTime;
            if (float.IsInfinity(num3)) { num3 = 0; }

            if (!previewedMotion.isLooping)
            {
                if (currentTime >= 1f) { num3 -= length; }
                else
                {
                    if (currentTime < 0f) { num3 += length; }
                }
            }

            animator.UpdateWrapper(num3);
        }
    }

    public void ResetStateMachine()
    {
        ClearStateMachine();
        CreateStateMachine();
    }

    private void OnPreviewAvatarChanged()
    {
        ResetStateMachine();
    }

    #endregion
}