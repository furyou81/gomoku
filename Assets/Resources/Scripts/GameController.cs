using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameController
{
    public static bool enlabledDebug = false;
    public static int nbCapturePlayer = 0;
    public static int nbCaptureIA = 0;
    public static int DEPTH = 5;

    public static bool captureEnabled = true;
    public static bool doubleFreeThreeEnabled = false;
    static int[,,] ZobristTable = new int[19, 19, 3];


    void initTable()
    {
        for (int i = 0; i < 19; i++)
            for (int j = 0; j < 19; j++)
                for (int k = 0; k < 3; k++)
                    ZobristTable[i, j, k] = Random.Range(int.MinValue, int.MaxValue);
    }

    public int computeHash(byte[,] board)
    {
        int h = 0;
        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 19; j++)
            {
                if (board[i, j] != 0)
                {
                    h ^= ZobristTable[i, j, board[i, j]];
                }
            }
        }
        return h;
    }

    public static int getNewHash(int hash, StonePosition stone, List<StonePosition> captured)
    {
        foreach (StonePosition s in captured)
        {
            hash ^= ZobristTable[s.row, s.col, 0];
        }
        hash ^= ZobristTable[stone.row, stone.col, stone.playerId];
        return hash;

    }

	public Node StartGomoku(byte[,] grid, List<StonePosition> stonePositions) {
        initTable();
        int defaultHash = computeHash(grid);
        
        Node node = new Node(stonePositions, grid, null, defaultHash, nbCapturePlayer, nbCaptureIA);
        Node result = node.minimax(node, GameController.DEPTH, true, int.MinValue, int.MaxValue);
        Node toBeReturn = result;
      
        int i = 0;
        while (toBeReturn.parent.parent != null)
        {
            i++;
            toBeReturn = toBeReturn.parent;
        }

        return toBeReturn;
	}

    void print(byte[,] grid, int depth)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("DEPTH: " + depth);
        sb.AppendLine("");
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                sb.Append((char)(grid[i, j] + 48));
            }
            sb.AppendLine("");
        }
        Debug.Log(sb.ToString());
    }
}
