// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Net
{
    [EventSource(Name = "Private.InternalDiagnostics.System.Net.Sockets", LocalizationResources = "FxResources.System.Net.Sockets.SR")]
    internal sealed partial class NetEventSource
    {
        private const int AcceptedId = NextAvailableEventId;
        private const int ConnectedId = AcceptedId + 1;
        private const int ConnectedAsyncDnsId = ConnectedId + 1;
        private const int NotLoggedFileId = ConnectedAsyncDnsId + 1;

        [NonEvent]
        public static void Accepted(Socket socket, object? remoteEp, object? localEp)
        {
            if (Log.IsEnabled())
            {
                Log.Accepted(IdOf(remoteEp), IdOf(localEp), GetHashCode(socket));
            }
        }

        [Event(AcceptedId, Keywords = Keywords.Default, Level = EventLevel.Informational)]
        private void Accepted(string remoteEp, string localEp, int socketHash)
        {
            WriteEvent(AcceptedId, remoteEp, localEp, socketHash);
        }

        [NonEvent]
        public static void Connected(Socket socket, object? localEp, object? remoteEp)
        {
            if (Log.IsEnabled())
            {
                Log.Connected(IdOf(localEp), IdOf(remoteEp), GetHashCode(socket));
            }
        }

        [Event(ConnectedId, Keywords = Keywords.Default, Level = EventLevel.Informational)]
        private void Connected(string localEp, string remoteEp, int socketHash)
        {
            WriteEvent(ConnectedId, localEp, remoteEp, socketHash);
        }

        [NonEvent]
        public static void ConnectedAsyncDns(Socket socket)
        {
            if (Log.IsEnabled())
            {
                Log.ConnectedAsyncDns(GetHashCode(socket));
            }
        }

        [Event(ConnectedAsyncDnsId, Keywords = Keywords.Default, Level = EventLevel.Informational)]
        private void ConnectedAsyncDns(int socketHash)
        {
            WriteEvent(ConnectedAsyncDnsId, socketHash);
        }

        [NonEvent]
        public static void NotLoggedFile(string filePath, Socket socket, SocketAsyncOperation completedOperation)
        {
            if (Log.IsEnabled())
            {
                Log.NotLoggedFile(filePath, GetHashCode(socket), completedOperation);
            }
        }

        [Event(NotLoggedFileId, Keywords = Keywords.Default, Level = EventLevel.Informational)]
        private void NotLoggedFile(string filePath, int socketHash, SocketAsyncOperation completedOperation)
        {
            WriteEvent(NotLoggedFileId, filePath, socketHash, (int)completedOperation);
        }

        /// <summary>Logs the contents of a buffer.</summary>
        /// <param name="thisOrContextObject">`this`, or another object that serves to provide context for the operation.</param>
        /// <param name="buffer">The buffer to be logged.</param>
        /// <param name="memberName">The calling member.</param>
        [NonEvent]
        public static void DumpBuffer(object thisOrContextObject, Memory<byte> buffer, [CallerMemberName] string? memberName = null)
        {
            DumpBuffer(thisOrContextObject, buffer, 0, buffer.Length, memberName);
        }

        /// <summary>Logs the contents of a buffer.</summary>
        /// <param name="thisOrContextObject">`this`, or another object that serves to provide context for the operation.</param>
        /// <param name="buffer">The buffer to be logged.</param>
        /// <param name="offset">The starting offset from which to log.</param>
        /// <param name="count">The number of bytes to log.</param>
        /// <param name="memberName">The calling member.</param>
        [NonEvent]
        public static void DumpBuffer(object thisOrContextObject, Memory<byte> buffer, int offset, int count, [CallerMemberName] string? memberName = null)
        {
            if (Log.IsEnabled())
            {
                if (offset < 0 || offset > buffer.Length - count)
                {
                    Debug.Fail($"Invalid {nameof(DumpBuffer)} Args. Length={buffer.Length}, Offset={offset}, Count={count}");
                    return;
                }

                buffer = buffer.Slice(offset, Math.Min(count, MaxDumpSize));
                byte[] slice = MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment) && arraySegment.Offset == 0 && arraySegment.Count == buffer.Length ?
                    arraySegment.Array! :
                    buffer.ToArray();

                Log.DumpBuffer(IdOf(thisOrContextObject), memberName, slice);
            }
        }
    }
}
