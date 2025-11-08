using UnityEngine;

[System.Serializable]
public class CookieTextureMetadata
{
    public bool useSpotlightRange;
    public float shadowPlaneDistance;
    public float shadowOpacity;
    public float cookieBrightness;
    public float shadowSampleRadius;
    public int shadowSamples;
    public int blurMethod;
    public int blurRadius;
    public int blurIterations;
    public Vector3 rotationOffset;
    public string baseName;
    public int resolution;

    public static CookieTextureMetadata FromSettings(CookieGeneratorSettings settings)
    {
        return new CookieTextureMetadata
        {
            baseName = settings.baseName,
            resolution = settings.resolution,
            useSpotlightRange = settings.useSpotlightRange,
            shadowPlaneDistance = settings.shadowPlaneDistance,
            shadowOpacity = settings.shadowOpacity,
            cookieBrightness = settings.cookieBrightness,
            shadowSampleRadius = settings.shadowSampleRadius,
            shadowSamples = settings.shadowSamples,
            blurMethod = (int)settings.blurMethod,
            blurRadius = settings.blurRadius,
            blurIterations = settings.blurIterations,
            rotationOffset = settings.rotationOffset,
        };
    }

    public void ApplyToSettings(CookieGeneratorSettings settings)
    {
        settings.baseName = baseName;
        settings.resolution = resolution;
        settings.useSpotlightRange = useSpotlightRange;
        settings.shadowPlaneDistance = shadowPlaneDistance;
        settings.shadowOpacity = shadowOpacity;
        settings.cookieBrightness = cookieBrightness;
        settings.shadowSampleRadius = shadowSampleRadius;
        settings.shadowSamples = shadowSamples;
        settings.blurMethod = (CookieTextureSmoother.SmoothingMethod)blurMethod;
        settings.blurRadius = blurRadius;
        settings.blurIterations = blurIterations;
        settings.rotationOffset = rotationOffset;
    }

    public static CookieTextureMetadata LoadFromTexture(Texture2D texture)
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(assetPath)) return null;

        UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
        if (importer == null || string.IsNullOrEmpty(importer.userData)) return null;

        try
        {
            return JsonUtility.FromJson<CookieTextureMetadata>(importer.userData);
        }
        catch
        {
            return null;
        }
    }

    public string GetSummary()
    {
        return $"Base Name: {baseName}\n" +
               $"Resolution: {resolution}\n" +
               $"Use Spotlight Range: {useSpotlightRange}\n" +
               $"Shadow Plane Distance: {shadowPlaneDistance}\n" +
               $"Shadow Opacity: {shadowOpacity}\n" +
               $"Cookie Brightness: {cookieBrightness}\n" +
               $"Shadow Sample Radius: {shadowSampleRadius}\n" +
               $"Shadow Samples: {shadowSamples}\n" +
               $"Blur Method: {(CookieTextureSmoother.SmoothingMethod)blurMethod}\n" +
               $"Blur Radius: {blurRadius}\n" +
               $"Blur Iterations: {blurIterations}\n" +
               $"Rotation Offset: {rotationOffset}";
    }
}