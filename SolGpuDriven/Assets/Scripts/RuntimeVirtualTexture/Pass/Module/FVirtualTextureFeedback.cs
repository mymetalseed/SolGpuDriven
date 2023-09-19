using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace RVT.Renderer
{
    internal unsafe class FVirtualTextureFeedback
    {
        internal bool isReady;
        internal NativeArray<Color32> readbackDatas;

        public FVirtualTextureFeedback(in bool bReady)
        {
            isReady = bReady;
        }
        /// <summary>
        /// 请求回读
        /// </summary>
        /// <param name="cmdBuffer"></param>
        /// <param name="feedbackTexture"></param>
        internal void RequestReadback(CommandBuffer cmdBuffer, RenderTexture feedbackTexture)
        {
            isReady = false;
            cmdBuffer.RequestAsyncReadback(feedbackTexture,0,feedbackTexture.graphicsFormat, EnqueueCopy);
        }

        private void EnqueueCopy(AsyncGPUReadbackRequest request)
        {
            if (request.hasError || request.done == true)
            {
                isReady = true;
                readbackDatas = request.GetData<Color32>();
            }
        }
    }
}