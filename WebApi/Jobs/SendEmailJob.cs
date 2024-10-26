using DataAccess.RepositoryInterfaces;
using Quartz;

namespace WebApi.Jobs;

public class SendEmailJob : IJob
{
    private readonly IEmailRepository _emailRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SendEmailJob(IEmailRepository emailRepository, IServiceScopeFactory serviceScopeFactory)
    {
        _emailRepository = emailRepository;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
    }
}