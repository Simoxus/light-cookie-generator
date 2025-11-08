using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CookieGizmoDrawer
{
    private CookieGeneratorSettings settings;
    private List<Light> lightsToVisualize = new List<Light>();

    public CookieGizmoDrawer(CookieGeneratorSettings settings)
    {
        this.settings = settings;
    }

    public void SetLights(List<Light> lights)
    {
        lightsToVisualize = lights ?? new List<Light>();
    }

    public void DrawSceneGizmos()
    {
        if (!settings.showGizmos) return;

        // Draw gizmos for all the lights
        if (lightsToVisualize != null && lightsToVisualize.Count > 0)
        {
            for (int i = 0; i < lightsToVisualize.Count; i++)
            {
                Light light = lightsToVisualize[i];
                if (light == null || light.transform == null) continue;

                DrawLightGizmo(light, i);
            }
        }
    }

    private void DrawLightGizmo(Light light, int index)
    {
        Vector3 position = light.transform.position;
        Quaternion rotation = light.transform.rotation * Quaternion.Euler(settings.rotationOffset);

        Color baseColor = GetGizmoColor(light);

        DrawCameraFrustum(light, position, rotation, baseColor);
        DrawAxisGizmos(position, rotation, baseColor);
        DrawInfoLabel(light, position, rotation, baseColor, index);
    }

    private Color GetGizmoColor(Light light)
    {
        Color lightColor = light.color;
        float brightness = lightColor.r + lightColor.g + lightColor.b;
        return brightness > 0.5f ? lightColor : Color.yellow;
    }

    private void DrawCameraFrustum(Light light, Vector3 position, Quaternion rotation, Color baseColor)
    {
        float size = 10f;
        float distance = 10f;

        // For spot lights, show the shadow plane at the appropriate distance
        if (light.type == LightType.Spot)
        {
            float planeDistance = settings.useSpotlightRange ? light.range * 0.95f : settings.shadowPlaneDistance;
            DrawShadowPlane(position, rotation, planeDistance, baseColor);
        }

        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right * size;
        Vector3 up = rotation * Vector3.up * size;

        Vector3 nearTopLeft = position + up - right;
        Vector3 nearTopRight = position + up + right;
        Vector3 nearBottomLeft = position - up - right;
        Vector3 nearBottomRight = position - up + right;

        Vector3 farTopLeft = nearTopLeft + forward * distance;
        Vector3 farTopRight = nearTopRight + forward * distance;
        Vector3 farBottomLeft = nearBottomLeft + forward * distance;
        Vector3 farBottomRight = nearBottomRight + forward * distance;

        void DrawPlane(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p4);
            Handles.DrawLine(p4, p3);
            Handles.DrawLine(p3, p1);
        }

        // Near plane
        Handles.color = baseColor;
        DrawPlane(nearTopLeft, nearTopRight, nearBottomLeft, nearBottomRight);

        // Far plane
        Handles.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
        DrawPlane(farTopLeft, farTopRight, farBottomLeft, farBottomRight);

        // Connecting lines
        Handles.DrawLine(nearTopLeft, farTopLeft);
        Handles.DrawLine(nearTopRight, farTopRight);
        Handles.DrawLine(nearBottomLeft, farBottomLeft);
        Handles.DrawLine(nearBottomRight, farBottomRight);

        // Forward arrow
        Handles.color = Color.Lerp(baseColor, Color.cyan, 0.5f);
        Handles.ArrowHandleCap(0, position, rotation, size * 0.5f, EventType.Repaint);
    }

    private void DrawShadowPlane(Vector3 position, Quaternion rotation, float distance, Color baseColor)
    {
        Vector3 forward = rotation * Vector3.forward;
        Vector3 planeCenter = position + forward * distance;

        float planeSize = distance * 0.5f;
        Vector3 right = rotation * Vector3.right * planeSize;
        Vector3 up = rotation * Vector3.up * planeSize;

        // Draw plane outline
        Handles.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);
        Vector3 topLeft = planeCenter + up - right;
        Vector3 topRight = planeCenter + up + right;
        Vector3 bottomLeft = planeCenter - up - right;
        Vector3 bottomRight = planeCenter - up + right;

        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
        Handles.DrawLine(topLeft, bottomRight);
        Handles.DrawLine(topRight, bottomLeft);

        Handles.Label(planeCenter + up * 1.2f, $"Shadow Plane\n{distance:F1}m");
    }

    private void DrawAxisGizmos(Vector3 position, Quaternion rotation, Color baseColor)
    {
        float size = 1f;
        float labelOffset = 1.2f;

        void DrawAxis(Vector3 direction, Color color, string label)
        {
            Vector3 worldDirection = rotation * direction;
            Handles.color = Color.Lerp(color, baseColor, 0.3f);
            Handles.DrawLine(position, position + worldDirection * size);
            Handles.Label(position + worldDirection * labelOffset, label);
        }

        DrawAxis(Vector3.right, Color.red, "X");
        DrawAxis(Vector3.up, Color.green, "Y");
        DrawAxis(Vector3.forward, Color.blue, "Z");
    }

    private void DrawInfoLabel(Light light, Vector3 position, Quaternion rotation, Color baseColor, int index)
    {
        Vector3 up = rotation * Vector3.up;
        float size = 10f;

        Handles.color = baseColor;

        string lightName = light.gameObject.name;
        if (lightName.EndsWith(" Light"))
        {
            lightName = lightName.Substring(0, lightName.Length - 6);
        }

        string label =
            $"Light [{index + 1}] - {lightName}\n" +
            $"Type: {light.type}\n" +
            $"Rotation Offset: {settings.rotationOffset}";

        if (light.type == LightType.Spot)
        {
            label += $"\nRange: {light.range:F1}m\nAngle: {light.spotAngle:F1}�";
        }

        Handles.Label(position + up * (size * 1.2f), label);
    }
}