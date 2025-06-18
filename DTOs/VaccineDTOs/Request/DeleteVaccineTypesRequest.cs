namespace DTOs.VaccineDTOs.Request
{
    public class DeleteVaccineTypesRequest : BatchIdsRequest
    {
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreVaccineTypesRequest : BatchIdsRequest
    {
    }

    public abstract class BatchIdsRequest
    {
        public List<Guid> Ids { get; set; } = new();
    }
}