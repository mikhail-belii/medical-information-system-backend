using Common.DtoModels.Inspection;

namespace DataAccess.RepositoryInterfaces;

public interface IInspectionRepository
{
    Task<InspectionModel> GetInspection(Guid id);
}