using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CookieGeneratorWindow
{
    private Vector2 scrollPosition;
    private SerializedObject serializedSettings;
    private List<Light> lights = new List<Light>();
    private bool showPreviews = false;

    public void Initialize(SerializedObject serializedSettings)
    {
        this.serializedSettings = serializedSettings;
    }

    public void BeginDraw()
    {
        if (serializedSettings != null)
        {
            serializedSettings.Update();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
    }

    public void EndDraw()
    {
        EditorGUILayout.EndScrollView();

        if (serializedSettings != null)
        {
            serializedSettings.ApplyModifiedProperties();
        }
    }

    public void DrawHeader()
    {
        GUILayout.Label("Light Cookie Generator", EditorStyles.whiteLargeLabel);
    }

    public void DrawVisibilityError()
    {
        bool supportsLightCookies = CookieSupportChecker.SupportsLightCookies();
        if (!supportsLightCookies)
        {
            EditorGUILayout.HelpBox(
                "Enable \"Light Cookies\" in the RP asset in order to see light cookies.",
                MessageType.Error
            );
        }
    }

    public void DrawValidationWarning()
    {
        if (lights.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Add at least one Light in order to generate light cookies.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();
    }

    public void DrawLoadSettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Load Settings from Cookie", EditorStyles.boldLabel);

        Texture2D cookieToLoad = (Texture2D)EditorGUILayout.ObjectField(
            "Cookie Texture",
            null,
            typeof(Texture2D),
            false
        );

        if (cookieToLoad != null)
        {
            CookieTextureMetadata metadata = CookieTextureMetadata.LoadFromTexture(cookieToLoad);

            if (metadata == null)
            {
                EditorUtility.DisplayDialog("No Metadata Found",
                    "This texture doesn't contain generation settings.",
                    "OK"
                );
                return;
            }

            if (EditorUtility.DisplayDialog("Load Settings",
                "Load these settings?\n\n" + metadata.GetSummary(),
                "Load", "Cancel"))
            {
                metadata.ApplyToSettings(settings);
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawLightsList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label($"Lights ({lights.Count})", EditorStyles.boldLabel);

        // Remove null entries
        for (int i = lights.Count - 1; i >= 0; i--)
        {
            if (lights[i] == null)
            {
                lights.RemoveAt(i);
            }
        }

        // Draw existing lights
        for (int i = lights.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(lights[i], typeof(Light), true);
            EditorGUI.EndDisabledGroup();

            GUILayout.Label($"{lights[i].type}", GUILayout.Width(80));

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                lights.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        // Add new light field
        Light newLight = EditorGUILayout.ObjectField("Add Light", null, typeof(Light), true) as Light;
        if (newLight != null && !lights.Contains(newLight))
        {
            if (newLight.type == LightType.Spot || newLight.type == LightType.Directional)
            {
                lights.Add(newLight);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Light Type",
                    "Only Spot and Directional lights are supported.",
                    "OK"
                );
            }
        }

        EditorGUILayout.Space();

        // Batch operations
        if (GUILayout.Button("Add Selected Lights"))
        {
            AddSelectedLights();
        }

        if (GUILayout.Button("Clear All Lights"))
        {
            lights.Clear();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawRenderingSettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Render Settings", EditorStyles.boldLabel);

        settings.useSpotlightRange = EditorGUILayout.Toggle("Use Spotlight Range", settings.useSpotlightRange);
        if (!settings.useSpotlightRange)
        {
            settings.shadowPlaneDistance = EditorGUILayout.FloatField("Shadow Plane Distance", settings.shadowPlaneDistance);
        }

        settings.shadowOpacity = EditorGUILayout.Slider("Shadow Opacity", settings.shadowOpacity, 0f, 1f);
        EditorGUILayout.HelpBox(
            "Controls how dark/visible shadows are.\n0 = No visible shadows, 1 = Literally only shadows",
            MessageType.None
        );

        settings.cookieBrightness = EditorGUILayout.Slider("Overall Brightness", settings.cookieBrightness, 0f, 1f);
        EditorGUILayout.HelpBox(
            "Lifts the entire image brightness.\n0 = Normal, 1 = Maximum (white)",
            MessageType.None
        );

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawShadowSettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Shadow Quality", EditorStyles.boldLabel);

        settings.shadowSampleRadius = EditorGUILayout.FloatField("Shadow Sample Radius", settings.shadowSampleRadius);
        settings.shadowSamples = EditorGUILayout.IntSlider("Shadow Samples", settings.shadowSamples, 1, 24);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawBlurSettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Blur Settings", EditorStyles.boldLabel);

        settings.blurMethod = (CookieTextureSmoother.SmoothingMethod)EditorGUILayout.EnumPopup("Blur Method", settings.blurMethod);
        settings.blurRadius = EditorGUILayout.IntSlider("Blur Radius", settings.blurRadius, 1, 10);
        settings.blurIterations = EditorGUILayout.IntSlider("Blur Iterations", settings.blurIterations, 1, 5);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawCameraSettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Camera Settings", EditorStyles.boldLabel);

        SerializedProperty rotationOffsetProp = serializedSettings.FindProperty("rotationOffset");

        EditorGUILayout.PropertyField(rotationOffsetProp, new GUIContent("Rotation Offset"));

        if (GUILayout.Button("Reset Rotation"))
        {
            CookieGeneratorSettings s = serializedSettings.targetObject as CookieGeneratorSettings;
            if (s != null)
            {
                s.rotationOffset = Vector3.zero;
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawUISettings(CookieGeneratorSettings settings)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("UI Settings", EditorStyles.boldLabel);

        SerializedProperty showGizmosProp = serializedSettings.FindProperty("showGizmos");
        EditorGUILayout.PropertyField(showGizmosProp, new GUIContent("Show Gizmos"));

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawOutputSettings(CookieGeneratorSettings settings, CookieTextureSaver saver)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Output Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        settings.savePath = EditorGUILayout.TextField("Save Folder", settings.savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(86)))
        {
            string selectedPath = saver.SelectSavePath();
            if (!string.IsNullOrEmpty(selectedPath))
            {
                settings.savePath = selectedPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        settings.baseName = EditorGUILayout.TextField("Base Name", settings.baseName);
        if (GUILayout.Button("Selected", GUILayout.Width(86)))
        {
            if (Selection.activeGameObject != null)
            {
                settings.baseName = Selection.activeGameObject.name;
            }
        }
        EditorGUILayout.EndHorizontal();

        settings.resolution = EditorGUILayout.IntPopup("Resolution", settings.resolution,
            new string[] { "256", "512", "1024", "2048" },
            new int[] { 256, 512, 1024, 2048 });

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawPreview(Dictionary<Light, Texture2D> previews)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showPreviews = EditorGUILayout.Foldout(showPreviews, $"Preview ({previews.Count})", true);

        if (showPreviews && previews.Count > 0)
        {
            EditorGUILayout.Space();

            foreach (var kvp in previews)
            {
                if (kvp.Key == null || kvp.Value == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(kvp.Key.name, EditorStyles.boldLabel);
                GUILayout.Box(kvp.Value, GUILayout.Width(128), GUILayout.Height(128));
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    public void DrawGenerateButtons(CookieGeneratorSettings settings, System.Action onGeneratePreviews, System.Action onGenerateAll)
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = lights.Count > 0;

        if (GUILayout.Button($"Generate {lights.Count} Preview(s)", GUILayout.Height(40)))
        {
            onGeneratePreviews?.Invoke();
        }

        if (GUILayout.Button($"Generate {lights.Count} Cookie(s)", GUILayout.Height(40)))
        {
            onGenerateAll?.Invoke();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    public void DrawResetButton(CookieGeneratorSettings settings)
    {
        if (GUILayout.Button("Reset Settings to Default"))
        {
            if (EditorUtility.DisplayDialog("Reset Settings",
                "Reset all settings to default values?", "Yes", "Cancel"))
            {
                settings.Reset();
            }
        }
    }

    public List<Light> GetLights()
    {
        return lights;
    }

    public void GenerateAllCookies(CookieGeneratorSettings settings, CookieRenderer renderer, CookieTextureSaver saver)
    {
        if (lights.Count == 0) return;

        EditorUtility.DisplayProgressBar("Generating Cookies", "Preparing...", 0f);

        try
        {
            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                if (light == null) continue;

                float progress = (float)i / lights.Count;
                EditorUtility.DisplayProgressBar("Generating Cookies",
                    $"Processing {light.name} ({i + 1}/{lights.Count})",
                    progress
                );

                CookieTextureSaver.GenerateCookieForLight(light, settings, settings.savePath, settings.baseName, true);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
    }

    private void AddSelectedLights()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection",
                "Please select one or more GameObjects with Light components.",
                "OK"
            );
            return;
        }

        int added = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            Light light = go.GetComponent<Light>();
            if (light != null && !lights.Contains(light))
            {
                if (light.type == LightType.Spot || light.type == LightType.Directional)
                {
                    lights.Add(light);
                    added++;
                }
            }
        }

        if (added > 0)
        {
            Debug.Log($"Added {added} light(s) to generator.");
        }
        else
        {
            EditorUtility.DisplayDialog("No Lights Found",
                "No valid Spot or Directional lights found in selection.",
                "OK"
            );
        }
    }
}