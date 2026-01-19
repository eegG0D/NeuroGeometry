using System;

namespace NeuroGeometry.Abstractions
{
    public interface IBiosensor
    {
        void Connect();
        void Disconnect();
        event Action<EegData> DataReceived;

        // NEW: Event for errors
        event Action<string> ErrorOccurred;
    }
}