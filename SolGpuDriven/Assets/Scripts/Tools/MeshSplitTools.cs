using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshSplitTools : MonoBehaviour
{
    [SerializeField]
    private MeshFilter mf;
    
    [ContextMenu("拆分SubMesh")]
    private void MeshSplit()
    {
        var mesh = mf.sharedMesh;
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.name = "[NewSplitMesh]";

        if (mesh)
        {
            int subMeshCount = mesh.subMeshCount;
            if (subMeshCount > 1)
            {
                for (int i = 0; i < subMeshCount; ++i)
                {
                    CombineInstance[] combine = new CombineInstance[1];
                    combine[0].mesh = mesh;
                    combine[0].transform = Matrix4x4.identity;
                    combine[0].subMeshIndex = i;
                    
                    
                    GameObject subMeshObj = new GameObject();
                    subMeshObj.transform.parent = obj.transform;
                    subMeshObj.transform.localPosition = Vector3.zero;
                    subMeshObj.transform.localScale = Vector3.one;
                    subMeshObj.name = mesh.name + "_" + i;
                    MeshFilter nmf = subMeshObj.AddComponent<MeshFilter>();
                    MeshRenderer mr = subMeshObj.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
                    
                    
                    nmf.sharedMesh = new Mesh();
                    nmf.sharedMesh.CombineMeshes(combine);
                    string name = mesh.name + "_" + i + ".mesh";
                    string path = "SplitMesh/" + mesh.name + "/";
                    DirectoryTools.CheckIfExistOrCreate(path);
                    AssetDatabase.CreateAsset(nmf.sharedMesh,"Assets/"+path+name);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
