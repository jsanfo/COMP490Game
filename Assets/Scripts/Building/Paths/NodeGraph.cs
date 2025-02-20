using System.Collections.Generic;
using UnityEngine;

public class NodeGraph
{
    public bool nodesVisible = false;
    public bool[,] matrix = new bool[0, 0];
    private PathNode[] nodes = new PathNode[0];
    public PathNode[] Nodes { get => nodes; }

    public PathNode[] GetConnectedNodes(PathNode node)
    {
        int index = GetNodeIndex(node);

        if (index == -1) return new PathNode[0];

        PathNode[] connectedNodes = new PathNode[matrix.GetLength(1)];
        
        for (int i = 0; i < connectedNodes.Length; i++)
        {
            if (matrix[index, i]) connectedNodes[i] = nodes[i];
            }
        return connectedNodes;
    }

    public void AddPath(Path path)
    {
        PathNode[] pathNodes = path.nodes;

        List<PathNode> nodeList = new List<PathNode>(nodes);
        
        // Add nodes to array if node is new
        if (!nodeList.Contains(pathNodes[0])) AddNode(pathNodes[0]);
        if (!nodeList.Contains(pathNodes[1])) AddNode(pathNodes[1]);

        // Get index of nodes
        int index1 = GetNodeIndex(pathNodes[0]);
        int index2 = GetNodeIndex(pathNodes[1]);
        if (index1 == -1 || index2 == -1) return;

        // Modify matrix
        matrix[index1, index2] = true;
        matrix[index2, index1] = true;
    }

    // Returns path between two specified nodes
    public Path GetPath(int nodeIndex1, int nodeIndex2)
    {
        var node1 = nodes[nodeIndex1];
        var node2 = nodes[nodeIndex2];
        if (matrix[nodeIndex1, nodeIndex2] == true)
        {
            var paths = node1.GetPaths();
            for (int i = 0; i < paths.Length; i++)
            {
                var pathNodes = paths[i].nodes;
                if ((pathNodes[0] == node1 && pathNodes[1] == node2) || pathNodes[0] == node2 && pathNodes[1] == node1)
                    return paths[i];
            }
        }
        return null;
    }

    void AddNode(PathNode node)
    {
        List<PathNode> nodeList = new List<PathNode>(nodes);
        nodeList.Add(node);

        nodes = nodeList.ToArray();
        ExpandMatrix();
    }

    void ExpandMatrix()
    {
        bool[,] newMatrix = new bool[matrix.GetLength(0) + 1, matrix.GetLength(1) + 1];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                newMatrix[i, j] = matrix[i, j];
            }
        }

        matrix = newMatrix;
    }

    public int GetNodeIndex(PathNode node)
    {
        return new List<PathNode>(nodes).IndexOf(node);
    }

    public PathNode CheckExistingNode(Vector3 nodePosition)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].transform.position == nodePosition) return nodes[i];
        }

        return null;
    }

    public void SetNodesVisibility(bool visible)
    {
        nodesVisible = visible;

        if (nodes == null) return;

        if (nodes[0].gameObject.activeSelf)
        {
            nodes[0].gameObject.SetActive(false);
        }

        for (int i = 1; i < nodes.Length; i++)
        {
            if (nodesVisible)
            {
                nodes[i].ShowNode();
            }
            else
            {
                nodes[i].HideNode();
            }
        }
    }
}
