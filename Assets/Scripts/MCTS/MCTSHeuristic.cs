using System.Collections.Generic;

public static class MCTSHeuristic
{
    // Kiểm tra nước thắng ngay cho symbol
    public static Point FindImmediateWin(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = symbol;
                bool isWin = TreeNodeStatic.checkNInRow(state.boardState, x, y, symbol, inrow);
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                if (isWin)
                {
                    return new Point(x, y);
                }
            }
        return null;
    }

    // Chặn nước thắng ngay của đối thủ
    public static Point BlockImmediateWin(State state, char oppSymbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = oppSymbol;
                bool isThreat = TreeNodeStatic.checkNInRow(state.boardState, x, y, oppSymbol, inrow);
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                if (isThreat)
                {
                    return new Point(x, y);
                }
            }
        return null;
    }

    // Tạo/chặn double threat
    public static Point FindDoubleThreat(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                state.boardState[x][y] = symbol;
                int winCount = 0;
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        if (state.boardState[i][j] != Square.SQUARE_EMPTY) continue;
                        state.boardState[i][j] = symbol;
                        if (TreeNodeStatic.checkNInRow(state.boardState, i, j, symbol, inrow))
                            winCount++;
                        state.boardState[i][j] = Square.SQUARE_EMPTY;
                        if (winCount >= 2) break;
                    }
                state.boardState[x][y] = Square.SQUARE_EMPTY;
                if (winCount >= 2)
                    return new Point(x, y);
            }
        return null;
    }

    // Tạo/chặn chuỗi 4 (mở hai đầu, một đầu)
    public static Point FindOpenFour(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int dir = 0; dir < 4; dir++)
                {
                    bool valid = true;
                    if (dir == 0 && x + inrow - 2 >= size) valid = false;
                    else if (dir == 1 && y + inrow - 2 >= size) valid = false;
                    else if (dir == 2 && (x + inrow - 2 >= size || y + inrow - 2 >= size)) valid = false;
                    else if (dir == 3 && (x + inrow - 2 >= size || y - (inrow - 2) < 0)) valid = false;
                    if (!valid) continue;
                    int cnt = 0;
                    int sx = x;
                    int sy = y;
                    for (int k = 0; k < inrow - 1; k++)
                    {
                        int nx = sx + dx[dir] * k;
                        int ny = sy + dy[dir] * k;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                        if (state.boardState[nx][ny] == symbol) cnt++;
                        else break;
                    }
                    if (cnt == inrow - 1)
                    {
                        int px1 = sx - dx[dir];
                        int py1 = sy - dy[dir];
                        int px2 = sx + dx[dir] * (inrow - 1);
                        int py2 = sy + dy[dir] * (inrow - 1);
                        bool head1 = (px1 >= 0 && px1 < size && py1 >= 0 && py1 < size && state.boardState[px1][py1] == Square.SQUARE_EMPTY);
                        bool head2 = (px2 >= 0 && px2 < size && py2 >= 0 && py2 < size && state.boardState[px2][py2] == Square.SQUARE_EMPTY);
                        if (head1) return new Point(px1, py1);
                        if (head2) return new Point(px2, py2);
                    }
                }
        return null;
    }

    // Tạo/chặn chuỗi 3 (mở hai đầu, một đầu)
    public static Point FindOpenThree(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int dir = 0; dir < 4; dir++)
                {
                    bool valid = true;
                    if (dir == 0 && x + inrow - 3 >= size) valid = false;
                    else if (dir == 1 && y + inrow - 3 >= size) valid = false;
                    else if (dir == 2 && (x + inrow - 3 >= size || y + inrow - 3 >= size)) valid = false;
                    else if (dir == 3 && (x + inrow - 3 >= size || y - (inrow - 3) < 0)) valid = false;
                    if (!valid) continue;
                    int cnt = 0;
                    int sx = x;
                    int sy = y;
                    for (int k = 0; k < inrow - 2; k++)
                    {
                        int nx = sx + dx[dir] * k;
                        int ny = sy + dy[dir] * k;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                        if (state.boardState[nx][ny] == symbol) cnt++;
                        else break;
                    }
                    if (cnt == inrow - 2)
                    {
                        int px1 = sx - dx[dir];
                        int py1 = sy - dy[dir];
                        int px2 = sx + dx[dir] * (inrow - 2);
                        int py2 = sy + dy[dir] * (inrow - 2);
                        bool head1 = (px1 >= 0 && px1 < size && py1 >= 0 && py1 < size && state.boardState[px1][py1] == Square.SQUARE_EMPTY);
                        bool head2 = (px2 >= 0 && px2 < size && py2 >= 0 && py2 < size && state.boardState[px2][py2] == Square.SQUARE_EMPTY);
                        if (head1) return new Point(px1, py1);
                        if (head2) return new Point(px2, py2);
                    }
                }
        return null;
    }

    // Tạo chuỗi dài nhất
    public static Point FindBestExtend(State state, char symbol, int inrow)
    {
        int size = Board.BOARD_SIZE;
        int maxLen = 1;
        Point best = null;
        int[] dx = {1, 0, 1, 1};
        int[] dy = {0, 1, 1, -1};
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (state.boardState[x][y] != Square.SQUARE_EMPTY) continue;
                int len = 1;
                for (int dir = 0; dir < 4; dir++)
                {
                    int cnt = 1;
                    for (int k = 1; k < inrow; k++)
                    {
                        int nx = x + dx[dir] * k;
                        int ny = y + dy[dir] * k;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                        if (state.boardState[nx][ny] == symbol) cnt++;
                        else break;
                    }
                    for (int k = 1; k < inrow; k++)
                    {
                        int nx = x - dx[dir] * k;
                        int ny = y - dy[dir] * k;
                        if (nx < 0 || nx >= size || ny < 0 || ny >= size) break;
                        if (state.boardState[nx][ny] == symbol) cnt++;
                        else break;
                    }
                    if (cnt > len) len = cnt;
                }
                if (len > maxLen)
                {
                    maxLen = len;
                    best = new Point(x, y);
                }
            }
        return best;
    }

    // Đánh gần quân đã có
    public static List<Point> FindNearMoves(char[][] board, int radius)
    {
        int size = Board.BOARD_SIZE;
        HashSet<string> nearSet = new HashSet<string>();
        List<Point> result = new List<Point>();
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                if (board[x][y] != Square.SQUARE_EMPTY)
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

    // Đánh giá điểm cho từng nước đi
    public static int EvaluateMove(State state, Point move, char mySymbol, char oppSymbol, int inrow)
    {
        // TODO: Implement
        return 0;
    }
} 