using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public struct StonePosition
{
    public sbyte row;
    public sbyte col;
    public byte playerId;

    public bool captured;
    
    public int position;
}

public class Node
{
    private byte[,] grid = new byte[Utils.SIZE_GRID, Utils.SIZE_GRID];

    public Node parent;
    public List<StonePosition> positions;
    private int hash;
    static int visited = 0;
    private int nbCapturePlayer;
    private int nbCaptureIA;
    private int currentScore = 0;
    private int captureScore = 0;

    public int Score
    {
        get;
        set;
    }

    public byte[,] getGrid() {
        return this.grid;
    }

    public Node(bool isMax)
    {
        this.Score = isMax ? int.MinValue : int.MaxValue;
    }

    public Node(List<StonePosition> positions, byte[,] grid, Node parent, int hash, int nbCapturePlayer, int nbCaptureIA)
    {
        this.positions = positions;
        this.grid = grid;
        this.parent = parent;
        this.hash = hash;
        this.nbCapturePlayer = nbCapturePlayer;
        this.nbCaptureIA = nbCaptureIA;
    }

    private Node checkPriorityMoves(Node currentNode, int depth)
    {
        if (((currentNode.currentScore + currentNode.captureScore) >= 250000 * 2 || (currentNode.currentScore + currentNode.captureScore) <= -449000 * 2) && (GameController.DEPTH - depth) >= 1)
        {
            currentNode.Score = currentNode.currentScore + currentNode.captureScore - (100 * (GameController.DEPTH - depth));
            return currentNode;
        }
        return null;
    }

    public Node minimax(Node currentNode, int depth, bool isMax, int alpha, int beta)
    {
        byte playerId = isMax ? (byte)2 : (byte)1;

        Node priorityMove = checkPriorityMoves(currentNode, depth);
        if (priorityMove != null)
        {
            return priorityMove;
        }

        if (depth == 0)
        {
            currentNode.Score = calculate(currentNode, playerId, GameController.DEPTH - depth);
            return currentNode;
        }

        List<Node> sortedChildren = findChildren(playerId, currentNode, depth);
        int sortedChildrenCount = sortedChildren.Count;

        if (sortedChildrenCount == 0)
        {
            currentNode.Score = calculate(currentNode, playerId, GameController.DEPTH - depth);
            return currentNode;
        }

        Node choosen = new Node(isMax);
        Node minimaxReturn = null;
        int cut = GameController.DEPTH > 5 ? 4 : 2;

        for (int i = 0; i < sortedChildrenCount / cut; i++)
        {
            Node child = sortedChildren[i];
            Node.visited += 1;

            if (isMax)
            {
                minimaxReturn = minimax(child, depth - 1, false, alpha, beta);
                if (minimaxReturn.Score > choosen.Score)
                {
                    choosen = minimaxReturn;
                }
                if (choosen.Score >= beta) {
                    break;
                }
                alpha = Math.Max(alpha, choosen.Score);
            }
            else
            {
                minimaxReturn = minimax(child, depth - 1, true, alpha, beta);
                if (minimaxReturn.Score < choosen.Score)
                {
                    choosen = minimaxReturn;
                }
                if (alpha >= choosen.Score) {
                    break;
                }
                beta = Math.Min(beta, choosen.Score);
            }
        }
        return choosen;
    }

    // Trouve les noeuds enfants
    public List<Node> findChildren(byte playerId, Node currentNode, int depth)
    {
        Dictionary<int, Node> children = new Dictionary<int, Node>();

        foreach (StonePosition stone in currentNode.positions)
        {
            // CHECKIF IMPORTANT
            if (GameController.DEPTH == depth) {
                if (!hasNeighboors(stone, currentNode.grid)) {
                    continue;
                }
            }

            for (int y = -2; y <= 2; y++)
            {
                for (int x = -2; x <= 2; x++)
                {
                    if ((x == 0 && y == 0) || stone.row + y < 0 || stone.row + y > 18 || stone.col + x < 0 || stone.col + x > 18 || currentNode.grid[stone.row + y, stone.col + x] != 0)
                    {
                        continue;
                    }
                    byte[,] copyGrid = currentNode.grid.Clone() as byte[,];
                    copyGrid[stone.row + y, stone.col + x] = playerId; // Place la pierre pour le nouvel etat

                    StonePosition newStone = stone;
                    newStone.row = (sbyte)(stone.row + y);
                    newStone.col = (sbyte)(stone.col + x);
                    newStone.playerId = playerId;

                    List<StonePosition> copyPosition = new List<StonePosition>(currentNode.positions);
                    List<StonePosition> capturedStones = checkCapture(newStone, copyGrid);

                    foreach (StonePosition captured in capturedStones)
                    {
                        copyGrid[captured.row, captured.col] = 0;
                        copyPosition.Remove(captured);
                    }

                    copyPosition.Add(newStone);

                    int h = GameController.getNewHash(currentNode.hash, newStone, capturedStones);

                    Node child = new Node(copyPosition, copyGrid, currentNode, h, currentNode.nbCapturePlayer, currentNode.nbCaptureIA);

                    if (playerId == 1)
                    {
                        child.nbCapturePlayer = currentNode.nbCapturePlayer + (capturedStones.Count / 2);
                    }
                    else
                    {
                        child.nbCaptureIA = currentNode.nbCaptureIA + (capturedStones.Count / 2);
                    }

                    int[] result = isForbidden(child.grid, child.positions[child.positions.Count - 1], playerId);
                    if (result[0] == 0)
                    {
                        float ponderate = GameController.DEPTH / (GameController.DEPTH - depth + 1);
                        child.currentScore = (int)(currentNode.currentScore + result[1] * ponderate);
                        int v1 = capturateScore(child.nbCapturePlayer, GameController.nbCapturePlayer, 1);
                        int v2 = capturateScore(child.nbCaptureIA, GameController.nbCaptureIA, 2);
                        int cap = defCapture(child.positions[child.positions.Count - 1], child.grid);
                        child.captureScore = (int)(currentNode.captureScore + (v1 + v2 + cap) * ponderate);

                        child.currentScore += defCapture(currentNode.positions[currentNode.positions.Count - 1], currentNode.grid);
                        child.Score = currentNode.currentScore + result[1];
                        children[child.hash] = child;
                    }
                }
            }
        }

        List<Node> sortedChildren = new List<Node>(children.Values);
        if (playerId == 2)
        {
            sortedChildren.Sort((x, y) => y.Score.CompareTo(x.Score));
        }
        else
        {
            sortedChildren.Sort((x, y) => x.Score.CompareTo(y.Score));
        }
        return sortedChildren;
    }

    public static bool hasNeighboors(StonePosition stone, byte[,] grid) {
        
        if (stone.playerId == 2) {
            return true;
        }
        for (int y = -2; y <= 2; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                if ((x == 0 && y == 0) || stone.row + y < 0 || stone.row + y > 18 || stone.col + x < 0 || stone.col + x > 18)
                {
                    continue;
                }
                if (grid[stone.row + y, stone.col + x] != 0) {
                    return true;
                }
            }
        }
        return false;
    }
    

    public static List<StonePosition> checkCapture(StonePosition stone, byte[,] currentGrid)
    {
        ushort b = 0;

        byte opponentId = stone.playerId == 1 ? (byte)2 : (byte)1;
        List<StonePosition> capturedStones = new List<StonePosition>();
        if (!GameController.captureEnabled) {
            return capturedStones;
        }
        for (sbyte c = -3; c <= 0; c++)
        {
            if (stone.col + c >= 0 && stone.col + c <= (18 - 3))
            {
                // patterns in row
                b = (ushort)((currentGrid[stone.row, stone.col + c + 0] << 6)
                    + (currentGrid[stone.row, stone.col + c + 1] << 4)
                    + (currentGrid[stone.row, stone.col + c + 2] << 2)
                    + (currentGrid[stone.row, stone.col + c + 3]));
                if (isCaptured(b, stone))
                {
                    capturedStones.Add(new StonePosition() { playerId = opponentId, row = stone.row, col = (sbyte)(stone.col + c + 1) });
                    capturedStones.Add(new StonePosition() { playerId = opponentId, row = stone.row, col = (sbyte)(stone.col + c + 2) });
                }

                // patterns in diagonal up left -> bottom right
                if (stone.row + c >= 0 && stone.row + c <= (18 - 3))
                {
                    b = (ushort)((currentGrid[stone.row + c + 0, stone.col + c + 0] << 6)
                        + (currentGrid[stone.row + c + 1, stone.col + c + 1] << 4)
                        + (currentGrid[stone.row + c + 2, stone.col + c + 2] << 2)
                        + (currentGrid[stone.row + c + 3, stone.col + c + 3]));
                    if (isCaptured(b, stone))
                    {
                        capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row + c + 1), col = (sbyte)(stone.col + c + 1) });
                        capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row + c + 2), col = (sbyte)(stone.col + c + 2) });
                    }
                }

                // patterns in diagonal bottom left -> up right
                if (stone.row - c >= 3 && stone.row - c < 18)
                {
                    b = (ushort)((currentGrid[stone.row - c - 0, stone.col + c + 0] << 6)
                        + (currentGrid[stone.row - c - 1, stone.col + c + 1] << 4)
                        + (currentGrid[stone.row - c - 2, stone.col + c + 2] << 2)
                        + (currentGrid[stone.row - c - 3, stone.col + c + 3]));
                    if (isCaptured(b, stone))
                    {
                        capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row - c - 1), col = (sbyte)(stone.col + c + 1) });
                        capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row - c - 2), col = (sbyte)(stone.col + c + 2) });
                    }
                }
            }

            // patterns in column
            if (stone.row + c >= 0 && stone.row + c <= (18 - 3))
            {
                b = (ushort)((currentGrid[stone.row + c + 0, stone.col] << 6)
                    + (currentGrid[stone.row + c + 1, stone.col] << 4)
                    + (currentGrid[stone.row + c + 2, stone.col] << 2)
                    + (currentGrid[stone.row + c + 3, stone.col]));
                if (isCaptured(b, stone))
                {
                    capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row + c + 1), col = stone.col });
                    capturedStones.Add(new StonePosition() { playerId = opponentId, row = (sbyte)(stone.row + c + 2), col = stone.col });
                }
            }
        }
        return capturedStones;
    }

    private static bool isDefCaptured(ushort pattern, StonePosition stone, int posInPattern)
    {
        int test1 = 0;
        int test2 = 0;
        int opponent = stone.playerId == 1 ? 2 : 1;

        if ((uint)(pattern << 24) >> (24 + 6) == opponent)
        {
            test1++;
        }
        if ((uint)(pattern << 26) >> (26 + 4) == stone.playerId)
        {
            test1++;
        }
        if ((uint)(pattern << 28) >> (28 + 2) == stone.playerId)
        {
            test1++;
        }
        if ((uint)(pattern << 30) >> (30) == (uint)(stone.playerId))
        {
            test1++;
        }
        
        if ((uint)(pattern << 24) >> (24 + 6) == stone.playerId)
        {
            test2++;
        }
        if ((uint)(pattern << 26) >> (26 + 4) == stone.playerId)
        {
            test2++;
        }
        if ((uint)(pattern << 28) >> (28 + 2) == stone.playerId)
        {
            test2++;
        }
        if ((uint)(pattern << 30) >> (30) == (uint)(opponent))
        {
            test2++;
        }
        return (test1 == 4 && posInPattern == 3) || (test2 == 4 && posInPattern == 0);
    }

    public static int defCapture(StonePosition stone, byte[,] currentGrid)
    {
        ushort b = 0;

        int value = 0;
        for (sbyte c = -3; c <= 0; c++)
        {
            if (stone.col + c >= 0 && stone.col + c <= (18 - 3))
            {
                // patterns in row
                b = (ushort)((currentGrid[stone.row, stone.col + c + 0] << 6)
                    + (currentGrid[stone.row, stone.col + c + 1] << 4)
                    + (currentGrid[stone.row, stone.col + c + 2] << 2)
                    + (currentGrid[stone.row, stone.col + c + 3]));
                if (isDefCaptured(b,  stone, 0  -c))
                {
                    value += 2000000;
                }

                // patterns in diagonal up left -> bottom right
                if (stone.row + c >= 0 && stone.row + c <= (18 - 3))
                {
                    b = (ushort)((currentGrid[stone.row + c + 0, stone.col + c + 0] << 6)
                        + (currentGrid[stone.row + c + 1, stone.col + c + 1] << 4)
                        + (currentGrid[stone.row + c + 2, stone.col + c + 2] << 2)
                        + (currentGrid[stone.row + c + 3, stone.col + c + 3]));
                    if (isDefCaptured(b,  stone, 0  -c))
                    {
                        value += 2000000;
                    }
                }

                // patterns in diagonal bottom left -> up right
                if (stone.row - c >= 3 && stone.row - c < 18)
                {
                    b = (ushort)((currentGrid[stone.row - c - 0, stone.col + c + 0] << 6)
                        + (currentGrid[stone.row - c - 1, stone.col + c + 1] << 4)
                        + (currentGrid[stone.row - c - 2, stone.col + c + 2] << 2)
                        + (currentGrid[stone.row - c - 3, stone.col + c + 3]));
                    if (isDefCaptured(b,  stone, 0  -c))
                    {
                        value += 2000000;
                    }
                }
            }

            // patterns in column
            if (stone.row + c >= 0 && stone.row + c <= (18 - 3))
            {
                b = (ushort)((currentGrid[stone.row + c + 0, stone.col] << 6)
                    + (currentGrid[stone.row + c + 1, stone.col] << 4)
                    + (currentGrid[stone.row + c + 2, stone.col] << 2)
                    + (currentGrid[stone.row + c + 3, stone.col]));
                if (isDefCaptured(b,  stone, 0  -c))
                {
                    value += 2000000;
                }
            }
        }
        return stone.playerId == 2 ? value : -value;
    }


    private static bool isCaptured(ushort pattern, StonePosition stone)
    {
        int test = 0;
        int opponent = stone.playerId == 1 ? 2 : 1;

        if ((uint)(pattern << 24) >> (24 + 6) == stone.playerId)
        {
            test++;
        }
        if ((uint)(pattern << 26) >> (26 + 4) == opponent)
        {
            test++;
        }
        if ((uint)(pattern << 28) >> (28 + 2) == opponent)
        {
            test++;
        }
        if ((uint)(pattern << 30) >> (30) == (uint)(stone.playerId))
        {
            test++;
        }
        return test == 4;
    }

    // Calcul le score des noeuds finaux
    public int calculate(Node currentNode, byte playerId, int turnPlayed)
    {
        List<ushort> patterns = findPatterns(currentNode, turnPlayed);
        int value = 0;

        foreach (ushort pattern in patterns)
        {
            value += applyHeuritic(pattern, playerId);
        }

        int v1 = capturateScore(currentNode.nbCapturePlayer, GameController.nbCapturePlayer, 1);
        int v2 = capturateScore(currentNode.nbCaptureIA, GameController.nbCaptureIA, 2);
        int cap = defCapture(currentNode.positions[currentNode.positions.Count - 1], currentNode.grid);

        value = value + v1 + v2 + cap;
        return value;
    }

    private int capturateScore(int newNbCapture, int oldNbCapture, byte playerId)
    {
        int value = 0;
        int nbCaptures = newNbCapture - oldNbCapture;
        for (int i = 1; i <= nbCaptures; i++)
        {
            if (i == 1)
            {
                value += 4000000;
            }
            else if (i == 2)
            {
                value += 6000000;
            }
            else if (i == 3)
            {
                value += 8000000;
            }
            else if (i == 4)
            {
                value += 16000000;
            }
            else if (i == 5)
            {
                value += Utils.VICTORY;
            }
        }
        value = playerId == 2 ? value : -value;

        return value;
    }

    public static List<ushort> findPatterns(Node currentNode, int turnPlayed)
    {
        List<ushort> patterns = new List<ushort>();
        ushort b = 0;
        for (int i = 0; i < turnPlayed; i++)
        {
            StonePosition stone = currentNode.positions[currentNode.positions.Count - 1 - i];
            for (sbyte c = -4; c <= 0; c++)
            {
                if (stone.col + c >= 0 && stone.col + c <= (18 - 4))
                {
                    // patterns in row
                    b = (ushort)((currentNode.grid[stone.row, stone.col + c + 0] << 8)
                        + (currentNode.grid[stone.row, stone.col + c + 1] << 6)
                        + (currentNode.grid[stone.row, stone.col + c + 2] << 4)
                        + (currentNode.grid[stone.row, stone.col + c + 3] << 2)
                        + (currentNode.grid[stone.row, stone.col + c + 4]));
                    patterns.Add(b);

                    // patterns in diagonal up left -> bottom right
                    if (stone.row + c >= 0 && stone.row + c <= (18 - 4))
                    {
                        b = (ushort)((currentNode.grid[stone.row + c + 0, stone.col + c + 0] << 8)
                            + (currentNode.grid[stone.row + c + 1, stone.col + c + 1] << 6)
                            + (currentNode.grid[stone.row + c + 2, stone.col + c + 2] << 4)
                            + (currentNode.grid[stone.row + c + 3, stone.col + c + 3] << 2)
                            + (currentNode.grid[stone.row + c + 4, stone.col + c + 4]));
                        patterns.Add(b);
                    }

                    // patterns in diagonal bottom left -> up right
                    if (stone.row - c >= 4 && stone.row - c <= 18)
                    {
                        b = (ushort)((currentNode.grid[stone.row - c - 0, stone.col + c + 0] << 8)
                            + (currentNode.grid[stone.row - c - 1, stone.col + c + 1] << 6)
                            + (currentNode.grid[stone.row - c - 2, stone.col + c + 2] << 4)
                            + (currentNode.grid[stone.row - c - 3, stone.col + c + 3] << 2)
                            + (currentNode.grid[stone.row - c - 4, stone.col + c + 4]));
                        patterns.Add(b);
                    }
                }

                // patterns in column
                if (stone.row + c >= 0 && stone.row + c <= (18 - 4))
                {
                    b = (ushort)((currentNode.grid[stone.row + c + 0, stone.col] << 8)
                        + (currentNode.grid[stone.row + c + 1, stone.col] << 6)
                        + (currentNode.grid[stone.row + c + 2, stone.col] << 4)
                        + (currentNode.grid[stone.row + c + 3, stone.col] << 2)
                        + (currentNode.grid[stone.row + c + 4, stone.col]));
                    patterns.Add(b);
                }
            }
            
        }

        return patterns;
    }

    public static int[] isForbidden(byte[,] grid, StonePosition stone, int playerId)
    {
        int col = 0;
        int row = 0;
        int ulbr = 0;
        int blur = 0;
        ushort b = 0;
        int h = 0;
        int sum = 0;

        for (sbyte c = -4; c <= 0; c++)
        {
            if (stone.col + c >= 0 && stone.col + c <= (18 - 4))
            {
                // patterns in row
                b = (ushort)((grid[stone.row, stone.col + c + 0] << 8)
                    + (grid[stone.row, stone.col + c + 1] << 6)
                    + (grid[stone.row, stone.col + c + 2] << 4)
                    + (grid[stone.row, stone.col + c + 3] << 2)
                    + (grid[stone.row, stone.col + c + 4]));

                h = applyHeuritic(b, playerId);
                sum = sum + h;
                if (h == 320000 || h == 250000 || h == -520000 || h == -449000)
                {
                    row++;
                }

                // patterns in diagonal up left -> bottom right
                if (stone.row + c >= 0 && stone.row + c <= (18 - 4))
                {
                    b = (ushort)((grid[stone.row + c + 0, stone.col + c + 0] << 8)
                            + (grid[stone.row + c + 1, stone.col + c + 1] << 6)
                            + (grid[stone.row + c + 2, stone.col + c + 2] << 4)
                            + (grid[stone.row + c + 3, stone.col + c + 3] << 2)
                            + (grid[stone.row + c + 4, stone.col + c + 4]));

                    h = applyHeuritic(b, playerId);
                    sum = sum + h;
                    if (h == 320000 || h == 250000 || h == -520000 || h == -449000)
                    {
                        ulbr++;
                    }
                }

                // patterns in diagonal bottom left -> up right
                if (stone.row - c >= 4 && stone.row - c < 18)
                {
                    b = (ushort)((grid[stone.row - c - 0, stone.col + c + 0] << 8)
                        + (grid[stone.row - c - 1, stone.col + c + 1] << 6)
                        + (grid[stone.row - c - 2, stone.col + c + 2] << 4)
                        + (grid[stone.row - c - 3, stone.col + c + 3] << 2)
                        + (grid[stone.row - c - 4, stone.col + c + 4]));
                    h = applyHeuritic(b, playerId);
                    sum = sum + h;
                    if (h == 320000 || h == 250000 || h == -520000 || h == -449000)
                    {
                        blur++;
                    }
                }
            }

            // patterns in column
            if (stone.row + c >= 0 && stone.row + c <= (18 - 4))
            {
                b = (ushort)((grid[stone.row + c + 0, stone.col] << 8)
                    + (grid[stone.row + c + 1, stone.col] << 6)
                    + (grid[stone.row + c + 2, stone.col] << 4)
                    + (grid[stone.row + c + 3, stone.col] << 2)
                    + (grid[stone.row + c + 4, stone.col]));
                h = applyHeuritic(b, playerId);
                sum = sum + h;
                if (h == 320000 || h == 250000 || h == -520000 || h == -449000)
                {
                    col++;
                }
            }
        }

        int isForb = 0;
        if (col >1)
        {
            isForb += 2;
        }
        if (row > 1)
        {
            isForb += 2;
        }
        if (ulbr > 1)
        {
            isForb += 2;
        }
        if (blur > 1)
        {
            isForb += 2;
        }

        int[] result = { GameController.doubleFreeThreeEnabled ? 0 : Convert.ToInt32(isForb > 3), sum };
        return result;
    }

    public static int applyHeuritic(ushort pattern, int playerId)
    {
        return Utils.Patterns[playerId].ContainsKey(pattern) ? Utils.Patterns[playerId][pattern] : 0;
    }

    private void printPattern(ushort pattern)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("pattern: ");
        sb.Append((((uint)pattern << 22) >> (22 + 8)));
        sb.Append((((uint)pattern << 24) >> (24 + 6)));
        sb.Append((((uint)pattern << 26) >> (26 + 4)));
        sb.Append((((uint)pattern << 28) >> (28 + 2)));
        sb.Append((((uint)pattern << 30) >> (30 + 0)));
        sb.Append(" " + pattern);
        sb.Append(" " + applyHeuritic(pattern, 2));
    }

    private string getPattern(ushort pattern)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append((((uint)pattern << 22) >> (22 + 8)));
        sb.Append((((uint)pattern << 24) >> (24 + 6)));
        sb.Append((((uint)pattern << 26) >> (26 + 4)));
        sb.Append((((uint)pattern << 28) >> (28 + 2)));
        sb.Append((((uint)pattern << 30) >> (30 + 0)));
        sb.Append(" " + pattern);

        return sb.ToString();
    }
}
