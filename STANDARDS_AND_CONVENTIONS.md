# Quy định tiêu chuẩn Frontend và Backend chuẩn Senior

## 1. Mục tiêu

Tài liệu này định nghĩa bộ nguyên tắc phát triển, cấu trúc mã nguồn, convention, quality gate và workflow làm việc cho dự án AI Warehouse Management System. Mục tiêu là đảm bảo:

- Dễ đọc, dễ bảo trì, dễ mở rộng
- Có tính nhất quán giữa Backend và Frontend
- Có chuẩn code và chuẩn review rõ ràng
- Dễ test, dễ debug, dễ deploy
- Dễ chuyển đổi và mở rộng cho team lớn

---

## 2. Nguyên tắc nền tảng

### 2.1 Tối ưu cho tính rõ ràng trước hiệu năng
- Ưu tiên code dễ hiểu và dễ kiểm soát hơn code tối ưu quá mức nhưng khó bảo trì.
- Chỉ tối ưu khi có bằng chứng thật sự cần thiết.

### 2.2 Mỗi module phải có trách nhiệm rõ ràng
- Một class/service/controller chỉ làm một tập hợp nhiệm vụ tương đối rõ.
- Không gom quá nhiều logic vào một file lớn.

### 2.3 Tối thiểu hóa phụ thuộc chéo
- Domain logic không phụ thuộc trực tiếp vào controller, UI hay framework.
- Service không nên lẫn thao tác HTTP, UI state, hay SQL query lặp lại.

### 2.4 Viết code theo business value trước
- Mỗi feature phải giải quyết đúng nghiệp vụ kho, không chỉ chạy được.
- Đầu tiên là nghiệp vụ đúng, sau đó là refactor và tối ưu.

### 2.5 Không để “code bí ẩn”
- Tên biến, tên hàm, tên class phải mô tả rõ ý nghĩa.
- Không dùng tên quá ngắn hoặc không rõ ràng như `x`, `obj`, `data1`.

---

## 3. Backend standards

## 3.1 Kiến trúc

### 3.1.1 Dùng 3-layer architecture chuẩn

- Controller: xử lý HTTP, validation đầu vào, response
- Service: business logic và orchestration
- Repository / Data access: truy vấn và persistence
- EF Core: dữ liệu cuối cùng

Nội dung nên tách rõ:

- Controllers/
- Services/
- Repositories/
- Entities/
- DTOs/
- Validators/
- Data/
- Helpers/
- Middleware/

### 3.1.2 Mỗi module có file structure rõ ràng
Ví dụ:

- Services/
  - ProductService.cs
  - IProductService.cs
- Controllers/
  - ProductsController.cs
- Models/
  - Products/
    - ProductDto.cs
    - CreateProductRequest.cs
    - UpdateProductRequest.cs

### 3.1.3 Không viết business logic trong controller
Controller chỉ:
- nhận request
- validate cơ bản
- gọi service
- trả response

---

## 3.2 Naming conventions

### 3.2.1 Tên class
- PascalCase
- Tên class phải là danh từ hoặc mô tả nghiệp vụ
- Ví dụ:
  - ProductService
  - GoodsReceiptService
  - InventoryService
  - AuthController

### 3.2.2 Tên method
- PascalCase
- Là động từ hoặc cụm động từ rõ nghĩa
- Ví dụ:
  - CreateAsync
  - UpdateAsync
  - ConfirmAsync
  - GetLowStockProductsAsync

### 3.2.3 Tên biến
- camelCase
- Có ý nghĩa
- Ví dụ:
  - productId
  - warehouseCode
  - availableQuantity

### 3.2.4 Tên file
- Nên theo tên class hoặc module
- Ví dụ:
  - ProductService.cs
  - AuthController.cs
  - CreateProductRequest.cs

---

## 3.3 Cấu trúc response chuẩn

### 3.3.1 Mỗi API phải trả response chuẩn
- Không trả text thô nếu không cần
- Dùng JSON với shape rõ ràng

Ví dụ:

```json
{
  "success": true,
  "data": { ... },
  "message": "Created successfully"
}
```

Hoặc cho lỗi:

```json
{
  "success": false,
  "message": "Product not found",
  "errors": ["Product with id ... does not exist"]
}
```

### 3.3.2 Error handling chuẩn
- Không để exception leak ra client quá mức
- Dùng middleware để xử lý lỗi thống nhất
- Dùng status code đúng:
  - 200 OK
  - 201 Created
  - 204 No Content
  - 400 Bad Request
  - 401 Unauthorized
  - 403 Forbidden
  - 404 Not Found
  - 409 Conflict
  - 500 Internal Server Error

---

## 3.4 Business rules

### 3.4.1 Không làm nghiệp vụ quan trọng ở controller
Logic nghiệp vụ phải ở service layer.

Ví dụ:
- Kiểm tra không cho xuất vượt tồn kho
- Tính available quantity
- Tạo inventory transaction
- Confirm receipt/issue/stocktake

### 3.4.2 Luôn kiểm tra tồn kho trước khi ghi
- Không cho lưu số liệu sai
- Không cho tồn kho âm
- Không cho confirm nếu điều kiện không hợp lệ

### 3.4.3 Dùng transaction cho nghiệp vụ quan trọng
Các operation cần transaction:
- Goods receipt confirmation
- Goods issue confirmation
- Stocktake confirmation
- Inventory update

### 3.4.4 Dùng kiểm tra dữ liệu đầu vào nghiêm ngặt
- Validate required fields
- Validate status enum
- Validate quantity > 0
- Validate SKU/barcode uniqueness

---

## 3.5 EF Core và database standards

### 3.5.1 Dùng DbContext trung tâm
- Không viết SQL trực tiếp nếu không thực sự cần
- Dùng EF Core query và repository pattern rõ ràng

### 3.5.2 Định nghĩa index cho field unique
- SKU, Barcode, Warehouse Code, Location Code
- email, username

### 3.5.3 Sử dụng soft delete thay vì xóa thật nếu phù hợp
- `IsDeleted` cho entity chính
- Hạn chế xóa mất dữ liệu lịch sử

### 3.5.4 Có audit cho thao tác quan trọng
- CREATE
- UPDATE
- DELETE
- CONFIRM
- CANCEL

### 3.5.5 Không để entity lẫn quá nhiều trách nhiệm
- Entity chỉ là model dữ liệu, không chứa logic nghiệp vụ phức tạp

---

## 3.6 Security standards

### 3.6.1 Mọi API phải có auth nếu cần
- JWT access token
- Refresh token nếu cần
- Role-based authorization

### 3.6.2 Không cho AI hoặc client thực hiện quyền ghi nếu không cần
- AI assistant chỉ đọc dữ liệu kho trong phiên bản hiện tại
- Không cho phép AI xác nhận phiếu, xóa dữ liệu

### 3.6.3 Không lưu mật khẩu ở dạng plain text
- Dùng bcrypt, PBKDF2 hoặc tiêu chuẩn bảo mật tương đương

### 3.6.4 Không lộ thông tin nhạy cảm trong response
- Không trả password hash
- Không trả secret token

---

## 3.7 Testing standards

### 3.7.1 Test theo behavior thật
- Unit test logic nghiệp vụ
- Integration test service flow
- Không test mock behavior

### 3.7.2 Test cases bắt buộc cho module kho
- Receipt increases inventory
- Issue decreases inventory
- Cannot issue insufficient stock
- Stocktake calculates difference
- Concurrent issue is handled correctly
- Low stock detection works

### 3.7.3 Dùng xUnit + FluentAssertions + Moq
- Dùng FluentAssertions cho assertion rõ ràng
- Dùng Moq cho dependency mock khi cần thiết

---

## 4. Frontend standards

## 4.1 Cấu trúc frontend

### 4.1.1 Tổ chức dự án rõ ràng
Ví dụ:

- src/
  - app/
  - components/
  - features/
  - pages/
  - hooks/
  - services/
  - utils/
  - types/
  - store/

### 4.1.2 Tách feature theo chức năng
Ví dụ:

- features/products
- features/inventory
- features/goods-receipt
- features/ai-assistant

Không nên gom tất cả logic trong một file lớn.

---

## 4.2 React/TypeScript standards

### 4.2.1 Dùng TypeScript chặt chẽ
- Không dùng any nếu không cần thiết
- Tạo type cho request/response API rõ ràng

### 4.2.2 Dùng component nguyên tử và feature-based
- Component nhỏ, rõ mục đích
- Không nhồi logic vào JSX quá nhiều

### 4.2.3 Tách logic API khỏi component
- Tất cả API call nên nằm trong service layer hoặc hook
- component chỉ render UI và gọi sự kiện

### 4.2.4 Không lạm dụng state cục bộ
- Nếu dữ liệu dùng nhiều nơi, nên đưa vào store hoặc query cache

---

## 4.3 UI/UX standards

### 4.3.1 Không dùng gradient làm điểm nhấn chính
- Giao diện nên nhẹ, rõ, dễ đọc, không gây rối mắt
- Dùng màu theo ý nghĩa chức năng

### 4.3.2 UI phải hướng human-first
- Người dùng kho cần thấy dữ liệu chính ngay
- Trạng thái phải rõ ràng: healthy / low stock / out of stock / draft / confirmed

### 4.3.3 Không làm UI quá nhiều màu, quá nhiều hiệu ứng
- Dùng spacing, hierarchy, typography và màu hiệu quả hơn.

### 4.3.4 Tất cả action quan trọng phải thấy ngay
- Confirm
- Cancel
- Delete
- Save

### 4.3.5 Form phải rõ ràng, không quá dài, có group hợp lý
- Group field theo chủ đề
- Có validation inline
- Không để user đoán trường nào bắt buộc

---

## 4.4 UX trong hệ thống kho

### 4.4.1 Màn hình inventory phải rất đọc nhanh
- Số lượng, vị trí, trạng thái phải dễ nhìn ngay
- Hàng thiếu kho phải có màu cảnh báo rõ ràng

### 4.4.2 Màn hình báo cáo / dashboard phải cho thấy số liệu chính ngay
- KPI cards
- trend / chart
- danh sách cần xử lý

### 4.4.3 Màn hình AI Assistant nên rõ ràng
- Chat history
- answer + sources
- badge phân biệt RAG vs Tool Calling

---

## 4.5 State management và API

### 4.5.1 Dùng React Query cho dữ liệu server
- cache dữ liệu
- invalidate dữ liệu sau mutation
- đồng bộ dữ liệu dễ hơn

### 4.5.2 Dùng Axios hoặc service layer
- Mỗi endpoint nên có service riêng hoặc module riêng
- Không mix trực tiếp call API trong component

### 4.5.3 Dùng form library nếu form phức tạp
- React Hook Form + Zod là lựa chọn phù hợp

---

## 4.6 Accessibility và usability

- Dùng contrast đủ cho text
- Dùng aria-label nếu cần thiết
- Focus state rõ ràng
- Button label rõ ràng
- Modal không quá phức tạp
- Hỗ trợ keyboard navigation cơ bản

---

## 5. Git workflow

### 5.1 Branch conventions
- feature/xxx
- fix/xxx
- hotfix/xxx

### 5.2 Commit conventions
- feat: add product module
- fix: inventory stock validation
- refactor: separate product service layer
- docs: update API standards

### 5.3 Review checklist
- Mã có test hay không
- Có validation không
- Có lỗi lạ không
- Có logging và audit không
- Có permission không
- Có lỗi concurrency không

---

## 6. Code review checklist

### 6.1 Backend checklist
- [ ] Business logic ở service layer
- [ ] Không lẫn logic UI trong API
- [ ] Validation đầy đủ
- [ ] Dùng transaction cho thao tác kho quan trọng
- [ ] Có audit log khi cần
- [ ] Không cho tồn kho âm
- [ ] Có test cho nghiệp vụ chính

### 6.2 Frontend checklist
- [ ] Màn hình rõ và dễ đọc
- [ ] Không quá nhiều khoảng trắng hoặc màu sắc rối
- [ ] Form validation rõ
- [ ] Data fetching qua service/hook
- [ ] Loading + error + empty states đầy đủ
- [ ] Có role-based access đúng

---

## 7. Definition of Done (DoD)

Một task được xem là hoàn thành khi:

- Code build thành công
- Có test coverage cho logic chính
- Không có lỗi compile
- Đã review code
- API response chuẩn
- UI có trạng thái loading/error/empty
- Thương mại nghiệp vụ đã được kiểm tra bằng dữ liệu giả hoặc thực tế
- Không có lộ dữ liệu nhạy cảm

---

## 8. Kết luận

Quy định này nhằm tạo ra một dự án có chuẩn senior và dễ phát triển lâu dài.

Dự án kho cần ưu tiên:

- Độ đúng của nghiệp vụ
- An toàn dữ liệu
- Dễ sử dụng cho người vận hành
- Dễ mở rộng cho team lớn
- Tính nhất quán giữa backend và frontend

Nếu mọi thành viên tuân thủ các tiêu chuẩn này, dự án sẽ bền vững, dễ review, dễ test và dễ triển khai.

---

## 9. Gợi ý thực thi hàng ngày

- Mỗi task phải có mục tiêu rõ ràng
- Mỗi module nhỏ phải test được
- Không code quá lớn trong một commit
- Sau mỗi module, build lại và test lại
- Luôn kiểm tra business rule trước khi làm UI

---
