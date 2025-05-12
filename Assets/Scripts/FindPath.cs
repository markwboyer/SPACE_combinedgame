using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class FindPath   
{
    static double Heuristic(Node_Lfant coord, float[,] map, int[] end, int mult)
    {


        if (coord.y > 360 || coord.y < 10)
        {
            return 9000000000;
        }

        // Favor the distance between two points not changing much
        float diff = map[coord.x,coord.y] - map[coord.cameFrom.x,coord.cameFrom.y];
        diff = Mathf.Abs(diff * mult); // TODO was 1000 - heuristic adjustment will occur here
        double dist = Math.Sqrt(Mathf.Pow((coord.x - end[0]),2) + Mathf.Pow((coord.y - end[1]),2));
        return Mathf.Pow(diff,2);
        //return dist * 10 + diff * 500; // Use 2000+ for a decent path
        //return diff * 2000;
        // Favor staying on flat, low ground instead of higher ground
        // return (Math.Sqrt(Mathf.Pow((coord.x - end[0]),2) + Mathf.Pow((coord.y - end[1]),2)) + (map[coord.x,coord.y] * 500));
    }

    static List<int[]> ReconstructPath(Node_Lfant[,] nodes, Node_Lfant finalPoint)
    {
        List<int[]> finalPath = new List<int[]>();
        finalPath.Add(new int[] {finalPoint.x, finalPoint.y});
        Node_Lfant current = finalPoint;
        while (current.cameFrom != null) {
            int[] point = {current.x, current.y};
            string pointStr = current.x + "," + current.y;
            current = nodes[current.x,current.y].cameFrom; //nodes[point].cameFrom
            //Debug.Log("New traceback x and y" + current.x + " " + current.y);
            finalPath.Add(new int[] {current.x, current.y});
        }
        return finalPath; //need to reverse
    }

    public static bool KeyExists(Dictionary<int[], Node_Lfant> dict ,int[] key)
    {
        foreach (KeyValuePair<int[], Node_Lfant> kvp in dict)
        {
            if (kvp.Key[0] == key[0] && kvp.Key[1] == key[1])
            {
                return true;
            }
        }
        return false;
    }

    public static Node_Lfant getNode(int[] key, Dictionary<int[], Node_Lfant> dict)
    {
        foreach (KeyValuePair<int[], Node_Lfant> kvp in dict)
        {
            if (kvp.Key[0] == key[0] && kvp.Key[1] == key[1])
            {
                return kvp.Value;
            }
        }
        return null;
    }

    public static List<int[]> AStar(int[] start, int[] end, float[,] map, int diff)
    {
        PriorityQueue pq = new PriorityQueue(); 
        //Dictionary<string, Node_Lfant> dict = new Dictionary<string, Node_Lfant>();
        Node_Lfant[,] nodeArr = new Node_Lfant[369,369];

        Node_Lfant startNode = new Node_Lfant(start[0],start[1]);
        string coordStr = start[0] + ","+ start[1];
        startNode.cameFrom = null;
        startNode.pathLength = 0;
        startNode.weight = 650; //Heuristic(startNode, map, end);

        startNode.isInPq = true;
        pq.Enqueue(startNode);
        //dict.Add(coordStr, startNode);
        nodeArr[start[0],start[1]] = startNode;

        int ctr = 0;

        // Stores different heuristics for each difficulty
        // Accounts for different terrain shapes and how a player will navigate
        int[] heuristics = {100,100,100,100,110,120,130,140}; 
        int usedHeuristic = heuristics[diff-1];

        while (pq.Count() > 0) // Continue as long as there are elements left to be processed pq.Count() > 0
        {
            Node_Lfant current = pq.Dequeue();
            nodeArr[current.x,current.y].isInPq = false;
            // Debug.Log("Current x: " + current.x + " Current y: " + current.y);

            if (current.x == end[0] && current.y == end[1])
            {
                return ReconstructPath(nodeArr,current);
            }
            int[,] arr = {{current.x+1,current.y},{current.x-1,current.y},{current.x,current.y+1},{current.x,current.y-1}};
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                if (arr[i,0] > 368 ||  arr[i,0] < 0 || arr[i,1] > 368 || arr[i,1] < 0) {
                }
                else
                {
                    //Debug.Log("Trying x y:" + arr[i,0] + " " + arr[i,1]);
                    int[] currentPoint = {arr[i,0], arr[i, 1]};
                    float height = map[arr[i,0], arr[i, 1]];
                    coordStr = currentPoint[0] + "," + currentPoint[1];
                    //Debug.Log("Dict contains key: " + currentPoint[0] + " " + currentPoint[1] + " is " + dict.ContainsKey(coordStr));
                    if (nodeArr[currentPoint[0],currentPoint[1]] == null || current.pathLength+1 < nodeArr[currentPoint[0],currentPoint[1]].pathLength)
                    {
                        Node_Lfant neighbor = new Node_Lfant(arr[i,0], arr[i,1]);
                        neighbor.cameFrom = current;
                        neighbor.pathLength = current.pathLength + 1;
                        neighbor.weight = neighbor.pathLength + Heuristic(neighbor, map, end, usedHeuristic);
                        if (nodeArr[neighbor.x,neighbor.y] == null || !nodeArr[neighbor.x,neighbor.y].isInPq) {
                            //Debug.Log("8");
                            neighbor.isInPq = true;
                            pq.Enqueue(neighbor);
                            nodeArr[currentPoint[0],currentPoint[1]] = neighbor;
                        }
                        else
                        {
                            //Debug.Log("Copied x y coords: " + neighbor.x + " " + neighbor.y);
                        }
                    }
                    }  
            }
            ctr++;
        }
        return null;
    }
        
}


public class Node_Lfant  {
    public int x,y;
    public double weight;
    public int pathLength;
    public Node_Lfant cameFrom;
    public bool isInPq = false;

    public Node_Lfant(int _x, int _y) {
        this.x = _x;
        this.y = _y;
    }

    public int CompareTo(Node_Lfant otherNode) {
        if (this.weight > otherNode.weight)
            return 1;
        else if (this.weight < otherNode.weight)
            return -1;
        else
            return 0;
    }
}

public class PriorityQueue {
	private List<Node_Lfant> data;

	public PriorityQueue() {
		this.data = new List<Node_Lfant>();
	}

	public void Enqueue(Node_Lfant item) {
		data.Add(item);
		int ci = data.Count - 1; // child index; start at end
		while (ci > 0) {
			int pi = (ci - 1) / 2; // parent index
			if (data[ci].CompareTo(data[pi]) >= 0)
				break; // child item is larger than (or equal) parent so we're done
			Node_Lfant tmp = data[ci];
			data[ci] = data[pi];
			data[pi] = tmp;
			ci = pi;
		}
	}

	public Node_Lfant Dequeue() {
		// assumes pq is not empty; up to calling code
		int li = data.Count - 1; // last index (before removal)
		Node_Lfant frontItem = data[0];   // fetch the front
		data[0] = data[li];
		data.RemoveAt(li);

		--li; // last index (after removal)
		int pi = 0; // parent index. start at front of pq
		while (true) {
			int ci = pi * 2 + 1; // left child index of parent
			if (ci > li)
				break;  // no children so done
			int rc = ci + 1;     // right child
			if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                ci = rc;
			if (data[pi].CompareTo(data[ci]) <= 0)
				break; // parent is smaller than (or equal to) smallest child so done
			Node_Lfant tmp = data[pi];
			data[pi] = data[ci];
			data[ci] = tmp; // swap parent and child
			pi = ci;
		}
		return frontItem;
	}

	public Node_Lfant Peek() {
		Node_Lfant frontItem = data[0];
		return frontItem;
	}

	public int Count() {
		return data.Count;
	}

    public bool Contains(Node_Lfant item) {
        return data.Contains(item);
    }

	public override string ToString() {
		string s = "";
		for (int i = 0; i < data.Count; ++i)
			s += data[i].ToString() + " ";
		s += "count = " + data.Count;
		return s;
	}
    public bool ContainsArr(int x, int y) {
        for (int i = 0; i < data.Count; i++) {
            if (data[i].x == x && data[i].y == y) {
                return true;
            }
        }
        return false;
    }

	public bool IsConsistent() {
		// is the heap property true for all data?
		if (data.Count == 0)
			return true;
		int li = data.Count - 1; // last index
		for (int pi = 0; pi < data.Count; ++pi) { // each parent index
			int lci = 2 * pi + 1; // left child index
			int rci = 2 * pi + 2; // right child index

			if (lci <= li && data[pi].CompareTo(data[lci]) > 0)
				return false; // if lc exists and it's greater than parent then bad.
			if (rci <= li && data[pi].CompareTo(data[rci]) > 0)
				return false; // check the right child too.
		}
		return true; // passed all checks
	}
	// IsConsistent
}
 // PriorityQueue