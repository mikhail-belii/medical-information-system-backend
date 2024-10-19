using Common.DtoModels.Others;

namespace Common.DtoModels.Inspection;

public class InspectionPagedListModel
{
    public List<InspectionPreviewModel>? Inspections { get; set; }
    public PageInfoModel Pagination { get; set; }
}