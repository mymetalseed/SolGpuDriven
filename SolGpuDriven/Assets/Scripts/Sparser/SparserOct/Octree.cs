using System;
using Sparser.Compact;
using Sparser.ComputeBufferEx;
using Sparser.Sort;
using UnityEngine;
using System.Threading.Tasks;

namespace Sparser
{
    public class Octree :IDisposable
    {
        internal unsafe struct Node
        {
            public int child_flags;
            public fixed int children[8];
        }

        private int maxDepth;
        private Bounds bounds;

        private ArgsBuffer indirectArgs;
        private CounterBuffer<Node> nodes;
        private Node[] nodeData;
        private int nodeCount;

        private static ComputeShader shader;
        private static int computeLeavesKernel;
        private static int markUniqueLeavesKernel;
        private static int computeArgsKernel;
        private static int subdivideKernel;

        private BitonicSort sorter = new BitonicSort();
        private ScanCompact compactor = new ScanCompact();

        static Octree()
        {
            shader = Resources.Load<ComputeShader>("Octree");
            computeLeavesKernel = shader.FindKernel("ComputeLeaves");
            markUniqueLeavesKernel = shader.FindKernel("MarkUniqueLeaves");
            computeArgsKernel = shader.FindKernel("ComputeArgs");
            subdivideKernel = shader.FindKernel("Subdivide");
        }

        public Octree(Bounds bounds, int maxDepth = 5)
        {
            indirectArgs = new ArgsBuffer();
            //计算最大的节点数量
            int maxNodes = 0;
            for (int i = 1; i <= maxDepth; ++i)
            {
                int res = 1 << i;
                maxNodes += res * res * res;
            }

            nodes = new CounterBuffer<Node>(maxNodes, 1);
            nodes.SetData(new Node[maxNodes]);

            this.bounds = bounds;
            this.maxDepth = maxDepth;
        }
        
        
        public void Dispose()
        {
            indirectArgs.Dispose();
            nodes.Dispose();
        }

        public async Task Insert(Mesh mesh)
        {
            await Insert(mesh.vertices);
        }

        public async Task Insert(Vector3[] data)
        {
            uint gx, gy, gz;
            shader.GetKernelThreadGroupSizes(computeLeavesKernel,out gx,out gy,out gz);
            int numGroupsX = Mathf.CeilToInt((float)data.Length / gx);
            
            using(var leaves = new StructuredBuffer<int>(data.Length))
            using(var leafCount = new RawBuffer<uint>(1))
            using(var keys = new CounterBuffer<int>(data.Length))
            using (var points = new StructuredBuffer<Vector3>(data.Length))
            {
                //先把所有的顶点数据存到structuredBuffer中
                points.SetData(data);
                //1. 最外圈包围盒大小
                shader.SetFloats("size", bounds.size.x, bounds.size.y, bounds.size.z);
                //三轴最小的那个顶点坐标
                shader.SetFloats("min_corner", bounds.min.x, bounds.min.y, bounds.min.z);
                //最大深度
                shader.SetInt("max_depth", maxDepth);
                //点的数量
                shader.SetInt("point_count", data.Length);
                //设置叶子节点Buffer
                shader.SetBuffer(computeLeavesKernel, "leaves", leaves.Buffer);
                //设置顶点Buffer
                shader.SetBuffer(computeLeavesKernel, "points", points.Buffer);
                //执行叶子核的计算
                shader.Dispatch(computeLeavesKernel, numGroupsX, 1, 1);
                
                sorter.Sort(leaves, data.Length);
                
                shader.SetBuffer(markUniqueLeavesKernel, "leaves", leaves.Buffer);
                shader.SetBuffer(markUniqueLeavesKernel, "unique", keys.Buffer);
                shader.Dispatch(markUniqueLeavesKernel, numGroupsX, 1, 1);

                compactor.Compact(leaves, keys, data.Length);
                
                keys.CopyCount(indirectArgs);
                shader.SetBuffer(computeArgsKernel, "args", indirectArgs.Buffer);
                shader.Dispatch(computeArgsKernel, 1, 1, 1);

                keys.CopyCount(leafCount);
                shader.SetBuffer(subdivideKernel, "leaf_count", leafCount.Buffer);
                shader.SetBuffer(subdivideKernel, "leaves", leaves.Buffer);
                shader.SetBuffer(subdivideKernel, "nodes", nodes.Buffer);
                for (int i = 0; i < maxDepth; i++) {
                    shader.SetInt("current_level", i);
                    shader.DispatchIndirect(subdivideKernel, indirectArgs.Buffer);
                }

                nodeData = await nodes.GetDataAsync();
                nodeCount = (int)nodes.GetCounterValue();
            }
        }
        
        public void Draw(float scale)
        {
            if (nodeData != null)
            {
                DrawNode(0,0,Vector3.zero,scale);
            }
        }

        private Color GetDepthColor(int depth)
        {
            if(depth == 1) return Color.black;
            if(depth == 2) return Color.blue;
            if(depth == 3) return Color.red;
            if(depth == 4) return Color.green;
            if(depth == 5) return Color.yellow;
            if(depth == 6) return Color.magenta;
            if(depth == 7) return Color.white;
            return Color.black;
        }
        
        private unsafe void DrawNode(int idx, int depth, Vector3 coords,float scale) {
            Node node = nodeData[idx];
            var nodeSize = bounds.size / (1 << depth) * scale;
            var center = bounds.min + Vector3.Scale(coords + 0.5f * Vector3.one, nodeSize);

            Gizmos.color = GetDepthColor(depth);
            Gizmos.DrawWireCube(center, nodeSize);

            for (int i = 0; i < 8; i++) {
                int child = node.children[i];
                if (child > 0 && child < nodeData.Length) {
                    Vector3 child_coords = Morton.Decode(i);
                    DrawNode(child, depth + 1, 2f * coords + child_coords,scale);
                }
            }
        }
    }
    

}