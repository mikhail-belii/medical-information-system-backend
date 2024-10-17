﻿using WebApi.DtoModels.Enums;

namespace DataAccess.DbModels;

public class DiagnosisEntity
{
    public Guid Id { get; set; }
    public DateTime CreateTime { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DiagnosisType Type { get; set; }
    public Icd10Entity Icd10 { get; set; }
}