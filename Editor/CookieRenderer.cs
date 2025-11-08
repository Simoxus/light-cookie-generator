using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CookieRenderer
{
    private CookieGeneratorSettings settings;

    public CookieRenderer(CookieGeneratorSettings settings)
    {
        this.settings = settings;
    }

    public Texture2D RenderCookie(int renderResolution)
    {
        Texture2D texture = RenderCookieWithShadows(renderResolution);

        // Apply blur/smoothing if needed
        if (settings.blurMethod != CookieTextureSmoother.SmoothingMethod.None)
        {
            Texture2D smoothed = CookieTextureSmoother.SmoothCookie(
                texture,
                settings.blurMethod,
                settings.blurRadius,
                settings.blurIterations
            );

            Object.DestroyImmediate(texture);
            texture = smoothed;
        }

        return texture;
    }

    private Texture2D RenderCookieWithShadows(int renderResolution)
    {
        // Store original light state
        Light lightToUse = settings.referenceLight;
        bool hadCookie = false;
        Texture originalCookie = null;
        float originalIntensity = 1f;
        LightShadows originalShadows = LightShadows.None;
        bool originalLightEnabled = true;

        if (lightToUse != null)
        {
            hadCookie = lightToUse.cookie != null;
            originalCookie = lightToUse.cookie;
            originalIntensity = lightToUse.intensity;
            originalShadows = lightToUse.shadows;
            originalLightEnabled = lightToUse.enabled;

            lightToUse.cookie = null;
            lightToUse.shadows = LightShadows.Soft;
            lightToUse.enabled = true;
        }

        GameObject tempCameraObj = new GameObject("TempCookieCamera");
        Camera tempCamera = tempCameraObj.AddComponent<Camera>();

        tempCameraObj.transform.position = settings.cameraTransform.position;
        tempCameraObj.transform.rotation = settings.cameraTransform.rotation * Quaternion.Euler(settings.rotationOffset);

        tempCamera.orthographic = true;
        tempCamera.orthographicSize = settings.orthographicSize;
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = Color.black;
        tempCamera.nearClipPlane = 0.01f;
        tempCamera.farClipPlane = 100f;
        tempCamera.aspect = 1f;
        tempCamera.cullingMask = ~0; // Render everything

        // Calculate plane position
        float planeDistance = settings.useSpotlightRange && lightToUse != null && lightToUse.type == LightType.Spot
            ? lightToUse.range
            : settings.shadowPlaneDistance;

        Material planeMaterial;
        GameObject planeObj = CreateShadowReceiver(tempCameraObj.transform, planeDistance, out planeMaterial);

        // Configure shadow casting
        var originalShadowModes = ConfigureSceneShadows(planeObj);

        // Force lighting update
        DynamicGI.UpdateEnvironment();

        // Render
        RenderTexture renderTexture = new RenderTexture(renderResolution, renderResolution, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        tempCamera.targetTexture = renderTexture;
        tempCamera.Render();

        Texture2D texture = CaptureRenderTexture(renderTexture, renderResolution);
        InvertAndApplyIntensity(texture);

        RestoreShadowModes(originalShadowModes);

        if (lightToUse != null)
        {
            lightToUse.cookie = originalCookie;
            lightToUse.intensity = originalIntensity;
            lightToUse.shadows = originalShadows;
            lightToUse.enabled = originalLightEnabled;
        }

        RenderTexture.active = null;
        renderTexture.Release();
        Object.DestroyImmediate(tempCameraObj);
        Object.DestroyImmediate(planeObj);
        Object.DestroyImmediate(planeMaterial);

        return texture;
    }

    private GameObject CreateShadowReceiver(Transform cameraTransform, float distance, out Material planeMaterial)
    {
        GameObject planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeObj.name = "TempShadowPlane";

        Vector3 forward = cameraTransform.forward;
        planeObj.transform.position = cameraTransform.position + forward * distance;
        planeObj.transform.rotation = cameraTransform.rotation;

        float planeScale = settings.orthographicSize * 0.2f;
        planeObj.transform.localScale = new Vector3(planeScale, 1f, planeScale);

        MeshRenderer planeRenderer = planeObj.GetComponent<MeshRenderer>();

        Material planeMat = new Material(Shader.Find("Unlit/Color"));
        planeMat.color = Color.white;
        planeRenderer.sharedMaterial = planeMat;
        planeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        planeRenderer.receiveShadows = true;

        // Ensure the plane is rendered
        planeRenderer.enabled = true;

        planeMaterial = planeMat;
        return planeObj;
    }

    private Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode> ConfigureSceneShadows(GameObject planeObj)
    {
        Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode> originalShadowModes =
            new Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode>();

        MeshRenderer planeRenderer = planeObj.GetComponent<MeshRenderer>();
        Renderer[] allRenderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == planeRenderer) continue;

            originalShadowModes[renderer] = renderer.shadowCastingMode;

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        return originalShadowModes;
    }

    private Texture2D CaptureRenderTexture(RenderTexture renderTexture, int resolution)
    {
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        texture.Apply();
        return texture;
    }

    private void InvertAndApplyIntensity(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            float luminance = pixels[i].grayscale;

            if (settings.shadowOpacity < 1f)
            {
                // Identify shadow regions (darker areas)
                float shadowMask = 1f - luminance;

                float opacityAdjustment = shadowMask * (1f - settings.shadowOpacity);
                luminance = Mathf.Lerp(luminance, 1f, opacityAdjustment);
            }

            // Entire image brightness
            if (settings.cookieBrightness > 0f)
            {
                luminance = Mathf.Lerp(luminance, 1f, settings.cookieBrightness);
            }

            pixels[i] = new Color(luminance, luminance, luminance, 1f);
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }

    private void RestoreShadowModes(Dictionary<Renderer, UnityEngine.Rendering.ShadowCastingMode> originalShadowModes)
    {
        foreach (var kvp in originalShadowModes)
        {
            if (kvp.Key != null)
            {
                kvp.Key.shadowCastingMode = kvp.Value;
            }
        }
    }
}