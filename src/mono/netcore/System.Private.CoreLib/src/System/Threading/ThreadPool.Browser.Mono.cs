// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
    public sealed partial class RegisteredWaitHandle : MarshalByRefObject
    {
        public bool Unregister(WaitHandle? waitObject)
        {
            throw new PlatformNotSupportedException();
        }
    }

    internal sealed partial class CompleteWaitThreadPoolWorkItem : IThreadPoolWorkItem
    {
        void IThreadPoolWorkItem.Execute()
        {
            Debug.Fail("Registered wait handles are currently not supported");
        }
    }

    public static partial class ThreadPool
    {
        // Time-sensitive work items are those that may need to run ahead of normal work items at least periodically. For a
        // runtime that does not support time-sensitive work items on the managed side, the thread pool yields the thread to the
        // runtime periodically (by exiting the dispatch loop) so that the runtime may use that thread for processing
        // any time-sensitive work. For a runtime that supports time-sensitive work items on the managed side, the thread pool
        // does not yield the thread and instead processes time-sensitive work items queued by specific APIs periodically.
        internal const bool SupportsTimeSensitiveWorkItems = false; // the timer currently doesn't queue time-sensitive work

        internal const bool EnableWorkerTracking = false;

        private static bool _callbackQueued;

        public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
        {
            if (workerThreads == 1 && completionPortThreads == 1)
                return true;
            return false;
        }

        public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
        {
            workerThreads = 1;
            completionPortThreads = 1;
        }

        public static bool SetMinThreads(int workerThreads, int completionPortThreads)
        {
            if (workerThreads == 1 && completionPortThreads == 1)
                return true;
            return false;
        }

        public static void GetMinThreads(out int workerThreads, out int completionPortThreads)
        {
            workerThreads = 1;
            completionPortThreads = 1;
        }

        public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
        {
            workerThreads = 1;
            completionPortThreads = 1;
        }

        public static int ThreadCount => 1;

        public static long CompletedWorkItemCount => 0;

        internal static void RequestWorkerThread()
        {
            if (_callbackQueued)
                return;
            _callbackQueued = true;
            QueueCallback();
        }

        internal static void NotifyWorkItemProgress()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool NotifyWorkItemComplete(object? threadLocalCompletionCountObject, int currentTimeMs)
        {
            return true;
        }

        internal static object? GetOrCreateThreadLocalCompletionCountObject() => null;

        private static void RegisterWaitForSingleObjectCore(WaitHandle? waitObject, RegisteredWaitHandle registeredWaitHandle) =>
            throw new PlatformNotSupportedException();

        [DynamicDependency("Callback")]
        [DynamicDependency("PumpThreadPool")]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void QueueCallback();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void PumpThreadPool(); // NOTE: this method is called via reflection by test code

        private static void Callback()
        {
            _callbackQueued = false;
            ThreadPoolWorkQueue.Dispatch();
        }
    }
}
