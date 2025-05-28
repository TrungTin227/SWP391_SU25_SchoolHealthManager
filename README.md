# School Health Management System - Backend API

Hệ thống quản lý sức khỏe trường học được xây dựng bằng .NET 8.0 với kiến trúc Clean Architecture.

## 🏗️ Kiến trúc dự án

```
SWP391_SU25_SchoolHealthManager/
├── BusinessObjects/     # Entities và Models
├── DTOs/               # Data Transfer Objects  
├── Repositories/       # Data Access Layer
├── Services/           # Business Logic Layer
└── WebAPI/            # Presentation Layer (Controllers)
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

## 🚀 Cách setup dự án

### 1. Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 hoặc VS Code

### 2. Clone và restore packages
```bash
git clone https://github.com/TrungTin227/SWP391_SU25_SchoolHealthManager.git
cd SWP391_SU25_SchoolHealthManager
dotnet restore
```

### 3. Cập nhật connection string
Chỉnh sửa file `appsettings.json` trong project WebAPI:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=SchoolHealthDB;Trusted_Connection=true;"
  }
}
```

### 4. Migration và chạy project
```bash
# Tạo database
dotnet ef database update --project WebAPI

# Chạy project
dotnet run --project WebAPI
```

## 🎯 Tính năng chính
- **JWT Authentication**: Xác thực người dùng
- **Email Service**: Gửi email tự động (MailKit)
- **Background Jobs**: Lập lịch công việc (Quartz)
- **API Documentation**: Swagger UI
- **Entity Framework**: ORM cho database

## 🔧 Cấu trúc Dependencies
- WebAPI → Services → Repositories → BusinessObjects
- DTOs được sử dụng ở tất cả các layer

*Lưu ý: Danh sách packages có thể không đầy đủ. [Xem thêm tại GitHub](https://github.com/TrungTin227/SWP391_SU25_SchoolHealthManager/search?q=*.csproj)*
