#ifndef OCTREE_COMMON

//octree node type.
struct Node
{
    int child_flags;
    int children[8];
};
RWStructuredBuffer<Node> nodes;

//octree parameters
float3 size;
float3 min_corner;
int max_depth;

//Flag this node as having a particular child. Return true if
//this thread sets the flag.
bool activate_child(int node,int3 child_coords)
{
    int code = morton_encode(child_coords);
    int mask = 1 << code;
    int old_val;
    InterlockedOr(nodes[node].child_flags,mask,old_val);
    return !(old_val & mask);
}

//Returns the coords of the child of this node <x,y,z> in [0,1]
//on the path to the given leaf.
int3 get_child_coords(int3 node_coords,int node_depth,int3 leaf_coords)
{
    int3 level_coords = leaf_coords >> (max_depth - (node_depth + 1));
    int3 child_coords = level_coords - (node_coords << 1);
    return child_coords;
}

void set_child(int node,int child,int3 child_coords)
{
    int code = morton_encode(child_coords);
    nodes[node].children[code] = child;
}

int get_child(int node,int3 child_coords)
{
    int code = morton_encode(child_coords);
    int mask = 1 << code;
    return (nodes[node].child_flags & mask) ? nodes[node].children[code] : -1;
}

//确保在最大的包围盒范围内
bool in_bounds(int3 coords)
{
    int upper_bound = 1 << max_depth;
    return all(coords >= 0) && all(coords < upper_bound);
}

void traverse(int3 leaf_coords,int depth,out int node,out int3 node_coords)
{
    node = 0;
    node_coords = (int3)0;
    int node_depth = 0;
    while(node_depth != depth && node >= 0)
    {
        int3 child_coords = get_child_coords(node_coords,node_depth,leaf_coords);
        node = get_child(node,child_coords);
        node_coords = (node_coords << 1) + child_coords;
        node_depth++;
    }
}

#endif