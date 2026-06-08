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

    /// <summary>
    /// Column-major well range: iterates columns first within the rectangular
    /// region defined by fromWell (top-left) and toWell (bottom-right).
    /// e.g. A01→H04 → A01,B01,C01…H01, A02,B02…H02, A03…H03, A04…H04
    /// </summary>
    public static List<string> GenerateWellRangeByColumn(string fromWell, string toWell)
    {
        var from = NormalizeWellId(fromWell);
        var to   = NormalizeWellId(toWell);

        if (!IsValidWellId(from) || !IsValidWellId(to))
            throw new ArgumentException($"Invalid well range: {fromWell} to {toWell}");

        int fromRowIdx = Array.IndexOf(Rows, from[0]);
        int toRowIdx   = Array.IndexOf(Rows, to[0]);
        int fromCol    = int.Parse(from[1..]);
        int toCol      = int.Parse(to[1..]);

        if (fromRowIdx < 0 || toRowIdx < 0 || fromCol < MinColumn || toCol > MaxColumn)
            throw new ArgumentException($"Well out of range: {fromWell} or {toWell}");

        if (fromRowIdx > toRowIdx || fromCol > toCol)
            throw new ArgumentException($"From well {fromWell} must be above-left of to well {toWell}");

        var wells = new List<string>();
        for (int col = fromCol; col <= toCol; col++)
            for (int rowIdx = fromRowIdx; rowIdx <= toRowIdx; rowIdx++)
                wells.Add($"{Rows[rowIdx]}{col:D2}");

        return wells;
    }
}
