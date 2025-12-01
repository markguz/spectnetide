# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SpectNetIDE** is a Visual Studio 2019+ extension providing an integrated development environment for ZX Spectrum emulation and development. The project supports Z80 assembly programming, ZX BASIC, code discovery/disassembly, debugging with the integrated emulator, and various development tools.

- **Status**: Active maintenance (V2.0 Preview 11)
- **Primary Focus**: ZX Spectrum 48K, 128K, +3E models; ZX Spectrum Next in development
- **Main Tech Stack**: C# .NET Framework and .NET Core, Visual Studio Extension SDK, WPF

## Repository Structure

The repository contains **two main versions**:

### Root Directory (`/`)
Legacy V1 implementation with archival code:
- `Assembler/` - Z80 assembler components
- `Core/` - Emulator and WPF UI
- `VsIntegration/` - Visual Studio integration
- `Tests/` - Test projects for legacy version
- `vsix-archive/` - Historical release builds
- `docs/` - Documentation (Jekyll-based website)

### V2 Directory (`/v2/`)
**Current active development** - .NET Core-based modernization:
- `Assembler/` - Modern Z80 and ZX BASIC parser/assembler projects
- `Core/` - Spect.Net.SpectrumEmu, Spect.Net.Wpf (current emulator)
- `VsIntegration/` - Current Visual Studio extension
- `Tests/` - Test projects for V2

**Build with V2 solution** (`/v2/Spect.Net.sln`). Root `Spect.Net.sln` is legacy.

## Key Components (V2)

### Assembler (`v2/Assembler/`)
- **Spect.Net.Assembler** - Z80 assembly language compiler with advanced features:
  - Macros, loops, conditional statements
  - String escape sequences for ZX Spectrum special characters
  - Export to `.TAP` and `.TZX` formats
- **Spect.Net.BasicParser** - ZX BASIC language parser (experimental)
- **Spect.Net.CommandParser** - Command line parser
- **Spect.Net.EvalParser** - Expression evaluation parser
- **Spect.Net.TestParser** - Z80 unit test language parser
- ANTLR-based generators for parser regeneration (AntlrZ80AsmParserGenerator, etc.)

### Core (`v2/Core/`)
- **Spect.Net.SpectrumEmu** - Main emulator:
  - CPU emulation (Z80), ULA, memory management
  - Device management (keyboard, tape, floppy disk for +3e)
  - Screen rendering with shadow screen support
  - Audio/beeper implementation
  - VM state save/load functionality
  - Debugger with breakpoints, watch expressions, stack introspection
- **Spect.Net.Wpf** - WPF UI framework for tool windows:
  - Disassembly viewer with ROM annotations
  - Registers and ULA counter display
  - Tape Explorer for `.TZX` and `.TAP` files
  - BASIC List viewer
  - Memory watch tool
  - Virtual floppy disk management

### VS Integration (`v2/VsIntegration/`)
- **Spect.Net.VsPackage** - Main VS extension:
  - Integration with VS menu system and project system
  - Package initialization and configuration
- **Spect.Net.ProjectWizard** - Project creation dialog
- **Spect.Net.CodeDiscover.ProjectTemplate** - Project type template
- **Templates** - Item templates:
  - `Spect.Net.Z80AsmTemplate` - Z80 assembly file template
  - `Spect.Net.ZxBasicTemplate` - ZX BASIC file template
  - `Spect.Net.Z80TestTemplate` - Z80 test file template
  - `Spect.Net.DisassAnnTemplate` - Disassembly annotation file template
- **Spect.Net.ProjectResources** - Shared project resources

## Build & Development

### Solution Files
- **Use `/v2/Spect.Net.sln`** for active development
- Do not use root `Spect.Net.sln` (legacy)

### SDK Requirements
- **.NET Core/Framework**: SDK version 3.0.101+ (see `/v2/global.json`)
- **Visual Studio**: 2019 or later (for debugging extension)
- **ANTLR 4**: Required for parser regeneration (AntlrZ80AsmParserGenerator, etc.)

### Build Commands
```bash
# Open the v2 solution
cd v2
dotnet build

# Build with Release configuration
dotnet build -c Release

# Run tests (from root or test directories)
dotnet test

# Clean build
dotnet clean
dotnet build

# Rebuild parser generators (if grammar files changed)
cd Assembler/AntlrZ80AsmParserGenerator
dotnet run  # Regenerates Spect.Net.Assembler parser classes
```

### Test Projects
Located in `v2/Tests/`:
- `Spect.Net.Assembler.Test` - Assembler functionality
- `Spect.Net.CommandParser.Test` - Command parser
- `Spect.Net.EvalParser.Test` - Expression parser
- `Spect.Net.SpectrumEmu.Test` - Emulator CPU, memory, device tests
- `Spect.Net.TestParser.Test` - Z80 unit test language
- `Spect.Net.ZxBasicParser.Test` - ZX BASIC parser
- `Spect.Net.VsPackage.Test` - VS integration tests (limited)

Run all tests:
```bash
cd v2
dotnet test
```

Run single test project:
```bash
dotnet test Tests/Spect.Net.Assembler.Test
```

## Architecture Patterns

### Namespace Organization
- **Spect.Net.Assembler.Assembler** - Z80 assembler logic
- **Spect.Net.SpectrumEmu.Cpu** - Z80 CPU emulation (registers, opcodes)
- **Spect.Net.SpectrumEmu.Devices** - Emulated hardware (ULA, tape, etc.)
- **Spect.Net.SpectrumEmu.Disasm** - Disassembler with ROM annotations
- **Spect.Net.VsPackage.** - Visual Studio extension classes

### Parser Architecture
Parsers use ANTLR 4 with visitor pattern:
- Grammar files define syntax (`.g4` files)
- ANTLR generators create `*Parser.cs` and `*Visitor.cs` classes
- Custom visitor implementations extend base visitor
- Symbol tables and semantic analysis performed in visitors

Example: Z80 assembler parsing
1. `Z80AsmLexer.g4` tokenizes input
2. `Z80AsmParser.g4` builds parse tree
3. `Z80AsmParseTreeVisitor` traverses tree, building `AssemblyProgram` objects
4. `Assembler` class compiles `AssemblyProgram` to code segments

### Emulator Architecture
CPU-centric design:
- **Z80Cpu** class: Executes instructions, manages registers, flags
- **IMemoryDevice** interface: Pluggable memory implementations
- **IPortDevice** interface: Hardware ports (ULA, etc.)
- **SpectrumVirtualMachine** class: Orchestrates CPU, memory, devices
- **UlaDevice** class: Screen/timer/interrupts

Device state machines emit events during execution (frame rendering, tape activity).

### VS Extension Architecture
- **SpectNetPackage** class: Main entry point, MEF composition
- **IServiceProvider**: Dependency injection for VS services
- **Commands** and **Tool Windows**: Registered via `.vsixmanifest`
- **Project system** integration via build rules (`.xaml` files in `BuildSystem/`)

## Important Files & Concepts

### Z80 Instruction Set
- Located in `v2/Core/Spect.Net.SpectrumEmu/Cpu/InstructionSet/` (organized by prefix)
- Each instruction implements `IInstruction` interface
- Emulation uses opcode lookup tables for performance

### ROM Disassembly Annotations
- `v2/Core/Spect.Net.SpectrumEmu/Disasm/` contains ROM symbol maps and comments
- Annotations provide labels, comments for Z80 ROM code
- Used in disassembly tool window

### Tape File Format Support
- **TzxLoader**: Reads `.TZX` files (complex format with blocks)
- **TapLoader**: Reads `.TAP` files (simple format)
- Both implement `ITapeDataBock` interface for polymorphism

### VM State Serialization
- **VmStateSerializer**: Saves/loads complete VM state
- Used for fast program load and state checkpoints
- Part of debugging workflow (pause/resume/step)

### Project Configuration
- **ProjectConfig.xaml**: Build configuration properties
- **SpConf** items: Spectrum-specific project settings
- **Rules** directory: MSBuild rule definitions for `.xaml` files

## Common Development Tasks

### Adding a New Z80 Instruction
1. Create instruction class in appropriate prefix folder: `v2/Core/Spect.Net.SpectrumEmu/Cpu/InstructionSet/`
2. Implement `IInstruction` interface
3. Update opcode table in `Z80Cpu.cs` or prefix table
4. Add test case in `v2/Tests/Spect.Net.SpectrumEmu.Test/CpuTests/`

### Fixing Assembler Issues
1. Grammar may need changes: Edit `.g4` file in grammar generators
2. Run ANTLR generator to regenerate parser
3. Update visitor implementation in `Spect.Net.Assembler`
4. Add test case in `v2/Tests/Spect.Net.Assembler.Test/`

### Extending Debugger Features
1. New watch expression features → modify `Spect.Net.EvalParser`
2. New memory introspection → extend `IMemoryDevice` or `SpectrumVirtualMachine`
3. New VS tool window → add to `Spect.Net.VsPackage` and register in `.vsixmanifest`

### Testing Emulator Accuracy
- `v2/Tests/Spect.Net.SpectrumEmu.Test/CpuTests/` - Instruction execution tests
- `MemoryTests/` - Memory access and pagination
- `DeviceTests/` - Keyboard, tape, sound, etc.
- Focus on edge cases, undocumented behavior (Z80 has quirks)

## Debugging the Extension

### Running VS Extension in Debug
1. Set `Spect.Net.VsPackage` as startup project
2. Press F5 (launches experimental VS instance)
3. Create/open SpectNetIDE project in experimental instance
4. Breakpoints in extension code work normally

### Checking Extension Installation
- Experimental instance: %LOCALAPPDATA%\Microsoft\VisualStudio\[version]_[hash]exp\
- VSIX package: Built to `bin/Release/` when packaging

### Extension Logging
- Output window → "Spect.Net.VsPackage" pane shows diagnostic info
- Debug assertions trigger when compiled with DEBUG configuration

## Deployment & Versioning

### Before Release
Follow **DeployChecklist.md**:
1. Update `AssemblyVersion` and `AssemblyFileVersion` in:
   - `Spect.Net.VsPackage/Properties/AssemblyInfo.cs`
   - `Spect.Net.CodeDiscover.ProjectType/Properties/AssemblyInfo.cs` (if present)
2. Update version in `source.extension.vsixmanifest` files
3. Update `CURRENT_CPS_VERSION` in `SpectNetPackage.cs`
4. Verify all build rules linked as Embedded Resources
5. Full solution build and test
6. Upload VSIX to Visual Studio Marketplace

### Semantic Versioning
- **Major**: Breaking changes, new major features
- **Minor**: New features, backward compatible
- **Build**: Patches, hotfixes
- **Revision**: Cosmetic changes

## Code Style & Conventions

- **Language**: C# (.NET Framework / .NET Core)
- **Naming**: PascalCase for classes/namespaces, camelCase for variables/parameters
- **Code Format**: Use Visual Studio default (Tools → Options → Text Editor → C# → Code Style)
- **Documentation**: XML doc comments on public APIs
- **Tests**: xUnit (modern) or MSTest (legacy tests) depending on test project

## Key Git Considerations

- **Main branch**: Production releases
- **Develop branch**: Integration branch for features
- **Feature branches**: Feature/[name] off develop
- **Recent**: V2.0 Preview 11 released with ZX Basic support and random exception fixes

## Limitations & Known Issues

- **ZX BASIC debugging**: Not yet implemented in debugger (V2 Preview 11)
- **ZX Spectrum Next**: Emulator support in development
- **Platform support**: Windows-only (VS extension requirement)
- **Parser generation**: ANTLR generators must be run manually if grammar changes
