using Common.DtoModels.Others;

namespace Common.DtoModels.Patient;

public class PatientPagedListModel
{
    public List<PatientModel>? Patients { get; set; }
    public PageInfoModel Pagination { get; set; }
}