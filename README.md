# School Health Management System - Backend API

Hệ thống quản lý sức khỏe trường học được xây dựng bằng .NET 8.0 với kiến trúc Clean Architecture.

## 🏗️ Kiến trúc dự án

```
SWP391_SU25_SchoolHealthManager/
├── BusinessObjects/     # Entities và Models
├── DTOs/               # Data Transfer Objects
├── Repositories/       # Data Access Layer
├── Services/           # Business Logic Layer
└── WebAPI/             # Presentation Layer (Controllers)
```

## 📦 NuGet Packages chính

### WebAPI Layer
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.16" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.16" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

### Repositories Layer
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.16" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.16" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.16" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
```

### Services Layer
```xml
<PackageReference Include="MailKit" Version="4.12.1" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.3.0" />
<PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="8.0.16" />
<PackageReference Include="Quartz" Version="3.14.0" />
<PackageReference Include="Quartz.Extensions.Hosting" Version="3.14.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.10.0" />
```

## 🚀 Hướng dẫn cài đặt & khởi chạy hệ thống

### 1. Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server (bất kỳ phiên bản nào hỗ trợ Entity Framework 8)
- Visual Studio 2022 hoặc VS Code

### 2. Clone mã nguồn và khôi phục package

```bash
git clone https://github.com/TrungTin227/SWP391_SU25_SchoolHealthManager.git
cd SWP391_SU25_SchoolHealthManager
dotnet restore
```

### 3. Cấu hình chuỗi kết nối Database

Chỉnh sửa file `appsettings.json` trong thư mục `WebAPI`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=SchoolHealthDB;Trusted_Connection=true;"
  }
}
```
- Thay `your_server` bằng tên hoặc địa chỉ SQL Server thực tế.
- Nếu sử dụng SQL Authentication, thay `Trusted_Connection=true;` bằng `User Id=<username>;Password=<password>;`.

### 4. Khởi tạo Database và Migration

```bash
# Tạo database và apply migration
dotnet ef database update --project WebAPI
```

### 5. Chạy hệ thống

```bash
dotnet run --project WebAPI
```
- Ứng dụng API sẽ chạy trên http://localhost:5039    https://localhost:7096 (hoặc cổng như cấu hình trong `launchSettings.json`).
- Truy cập tài liệu API bằng Swagger tại http://localhost:7096/swagger

### 6. Các biến môi trường thường gặp

- `ASPNETCORE_ENVIRONMENT` : `Development` | `Production`
- `ConnectionStrings__DefaultConnection` : override chuỗi kết nối qua biến môi trường nếu cần.

### 7. Hệ thống gửi mail

- Chỉnh sửa cấu hình email trong `appsettings.json` (nếu có gửi mail thực tế):
```json
"EmailSettings": {
  "SmtpServer": "smtp.example.com",
  "SmtpPort": 587,
  "SenderName": "School Health System",
  "SenderEmail": "noreply@example.com",
  "Username": "username",
  "Password": "password"
}
```
- Đảm bảo tài khoản email có quyền gửi mail SMTP.

### 8. Lưu ý khác

- Chạy lệnh migrate chỉ cần thực hiện lần đầu hoặc khi có cập nhật database.
- Nếu gặp lỗi EF CLI chưa cài đặt, cài thêm:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## 🎯 Tính năng chính

- Phụ huynh khai báo hồ sơ sức khỏe học sinh: dị ứng, bệnh mãn tính, tiền sử điều trị, thị lực, thính lực, tiêm chủng,...
- Phụ huynh gửi thuốc, nhân viên y tế cho học sinh uống thuốc.
- Ghi nhận, xử lý sự kiện y tế (tai nạn, sốt, dịch bệnh, ...).
- Quản lý thuốc, vật tư y tế.
- Quản lý quá trình tiêm chủng, kiểm tra y tế định kỳ.
- Quản lý hồ sơ người dùng, lịch sử sức khỏe.
- Dashboard & báo cáo.
- Authentication JWT, gửi email tự động, background job, API doc qua Swagger.

---

## 🔧 Cấu trúc phụ thuộc

- WebAPI → Services → Repositories → BusinessObjects
- DTOs sử dụng ở tất cả các layer

---

## ❓ Hỗ trợ

Nếu gặp vấn đề khi setup, hãy kiểm tra lại:
- Đã cài đúng .NET SDK 8.0+
- Cấu hình đúng chuỗi kết nối database
- SQL Server đã bật và cho phép kết nối
- Đã apply migration trước khi chạy

Nếu vẫn gặp lỗi, hãy tạo issue kèm log cụ thể.

---

*Lưu ý: Danh sách package có thể chưa đầy đủ. Xem thêm các file .csproj trong từng layer nếu cần.*
