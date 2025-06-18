namespace Services.Helpers.Mappers
{
    public static class VaccineLotMapper
    {
        public static MedicationLot MapFromCreateRequest(CreateVaccineLotRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new MedicationLot
            {
                Id = Guid.NewGuid(),
                VaccineTypeId = request.VaccineTypeId,
                Type = LotType.Vaccine,
                LotNumber = request.LotNumber,
                ExpiryDate = request.ExpiryDate,
                Quantity = request.Quantity,
                StorageLocation = request.StorageLocation
            };
        }

        public static void MapFromUpdateRequest(UpdateVaccineLotRequest request, MedicationLot lot)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (lot == null)
                throw new ArgumentNullException(nameof(lot));

            lot.LotNumber = request.LotNumber;
            lot.ExpiryDate = request.ExpiryDate;
            lot.Quantity = request.Quantity;
            lot.StorageLocation = request.StorageLocation;
        }

        public static VaccineLotResponseDTO MapToResponseDTO(MedicationLot lot)
        {
            if (lot == null)
                throw new ArgumentNullException(nameof(lot));

            return new VaccineLotResponseDTO
            {
                Id = lot.Id,
                VaccineTypeId = lot.VaccineTypeId ?? Guid.Empty,
                VaccineTypeName = lot.VaccineType?.Name ?? string.Empty,
                LotNumber = lot.LotNumber,
                ExpiryDate = lot.ExpiryDate,
                Quantity = lot.Quantity,
                StorageLocation = lot.StorageLocation,
                IsDeleted = lot.IsDeleted,
                CreatedAt = lot.CreatedAt,
                UpdatedAt = lot.UpdatedAt,
                CreatedBy = lot.CreatedBy.ToString(),
                UpdatedBy = lot.UpdatedBy.ToString()
            };
        }

        public static List<VaccineLotResponseDTO> MapToResponseDTOList(IEnumerable<MedicationLot> lots)
        {
            return lots?.Select(MapToResponseDTO).ToList() ?? new List<VaccineLotResponseDTO>();
        }

        public static PagedList<VaccineLotResponseDTO> MapToPagedResponseDTO(PagedList<MedicationLot> pagedLots)
        {
            if (pagedLots == null)
                throw new ArgumentNullException(nameof(pagedLots));

            var mappedItems = MapToResponseDTOList(pagedLots);

            // Sử dụng constructor với các tham số riêng lẻ từ MetaData
            return new PagedList<VaccineLotResponseDTO>(
                mappedItems,
                pagedLots.MetaData.TotalCount,
                pagedLots.MetaData.CurrentPage,
                pagedLots.MetaData.PageSize);
        }
    }
}