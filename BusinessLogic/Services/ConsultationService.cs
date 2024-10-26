using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Comment;
using Common.DtoModels.Consultation;
using Common.DtoModels.Inspection;
using DataAccess.RepositoryInterfaces;

namespace BusinessLogic.Services;

public class ConsultationService : IConsultationService
{
    private readonly IConsultationRepository _consultationRepository;

    public ConsultationService(IConsultationRepository consultationRepository)
    {
        _consultationRepository = consultationRepository;
    }

    public async Task<ConsultationModel> GetConsultation(Guid id)
    {
        return await _consultationRepository.GetConsultation(id);
    }

    public async Task<InspectionPagedListModel> GetInspectionsList(
        bool grouped, 
        List<Guid> icdRoots,
        int page,
        int size)
    {
        return await _consultationRepository.GetInspectionsList(grouped, icdRoots, page, size);
    }

    public async Task<Guid> AddComment(Guid consultationId, CommentCreateModel commentCreateModel, Guid userId)
    {
        return await _consultationRepository.AddComment(
            consultationId,
            commentCreateModel,
            userId);
    }

    public async Task EditComment(Guid commentId, InspectionCommentCreateModel model, Guid userId)
    {
        await _consultationRepository.EditComment(
            commentId,
            model,
            userId);
    }
}