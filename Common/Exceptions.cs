using Common.DtoModels.Others;

namespace Common;

public class IncorrectModelException(string message) : Exception(message);