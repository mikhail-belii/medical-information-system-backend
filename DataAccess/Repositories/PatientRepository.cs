using Common.DbModels;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Common.DtoModels.Patient;
using Common.Enums;
using DataAccess.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _dbContext;

    public PatientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreatePatient(PatientCreateModel patientCreateModel, Guid doctorId)
    {
        var patient = new PatientEntity
        {
            Id = new Guid(),
            DoctorId = doctorId,
            CreateTime = DateTime.UtcNow,
            Name = patientCreateModel.Name,
            Birthday = patientCreateModel.Birthday,
            Gender = patientCreateModel.Gender
        };
        await _dbContext.Patients.AddAsync(patient);
        await _dbContext.SaveChangesAsync();

        return patient.Id;
    }

    public async Task<PatientModel?> GetPatientById(Guid id)
    {
        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null)
        {
            return null;
        }

        var patientModel = new PatientModel
        {
            Birthday = patient.Birthday,
            CreateTime = patient.CreateTime,
            Gender = patient.Gender,
            Id = patient.Id,
            Name = patient.Name
        };

        return patientModel;
    }

    public async Task<Guid> CreateInspection(
        InspectionCreateModel inspectionCreateModel,
        Guid doctorId,
        Guid patientId)
    {
        var baseInspectionId = await FindBaseInspectionId(inspectionCreateModel.PreviousInspectionId);
        
        var inspectionEntity = new InspectionEntity
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Date = inspectionCreateModel.Date,
            Anamnesis = inspectionCreateModel.Anamnesis,
            Complaints = inspectionCreateModel.Complaints,
            Treatment = inspectionCreateModel.Treatment,
            Conclusion = inspectionCreateModel.Conclusion,
            NextVisitDate = inspectionCreateModel.NextVisitDate,
            DeathDate = inspectionCreateModel.DeathDate,
            PreviousInspectionId = inspectionCreateModel.PreviousInspectionId,
            BaseInspectionId = baseInspectionId,
            Patient = await _dbContext.Patients.FindAsync(patientId),
            Doctor = await _dbContext.Doctors.FindAsync(doctorId),
            Diagnoses = new List<DiagnosisEntity>(),
            Consultations = new List<ConsultationEntity>()
        };
        foreach (var diagnosisModel in inspectionCreateModel.Diagnoses)
        {
            var icd = await _dbContext.Icd10s.FindAsync(diagnosisModel.IcdDiagnosisId);
            var diagnosisEntity = new DiagnosisEntity
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.UtcNow,
                Name = icd.Name,
                Description = diagnosisModel.Description,
                Type = diagnosisModel.Type,
                Icd10Id = diagnosisModel.IcdDiagnosisId
            };
            inspectionEntity.Diagnoses.Add(diagnosisEntity);
        }

        if (inspectionCreateModel.Consultations != null)
            foreach (var consultationModel in inspectionCreateModel.Consultations)
            {
                var consultationEntity = new ConsultationEntity
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    InspectionId = inspectionEntity.Id,
                    SpecialityId = consultationModel.SpecialityId,
                    Comments = new List<CommentEntity>()
                };
                var commentEntity = new CommentEntity
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    Content = consultationModel.Comment.Content,
                    AuthorId = doctorId,
                    ConsultationId = consultationEntity.Id
                };
                consultationEntity.Comments.Add(commentEntity);
                inspectionEntity.Consultations.Add(consultationEntity);
                await _dbContext.Consultations.AddAsync(consultationEntity);
            }

        await _dbContext.Inspections.AddAsync(inspectionEntity);
        await _dbContext.SaveChangesAsync();
        return inspectionEntity.Id;
    }

    public async Task<Guid?> FindBaseInspectionId(Guid? inspectionId)
    {
        if (inspectionId is null || inspectionId == Guid.Empty)
        {
            return null;
        }
        
        var inspection = await _dbContext.Inspections.FindAsync(inspectionId);

        if (inspection is null)
        {
            return null;
        }

        if (inspection.BaseInspectionId is null || inspection.BaseInspectionId == Guid.Empty)
        {
            return inspection.Id;
        }

        return await FindBaseInspectionId(inspection.BaseInspectionId);
    }

    public async Task<PatientPagedListModel> GetPatientsList(
        string name,
        List<Conclusion>? conclusions,
        PatientSorting? sorting,
        bool scheduledVisits, 
        bool onlyMine,
        int page,
        int size, 
        Guid doctorId)
    {
        var query = _dbContext.Patients.AsQueryable();
        
        query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        
        if (onlyMine)
        {
            query = query.Where(p => p.DoctorId == doctorId);
        }

        if (scheduledVisits)
        {
            query = query.Where(p =>
                _dbContext.Inspections
                    .Where(i => i.Patient.Id == p.Id)
                    .Any(i => i.NextVisitDate > DateTime.UtcNow));
        }

        if (conclusions != null && conclusions.Any())
        {
            var inspectionQuery = _dbContext.Inspections
                .Where(i => conclusions.Contains((Conclusion)i.Conclusion))
                .Select(i => i.Patient.Id)
                .Distinct();

            query = query.Where(p => inspectionQuery.Contains(p.Id));
        }

        if (sorting != null)
        {
            switch (sorting)
            {
                case PatientSorting.CreateAsc:
                    query = query.OrderBy(p => p.CreateTime);
                    break;
                case PatientSorting.CreateDesc:
                    query = query.OrderByDescending(p => p.CreateTime);
                    break;
                case PatientSorting.NameAsc:
                    query = query.OrderBy(p => p.Name);
                    break;
                case PatientSorting.NameDesc:
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case PatientSorting.InspectionAsc:
                    query = query.OrderBy(p => p.Inspections.Max(i => i.Date));
                    break;
                case PatientSorting.InspectionDesc:
                    query = query.OrderByDescending(p => p.Inspections.Max(i => i.Date));
                    break;
            }
        }

        var pagination = new PageInfoModel
        {
            Size = size,
            Current = page,
            Count = (int)Math.Ceiling((double) await query.CountAsync() / size)
        };

        var patients = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        var patientModels = new List<PatientModel>();
        foreach (var patient in patients)
        {
            var patientModel = new PatientModel
            {
                Id = patient.Id,
                Birthday = patient.Birthday,
                CreateTime = patient.CreateTime,
                Gender = patient.Gender,
                Name = patient.Name
            };
            patientModels.Add(patientModel);
        }
        
        var model = new PatientPagedListModel
        {
            Pagination = pagination,
            Patients = patientModels
        };

        return model;
    }

    // public async Task WriteRootCodes()
    // {
    //     var icd10Entities = await _dbContext.Icd10s.ToListAsync();
    //
    //     foreach (var entity in icd10Entities)
    //     {
    //         entity.IcdRootCode = FindRootCode(entity, icd10Entities);
    //     }
    //
    //     await _dbContext.SaveChangesAsync();
    // }
    //
    // public string FindRootCode(Icd10Entity entity, List<Icd10Entity> allEntities)
    // {
    //     if (entity.IcdParentId == null)
    //     {
    //         return entity.Code;
    //     }
    //
    //     var parentEntity = allEntities.FirstOrDefault(e => e.IcdId == entity.IcdParentId);
    //     if (parentEntity == null)
    //     {
    //         return entity.Code;
    //     }
    //
    //     return FindRootCode(parentEntity, allEntities);
    // }
}