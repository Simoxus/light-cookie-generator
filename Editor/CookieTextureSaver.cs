using UnityEngine;
using UnityEditor;
using System.IO;

public class CookieTextureSaver
{
    private CookieGeneratorSettings settings;

    public CookieTextureSaver(CookieGeneratorSettings settings)
    {
        this.settings = settings;
    }

    public bool SaveCookie(Texture2D texture, string customFileName = null, bool silent = false)
    {
        string originalFileName = settings.baseName;
        if (!string.IsNullOrEmpty(customFileName))
        {
            settings.baseName = customFileName;
        }

        if (!EnsureFolderExists())
        {
            settings.baseName = originalFileName;
            return false;
        }

        string fullPath = GetFullPath();

        if (!silent && File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog("File Exists",
                $"A file named '{settings.baseName}.png' already exists at this location.\n\nDo you want to overwrite it?",
                "Overwrite", "Cancel"))
            {
                settings.baseName = originalFileName;
                return false;
            }
        }

        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();
        ConfigureTextureImporter(fullPath);

        if (!silent)
        {
            ShowSuccessDialog(fullPath);
        }

        settings.baseName = originalFileName;
        return true;
    }

    public bool SaveCookieToPath(Texture2D texture, string savePath, string fileName, bool silent = false)
    {
        string originalPath = settings.savePath;
        string originalFileName = settings.baseName;

        settings.savePath = savePath;
        settings.baseName = fileName;

        bool result = SaveCookie(texture, null, silent);

        settings.savePath = originalPath;
        settings.baseName = originalFileName;

        return result;
    }

    private bool EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(settings.savePath))
        {
            string[] folders = settings.savePath.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
        return true;
    }

    private string GetFullPath()
    {
        return settings.savePath + "/" + settings.baseName + ".png";
    }

    private void ConfigureTextureImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Cookie;
            importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.maxTextureSize = settings.resolution;
            importer.npotScale = TextureImporterNPOTScale.None;

            CookieTextureMetadata metadata = new CookieTextureMetadata
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

            importer.userData = JsonUtility.ToJson(metadata);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

    private void ShowSuccessDialog(string path)
    {
        EditorUtility.DisplayDialog("Success",
            $"Cookie texture generated at:\n{path}",
            "OK"
        );

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    public string SelectSavePath()
    {
        string selectedPath = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");

        if (string.IsNullOrEmpty(selectedPath))
            return null;

        if (selectedPath.StartsWith(Application.dataPath))
        {
            return "Assets" + selectedPath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid Path",
                "Please select a folder inside your Assets folder.", "OK"
            );
            return null;
        }
    }

    public static bool GenerateCookieForLight(Light light, CookieGeneratorSettings sourceSettings, string savePath, string baseName, bool autoAssign)
    {
        if (light == null) return false;

        CookieGeneratorSettings tempSettings = ScriptableObject.CreateInstance<CookieGeneratorSettings>();

        tempSettings.cameraTransform = light.transform;
        tempSettings.referenceLight = light;

        // Copy settings
        tempSettings.baseName = baseName;
        tempSettings.resolution = sourceSettings.resolution;
        tempSettings.useSpotlightRange = sourceSettings.useSpotlightRange;
        tempSettings.shadowPlaneDistance = sourceSettings.shadowPlaneDistance;
        tempSettings.shadowOpacity = sourceSettings.shadowOpacity;
        tempSettings.cookieBrightness = sourceSettings.cookieBrightness;
        tempSettings.shadowSampleRadius = sourceSettings.shadowSampleRadius;
        tempSettings.shadowSamples = sourceSettings.shadowSamples;
        tempSettings.blurMethod = sourceSettings.blurMethod;
        tempSettings.blurRadius = sourceSettings.blurRadius;
        tempSettings.blurIterations = sourceSettings.blurIterations;
        tempSettings.rotationOffset = sourceSettings.rotationOffset;
        tempSettings.savePath = savePath;

        CookieRenderer cookieRenderer = new CookieRenderer(tempSettings);
        Texture2D texture = cookieRenderer.RenderCookie(sourceSettings.resolution);

        if (texture == null)
        {
            Object.DestroyImmediate(tempSettings);
            return false;
        }

        string fileName = $"{baseName}_{light.gameObject.name}";
        CookieTextureSaver cookieSaver = new CookieTextureSaver(tempSettings);
        bool saved = cookieSaver.SaveCookieToPath(texture, savePath, fileName, true);

        if (saved && autoAssign)
        {
            string assetPath = $"{savePath}/{fileName}.png";
            Texture2D cookieTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (cookieTexture != null)
            {
                light.cookie = cookieTexture;
                EditorUtility.SetDirty(light);
            }
        }

        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(tempSettings);

        return saved;
    }
}