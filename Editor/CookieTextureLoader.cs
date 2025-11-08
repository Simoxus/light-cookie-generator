using UnityEngine;
using UnityEditor;

public class CookieTextureLoader
{
    public void LoadAndApplySettings(CookieGeneratorSettings settings, Texture2D cookieTexture)
    {
        if (cookieTexture == null) return;

        CookieTextureMetadata metadata = CookieTextureMetadata.LoadFromTexture(cookieTexture);

        if (metadata == null)
        {
            EditorUtility.DisplayDialog("No Metadata Found",
                "This texture doesn't contain generation settings.",
                "OK"
            );
            return;
        }

        string settingsSummary = GetSettingsSummary(metadata);

        if (EditorUtility.DisplayDialog("Load Settings",
            "Load these settings?\n\n" + settingsSummary,
            "Load", "Cancel"))
        {
            ApplyMetadataToSettings(settings, metadata);
        }
    }

    private string GetSettingsSummary(CookieTextureMetadata metadata)
    {
        return $"Base Name: {metadata.baseName}\n" +
               $"Resolution: {metadata.resolution}\n" +
               $"Use Spotlight Range: {metadata.useSpotlightRange}\n" +
               $"Shadow Plane Distance: {metadata.shadowPlaneDistance}\n" +
               $"Shadow Opacity: {metadata.shadowOpacity}\n" +
               $"Cookie Brightness: {metadata.cookieBrightness}\n" +
               $"Shadow Sample Radius: {metadata.shadowSampleRadius}\n" +
               $"Shadow Samples: {metadata.shadowSamples}\n" +
               $"Blur Method: {(CookieTextureSmoother.SmoothingMethod)metadata.blurMethod}\n" +
               $"Blur Radius: {metadata.blurRadius}\n" +
               $"Blur Iterations: {metadata.blurIterations}\n" +
               $"Rotation Offset: {metadata.rotationOffset}";
    }

    private static void ApplyMetadataToSettings(CookieGeneratorSettings settings, CookieTextureMetadata metadata)
    {
        settings.baseName = metadata.baseName;
        settings.resolution = metadata.resolution;
        settings.useSpotlightRange = metadata.useSpotlightRange;
        settings.shadowPlaneDistance = metadata.shadowPlaneDistance;
        settings.shadowOpacity = metadata.shadowOpacity;
        settings.cookieBrightness = metadata.cookieBrightness;
        settings.shadowSampleRadius = metadata.shadowSampleRadius;
        settings.shadowSamples = metadata.shadowSamples;
        settings.blurMethod = (CookieTextureSmoother.SmoothingMethod)metadata.blurMethod;
        settings.blurRadius = metadata.blurRadius;
        settings.blurIterations = metadata.blurIterations;
        settings.rotationOffset = metadata.rotationOffset;
    }
}