using Common.DtoModels.Icd10;
using Common.DtoModels.Speciality;

namespace BusinessLogic.ServiceInterfaces;

public interface IDictionaryService
{
    public Task<SpecialtiesPagedListModel> GetSpecialities(string name, int page, int size);
    public Task<IEnumerable<Icd10RecordModel>> GetRoots();
    public Task<Icd10SearchModel> SearchForDiagnoses(string request, int page, int size);
}