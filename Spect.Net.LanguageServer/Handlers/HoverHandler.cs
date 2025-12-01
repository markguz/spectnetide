using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Spect.Net.LanguageServer.Services;
using Spect.Net.Assembler.Assembler;

namespace Spect.Net.LanguageServer.Handlers;

public class HoverHandler : IHoverHandler
{
    private readonly Z80CompilationCache _compilationCache;

    public HoverHandler(Z80CompilationCache compilationCache)
    {
        _compilationCache = compilationCache;
    }

    public Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        var cacheEntry = _compilationCache.Get(request.TextDocument.Uri);
        if (cacheEntry == null)
        {
            return Task.FromResult<Hover>(null);
        }

        var (assembler, output) = cacheEntry.Value;
        
        // Convert URI to file path to find the correct FileIndex
        var filePath = request.TextDocument.Uri.GetFileSystemPath();
        
        // Find FileIndex
        var fileIndex = -1;
        for (var i = 0; i < output.SourceFileList.Count; i++)
        {
            // Simple string comparison, might need normalization
            if (string.Equals(output.SourceFileList[i].Filename, filePath, StringComparison.OrdinalIgnoreCase))
            {
                fileIndex = i;
                break;
            }
        }
        
        // If file not found (e.g. "Untitled-1" vs "NO_FILE_ITEM"), try to fallback if there is only one file
        if (fileIndex == -1 && output.SourceFileList.Count == 1 && output.SourceFileList[0].Filename == Z80Assembler.NO_FILE_ITEM)
        {
            fileIndex = 0;
        }

        if (fileIndex == -1)
        {
            return Task.FromResult<Hover>(null);
        }

        // Find the line
        var lineNum = request.Position.Line + 1; // 1-based
        var line = assembler.PreprocessedLines.FirstOrDefault(l => l.FileIndex == fileIndex && l.SourceLine == lineNum);
        
        if (line == null)
        {
             return Task.FromResult<Hover>(null);
        }
        
        var text = line.SourceText;
        var word = GetWordAtPosition(text, request.Position.Character);
        
        if (string.IsNullOrEmpty(word))
        {
             return Task.FromResult<Hover>(null);
        }
        
        // Look up the symbol
        // We check local scopes first?
        // Z80Assembler.GetSymbolValue handles scope resolution but it needs context.
        // But Output.Symbols is the global symbol table.
        // Output.Symbols contains all symbols?
        // AssemblyModule.Symbols contains symbols for that module.
        // Z80Assembler.DoCompile sets CurrentModule = Output.
        // So Output.Symbols are the global symbols.
        // What about local symbols?
        // We don't easily know which scope we are in without traversing the AST or scopes.
        // But for a basic implementation, global symbols are a good start.
        
        if (output.Symbols.TryGetValue(word, out var symbolInfo))
        {
             return Task.FromResult(new Hover
             {
                 Contents = new MarkedStringsOrMarkupContent(
                     new MarkupContent
                     {
                         Kind = MarkupKind.Markdown,
                         Value = $"**{symbolInfo.Name}**: {symbolInfo.Value.Value} (0x{symbolInfo.Value.Value:X4})"
                     }
                 )
             });
        }
        
        return Task.FromResult<Hover>(null);
    }
    
    private string GetWordAtPosition(string text, int column)
    {
        if (string.IsNullOrEmpty(text) || column < 0 || column >= text.Length) return null;
        
        // Search backwards
        var start = column;
        while (start > 0 && IsIdentifierChar(text[start - 1])) start--;
        
        // Search forwards
        var end = column;
        while (end < text.Length && IsIdentifierChar(text[end])) end++;
        
        if (start == end) return null;
        return text.Substring(start, end - start);
    }
    
    private bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '`' || c == '@';
    }

    public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm")
        };
    }
}
