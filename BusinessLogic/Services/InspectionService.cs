using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Inspection;
using DataAccess.RepositoryInterfaces;

namespace BusinessLogic.Services;

public class InspectionService : IInspectionService
{
    private readonly IInspectionRepository _inspectionRepository;

    public InspectionService(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<InspectionModel> GetInspection(Guid id)
    {
        return await _inspectionRepository.GetInspection(id);
    }

    public async Task EditInspection(Guid id, InspectionEditModel model, Guid doctorId)
    {
        await _inspectionRepository.EditInspection(id, doctorId, model);
    }

    public async Task<List<InspectionPreviewModel>> GetInspectionChain(Guid id)
    {
        return await _inspectionRepository.GetInspectionChain(id);
    }
}