using RVT.Job;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RVT.Renderer
{
    internal class FeedbackRenderPass : ScriptableRenderPass
    {
        ProfilingSampler m_DrawFeedbackSampler;
        ProfilingSampler m_DrawPageTableSampler;
        ProfilingSampler m_DrawPageColorSampler;
        ProfilingSampler m_VirtualTextureSampler;
        
        int2 m_FeedbackSize;
        LayerMask m_LayerMask;
        private ShaderTagId m_ShaderPassID;
        private EFeedbackScale m_FeedbackScale;
        private FilteringSettings m_FilterSetting;
        private RenderTexture m_FeedbackTexture;
        RenderTargetIdentifier m_FeedbackTextureID;
        //这个Pass执行了以后会将对应的数据回读到CPU
        private FVirtualTextureFeedback m_FeedbackProcessor;

        public FeedbackRenderPass(in LayerMask layerMask, int2 feedbackSize, in EFeedbackScale feedbackScale)
        {
            m_LayerMask = layerMask;
            m_FeedbackSize = feedbackSize;
            m_FeedbackScale = feedbackScale;
            m_ShaderPassID = new ShaderTagId("VTFeedback");
            m_FilterSetting = new FilteringSettings(RenderQueueRange.opaque, m_LayerMask);
            m_DrawFeedbackSampler = ProfilingSampler.Get(EVirtualTexturePass.DrawFeedback);
            m_DrawPageTableSampler = ProfilingSampler.Get(EVirtualTexturePass.DrawPageTable);
            m_DrawPageColorSampler = ProfilingSampler.Get(EVirtualTexturePass.DrawPageColor);
            m_VirtualTextureSampler = ProfilingSampler.Get(EVirtualTexturePass.VirtualTexture);
            m_FeedbackProcessor = new FVirtualTextureFeedback(true);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //相机数据准备好以后,生成回读用的RenderTexture
            Camera camera = renderingData.cameraData.camera;
            int2 size = new int2(math.min(m_FeedbackSize.x, camera.pixelWidth),
                math.min(m_FeedbackSize.y, camera.pixelWidth));
            m_FeedbackTexture = RenderTexture.GetTemporary(size.x / (int)m_FeedbackScale, size.y / (int)m_FeedbackScale, 1, GraphicsFormat.R8G8B8A8_UNorm, 1);
            m_FeedbackTexture.name = "FeedbackTexture";
            m_FeedbackTextureID = new RenderTargetIdentifier(m_FeedbackTexture);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying || VirtualTextureVolume.s_VirtualTextureVolume == null) { return; }

            CommandBuffer cmdBuffer = CommandBufferPool.Get();
            Camera camera = renderingData.cameraData.camera;

            using (new ProfilingScope(cmdBuffer, m_VirtualTextureSampler))
            {
                context.ExecuteCommandBuffer(cmdBuffer);
                cmdBuffer.Clear();
                DrawVirtualTexture(context, cmdBuffer, camera, ref renderingData);
            }
            context.ExecuteCommandBuffer(cmdBuffer);
            CommandBufferPool.Release(cmdBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmdBuffer)
        {
            RenderTexture.ReleaseTemporary(m_FeedbackTexture);
        }

        public unsafe void DrawVirtualTexture(ScriptableRenderContext renderContext, CommandBuffer cmdBuffer, Camera camera, ref RenderingData renderingData)
        {
            FPageProducer pageProducer = VirtualTextureVolume.s_VirtualTextureVolume.pageProducer;
            FPageRenderer pageRenderer = VirtualTextureVolume.s_VirtualTextureVolume.pageRenderer;
            VirtualTextureAsset virtualTexture = VirtualTextureVolume.s_VirtualTextureVolume.virtualTexture;

            using (new ProfilingScope(cmdBuffer, m_DrawPageTableSampler))
            {
                if (m_FeedbackProcessor.isReady)
                {
                    NativeArray<int4> decodeDatas = new NativeArray<int4>(m_FeedbackProcessor.readbackDatas.Length, Allocator.TempJob);

                    FDecodeFeedbackJob decodeFeedbackJob;
                    decodeFeedbackJob.pageSize = virtualTexture.pageSize;
                    decodeFeedbackJob.decodeDatas = decodeDatas;
                    decodeFeedbackJob.encodeDatas = m_FeedbackProcessor.readbackDatas;
                    decodeFeedbackJob.Schedule(m_FeedbackProcessor.readbackDatas.Length, 256).Complete();
                    //先把feedBack计算出来
                    pageProducer.ProcessFeedback(ref decodeDatas, virtualTexture.NumMip, virtualTexture.tileNum, virtualTexture.pageSize, virtualTexture.lruCache, ref pageRenderer.loadRequests);
                    decodeDatas.Dispose();
                    //设置RenderTarget为vt
                    cmdBuffer.SetRenderTarget(virtualTexture.tableTextureID);
                    //绘制RenderTexture
                    pageRenderer.DrawPageTable(renderContext, cmdBuffer, pageProducer);
                    
                }
            }


            using (new ProfilingScope(cmdBuffer, m_DrawFeedbackSampler))
            {
                DrawingSettings drawSetting = new DrawingSettings(m_ShaderPassID, new SortingSettings(camera) { criteria = SortingCriteria.QuantizedFrontToBack })
                {
                    enableInstancing = true,
                };
                
                //设置feedBack的renderTarget为FeedbackTexture
                cmdBuffer.SetRenderTarget(m_FeedbackTextureID);
                cmdBuffer.ClearRenderTarget(true, true, Color.black);       
                // x: 页表大小(单位: 页)
                // y: 虚拟贴图大小(单位: 像素)
                // z: 最大mipmap等级
                // w: mipBias
                cmdBuffer.SetGlobalVector("_VTFeedbackParams", new Vector4(virtualTexture.pageSize, virtualTexture.pageSize * virtualTexture.tileSize * (1.0f / (float)m_FeedbackScale), virtualTexture.NumMip, 0.1f));
                
                float cameraAspect = (float) camera.pixelRect.width / (float) camera.pixelRect.height;
                Matrix4x4 projectionMatrix = Matrix4x4.Perspective(90, cameraAspect, camera.nearClipPlane, camera.farClipPlane);
                projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, true);
                RenderingUtils.SetViewAndProjectionMatrices(cmdBuffer, camera.worldToCameraMatrix, projectionMatrix, false);
                renderContext.ExecuteCommandBuffer(cmdBuffer);
                cmdBuffer.Clear();

                renderContext.DrawRenderers(renderingData.cullResults, ref drawSetting, ref m_FilterSetting);

                projectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                RenderingUtils.SetViewAndProjectionMatrices(cmdBuffer, camera.worldToCameraMatrix, projectionMatrix, false);
                renderContext.ExecuteCommandBuffer(cmdBuffer);
                cmdBuffer.Clear();
            }
            //read-back feedback,回读feedback
            m_FeedbackProcessor.RequestReadback(cmdBuffer, m_FeedbackTexture);

            using (new ProfilingScope(cmdBuffer, m_DrawPageColorSampler))
            {
                cmdBuffer.SetRenderTarget(virtualTexture.colorTextureIDs, virtualTexture.colorTextureIDs[0]);
                renderContext.ExecuteCommandBuffer(cmdBuffer);
                cmdBuffer.Clear();

                FDrawPageParameter drawPageParameter = VirtualTextureVolume.s_VirtualTextureVolume.GetDrawPageParamter();
                pageRenderer.DrawPageColor(renderContext, cmdBuffer, pageProducer, virtualTexture, ref virtualTexture.lruCache[0], drawPageParameter);
            }
        }


    }
}