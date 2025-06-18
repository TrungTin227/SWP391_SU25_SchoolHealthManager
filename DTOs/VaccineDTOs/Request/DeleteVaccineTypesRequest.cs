using DTOs.Common;

namespace DTOs.VaccineDTOs.Request
{
    public class DeleteVaccineTypesRequest : BatchIdsRequest
    {
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreVaccineTypesRequest : BatchIdsRequest
    {
    }

}