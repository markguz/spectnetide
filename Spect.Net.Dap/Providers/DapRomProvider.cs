using Spect.Net.RomResources;
using Spect.Net.SpectrumEmu.Abstraction.Providers;
using System.IO;
using System.Reflection;

namespace Spect.Net.Dap.Providers;

public class DapRomProvider : VmComponentProviderBase, IRomProvider
{
    private const string RESOURCE_NAMESPACE = "Spect.Net.RomResources.Roms";



    public string GetRomResourceName(string romName, int page = -1)
    {
        if (page == -1)
        {
            return $"{RESOURCE_NAMESPACE}.{romName}.{romName}.rom";
        }
        return $"{RESOURCE_NAMESPACE}.{romName}.{romName}-{page}.rom";
    }

    public string GetAnnotationResourceName(string romName, int page = -1)
    {
        if (page == -1)
        {
            return $"{RESOURCE_NAMESPACE}.{romName}.{romName}.disann";
        }
        return $"{RESOURCE_NAMESPACE}.{romName}.{romName}-{page}.disann";
    }

    public byte[] LoadRomBytes(string romName, int page = -1)
    {
        var resourceName = GetRomResourceName(romName, page);
        var assembly = typeof(RomResourcesPlaceHolder).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public string LoadRomAnnotations(string romName, int page = -1)
    {
        var resourceName = GetAnnotationResourceName(romName, page);
        var assembly = typeof(RomResourcesPlaceHolder).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
