using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Icd10;
using DataAccess.RepositoryInterfaces;

namespace BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IcdRootsReportModel> GetReport(DateTime start, DateTime end, List<Guid> icdRoots)
    {
        return await _reportRepository.GetReport(start, end, icdRoots);
    }
}