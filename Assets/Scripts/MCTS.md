# Thuật toán MCTS trong Game Caro

## Tổng quan

Thuật toán Monte Carlo Tree Search (MCTS) được triển khai trong các file:

- `MCTSAI.cs`: Quản lý AI và điều khiển game
- `TreeNode.cs`: Cài đặt cốt lõi của thuật toán MCTS
- `State.cs`: Quản lý trạng thái bàn cờ
- `Point.cs`: Biểu diễn vị trí trên bàn cờ
- `Util.cs`: Các hàm tiện ích

## Luồng chạy chính

### 1. Khởi tạo AI

- Tạo cây tìm kiếm với nút gốc là trạng thái hiện tại
- Khởi tạo ma trận UCT values để lưu giá trị của các nước đi

### 2. Quá trình tìm nước đi

Mỗi lượt, AI thực hiện các bước:

1. **Cập nhật cây tìm kiếm**:

   - Nếu đã có nút con phù hợp, sử dụng nút đó
   - Nếu không, tạo nút mới từ trạng thái hiện tại

2. **Lặp lại MCTS**:

   - Thực hiện `iterationNumber` lần lặp
   - Mỗi lần lặp gồm 4 bước: Selection, Expansion, Simulation, Backpropagation

3. **Chọn nước đi tốt nhất**:
   - Chọn nút con có giá trị UCT cao nhất
   - Cập nhật ma trận UCT values
   - Thực hiện nước đi được chọn

### 3. Thuật toán MCTS

#### Các thành phần chính:

1. **Selection**:

   - Chọn nút con dựa trên công thức UCT
   - UCT = (giá trị trung bình) + C \* sqrt(ln(N)/n)
   - C là hằng số khám phá
   - N là số lần thăm nút cha
   - n là số lần thăm nút con

2. **Expansion**:

   - Tạo các nút con cho nút lá
   - Mỗi nút con đại diện cho một nước đi có thể
   - Sử dụng các heuristic để lọc nước đi

3. **Simulation**:

   - Mô phỏng game từ nút lá đến kết thúc
   - Sử dụng các heuristic:
     - Kiểm tra 2 quân liên tiếp
     - Đặt quân cạnh quân cuối
     - Nước đi ngẫu nhiên

4. **Backpropagation**:
   - Cập nhật thống kê ngược lên cây
   - Cập nhật số lần thăm và tổng giá trị

### 4. Các tính năng đặc biệt

1. **Heuristic thông minh**:

   - Kiểm tra 2 quân liên tiếp để tấn công/phòng thủ
   - Đặt quân cạnh quân cuối để tạo chuỗi
   - Ưu tiên các nước đi có tiềm năng cao

2. **Tối ưu hiệu suất**:

   - Số lần lặp có thể điều chỉnh
   - Sử dụng deep clone để tránh ảnh hưởng trạng thái
   - Lưu trữ thông tin để tái sử dụng

3. **Đánh giá nước đi**:
   - Sử dụng UCT để cân bằng khám phá/khai thác
   - Cập nhật giá trị UCT cho mỗi nước đi
   - Hiển thị giá trị UCT trên bàn cờ

## Kết luận

Thuật toán MCTS trong game Caro này có những ưu điểm:

- Không cần hàm đánh giá phức tạp
- Có thể học từ kinh nghiệm chơi
- Dễ dàng điều chỉnh và mở rộng
- Hiệu quả trong không gian tìm kiếm lớn
