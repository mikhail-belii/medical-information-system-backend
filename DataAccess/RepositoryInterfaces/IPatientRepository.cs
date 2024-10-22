using Common.DbModels;
using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;
using Common.Enums;

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

    Task<PatientPagedListModel> GetPatientsList(
        string name,
        List<Conclusion>? conclusions,
        PatientSorting? sorting,
        bool scheduledVisits,
        bool onlyMine,
        int page,
        int size,
        Guid doctorId);

    // Task WriteRootCodes();
    // public string FindRootCode(Icd10Entity entity, List<Icd10Entity> allEntities);
}