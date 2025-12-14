# MinRide - Hệ thống Quản lý Đặt Xe Công Nghệ

## Mô tả

MinRide là hệ thống quản lý đặt xe công nghệ được xây dựng bằng C# (.NET 7.0). Hệ thống hỗ trợ Admin quản lý dữ liệu tài xế, khách hàng, chuyến đi và xử lý đặt xe thông minh.

## Yêu cầu hệ thống

- .NET SDK 7.0 trở lên
- Windows/macOS/Linux

## Cài đặt và chạy

### 1. Clone hoặc tải project

```bash
cd MinRide
```

### 2. Khôi phục packages

```bash
dotnet restore
```

### 3. Chạy chương trình

```bash
dotnet run
```

## Cấu trúc thư mục

```
MinRide/
├── Program.cs                 # Entry point
├── MinRideSystem.cs           # Main system controller
├── MinRide.csproj             # Project configuration
├── Models/
│   ├── Driver.cs              # Driver model
│   ├── Customer.cs            # Customer model
│   └── Ride.cs                # Ride model
├── Managers/
│   ├── DriverManager.cs       # Driver CRUD operations
│   ├── CustomerManager.cs     # Customer CRUD operations
│   └── RideManager.cs         # Ride management
├── Algorithms/
│   ├── SpatialSearch.cs       # Spatial search algorithms
│   └── SortAlgorithms.cs      # Sorting algorithms (MergeSort)
├── Utils/
│   ├── FileHandler.cs         # CSV file I/O
│   ├── UndoStack.cs           # Undo functionality
│   ├── MenuHelper.cs          # Menu display utilities
│   ├── InputHelper.cs         # Input validation
│   └── ValidationHelper.cs    # Data validation
└── Data/
    ├── drivers.csv            # Driver data
    ├── customers.csv          # Customer data
    └── rides.csv              # Ride history
```

## Dữ liệu đầu vào mẫu

### Tài xế (drivers.csv)
```csv
ID,Name,Rating,X,Y,TotalRides
1,An,4.8,2,5,10
2,Bình,4.9,4,1,15
3,Cường,4.5,1,3,8
4,Dũng,4.7,5,4,12
```

### Khách hàng (customers.csv)
```csv
ID,Name,District,X,Y
1,Hoa,Q1,3,3
2,Minh,Q3,6,2
```

### Lịch sử chuyến đi (rides.csv)
```csv
RideId,CustomerId,DriverId,Distance,Fare,Timestamp,Status
1,1,2,5.2,62400,2024-12-10T10:30:00,CONFIRMED
2,2,3,3.5,42000,2024-12-11T14:15:00,CONFIRMED
```

## Các chức năng chính

### 1. Quản lý Tài xế
- Hiển thị tất cả / Top K tài xế
- Thêm, cập nhật, xóa tài xế
- Tìm kiếm theo tên hoặc ID
- Sắp xếp theo rating
- Cập nhật theo tên (xử lý trùng tên → chọn ID)

### 2. Quản lý Khách hàng
- Hiển thị tất cả / Top K khách hàng
- Thêm, cập nhật, xóa khách hàng
- Tìm kiếm theo tên hoặc ID
- Liệt kê khách hàng theo quận (phân trang)

### 3. Quản lý Chuyến đi
- Xem lịch sử chuyến đi của tài xế
- Lọc theo trạng thái (CONFIRMED, CANCELLED)
- Lọc theo khoảng thời gian
- Thống kê doanh thu, quãng đường

### 4. Tìm Tài xế Phù hợp
- Nhập ID khách hàng và bán kính R
- Tìm tài xế gần nhất trong phạm vi
- Sắp xếp theo: Khoảng cách, Rating, Số chuyến đi

### 5. Đặt xe
- Nhập ID khách hàng, ID tài xế, quãng đường
- Tự động tính Distance (bao gồm khoảng cách tài xế → khách)
- Fare = Distance × 12,000 VND
- Hỗ trợ xác nhận, hủy chuyến đi

### 6. Tự động Ghép cặp
- 3 chiến lược: Gần nhất, Rating cao nhất, Cân bằng
- Tự động mở rộng bán kính nếu không tìm thấy

### 7. Undo
- Quay lại bước trước đó
- Lưu tối đa 50 thao tác

### 8. Lưu dữ liệu
- Export ra file CSV

## Công thức tính giá

```
Fare = Total Distance × 12,000 VND
Total Distance = Khoảng cách tài xế đến khách + Khoảng cách điểm đón đến điểm đích
```

## Cấu trúc dữ liệu sử dụng

| Chức năng | CTDL | Lý do |
|-----------|------|-------|
| Lưu danh sách | List<T> | Truy cập O(1) theo index |
| Tra cứu theo ID | Dictionary<int, int> | Lookup O(1) |
| Tra cứu theo quận | Dictionary<string, List<int>> | Nhóm theo quận O(1) |
| Hàng đợi đặt xe | Queue<Ride> | FIFO |
| Lịch sử chuyến đi | LinkedList<Ride> | Thêm cuối O(1) |
| Undo Stack | Stack<Action> | LIFO |

## Thuật toán sử dụng

| Thuật toán | Độ phức tạp | Ứng dụng |
|------------|-------------|----------|
| Linear Search | O(n) | Tìm theo tên |
| Dictionary Lookup | O(1) | Tìm theo ID |
| MergeSort | O(n log n) | Sắp xếp rating |
| Euclidean Distance | O(1) | Tính khoảng cách |

## Tác giả

- Dự án NOW CHALLENGE - MinRide
- Thầy Bảo Dev Di

## License

Educational use only.

