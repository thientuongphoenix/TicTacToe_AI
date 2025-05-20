using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cung cấp logic thuật toán Monte Carlo Tree Search (MCTS) cho AI Caro.
/// Dùng để tìm nước đi tối ưu cho AI dựa trên trạng thái bàn cờ.
/// Có thể sử dụng độc lập hoặc gọi từ các script quản lý game.
/// </summary>
public class MCTSLogic
{
    public int boardSize = 10;
    public int iterationNumber = 200;
    public char myTurn = 'O'; // MCTS sẽ đánh "O" khi đấu với MiniMax

    // Hàm lấy nước đi tốt nhất cho MCTS
    public Vector2Int GetBestMove(char[][] boardState, char currentTurn, int pieceNumber, Point lastPos, Point lastOPos)
    {
        State rootState = new State(boardState, currentTurn, lastPos, lastOPos, pieceNumber);
        TreeNode root = new TreeNode(rootState);
        MCTSAI.myTurn = myTurn;
        for (int i = 0; i < iterationNumber; i++)
        {
            root.iterateMCTS();
        }
        // Chọn child có số lượt thăm lớn nhất
        TreeNode bestChild = null;
        double maxVisit = -1;
        foreach (var child in root.children)
        {
            if (child != null && child.state != null && child.state.lastOPos != null && child.NVisits > maxVisit)
            {
                bestChild = child;
                maxVisit = child.NVisits;
            }
        }
        if (bestChild != null && bestChild.state.lastOPos != null)
        {
            return new Vector2Int(bestChild.state.lastOPos.x, bestChild.state.lastOPos.y);
        }
        // Nếu không tìm được, chọn ô trống đầu tiên
        for (int i = 0; i < boardSize; i++)
            for (int j = 0; j < boardSize; j++)
                if (boardState[i][j] == Square.SQUARE_EMPTY)
                    return new Vector2Int(i, j);
        return new Vector2Int(-1, -1);
    }

    // Hàm chuyển đổi từ string[,] sang char[][]
    public static char[][] ConvertToMCTSBoard(string[,] board)
    {
        int size = board.GetLength(0);
        char[][] result = new char[size][];
        for (int i = 0; i < size; i++)
        {
            result[i] = new char[size];
            for (int j = 0; j < size; j++)
            {
                if (board[i, j] == "X") result[i][j] = Square.SQUARE_X;
                else if (board[i, j] == "O") result[i][j] = Square.SQUARE_O;
                else result[i][j] = Square.SQUARE_EMPTY;
            }
        }
        return result;
    }

    // Hàm lấy số quân đã đánh
    public static int CountStones(char[][] board)
    {
        int count = 0;
        for (int i = 0; i < board.Length; i++)
            for (int j = 0; j < board[i].Length; j++)
                if (board[i][j] != Square.SQUARE_EMPTY) count++;
        return count;
    }
} 