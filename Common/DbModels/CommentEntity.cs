namespace Common.DbModels;

public class CommentEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public DoctorEntity Author { get; set; }
    public Guid? ParentId { get; set; }
    public ConsultationEntity Consultation { get; set; }
}