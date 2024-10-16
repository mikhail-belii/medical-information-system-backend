using WebApi.DtoModels.Others;

namespace WebApi.DtoModels.Patient;

public class PatientPagedListModel
{
    public List<PatientModel>? Patients { get; set; }
    public PageInfoModel Pagination { get; set; }
}