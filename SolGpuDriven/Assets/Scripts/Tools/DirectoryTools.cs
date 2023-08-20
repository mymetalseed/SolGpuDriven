using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DirectoryTools
{
    public static void CheckIfExistOrCreate(string path)
    {
        string newPath = Application.dataPath + "/" + path;
        Debug.Log(newPath);
        if (Directory.Exists(newPath))
        {
            
        }
        else
        {
            Directory.CreateDirectory(newPath);
        }
    }
}
