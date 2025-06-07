using DTOs.MedicationLotDTOs.Response;

namespace DTOs.MedicationDTOs.Response
{
    public class MedicationDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public string DosageForm { get; set; } = "";
        public string Category { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalLots { get; set; }
        public int TotalQuantity { get; set; }

        // Danh sách chi tiết các lô thuốc
        public List<MedicationLotDetailResponse> Lots { get; set; } = new List<MedicationLotDetailResponse>();
    }
}