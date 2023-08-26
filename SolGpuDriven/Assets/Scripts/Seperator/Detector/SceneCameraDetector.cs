using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 该触发器根据相机裁剪区域触发
/// </summary>
public class SceneCameraDetector : SceneDetectorBase
{
    protected Camera m_camera;

    public override bool UseCameraCulling
    {
        get
        {
            return true;
        }
    }

    private void Start()
    {
        m_camera = gameObject.GetComponent<Camera>();
    }

    public override bool IsDetected(Bounds bounds)
    {
        if (m_camera == null)
            return false;
        return bounds.IsBoundsInCamera(m_camera);
    }

    public override int GetDetectedCode(float x, float y, float z, bool ignoreY)
    {
        if (m_camera == null)
            return 0;
        Matrix4x4 matrix = m_camera.cullingMatrix;
        return CalculateCullCode(new Vector4(x, y, z, 1.0f), matrix);
    }

    protected virtual int CalculateCullCode(Vector4 position, Matrix4x4 matrix)
    {
        return MatrixEx.ComputeOutCode(position, matrix);
    }
    
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Camera camera = gameObject.GetComponent<Camera>();
        if(camera)
            GizmosEx.DrawViewFrustum(camera,Color.yellow);
    }

#endif
}