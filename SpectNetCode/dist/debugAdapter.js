"use strict";
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));

// src/debugAdapter.ts
var path = __toESM(require("path"));
var import_child_process = require("child_process");
var serverPath = path.join(__dirname, "..", "..", "Spect.Net.Dap", "bin", "Debug", "net6.0", "Spect.Net.Dap.dll");
var dotnet = "dotnet";
var serverProcess = (0, import_child_process.spawn)(dotnet, [serverPath]);
process.stdin.pipe(serverProcess.stdin);
serverProcess.stdout.pipe(process.stdout);
serverProcess.stderr.pipe(process.stderr);
serverProcess.on("exit", (code) => {
  process.exit(code);
});
serverProcess.on("error", (err) => {
  console.error(`Failed to start server: ${err}`);
  process.exit(1);
});
//# sourceMappingURL=debugAdapter.js.map
