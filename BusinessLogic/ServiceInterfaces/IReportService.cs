using Common.DtoModels.Icd10;

namespace BusinessLogic.ServiceInterfaces;

public interface IReportService
{
    public Task<IcdRootsReportModel> GetReport(
        DateTime start,
        DateTime end, 
        List<Guid> icdRoots,
        CancellationToken cancellationToken = default);
}