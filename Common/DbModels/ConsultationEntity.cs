namespace Common.DbModels;

public class ConsultationEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public Guid InspectionId { get; set; }
    public Guid SpecialityId { get; set; }
    public List<CommentEntity>? Comments { get; set; }
}