using Common.DtoModels.Icd10;
using Common.DtoModels.Speciality;

namespace DataAccess.RepositoryInterfaces;

public interface IDictionaryRepository
{
    Task<SpecialtiesPagedListModel> GetSpecialities(string name, int page, int size);
    Task<IEnumerable<Icd10RecordModel>> GetRoots();
    Task<Icd10SearchModel> SearchForDiagnoses(string request, int page, int size);
}