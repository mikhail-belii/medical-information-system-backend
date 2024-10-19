namespace Common;

public static class RegexPatterns
{
    public const string Phone = @"^\+7\d{10}$";
    public const string Email = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
}