/**
 * Represents the configuration for a SpectNetIDE project.
 * This corresponds to the spectnet.config.json file.
 */
export interface SpectNetProjectConfiguration {
    /**
     * The ZX Spectrum model to emulate.
     * Default: "ZX Spectrum 48K"
     */
    model?: string;

    /**
     * The edition of the model (e.g., "PAL", "NTSC").
     * Default: "PAL"
     */
    edition?: string;

    /**
     * Compiler options for the Z80 assembler.
     */
    compiler?: {
        /**
         * The default output directory for compiled files.
         */
        outputDirectory?: string;

        /**
         * Whether to export debug information.
         * Default: true
         */
        exportDebugInfo?: boolean;

        /**
         * Predefined compilation symbols.
         */
        predefinedSymbols?: string[];

        /**
         * The default start address of the compilation.
         */
        defaultStartAddress?: number;

        /**
         * The default displacement address of the compilation.
         */
        defaultDisplacement?: number;
    };

    /**
     * The default program file to run/debug.
     */
    defaultProgram?: string;
}
