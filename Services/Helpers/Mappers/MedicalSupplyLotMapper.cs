using DTOs.MedicalSupplyLotDTOs.Request;
using DTOs.MedicalSupplyLotDTOs.Response;

namespace Services.Helpers.Mapers
{
    public static class MedicalSupplyLotMapper
    {
        public static MedicalSupplyLot MapFromCreateRequest(CreateMedicalSupplyLotRequest request)
        {
            return new MedicalSupplyLot
            {
                MedicalSupplyId = request.MedicalSupplyId,
                LotNumber = request.LotNumber,
                ExpirationDate = request.ExpirationDate,
                ManufactureDate = request.ManufactureDate,
                Quantity = request.Quantity
            };
        }

        public static void MapFromUpdateRequest(UpdateMedicalSupplyLotRequest request, MedicalSupplyLot lot)
        {
            lot.LotNumber = request.LotNumber;
            lot.ExpirationDate = request.ExpirationDate;
            lot.ManufactureDate = request.ManufactureDate;
            lot.Quantity = request.Quantity;
        }

        public static MedicalSupplyLotResponseDTO ToResponseDTO(MedicalSupplyLot lot)
        {
            return new MedicalSupplyLotResponseDTO
            {
                Id = lot.Id,
                MedicalSupplyId = lot.MedicalSupplyId,
                MedicalSupplyName = lot.MedicalSupply?.Name ?? string.Empty,
                LotNumber = lot.LotNumber,
                ExpirationDate = lot.ExpirationDate,
                ManufactureDate = lot.ManufactureDate,
                Quantity = lot.Quantity,
                IsExpired = lot.ExpirationDate.Date <= DateTime.UtcNow.Date,
                DaysUntilExpiry = (lot.ExpirationDate.Date - DateTime.UtcNow.Date).Days,
                CreatedAt = lot.CreatedAt,
                UpdatedAt = lot.UpdatedAt,
                CreatedBy = lot.CreatedBy.ToString(),
                UpdatedBy = lot.UpdatedBy.ToString()
            };
        }

        public static PagedList<MedicalSupplyLotResponseDTO> ToPagedResult(
            PagedList<MedicalSupplyLot> source,
            IEnumerable<MedicalSupplyLotResponseDTO> items)
        {
            var meta = source.MetaData;
            return new PagedList<MedicalSupplyLotResponseDTO>(
                items.ToList(),
                meta.TotalCount,
                meta.CurrentPage,
                meta.PageSize);
        }
    }
}
