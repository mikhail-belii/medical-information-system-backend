using Common.DtoModels.Inspection;

namespace BusinessLogic.ServiceInterfaces;

public interface IInspectionService
{
    Task<InspectionModel> GetInspection(Guid id);
    Task EditInspection(Guid id, InspectionEditModel model, Guid doctorId);
    Task<List<InspectionPreviewModel>> GetInspectionChain(Guid id);
}