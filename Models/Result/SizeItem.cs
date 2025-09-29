namespace MakeIndex.Models.Result;

public class SizeItem
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public double Percentage { get; set; }
}