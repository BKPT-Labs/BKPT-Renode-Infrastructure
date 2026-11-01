//
// Copyright (c) 2010-2025 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System.Collections.Generic;

namespace Antmicro.Renode.Peripherals.CPU
{
    public interface ICpuSupportingGdb : ICPUWithHooks, ICPUWithRegisters
    {
        void EnterSingleStepModeSafely(HaltArguments args);

        string GDBArchitecture { get; }

        List<GDBFeatureDescriptor> GDBFeatures { get; }

        bool DebuggerConnected { get; set; }

        bool ShouldEnterDebugMode { get; set; }

        /// <summary>
        /// Grows each time the CPU is given instructions to execute and is not cleared by a reset,
        /// so a debugger can tell whether the CPU ran between two readings, including at a halt
        /// reported during that execution.
        /// </summary>
        ulong ExecutionMarker { get; }
    }
}