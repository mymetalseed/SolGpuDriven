using UnityEngine;

public abstract class SceneDetectorBase : MonoBehaviour,IDetector
{
    public abstract bool UseCameraCulling { get; }
    public abstract bool IsDetected(Bounds bounds);

    public abstract int GetDetectedCode(float x, float y, float z, bool ignoreY);

    public Vector3 Position {
        get
        {
            return transform.position;
        }
    }
}