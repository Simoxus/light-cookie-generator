using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CookieSupportChecker
{
    static RenderPipelineAsset rpAsset = GraphicsSettings.defaultRenderPipeline;

    public static bool IsScriptablePipeline()
    {
        return rpAsset != null;
    }

    public static bool SupportsLightCookies()
    {
        if (rpAsset == null) return true;

        // Check if URP
        if (rpAsset is UniversalRenderPipelineAsset urpAsset)
        {
            return urpAsset.supportsLightCookies;
        }

        // Check if HDRP (kinda hacky, but it works)
        string typeName = rpAsset.GetType().FullName;
        if (typeName != null && typeName.Contains("HDRenderPipelineAsset"))
        {
            return true;
        }

        return false;
    }
}