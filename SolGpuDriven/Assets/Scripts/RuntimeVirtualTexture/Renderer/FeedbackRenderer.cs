using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RVT.Renderer
{
    public enum EFeedbackScale
    {
        X1 = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8,
        X16 = 16
    }

    internal enum EVirtualTexturePass
    {
        VirtualTexture,
        DrawFeedback,
        DrawPageTable,
        DrawPageColor,
        CompressPage,
        CopyPageToPhyscis
    }

    public class FeedbackRenderer : ScriptableRendererFeature
    {
        [Header("Filter")]
        public LayerMask layerMask;
        
        [Header("Feedback")]
        public int2 feedbackSize;
        public EFeedbackScale feedbackScale;

        FeedbackRenderPass m_FeedbackRenderPass;

        public override void Create()
        {
            m_FeedbackRenderPass = new FeedbackRenderPass(layerMask, feedbackSize, feedbackScale);
            m_FeedbackRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_FeedbackRenderPass);
        }
    }
}