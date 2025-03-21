﻿using Common.DbModels;
using Common.DtoModels.Inspection;

namespace BusinessLogic.ServiceInterfaces;

public interface IInspectionService
{
    Task<InspectionModel> GetInspection(Guid id);
    Task EditInspection(Guid id, InspectionEditModel model, Guid doctorId);
    Task<List<InspectionPreviewModel>> GetInspectionChain(Guid id,
        CancellationToken cancellationToken = default);
    Task<List<InspectionEntity>> GetChain(Guid id);
    Task GetChainRecursive(Guid id, List<InspectionEntity> chain);
}