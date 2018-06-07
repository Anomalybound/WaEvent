using System;
using System.Reflection;
using UnityEngine;

namespace WaEvent.Editor.Util
{
    public static class HandleUtilityWrapper
    {
        private static Type realType;
        private static PropertyInfo s_property_handleWireMaterial;

        private static void InitType()
        {
            if (realType == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                realType = assembly.GetType("UnityEditor.HandleUtility");

                s_property_handleWireMaterial = realType.GetProperty("handleWireMaterial",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
        }

        public static Material handleWireMaterial
        {
            get
            {
                InitType();
                return s_property_handleWireMaterial.GetValue(null, null) as Material;
            }
        }
    }
}