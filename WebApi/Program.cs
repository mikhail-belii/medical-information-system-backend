using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using BusinessLogic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using WebApi;
using WebApi.Jobs;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        var key = configuration
            .GetSection("Jwt:Secret")
            .Get<string>();
        
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
        };
});

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseNpgsql(configuration.GetConnectionString(nameof(AppDbContext)));
    });
builder.Services.AddBusinessLogic();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSwaggerGen(c =>
{
    
    c.SchemaFilter<EnumSchemaFilter>();
    
    var securityScheme = new OpenApiSecurityScheme()
    {
        Name = "JWT Authentication",
        Description = "Please enter token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference()
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    // c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    // {
    //     { securityScheme, new List<string>() }
    // });
    c.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
    
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddQuartz(configure =>
{
    var emailJobKey = new JobKey(nameof(SendEmailJob));
    var jwtJobKey = new JobKey(nameof(RemoveJwtJob));
    
    var intervalForEmailSender = configuration
        .GetSection("Scheduler:EmailSenderIntervalInMinutes")
        .Get<int>();
    var intervalForRemovingJwt = configuration
        .GetSection("Scheduler:JwtRemoverIntervalInMinutes")
        .Get<int>();

    configure.AddJob<SendEmailJob>(emailJobKey)
        .AddTrigger(trigger => trigger.ForJob(emailJobKey).WithSimpleSchedule(
            schedule => schedule.WithIntervalInMinutes(intervalForEmailSender).RepeatForever()));
    configure.AddJob<RemoveJwtJob>(jwtJobKey)
        .AddTrigger(trigger => trigger.ForJob(jwtJobKey).WithSimpleSchedule(
            schedule => schedule.WithIntervalInMinutes(intervalForRemovingJwt).RepeatForever()));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
    
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();