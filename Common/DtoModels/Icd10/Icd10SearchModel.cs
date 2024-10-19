using Common.DtoModels.Others;

namespace Common.DtoModels.Icd10;

public class Icd10SearchModel
{
    public List<Icd10RecordModel>? Records { get; set; }
    public PageInfoModel? Pagination { get; set; }
}