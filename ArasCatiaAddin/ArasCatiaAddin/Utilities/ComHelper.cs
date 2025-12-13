using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ArasCatiaAddin.Utilities
{
    /// <summary>
    /// Helper class for COM interop operations.
    /// </summary>
    public static class ComHelper
    {
        /// <summary>
        /// Get a property from a COM object using reflection.
        /// </summary>
        public static object GetProperty(object comObject, string propertyName)
        {
            if (comObject == null) return null;

            return comObject.GetType().InvokeMember(propertyName,
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, null);
        }

        /// <summary>
        /// Set a property on a COM object using reflection.
        /// </summary>
        public static void SetProperty(object comObject, string propertyName, object value)
        {
            if (comObject == null) return;

            comObject.GetType().InvokeMember(propertyName,
                BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, new object[] { value });
        }

        /// <summary>
        /// Call a method on a COM object using reflection.
        /// </summary>
        public static object CallMethod(object comObject, string methodName, params object[] args)
        {
            if (comObject == null) return null;

            return comObject.GetType().InvokeMember(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                null, comObject, args);
        }

        /// <summary>
        /// Safely release a COM object.
        /// </summary>
        public static void ReleaseComObject(object comObject)
        {
            if (comObject != null)
            {
                try
                {
                    Marshal.ReleaseComObject(comObject);
                }
                catch { }
            }
        }

        /// <summary>
        /// Get a property value as string, with default.
        /// </summary>
        public static string GetPropertyString(object comObject, string propertyName, string defaultValue = "")
        {
            try
            {
                object value = GetProperty(comObject, propertyName);
                return value?.ToString() ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a property value as int, with default.
        /// </summary>
        public static int GetPropertyInt(object comObject, string propertyName, int defaultValue = 0)
        {
            try
            {
                object value = GetProperty(comObject, propertyName);
                return Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a property value as bool, with default.
        /// </summary>
        public static bool GetPropertyBool(object comObject, string propertyName, bool defaultValue = false)
        {
            try
            {
                object value = GetProperty(comObject, propertyName);
                return Convert.ToBoolean(value);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
