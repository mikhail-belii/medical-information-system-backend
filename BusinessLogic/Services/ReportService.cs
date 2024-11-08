using BusinessLogic.ServiceInterfaces;
using Common;
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

    public async Task<IcdRootsReportModel> GetReport(
        DateTime start,
        DateTime end,
        List<Guid> icdRoots,
        CancellationToken cancellationToken = default)
    {
        var roots = new List<string>();
        
        if (icdRoots.Count == 0)
        {
            roots = (await _dbContext.Icd10s
                .Where(i => i.IcdParentId == null)
                .Select(i => i.Code)
                .ToListAsync(cancellationToken))!;
        }
        else
        {
            foreach (var id in icdRoots)
            {
                var root = await _dbContext.Icd10s
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (root != null && root.IcdParentId != null)
                {
                    throw new IncorrectModelException($"ICD-10 with id {id} is not the root");
                }

                if (root == null)
                {
                    throw new KeyNotFoundException($"There is no ICD-10 with id {id}");
                }
                roots.Add(root.Code);
            }
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
            .ToListAsync(cancellationToken);

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
                .FirstOrDefaultAsync(i => i.Id == mainDiagnosis.Icd10Id, cancellationToken);
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

        var sortedPatients = patientRecords.Values
            .OrderBy(p => p.PatientName)
            .ToList();
        
        model.Records = sortedPatients;

        return model;
    }
}