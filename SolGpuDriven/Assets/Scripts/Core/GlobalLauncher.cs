
using System;
using UnityEngine;

public class GlobalLauncher : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        CoroutineManager.Update();
    }
}
