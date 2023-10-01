using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(TiledTexture))]
public class TileTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            var tileTexture = (TiledTexture)target;

            RVTUtil.DrawTexture(tileTexture.VTRTs[0], "Diffuse");
            RVTUtil.DrawTexture(tileTexture.VTRTs[1], "Normal");
            // Util.DrawTexture(tileTexture.VTs[1], "CompressedNormal");
            //RVTUtil.DrawTexture(tileTexture.VTs[0], "CompressedDiffuse");
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
}
#endif