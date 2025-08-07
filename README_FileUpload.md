# Hướng dẫn sử dụng tính năng Upload File cho ParentMedicationDelivery

## Tổng quan

Tính năng upload file đã được thêm vào class `ParentMedicationDelivery` để cho phép phụ huynh upload hình ảnh đơn thuốc khi tạo phiếu giao thuốc.

## Các thay đổi đã thực hiện

### 1. Database Changes
- Thêm enum `ParentMedicationDelivery` vào `ReferenceType`
- Thêm quan hệ `Attachments` vào class `ParentMedicationDelivery`
- Tạo migration `AddParentMedicationDeliveryAttachments`

### 2. Service Layer
- Tạo `IFileStorageService` interface
- Implement `FileStorageService` để xử lý upload file vào local storage
- Cập nhật `ParentMedicationDeliveryService` để xử lý upload file

### 3. DTO Changes
- Cập nhật `CreateParentMedicationDeliveryRequestDTO` để hỗ trợ `List<IFormFile> PrescriptionImages`
- Cập nhật `ParentMedicationDeliveryResponseDTO` để bao gồm `List<string> AttachmentUrls`

### 4. Controller Changes
- Cập nhật `ParentController` để sử dụng `[FromForm]` thay vì `[FromBody]`
- Tạo `FileController` để xử lý download và delete file

## Cách sử dụng

### 1. Upload file khi tạo ParentMedicationDelivery

**Endpoint:** `POST /api/parents/medication-deliveries`

**Content-Type:** `multipart/form-data`

**Request Body:**
```json
{
  "studentId": "guid",
  "notes": "string",
  "medications": [
    {
      "medicationName": "string",
      "quantityDelivered": 10,
      "dosageInstruction": "string",
      "dailySchedule": [
        {
          "time": "08:00:00",
          "dosage": 1,
          "note": "string"
        }
      ]
    }
  ],
  "prescriptionImages": [file1, file2, ...] // Optional
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Tạo phiếu giao thuốc thành công!",
  "data": {
    "id": "guid",
    "studentId": "guid",
    "studentName": "string",
    "parentId": "guid",
    "receivedBy": "guid",
    "notes": "string",
    "deliveredAt": "2024-01-01T00:00:00",
    "status": "Pending",
    "medications": [...],
    "attachmentUrls": [
      "/uploads/prescriptions/guid_filename1.jpg",
      "/uploads/prescriptions/guid_filename2.jpg"
    ]
  }
}
```

### 2. Download file

**Endpoint:** `GET /api/files/{filePath}`

**Example:** `GET /api/files/prescriptions/guid_filename.jpg`

### 3. Delete file (Admin/SchoolNurse only)

**Endpoint:** `DELETE /api/files/{filePath}`

**Example:** `DELETE /api/files/prescriptions/guid_filename.jpg`

## Cấu trúc thư mục

Files sẽ được lưu trong thư mục:
```
wwwroot/
└── uploads/
    └── prescriptions/
        ├── guid_filename1.jpg
        ├── guid_filename2.png
        └── ...
```

## Validation

- File size: Không có giới hạn cứng, nhưng nên giới hạn ở client
- File type: Hỗ trợ các định dạng hình ảnh phổ biến (jpg, jpeg, png, gif)
- File name: Tự động tạo tên file unique với format `{Guid}_{OriginalFileName}`

## Error Handling

- Nếu upload file thất bại, quá trình tạo ParentMedicationDelivery vẫn tiếp tục
- File không hợp lệ sẽ được bỏ qua và log lỗi
- Response sẽ bao gồm danh sách URL của các file đã upload thành công

## Security

- Chỉ Admin và SchoolNurse mới có quyền delete file
- File được lưu trong thư mục wwwroot để có thể truy cập qua web
- Tên file được tạo unique để tránh conflict

## Migration

Để áp dụng thay đổi database:

```bash
dotnet ef database update --project ../Repositories
```

## Testing

1. Tạo ParentMedicationDelivery với file upload
2. Kiểm tra file được lưu trong thư mục uploads
3. Test download file qua API
4. Test delete file (với quyền Admin/SchoolNurse) 