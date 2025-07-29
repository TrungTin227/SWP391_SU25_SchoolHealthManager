namespace Services.Helpers.Mappers
{
    public static class MedicalSupplyMapper
    {
        public static MedicalSupply MapFromCreateRequest(CreateMedicalSupplyRequest request)
        {
            return new MedicalSupply
            {
                Name = request.Name,
                Unit = request.Unit,
                MinimumStock = request.MinimumStock,
                IsActive = request.IsActive
            };
        }

        public static void MapFromUpdateRequest(UpdateMedicalSupplyRequest request, MedicalSupply supply)
        {
            supply.Name = request.Name;
            supply.Unit = request.Unit;
            supply.MinimumStock = request.MinimumStock;
            supply.IsActive = request.IsActive;
        }

        public static MedicalSupplyResponseDTO MapToResponseDTO(MedicalSupply supply)
        {
            return new MedicalSupplyResponseDTO
            {
                Id = supply.Id,
                Name = supply.Name,
                Unit = supply.Unit,
                CurrentStock = supply.CurrentStock,
                MinimumStock = supply.MinimumStock,
                IsActive = supply.IsActive,
                IsDeleted = supply.IsDeleted,
                CreatedAt = supply.CreatedAt,
                UpdatedAt = supply.UpdatedAt,
                CreatedBy = supply.CreatedBy == Guid.Empty ? string.Empty : supply.CreatedBy.ToString(),
                UpdatedBy = supply.UpdatedBy == Guid.Empty ? string.Empty : supply.UpdatedBy.ToString()
            };
        }

        public static MedicalSupplyDetailResponseDTO MapToDetailResponseDTO(MedicalSupply supply)
        {
            return new MedicalSupplyDetailResponseDTO
            {
                Id = supply.Id,
                Name = supply.Name,
                Unit = supply.Unit,
                CurrentStock = supply.CurrentStock,
                MinimumStock = supply.MinimumStock,
                IsActive = supply.IsActive,
                IsDeleted = supply.IsDeleted,
                CreatedAt = supply.CreatedAt,
                UpdatedAt = supply.UpdatedAt,
                CreatedBy = supply.CreatedBy == Guid.Empty ? string.Empty : supply.CreatedBy.ToString(),
                UpdatedBy = supply.UpdatedBy == Guid.Empty ? string.Empty : supply.UpdatedBy.ToString(),
                TotalLots = supply.Lots?.Count ?? 0,
                Lots = supply.Lots?.Select(lot => new MedicalSupplyLotDetailResponseDTO
                {
                    Id = lot.Id,
                    LotNumber = lot.LotNumber,
                    ExpirationDate = lot.ExpirationDate,
                    ManufactureDate = lot.ManufactureDate,
                    Quantity = lot.Quantity,
                    CreatedAt = lot.CreatedAt,
                    UpdatedAt = lot.UpdatedAt
                })
                .OrderBy(l => l.ExpirationDate)
                .ToList() ?? new List<MedicalSupplyLotDetailResponseDTO>()
            };
        }

        public static PagedList<MedicalSupplyResponseDTO> CreatePagedResult(
            PagedList<MedicalSupply> sourcePaged,
            List<MedicalSupplyResponseDTO> mappedItems)
        {
            return new PagedList<MedicalSupplyResponseDTO>(
                mappedItems,
                sourcePaged.MetaData.TotalCount,
                sourcePaged.MetaData.CurrentPage,
                sourcePaged.MetaData.PageSize);
        }
    }
}
