# AI WAREHOUSE MANAGEMENT SYSTEM

## 1. Mục tiêu dự án

Xây dựng hệ thống quản lý kho cho doanh nghiệp, hỗ trợ:

- Người dùng và phân quyền
- Sản phẩm
- Danh mục
- Nhà cung cấp
- Kho và vị trí lưu trữ
- Tồn kho
- Nhập kho
- Xuất kho
- Kiểm kê
- Lịch sử biến động tồn kho
- Dashboard
- Audit Log
- AI Assistant với RAG + Tool Calling

AI Assistant có nhiệm vụ:

1. Tra cứu tài liệu nghiệp vụ bằng RAG.
2. Tra cứu dữ liệu kho thực tế thông qua Tool Calling.

---

## 2. Công nghệ chính

### Backend
- C#
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- Redis
- JWT Authentication
- FluentValidation
- Swagger / OpenAPI

### AI
- Google Gemini API
- Gemini Embedding
- RAG
- Tool Calling
- PostgreSQL
- pgvector

### Frontend
- React
- TypeScript
- Vite
- Tailwind CSS
- React Query
- Axios

### Testing
- xUnit
- Moq
- FluentAssertions

### DevOps
- Docker
- GitHub
- GitHub Actions

---

## 3. Kiến trúc

### 3-layer architecture

Frontend
  ↓
ASP.NET Core Web API
  ↓
Controller
  ↓
Service
  ↓
Repository
  ↓
EF Core
  ↓
SQL Server

### AI nằm trong Backend

AI Controller
  ↓
AI Service
  ├───────────────┐
  ↓               ↓
RAG Service      Tool Service
  ↓               ↓
pgvector         WMS Services
                     ↓
                 SQL Server

---

## 4. Roadmap triển khai 7 tuần

### Tuần 1 — Backend Foundation

Mục tiêu: dựng nền tảng backend, auth, database và cấu trúc dự án.

Task chính:
- Khởi tạo ASP.NET Core 8 API
- Thiết lập 3-layer architecture
- Cấu hình EF Core + SQL Server
- Tạo DbContext và migration đầu tiên
- Cấu hình JWT
- Role-based authorization
- Swagger/OpenAPI
- Middleware xử lý lỗi và validation
- Seed data ban đầu

Kết quả mong đợi:
- API chạy được
- Database kết nối thành công
- Login/JWT hoạt động
- Swagger hiển thị đầy đủ

---

### Tuần 2 — Master Data

Mục tiêu: hoàn thiện dữ liệu gốc của hệ thống.

Task chính:
- User & Role
- Category
- Supplier
- Unit
- Warehouse
- Warehouse Location
- Product
- Search, filter, sort, pagination

Kết quả mong đợi:
- Có thể quản lý dữ liệu nền đầy đủ
- API master data đã sẵn sàng cho nghiệp vụ kho

---

### Tuần 3 — Inventory Core

Mục tiêu: xây dựng luồng nhập kho, xuất kho, tồn kho và transaction.

Task chính:
- Inventory entity
- Goods Receipt
- Goods Issue
- InventoryTransaction
- Database transaction
- Business validation
- Kiểm tra không cho tồn kho âm

Kết quả mong đợi:
- Nhập kho tăng tồn kho đúng
- Xuất kho giảm tồn kho đúng
- Có lịch sử biến động tồn kho

---

### Tuần 4 — Inventory Advanced

Mục tiêu: nâng cấp kiểm kê, concurrency, cache, audit log, dashboard.

Task chính:
- Stocktake
- RowVersion
- Redis cache
- Redis distributed lock
- Audit Log
- Dashboard API

Kết quả mong đợi:
- Xử lý concurrent update an toàn
- Có dashboard dữ liệu kho
- Audit log đầy đủ

---

### Tuần 5 — AI RAG

Mục tiêu: xây dựng AI tra cứu SOP và tài liệu nghiệp vụ.

Task chính:
- Document management
- Upload PDF/DOCX
- Text extraction
- Chunking
- Embedding
- PostgreSQL + pgvector
- Vector search
- Gemini response

Kết quả mong đợi:
- AI trả lời câu hỏi về quy trình nghiệp vụ
- Có source rõ ràng

---

### Tuần 6 — AI Tool Calling

Mục tiêu: cho AI truy cập dữ liệu kho thực tế thông qua tool calling.

Task chính:
- AI Service
- Tool definitions
- Inventory tools
- Product tools
- Warehouse tools
- Receipt/Issue tools
- History tools
- Permission checking
- RAG + Tool Calling orchestration

Kết quả mong đợi:
- AI phân biệt đúng câu hỏi tài liệu và dữ liệu kho
- Chỉ đọc dữ liệu, không thực hiện nghiệp vụ ghi

---

### Tuần 7 — Frontend + Testing + Deployment

Mục tiêu: hoàn thiện hệ thống end-to-end.

Task chính:
- React + TypeScript + Vite + Tailwind
- Login, Dashboard, Product, Warehouse, Inventory, Goods Receipt, Goods Issue, Stocktake, AI Assistant
- Unit tests
- Docker
- GitHub Actions
- README

Kết quả mong đợi:
- Hệ thống chạy bằng Docker
- CI/CD build/test tự động
- UI hoàn thiện và có thể dùng được

---

## 5. Backlog theo mức ưu tiên

### Must Have
- Auth + Role
- Product management
- Category management
- Supplier management
- Warehouse + Location
- Inventory management
- Goods Receipt
- Goods Issue
- Stocktake
- InventoryTransaction
- Audit Log
- Dashboard API
- AI RAG
- AI Tool Calling
- Frontend MVP
- Unit tests
- Docker + CI/CD

### Should Have
- Redis cache optimization
- Advanced permission model
- Better dashboard charts
- Enhanced AI retrieval quality
- Advanced reporting

### Nice to Have
- Notification system
- Export Excel/PDF
- Demand forecasting
- Mobile app
- Transfer stock between warehouses

---

## 6. Backlog task chi tiết

### 1. Nền tảng backend
- [ ] Khởi tạo solution ASP.NET Core 8
- [ ] Thiết lập folder structure
- [ ] Cài đặt packages
- [ ] Cấu hình EF Core + SQL Server
- [ ] Tạo migration đầu tiên
- [ ] Cấu hình JWT
- [ ] Cấu hình Swagger
- [ ] Exception middleware
- [ ] Role/permission policy

### 2. Quản lý người dùng
- [ ] User CRUD
- [ ] Role CRUD
- [ ] Login/Refresh/Logout
- [ ] Change password
- [ ] Me endpoint

### 3. Master data
- [ ] Category CRUD
- [ ] Supplier CRUD
- [ ] Warehouse CRUD
- [ ] Location CRUD
- [ ] Product CRUD
- [ ] Search/filter/pagination

### 4. Inventory core
- [ ] Inventory entity and API
- [ ] Goods receipt flow
- [ ] Goods issue flow
- [ ] Inventory transaction log
- [ ] Validation không cho âm tồn kho

### 5. Stocktake + concurrency
- [ ] Stocktake workflow
- [ ] Difference calculation
- [ ] Inventory update after confirm
- [ ] RowVersion
- [ ] Redis distributed lock

### 6. Audit & Dashboard
- [ ] Audit log entity
- [ ] Audit log service
- [ ] Dashboard API
- [ ] KPI queries

### 7. AI RAG
- [ ] Document upload API
- [ ] Text extraction
- [ ] Chunking
- [ ] Embedding generation
- [ ] pgvector storage
- [ ] Vector search

### 8. AI Tool Calling
- [ ] Tool registry
- [ ] Product stock tool
- [ ] Warehouse inventory tool
- [ ] Low stock tool
- [ ] Inventory history tool
- [ ] Permission guard

### 9. Frontend
- [ ] Login page
- [ ] Dashboard page
- [ ] Product page
- [ ] Warehouse page
- [ ] Inventory page
- [ ] Receipt/Issue/Stocktake pages
- [ ] AI assistant page

### 10. Testing
- [ ] Unit test InventoryService
- [ ] Unit test GoodsReceiptService
- [ ] Unit test GoodsIssueService
- [ ] Unit test StocktakeService
- [ ] Unit test AIService
- [ ] Concurrency test

### 11. DevOps
- [ ] Docker Compose
- [ ] GitHub Actions
- [ ] Build/test pipeline
- [ ] Docker image build
- [ ] Deploy workflow

---

## 7. Kết luận

Dự án này cần được triển khai theo hướng tập trung vào 3 phần cốt lõi:

1. Core warehouse operations: nhập kho, xuất kho, kiểm kê, tồn kho, audit.
2. Data integrity: transaction, rowversion, distributed lock, validation.
3. AI assistant: RAG cho SOP và tool calling cho dữ liệu kho thực tế.

Nếu thực hiện đúng thứ tự trên, hệ thống sẽ phát triển bền vững và tránh rủi ro lớn về nghiệp vụ kho, đặc biệt là vấn đề tồn kho và concurrency.

---

## 8. Gợi ý thực thi tiếp theo

Nếu muốn tiến nhanh, nên làm tiếp theo thứ tự:

1. Chốt database schema
2. Chốt role permission matrix
3. Build backend auth + master data
4. Build inventory/receipt/issue/stocktake
5. Test concurrency và audit
6. Build AI RAG
7. Build AI Tool Calling
8. Build frontend
9. Docker + CI/CD

---

## 9. Tóm tắt ngắn

AI WAREHOUSE MANAGEMENT SYSTEM =
- Backend: ASP.NET Core 8 + SQL Server + Redis
- AI: Gemini + pgvector + RAG + Tool Calling
- Frontend: React + TypeScript + Vite + Tailwind
- Testing: xUnit + Moq + FluentAssertions
- Deployment: Docker + GitHub Actions

---
