
using System;
using UnityEngine;

namespace Sparser
{
    [RequireComponent(typeof(MeshFilter))]
    public class OctreeLoader : MonoBehaviour
    {
        public int maxDepth = 2;
        private Octree octree;

        async void Start()
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            octree = new Octree(mesh.bounds, maxDepth);
            await octree.Insert(mesh);
        }

        private void OnDestroy()
        {
            octree.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying) {
                octree.Draw(transform.localScale.x);
            }
        }
    }
}
