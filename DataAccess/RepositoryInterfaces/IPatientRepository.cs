using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;

namespace DataAccess.RepositoryInterfaces;

public interface IPatientRepository
{
    Task<Guid> CreatePatient(PatientCreateModel patientCreateModel, Guid doctorId);
    Task<PatientModel?> GetPatientById(Guid id);
    Task<Guid> CreateInspection(
        InspectionCreateModel inspectionCreateModel, 
        Guid doctorId, 
        Guid patientId);
    Task<Guid?> FindBaseInspectionId(Guid? inspectionId);
}