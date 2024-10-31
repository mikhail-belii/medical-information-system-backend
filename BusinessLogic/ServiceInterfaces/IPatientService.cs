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
    Task<Guid?> FindBaseInspectionId(Guid? inspectionId);

    public Task<PatientPagedListModel> GetPatientsList(
        string name,
        List<Conclusion>? conclusions,
        PatientSorting? sorting,
        bool scheduledVisits,
        bool onlyMine,
        int page,
        int size,
        Guid doctorId);

    public Task<InspectionPagedListModel> GetInspectionsList(
        Guid id,
        bool grouped,
        List<Guid> icdRoots,
        int page,
        int size);

    public Task<List<InspectionShortModel>> GetInspectionsWithoutChildren(Guid id, string request);
}