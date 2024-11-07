using BusinessLogic.ServiceInterfaces;
using MailKit.Net.Smtp;
using Quartz;

namespace WebApi.Jobs;

public class SendEmailJob : IJob
{
    private readonly IEmailService _emailService;
    private readonly IEmailSender _emailSender;

    public SendEmailJob(IEmailSender emailSender, IEmailService emailService)
    {
        _emailSender = emailSender;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var res = await _emailService.CheckInspections();
        if (res != null)
        {
            foreach (var (patient, doctor, inspection) in res)
            {
                var name = doctor.Name;
                var email = doctor.Email;
                const string subject = "Пропущено запланированное посещение";
                var message = $"Здравствуйте, {name}! Это сообщение автоматически сгенерировано системой.\n" +
                              $"На него отвечать не требуется.\n\n" +
                              $"{inspection.Date} вы запланировали повторный визит пациента {patient.Name} на {inspection.NextVisitDate}.";
                try
                {
                    await _emailSender.SendEmailAsync(name, email, subject, message);
                    await _emailService.AddNotification(inspection.Id, true);
                }
                catch (SmtpCommandException ex)
                {
                    await _emailService.AddNotification(inspection.Id, false, ex.Message);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }
}