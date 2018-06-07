using UnityEngine;
using System;
using System.Reflection;

namespace WaEvent.Editor.Util
{
    public class AvatarPreviewWrapper
    {
        #region Reflection

        private static Type realType;

        private static ConstructorInfo method_ctor;
        private static PropertyInfo property_OnAvatarChangeFunc;
        private static PropertyInfo property_IKOnFeet;
        private static PropertyInfo property_Animator;
        private static PropertyInfo property_PreviewObject;
        private static MethodInfo method_DoPreviewSettings;
        private static MethodInfo method_OnDestroy;
        private static MethodInfo method_DoAvatarPreview;
        private static MethodInfo method_ResetPreviewInstance;

        private static FieldInfo field_timeControl;

        public static void InitType()
        {
            if (realType == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                realType = assembly.GetType("UnityEditor.AvatarPreview");

                method_ctor = realType.GetConstructor(new[] {typeof(Animator), typeof(Motion)});
                property_OnAvatarChangeFunc = realType.GetProperty("OnAvatarChangeFunc");
                property_IKOnFeet = realType.GetProperty("IKOnFeet");
                property_Animator = realType.GetProperty("Animator");
                property_PreviewObject = realType.GetProperty("PreviewObject");
                method_DoPreviewSettings = realType.GetMethod("DoPreviewSettings");
                method_OnDestroy = realType.GetMethod("OnDestroy");
                method_DoAvatarPreview =
                    realType.GetMethod("DoAvatarPreview", new[] {typeof(Rect), typeof(GUIStyle)});
                method_ResetPreviewInstance = realType.GetMethod("ResetPreviewInstance");
//			method_CalculatePreviewGameObject = realType.GetMethod("CalculatePreviewGameObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                field_timeControl = realType.GetField("timeControl");
            }
        }

        #endregion

        #region Wrapper

        private object instance;

        public delegate void OnAvatarChange();

        public AvatarPreviewWrapper(Animator previewObjectInScene, Motion objectOnSameAsset)
        {
            InitType();

            instance = method_ctor.Invoke(new object[] {previewObjectInScene, objectOnSameAsset});
        }

        public Animator Animator
        {
            get { return property_Animator.GetValue(instance, null) as Animator; }
        }

        public bool IKOnFeet
        {
            get { return (bool) property_IKOnFeet.GetValue(instance, null); }
        }

        public OnAvatarChange OnAvatarChangeFunc
        {
            set
            {
                property_OnAvatarChangeFunc.SetValue(instance,
                    Delegate.CreateDelegate(property_OnAvatarChangeFunc.PropertyType, value.Target, value.Method),
                    null);
            }
        }

        public GameObject PreviewObject
        {
            get { return property_PreviewObject.GetValue(instance, null) as GameObject; }
        }

        public void DoPreviewSettings()
        {
            method_DoPreviewSettings.Invoke(instance, null);
        }

        public void OnDestroy()
        {
            method_OnDestroy.Invoke(instance, null);
        }

        public void DoAvatarPreview(Rect rect, GUIStyle background)
        {
            method_DoAvatarPreview.Invoke(instance, new object[] {rect, background});
        }

        public void ResetPreviewInstance()
        {
            method_ResetPreviewInstance.Invoke(instance, null);
        }

        public TimeControlWrapper timeControl
        {
            get { return new TimeControlWrapper(field_timeControl.GetValue(instance)); }
        }

        #endregion
    }
}