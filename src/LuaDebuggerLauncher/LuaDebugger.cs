﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.Debugger.Lua
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.ProjectSystem;
    using Microsoft.VisualStudio.ProjectSystem.Debug;
    using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

    /// <summary>
    /// A Visual C++ extension that launches a custom debugger.
    /// </summary>
    [ExportDebugger("LuaDebugger")] // Keep this string in sync with the one in your debugger's XAML file.
    [AppliesTo(ProjectCapabilities.VisualC)]
    public class LuaDebugger : DebugLaunchProviderBase
    {
        [ImportingConstructor]
        public LuaDebugger(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        /// <summary>
        /// Gets project properties that the debugger needs to launch.
        /// </summary>
        [Import]
        private Rules.RuleProperties DebuggerProperties { get; set; }

        public override async Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
        {
            var properties = await this.DebuggerProperties.GetLuaDebuggerPropertiesAsync();
            string commandValue = await properties.LocalDebuggerCommand.GetEvaluatedValueAtEndAsync();
            return !string.IsNullOrEmpty(commandValue);
        }

        public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
        {
            var settings = new DebugLaunchSettings(launchOptions);

            // The properties that are available via DebuggerProperties are determined by the property XAML files in your project.
            var debuggerProperties = await this.DebuggerProperties.GetLuaDebuggerPropertiesAsync();
            settings.Executable = await debuggerProperties.LocalDebuggerCommand.GetEvaluatedValueAtEndAsync();
            if (settings.Executable == null)
            {
                var generalProperties = await this.DebuggerProperties.GetConfigurationGeneralPropertiesAsync();
                settings.Executable = await generalProperties.TargetPath.GetEvaluatedValueAtEndAsync();
            }

            settings.Arguments = await debuggerProperties.LocalDebuggerCommandArguments.GetEvaluatedValueAtEndAsync();
            settings.CurrentDirectory = await debuggerProperties.LocalDebuggerWorkingDirectory.GetEvaluatedValueAtEndAsync();
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.LaunchDebugEngineGuid = new Guid(Microsoft.VisualStudio.Debugger.Lua.EngineConstants.EngineId);// DebuggerEngines.NativeOnlyEngine;

            return new IDebugLaunchSettings[] { settings };
        }
    }
}
