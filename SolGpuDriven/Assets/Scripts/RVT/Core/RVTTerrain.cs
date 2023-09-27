using System;
using UnityEngine;
using UnityEngine.Profiling;

public class RVTTerrain : MonoBehaviour
{
    [Space] public Terrain terrain;
    // Feedback Pass Renderer & Reader
    private FeedbackReader _feedbackReader;
    private FeedbackRenderer _feedbackRenderer;
    
    private readonly int _feedbackInterval = 8;
    
    private void Start()
    {
        _feedbackReader = GetComponent<FeedbackReader>();
        _feedbackRenderer = GetComponent<FeedbackRenderer>();
    }

    private void Update()
    {
        Profiler.BeginSample("RVT");
        _feedbackReader.UpdateRequest();
        if (_feedbackReader.CanRead && Time.frameCount % _feedbackInterval == 0)
        {
            Profiler.BeginSample("Feedback Render");
            _feedbackRenderer.FeedbackCamera.Render();
            Profiler.EndSample();

            _feedbackReader.ReadbackRequest(_feedbackRenderer.TargetTexture);
        }

        Profiler.EndSample();
    }
}