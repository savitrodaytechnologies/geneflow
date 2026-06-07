namespace GeneFlow.Infrastructure.Helpers;

public static class WellIdHelper
{
    private static readonly char[] Rows = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
    private const int MinColumn = 1;
    private const int MaxColumn = 12;

    public static string NormalizeWellId(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        input = input.Trim().ToUpperInvariant();
        if (input.Length < 2) return string.Empty;

        char row = input[0];
        if (!int.TryParse(input[1..], out int col)) return string.Empty;

        return $"{row}{col:D2}";
    }

    public static bool IsValidWellId(string input)
    {
        var normalized = NormalizeWellId(input);
        if (normalized.Length != 3) return false;

        char row = normalized[0];
        if (!int.TryParse(normalized[1..], out int col)) return false;

        return Array.IndexOf(Rows, row) >= 0 && col >= MinColumn && col <= MaxColumn;
    }

    public static List<string> Generate96WellIds()
    {
        var wells = new List<string>(96);
        foreach (char row in Rows)
            for (int col = MinColumn; col <= MaxColumn; col++)
                wells.Add($"{row}{col:D2}");
        return wells;
    }

    public static List<string> GenerateWellRange(string fromWell, string toWell)
    {
        var from = NormalizeWellId(fromWell);
        var to = NormalizeWellId(toWell);

        if (!IsValidWellId(from) || !IsValidWellId(to))
            throw new ArgumentException($"Invalid well range: {fromWell} to {toWell}");

        var allWells = Generate96WellIds();
        int fromIndex = allWells.IndexOf(from);
        int toIndex = allWells.IndexOf(to);

        if (fromIndex < 0 || toIndex < 0)
            throw new ArgumentException($"Well not found in 96-well plate: {fromWell} or {toWell}");

        if (fromIndex > toIndex)
            throw new ArgumentException($"From well {fromWell} must come before to well {toWell}");

        return allWells.GetRange(fromIndex, toIndex - fromIndex + 1);
    }
}
