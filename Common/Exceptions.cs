using Common.DtoModels.Others;

namespace Common;

public class IncorrectModelException(string message) : Exception(message);
public class ForbiddenException(string message) : Exception(message);