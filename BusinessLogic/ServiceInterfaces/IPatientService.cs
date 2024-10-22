using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;
using Common.Enums;

namespace BusinessLogic.ServiceInterfaces;

public interface IPatientService
{
    public Task<Guid> CreatePatient(PatientCreateModel patientCreateModel, Guid doctorId);
    public Task<PatientModel?> GetPatientById(Guid id);
    public Task<Guid> CreateInspection(
        InspectionCreateModel inspectionCreateModel,
        Guid doctorId, 
        Guid patientId);

    public Task<PatientPagedListModel> GetPatientsList(
        string name,
        List<Conclusion>? conclusions,
        PatientSorting? sorting,
        bool scheduledVisits,
        bool onlyMine,
        int page,
        int size,
        Guid doctorId);
}