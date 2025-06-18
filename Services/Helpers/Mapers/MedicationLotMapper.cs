using DTOs.MedicationLotDTOs.Request;
using DTOs.MedicationLotDTOs.Response;

namespace Services.Mappers
{
    public static class MedicationLotMapper
    {
        /// <summary>
        /// Maps CreateMedicationLotRequest to MedicationLot entity
        /// </summary>
        public static MedicationLot MapFromCreateRequest(CreateMedicationLotRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new MedicationLot
            {
                MedicationId = request.MedicationId,
                LotNumber = request.LotNumber,
                ExpiryDate = request.ExpiryDate,
                Quantity = request.Quantity,
                StorageLocation = request.StorageLocation
            };
        }

        /// <summary>
        /// Maps UpdateMedicationLotRequest to existing MedicationLot entity
        /// </summary>
        public static void MapFromUpdateRequest(UpdateMedicationLotRequest request, MedicationLot lot)
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

        /// <summary>
        /// Maps MedicationLot entity to MedicationLotResponseDTO
        /// </summary>
        public static MedicationLotResponseDTO MapToResponseDTO(MedicationLot lot)
        {
            if (lot == null)
                throw new ArgumentNullException(nameof(lot));

            return new MedicationLotResponseDTO
            {
                Id = lot.Id,
                MedicationId = lot.MedicationId ?? Guid.Empty,
                LotNumber = lot.LotNumber,
                ExpiryDate = lot.ExpiryDate,
                Quantity = lot.Quantity,
                StorageLocation = lot.StorageLocation,
                IsDeleted = lot.IsDeleted,
                CreatedAt = lot.CreatedAt,
                UpdatedAt = lot.UpdatedAt,
                CreatedBy = lot.CreatedBy == Guid.Empty ? string.Empty : lot.CreatedBy.ToString(),
                UpdatedBy = lot.UpdatedBy == Guid.Empty ? string.Empty : lot.UpdatedBy.ToString(),
                MedicationName = lot.Medication?.Name ?? string.Empty,
                MedicationUnit = lot.Medication?.Unit ?? string.Empty
            };
        }

        /// <summary>
        /// Maps a collection of MedicationLot entities to MedicationLotResponseDTO list
        /// </summary>
        public static List<MedicationLotResponseDTO> MapToResponseDTOList(IEnumerable<MedicationLot> lots)
        {
            if (lots == null)
                return new List<MedicationLotResponseDTO>();

            return lots.Select(MapToResponseDTO).ToList();
        }

        /// <summary>
        /// Creates a paged result from source paged list and mapped items
        /// </summary>
        public static PagedList<MedicationLotResponseDTO> CreatePagedResult(
            PagedList<MedicationLot> sourcePaged,
            List<MedicationLotResponseDTO> mappedItems)
        {
            if (sourcePaged == null)
                throw new ArgumentNullException(nameof(sourcePaged));
            if (mappedItems == null)
                throw new ArgumentNullException(nameof(mappedItems));

            return new PagedList<MedicationLotResponseDTO>(
                mappedItems,
                sourcePaged.MetaData.TotalCount,
                sourcePaged.MetaData.CurrentPage,
                sourcePaged.MetaData.PageSize);
        }

        /// <summary>
        /// Creates a paged result directly from source paged list by mapping internally
        /// </summary>
        public static PagedList<MedicationLotResponseDTO> MapToPagedResponseDTO(PagedList<MedicationLot> sourcePaged)
        {
            if (sourcePaged == null)
                throw new ArgumentNullException(nameof(sourcePaged));

            var mappedItems = MapToResponseDTOList(sourcePaged);
            return CreatePagedResult(sourcePaged, mappedItems);
        }
    }
}