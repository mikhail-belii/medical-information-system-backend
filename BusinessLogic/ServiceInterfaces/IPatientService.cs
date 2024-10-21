using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;

namespace BusinessLogic.ServiceInterfaces;

public interface IPatientService
{
    public Task<Guid> CreatePatient(PatientCreateModel patientCreateModel, Guid doctorId);
    public Task<PatientModel?> GetPatientById(Guid id);
    public Task<Guid> CreateInspection(
        InspectionCreateModel inspectionCreateModel,
        Guid doctorId, 
        Guid patientId);
}