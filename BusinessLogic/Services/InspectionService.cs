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
}