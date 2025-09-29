namespace MakeIndex.Models;

[Serializable]
public class Registry
{
    public Version Version { get; set; } = new();
    public List<RegistryEntity> Entities { get; set; } = new(); 
}