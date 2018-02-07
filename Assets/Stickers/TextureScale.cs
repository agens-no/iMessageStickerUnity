using System;
using UnityEngine;

namespace Agens.Stickers
{
    public static class TextureScale
    {
        public static Texture2D ScaledResized(Texture2D src, int width, int height, Color backgroundColor, float fillPercentage, FilterMode mode = FilterMode.Trilinear, ScaleMode anchor = ScaleMode.ScaleToFit)
        {
            var result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            if (src == null)
            {
                Debug.LogWarning("Source texture is null");
                return result;
            }

            var rtt = CreateScaledTexture(src,width,height,backgroundColor,fillPercentage,mode, anchor);

            var texR = new Rect(0,0,width,height);
            result.ReadPixels(texR,0,0,true);
            result.Apply(false);
            RenderTexture.active = null;
            rtt.Release();
            return result;
        }

        private static RenderTexture CreateScaledTexture(Texture2D src, int width, int height, Color backgroundColor, float fillPercentage, FilterMode fmode = FilterMode.Trilinear, ScaleMode scaleMode = ScaleMode.ScaleToFit)
        {
            var rtt = new RenderTexture(width, height, 32);

            try
            {
                src.filterMode = fmode;
                src.Apply(true);
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e);
                return rtt;
            }

            RenderTexture.active = rtt;

            GL.LoadPixelMatrix(0, width, height, 0);

            GL.Clear(true,true,backgroundColor);

            fillPercentage = scaleMode == ScaleMode.ScaleToFit ? Mathf.Clamp01(fillPercentage) : 1;

            var scaledWidth = width * fillPercentage;
            var scaledHeight = height * fillPercentage;
            var xOffset = (width - scaledWidth) / 2f;
            var yOffset = (height - scaledHeight) / 2f;

            DrawTexture(new Rect(xOffset, yOffset, scaledWidth, scaledHeight), src, scaleMode);
            return rtt;
        }

        private static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, float imageAspect = 0)
        {
            if (imageAspect == 0.0)
                imageAspect = image.width / (float) image.height;

            var screenRect = new Rect();
            var sourceRect = new Rect();

            CalculateScaledTextureRects(position, scaleMode, imageAspect, ref screenRect, ref sourceRect);
            Graphics.DrawTexture(screenRect, image, sourceRect, 0, 0, 0, 0);
        }

        /// <summary>
        /// Taken from UnityEngine
        /// </summary>
        /// <param name="position"></param>
        /// <param name="scaleMode"></param>
        /// <param name="imageAspect"></param>
        /// <param name="outScreenRect"></param>
        /// <param name="outSourceRect"></param>
        /// <returns></returns>
        private static bool CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, ref Rect outScreenRect, ref Rect outSourceRect)
        {
            float posAspect = position.width / position.height;
            bool flag = false;
            if (scaleMode != ScaleMode.StretchToFill)
            {
                if (scaleMode != ScaleMode.ScaleAndCrop)
                {
                    if (scaleMode == ScaleMode.ScaleToFit)
                    {
                        if (posAspect > imageAspect)
                        {
                            float num2 = imageAspect / posAspect;
                            outScreenRect = new Rect(position.xMin + (float) (position.width * (1.0 - num2) * 0.5), position.yMin, num2 * position.width, position.height);
                            outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                            flag = true;
                        }
                        else
                        {
                            float num2 = posAspect / imageAspect;
                            outScreenRect = new Rect(position.xMin, position.yMin + (float) (position.height * (1.0 - num2) * 0.5), position.width, num2 * position.height);
                            outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                            flag = true;
                        }
                    }
                }
                else if (posAspect > imageAspect)
                {
                    float height = imageAspect / posAspect;
                    outScreenRect = position;
                    outSourceRect = new Rect(0.0f, (float) ((1.0 - height) * 0.5), 1f, height);
                    flag = true;
                }
                else
                {
                    float width = posAspect / imageAspect;
                    outScreenRect = position;
                    outSourceRect = new Rect((float) (0.5 - width * 0.5), 0.0f, width, 1f);
                    flag = true;
                }
            }
            else
            {
                outScreenRect = position;
                outSourceRect = new Rect(0.0f, 0.0f, 1f, 1f);
                flag = true;
            }
            return flag;
        }
    }
}