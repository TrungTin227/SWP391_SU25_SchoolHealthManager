using DTOs.VaccineDoseInfoDTOs.Response;
using DTOs.MedicationLotDTOs.Response;

namespace DTOs.VaccineDTOs.Response
{
    public class VaccineTypeDetailResponseDTO : VaccineTypeResponseDTO
    {
        public List<VaccineDoseInfoResponseDTO> DoseInfos { get; set; } = new();
        public List<MedicationLotResponseDTO> MedicationLots { get; set; } = new();
    }
}