public static class TreeNodeStatic
{
    // Kiểm tra có chuỗi inrow quân liên tiếp tại (x, y)
    public static bool checkNInRow(char[][] board, int x, int y, char symbol, int inrow)
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
} 