using Common.DbModels;

namespace BusinessLogic.ServiceInterfaces;

public interface IEmailService
{
    Task<List<(PatientEntity, DoctorEntity, InspectionEntity)>?> CheckInspections();
    Task AddNotification(Guid id, bool isSent);
    Task AddNotification(Guid id, bool isSent, string exceptionMsg);
}