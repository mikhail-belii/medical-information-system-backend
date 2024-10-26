using Common.DtoModels.Comment;
using Common.DtoModels.Consultation;
using Common.DtoModels.Inspection;

namespace DataAccess.RepositoryInterfaces;

public interface IConsultationRepository
{
    Task<ConsultationModel> GetConsultation(Guid id);
    Task<InspectionPagedListModel> GetInspectionsList(
        bool grouped,
        List<Guid> icdRoots,
        int page,
        int size);

    Task<Guid> AddComment(Guid consultationId, CommentCreateModel commentCreateModel, Guid userId);
    Task EditComment(Guid commentId, InspectionCommentCreateModel model, Guid userId);
}