using Common.DtoModels.Inspection;

namespace BusinessLogic.ServiceInterfaces;

public interface IInspectionService
{
    Task<InspectionModel> GetInspection(Guid id);
}