using Common.DtoModels.Icd10;
using Common.DtoModels.Others;
using Common.DtoModels.Speciality;

namespace BusinessLogic.ServiceInterfaces;

public interface IDictionaryService
{
    public Task<SpecialtiesPagedListModel> GetSpecialities(string name, int page, int size);
    public Task<IEnumerable<Icd10RecordModel>> GetRoots(CancellationToken cancellationToken = default);
    public Task<Icd10SearchModel> SearchForDiagnoses(
        string request, 
        int page, 
        int size,
        CancellationToken cancellationToken = default);
    public Task<ResponseModel> ImportIcd(string jsonPath, CancellationToken cancellationToken = default);
    public Task<ResponseModel> CreateSpeciality(CreateSpecialityModel model);
}