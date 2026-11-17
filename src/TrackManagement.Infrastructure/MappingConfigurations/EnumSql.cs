namespace TrackManagement.Infrastructure.MappingConfigurations;

internal static class EnumSql
{
    /// <summary>Builds "[Column] IN ('A', 'B', ...)" from an enum's names, for CHECK constraints.</summary>
    public static string InList<TEnum>(string column) where TEnum : struct, Enum
    {
        var values = string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"));
        return $"[{column}] IN ({values})";
    }
}
