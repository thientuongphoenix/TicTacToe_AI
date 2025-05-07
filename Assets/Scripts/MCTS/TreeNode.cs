using UnityEngine;
using System;
using System.Collections.Generic;
// using Assets.Scripts.MCTS;

/*
 * This code was modified from Simon Lucas' implementation of MCTS
 * https://web.archive.org/web/20151213012727/http://mcts.ai/code/java.html
 */
public class TreeNode
{
    static System.Random r = new System.Random();
    static double epsilon = 1e-6;

    public List<TreeNode> children;
    double nVisits, totValue;
    public double NVisits => nVisits;
    public double uctValue;

    public State state;

    public TreeNode(State state)
    {
        children = new List<TreeNode>();
        nVisits = 0;
        totValue = 0;

        this.state = state;
    }

    public void iterateMCTS()
    {
        LinkedList<TreeNode> visited = new LinkedList<TreeNode>();
        TreeNode cur = this;
        visited.AddLast(this);
        while (!cur.isLeaf()) //1. SELECTION
        {
            cur = cur.select();

            visited.AddLast(cur);
        }
        if (cur.state.stateResult == Board.RESULT_NONE)
        {
            cur.expand(); //2. EXPANSION
            TreeNode newNode = cur.select();
            visited.AddLast(newNode);
            double value = newNode.simulate(); //3. SIMULATION

            foreach (TreeNode node in visited)
            {
                node.updateStats(value); //4. BACKPROPAGATION
            }
        }
    }

    public void expand()
    {
        //List<Point> childrenMoves = listPossibleMoves(state.boardState);
        //Apply one move for each expansion child
        //foreach (Point move in childrenMoves)
        //{
        //    TreeNode childNode = new TreeNode(new State(state.boardState, state.currentTurn, state.lastPos, state.lastOPos, state.pieceNumber));
        //    if (state.currentTurn == Board.TURN_X)
        //    {
        //        childNode.state.boardState[move.x][move.y] = Square.SQUARE_X;
        //        childNode.state.lastPos = new Point(move.x, move.y);
        //        childNode.state.pieceNumber++;
        //        childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_X);
        //        childNode.state.currentTurn = Board.TURN_O; //apply new turn
        //    }
        //    else //state.currentTurn == Board.TURN_O
        //    {
        //        childNode.state.boardState[move.x][move.y] = Square.SQUARE_O;
        //        childNode.state.lastOPos = new Point(move.x, move.y);
        //        childNode.state.pieceNumber++;
        //        childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_O);
        //        childNode.state.currentTurn = Board.TURN_X; //apply new turn
        //    }
        //    children.Add(childNode);
        //}
        // --- BỔ SUNG: Chỉ sinh node con cho các ô gần quân đã đánh (bán kính 2) ---
        List<Point> childrenMoves = MCTSHeuristic.FindNearMoves(state.boardState, 2);
        if (childrenMoves.Count == 0) childrenMoves = listPossibleMoves(state.boardState); // fallback nếu bàn trống
        foreach (Point move in childrenMoves)
        {
            TreeNode childNode = new TreeNode(new State(state.boardState, state.currentTurn, state.lastPos, state.lastOPos, state.pieceNumber));
            if (state.currentTurn == Board.TURN_X)
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_X;
                childNode.state.lastPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_X);
                childNode.state.currentTurn = Board.TURN_O; //apply new turn
            }
            else //state.currentTurn == Board.TURN_O
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_O;
                childNode.state.lastOPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_O);
                childNode.state.currentTurn = Board.TURN_X; //apply new turn
            }
            children.Add(childNode);
        }
        // --- HẾT BỔ SUNG ---
    }

    public TreeNode select()
    {
        TreeNode selected = null;
        double bestValue = Double.MinValue;
        foreach (TreeNode c in children)
        {
            //UCT value calculation
            double uctValue =
                    c.totValue / (c.nVisits + epsilon) +
                            Math.Sqrt(Math.Log(nVisits + 1) / (c.nVisits + epsilon)) +
                            r.NextDouble() * epsilon; // small random number to break ties randomly in unexpanded nodes
            c.uctValue = uctValue;
            if (uctValue > bestValue)
            {
                selected = c;
                bestValue = uctValue;
            }
        }
        return selected;
    }

    public bool isLeaf()
    {
        return children.Count == 0;
    }

    public double simulate()
    {
        State simState = new State(Util.deepcloneArray(state.boardState), state.currentTurn, state.lastPos, state.lastOPos, state.pieceNumber);
        simState.stateResult = state.stateResult;

        char simCurrentTurn = state.currentTurn;
        char simOppTurn = (state.currentTurn == Board.TURN_O ? Board.TURN_X : Board.TURN_O);

        int simValue = int.MinValue;
        int inrow = Board.INROW;

        //simulate semi-randomly (for both players) until a terminal result is achieved
        while (simState.stateResult == Board.RESULT_NONE)
        {
            Point chosenMove = null;
            // --- CHIẾN LƯỢC ƯU TIÊN HIỆN ĐẠI ---
            // 1. Thắng ngay
            chosenMove = MCTSHeuristic.FindImmediateWin(simState, simCurrentTurn, inrow);
            // 2. Chặn thắng ngay của đối thủ
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.BlockImmediateWin(simState, simOppTurn, inrow);
            // 3. Tạo/chặn double threat
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindDoubleThreat(simState, simCurrentTurn, inrow);
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindDoubleThreat(simState, simOppTurn, inrow);
            // 4. Tạo/chặn chuỗi 4 (mở hai đầu, một đầu)
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindOpenFour(simState, simCurrentTurn, inrow);
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindOpenFour(simState, simOppTurn, inrow);
            // 5. Tạo/chặn chuỗi 3 (mở hai đầu, một đầu)
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindOpenThree(simState, simCurrentTurn, inrow);
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindOpenThree(simState, simOppTurn, inrow);
            // 6. Tạo chuỗi dài nhất
            if (chosenMove == null)
                chosenMove = MCTSHeuristic.FindBestExtend(simState, simCurrentTurn, inrow);
            // 7. Đánh gần quân đã có
            if (chosenMove == null)
            {
                List<Point> nearMoves = MCTSHeuristic.FindNearMoves(simState.boardState, 2);
                if (nearMoves.Count > 0)
                    chosenMove = nearMoves[r.Next(nearMoves.Count)];
            }
            // 8. Random toàn bàn
            if (chosenMove == null)
                chosenMove = doRandomMove();
            // --- HẾT CHIẾN LƯỢC ---

            if (simState.boardState[chosenMove.x][chosenMove.y] == Square.SQUARE_EMPTY)
            {
                simState.boardState[chosenMove.x][chosenMove.y] = (simCurrentTurn == Board.TURN_X ? Square.SQUARE_X : Square.SQUARE_O);

                if (simCurrentTurn == Board.TURN_X)
                {
                    simState.lastPos = new Point(chosenMove.x, chosenMove.y);
                }
                else //simCurrentTurn == Board.TURN_O
                {
                    simState.lastOPos = new Point(chosenMove.x, chosenMove.y);
                }
                simState.pieceNumber++;
                simState.stateResult= checkWin(simState, simCurrentTurn); //check terminal condition
                simCurrentTurn = (simCurrentTurn == Board.TURN_X ? Board.TURN_O : Board.TURN_X); //switch turn
                simOppTurn = (simCurrentTurn == Board.TURN_X ? Board.TURN_O : Board.TURN_X); //switch turn
            }
        }

        switch (simState.stateResult)
        {
            case Board.RESULT_DRAW:
            {
                simValue = 0;
                break;
            }
            case Board.RESULT_X:
            {
                simValue = MCTSAI.myTurn == Board.TURN_X ? 1 : -1; //1 means victory, -1 means defeat
                break;
            }
            case Board.RESULT_O:
            {
                simValue = MCTSAI.myTurn == Board.TURN_O ? 1 : -1;
                break;
            }
            default:
            {
                Debug.LogError("illegal simStateResult value");
                break;
            }
        }
        return simValue;
    }

    // Tìm nước đi thắng ngay cho symbol
    public Point FindImmediateWin(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = symbol;
                if (checkNInRow(state.boardState, x, y, symbol, inrow))
                {
                    state.boardState[x][y] = Square.SQUARE_EMPTY;
                    return new Point(x, y);
                }
                state.boardState[x][y] = Square.SQUARE_EMPTY;
            }
        return null;
    }

    // Tìm nước đi tạo chuỗi dài nhất
    public Point FindBestExtend(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        int maxLen = 1;
        Point best = null;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                int len = MaxLineLen(state.boardState, x, y, symbol, inrow);
                if (len > maxLen)
                {
                    maxLen = len;
                    best = new Point(x, y);
                }
            }
        return best;
    }

    // Kiểm tra có chuỗi inrow quân liên tiếp tại (x, y)
    public bool checkNInRow(char[][] board, int x, int y, char symbol, int inrow)
    {
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        int size = Board.BOARD_SIZE;
        for (int d = 0; d < 4; d++)
        {
            int count = 1;
            for (int k = 1; k < inrow; k++)
            {
                int nx = x + dx[d] * k;
                int ny = y + dy[d] * k;
                if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                if (board[nx][ny] == symbol) count++;
                else break;
            }
            for (int k = 1; k < inrow; k++)
            {
                int nx = x - dx[d] * k;
                int ny = y - dy[d] * k;
                if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                if (board[nx][ny] == symbol) count++;
                else break;
            }
            if (count >= inrow) return true;
        }
        return false;
    }

    // Tính độ dài chuỗi dài nhất có thể tạo tại (x, y)
    public int MaxLineLen(char[][] board, int x, int y, char symbol, int inrow)
    {
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        int size = Board.BOARD_SIZE;
        int maxLen = 1;
        for (int d = 0; d < 4; d++)
        {
            int count = 1;
            for (int k = 1; k < inrow; k++)
            {
                int nx = x + dx[d] * k;
                int ny = y + dy[d] * k;
                if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                if (board[nx][ny] == symbol) count++;
                else break;
            }
            for (int k = 1; k < inrow; k++)
            {
                int nx = x - dx[d] * k;
                int ny = y - dy[d] * k;
                if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                if (board[nx][ny] == symbol) count++;
                else break;
            }
            if (count > maxLen) maxLen = count;
        }
        return maxLen;
    }

    public void updateStats(double value)
    {
        nVisits++;
        totValue += value;
    }

    //A heuristic to check one piece in a row, placing one piece adjacent to previous own piece
    public Point placeAdjacent(State state, char currentTurn)
    {
        List<Point> moveList = new List<Point>();
        Point result = null;
        Point lastPosition = currentTurn == Board.TURN_X ? state.lastPos : state.lastOPos;

        //search for possible moves adjacent to lastPosition
        for (int y = 1; y > -2; y--)
        {
            for (int x = -1; x < 2; x++)
            {
                if ((lastPosition.x + x >= 0) && (lastPosition.x + x < Board.BOARD_SIZE)
                    && (lastPosition.y + y >= 0) && (lastPosition.y + y < Board.BOARD_SIZE))
                {
                    if (state.boardState[lastPosition.x + x][lastPosition.y + y] == Square.SQUARE_EMPTY)
                    {
                        moveList.Add(new Point(lastPosition.x + x, lastPosition.y + y));
                    }
                }
            }
        }

        switch (moveList.Count)
        {
            case 0:
                {
                    result = null;
                    break;
                }
            case 1:
                {
                    result = moveList[0];
                    break;
                }
            default:
                {
                    result = moveList[r.Next(moveList.Count)];
                    break;
                }
        }

        return result;
    }

    //A heuristic to check two pieces in a row, winning a game or preventing the opponent from winning the game
    public Point checkTwoPieces(State state, char currentTurn)
    {
        List<Point> moveList = new List<Point>();
        Point result = null;

        // Tạo pattern động cho INROW
        int inrow = Board.INROW;
        // Pattern: [empty, X, X, X, ...] (1 empty, còn lại là currentTurn)
        char[] pattern1 = new char[inrow];
        pattern1[0] = Square.SQUARE_EMPTY;
        for (int i = 1; i < inrow; i++) pattern1[i] = currentTurn;
        string pattern1Str = new string(pattern1);
        // Pattern: [X, empty, X, X, ...] (1 empty ở giữa)
        char[] pattern2 = new char[inrow];
        pattern2[0] = currentTurn;
        pattern2[1] = Square.SQUARE_EMPTY;
        for (int i = 2; i < inrow; i++) pattern2[i] = currentTurn;
        string pattern2Str = new string(pattern2);
        // Pattern: [X, X, ..., empty] (1 empty ở cuối)
        char[] pattern3 = new char[inrow];
        for (int i = 0; i < inrow - 1; i++) pattern3[i] = currentTurn;
        pattern3[inrow - 1] = Square.SQUARE_EMPTY;
        string pattern3Str = new string(pattern3);

        // Kiểm tra hàng ngang
        for (int y = 0; y < Board.BOARD_SIZE; y++)
        {
            for (int x = 0; x <= Board.BOARD_SIZE - inrow; x++)
            {
                char[] window = new char[inrow];
                for (int k = 0; k < inrow; k++) window[k] = state.boardState[x + k][y];
                string winStr = new string(window);
                if (winStr.Equals(pattern1Str)) moveList.Add(new Point(x, y));
                else if (winStr.Equals(pattern2Str)) moveList.Add(new Point(x + 1, y));
                else if (winStr.Equals(pattern3Str)) moveList.Add(new Point(x + inrow - 1, y));
            }
        }
        // Kiểm tra hàng dọc
        for (int x = 0; x < Board.BOARD_SIZE; x++)
        {
            for (int y = 0; y <= Board.BOARD_SIZE - inrow; y++)
            {
                char[] window = new char[inrow];
                for (int k = 0; k < inrow; k++) window[k] = state.boardState[x][y + k];
                string winStr = new string(window);
                if (winStr.Equals(pattern1Str)) moveList.Add(new Point(x, y));
                else if (winStr.Equals(pattern2Str)) moveList.Add(new Point(x, y + 1));
                else if (winStr.Equals(pattern3Str)) moveList.Add(new Point(x, y + inrow - 1));
            }
        }
        // Kiểm tra chéo chính
        for (int x = 0; x <= Board.BOARD_SIZE - inrow; x++)
        {
            for (int y = 0; y <= Board.BOARD_SIZE - inrow; y++)
            {
                char[] window = new char[inrow];
                for (int k = 0; k < inrow; k++) window[k] = state.boardState[x + k][y + k];
                string winStr = new string(window);
                if (winStr.Equals(pattern1Str)) moveList.Add(new Point(x, y));
                else if (winStr.Equals(pattern2Str)) moveList.Add(new Point(x + 1, y + 1));
                else if (winStr.Equals(pattern3Str)) moveList.Add(new Point(x + inrow - 1, y + inrow - 1));
            }
        }
        // Kiểm tra chéo phụ
        for (int x = 0; x <= Board.BOARD_SIZE - inrow; x++)
        {
            for (int y = inrow - 1; y < Board.BOARD_SIZE; y++)
            {
                char[] window = new char[inrow];
                for (int k = 0; k < inrow; k++) window[k] = state.boardState[x + k][y - k];
                string winStr = new string(window);
                if (winStr.Equals(pattern1Str)) moveList.Add(new Point(x, y));
                else if (winStr.Equals(pattern2Str)) moveList.Add(new Point(x + 1, y - 1));
                else if (winStr.Equals(pattern3Str)) moveList.Add(new Point(x + inrow - 1, y - inrow + 1));
            }
        }
        // Chọn ngẫu nhiên nếu có nhiều nước đi
        if (moveList.Count == 0) result = null;
        else if (moveList.Count == 1) result = moveList[0];
        else result = moveList[r.Next(moveList.Count)];
        return result;
    }

    public List<Point> listPossibleMoves(char[][] boardState)
    {   
        List<Point> possibleMoves = new List<Point>();
        //list all 9 possible moves
        for (int i = 0; i < Board.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Board.BOARD_SIZE; j++)
            {
                //check for legal board coordinate
                if (boardState[i][j] == Square.SQUARE_EMPTY)
                {
                    possibleMoves.Add(new Point(i, j));
                }
            }
        }
        return possibleMoves;
    }

    public Point doRandomMove()
    {
        return new Point(r.Next(Board.BOARD_SIZE), r.Next(Board.BOARD_SIZE));
    }

    // Sửa các hàm kiểm tra thắng/thua động theo INROW
    public int checkWin(State state, char currTurn)
    {
        int result = Board.RESULT_NONE;
        int inrow = Board.INROW;
        int size = Board.BOARD_SIZE;
        int lastX = currTurn == Board.TURN_X ? state.lastPos.x : state.lastOPos.x;
        int lastY = currTurn == Board.TURN_X ? state.lastPos.y : state.lastOPos.y;
        // Kiểm tra hàng ngang
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x <= size - inrow; x++)
            {
                bool win = true;
                for (int k = 0; k < inrow; k++)
                {
                    if (state.boardState[x + k][y] != currTurn) { win = false; break; }
                }
                if (win) { result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O); return result; }
            }
        }
        // Kiểm tra hàng dọc
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y <= size - inrow; y++)
            {
                bool win = true;
                for (int k = 0; k < inrow; k++)
                {
                    if (state.boardState[x][y + k] != currTurn) { win = false; break; }
                }
                if (win) { result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O); return result; }
            }
        }
        // Kiểm tra chéo chính
        for (int x = 0; x <= size - inrow; x++)
        {
            for (int y = 0; y <= size - inrow; y++)
            {
                bool win = true;
                for (int k = 0; k < inrow; k++)
                {
                    if (state.boardState[x + k][y + k] != currTurn) { win = false; break; }
                }
                if (win) { result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O); return result; }
            }
        }
        // Kiểm tra chéo phụ
        for (int x = 0; x <= size - inrow; x++)
        {
            for (int y = inrow - 1; y < size; y++)
            {
                bool win = true;
                for (int k = 0; k < inrow; k++)
                {
                    if (state.boardState[x + k][y - k] != currTurn) { win = false; break; }
                }
                if (win) { result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O); return result; }
            }
        }
        // Kiểm tra hòa
        int count = 0;
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                if (state.boardState[i][j] != Square.SQUARE_EMPTY) count++;
        if (count == size * size) result = Board.RESULT_DRAW;
        return result;
    }

    // Tìm nước đi chặn đối thủ tạo chuỗi (dọa thắng)
    public Point FindThreatBlock(State state, char oppSymbol, int threatLen)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = oppSymbol;
                if (checkNInRow(state.boardState, x, y, oppSymbol, threatLen))
                {
                    state.boardState[x][y] = Square.SQUARE_EMPTY;
                    return new Point(x, y);
                }
                state.boardState[x][y] = Square.SQUARE_EMPTY;
            }
        return null;
    }

    // Kiểm tra có chuỗi 3 và khả năng tạo chuỗi 4
    public Point FindThreeAndThreat(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                
                // Kiểm tra có chuỗi 3 không
                state.boardState[x][y] = symbol;
                bool hasThree = checkNInRow(state.boardState, x, y, symbol, 3);
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                
                if (hasThree)
                {
                    // Kiểm tra có thể tạo chuỗi 4 không
                    state.boardState[x][y] = symbol;
                    bool canMakeFour = false;
                    
                    // Kiểm tra 8 hướng xung quanh
                    int[] dx = {1, 1, 1, 0, 0, -1, -1, -1};
                    int[] dy = {1, 0, -1, 1, -1, 1, 0, -1};
                    
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = x + dx[d];
                        int ny = y + dy[d];
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size && state.boardState[nx][ny] == Square.SQUARE_EMPTY)
                        {
                            state.boardState[nx][ny] = symbol;
                            if (checkNInRow(state.boardState, nx, ny, symbol, 4))
                            {
                                canMakeFour = true;
                            }
                            state.boardState[nx][ny] = Square.SQUARE_EMPTY;
                            if (canMakeFour) break;
                        }
                    }
                    
                    state.boardState[x][y] = Square.SQUARE_EMPTY;
                    if (canMakeFour)
                    {
                        return new Point(x, y);
                    }
                }
            }
        return null;
    }

    // --- BỔ SUNG: Hàm nhận diện nước đi double threat ---
    // Nếu đánh vào ô này, đối thủ sẽ có ít nhất 2 nước thắng ngay ở lượt sau
    public Point FindDoubleThreatBlock(State state, char oppSymbol, int threatLen)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                // Giả lập đối thủ đánh vào ô này
                state.boardState[x][y] = oppSymbol;
                int winCount = 0;
                // Đếm số nước thắng ngay nếu đối thủ đánh tiếp các ô trống còn lại
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        if (state.boardState[i][j] != Square.SQUARE_EMPTY) continue;
                        state.boardState[i][j] = oppSymbol;
                        if (checkNInRow(state.boardState, i, j, oppSymbol, threatLen))
                        {
                            winCount++;
                        }
                        state.boardState[i][j] = Square.SQUARE_EMPTY;
                        if (winCount >= 2) break;
                    }
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                if (winCount >= 2)
                {
                    return new Point(x, y);
                }
            }
        return null;
    }
    // --- HẾT BỔ SUNG ---

    // --- BỔ SUNG: Hàm nhận diện và chặn chuỗi 3 mở hai đầu ---
    public Point FindOpenThreeBlock(State state, char oppSymbol)
    {
        int size = Board.BOARD_SIZE;
        //int inrow = Board.INROW;
        List<Point> blockMoves = new List<Point>();
        // Kiểm tra tất cả các hướng
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    int cnt = 0;
                    int sx = x;
                    int sy = y;
                    // Kiểm tra chuỗi 3 liên tiếp
                    for (int k = 0; k < 3; k++)
                    {
                        int nx = sx + dx[dir] * k;
                        int ny = sy + dy[dir] * k;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                        if (state.boardState[nx][ny] == oppSymbol) cnt++;
                        else break;
                    }
                    if (cnt == 3)
                    {
                        // Kiểm tra hai đầu có trống không
                        int px1 = sx - dx[dir];
                        int py1 = sy - dy[dir];
                        int px2 = sx + dx[dir] * 3;
                        int py2 = sy + dy[dir] * 3;
                        bool head1 = (px1 >= 0 && px1 < size && py1 >= 0 && py1 < size && state.boardState[px1][py1] == Square.SQUARE_EMPTY);
                        bool head2 = (px2 >= 0 && px2 < size && py2 >= 0 && py2 < size && state.boardState[px2][py2] == Square.SQUARE_EMPTY);
                        if (head1) blockMoves.Add(new Point(px1, py1));
                        if (head2) blockMoves.Add(new Point(px2, py2));
                    }
                }
            }
        if (blockMoves.Count > 0)
        {
            return blockMoves[r.Next(blockMoves.Count)];
        }
        return null;
    }
    // --- HẾT BỔ SUNG ---

    // --- BỔ SUNG: Hàm nhận diện và chặn mọi chuỗi 3 hoặc 4 còn ô trống ở đầu/cuối ---
    public Point FindAnyOpenBlock(State state, char oppSymbol)
    {
        int size = Board.BOARD_SIZE;
        int[] checkLens = {3, 4};
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        List<Point> blockMoves = new List<Point>();
        foreach (int len in checkLens)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    for (int dir = 0; dir < 4; dir++)
                    {
                        // --- BỔ SUNG: Kiểm tra điều kiện biên cho từng hướng ---
                        bool valid = true;
                        if (dir == 0) // ngang
                        {
                            if (x + len - 1 >= size) valid = false;
                        }
                        else if (dir == 1) // dọc
                        {
                            if (y + len - 1 >= size) valid = false;
                        }
                        else if (dir == 2) // chéo xuôi
                        {
                            if (x + len - 1 >= size || y + len - 1 >= size) valid = false;
                        }
                        else if (dir == 3) // chéo ngược
                        {
                            if (x + len - 1 >= size || y - (len - 1) < 0) valid = false;
                        }
                        if (!valid) continue;
                        // --- HẾT BỔ SUNG ---
                        int cnt = 0;
                        int sx = x;
                        int sy = y;
                        // Kiểm tra chuỗi len liên tiếp
                        for (int k = 0; k < len; k++)
                        {
                            int nx = sx + dx[dir] * k;
                            int ny = sy + dy[dir] * k;
                            if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                            if (state.boardState[nx][ny] == oppSymbol) cnt++;
                            else break;
                        }
                        if (cnt == len)
                        {
                            // Kiểm tra hai đầu có trống không
                            int px1 = sx - dx[dir];
                            int py1 = sy - dy[dir];
                            int px2 = sx + dx[dir] * len;
                            int py2 = sy + dy[dir] * len;
                            bool head1 = (px1 >= 0 && px1 < size && py1 >= 0 && py1 < size && state.boardState[px1][py1] == Square.SQUARE_EMPTY);
                            bool head2 = (px2 >= 0 && px2 < size && py2 >= 0 && py2 < size && state.boardState[px2][py2] == Square.SQUARE_EMPTY);
                            // --- BỔ SUNG: Chặn cả khi chỉ còn một đầu trống ---
                            if (head1) blockMoves.Add(new Point(px1, py1));
                            if (head2) blockMoves.Add(new Point(px2, py2));
                            // --- HẾT BỔ SUNG ---
                        }
                    }
                }
        }
        if (blockMoves.Count > 0)
        {
            return blockMoves[r.Next(blockMoves.Count)];
        }
        return null;
    }
    // --- HẾT BỔ SUNG ---

    //debugging purpose
    public void printState(char[][] boardState)
    {
        String s = null;
        for (int y = boardState.Length - 1; y > -1; y--)
        {
            for (int x = 0; x < boardState.Length; x++)
            {
                s += boardState[x][y];
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    // --- BỔ SUNG: Hàm lấy danh sách ô trống gần các quân đã đánh ---
    public List<Point> FindNearMoves(char[][] board, int radius)
    {
        int size = Board.BOARD_SIZE;
        HashSet<string> nearSet = new HashSet<string>();
        List<Point> result = new List<Point>();
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (board[x][y] != Square.SQUARE_EMPTY) // Nếu là quân đã đánh
                {
                    for (int dx = -radius; dx <= radius; dx++)
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < size && ny >= 0 && ny < size && board[nx][ny] == Square.SQUARE_EMPTY)
                            {
                                string key = nx + "," + ny;
                                if (!nearSet.Contains(key))
                                {
                                    nearSet.Add(key);
                                    result.Add(new Point(nx, ny));
                                }
                            }
                        }
                }
            }
        return result;
    }
    // --- HẾT BỔ SUNG ---

    // --- BỔ SUNG: Hàm phòng thủ tuyệt đối, chặn mọi nước giúp đối thủ tạo chuỗi 4 ---
    public Point BlockImmediateThreat(State state, char oppSymbol, int threatLen)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = oppSymbol;
                bool isThreat = checkNInRow(state.boardState, x, y, oppSymbol, threatLen);
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                if (isThreat)
                {
                    return new Point(x, y);
                }
            }
        return null;
    }
    // --- HẾT BỔ SUNG ---
}