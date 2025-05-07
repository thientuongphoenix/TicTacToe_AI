# Edited.md

## BÁO CÁO TỔNG HỢP TOÀN BỘ THAY ĐỔI, NÂNG CẤP DỰ ÁN CARO AI

### 1. Tổng quan

- Dự án Caro AI ban đầu gồm 2 thuật toán: MiniMax (TicTacToePackage) và MCTS (Scripts/MCTS).
- Sau quá trình nâng cấp, hệ thống hỗ trợ bàn cờ động, luật thắng động, hai chế độ chơi, AI dừng đúng lúc, kiểm tra thắng/thua tối ưu, và đặc biệt là nâng cấp mạnh mẽ cho MCTS.

---

## 2. Mục lục thay đổi

1. **Tối ưu hóa cấu trúc code, enum chế độ chơi, UI**
2. **Tích hợp và nâng cấp MCTS**
3. **Nâng cấp kiểm tra thắng/thua, tô màu chuỗi thắng**
4. **Nâng cấp logic phòng thủ, tấn công cho MCTS**
5. **Tối ưu simulate, rollout, vùng đánh**
6. **Tăng số vòng lặp, cải thiện chất lượng nước đi**
7. **Tạo file helper, tách logic, chuẩn hóa code**
8. **Tạo file tài liệu, báo cáo**

---

### 3. Chi tiết các thay đổi

#### 3.1. Tối ưu hóa cấu trúc code, enum chế độ chơi, UI

- Thêm enum chế độ chơi (Player vs AI, AI vs AI), biến điều khiển, hàm chuyển chế độ, reset, khung hàm cho AI vs AI.
- UI có 3 nút: Player vs AI, AI vs AI, Reset.
- Tối ưu code quản lý trạng thái, khởi tạo lại bàn cờ, cập nhật UI.

#### 3.2. Tích hợp và nâng cấp MCTS

- Tạo class MCTSLogic.cs (hoặc tích hợp trực tiếp vào TicTacToeManager) để gọi MCTS thuần túy, chuyển đổi dữ liệu bàn cờ, cập nhật UI, kiểm tra thắng/thua/hòa.
- Sửa các class MCTS (Board, TreeNode, State, ...) để hỗ trợ bàn cờ động (10x10), cho phép ghi đè kích thước từ TicTacToeManager.
- Sửa triệt để các hàm kiểm tra thắng/thua, pattern, khởi tạo mảng trong TreeNode.cs để không còn dùng mảng cố định 3 phần tử, mà chuyển sang động theo BoardSize và INROW.

#### 3.3. Nâng cấp kiểm tra thắng/thua, tô màu chuỗi thắng

- Sửa lại hàm CheckWin để chỉ tô màu vàng đúng chuỗi thắng 5 ô liên tiếp, không tô nhầm các chuỗi dài hơn hoặc ngắn hơn.
- Thêm biến gameOver, đảm bảo khi một AI thắng hoặc hòa thì dừng không đánh nữa, Debug ra AI nào thắng.

#### 3.4. Nâng cấp logic phòng thủ, tấn công cho MCTS

- Thêm các hàm chiến lược hiện đại vào MCTSHeuristic.cs:
  - FindImmediateWin: Kiểm tra nước thắng ngay.
  - BlockImmediateWin: Chặn nước thắng ngay của đối thủ.
  - FindDoubleThreat: Tìm/chặn double threat.
  - FindOpenFour: Tìm/chặn chuỗi 4 (mở hai đầu, một đầu).
  - FindOpenThree: Tìm/chặn chuỗi 3 (mở hai đầu, một đầu).
  - FindBestExtend: Tìm nước đi nối chuỗi dài nhất.
  - FindNearMoves: Đánh gần quân đã có.
- Tích hợp toàn bộ chiến lược này vào simulate của MCTS, ưu tiên phòng thủ tuyệt đối, sau đó mới tấn công, cuối cùng mới random.

#### 3.5. Tối ưu simulate, rollout, vùng đánh

- Khi sinh nước đi, chỉ xét các ô trống nằm trong vùng 2-3 ô quanh các quân đã đánh để tăng tốc và độ chính xác.
- Tối ưu hàm simulate: kiểm tra lần lượt các chiến lược, không bỏ sót thế nguy hiểm.

#### 3.6. Tăng số vòng lặp, cải thiện chất lượng nước đi

- Tăng iterationNumber mặc định lên 50.000 để MCTS mạnh hơn.
- Cho phép điều chỉnh số vòng lặp qua Inspector.

#### 3.7. Tạo file helper, tách logic, chuẩn hóa code

- Tạo file TreeNodeStatic.cs để dùng chung hàm kiểm tra chuỗi liên tiếp cho mọi class.
- Tách logic đánh giá nước đi vào file MCTSHeuristic.cs.
- Chuẩn hóa code, xóa các đoạn code tạm thời, sửa lỗi linter, import.

#### 3.8. Tạo file tài liệu, báo cáo

- Tổng hợp toàn bộ thay đổi vào file Edited.md để phục vụ báo cáo, trình bày rõ ràng, có mục lục, phân loại từng nhóm thay đổi.

---

### 4. Kết quả đạt được

- MCTS đã mạnh lên rõ rệt, biết phòng thủ, tấn công, tạo thế, không còn thua dễ như trước.
- MiniMax vẫn nhỉnh hơn về lý thuyết, nhưng MCTS đã đủ sức kéo dài trận đấu, tạo ra nhiều thế cờ phức tạp, hấp dẫn.
- Hệ thống code rõ ràng, dễ mở rộng, dễ bảo trì, thuận tiện cho nghiên cứu và phát triển tiếp theo.

---

**Báo cáo hoàn tất. Nếu Bệ Hạ cần bổ sung chi tiết nào, xin cứ chỉ dụ!**
