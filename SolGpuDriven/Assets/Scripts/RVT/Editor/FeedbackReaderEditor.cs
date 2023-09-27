﻿using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(FeedbackReader))]
public class FeedbackReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            var reader = (FeedbackReader)target;
            RVTUtil.DrawTexture(reader.DebugTexture, "Mipmap Level Debug Texture");
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
}
#endif