using System.Collections.Generic;
using StupidTemplate.Menu;
using UnityEngine;

namespace StupidTemplate.Classes
{
    public class ColorChanger : MonoBehaviour
    {
        private const int GradientResolution = 128;

        private static readonly Dictionary<
                (ExtGradient Gradient, bool Vertical),
                GradientTextureData> gradientTextures = new();

        private static          Shader gradientShader;
        private static readonly int    MainTex    = Shader.PropertyToID("_MainTex");
        private static readonly int    Color1     = Shader.PropertyToID("_Color");
        private static readonly int    Glossiness = Shader.PropertyToID("_Glossiness");

        public bool spatialGradient;
        public bool verticalGradient;

        public  Renderer    targetRenderer;
        public  ExtGradient colors;
        private Shader      solidShader;

        private Material targetMaterial;

        private void Start()
        {
            if (colors == null)
            {
                Destroy(this);

                return;
            }

            targetRenderer = GetComponent<Renderer>();

            if (targetRenderer == null)
            {
                Destroy(this);

                return;
            }

            targetMaterial = targetRenderer.material;
            solidShader    = targetMaterial.shader;

            Update();
        }

        private void Update()
        {
            if (colors         == null ||
                targetRenderer == null ||
                targetMaterial == null)
            {
                return;
            }

            targetRenderer.enabled = !colors.transparent;

            if (colors.transparent)
                return;

            if (!spatialGradient || colors.IsFlat())
            {
                ApplySolidColor();

                return;
            }

            ApplyGradient();
        }

        private void ApplySolidColor()
        {
            if (solidShader           != null &&
                targetMaterial.shader != solidShader)
            {
                targetMaterial.shader = solidShader;
            }

            targetMaterial.mainTexture = null;
            targetMaterial.color       = colors.GetCurrentColor();
        }

        private void ApplyGradient()
        {
            Shader shader = GetGradientShader();

            if (shader == null)
            {
                ApplySolidColor();

                return;
            }

            if (targetMaterial.shader != shader)
                targetMaterial.shader = shader;

            Texture2D texture = GetGradientTexture(
                    colors,
                    verticalGradient);

            if (targetMaterial.HasProperty(MainTex))
                targetMaterial.SetTexture(MainTex, texture);

            targetMaterial.mainTexture = texture;

            if (targetMaterial.HasProperty(Color1))
                targetMaterial.SetColor(Color1, Color.white);

            if (targetMaterial.HasProperty(Glossiness))
                targetMaterial.SetFloat(Glossiness, 0f);

            targetMaterial.renderQueue = 2000;
        }

        private static Shader GetGradientShader()
        {
            if (gradientShader != null)
                return gradientShader;

            gradientShader = Shader.Find("Unlit/Texture");

            if (gradientShader == null)
                gradientShader = Shader.Find("Standard");

            return gradientShader;
        }

        private static Texture2D GetGradientTexture(
                ExtGradient gradient,
                bool        vertical)
        {
            (ExtGradient Gradient, bool Vertical) key =
                    (gradient, vertical);

            if (!gradientTextures.TryGetValue(
                        key,
                        out GradientTextureData data))
            {
                int width =
                        vertical
                                ? 1
                                : GradientResolution;

                int height =
                        vertical
                                ? GradientResolution
                                : 1;

                data = new GradientTextureData
                {
                        texture = new Texture2D(
                                width,
                                height,
                                TextureFormat.RGBA32,
                                false)
                        {
                                filterMode = FilterMode.Bilinear,
                                wrapMode   = TextureWrapMode.Clamp,
                        },
                };

                gradientTextures.Add(
                        key,
                        data);
            }

            if (data.lastFrame == Time.frameCount)
                return data.texture;

            data.lastFrame =
                    Time.frameCount;

            float animationTime =
                    Time.time *
                    Settings.gradientSpeed;

            float pulse =
                    0.5f -
                    Mathf.Cos(
                            animationTime *
                            Mathf.PI      *
                            2f) *
                    0.5f;

            for (int i = 0; i < GradientResolution; i++)
            {
                float position =
                        i /
                        (float)(GradientResolution - 1);

                float sampleTime;

                switch (Settings.gradientAnimationMode)
                {
                    case GradientAnimationMode.Pulse:
                    {
                        /*
                         * Smoothly reverses the gradient.
                         *
                         * A -> B
                         * B -> A
                         * A -> B
                         */
                        sampleTime =
                                Mathf.Lerp(
                                        position,
                                        1f - position,
                                        pulse);

                        break;
                    }

                    default:
                    {
                        sampleTime =
                                Mathf.Repeat(
                                        position +
                                        animationTime,
                                        1f);

                        break;
                    }
                }

                data.pixels[i] =
                        gradient.GetColorTime(
                                sampleTime);
            }

            data.texture.SetPixels(
                    data.pixels);

            data.texture.Apply(
                    false,
                    false);

            return data.texture;
        }

        public static void ClearGradientCache()
        {
            foreach (GradientTextureData data in gradientTextures.Values)
            {
                if (data.texture != null)
                    Destroy(data.texture);
            }

            gradientTextures.Clear();
        }

        private sealed class GradientTextureData
        {
            public readonly Color[] pixels =
                    new Color[GradientResolution];

            public int lastFrame = -1;

            public Texture2D texture;
        }
    }
}