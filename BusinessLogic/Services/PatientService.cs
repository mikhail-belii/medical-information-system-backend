using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Inspection;
using Common.DtoModels.Patient;
using Common.Enums;
using DataAccess.RepositoryInterfaces;

namespace BusinessLogic.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<Guid> CreatePatient(PatientCreateModel patientCreateModel, Guid doctorId)
    {
        return await _patientRepository.CreatePatient(patientCreateModel, doctorId);
    }

    public async Task<PatientModel?> GetPatientById(Guid id)
    {
        return await _patientRepository.GetPatientById(id);
    }

    public async Task<Guid> CreateInspection(InspectionCreateModel inspectionCreateModel, Guid doctorId, Guid patientId)
    {
        return await _patientRepository.CreateInspection(inspectionCreateModel, doctorId, patientId);
    }

    public async Task<PatientPagedListModel> GetPatientsList(
        string name, 
        List<Conclusion>? conclusions, 
        PatientSorting? sorting, 
        bool scheduledVisits,
        bool onlyMine,
        int page,
        int size, 
        Guid doctorId)
    {
        return await _patientRepository.GetPatientsList(
            name, conclusions, sorting, scheduledVisits, onlyMine, page, size, doctorId);
    }
}