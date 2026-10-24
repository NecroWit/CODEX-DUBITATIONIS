using System.Security.Cryptography;
using System.Text.Json;

namespace Aletheon.ExperimentKit;

public static class ExperimentArtifacts
{
    public static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    public static void Save(string path, object data) => File.WriteAllText(path,
        JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
}
