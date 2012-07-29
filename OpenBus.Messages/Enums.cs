using System;

namespace OpenBus.Messages
{
    /// <summary>
    /// 
    /// </summary>
    [Flags()]
    public enum PositionState
    {
        Unknown = 0,
        Open = 1,
        Closed = 2
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags()]
    public enum LockState
    {
        Unknown = 0,
        Unlocked = 1,
        Locked = 2
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags()]
    public enum ToggleState
    {
        Unknown = 0,
        On = 1,
        Off = 2
    }

    public enum Location
    {
        IsHome = 1,
        LeavingHome = 2,
        CommingHome = 3
    }
}
