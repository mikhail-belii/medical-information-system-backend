using Common.DbModels;

namespace DataAccess.RepositoryInterfaces;

public interface IEmailRepository
{
    Task<List<(PatientEntity, DoctorEntity, InspectionEntity)>?> CheckInspections();
    Task AddNotification(Guid id);
}