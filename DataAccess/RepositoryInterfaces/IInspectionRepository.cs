using Common.DbModels;
using Common.DtoModels.Inspection;

namespace DataAccess.RepositoryInterfaces;

public interface IInspectionRepository
{
    Task<InspectionModel> GetInspection(Guid id);
    Task EditInspection(Guid id, Guid doctorId, InspectionEditModel model);
    Task<List<InspectionPreviewModel>> GetInspectionChain(Guid id);
    Task<List<InspectionEntity>> GetChain(Guid id);
    Task GetChainRecursive(Guid id, List<InspectionEntity> chain);
}