using WebApi.DtoModels.Others;

namespace WebApi.DtoModels.Inspection;

public class InspectionPagedListModel
{
    public List<InspectionPreviewModel>? Inspections { get; set; }
    public PageInfoModel Pagination { get; set; }
}