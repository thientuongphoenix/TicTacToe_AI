using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : MonoBehaviour
{
    public GameObject cellPrefab; //Prefab nút bấm
    public Transform boardParent; //Panel
    public int boardSize; //Kích thước bàn cờ 10x10

    private string[,] board; //Mảng 2 chiều để lưu trạng thái của bàn cờ
    public List<ButtonCell> cells = new List<ButtonCell>(); //Danh sách các ô trong bàn cờ

    //Turn của người chơi
    public bool isPlayerTurn = true;

    //Chiều dài của chuỗi để chiến thắng
    public int winLenght = 5;

    //Các hướng để kiểm tra chiến thắng
    private readonly int[] dx = { 1, 0, 1, 1 };
    private readonly int[] dy = { 0, 1, 1, -1 };

    private void Start()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        board = new string[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++) //row
        {
            for (int j = 0; j < boardSize; j++) //col
            {
                var go = Instantiate(cellPrefab, new Vector3(j, -i, 0), Quaternion.identity, boardParent);
                var cell = go.GetComponent<ButtonCell>();
                cell.row = i;
                cell.column = j;
                cell.ticTacToeManager = this;
                int row = i;
                int column = j;
                //Đăng ký sự kiện
                cell.GetComponent<Button>().onClick.AddListener(() => HandlePlayerMove(row, column));
                cells.Add(cell);
            }
        }
    }

    //Hàm xử lý khi người chơi nhấn và ô
    public void HandlePlayerMove(int row, int column)
    {
        if (!isPlayerTurn || board[row, column] != null) return;
        //Cập nhật trạng thái bàn cờ
        board[row, column] = "O";
        UpdateCellUI(row, column, "O");
        //Kiểm tra xem người chơi có chiến thắng không
        //Kiểm tra xem bàn cờ full hay chưa
        var isWin = CheckWin("O");
        if (isWin)
        {
            Debug.Log("Player wins!");
            return;
        }

        var isFull = IsBoardFull(board);
        if (isFull)
        {
            Debug.Log("Deuce");
            return;
        }
        else
        {
            isPlayerTurn = false; //Chuyển lượt cho máy chơi
            Invoke(nameof(PlayerAIMove), 0.3f); //Gọi AI sau 0.3s
        }
    }

    //Hàm xử lý khi máy chơi
    void PlayerAIMove()
    {
        int stoneCount = CountStones(board);

        //Độ sâu tối đa
        int depth = stoneCount < 10 ? 4 : 3;

        //Kiểm tra ngay lập tức có thắng hay không
        var immediateMove = FindImmediateMove();

        Vector2Int bestMove;
        if(immediateMove != Vector2Int.one * -1)
        {
            bestMove = immediateMove;
        }
        else
        {
            var(move, _) = Minimax(board, depth, true, int.MinValue, int.MaxValue);
            bestMove = move;
        }
        board[bestMove.x, bestMove.y] = "X"; //Cập nhật bàn cờ
        UpdateCellUI(bestMove.x, bestMove.y, "X"); //Cập nhật giao diện

        // kiểm tra máy có thắng hay không
        if (CheckWin("X"))
        {
            Debug.Log("Máy thắng");
        }
        else if (IsBoardFull(board))
        {
            Debug.Log("Hòa");
        }
        else
        {
            isPlayerTurn = true; // đến lượt người chơi
        }
    }

    //Thuật toán miniMax
    (Vector2Int move, int score) Minimax(string[,] b, int depth, bool isMax, int alpha, int beta)
    {
        //Kiểm tra thắng thua
        //Nếu máy thắng, trả về điểm số
        if (CheckWin("X", b)) return (Vector2Int.zero, 1000 + depth);
        //Nếu người chơi thắng, trả về điểm số
        if(CheckWin("O", b)) return (Vector2Int.zero, -1000 - depth);
        //Nếu bàn cờ đầy hoặc độ sâu tối đa đạt được, trả về điểm số
        if(IsBoardFull(b) || depth == 0) return (Vector2Int.zero, EvaluateBoard(b));
        //Khởi tạo biến để lưu điểm tốt nhất
        List<Vector2Int> moves = GetSmartCandidateMoves(b);
        Vector2Int bestMove = moves.Count > 0 ? moves[0] : Vector2Int.zero;
        int bestScore = isMax ? int.MinValue : int.MaxValue;
        //Duyệt qua tất cả các nước đi
        foreach(var move in moves)
        {
            b[move.x, move.y] = isMax ? "X" : "O";//Giả lập nước đi
            //Gọi đệ quy MiniMax
            var score = Minimax(b, depth - 1, !isMax, alpha, beta).score;
            b[move.x, move.y] = null; //Khôi phục trạng thái
            //cập nhật điểm số tốt nhất
            if(isMax && score > bestScore)
            {
                bestScore = score;
                bestMove = move;
                alpha = Mathf.Max(alpha, score);//Cập nhật alpha
            }
            else if(!isMax && score < bestScore)
            {
                bestScore = score;
                bestMove= move;
                beta = Mathf.Min(beta, score);//Cập nhật beta
            }
            //Cắt tỉa cây tìm kiếm
            if(beta <= alpha)
            {
                break;//Không cần kiểm tra thêm
            }
        }
        return(bestMove, bestScore);
    }

    //Hàm lấy các nước đi thống minh
    List<Vector2Int> GetSmartCandidateMoves(string[,] b)
    {
        //Tạo danh sách các nước đi
        List<Vector2Int> candidates = new List<Vector2Int>();
        //Tìm nước đi đã được xem xét
        HashSet<Vector2Int> consideredCells = new HashSet<Vector2Int>();

        //Phạm vi xem xét xung quanh các quân đã đặt
        int searchRange = 2;
        //Tìm các ô trống xung quanh các quân đã đặt
        for(var row = 0; row < boardSize; row++)
        {
            for(var col = 0; col < boardSize; col++)
            {
                if (b[row, col] != null) //Tìm các ô đã có quân
                {
                    //kiểm tra các ô xung quanh
                    for(int dr = -searchRange; dr <= searchRange; dr++)
                    {
                        for(int dc = -searchRange; dc <= searchRange; dc++)
                        {
                            int newRow = row + dr;
                            int newCol = col + dc;
                            //Kiểm tra xem ô mới có nằm trong bàn cờ không
                            if(newRow >= 0 && newRow < boardSize 
                                && newCol >= 0 && newCol < boardSize
                                && b[newRow,newCol] == null
                                && !consideredCells.Contains(new Vector2Int(newRow, newCol)))
                            {
                                consideredCells.Add(new Vector2Int(newRow, newCol));
                                candidates.Add(new Vector2Int(newRow, newCol));
                            }
                        }
                    }
                }
            }
        }
        //Nếu không có bước đi nào, chọn vị trí trung tâm
        if(candidates.Count == 0)
        {
            int center = boardSize / 2;
            if (b[center, center] == null)
            {
                candidates.Add(new Vector2Int(center, center));
            }
            else
            {
                //Tìm ô trống đầu tiên
                for(int row = 0; row < boardSize; row++)
                {
                    for(int col = 0; col < boardSize; col++)
                    {
                        if(b[row, col] == null)
                        {
                            candidates.Add(new Vector2Int(row, col));
                            break;
                        }
                    }
                    if (candidates.Count > 0) break;
                }
            }
        }

        //Sắp xếp các nước đi theo thứ tự ưu tiên
        candidates = candidates.OrderByDescending(pos => EvaluateMove(b, pos.x, pos.y)).ToList();

        return candidates;
    }

    //Đánh giá giá trị của bàn cờ hiện tại
    int EvaluateBoard(string[,] b)
    {
        int score = 0;
        //Mảng lưu các giá trị của chuỗi
        int[] aiValues = { 0, 1, 10, 100, 1000 };
        //Giá trị phòng thủ sẽ cao hơn tấn công
        int[] playerValues = { 0, -1, -15, -150, -2000 };
        //Tính điểm theo dòng cột và đường chéo
        for(var row = 0; row < boardSize; row++)
        {
            for(var col = 0; col < boardSize; col++)
            {
                for(int d = 0; d < 4; d++)
                {
                    //Kiểm tra xem có thể tạo được chuỗi chiến thắng không
                    if (row + dx[d] * (winLenght - 1) >= boardSize || row + dx[d] * (winLenght - 1) < 0
                        || col + dy[d] * (winLenght - 1) >= boardSize || col + dy[d] * (winLenght - 1) < 0) continue;
                    int aiCount = 0, playerCount = 0, emptyCount = 0;
                    for(int k = 0; k < winLenght; k++)
                    {
                        int newRow = row + dx[d] * k;
                        int newCol = col + dy[d] * k;
                        if (b[newRow, newCol] == "X") aiCount++;
                        else if (b[newRow, newCol] == "O") playerCount++;
                        else emptyCount++;
                    }
                    //chỉ tính điểm nếu chuỗi không bị chặn bởi quân của đối thủ
                    if (playerCount == 0 && aiCount > 0) score += aiValues[aiCount];
                    else if (aiCount == 0 && playerCount > 0) score += playerValues[playerCount];
                }
            }
        }

        //Thêm yếu tố vị trí - ưu tiên các ô ở giữa
        for(var row = 0; row < winLenght; row++)
            for(int col = 0; col < winLenght; col++)
            {
                if (board[row, col] == "X")
                {
                    //Điểm vị trí - khoảng cách đến ô giữa
                    int centerRow = boardSize / 2;
                    int centerCol = boardSize / 2;
                    float distance = Mathf.Sqrt(Mathf.Pow(row - centerRow, 2) + Mathf.Pow(col - centerCol, 2));
                    score += Mathf.Max(5 - (int)distance, 0);
                }
            }
        return score;
    }

    //Đánh giá nước đi tại vị trí cụ thể
    int EvaluateMove(string[,] b, int row, int col)
    {
        int value = 0;
        //Giả lập nước đi
        b[row, col] = "X";
        //Tính điểm cho nước đi
        value += CalculateStrength(b, row, col, "X");
        b[row, col] = null;

        //Giả lập nước đi của đối thủ
        b[row, col] = "O";
        //Tính điểm nước đi
        value -= CalculateStrength(b, row, col, "O");
        b[row, col] = null;

        //Yếu tố vị trí - Ưu tiên các ô ở giữa
        int centerRow = boardSize / 2;
        int CenterCol = boardSize / 2;
        int distance = Mathf.Abs(row- centerRow) + Mathf.Abs(col - CenterCol);
        value += Mathf.Max(5 - distance, 0);
        return value;
    }

    // Tính điểm cho nước đi
    int CalculateStrength(string[,] b, int row, int col, string symbol)
    {
        int strength = 0;
        //Kiểm tra 4 hướng
        for(int d = 0; d < 4; d++)
        {
            int count = 0; // Đếm số quân liên tiếp
            int emptyBefore = 0; // Đếm số quân trống trước quân
            int emptyAfter = 0; // Đếm số ô trống sau quân

            //Đếm về phía trước
            for(int k = 0; k < winLenght; k++)
            {
                int newRow = row + dx[d] * k;
                int newCol = col + dy[d] * k;
                if (newRow < 0 || newRow >= boardSize || newCol < 0 || newCol >= boardSize) break;
                if (b[newRow, newCol] == symbol) count++;
                else if (b[newRow, newCol] == null) 
                { 
                    emptyAfter++;
                    break;
                }
                else break;
            }
            //Đếm về phía sau
            for (int k = 0; k < winLenght; k++)
            {
                int newRow = row + dx[d] * k;
                int newCol = col + dy[d] * k;
                if (newRow < 0 || newRow >= boardSize || newCol < 0 || newCol >= boardSize) break;
                if (b[newRow, newCol] == symbol) count++;
                else if (b[newRow, newCol] == null)
                {
                    emptyBefore++;
                    break;
                }
                else break;
            }
            //Đánh giá sức mạnh dựa trên số quân liên tiếp và số ô trống
            if(count + emptyBefore + emptyAfter >= winLenght)
            {
                if (count == 4) strength += 1000; // 4 ô liên tiếp, nguy hiểm
                else if (count == 3) strength += 100; //Cần lưu ý
                else if (count == 2) strength += 10; // Ít quan trọng

                //Thêm giá trị dựa trên không gian có sẵn
                strength += (emptyBefore + emptyAfter) * 2;
            }
        }
        return strength;
    }

    //Tìm nước đi ngay lập tức - Thắng hoặc chặn
    Vector2Int FindImmediateMove()
    {
        //Kiểm tra xem máy có thể thắng ngày lặp tức hay không
        for(var row = 0; row < boardSize; row++)
            for(var col = 0; col < boardSize; col++)
            {
                if(board[row, col] != null) continue;
                board[row, col] = "X";//Giả lập nước đi
                if (CheckWin("X"))
                {
                    board[row, col] = null; //Khôi phục lại trạng thái
                    return new Vector2Int(row, col); //Trả về nước đi thắng
                } 
                board[row, col] = null; //Khôi phục lại trạng thái
            }

        //Kiểm tra xem máy cần phải chặn người chơi thắng không 
        for(var row = 0; row < boardSize; row++)
            for(int col = 0; col < boardSize; col++)
            {
                if (board[row, col] != null) continue;
                board[row, col] = "O";//Giả lập nước đi
                if (CheckWin("O"))
                {
                    board[row, col] = null; //Khôi phục lại trạng thái
                    return new Vector2Int (row, col); //Trả về nước đi chặn
                }
                board[row, col] = null;//Khôi phục lại trạng thái
            }
        return new Vector2Int(-1, -1);//Không tìm thấy nước đi nào
    }

    //Hàm đếm số quân trên bàn cờ
    int CountStones(string[,] b)
    {
        int count = 0;
        foreach(var s in b)
        {
            if(s != null) count++;
        }
        return count;
    }

    private bool IsBoardFull(string[,] board)
    {
        for (int i = 0; i < boardSize; i++)//row
        {
            for (int j = 0; j < boardSize; j++)//colmn
            {
                if (board[i, j] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    //Hàm kiểm tra xem người chơi có thắng không
    private bool CheckWin(string symbol, string[,] b = null)
    {
        //Nếu không có bàn cờ nào được truyền vào, sử dụng bàn cờ hiện tại
        b ??= board; // ??= Nếu b null thì lấy board gán vô, còn không giữ nguyên b
        for (var row = 0; row < boardSize; row++)
            for (var col = 0; col < boardSize; col++)
            {
                if (b[row, col] != symbol) continue;

                //Kiểm tra cả 4 hướng
                for(int d  = 0; d < 4; d++)
                {
                    int count = 1; //Bắt đầu đếm từ 1
                    for(int k = 1; k < winLenght; k++)
                    {
                        int newRow = row + dx[d] * k;
                        int newCol = col + dy[d] * k;

                        //Kiểm tra xem ô mới có nằm trong bàn cờ không

                        if (newRow < 0 || newRow >= boardSize ||
                            newCol < 0 || newCol >= boardSize ||
                            b[newRow, newCol] != symbol) break;
                        count++;
                    }
                    if(count >= winLenght)
                    {
                        //Nếu có ít nhất winLength ô liên tiếp, người chơi thắng
                        Debug.Log($"{symbol} Win!");
                        return true;
                    }
                }
            }
        //Nếu không có ai thắng, trả về false
        return false;
    }

    void UpdateCellUI(int row, int column, string symbol)
    {
        //Cập nhật giao diện cho ô
        var cell = cells.First(x => x.row == row && x.column == column);
        cell.SetSymbol(symbol);

        //Tô màu cho ô
        var image = cell.GetComponent<Image>();
        if (symbol == "X")
        {
            image.color = Color.red;
        }
        else if (symbol == "O")
        {
            image.color = Color.blue;
        }
    }
}
