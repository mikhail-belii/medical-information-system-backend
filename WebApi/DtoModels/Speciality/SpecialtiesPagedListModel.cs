using WebApi.DtoModels.Others;

namespace WebApi.DtoModels.Speciality;

public class SpecialtiesPagedListModel
{
    public List<SpecialityModel>? Specialties { get; set; }
    public PageInfoModel? Pagination { get; set; }
}