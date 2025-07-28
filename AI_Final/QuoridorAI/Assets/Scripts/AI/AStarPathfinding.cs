using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, Grid grid)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        Node startNode = grid.GetNode(start);
        Node goalNode = grid.GetNode(goal);
        
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet, goalNode);
            if (currentNode == goalNode)
            {
                return RetracePath(startNode, currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (Node neighbor in grid.GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor) || !neighbor.walkable)
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, goalNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2Int>(); // Return an empty path if no path is found
    }

    private Node GetLowestFCostNode(List<Node> openSet, Node goalNode)
    {
        Node lowestFCostNode = openSet[0];
        foreach (Node node in openSet)
        {
            if (node.fCost < lowestFCostNode.fCost || (node.fCost == lowestFCostNode.fCost && node.hCost < lowestFCostNode.hCost))
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.gridPosition);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int dstY = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
        return dstX + dstY; // Manhattan distance
    }
}