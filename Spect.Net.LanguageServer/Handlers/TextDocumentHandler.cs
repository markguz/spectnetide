using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Microsoft.Extensions.Logging;
using Spect.Net.Assembler.Assembler;
using Spect.Net.LanguageServer.Services;

namespace Spect.Net.LanguageServer.Handlers;

public class TextDocumentHandler : ITextDocumentSyncHandler
{
    private readonly ILogger<TextDocumentHandler> _logger;
    private readonly ILanguageServerFacade _languageServer;
    private readonly Z80CompilationCache _compilationCache;

    public TextDocumentHandler(ILogger<TextDocumentHandler> logger, ILanguageServerFacade languageServer, Z80CompilationCache compilationCache)
    {
        _logger = logger;
        _languageServer = languageServer;
        _compilationCache = compilationCache;
    }

    public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("DidOpenTextDocumentParams: {Uri}", request.TextDocument.Uri);
        PublishDiagnostics(request.TextDocument.Uri, request.TextDocument.Text);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("DidChangeTextDocumentParams: {Uri}", request.TextDocument.Uri);
        // In Full sync mode, ContentChanges has one item with the full text
        var text = request.ContentChanges.FirstOrDefault()?.Text;
        if (text != null)
        {
            PublishDiagnostics(request.TextDocument.Uri, text);
        }
        return Unit.Task;
    }

    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("DidCloseTextDocumentParams: {Uri}", request.TextDocument.Uri);
        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("DidSaveTextDocumentParams: {Uri}", request.TextDocument.Uri);
        if (request.Text != null) 
        {
             PublishDiagnostics(request.TextDocument.Uri, request.Text);
        }
        return Unit.Task;
    }

    private void PublishDiagnostics(DocumentUri uri, string text)
    {
        var assembler = new Z80Assembler();
        var output = assembler.Compile(text);
        
        _compilationCache.Update(uri, assembler, output);

        var diagnostics = new List<Diagnostic>();

        foreach (var error in output.Errors)
        {
            diagnostics.Add(new Diagnostic
            {
                Code = error.ErrorCode,
                Severity = DiagnosticSeverity.Error,
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                    new Position(error.Line - 1, error.Column),
                    new Position(error.Line - 1, error.Column + 1) // Simple range for now
                ),
                Message = error.Message,
                Source = "z80asm"
            });
        }

        _languageServer.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams
        {
            Uri = uri,
            Diagnostics = diagnostics
        });
    }

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, "z80asm");
    }

    public TextDocumentChangeRegistrationOptions GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentChangeRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm"),
            SyncKind = Change
        };
    }

    TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentOpenRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm")
        };
    }

    TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentCloseRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm")
        };
    }

    TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSaveRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("z80asm"),
            IncludeText = true
        };
    }
}
