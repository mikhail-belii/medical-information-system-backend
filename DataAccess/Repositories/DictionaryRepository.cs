using System.Text.RegularExpressions;
using Common;
using Common.DtoModels.Icd10;
using Common.DtoModels.Others;
using Common.DtoModels.Speciality;
using DataAccess.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class DictionaryRepository : IDictionaryRepository
{
    private readonly AppDbContext _dbContext;

    public DictionaryRepository(AppDbContext dbContext)
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

    public async Task<IEnumerable<Icd10RecordModel>> GetRoots()
    {
        var list = await _dbContext.Icd10s
            .Where(i => i.IcdParentId == null)
            .OrderBy(i => i.Code)
            .ToListAsync();
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

    public async Task<Icd10SearchModel> SearchForDiagnoses(string request, int page, int size)
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
            Count = (int)Math.Ceiling((double) await query.CountAsync() / size)
        };
        
        var icd10s = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
        
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
}