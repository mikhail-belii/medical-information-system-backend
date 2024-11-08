using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DbModels;
using Common.DtoModels.Comment;
using Common.DtoModels.Consultation;
using Common.DtoModels.Diagnosis;
using Common.DtoModels.Inspection;
using Common.DtoModels.Others;
using Common.DtoModels.Speciality;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class ConsultationService : IConsultationService
{
    private readonly AppDbContext _dbContext;

    public ConsultationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConsultationModel> GetConsultation(Guid id)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Comments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null)
        {
            throw new KeyNotFoundException();
        }

        var consultationModel = new ConsultationModel
        {
            Id = consultation.Id,
            CreateTime = consultation.CreateTime,
            InspectionId = consultation.InspectionId
        };
        var speciality = await _dbContext.Specialities
            .FindAsync(consultation.SpecialityId);
        var specialityModel = new SpecialityModel
        {
            CreateTime = speciality.CreateTime,
            Id = speciality.Id,
            Name = speciality.Name
        };
        consultationModel.Speciality = specialityModel;
        var comments = new List<CommentModel>();
        foreach (var comment in consultation.Comments)
        {
            var doctor = await _dbContext.Doctors
                .FindAsync(comment.AuthorId);
            var commentModel = new CommentModel
            {
                Author = doctor.Name,
                AuthorId = comment.AuthorId,
                Content = comment.Content,
                CreateTime = comment.CreateTime,
                Id = comment.Id,
                ModifiedDate = comment.ModifiedDate,
                ParentId = comment.ParentId
            };
            comments.Add(commentModel);
        }
        consultationModel.Comments = comments;

        return consultationModel;
    }

    public async Task<InspectionPagedListModel> GetInspectionsList(
        bool grouped, 
        List<Guid> icdRoots,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Inspections
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Diagnoses)
            .Include(i => i.Consultations)
            .AsQueryable();
        
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

        query = query.Where(i => _dbContext.Consultations
            .Any(c => c.InspectionId == i.Id));

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

    public async Task<Guid> AddComment(Guid consultationId, CommentCreateModel commentCreateModel, Guid userId)
    {
        var consultation = await _dbContext.Consultations
            .Include(c => c.Comments)
            .FirstOrDefaultAsync(c => c.Id == consultationId);

        if (consultation == null)
        {
            throw new KeyNotFoundException("Consultation");
        }

        if (!await _dbContext.Comments
                .AnyAsync(c => c.Id == commentCreateModel.ParentId))
        {
            throw new KeyNotFoundException("ParentId");
        }

        var user = await _dbContext.Doctors
            .FindAsync(userId);
        var inspection = await _dbContext.Inspections
            .Include(i => i.Doctor)
            .FirstOrDefaultAsync(i => i.Id == consultation.InspectionId);
        if (user.Speciality != consultation.SpecialityId && inspection.Doctor.Id != user.Id)
        {
            throw new ForbiddenException("No permission for adding comment");
        }

        var comment = new CommentEntity
        {
            AuthorId = userId,
            ConsultationId = consultationId,
            Content = commentCreateModel.Content,
            CreateTime = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            ParentId = commentCreateModel.ParentId,
        };
        consultation.Comments.Add(comment);
        await _dbContext.Comments.AddAsync(comment);
        await _dbContext.SaveChangesAsync();

        return comment.Id;
    }

    public async Task EditComment(Guid commentId, InspectionCommentCreateModel model, Guid userId)
    {
        var comment = await _dbContext.Comments
            .FindAsync(commentId);

        if (comment == null)
        {
            throw new KeyNotFoundException();
        }

        if (comment.AuthorId != userId)
        {
            throw new ForbiddenException("User is not the author of the comment");
        }

        comment.Content = model.Content;
        comment.ModifiedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}