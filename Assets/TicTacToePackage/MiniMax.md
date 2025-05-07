# Thuật toán MiniMax trong Game Caro

## Tổng quan

Thuật toán MiniMax được triển khai trong 2 file chính:

- `TicTacToeManager.cs`: Quản lý logic game và thuật toán MiniMax
- `ButtonCell.cs`: Quản lý UI của từng ô cờ

## Luồng chạy chính

### 1. Khởi tạo game

- Tạo bàn cờ kích thước `boardSize x boardSize` (mặc định 10x10)
- Khởi tạo mảng 2 chiều `board` để lưu trạng thái bàn cờ
- Tạo các ô cờ UI và gán sự kiện click

### 2. Luồng chơi game

#### Lượt người chơi:

1. Người chơi click vào ô cờ
2. `HandlePlayerMove()` được gọi:
   - Kiểm tra lượt và ô trống
   - Đánh dấu "O" vào ô được chọn
   - Kiểm tra thắng/thua/hòa
   - Nếu chưa kết thúc, chuyển lượt cho máy

#### Lượt máy (AI):

1. `PlayerAIMove()` được gọi sau 0.3s
2. AI thực hiện các bước:
   - Kiểm tra nước đi thắng ngay (`FindImmediateMove()`)
   - Nếu không có nước thắng ngay, sử dụng MiniMax để tìm nước đi tốt nhất
   - Đánh dấu "X" vào ô được chọn
   - Kiểm tra thắng/thua/hòa

### 3. Thuật toán MiniMax

#### Các thành phần chính:

1. **Hàm MiniMax**:

   - Sử dụng Alpha-Beta Pruning để tối ưu
   - Độ sâu tìm kiếm thay đổi theo số quân trên bàn (3-4)
   - Trả về nước đi tốt nhất và điểm số

2. **Đánh giá bàn cờ**:

   - Tính điểm dựa trên số quân liên tiếp
   - Ưu tiên các ô ở giữa bàn cờ
   - Phòng thủ được đánh giá cao hơn tấn công

3. **Tìm nước đi thông minh**:
   - Chỉ xét các ô xung quanh quân đã đánh
   - Sắp xếp các nước đi theo độ ưu tiên
   - Ưu tiên các nước tạo được chuỗi dài

### 4. Các tính năng đặc biệt

1. **Tìm nước đi thắng ngay**:

   - Kiểm tra nước thắng của máy
   - Kiểm tra nước chặn thắng của người chơi

2. **Đánh giá sức mạnh nước đi**:

   - Tính điểm dựa trên số quân liên tiếp
   - Xét không gian trống xung quanh
   - Ưu tiên các nước tạo được nhiều cơ hội

3. **Tối ưu hiệu suất**:
   - Alpha-Beta Pruning
   - Giới hạn độ sâu tìm kiếm
   - Chỉ xét các ô có khả năng cao

## Kết luận

Thuật toán MiniMax trong game Caro này được tối ưu tốt với:

- Chiến lược tìm kiếm thông minh
- Đánh giá bàn cờ toàn diện
- Tối ưu hiệu suất
- Khả năng phòng thủ và tấn công cân bằng
