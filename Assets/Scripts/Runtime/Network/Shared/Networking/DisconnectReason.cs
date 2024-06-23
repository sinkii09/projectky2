using UnityEngine;

public class DisconnectReason
{
    public ConnectStatus Reason { get; private set; } = ConnectStatus.Undefined;
    public void SetDisconnectReason(ConnectStatus reason)
    {
        //using an explicit setter here rather than the auto-property, to make the code locations where disconnect information is set more obvious.
        Debug.Assert(reason != ConnectStatus.Success);
        Reason = reason;
    }
    public void Clear()
    {
        Reason = ConnectStatus.Undefined;
    }
    public bool HasTransitionReason => Reason != ConnectStatus.Undefined;
}
