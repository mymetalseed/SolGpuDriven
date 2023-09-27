using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ScaleFactor
{
    One,
    Half,
    Quarter,
    Eighth,
}

public static class ScaleModeExtensions
{
    public static float ToFloat(this ScaleFactor mode)
    {
        switch (mode)
        {
            case ScaleFactor.Eighth:
                return 0.125f;
            case ScaleFactor.Quarter:
                return 0.25f;
            case ScaleFactor.Half:
                return 0.5f;
        }

        return 1.0f;
    }
}


public static class RVTUtil
{
    /// <summary>
    /// 在Editor面板绘制Texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="label"></param>
    public static void DrawTexture(Texture texture, string label = null)
    {
#if UNITY_EDITOR
        if(texture == null)
        {
            return;
        }
        EditorGUILayout.Space();
        if(!string.IsNullOrEmpty(label)) EditorGUILayout.LabelField(label);
        EditorGUILayout.LabelField($"Size: {texture.width} X {texture.height}");
        EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetAspectRect(texture.width / (float)texture.height),texture);
#endif
    }
}
