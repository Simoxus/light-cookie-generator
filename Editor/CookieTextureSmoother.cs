// I'm sorry to the GitHub repositories I yoinked these formulas from

using UnityEngine;

public class CookieTextureSmoother
{
    public enum SmoothingMethod
    {
        None,
        GaussianBlur,
        BoxBlur,
        MedianFilter,
        PenumbraBlur
    }

    public static Texture2D SmoothCookie(Texture2D source, SmoothingMethod method = SmoothingMethod.GaussianBlur, int radius = 2, int iterations = 1)
    {
        if (source == null) return null;

        Texture2D result = DuplicateTexture(source);

        for (int i = 0; i < iterations; i++)
        {
            Texture2D tempTexture = result;

            switch (method)
            {
                case SmoothingMethod.None:
                    break;
                case SmoothingMethod.GaussianBlur:
                    result = ApplyGaussianBlur(tempTexture, radius);
                    break;
                case SmoothingMethod.BoxBlur:
                    result = ApplyBoxBlur(tempTexture, radius);
                    break;
                case SmoothingMethod.MedianFilter:
                    result = ApplyMedianFilter(tempTexture, radius);
                    break;
                case SmoothingMethod.PenumbraBlur:
                    result = ApplyPenumbraBlur(tempTexture, radius);
                    break;
            }

            if (i > 0)
            {
                Object.DestroyImmediate(tempTexture);
            }
        }

        return result;
    }

    private static Texture2D ApplyGaussianBlur(Texture2D source, int radius)
    {
        float[] kernel = GenerateGaussianKernel(radius);
        int kernelSize = kernel.Length;
        int kernelRadius = kernelSize / 2;

        Color[] pixels = source.GetPixels();
        float[] luminance = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            luminance[i] = pixels[i].grayscale;
        }

        float[] horizontal = new float[pixels.Length];
        float[] result = new float[pixels.Length];
        int width = source.width;
        int height = source.height;

        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width;
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                for (int k = 0; k < kernelSize; k++)
                {
                    int nx = Mathf.Clamp(x + k - kernelRadius, 0, width - 1);
                    sum += luminance[rowIndex + nx] * kernel[k];
                }
                horizontal[rowIndex + x] = sum;
            }
        }

        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width;
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                for (int k = 0; k < kernelSize; k++)
                {
                    int ny = Mathf.Clamp(y + k - kernelRadius, 0, height - 1);
                    sum += horizontal[ny * width + x] * kernel[k];
                }
                result[rowIndex + x] = sum;
            }
        }

        Color[] colorResult = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            float val = result[i];
            colorResult[i] = new Color(val, val, val, 1f);
        }

        Texture2D output = new Texture2D(width, height, TextureFormat.RGB24, false);
        output.SetPixels(colorResult);
        output.Apply();

        return output;
    }

    private static Texture2D ApplyBoxBlur(Texture2D source, int radius)
    {
        Color[] pixels = source.GetPixels();
        float[] luminance = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            luminance[i] = pixels[i].grayscale;
        }

        float[] horizontal = new float[pixels.Length];
        float[] result = new float[pixels.Length];
        int width = source.width;
        int height = source.height;
        int kernelSize = radius * 2 + 1;
        float invKernelSize = 1f / kernelSize;

        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width;
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                for (int k = -radius; k <= radius; k++)
                {
                    int nx = Mathf.Clamp(x + k, 0, width - 1);
                    sum += luminance[rowIndex + nx];
                }
                horizontal[rowIndex + x] = sum * invKernelSize;
            }
        }

        for (int y = 0; y < height; y++)
        {
            int rowIndex = y * width;
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                for (int k = -radius; k <= radius; k++)
                {
                    int ny = Mathf.Clamp(y + k, 0, height - 1);
                    sum += horizontal[ny * width + x];
                }
                result[rowIndex + x] = sum * invKernelSize;
            }
        }

        Color[] colorResult = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            float val = result[i];
            colorResult[i] = new Color(val, val, val, 1f);
        }

        Texture2D output = new Texture2D(width, height, TextureFormat.RGB24, false);
        output.SetPixels(colorResult);
        output.Apply();

        return output;
    }

    private static Texture2D ApplyMedianFilter(Texture2D source, int radius)
    {
        Color[] pixels = source.GetPixels();
        Color[] result = new Color[pixels.Length];
        int width = source.width;
        int height = source.height;

        int windowSize = (radius * 2 + 1) * (radius * 2 + 1);
        float[] values = new float[windowSize];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int count = 0;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int ny = Mathf.Clamp(y + dy, 0, height - 1);
                        values[count++] = pixels[ny * width + nx].grayscale;
                    }
                }

                System.Array.Sort(values, 0, count);
                float median = values[count / 2];
                result[y * width + x] = new Color(median, median, median, 1f);
            }
        }

        Texture2D output = new Texture2D(width, height, TextureFormat.RGB24, false);
        output.SetPixels(result);
        output.Apply();

        return output;
    }

    private static Texture2D ApplyPenumbraBlur(Texture2D source, int radius)
    {
        // Shadows get more blur, while brighter areas get less
        Color[] pixels = source.GetPixels();
        float[] luminance = new float[pixels.Length];
        float[] result = new float[pixels.Length];
        int width = source.width;
        int height = source.height;

        for (int i = 0; i < pixels.Length; i++)
        {
            luminance[i] = pixels[i].grayscale;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                float centerLum = luminance[index];

                float blurAmount = 1f - centerLum;
                int dynamicRadius = Mathf.Max(1, Mathf.RoundToInt(radius * blurAmount));

                float sum = 0f;
                int count = 0;

                for (int dy = -dynamicRadius; dy <= dynamicRadius; dy++)
                {
                    for (int dx = -dynamicRadius; dx <= dynamicRadius; dx++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int ny = Mathf.Clamp(y + dy, 0, height - 1);
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);

                        if (distance <= dynamicRadius)
                        {
                            float weight = 1f - (distance / dynamicRadius);
                            sum += luminance[ny * width + nx] * weight;
                            count++;
                        }
                    }
                }

                result[index] = count > 0 ? sum / count : centerLum;
            }
        }

        Color[] colorResult = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            float val = result[i];
            colorResult[i] = new Color(val, val, val, 1f);
        }

        Texture2D output = new Texture2D(width, height, TextureFormat.RGB24, false);
        output.SetPixels(colorResult);
        output.Apply();

        return output;
    }

    private static float[] GenerateGaussianKernel(int radius)
    {
        int size = radius * 2 + 1;
        float[] kernel = new float[size];
        float sigma = radius / 3f;
        float twoSigmaSquare = 2f * sigma * sigma;
        float sum = 0f;

        for (int i = 0; i < size; i++)
        {
            int x = i - radius;
            kernel[i] = Mathf.Exp(-(x * x) / twoSigmaSquare);
            sum += kernel[i];
        }

        for (int i = 0; i < size; i++)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }

    private static Texture2D DuplicateTexture(Texture2D source)
    {
        Texture2D copy = new Texture2D(source.width, source.height, source.format, false);
        copy.SetPixels(source.GetPixels());
        copy.Apply();

        return copy;
    }
}