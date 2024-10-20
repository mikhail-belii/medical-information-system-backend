using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Common.DbModels;
using Common.DtoModels.Doctor;
using Common.DtoModels.Others;
using DataAccess.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

    public async Task<bool> IsSpecialityExisting(Guid specialityId)
    {
        var speciality = await _dbContext.Specialities
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == specialityId);
        return speciality != null;
    }

    public async Task<DoctorEntity?> GetDoctorById(Guid id)
    {
        return await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DoctorEntity?> GetProfile(Guid id)
    {
        return await _dbContext.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task EditProfile(Guid id, DoctorEditModel doctorEditModel)
    {
        await _dbContext.Doctors
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(x => x
                .SetProperty(e => e.Email, doctorEditModel.Email)
                .SetProperty(e => e.Name, doctorEditModel.Name)
                .SetProperty(e => e.Birthday, doctorEditModel.BirthDay)
                .SetProperty(e => e.Gender, doctorEditModel.Gender)
                .SetProperty(e => e.Phone, doctorEditModel.Phone));
        await _dbContext.SaveChangesAsync();
    }

    // public void Read()
    // {
    //     string path = @"C:\Users\а\RiderProjects\HITS_2grade_2module\DataAccess\ImportData\ICD10.json";
    //     var res = JsonConvert.DeserializeObject<Root>(File.ReadAllText(path));
    //     foreach (var record in res.Records)
    //     {
    //         var icd = new Icd10Entity
    //         {
    //             Id = new Guid(),
    //             IcdId = record.ID,
    //             IcdParentId = record.ID_PARENT,
    //             Code = record.MKB_CODE,
    //             Name = record.MKB_NAME
    //         };
    //         _dbContext.Icd10s.Add(icd);
    //     }
    //
    //     _dbContext.SaveChanges();
    // }
}

// public class Record
// {
//     public string ID { get; set; }
//     public string REC_CODE { get; set; }
//     public string MKB_CODE { get; set; }
//     public string MKB_NAME { get; set; }
//     public string ID_PARENT { get; set; }
//     public int ACTUAL { get; set; }
// }
//
// public class Root
// {
//     public List<Record> Records { get; set; }
// }