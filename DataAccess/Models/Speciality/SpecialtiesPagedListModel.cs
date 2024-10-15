using DataAccess.Models.Others;

namespace DataAccess.Models.Speciality;

public class SpecialtiesPagedListModel
{
    public List<SpecialityModel>? Specialties { get; set; }
    public PageInfoModel? Pagination { get; set; }
}