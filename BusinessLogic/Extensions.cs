using BusinessLogic.ServiceInterfaces;
using BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic;

public static class Extensions
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDoctorService, DoctorService>();
        serviceCollection.AddScoped<IPatientService, PatientService>();
        serviceCollection.AddScoped<IDictionaryService, DictionaryService>();
        serviceCollection.AddScoped<IInspectionService, InspectionService>();
        serviceCollection.AddScoped<IConsultationService, ConsultationService>();
        serviceCollection.AddScoped<IReportService, ReportService>();
        serviceCollection.AddScoped<IEmailService, EmailService>();
        serviceCollection.AddSingleton<ITokenService, TokenService>();
        return serviceCollection;
    }
}