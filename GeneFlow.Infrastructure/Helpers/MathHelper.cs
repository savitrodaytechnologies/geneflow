namespace GeneFlow.Infrastructure.Helpers;

public static class MathHelper
{
    public static decimal? Mean(IEnumerable<decimal> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return null;
        return list.Average();
    }

    public static decimal? SampleStandardDeviation(IEnumerable<decimal> values)
    {
        var list = values.ToList();
        if (list.Count < 2) return null;

        decimal mean = list.Average();
        decimal sumSquaredDiffs = list.Sum(v => (v - mean) * (v - mean));
        return (decimal)Math.Sqrt((double)(sumSquaredDiffs / (list.Count - 1)));
    }

    public static decimal FoldChange(decimal deltaDeltaCt)
    {
        return (decimal)Math.Pow(2, (double)-deltaDeltaCt);
    }

    public static decimal Log2FoldChange(decimal deltaDeltaCt)
    {
        return -deltaDeltaCt;
    }
}
