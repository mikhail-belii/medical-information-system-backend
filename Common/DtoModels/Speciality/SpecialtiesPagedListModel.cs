using Common.DtoModels.Others;

namespace Common.DtoModels.Speciality;

public class SpecialtiesPagedListModel
{
    public List<SpecialityModel>? Specialties { get; set; }
    public PageInfoModel? Pagination { get; set; }
}