using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Spect.Net.Assembler.Assembler;

namespace Spect.Net.LanguageServer.Services;

public class Z80CompilationCache
{
    private readonly ConcurrentDictionary<DocumentUri, (Z80Assembler Assembler, AssemblerOutput Output)> _cache = new();

    public void Update(DocumentUri uri, Z80Assembler assembler, AssemblerOutput output)
    {
        _cache[uri] = (assembler, output);
    }

    public (Z80Assembler Assembler, AssemblerOutput Output)? Get(DocumentUri uri)
    {
        if (_cache.TryGetValue(uri, out var result))
        {
            return result;
        }
        return null;
    }
}
