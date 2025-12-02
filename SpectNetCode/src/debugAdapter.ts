import * as path from 'path';
import { DebugSession } from 'vscode-debugadapter';
import { DebugProtocol } from 'vscode-debugprotocol';
import { Variable } from 'vscode-debugadapter';

// This file is the entry point for the debug adapter.
// Since we are using a C# backend, this file might not be strictly necessary if we configured
// package.json to launch the C# executable directly.
// However, having a TS wrapper allows us to do some setup if needed, or just spawn the process.

// Actually, for "runtime": "dotnet", VS Code expects the "program" to be the dll or exe?
// If we use "program": "./dist/debugAdapter.js", VS Code will launch this JS file using node.
// So we need to spawn the dotnet process here and pipe stdin/stdout.

// But wait, if we use the "executable" pattern in package.json (which I didn't use), VS Code handles it.
// In my package.json, I used:
// "program": "./dist/debugAdapter.js",
// "runtime": "dotnet" -> This is wrong if program is js.
// "runtime" is usually "node" for js adapters.

// If I want to launch the C# adapter directly, I should probably use a different configuration or
// have this JS file spawn the dotnet process.

// Let's implement a simple adapter that spawns the dotnet process and pipes I/O.
// Or better, let's use the `DebugSession` from `vscode-debugadapter` to handle the session
// and forward requests? No, that's too complex.

// The standard way for a "pipe" transport is that the client connects to the server.
// Or the server is launched by VS Code and communicates via stdin/stdout.

// If I want VS Code to launch my C# adapter directly:
// I should remove "program" and "runtime" from "debuggers" and use "adapterExecutableCommand" in extension.ts?
// Or use "program" pointing to the C# executable?
// But "program" is usually for the adapter executable if "runtime" is not specified?

// Let's stick to the plan: `debugAdapter.ts` will spawn the C# process.
// This is a common pattern.

import { spawn } from 'child_process';

const serverPath = path.join(__dirname, '..', '..', 'Spect.Net.Dap', 'bin', 'Debug', 'net6.0', 'Spect.Net.Dap.dll');
const dotnet = 'dotnet';

// We simply spawn the dotnet process and let it handle stdin/stdout.
// But wait, if this process is launched by VS Code, VS Code communicates with THIS process via stdin/stdout.
// So we need to pipe THIS process's stdin to dotnet's stdin, and dotnet's stdout to THIS process's stdout.

const serverProcess = spawn(dotnet, [serverPath]);

process.stdin.pipe(serverProcess.stdin);
serverProcess.stdout.pipe(process.stdout);
serverProcess.stderr.pipe(process.stderr);

serverProcess.on('exit', (code) => {
    process.exit(code);
});

serverProcess.on('error', (err) => {
    console.error(`Failed to start server: ${err}`);
    process.exit(1);
});
