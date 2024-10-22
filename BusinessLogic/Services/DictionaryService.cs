using BusinessLogic.ServiceInterfaces;
using Common.DtoModels.Icd10;
using Common.DtoModels.Speciality;
using DataAccess.RepositoryInterfaces;

namespace BusinessLogic.Services;

public class DictionaryService : IDictionaryService
{
    private readonly IDictionaryRepository _dictionaryRepository;

    public DictionaryService(IDictionaryRepository dictionaryRepository)
    {
        _dictionaryRepository = dictionaryRepository;
    }

    public async Task<SpecialtiesPagedListModel> GetSpecialities(string name, int page, int size)
    {
        return await _dictionaryRepository.GetSpecialities(name, page, size);
    }

    public async Task<IEnumerable<Icd10RecordModel>> GetRoots()
    {
        return await _dictionaryRepository.GetRoots();
    }

    public async Task<Icd10SearchModel> SearchForDiagnoses(string request, int page, int size)
    {
        return await _dictionaryRepository.SearchForDiagnoses(request, page, size);
    }
}