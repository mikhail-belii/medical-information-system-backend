using Common.DtoModels.Comment;
using Common.DtoModels.Consultation;
using Common.DtoModels.Inspection;

namespace BusinessLogic.ServiceInterfaces;

public interface IConsultationService
{
    public Task<ConsultationModel> GetConsultation(Guid id);

    public Task<InspectionPagedListModel> GetInspectionsList(
        bool grouped,
        List<Guid> icdRoots,
        int page,
        int size);

    public Task<Guid> AddComment(
        Guid consultationId,
        CommentCreateModel commentCreateModel,
        Guid userId);

    public Task EditComment(
        Guid commentId,
        InspectionCommentCreateModel model,
        Guid userId);
}