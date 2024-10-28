using DataAccess.RepositoryInterfaces;
using Quartz;

namespace WebApi.Jobs;

[DisallowConcurrentExecution]
public class SendEmailJob : IJob
{
    private readonly IEmailRepository _emailRepository;
    private readonly IEmailSender _emailSender;

    public SendEmailJob(IEmailRepository emailRepository, IEmailSender emailSender)
    {
        _emailRepository = emailRepository;
        _emailSender = emailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var res = await _emailRepository.CheckInspections();
        if (res != null)
        {
            foreach (var (patient, doctor, inspection) in res)
            {
                var name = doctor.Name;
                var email = doctor.Email;
                const string subject = "Пропущено запланированное посещение";
                var message = $"Здравствуйте! Это сообщение автоматически сгенерировано системой.\n" +
                              $"На него отвечать не требуется.\n\n" +
                              $"{inspection.Date} вы запланировали повторный визит пациента {patient.Name} на {inspection.NextVisitDate}.";
                await _emailSender.SendEmailAsync(name, email, subject, message);
                await _emailRepository.AddNotification(inspection.Id);
            }
        }
    }
}