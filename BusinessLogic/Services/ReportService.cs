using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Icd10;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IcdRootsReportModel> GetReport(DateTime start, DateTime end, List<Guid> icdRoots)
    {
        var roots = new List<string>();
        
        if (icdRoots.Count == 0)
        {
            roots = (await _dbContext.Icd10s
                .Where(i => i.IcdParentId == null)
                .Select(i => i.Code)
                .ToListAsync())!;
        }
        else
        {
            foreach (var id in icdRoots)
            {
                var root = await _dbContext.Icd10s
                    .Where(i => i.Id == id)
                    .Select(i => i.Code)
                    .FirstOrDefaultAsync();
                roots.Add(root);
            }
        }

        if (roots.Any(e => e == null))
        {
            throw new KeyNotFoundException();
        }

        var model = new IcdRootsReportModel
        {
            Filters = new IcdRootsReportFiltersModel
            {
                Start = start,
                End = end,
                IcdRoots = roots
            }
        };
        var summary = new Dictionary<string, int>();
        foreach (var root in roots)
        {
            summary[root] = 0;
        }

        model.SummaryByRoot = summary;

        var inspections = await _dbContext.Inspections
            .Include(i => i.Diagnoses)
            .Include(i => i.Patient)
            .Where(i => i.Date >= start)
            .Where(i => i.Date <= end)
            .ToListAsync();

        var patientRecords = new Dictionary<Guid, IcdRootsReportRecordModel>();
        
        foreach (var inspection in inspections)
        {
            var patientId = inspection.Patient.Id;
            if (!patientRecords.ContainsKey(patientId))
            {
                patientRecords[patientId] = new IcdRootsReportRecordModel
                {
                    Gender = inspection.Patient.Gender,
                    PatientBirthdate = inspection.Patient.Birthday.Value,
                    PatientName = inspection.Patient.Name,
                    VisitsByRoot = new Dictionary<string, int>()
                };
            }

            var mainDiagnosis = inspection.Diagnoses
                .FirstOrDefault(d => d.Type == DiagnosisType.Main);
            var icd = await _dbContext.Icd10s
                .FirstOrDefaultAsync(i => i.Id == mainDiagnosis.Icd10Id);
            var rootCode = icd.IcdRootCode;
            if (roots.Contains(rootCode))
            {
                if (!patientRecords[patientId].VisitsByRoot.ContainsKey(rootCode))
                {
                    patientRecords[patientId].VisitsByRoot[rootCode] = 0;
                }
                patientRecords[patientId].VisitsByRoot[rootCode]++;

                model.SummaryByRoot[rootCode]++;
            }
        }

        model.Records = patientRecords.Values.ToList();

        return model;
    }
}