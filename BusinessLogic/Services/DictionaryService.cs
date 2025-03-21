using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessLogic.ServiceInterfaces;
using Common;
using Common.DbModels;
using Common.DtoModels.Icd10;
using Common.DtoModels.Others;
using Common.DtoModels.Speciality;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class DictionaryService : IDictionaryService
{
    private readonly AppDbContext _dbContext;

    public DictionaryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpecialtiesPagedListModel> GetSpecialities(string name, int page, int size)
    {
        var query = _dbContext.Specialities.AsQueryable();
        
        query = query.Where(s => s.Name.ToLower().Contains(name.ToLower()));
        
        var pagination = new PageInfoModel
        {
            Size = size,
            Current = page,
            Count = (int)Math.Ceiling((double) await query.CountAsync() / size)
        };
        
        var specialities = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
        
        var specialityModels = new List<SpecialityModel>();
        foreach (var speciality in specialities)
        {
            var specialityModel = new SpecialityModel
            {
                Id = speciality.Id,
                CreateTime = speciality.CreateTime,
                Name = speciality.Name
            };
            specialityModels.Add(specialityModel);
        }

        var model = new SpecialtiesPagedListModel
        {
            Pagination = pagination,
            Specialties = specialityModels
        };

        return model;
    }

    public async Task<IEnumerable<Icd10RecordModel>> GetRoots(CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.Icd10s
            .Where(i => i.IcdParentId == null)
            .OrderBy(i => i.Code)
            .ToListAsync(cancellationToken);
        var list2 = new List<Icd10RecordModel>();
        foreach (var icd in list)
        {
            var icd10RecordModel = new Icd10RecordModel
            {
                Code = icd.Code,
                CreateTime = icd.CreateTime,
                Id = icd.Id,
                Name = icd.Name
            };
            list2.Add(icd10RecordModel);
        }

        return list2;
    }

    public async Task<Icd10SearchModel> SearchForDiagnoses(
        string request,
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Icd10s.AsQueryable();
        
        if (Regex.IsMatch(request, RegexPatterns.IcdCode))
        {
            query = query.Where(i => i.Code.Contains(request));
        }
        else
        {
            query = query.Where(i => i.Name.Contains(request));
        }

        var pagination = new PageInfoModel
        {
            Size = size,
            Current = page,
            Count = (int)Math.Ceiling((double) await query.CountAsync(cancellationToken) / size)
        };
        
        var icd10s = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);
        
        var icd10RecordModels = new List<Icd10RecordModel>();
        foreach (var icd in icd10s)
        {
            var icd10RecordModel = new Icd10RecordModel
            {
                Id = icd.Id,
                CreateTime = icd.CreateTime,
                Code = icd.Code,
                Name = icd.Name
            };
            icd10RecordModels.Add(icd10RecordModel);
        }

        var model = new Icd10SearchModel
        {
            Pagination = pagination,
            Records = icd10RecordModels
        };

        return model;
    }
    
        public async Task<ResponseModel> ImportIcd(string jsonPath, CancellationToken cancellationToken = default)
    {
        var icds = await _dbContext.Icd10s.AnyAsync(cancellationToken);
        if (icds)
        {
            return new ResponseModel
            {
                Status = "400",
                Message = "ICD-10 is already imported"
            };
        }
        
        try
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath, cancellationToken);

            var icd10Data = JsonSerializer.Deserialize<Icd10JsonModel>(jsonContent);

            if (icd10Data?.records == null || !icd10Data.records.Any())
            {
                throw new Exception();
            }

            var icd10Dictionary = icd10Data.records.ToDictionary(r => r.ID.ToString(), r => r);
            

            var icd10Entities = icd10Data.records.Select(r => new Icd10Entity
            {
                Id = Guid.NewGuid(),
                Code = r.MKB_CODE,
                CreateTime = DateTime.UtcNow,
                IcdId = r.ID.ToString(),
                IcdParentId = r.ID_PARENT,
                IcdRootCode = GetRootCode(r.ID.ToString(), icd10Dictionary),
                Name = r.MKB_NAME
            }).ToList();

            await _dbContext.Icd10s.AddRangeAsync(icd10Entities, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return new ResponseModel
            {
                Status = "400",
                Message = "Some errors occured while importing json file"
            };
        }

        return new ResponseModel
        {
            Status = "200",
            Message = "ICD-10 was successfully imported"
        };
    }

    public async Task<ResponseModel> CreateSpeciality(CreateSpecialityModel model)
    {
        var doesExist = await _dbContext.Specialities.AnyAsync(s => s.Name == model.Name);
        if (doesExist)
        {
            return new ResponseModel
            {
                Status = "400",
                Message = "This speciality already exists"
            };
        }

        var specialityEntity = new SpecialityEntity
        {
            CreateTime = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            Name = model.Name
        };
        await _dbContext.Specialities.AddAsync(specialityEntity);
        await _dbContext.SaveChangesAsync();
        
        return new ResponseModel
        {
            Status = "200",
            Message = "Speciality was successfully added"
        };
    }

    private string GetRootCode(string currentId, Dictionary<string, Icd10JsonRecord> icd10Dictionary)
    {
        if (!icd10Dictionary.TryGetValue(currentId, out var currentRecord))
        {
            return null;
        }
        
        return string.IsNullOrEmpty(currentRecord.ID_PARENT) ? currentRecord.MKB_CODE :
            GetRootCode(currentRecord.ID_PARENT, icd10Dictionary);
    }
    
    private class Icd10JsonModel
    {
        public List<Icd10JsonRecord> records { get; set; }
    }

    private class Icd10JsonRecord
    {
        public int ID { get; set; }
        public string REC_CODE { get; set; }
        public string MKB_CODE { get; set; }
        public string MKB_NAME { get; set; }
        public string ID_PARENT { get; set; }
        public int ACTUAL { get; set; }
    }
}