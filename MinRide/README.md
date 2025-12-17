# MinRide - Hệ thống Quản lý Đặt Xe Công Nghệ

## Mô tả

MinRide là hệ thống quản lý đặt xe công nghệ được xây dựng bằng C# (.NET 8.0). Hệ thống hỗ trợ Admin quản lý dữ liệu tài xế, khách hàng, chuyến đi và xử lý đặt xe thông minh với mô phỏng thời gian di chuyển thực tế.

## Yêu cầu hệ thống

- .NET SDK 8.0 trở lên
- Windows/macOS/Linux

## Cài đặt và chạy

```bash
cd MinRide
dotnet restore
dotnet run
```

## Luồng xử lý chuyến đi (Ride Flow)

```
┌─────────────┐      2 phút      ┌──────────────┐    distance×15s    ┌───────────────┐
│   PENDING   │ ───────────────► │ IN_PROGRESS  │ ─────────────────► │   COMPLETED   │
│ (Đang chờ)  │   auto-start     │ (Đang chạy)  │    auto-complete   │ (Hoàn thành)  │
└─────────────┘                  └──────────────┘                    └───────────────┘
      │                                                                      │
      │ Hủy trong 2 phút                                                     │
      ▼                                                                      ▼
┌─────────────┐                                                      Lưu vào CSV
│  CANCELLED  │                                                      TotalRides++
│   (Đã hủy)  │
└─────────────┘
```

### Quy tắc:
- **PENDING → IN_PROGRESS**: Sau 2 phút hoặc xác nhận thủ công
- **IN_PROGRESS → COMPLETED**: Sau `distance × 15 giây` (1km = 15s)
- **Hủy chuyến**: Chỉ được hủy trong 2 phút đầu (khi còn PENDING)
- **TotalRides**: Chỉ tăng khi chuyến đi COMPLETED

## Cấu trúc thư mục

```
MinRide/
├── Program.cs                 # Entry point
├── MinRideSystem.cs           # Main system controller
├── Models/
│   ├── Driver.cs              # Driver model với rating, location
│   ├── Customer.cs            # Customer model với district
│   └── Ride.cs                # Ride model với status flow
├── Managers/
│   ├── DriverManager.cs       # CRUD + Search + Sort cho tài xế
│   ├── CustomerManager.cs     # CRUD + District grouping
│   └── RideManager.cs         # Pending/InProgress/Completed management
├── Algorithms/
│   ├── SpatialSearch.cs       # Tìm kiếm theo khoảng cách
│   └── SortAlgorithms.cs      # MergeSort implementation
├── Utils/
│   ├── FileHandler.cs         # CSV I/O
│   ├── UndoStack.cs           # Undo với Stack
│   └── DataGenerator.cs       # Sinh dữ liệu mẫu
└── Data/
    ├── drivers.csv
    ├── customers.csv
    └── rides.csv
```

---

## Cấu trúc dữ liệu (Data Structures)

| CTDL | Ứng dụng | Độ phức tạp | Lý do chọn |
|------|----------|-------------|------------|
| **List\<T\>** | Lưu danh sách tài xế, khách hàng | O(1) truy cập | Random access nhanh |
| **Dictionary\<int, int\>** | Map ID → Index | O(1) lookup | Tìm kiếm theo ID cực nhanh |
| **Dictionary\<string, List\<int\>\>** | Nhóm khách theo quận | O(1) lookup | Truy vấn theo nhóm |
| **Queue\<Ride\>** | Hàng đợi chuyến đi PENDING | O(1) enqueue/dequeue | FIFO - xử lý theo thứ tự đặt |
| **List\<Ride\>** | Chuyến đi IN_PROGRESS | O(n) search | Cần duyệt để check completion |
| **LinkedList\<Ride\>** | Lịch sử COMPLETED | O(1) AddLast | Thêm cuối nhanh, không cần resize |
| **Stack\<Action\>** | Undo operations | O(1) push/pop | LIFO - hoàn tác theo thứ tự ngược |

### Sơ đồ CTDL cho Ride Management:

```
                    ┌─────────────────────┐
                    │    CreateRide()     │
                    └──────────┬──────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                    Queue<Ride> pendingRides                  │
│  [Ride1] → [Ride2] → [Ride3] → ...                          │
│  FIFO: Đặt trước xử lý trước                                │
└──────────────────────────────────────────────────────────────┘
                               │
                               │ Start() - sau 2 phút
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                   List<Ride> inProgressRides                 │
│  [Ride1, Ride2, ...]                                        │
│  Mỗi ride có ExpectedCompletionTime                         │
└──────────────────────────────────────────────────────────────┘
                               │
                               │ Complete() - sau distance×15s
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                LinkedList<Ride> rideHistory                  │
│  [Ride1] ↔ [Ride2] ↔ [Ride3] ↔ ...                          │
│  Doubly linked: thêm cuối O(1)                              │
└──────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│           Dictionary<int, List<Ride>> driverRides            │
│  {                                                          │
│    1: [Ride1, Ride5, ...],  // Tài xế ID=1                  │
│    2: [Ride2, Ride3, ...],  // Tài xế ID=2                  │
│  }                                                          │
│  O(1) lookup theo DriverId                                  │
└──────────────────────────────────────────────────────────────┘
```

---

## Thuật toán (Algorithms)

### 1. Dictionary Lookup - O(1)

**Ứng dụng**: Tìm tài xế/khách hàng theo ID

```csharp
// Thay vì duyệt O(n):
foreach (var driver in drivers)
    if (driver.Id == id) return driver;

// Dùng Dictionary O(1):
if (idToIndex.TryGetValue(id, out int index))
    return drivers[index];
```

**Ưu điểm**: Cực nhanh cho lookup theo key
**Nhược điểm**: Tốn thêm bộ nhớ cho Dictionary

---

### 2. Linear Search - O(n)

**Ứng dụng**: Tìm theo tên (partial match)

```csharp
drivers.Where(d => d.Name.Contains(searchName, 
    StringComparison.OrdinalIgnoreCase))
```

**Ưu điểm**: Đơn giản, hỗ trợ partial match
**Nhược điểm**: Chậm với dataset lớn

---

### 3. MergeSort - O(n log n)

**Ứng dụng**: Sắp xếp tài xế theo Rating

```csharp
// Stable sort - giữ thứ tự tương đối
drivers.OrderByDescending(d => d.Rating).ToList();
```

**Ưu điểm**: 
- Stable sort (giữ thứ tự gốc khi rating bằng nhau)
- Worst case O(n log n)

**Nhược điểm**: Tốn O(n) bộ nhớ phụ

---

### 4. Euclidean Distance - O(1)

**Ứng dụng**: Tính khoảng cách tài xế → khách hàng

```csharp
double distance = Math.Sqrt(
    Math.Pow(driver.X - customer.X, 2) + 
    Math.Pow(driver.Y - customer.Y, 2)
);
```

**Ưu điểm**: Chính xác cho hệ tọa độ 2D
**Nhược điểm**: Không phản ánh đường đi thực tế (đường phố)

---

### 5. Spatial Search with Radius - O(n)

**Ứng dụng**: Tìm tài xế trong bán kính R km

```csharp
drivers
    .Select(d => (Distance: d.DistanceTo(location), Driver: d))
    .Where(t => t.Distance <= radius)
    .OrderBy(t => t.Distance)
    .ThenByDescending(t => t.Driver.Rating);
```

**Ưu điểm**: Kết hợp nhiều tiêu chí (khoảng cách + rating)
**Nhược điểm**: O(n) - có thể cải thiện với Spatial Index (R-tree, QuadTree)

---

### 6. Time-based Auto Processing

**Ứng dụng**: Tự động xử lý chuyến đi theo thời gian

```csharp
// Check if ride can be cancelled (within 2 minutes)
public bool CanBeCancelled() {
    TimeSpan elapsed = DateTime.Now - Timestamp;
    return elapsed.TotalMinutes < 2;
}

// Check if ride has finished traveling
public bool HasFinishedTraveling() {
    return DateTime.Now >= ExpectedCompletionTime;
}
```

**Công thức thời gian di chuyển**:
```
TravelTime (seconds) = Distance (km) × 15
ExpectedCompletionTime = StartTime + TravelTime
```

---

## Ưu điểm của chương trình

### 1. Hiệu suất (Performance)
- ✅ O(1) lookup theo ID với Dictionary
- ✅ O(n log n) sorting với stable sort
- ✅ O(1) thêm chuyến đi với LinkedList

### 2. Tính năng (Features)
- ✅ Mô phỏng thời gian thực (1km = 15s)
- ✅ Hủy chuyến trong 2 phút đầu
- ✅ Tự động xử lý chuyến đi
- ✅ Undo/Redo operations
- ✅ Lưu/Load từ CSV

### 3. Code Quality
- ✅ Separation of Concerns (Models/Managers/Utils)
- ✅ XML Documentation
- ✅ Validation ở nhiều tầng
- ✅ Error handling với try-catch

---

## Nhược điểm và hạn chế

### 1. Hiệu suất (Performance)
- ❌ Linear Search O(n) cho tìm theo tên - có thể dùng Trie
- ❌ Spatial Search O(n) - có thể dùng R-tree/QuadTree
- ❌ Không có caching/indexing cho queries phức tạp

### 2. Tính năng (Features)
- ❌ Không có real-time notification
- ❌ Khoảng cách Euclidean không phản ánh đường thực
- ❌ Không hỗ trợ multi-threading
- ❌ Chưa có payment system

### 3. Scalability
- ❌ In-memory storage - không phù hợp dataset lớn
- ❌ Single instance - không hỗ trợ distributed system
- ❌ CSV storage - không optimal cho concurrent access

---

## Cải tiến đề xuất

| Vấn đề | Giải pháp | Độ phức tạp mới |
|--------|-----------|-----------------|
| Linear search theo tên | Trie hoặc Suffix Tree | O(m) với m = độ dài search |
| Spatial search O(n) | R-tree hoặc QuadTree | O(log n) |
| In-memory storage | Database (SQLite/PostgreSQL) | Persistent + Indexing |
| Single thread | Async/await + Background tasks | Non-blocking |
| Euclidean distance | Google Maps API / OSM | Real-world routing |

---

## Công thức tính giá

```
Fare = Total Distance × 12,000 VND
Total Distance = Khoảng cách tài xế→khách + Khoảng cách đón→đích
Travel Time = Total Distance × 15 seconds
```

---

## Các chức năng chính

### 1. Quản lý Tài xế
- CRUD operations (thêm/sửa/xóa)
- Tìm kiếm theo ID (O(1)) hoặc tên (O(n))
- Sắp xếp theo rating (MergeSort)
- Top K tài xế theo rating

### 2. Quản lý Khách hàng
- CRUD operations
- Phân nhóm theo quận/huyện
- Pagination cho danh sách

### 3. Quản lý Chuyến đi
- Xem PENDING / IN_PROGRESS / COMPLETED
- Hủy chuyến (trong 2 phút)
- Tự động xử lý theo thời gian
- Lịch sử theo tài xế

### 4. Tìm & Ghép tài xế
- Tìm trong bán kính R km
- 3 chiến lược: Gần nhất / Rating cao / Cân bằng
- Tự động mở rộng bán kính

### 5. Undo
- Stack-based undo (LIFO)
- Tối đa 50 operations

---

## Tác giả

- Dự án NOW CHALLENGE - MinRide
- Nhóm 7

## License

Educational use only.
