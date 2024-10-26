using Common.DtoModels.Icd10;

namespace DataAccess.RepositoryInterfaces;

public interface IReportRepository
{
    Task<IcdRootsReportModel> GetReport(DateTime start, DateTime end, List<Guid> icdRoots);
}