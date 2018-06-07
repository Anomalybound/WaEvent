using System;
using System.Reflection;

namespace WaEvent.Editor.Util
{
    public class TimeControlWrapper
    {
        private static Type realType;
        private object instance;

        private static FieldInfo field_currentTime;
        private static FieldInfo field_loop;
        private static FieldInfo field_startTime;
        private static FieldInfo field_stopTime;
        private static MethodInfo method_Update;
        private static PropertyInfo property_deltaTime;
        private static PropertyInfo property_normalizedTime;
        private static PropertyInfo property_playing;
        private static PropertyInfo property_nextCurrentTime;

        public static void InitType()
        {
            if (realType == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                realType = assembly.GetType("UnityEditor.TimeControl");

                field_currentTime = realType.GetField("currentTime");
                field_loop = realType.GetField("loop");
                field_startTime = realType.GetField("startTime");
                field_stopTime = realType.GetField("stopTime");
                method_Update = realType.GetMethod("Update");
                property_deltaTime = realType.GetProperty("deltaTime");
                property_normalizedTime = realType.GetProperty("normalizedTime");
                property_playing = realType.GetProperty("playing");
                property_nextCurrentTime = realType.GetProperty("nextCurrentTime");
            }
        }

        public TimeControlWrapper(object realTimeControl)
        {
            InitType();
            this.instance = realTimeControl;
        }

        public float currentTime
        {
            get { return (float) field_currentTime.GetValue(instance); }
            set { field_currentTime.SetValue(instance, value); }
        }

        public bool loop
        {
            get { return (bool) field_loop.GetValue(instance); }
            set { field_loop.SetValue(instance, value); }
        }

        public float startTime
        {
            get { return (float) field_startTime.GetValue(instance); }
            set { field_startTime.SetValue(instance, value); }
        }

        public float stopTime
        {
            get { return (float) field_stopTime.GetValue(instance); }
            set { field_stopTime.SetValue(instance, value); }
        }

        public float deltaTime
        {
            get { return (float) property_deltaTime.GetValue(instance, null); }
            set { property_deltaTime.SetValue(instance, value, null); }
        }

        public float normalizedTime
        {
            get { return (float) property_normalizedTime.GetValue(instance, null); }
            set { property_normalizedTime.SetValue(instance, value, null); }
        }

        public bool playing
        {
            get { return (bool) property_playing.GetValue(instance, null); }
            set { property_playing.SetValue(instance, value, null); }
        }

        public float nextCurrentTime
        {
            set { property_nextCurrentTime.SetValue(instance, value, null); }
        }

        public void Update()
        {
            method_Update.Invoke(instance, null);
        }
    }
}