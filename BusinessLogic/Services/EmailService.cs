using BusinessLogic.ServiceInterfaces;
using Common.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class EmailService : IEmailService
{
    private readonly AppDbContext _dbContext;

    public EmailService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<(PatientEntity, DoctorEntity, InspectionEntity)>?> CheckInspections()
    {
        var inspections = await _dbContext.Inspections
            .Include(i => i.Doctor)
            .Include(i => i.Patient)
            .Where(i => i.NextVisitDate != null && i.NextVisitDate < DateTime.UtcNow)
            .Where(i => ! _dbContext.NotificationLogs.Any(l => l.InspectionId == i.Id))
            .ToListAsync();

        if (inspections.Count == 0)
        {
            return null;
        }

        var list = new List<(PatientEntity, DoctorEntity, InspectionEntity)>();
        foreach (var inspection in inspections)
        {
            list.Add((inspection.Patient, inspection.Doctor, inspection)!);
        }
        return list;
    }

    public async Task AddNotification(Guid id, bool isSent)
    {
        var notification = new NotificationLog
        {
            Id = Guid.NewGuid(),
            InspectionId = id,
            SentDate = DateTime.UtcNow,
            IsNotificationSent = isSent
        };
        await _dbContext.NotificationLogs.AddAsync(notification);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task AddNotification(Guid id, bool isSent, string exceptionMsg)
    {
        var notification = new NotificationLog
        {
            Id = Guid.NewGuid(),
            InspectionId = id,
            SentDate = DateTime.UtcNow,
            IsNotificationSent = isSent,
            ExceptionMessage = exceptionMsg
        };
        await _dbContext.NotificationLogs.AddAsync(notification);
        await _dbContext.SaveChangesAsync();
    }
}