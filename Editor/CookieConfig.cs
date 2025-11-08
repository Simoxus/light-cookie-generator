using UnityEngine;

[System.Serializable]
public static class CookieConfig
{
    public const string PREFS_PREFIX = "CookieGenerator_";

    // Output Settings
    public const string DEFAULT_BASE_NAME = "cookiesyum";
    public const int DEFAULT_RESOLUTION = 512;
    public const string DEFAULT_SAVE_PATH = "Assets/Art/Cookies";

    // Render Settings
    public const bool DEFAULT_USE_SPOTLIGHT_RANGE = false;
    public const float DEFAULT_SHADOW_PLANE_DISTANCE = 10f;
    public const float DEFAULT_SHADOW_OPACITY = 1f;
    public const float DEFAULT_COOKIE_BRIGHTNESS = 0f;

    // Quality Settings
    public const float DEFAULT_SHADOW_SAMPLE_RADIUS = 0.2f;
    public const int DEFAULT_SHADOW_SAMPLES = 12;
    public const CookieTextureSmoother.SmoothingMethod DEFAULT_BLUR_METHOD = CookieTextureSmoother.SmoothingMethod.None;
    public const int DEFAULT_BLUR_RADIUS = 3;
    public const int DEFAULT_BLUR_ITERATIONS = 1;

    // Camera Settings
    public static readonly Vector3 DEFAULT_ROTATION_OFFSET = Vector3.zero;
    public const float DEFAULT_ORTHOGRAPHIC_SIZE = 10f;

    // UI Settings
    public const bool DEFAULT_SHOW_GIZMOS = false;
    public const bool DEFAULT_SHOW_PREVIEW = true;
}