using System.IdentityModel.Tokens.Jwt;
using Common.DbModels;
using Common.DtoModels.Others;
using DataAccess.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _dbContext;

    public DoctorRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Register(DoctorEntity doctorEntity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Doctors.AddAsync(doctorEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DoctorEntity?> GetByCredentials(string email, string password)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Email == email.ToLower() && d.Password == password);
        return doctor;
    }

    public async Task<bool> IsEmailUnique(string email)
    {
        var doctor = await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
        return doctor == null;
    }
}