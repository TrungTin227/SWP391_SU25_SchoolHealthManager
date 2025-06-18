using DTOs.Common;

namespace DTOs.VaccineDoseInfoDTOs.Request
{
    public class DeleteVaccineDoseInfosRequest : BatchIdsRequest
    {
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreVaccineDoseInfosRequest : BatchIdsRequest
    {
    }
}