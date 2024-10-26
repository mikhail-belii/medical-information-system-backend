using Common;
using Common.DbModels;
using Common.DtoModels.Diagnosis;
using Common.DtoModels.Doctor;
using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;
using Common.DtoModels.Speciality;
using Common.Enums;
using DataAccess.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class InspectionRepository : IInspectionRepository
{
    private readonly AppDbContext _dbContext;

    public InspectionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InspectionModel> GetInspection(Guid id)
    {
        var inspection = await _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .Include(i => i.Consultations)!
            .ThenInclude(c => c.Comments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inspection == null)
        {
            throw new KeyNotFoundException();
        }

        var inspectionModel = new InspectionModel
        {
            Id = inspection.Id,
            CreateTime = inspection.CreateTime,
            Date = inspection.Date.HasValue ? inspection.Date.Value : DateTime.UtcNow,
            Anamnesis = inspection.Anamnesis,
            Complaints = inspection.Complaints,
            Treatment = inspection.Treatment,
            Conclusion = inspection.Conclusion,
            NextVisitDate = inspection.NextVisitDate,
            DeathDate = inspection.DeathDate,
            BaseInspectionId = inspection.BaseInspectionId,
            PreviousInspectionId = inspection.PreviousInspectionId
        };
        var patientModel = new PatientModel
        {
            Birthday = inspection.Patient.Birthday,
            CreateTime = inspection.Patient.CreateTime,
            Gender = inspection.Patient.Gender,
            Id = inspection.Patient.Id,
            Name = inspection.Patient.Name
        };
        var doctorModel = new DoctorModel
        {
            Birthday = inspection.Doctor.Birthday,
            CreateTime = inspection.Doctor.CreateTime,
            Email = inspection.Doctor.Email,
            Gender = inspection.Doctor.Gender,
            Id = inspection.Doctor.Id,
            Name = inspection.Doctor.Name,
            Phone = inspection.Doctor.Phone,
            Speciality = inspection.Doctor.Speciality
        };
        inspectionModel.Patient = patientModel;
        inspectionModel.Doctor = doctorModel;
        var diagnosisModels = new List<DiagnosisModel>();
        foreach (var diagnosisEntity in inspection.Diagnoses)
        {
            var diagnosisModel = new DiagnosisModel
            {
                Code = (await _dbContext.Icd10s
                    .Where(i => i.Id == diagnosisEntity.Icd10Id)
                    .Select(i => i.Code)
                    .FirstOrDefaultAsync())!,
                CreateTime = diagnosisEntity.CreateTime,
                Description = diagnosisEntity.Description,
                Id = diagnosisEntity.Id,
                Name = diagnosisEntity.Name,
                Type = diagnosisEntity.Type
            };
            diagnosisModels.Add(diagnosisModel);
        }

        var inspectionConsultationModels = new List<InspectionConsultationModel>();
        foreach (var consultation in inspection.Consultations)
        {
            var inspectionConsultationModel = new InspectionConsultationModel
            {
                CommentsNumber = _dbContext.Comments
                    .Count(c => c.ConsultationId == consultation.Id),
                CreateTime = consultation.CreateTime,
                Id = consultation.Id,
                InspectionId = consultation.InspectionId,
                Speciality = new SpecialityModel
                {
                    CreateTime = await _dbContext.Specialities
                        .Where(s => s.Id == consultation.SpecialityId)
                        .Select(s => s.CreateTime)
                        .FirstOrDefaultAsync(),
                    Id = consultation.SpecialityId,
                    Name = (await _dbContext.Specialities
                        .Where(s => s.Id == consultation.SpecialityId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync())!
                }
                // RootComment
            };
            var rootCommentEntity = await _dbContext.Comments
                .Where(c => c.ConsultationId == consultation.Id)
                .Where(c => c.ParentId == null)
                .FirstOrDefaultAsync();
            var rootCommentModel = new InspectionCommentModel
            {
                Content = rootCommentEntity.Content,
                CreateTime = rootCommentEntity.CreateTime,
                Id = rootCommentEntity.Id,
                ModifyTime = rootCommentEntity.ModifiedDate,
                ParentId = rootCommentEntity.ParentId
            };
            var doctorAuthor = await _dbContext.Doctors
                .Where(d => d.Id == rootCommentEntity.AuthorId)
                .FirstOrDefaultAsync();
            var doctorModel2 = new DoctorModel
            {
                Birthday = doctorAuthor.Birthday,
                CreateTime = doctorAuthor.CreateTime,
                Email = doctorAuthor.Email,
                Gender = doctorAuthor.Gender,
                Id = doctorAuthor.Id,
                Name = doctorAuthor.Name,
                Phone = doctorAuthor.Phone,
                Speciality = doctorAuthor.Speciality
            };
            rootCommentModel.Author = doctorModel2;
            inspectionConsultationModel.RootComment = rootCommentModel;
            inspectionConsultationModels.Add(inspectionConsultationModel);
        }

        inspectionModel.Diagnoses = diagnosisModels;
        inspectionModel.Consultations = inspectionConsultationModels;

        return inspectionModel;
    }

    public async Task EditInspection(Guid id, Guid doctorId, InspectionEditModel model)
    {
        var inspection = await _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .Include(i => i.Consultations)!
            .ThenInclude(c => c.Comments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inspection == null)
        {
            throw new KeyNotFoundException();
        }

        if (inspection.Doctor.Id != doctorId)
        {
            throw new ForbiddenException($"The user is not the author of the inspection with id '{id}'");
        }
        
        if (model.NextVisitDate != null && model.DeathDate != null)
        {
            throw new IncorrectModelException("Next Visit Date and Death Date can not be not null at the same time");
        }

        if (model.NextVisitDate != null &&
            model.NextVisitDate < DateTime.UtcNow.AddMinutes(1))
        {
            throw new IncorrectModelException("Next visit date and time must be later than now");
        }
        
        if (model.DeathDate != null &&
            model.DeathDate > DateTime.UtcNow.AddMinutes(1))
        {
            throw new IncorrectModelException("Death date and time cannot be later than now");
        }

        if (model.Diagnoses
                .Where(d => d.Type == DiagnosisType.Main)
                .ToList().Count > 1)
        {
            throw new IncorrectModelException("There are more than one diagnosis with type 'Main'");
        }

        if (model.Conclusion == Conclusion.Disease && model.NextVisitDate == null)
        {
            throw new IncorrectModelException("There is no expected Next Visit Date");
        }
        
        if (model.Conclusion == Conclusion.Death && model.DeathDate == null)
        {
            throw new IncorrectModelException("There is no expected Death Date");
        }

        await _dbContext.Inspections.Where(i => i.Id == id)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(c => c.Anamnesis, model.Anamnesis)
                    .SetProperty(c => c.Complaints, model.Complaints)
                    .SetProperty(c => c.Treatment, model.Treatment)
                    .SetProperty(c => c.Conclusion, model.Conclusion)
                    .SetProperty(c => c.NextVisitDate, model.NextVisitDate)
                    .SetProperty(c => c.DeathDate, model.DeathDate));
        
        _dbContext.Diagnoses.RemoveRange(inspection.Diagnoses);
        
        foreach (var diagnosisCreateModel in model.Diagnoses)
        {
            var diagnosisEntity = new DiagnosisEntity
            {
                CreateTime = DateTime.UtcNow,
                Description = diagnosisCreateModel.Description,
                Icd10Id = diagnosisCreateModel.IcdDiagnosisId,
                Id = Guid.NewGuid(),
                Name = (await _dbContext.Icd10s.Where(i => i.Id == diagnosisCreateModel.IcdDiagnosisId)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync())!,
                Type = diagnosisCreateModel.Type
            };
            await _dbContext.Diagnoses.AddAsync(diagnosisEntity);
            inspection.Diagnoses.Add(diagnosisEntity);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<InspectionPreviewModel>> GetInspectionChain(Guid id)
    {
        var inspection = await _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .Include(i => i.Consultations)!
            .ThenInclude(c => c.Comments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inspection == null)
        {
            throw new KeyNotFoundException();
        }

        if (inspection.PreviousInspectionId != null)
        {
            throw new IncorrectModelException($"Try to get chain for non-root medical inspection with id '{id}'");
        }

        var entityChain = await GetChain(id);
        if (entityChain.Count == 0)
        {
            return new List<InspectionPreviewModel>();
        }

        var inspectionPreviewModelChain = new List<InspectionPreviewModel>();
        foreach (var inspectionEntity in entityChain)
        {
            var inspectionPreviewModel = new InspectionPreviewModel
            {
                Conclusion = inspectionEntity.Conclusion,
                CreateTime = inspectionEntity.CreateTime,
                Date = inspectionEntity.Date.HasValue ? inspectionEntity.Date.Value : DateTime.UtcNow,
                Doctor = inspectionEntity.Doctor.Name,
                DoctorId = inspectionEntity.Doctor.Id,
                Id = inspectionEntity.Id,
                Patient = inspectionEntity.Patient.Name,
                PatientId = inspectionEntity.Patient.Id,
                PreviousId = inspectionEntity.PreviousInspectionId,
                HasChain = false,
                HasNested = _dbContext.Inspections
                    .Any(i => i.PreviousInspectionId == inspectionEntity.Id)
            };
            var diagnosisEntity = inspectionEntity.Diagnoses
                .FirstOrDefault(d => d.Type == DiagnosisType.Main);
            var diagnosisModel = new DiagnosisModel
            {
                Code = await _dbContext.Icd10s
                    .Where(i => i.Id == diagnosisEntity.Icd10Id)
                    .Select(i => i.Code)
                    .FirstOrDefaultAsync(),
                CreateTime = diagnosisEntity.CreateTime,
                Description = diagnosisEntity.Description,
                Id = diagnosisEntity.Id,
                Name = diagnosisEntity.Name,
                Type = diagnosisEntity.Type
            };
            inspectionPreviewModel.Diagnosis = diagnosisModel;
            inspectionPreviewModelChain.Add(inspectionPreviewModel);
        }

        return inspectionPreviewModelChain;
    }

    public async Task<List<InspectionEntity>> GetChain(Guid id)
    {
        var chain = new List<InspectionEntity>();
        await GetChainRecursive(id, chain);
        return chain;
    }

    public async Task GetChainRecursive(Guid id, List<InspectionEntity> chain)
    {
        var child = await _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .Include(i => i.Consultations)!
            .ThenInclude(c => c.Comments)
            .FirstOrDefaultAsync(i => i.PreviousInspectionId == id);
        if (child != null)
        {
            chain.Add(child);
            await GetChainRecursive(child.Id, chain);
        }
    }
}