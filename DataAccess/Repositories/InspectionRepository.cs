using Common.DtoModels.Diagnosis;
using Common.DtoModels.Doctor;
using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;
using Common.DtoModels.Speciality;
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
            .Include(i => i.Consultations)
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
}