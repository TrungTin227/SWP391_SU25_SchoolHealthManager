namespace DTOs.VaccineDoseInfoDTOs.Response
{
    public class VaccineDoseInfoDetailResponseDTO : VaccineDoseInfoResponseDTO
    {
        public List<VaccineDoseInfoResponseDTO> NextDoses { get; set; } = new();
        public VaccineDoseInfoResponseDTO? PreviousDose { get; set; }
    }
}