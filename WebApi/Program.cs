using System.Text;
using System.Text.Json.Serialization;
using BusinessLogic;
using DataAccess;
using DataAccess.Repositories;
using DataAccess.RepositoryInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters()
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = configuration["Jwt:Issuer"],
//             ValidAudience = configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]))
//         };
//     });

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseNpgsql(configuration.GetConnectionString(nameof(AppDbContext)));
    });
builder.Services.AddDataBase();
builder.Services.AddBusinessLogic();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<EnumSchemaFilter>();

    // var securityScheme = new OpenApiSecurityScheme()
    // {
    //     Name = "JWT Authentication",
    //     Description = "Please enter token",
    //     In = ParameterLocation.Header,
    //     Type = SecuritySchemeType.Http,
    //     Scheme = "bearer",
    //     BearerFormat = "JWT",
    //     Reference = new OpenApiReference()
    //     {
    //         Id = JwtBearerDefaults.AuthenticationScheme,
    //         Type = ReferenceType.SecurityScheme
    //     }
    // };
    //
    // c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    // c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    // {
    //     { securityScheme, new List<string>() }
    // });
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();