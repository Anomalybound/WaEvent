using System;
using System.Reflection;
using UnityEditor.Animations;
using UnityEngine;

namespace WaEvent
{
    public static class AnimatorExtension
    {
        private static Type realType;

        private static MethodInfo method_Update;

        public static void InitType()
        {
            if (realType == null)
            {
                realType = typeof(Animator);

                method_Update = realType.GetMethod("Update",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
        }

        public static void UpdateWrapper(this Animator animator, float diff)
        {
            InitType();

            method_Update.Invoke(animator, new object[] {diff});
        }
    }

    public static class BlendTreeExtension
    {
        public static int GetRecursiveBlendParamCount(this BlendTree bt)
        {
            object val = bt.GetType()
                .GetProperty("recursiveBlendParameterCount",
                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public)
                .GetValue(bt, new object[] { });
            return (int) val;
        }

        public static string GetRecursiveBlendParam(this BlendTree bt, int index)
        {
            object val = bt.GetType()
                .GetMethod("GetRecursiveBlendParameter",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Invoke(bt, new object[] {index});
            return (string) val;
        }

        public static float GetInputBlendVal(this BlendTree bt, string blendValueName)
        {
            object val = bt.GetType()
                .GetMethod("GetInputBlendValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Invoke(bt, new object[] {blendValueName});
            return (float) val;
        }
    }

    public static class AnimatorControllerExtension
    {
        private static Type realType;
        private static MethodInfo method_GetEffectiveAnimatorController;
        private static FieldInfo field_OnAnimatorControllerDirty;
        private static PropertyInfo property_pushUndo;

        public static void InitType()
        {
            if (realType == null)
            {
                realType = typeof(AnimatorController);

                method_GetEffectiveAnimatorController = realType.GetMethod("GetEffectiveAnimatorController",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                field_OnAnimatorControllerDirty = realType.GetField("OnAnimatorControllerDirty",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                property_pushUndo = realType.GetProperty("pushUndo",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
        }

        public static void SetPushUndo(this AnimatorController controller, bool val)
        {
            InitType();
            property_pushUndo.SetValue(controller, val, null);
        }

        public static AnimatorController GetEffectiveAnimatorController(Animator animator)
        {
            InitType();
            object val =
                (AnimatorController) (method_GetEffectiveAnimatorController.Invoke(null, new object[] {animator}));
            return (AnimatorController) val;
        }

        public static void AppendOnAnimatorControllerDirtyCallback(this AnimatorController controller,
            System.Action callback)
        {
            InitType();
            System.Action oldCallback = (System.Action) field_OnAnimatorControllerDirty.GetValue(controller);
            System.Action newCallback = (System.Action) Delegate.Combine(oldCallback, new System.Action(callback));

            field_OnAnimatorControllerDirty.SetValue(controller, newCallback);
        }

        public static void RemoveOnAnimatorControllerDirtyCallback(this AnimatorController controller,
            System.Action callback)
        {
            InitType();
            System.Action oldCallback = (System.Action) field_OnAnimatorControllerDirty.GetValue(controller);
            System.Action newCallback = (System.Action) Delegate.Remove(oldCallback, new System.Action(callback));

            field_OnAnimatorControllerDirty.SetValue(controller, newCallback);
        }
    }

    public static class AnimatorStateExtension
    {
        private static Type realType;
        private static PropertyInfo property_pushUndo;

        public static void InitType()
        {
            if (realType == null)
                realType = typeof(AnimatorState);

            property_pushUndo = realType.GetProperty("pushUndo",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public static void SetPushUndo(this AnimatorState controller, bool val)
        {
            InitType();
            property_pushUndo.SetValue(controller, val, null);
        }
    }

    public static class AnimatorStateMachineExtension
    {
        private static Type realType;
        private static PropertyInfo property_pushUndo;

        public static void InitType()
        {
            if (realType == null)
                realType = typeof(AnimatorStateMachine);

            property_pushUndo = realType.GetProperty("pushUndo",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public static void SetPushUndo(this AnimatorStateMachine controller, bool val)
        {
            InitType();
            property_pushUndo.SetValue(controller, val, null);
        }
    }
}