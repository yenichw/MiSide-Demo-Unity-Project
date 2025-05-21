using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public static class OutlineEffect
    {
        public static readonly int FillRefHash = Shader.PropertyToID("_FillRef");
        public static readonly int ColorMaskHash = Shader.PropertyToID("_ColorMask");
        public static readonly int OutlineRefHash = Shader.PropertyToID("_OutlineRef");
        public static readonly int RefHash = Shader.PropertyToID("_Ref");
        public static readonly int EffectSizeHash = Shader.PropertyToID("_EffectSize");
        public static readonly int CullHash = Shader.PropertyToID("_Cull");
        public static readonly int ZTestHash = Shader.PropertyToID("_ZTest");
        public static readonly int ColorHash = Shader.PropertyToID("_EPOColor");
        public static readonly int ScaleHash = Shader.PropertyToID("_Scale");
        public static readonly int ShiftHash = Shader.PropertyToID("_Shift");
        public static readonly int InfoBufferHash = Shader.PropertyToID("_InfoBuffer");
        public static readonly int ComparisonHash = Shader.PropertyToID("_Comparison");
        public static readonly int ReadMaskHash = Shader.PropertyToID("_ReadMask");
        public static readonly int OperationHash = Shader.PropertyToID("_Operation");
        public static readonly int CutoutThresholdHash = Shader.PropertyToID("_CutoutThreshold");
        public static readonly int CutoutMaskHash = Shader.PropertyToID("_CutoutMask");
        public static readonly int TextureIndexHash = Shader.PropertyToID("_TextureIndex");
        public static readonly int CutoutTextureHash = Shader.PropertyToID("_CutoutTexture");
        public static readonly int CutoutTextureSTHash = Shader.PropertyToID("_CutoutTexture_ST");
        public static readonly int SrcBlendHash = Shader.PropertyToID("_SrcBlend");
        public static readonly int DstBlendHash = Shader.PropertyToID("_DstBlend");

        private static Material OutlineMaterial;
        private static Material ObstacleMaterial;
        private static Material FillMaskMaterial;
        private static Material ZPrepassMaterial;
        private static Material DilateMaterial;
        private static Material BlurMaterial;
        private static Material FinalBlitMaterial;
        private static Material BasicBlitMaterial;
        private static Material ClearStencilMaterial;

        private static RTHandleSystem system;

        public static RTHandleSystem HandleSystem
        {
            get
            {
                if (system != null) 
                    return system;
                
                system = new RTHandleSystem();
                system.Initialize(1, 1);

                return system;
            }
        }

        private struct OutlineTargetGroup
        {
            public readonly Outlinable Outlinable;
            public readonly OutlineTarget Target;

            public OutlineTargetGroup(Outlinable outlinable, OutlineTarget target)
            {
                Outlinable = outlinable;
                Target = target;
            }
        }

        private static List<OutlineTargetGroup> targets = new List<OutlineTargetGroup>();

        private static List<string> keywords = new List<string>();

        private static Material LoadMaterial(string shaderName)
        {
            var material = new Material(Resources.Load<Shader>(string.Format("Easy performant outline/Shaders/{0}", shaderName)));
            if (SystemInfo.supportsInstancing)
                material.enableInstancing = true;

            return material;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitMaterials()
        {
            if (ObstacleMaterial == null)
                ObstacleMaterial = LoadMaterial("Obstacle");

            if (OutlineMaterial == null)
                OutlineMaterial = LoadMaterial("Outline");

            if (ZPrepassMaterial == null)
                ZPrepassMaterial = LoadMaterial("ZPrepass");

            if (DilateMaterial == null)
                DilateMaterial = LoadMaterial("Dilate");

            if (BlurMaterial == null)
                BlurMaterial = LoadMaterial("Blur");

            if (FinalBlitMaterial == null)
                FinalBlitMaterial = LoadMaterial("FinalBlit");

            if (BasicBlitMaterial == null)
                BasicBlitMaterial = LoadMaterial("BasicBlit");

            if (FillMaskMaterial == null)
                FillMaskMaterial = LoadMaterial("Fills/FillMask");

            if (ClearStencilMaterial == null)
                ClearStencilMaterial = LoadMaterial("ClearStencil");
        }

        private static void Postprocess(OutlineParameters parameters, RTHandle first, RTHandle second, Material material, int iterations, int eyeSlice, bool additionalShift, float shiftValue, ref int stencil, Rect viewport, float scale)
        {
            if (iterations <= 0)
                return;

            parameters.Buffer.SetGlobalInt(ComparisonHash, (int)CompareFunction.Equal);

            for (var index = 1; index <= iterations; index++)
            {
                parameters.Buffer.SetGlobalInt(RefHash, stencil);

                var shift = (additionalShift ? (float)index : 1.0f);

                parameters.Buffer.SetGlobalVector(ShiftHash, new Vector4(shift * scale, 0));
                Blit(parameters, first, second, first, material, shiftValue, eyeSlice, -1, viewport);

                stencil = (stencil + 1) % 255;
                parameters.Buffer.SetGlobalInt(RefHash, stencil);

                parameters.Buffer.SetGlobalVector(ShiftHash, new Vector4(0, shift * scale));
                Blit(parameters, second, first, first, material, shiftValue, eyeSlice, -1, viewport);

                stencil = (stencil + 1) % 255;
            }
        }

        private static void Blit(OutlineParameters parameters, RTHandle source, RTHandle destination, RTHandle destinationDepth, Material material, float effectSize, int eyeSlice, int pass = -1, Rect? viewport = null)
        {
            parameters.Buffer.SetGlobalFloat(EffectSizeHash, effectSize);
            BlitUtility.Blit(parameters, source, destination, destinationDepth, eyeSlice, material, pass, viewport);
        }
        
        private static void Draw(OutlineParameters parameters, RTHandle destination, RTHandle destinationDepth, Material material, float effectSize, int eyeSlice, int pass = -1, Rect? viewport = null)
        {
            parameters.Buffer.SetGlobalFloat(EffectSizeHash, effectSize);
            BlitUtility.Draw(parameters, destination, destinationDepth, eyeSlice, material, pass, viewport);
        }

        private static float GetBlurShift(BlurType blurType, int iterationsCount)
        {
            switch (blurType)
            {
                case BlurType.Box:
                    return (float)(iterationsCount * 0.65f) + 1.0f;
                case BlurType.Gaussian5x5:
                    return 3.0f *  + iterationsCount;
                case BlurType.Gaussian9x9:
                    return 5.0f + iterationsCount;
                case BlurType.Gaussian13x13:
                    return 7.0f + iterationsCount;
                default:
                    throw new ArgumentException("Unknown blur type");
            }
        }

        private static float GetMaskingValueForMode(OutlinableDrawingMode mode)
        {
            if ((mode & OutlinableDrawingMode.Mask) != 0)
                return 0.6f;
            
            return (mode & OutlinableDrawingMode.Obstacle) != 0 ? 0.25f : 1.0f;
        }

        private static float ComputeEffectShift(OutlineParameters parameters)
        {
            var effectShift = GetBlurShift(parameters.BlurType, parameters.BlurIterations) * parameters.BlurShift + parameters.DilateIterations * 4.0f * parameters.DilateShift;
            return effectShift * 1.1f;
        }
        
        private static void PrepareTargets(OutlineParameters parameters)
        {
            targets.Clear();

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                {
                    var target = outlinable.OutlineTargets[index];
                    var renderer = target.Renderer;
                    if (!target.IsVisible)
                    {
                        if ((outlinable.DrawingMode & OutlinableDrawingMode.GenericMask) == 0 || renderer == null)
                            continue;
                    }

                    targets.Add(new OutlineTargetGroup(outlinable, target));
                }
            }
        }

        public static void SetupOutline(OutlineParameters parameters)
        {
            parameters.Buffer.SetRenderTarget(parameters.Handles.Target, -1);
            parameters.Buffer.SetGlobalVector(ScaleHash, parameters.Scale);
            PrepareTargets(parameters);
            InitMaterials();
            var expectedEffectShift = ComputeEffectShift(parameters);
            var effectShift = expectedEffectShift + 3;
            var finalBlitEffectShift = expectedEffectShift;
            
            var eyeDepthSlice = RenderTargetUtility.GetDepthSliceForEye(parameters.EyeMask);

            parameters.Buffer.ClearRenderTarget(true, true, Color.clear);
            
            parameters.Buffer.SetRenderTarget(parameters.Handles.PrimaryTarget, -1);
            parameters.Buffer.ClearRenderTarget(true, true, Color.clear);
            
            parameters.Buffer.SetRenderTarget(parameters.Handles.SecondaryTarget, -1);
            parameters.Buffer.ClearRenderTarget(true, true, Color.clear);

            if (parameters.UseInfoBuffer)
            {
                parameters.Buffer.SetRenderTarget(parameters.Handles.InfoTarget, -1);
                parameters.Buffer.ClearRenderTarget(true, true, Color.clear);
                
                parameters.Buffer.SetRenderTarget(parameters.Handles.PrimaryInfoBufferTarget, -1);
                parameters.Buffer.ClearRenderTarget(true, true, Color.clear);
                
                parameters.Buffer.SetRenderTarget(parameters.Handles.SecondaryInfoBufferTarget, -1);
                parameters.Buffer.ClearRenderTarget(true, true, Color.clear);
            }
            
            parameters.Buffer.SetGlobalInt(SrcBlendHash, (int)BlendMode.One);
            parameters.Buffer.SetGlobalInt(DstBlendHash, (int)BlendMode.Zero);

            var outlineRef = 1;
            parameters.Buffer.SetGlobalInt(OutlineRefHash, outlineRef);

            SetupDilateKeyword(parameters);

            var scaledViewVector = new Vector2Int(parameters.ScaledBufferWidth, parameters.ScaledBufferHeight);

            BlitUtility.PrepareForRendering(parameters);

            parameters.Buffer.SetRenderTarget(parameters.Handles.Target, parameters.DepthTarget, eyeDepthSlice);
            parameters.Buffer.ClearRenderTarget(false, true, Color.clear);
            parameters.Buffer.SetViewport(parameters.Viewport);
            
            DrawOutlineables(parameters, CompareFunction.LessEqual, x => true, x => Color.clear, x => ZPrepassMaterial, RenderStyle.FrontBack | RenderStyle.Single, OutlinableDrawingMode.ZOnly);

            parameters.Buffer.DisableShaderKeyword(KeywordsUtility.GetEnabledInfoBufferKeyword());

            if (parameters.UseInfoBuffer)
            {
                parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetInfoBufferStageKeyword());

                parameters.Buffer.SetRenderTarget(parameters.Handles.InfoTarget, parameters.DepthTarget, eyeDepthSlice);
                parameters.Buffer.ClearRenderTarget(false, true, Color.clear);
                parameters.Buffer.SetViewport(parameters.Viewport);

                DrawOutlineables(parameters, CompareFunction.Always, x => x.OutlineParameters.Enabled,   x => new Color(x.OutlineParameters.DilateShift, x.OutlineParameters.BlurShift, 0, 1),
                    x => OutlineMaterial,
                    RenderStyle.Single, 
                    OutlinableDrawingMode.Normal);

                DrawOutlineables(parameters, CompareFunction.NotEqual, x => x.BackParameters.Enabled,      x => new Color(x.BackParameters.DilateShift, x.BackParameters.BlurShift, 0, 1),
                    x => OutlineMaterial,
                    RenderStyle.FrontBack);

                DrawOutlineables(parameters, CompareFunction.LessEqual, x => x.FrontParameters.Enabled,     x => new Color(x.FrontParameters.DilateShift, x.FrontParameters.BlurShift, 0, 1),
                    x => OutlineMaterial,
                    RenderStyle.FrontBack, 
                    OutlinableDrawingMode.Normal);

                DrawOutlineables(parameters, CompareFunction.LessEqual, x => true, x => new Color(0, 0, GetMaskingValueForMode(x.DrawingMode), 1),
                    x => ObstacleMaterial,
                    RenderStyle.Single | RenderStyle.FrontBack,
                    OutlinableDrawingMode.Obstacle | OutlinableDrawingMode.Mask);

                parameters.Buffer.SetGlobalInt(ComparisonHash, (int)CompareFunction.Always);
                parameters.Buffer.SetGlobalInt(OperationHash, (int)StencilOp.Keep);
                Blit(parameters,
                    parameters.Handles.InfoTarget,
                    parameters.Handles.PrimaryInfoBufferTarget,
                    parameters.Handles.PrimaryInfoBufferTarget,
                    BasicBlitMaterial, effectShift,
                    -1, -1, new Rect(0, 0, scaledViewVector.x, scaledViewVector.y));
                
                var iterationsCount =
                    (parameters.DilateQuality == DilateQuality.Base
                         ? parameters.DilateIterations
                         : parameters.DilateIterations * 2) + parameters.BlurIterations;
                
                var infoRef = 0;
                Postprocess(parameters, parameters.Handles.PrimaryInfoBufferTarget, parameters.Handles.SecondaryInfoBufferTarget, DilateMaterial, iterationsCount, 
                            eyeDepthSlice,
                            true, 
                            effectShift, ref infoRef, 
                            new Rect(0, 0, scaledViewVector.x, scaledViewVector.y), 
                            1.0f);
                
                parameters.Buffer.SetRenderTarget(parameters.Handles.InfoTarget, parameters.DepthTarget, eyeDepthSlice);
                parameters.Buffer.SetViewport(parameters.Viewport);

                parameters.Buffer.SetGlobalTexture(InfoBufferHash, parameters.Handles.PrimaryInfoBufferTarget);

                parameters.Buffer.DisableShaderKeyword(KeywordsUtility.GetInfoBufferStageKeyword());
            }

            if (parameters.UseInfoBuffer)
                parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetEnabledInfoBufferKeyword());
            
            parameters.Buffer.SetRenderTarget(parameters.Handles.Target, parameters.DepthTarget, eyeDepthSlice);
            parameters.Buffer.ClearRenderTarget(false, true, Color.clear);
            parameters.Buffer.SetViewport(parameters.Viewport);
            
            var drawnOutlinablesCount = 0;
            drawnOutlinablesCount += DrawOutlineables(parameters, CompareFunction.Always, x => x.OutlineParameters.Enabled, x => x.OutlineParameters.Color,
                x => OutlineMaterial,
                RenderStyle.Single, OutlinableDrawingMode.Normal);

            drawnOutlinablesCount += DrawOutlineables(parameters, CompareFunction.NotEqual, x => x.BackParameters.Enabled, x => x.BackParameters.Color,
                x => OutlineMaterial,
                RenderStyle.FrontBack, OutlinableDrawingMode.Normal);

            drawnOutlinablesCount += DrawOutlineables(parameters, CompareFunction.LessEqual, x => x.FrontParameters.Enabled, x => x.FrontParameters.Color,
                x => OutlineMaterial,
                RenderStyle.FrontBack, OutlinableDrawingMode.Normal);

            var postProcessingRef = 0;
            if (drawnOutlinablesCount > 0)
            {
                parameters.Buffer.SetGlobalInt(ComparisonHash, (int)CompareFunction.Always);
                parameters.Buffer.SetGlobalInt(OperationHash, (int)StencilOp.Keep);
                Blit(parameters, parameters.Handles.Target,
                    parameters.Handles.PrimaryTarget,
                    parameters.Handles.PrimaryTarget,
                    BasicBlitMaterial, effectShift, eyeDepthSlice, -1,
                    new Rect(0, 0, scaledViewVector.x, scaledViewVector.y));
                
                Postprocess(parameters, parameters.Handles.PrimaryTarget, parameters.Handles.SecondaryTarget, DilateMaterial, parameters.DilateIterations, eyeDepthSlice, false, effectShift, ref postProcessingRef,
                            new Rect(0, 0, scaledViewVector.x, scaledViewVector.y),
                            parameters.DilateShift);
            }

            parameters.Buffer.SetRenderTarget(parameters.Handles.Target, parameters.DepthTarget, eyeDepthSlice);
            parameters.Buffer.ClearRenderTarget(false, true, Color.clear);
            parameters.Buffer.SetViewport(parameters.Viewport);

            if (drawnOutlinablesCount > 0)
            {
                SetupBlurKeyword(parameters);
                Postprocess(parameters, parameters.Handles.PrimaryTarget, parameters.Handles.SecondaryTarget,
                    BlurMaterial, parameters.BlurIterations, eyeDepthSlice, false,
                    effectShift,
                    ref postProcessingRef,
                    new Rect(0, 0, scaledViewVector.x, scaledViewVector.y),
                    parameters.BlurShift);
            }

            parameters.Buffer.SetGlobalInt(ComparisonHash, (int)CompareFunction.NotEqual);
            parameters.Buffer.SetGlobalInt(ReadMaskHash, 255);
            parameters.Buffer.SetGlobalInt(OperationHash, (int)StencilOp.Replace);

            Blit(parameters, 
                parameters.Handles.PrimaryTarget, 
                parameters.Target, 
                parameters.DepthTarget, 
                FinalBlitMaterial, 
                finalBlitEffectShift, eyeDepthSlice, -1,
                parameters.Viewport);

            DrawFill(parameters, parameters.Target);
            
            Draw(parameters, 
                parameters.Target,
                parameters.DepthTarget, 
                ClearStencilMaterial, 
                finalBlitEffectShift, eyeDepthSlice, -1,
                parameters.Viewport);
        }

        private static void SetupDilateKeyword(OutlineParameters parameters)
        {
            KeywordsUtility.GetAllDilateKeywords(keywords);
            foreach (var keyword in keywords)
                parameters.Buffer.DisableShaderKeyword(keyword);

            parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetDilateQualityKeyword(parameters.DilateQuality));
        }

        private static void SetupBlurKeyword(OutlineParameters parameters)
        {
            KeywordsUtility.GetAllBlurKeywords(keywords);
            foreach (var keyword in keywords)
                parameters.Buffer.DisableShaderKeyword(keyword);

            parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetBlurKeyword(parameters.BlurType));
        }

        private static int DrawOutlineables(OutlineParameters parameters, CompareFunction function, Func<Outlinable, bool> shouldRender, Func<Outlinable, Color> colorProvider, Func<Outlinable, Material> materialProvider, RenderStyle styleMask, OutlinableDrawingMode modeMask = OutlinableDrawingMode.Normal)
        {
            var drawnCount = 0;
            parameters.Buffer.SetGlobalInt(ZTestHash, (int)function);
            
            SetMaskingMasking(parameters.Buffer, ComplexMaskingMode.None);
            var complexMaskingMode = ComplexMaskingMode.None;

            foreach (var targetGroup in targets)
            {
                var outlinable = targetGroup.Outlinable;
                if ((int)(outlinable.RenderStyle & styleMask) == 0)
                    continue;

                if ((int)(outlinable.DrawingMode & modeMask) == 0)
                    continue;

                if ((function == CompareFunction.NotEqual || function == CompareFunction.Always) &&
                    outlinable.ComplexMaskingMode != complexMaskingMode)
                {
                    SetMaskingMasking(parameters.Buffer, outlinable.ComplexMaskingMode);
                    complexMaskingMode = outlinable.ComplexMaskingMode;
                }

                var color = shouldRender(outlinable) ? colorProvider(outlinable) : Color.clear;

                parameters.Buffer.SetGlobalColor(ColorHash, color);
                var target = targetGroup.Target;

                parameters.Buffer.SetGlobalInt(ColorMaskHash, 255);

                SetupCutout(parameters, target);
                SetupCull(parameters, target);

                drawnCount++;

                var materialToUse = materialProvider(outlinable);
                parameters.Buffer.DrawRenderer(target.Renderer, materialToUse, target.ShiftedSubmeshIndex);
            }
            
            SetMaskingMasking(parameters.Buffer, ComplexMaskingMode.None);

            return drawnCount;
        }

        private static void SetMaskingMasking(CommandBufferWrapper buffer, ComplexMaskingMode maskingMode)
        {
            buffer.DisableShaderKeyword(KeywordsUtility.GetBackKeyword(ComplexMaskingMode.MaskingMode));
            buffer.DisableShaderKeyword(KeywordsUtility.GetBackKeyword(ComplexMaskingMode.ObstaclesMode));
            
            if (maskingMode != ComplexMaskingMode.None)
                buffer.EnableShaderKeyword(KeywordsUtility.GetBackKeyword(maskingMode));
        }

        private static void DrawFill(OutlineParameters parameters, RTHandle targetSurface)
        {
            var eye = RenderTargetUtility.GetDepthSliceForEye(parameters.EyeMask);
            parameters.Buffer.SetRenderTarget(targetSurface, parameters.DepthTarget, eye);
            parameters.Buffer.SetViewport(parameters.Viewport);

            var singleMask = 1;
            var frontMask = 2;
            var backMask = 3;

            parameters.Buffer.SetGlobalInt(ZTestHash, (int)CompareFunction.Greater);
            parameters.Buffer.SetGlobalInt(FillRefHash, backMask);
            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if ((outlinable.DrawingMode & OutlinableDrawingMode.Normal) == 0)
                    continue;

                for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                {
                    var target = outlinable.OutlineTargets[index];
                    if (!target.IsVisible)
                        continue;

                    var renderer = target.Renderer;
                    if (!outlinable.NeedsFillMask)
                        continue;

                    SetupCutout(parameters, target);
                    SetupCull(parameters, target);

                    parameters.Buffer.DrawRenderer(renderer, FillMaskMaterial, target.ShiftedSubmeshIndex);
                }
            }

            parameters.Buffer.SetGlobalInt(ZTestHash, (int)CompareFunction.LessEqual);
            parameters.Buffer.SetGlobalInt(FillRefHash, frontMask);
            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if ((outlinable.DrawingMode & OutlinableDrawingMode.Normal) == 0)
                    continue;

                for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                {
                    var target = outlinable.OutlineTargets[index];
                    if (!target.IsVisible)
                        continue;

                    if (!outlinable.NeedsFillMask)
                        continue;

                    var renderer = target.Renderer;
                    SetupCutout(parameters, target);
                    SetupCull(parameters, target);

                    parameters.Buffer.DrawRenderer(renderer, FillMaskMaterial, target.ShiftedSubmeshIndex);
                }
            }

            var currentMaskingMode = ComplexMaskingMode.None;
            SetMaskingMasking(parameters.Buffer, ComplexMaskingMode.None);

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if ((outlinable.DrawingMode & OutlinableDrawingMode.Normal) == 0)
                    continue;

                if (outlinable.ComplexMaskingMode != currentMaskingMode)
                {
                    SetMaskingMasking(parameters.Buffer, outlinable.ComplexMaskingMode);
                    currentMaskingMode = ComplexMaskingMode.None;
                }

                if (outlinable.RenderStyle == RenderStyle.FrontBack)
                {
                    if ((outlinable.BackParameters.FillPass.Material == null || !outlinable.BackParameters.Enabled) &&
                        (outlinable.FrontParameters.FillPass.Material == null || !outlinable.FrontParameters.Enabled))
                        continue;

                    var frontMaterial = outlinable.FrontParameters.FillPass.Material;
                    parameters.Buffer.SetGlobalInt(FillRefHash, frontMask);
                    if (frontMaterial != null && outlinable.FrontParameters.Enabled)
                    {
                        for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                        {
                            var target = outlinable.OutlineTargets[index];
                            if (!target.IsVisible)
                                continue;

                            var renderer = target.Renderer;
                            SetupCutout(parameters, target);
                            SetupCull(parameters, target);

                            parameters.Buffer.DrawRenderer(renderer, frontMaterial, target.ShiftedSubmeshIndex);
                        }
                    }

                    var backMaterial = outlinable.BackParameters.FillPass.Material;
                    parameters.Buffer.SetGlobalInt(FillRefHash, backMask);
                    if (backMaterial == null || !outlinable.BackParameters.Enabled) 
                        continue;

                    for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                    {
                        var target = outlinable.OutlineTargets[index];
                        if (!target.IsVisible)
                            continue;

                        var renderer = target.Renderer;
                        SetupCutout(parameters, target);
                        SetupCull(parameters, target);

                        parameters.Buffer.DrawRenderer(renderer, backMaterial, target.ShiftedSubmeshIndex);
                    }
                }
                else
                {
                    if (outlinable.OutlineParameters.FillPass.Material == null)
                        continue;

                    if (!outlinable.OutlineParameters.Enabled)
                        continue;

                    parameters.Buffer.SetGlobalInt(ZTestHash, (int)CompareFunction.Always);
                    parameters.Buffer.SetGlobalInt(FillRefHash, singleMask);
                    var fillMaterial = outlinable.OutlineParameters.FillPass.Material;

                    for (var index = 0; index < outlinable.OutlineTargets.Count; index++)
                    {
                        var target = outlinable.OutlineTargets[index];
                        if (!target.IsVisible)
                            continue;

                        var renderer = target.Renderer;
                        SetupCutout(parameters, target);
                        SetupCull(parameters, target);

                        parameters.Buffer.DrawRenderer(renderer, fillMaterial, target.ShiftedSubmeshIndex);
                    }
                }
                
                if (outlinable.ComplexMaskingMode != ComplexMaskingMode.None)
                    SetMaskingMasking(parameters.Buffer, ComplexMaskingMode.None);
            }
        }

        private static void SetupCutout(OutlineParameters parameters, OutlineTarget target)
        {
            if (target.Renderer == null)
                return;

            var mask = new Vector4(
                (target.CutoutMask & ColorMask.R) != ColorMask.None ? 1.0f : 0.0f,
                (target.CutoutMask & ColorMask.G) != ColorMask.None ? 1.0f : 0.0f,
                (target.CutoutMask & ColorMask.B) != ColorMask.None ? 1.0f : 0.0f,
                (target.CutoutMask & ColorMask.A) != ColorMask.None ? 1.0f : 0.0f);

            parameters.Buffer.SetGlobalVector(CutoutMaskHash, mask);

            if (target.Renderer is SpriteRenderer spriteRenderer)
            {
                var sprite = spriteRenderer.sprite;
                if (sprite == null)
                {
                    parameters.Buffer.DisableShaderKeyword(KeywordsUtility.GetCutoutKeyword());
                    return;
                }

                parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetCutoutKeyword());
                parameters.Buffer.SetGlobalFloat(CutoutThresholdHash, target.CutoutThreshold);
                parameters.Buffer.SetGlobalTexture(CutoutTextureHash, parameters.TextureHandleMap[spriteRenderer.sprite.texture]);

                return;
            }

            if (target.IsValidForCutout)
            {
                var sharedMaterial = target.SharedMaterial;

                parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetCutoutKeyword());
                parameters.Buffer.SetGlobalFloat(CutoutThresholdHash, target.CutoutThreshold);

                var offset = sharedMaterial.GetTextureOffset(target.CutoutTextureId);
                var scale = sharedMaterial.GetTextureScale(target.CutoutTextureId);

                parameters.Buffer.SetGlobalVector(CutoutTextureSTHash, new Vector4(scale.x, scale.y, offset.x, offset.y));

                var texture = target.CutoutTexture;
                if (texture == null || texture.dimension != TextureDimension.Tex2DArray)
                    parameters.Buffer.DisableShaderKeyword(KeywordsUtility.GetTextureArrayCutoutKeyword());
                else
                {
                    parameters.Buffer.SetGlobalFloat(TextureIndexHash, target.CutoutTextureIndex);
                    parameters.Buffer.EnableShaderKeyword(KeywordsUtility.GetTextureArrayCutoutKeyword());
                }

                parameters.Buffer.SetGlobalTexture(CutoutTextureHash, parameters.TextureHandleMap[texture]);
            }
            else
                parameters.Buffer.DisableShaderKeyword(KeywordsUtility.GetCutoutKeyword());

        }

        private static void SetupCull(OutlineParameters parameters, OutlineTarget target)
        {
            parameters.Buffer.SetGlobalInt(CullHash, (int)target.CullMode);
        }
    }
}