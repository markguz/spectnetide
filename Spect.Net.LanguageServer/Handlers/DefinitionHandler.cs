using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol;
using Spect.Net.LanguageServer.Services;
using Spect.Net.Assembler.Assembler;

namespace Spect.Net.LanguageServer.Handlers;

public class DefinitionHandler : IDefinitionHandler
{
    private readonly Z80CompilationCache _compilationCache;

    public DefinitionHandler(Z80CompilationCache compilationCache)
    {
        _compilationCache = compilationCache;
    }

    public Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        var cacheEntry = _compilationCache.Get(request.TextDocument.Uri);
        if (cacheEntry == null)
        {
            return Task.FromResult<LocationOrLocationLinks>(new LocationOrLocationLinks());
        }

        var (assembler, output) = cacheEntry.Value;

        // Convert URI to file path to find the correct FileIndex
        var filePath = request.TextDocument.Uri.GetFileSystemPath();
        var fileIndex = -1;
        for (var i = 0; i < output.SourceFileList.Count; i++)
        {
            if (string.Equals(output.SourceFileList[i].Filename, filePath, StringComparison.OrdinalIgnoreCase))
            {
                fileIndex = i;
                break;
            }
        }
        
        if (fileIndex == -1 && output.SourceFileList.Count == 1 && output.SourceFileList[0].Filename == Z80Assembler.NO_FILE_ITEM)
        {
            fileIndex = 0;
        }

        if (fileIndex == -1)
        {
            return Task.FromResult<LocationOrLocationLinks>(new LocationOrLocationLinks());
        }

        // Find the line
        var lineNum = request.Position.Line + 1;
        var line = assembler.PreprocessedLines.FirstOrDefault(l => l.FileIndex == fileIndex && l.SourceLine == lineNum);
        
        if (line == null)
        {
             return Task.FromResult<LocationOrLocationLinks>(new LocationOrLocationLinks());
        }
        
        var text = line.SourceText;
        var word = GetWordAtPosition(text, request.Position.Character);
        
        if (string.IsNullOrEmpty(word))
        {
             return Task.FromResult<LocationOrLocationLinks>(new LocationOrLocationLinks());
        }

        // Look up the symbol
        if (output.Symbols.TryGetValue(word, out var symbolInfo))
        {
            // We have the value (Address).
            // Try to find the definition line.
            // 1. Check if we can find it in PreprocessedLines (e.g. "Label:")
            // This is better than SourceMap for labels that don't emit code.
            
            var defLine = assembler.PreprocessedLines.FirstOrDefault(l => l.Label == word);
            if (defLine != null)
            {
                var defFileIndex = defLine.FileIndex;
                var defSourceLine = defLine.SourceLine;
                
                // Map defFileIndex back to URI
                if (defFileIndex >= 0 && defFileIndex < output.SourceFileList.Count)
                {
                    var defFilename = output.SourceFileList[defFileIndex].Filename;
                    DocumentUri defUri;
                    if (defFilename == Z80Assembler.NO_FILE_ITEM)
                    {
                        // If it's the same file as request, use request URI
                        // But we should be careful if we have multiple files and one is NO_FILE_ITEM (unlikely in VS Code context?)
                        // In VS Code, we usually have real paths.
                        // If we are editing an unsaved file, it might be NO_FILE_ITEM?
                        // But TextDocumentSync usually sends URI.
                        // If we compiled with NO_FILE_ITEM, then we assume it's the request URI.
                        defUri = request.TextDocument.Uri;
                    }
                    else
                    {
                        defUri = DocumentUri.File(defFilename);
                    }
                    
                    return Task.FromResult(new LocationOrLocationLinks(new Location
                    {
                        Uri = defUri,
                        Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                            new Position(defSourceLine - 1, 0),
                            new Position(defSourceLine - 1, 0)
                        )
                    }));
                }
            }
            
            // Fallback to SourceMap if not found (e.g. EQU?)
            // But EQU also has a Label in SourceLineBase.
            // So PreprocessedLines search should cover it.
            
            // If still not found, maybe it's a variable?
        }
        
        return Task.FromResult<LocationOrLocationLinks>(new LocationOrLocationLinks());
    }

    private string GetWordAtPosition(string text, int column)
    {
        if (string.IsNullOrEmpty(text) || column < 0 || column >= text.Length) return null;
        
        var start = column;
        while (start > 0 && IsIdentifierChar(text[start - 1])) start--;
        
        var end = column;
        while (end < text.Length && IsIdentifierChar(text[end])) end++;
        
        if (start == end) return null;
        return text.Substring(start, end - start);
    }
    
    private bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '`' || c == '@';
    }

    public DefinitionRegistrationOptions GetRegistrationOptions(DefinitionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm")
        };
    }
}
