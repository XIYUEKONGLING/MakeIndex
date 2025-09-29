namespace MakeIndex.Models.Result;

public class SizeAnalysisResult
{
    public List<SizeItem> Items { get; set; } = new();
    public long TotalSize { get; set; }
}