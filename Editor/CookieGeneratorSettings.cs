using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

public class CookieGeneratorSettings : ScriptableObject
{
    private const string PREFS_PREFIX = "CookieGenerator_";

    public bool useSpotlightRange = false;
    public float shadowPlaneDistance = 10f;
    public float shadowOpacity = 1f;
    public float cookieBrightness = 0f;

    public float shadowSampleRadius = 0.2f;
    public int shadowSamples = 12;
    public CookieTextureSmoother.SmoothingMethod blurMethod = CookieTextureSmoother.SmoothingMethod.GaussianBlur;
    public int blurRadius = 3;
    public int blurIterations = 1;

    public Vector3 rotationOffset = Vector3.zero;
    public float orthographicSize = 10f;

    public List<GameObject> meshObjects = new List<GameObject>();
    public List<GameObject> blacklistedObjects = new List<GameObject>();

    public float cookieIntensity = 0f;

    public Transform cameraTransform;
    public Light referenceLight;
    public bool showGizmos = false;
    public bool showPreview = true;

    public string baseName = "cookiesyum";
    public int resolution = 512;
    public string savePath = "Assets/Art/Cookies";

    public bool CanGenerate()
    {
        return cameraTransform != null;
    }

    public void Reset()
    {
        // Render Settings
        useSpotlightRange = false;
        shadowPlaneDistance = 10f;
        shadowOpacity = 1f;
        cookieBrightness = 0f;

        // Quality Settings
        shadowSampleRadius = 0.2f;
        shadowSamples = 12;
        blurMethod = CookieTextureSmoother.SmoothingMethod.GaussianBlur;
        blurRadius = 3;
        blurIterations = 1;

        // Camera Settings
        rotationOffset = Vector3.zero;
        orthographicSize = 10f;

        // Shadow Casters
        meshObjects.Clear();
        blacklistedObjects.Clear();

        // Rendering Mode
        cookieIntensity = 0f;

        // Clear references
        cameraTransform = null;
        referenceLight = null;

        // UI Settings
        showGizmos = false;
        showPreview = true;

        // Output Settings
        baseName = "cookiesyum";
        resolution = 512;
        savePath = "Assets/Art/Cookies";
    }

    public void SaveToEditorPrefs()
    {
        foreach (var field in GetSerializableFields())
        {
            string key = PREFS_PREFIX + field.Name;
            object value = field.GetValue(this);

            // Skip Unity Object references and Lists
            if (value is UnityEngine.Object && !(value is string))
                continue;
            if (value is System.Collections.IList)
                continue;

            SaveFieldValue(key, value, field.FieldType);
        }
    }

    public void LoadFromEditorPrefs()
    {
        foreach (var field in GetSerializableFields())
        {
            string key = PREFS_PREFIX + field.Name;

            // Skip Unity Object references and Lists
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
                continue;
            if (typeof(System.Collections.IList).IsAssignableFrom(field.FieldType))
                continue;

            object defaultValue = field.GetValue(this);
            object loadedValue = LoadFieldValue(key, field.FieldType, defaultValue);
            field.SetValue(this, loadedValue);
        }
    }

    private FieldInfo[] GetSerializableFields()
    {
        return GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    private void SaveFieldValue(string key, object value, Type fieldType)
    {
        if (value == null) return;

        if (fieldType == typeof(int))
            EditorPrefs.SetInt(key, (int)value);
        else if (fieldType == typeof(float))
            EditorPrefs.SetFloat(key, (float)value);
        else if (fieldType == typeof(bool))
            EditorPrefs.SetBool(key, (bool)value);
        else if (fieldType == typeof(string))
            EditorPrefs.SetString(key, (string)value);
        else if (fieldType.IsEnum)
            EditorPrefs.SetInt(key, (int)value);
        else if (fieldType == typeof(Vector3) || fieldType == typeof(Vector2) || fieldType == typeof(Color))
            EditorPrefs.SetString(key, JsonUtility.ToJson(value));
    }

    private object LoadFieldValue(string key, Type fieldType, object defaultValue)
    {
        if (fieldType == typeof(int))
            return EditorPrefs.GetInt(key, defaultValue != null ? (int)defaultValue : 0);
        else if (fieldType == typeof(float))
            return EditorPrefs.GetFloat(key, defaultValue != null ? (float)defaultValue : 0f);
        else if (fieldType == typeof(bool))
            return EditorPrefs.GetBool(key, defaultValue != null ? (bool)defaultValue : false);
        else if (fieldType == typeof(string))
            return EditorPrefs.GetString(key, defaultValue != null ? (string)defaultValue : "");
        else if (fieldType.IsEnum)
        {
            int enumValue = EditorPrefs.GetInt(key, defaultValue != null ? (int)defaultValue : 0);
            return Enum.ToObject(fieldType, enumValue);
        }
        else if (fieldType == typeof(Vector3))
        {
            string json = EditorPrefs.GetString(key, "");
            return string.IsNullOrEmpty(json) ? (defaultValue ?? Vector3.zero) : JsonUtility.FromJson<Vector3>(json);
        }
        else if (fieldType == typeof(Vector2))
        {
            string json = EditorPrefs.GetString(key, "");
            return string.IsNullOrEmpty(json) ? (defaultValue ?? Vector2.zero) : JsonUtility.FromJson<Vector2>(json);
        }
        else if (fieldType == typeof(Color))
        {
            string json = EditorPrefs.GetString(key, "");
            return string.IsNullOrEmpty(json) ? (defaultValue ?? Color.white) : JsonUtility.FromJson<Color>(json);
        }

        return defaultValue;
    }
}