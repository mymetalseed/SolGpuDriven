using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(FeedbackRenderer))]
public class FeedbackRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            var renderer = (FeedbackRenderer)target;
            RVTUtil.DrawTexture(renderer.TargetTexture, "Feedback Texture");
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
}
#endif