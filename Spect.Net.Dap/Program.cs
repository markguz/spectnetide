using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.DebugAdapter.Server;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.DebugAdapter.Protocol;
using OmniSharp.Extensions.DebugAdapter.Protocol.Requests;
using Spect.Net.Dap.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Spect.Net.Dap;

class Program
{
    static async Task Main(string[] args)
    {
        var session = new SpectNetDebugSession();
        var launchHandler = new LaunchHandler(session);
        var attachHandler = new AttachHandler();
        var setBreakpointsHandler = new SetBreakpointsHandler(session);
        var threadsHandler = new ThreadsHandler();
        var stackTraceHandler = new StackTraceHandler(session);
        var scopesHandler = new ScopesHandler();
        var variablesHandler = new VariablesHandler(session);
        var configurationDoneHandler = new ConfigurationDoneHandler(session);
        var pauseHandler = new PauseHandler(session);
        var continueHandler = new ContinueHandler(session);
        var nextHandler = new NextHandler(session);
        var stepInHandler = new StepInHandler(session);
        var stepOutHandler = new StepOutHandler(session);

        try 
        {
            var server = await OmniSharp.Extensions.DebugAdapter.Server.DebugAdapterServer.From(options =>
            {
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithServices(services =>
                    {
                        services.AddSingleton(session);
                        services.AddSingleton<IJsonRpcHandler>(launchHandler);
                        services.AddSingleton<IJsonRpcHandler>(attachHandler);
                        services.AddSingleton<IJsonRpcHandler>(setBreakpointsHandler);
                        services.AddSingleton<IJsonRpcHandler>(threadsHandler);
                        services.AddSingleton<IJsonRpcHandler>(stackTraceHandler);
                        services.AddSingleton<IJsonRpcHandler>(scopesHandler);
                        services.AddSingleton<IJsonRpcHandler>(variablesHandler);
                        services.AddSingleton<IJsonRpcHandler>(configurationDoneHandler);
                        services.AddSingleton<IJsonRpcHandler>(pauseHandler);
                        services.AddSingleton<IJsonRpcHandler>(continueHandler);
                        services.AddSingleton<IJsonRpcHandler>(nextHandler);
                        services.AddSingleton<IJsonRpcHandler>(stepInHandler);
                        services.AddSingleton<IJsonRpcHandler>(stepOutHandler);
                    })
                    .ConfigureLogging(x => x
                        .SetMinimumLevel(LogLevel.Debug)
                    );
            });

            session.Initialize(server);
            
            await Task.Delay(-1);
        }
        catch (Exception)
        {
             // Log error if needed, but for now just exit.
             throw;
        }
    }
}
