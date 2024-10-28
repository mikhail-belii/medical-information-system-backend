namespace Common.DbModels;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid InspectionId { get; set; }
    public DateTime SentDate { get; set; }
}