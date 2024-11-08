using System.Text.RegularExpressions;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DbModels;
using Common.DtoModels.Diagnosis;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Common.DtoModels.Patient;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;

namespace BusinessLogic.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _dbContext;

    public PatientService(AppDbContext dbContext)
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
        if (patient == null)
        {
            throw new KeyNotFoundException();
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

    public async Task<Guid> CreateInspection(InspectionCreateModel inspectionCreateModel, Guid doctorId, Guid patientId)
    {
        if (await _dbContext.Patients.FindAsync(patientId) == null)
        {
            throw new KeyNotFoundException();
        }

        foreach (var diagnosis in inspectionCreateModel.Diagnoses)
        {
            if (await _dbContext.Icd10s.FindAsync(diagnosis.IcdDiagnosisId) == null)
            {
                throw new IncorrectModelException($"Diagnosis with id {diagnosis.IcdDiagnosisId} does not exist");
            }
        }

        if (inspectionCreateModel.Consultations != null)
        {
            foreach (var consultation in inspectionCreateModel.Consultations)
            {
                if (await _dbContext.Specialities.FindAsync(consultation.SpecialityId) == null)
                {
                    throw new IncorrectModelException($"Speciality with id {consultation.SpecialityId} does not exist");
                }
            }
        }
        
        if (inspectionCreateModel.PreviousInspectionId != null && await _dbContext.Inspections
                .AnyAsync(i => i.PreviousInspectionId == inspectionCreateModel.PreviousInspectionId))
        {
            throw new IncorrectModelException(
                $"Daughter inspection for inspection with id {inspectionCreateModel.PreviousInspectionId} already exists");
        }

        if (inspectionCreateModel.PreviousInspectionId != null)
        {
            var patient = await _dbContext.Patients
                .Include(p => p.Inspections)
                .FirstOrDefaultAsync(p => p.Id == patientId);
            if (patient.Inspections.FirstOrDefault(i => i.Id == inspectionCreateModel.PreviousInspectionId) == null)
            {
                throw new IncorrectModelException(
                    $"Patient with id {patientId} does not have inspection with id {inspectionCreateModel.PreviousInspectionId}");
            }

            var parentInsp = patient.Inspections.FirstOrDefault(i => i.Id == inspectionCreateModel.PreviousInspectionId);
            if (parentInsp.Date > inspectionCreateModel.Date)
            {
                throw new IncorrectModelException("Daughter inspection cannot be earlier than parent inspection");
            }
        }

        if (inspectionCreateModel.NextVisitDate != null && inspectionCreateModel.DeathDate != null)
        {
            throw new IncorrectModelException("Next Visit Date and Death Date can not be not null at the same time");
        }

        if (inspectionCreateModel.Date > DateTime.UtcNow)
        {
            throw new IncorrectModelException("Date can not be in the future");
        }

        if (inspectionCreateModel.NextVisitDate != null &&
            inspectionCreateModel.NextVisitDate < DateTime.UtcNow.AddMinutes(1))
        {
                throw new IncorrectModelException("Next visit date and time must be later than now");
        }
        
        if (inspectionCreateModel.DeathDate != null &&
            inspectionCreateModel.DeathDate > DateTime.UtcNow.AddMinutes(1))
        {
            throw new IncorrectModelException("Death date and time cannot be later than now");
        }

        if (inspectionCreateModel.PreviousInspectionId != null)
        {
            var prev = await _dbContext.Inspections.FindAsync(inspectionCreateModel.PreviousInspectionId);
            if (inspectionCreateModel.Date < prev?.Date)
            {
                throw new IncorrectModelException("Date can not be earlier than previous inspection");
            }
        }

        if (inspectionCreateModel.Diagnoses
                .Where(d => d.Type == DiagnosisType.Main)
                .ToList().Count != 1)
        {
            throw new IncorrectModelException("There are more or less than one diagnosis with type 'Main'");
        }

        if (inspectionCreateModel.Conclusion == Conclusion.Disease && inspectionCreateModel.NextVisitDate == null)
        {
            throw new IncorrectModelException("There is no expected Next Visit Date");
        }
        
        if (inspectionCreateModel.Conclusion == Conclusion.Death && inspectionCreateModel.DeathDate == null)
        {
            throw new IncorrectModelException("There is no expected Death Date");
        }

        if (_dbContext.Patients
                .Include(patientEntity => patientEntity.Inspections)
                .FirstOrDefault(p => p.Id == patientId)
                .Inspections.Any(i => i.Conclusion == Conclusion.Death))
        {
            throw new IncorrectModelException("This patient is already dead");
        }

        if (inspectionCreateModel.Consultations != null)
        {
            var specialityIds = inspectionCreateModel.Consultations
                .Select(c => c.SpecialityId)
                .ToList();
            if (specialityIds.Distinct().Count() != specialityIds.Count)
            {
                throw new IncorrectModelException("Inspection cannot have several consultations with the same specialty of a doctor");
            }
        }
        
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
        Guid doctorId,
        CancellationToken cancellationToken = default)
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
            Count = (int)Math.Ceiling((double) await query.CountAsync(cancellationToken) / size)
        };

        var patients = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

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

    public async Task<InspectionPagedListModel> GetInspectionsList(
        Guid id, 
        bool grouped,
        List<Guid> icdRoots,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Patients.FindAsync(id, cancellationToken) == null)
        {
            throw new KeyNotFoundException();
        }
        
        var query = _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .AsQueryable();

        query = query.Where(i => i.Patient.Id == id);
        if (icdRoots.Count != 0)
        {
            var childrens = new List<Guid>();
            foreach (var rootId in icdRoots)
            {
                var root = await _dbContext.Icd10s
                    .Where(i => i.Id == rootId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (root == null)
                {
                    throw new IncorrectModelException($"Incorrect ICD-10 root with id {rootId}");
                }

                if (root.IcdParentId != null)
                {
                    throw new IncorrectModelException($"ICD-10 with id {rootId} is not ICD-10 root");
                }

                var rootCode = root.Code;
                var icdChildrens = await _dbContext.Icd10s
                    .Where(i => i.IcdRootCode == rootCode)
                    .Select(i => i.Id)
                    .ToListAsync();

                childrens.AddRange(icdChildrens);
            }

            query = query
                .Where(i => i.Diagnoses
                    .Any(d => childrens.Contains(d.Icd10Id)));
        }

        if (grouped)
        {
            query = query
                .Where(i => i.PreviousInspectionId == null);
        }

        var inspections = await query.ToListAsync(cancellationToken);

        var previewModels = new List<InspectionPreviewModel>();
        foreach (var inspection in inspections)
        {
            var model = new InspectionPreviewModel
            {
                Id = inspection.Id,
                Conclusion = inspection.Conclusion,
                CreateTime = DateTime.UtcNow,
                Date = inspection.CreateTime,
                Doctor = inspection.Doctor.Name,
                DoctorId = inspection.Doctor.Id,
                Patient = inspection.Patient.Name,
                PatientId = inspection.Patient.Id,
                PreviousId = inspection.PreviousInspectionId
            };
            var diagnosisEntity = inspection.Diagnoses
                .FirstOrDefault(d => d.Type == DiagnosisType.Main);
            var icd10 = await _dbContext.Icd10s
                .FirstOrDefaultAsync(i => i.Id == diagnosisEntity.Icd10Id, cancellationToken);
            var diagnosisModel = new DiagnosisModel
            {
                Code = icd10.Code,
                CreateTime = diagnosisEntity.CreateTime,
                Description = diagnosisEntity.Description,
                Id = diagnosisEntity.Id,
                Name = diagnosisEntity.Name,
                Type = diagnosisEntity.Type
            };
            model.Diagnosis = diagnosisModel;
            var hasNested = await _dbContext.Inspections
                .AnyAsync(i => i.PreviousInspectionId == model.Id, cancellationToken);
            var hasChain = model.PreviousId == null && hasNested;
            model.HasNested = hasNested;
            model.HasChain = hasChain;
            previewModels.Add(model);
        }
        
        var pagination = new PageInfoModel
        {
            Size = size,
            Current = page,
            Count = (int)Math.Ceiling((double) previewModels.Count / size)
        };

        var inspections2 = previewModels
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();
        
        
        var finalModel = new InspectionPagedListModel
        {
            Pagination = pagination,
            Inspections = inspections2
        };

        return finalModel;
    }

    public async Task<List<InspectionShortModel>> GetInspectionsWithoutChildren(
        Guid id, 
        string request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .AsQueryable();

        if (await _dbContext.Patients.FindAsync(id) == null)
        {
            throw new KeyNotFoundException();
        }
        
        query = query.Where(i => i.Patient.Id == id);

        if (!string.IsNullOrEmpty(request))
        {
            if (Regex.IsMatch(request, RegexPatterns.IcdCode))
            {
                var diagnosisIds = _dbContext.Icd10s
                    .Where(i => i.Code.Contains(request))
                    .Select(i => i.Id)
                    .ToList();

                query = query.Where(i => i.Diagnoses
                    .Any(d => diagnosisIds.Contains(d.Icd10Id)));
            }
            else
            {
                query = query.Where(i => i.Diagnoses
                    .Any(d => d.Name.ToLower().Contains(request.ToLower())));
            }
        }

        await query.ToListAsync(cancellationToken);
        var list = query
            .Where(inspection => !query.Any(i => i.PreviousInspectionId == inspection.Id))
            .ToList();

        var newList = new List<InspectionShortModel>();
        foreach (var inspection in list)
        {
            var shortModel = new InspectionShortModel
            {
                CreateTime = inspection.CreateTime,
                Date = inspection.Date.HasValue ? inspection.Date.Value : DateTime.UtcNow,
                Id = inspection.Id
            };
            var diagnosisEntity = inspection.Diagnoses.FirstOrDefault(d => d.Type == DiagnosisType.Main);
            var diagnosisModel = new DiagnosisModel
            {
                Code = _dbContext.Icd10s
                    .Where(i => i.Id == diagnosisEntity.Icd10Id)
                    .Select(i => i.Code)
                    .FirstOrDefault(),
                CreateTime = diagnosisEntity.CreateTime,
                Description = diagnosisEntity.Description,
                Id = diagnosisEntity.Id,
                Name = diagnosisEntity.Name,
                Type = diagnosisEntity.Type
            };
            shortModel.Diagnosis = diagnosisModel;
            newList.Add(shortModel);
        }

        return newList;
    }
}