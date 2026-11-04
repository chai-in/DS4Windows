using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS4WinWPF.DS4Forms;

public class PrecisionScrollViewer : ScrollViewer
{
    private int pendingWheelDelta;

    protected override void OnMouseWheel(MouseWheelEventArgs eventArgs)
    {
        if (eventArgs.Handled) return;

        pendingWheelDelta += eventArgs.Delta;
        int wheelSteps = pendingWheelDelta / Mouse.MouseWheelDeltaForOneLine;
        pendingWheelDelta %= Mouse.MouseWheelDeltaForOneLine;

        int direction = Math.Sign(wheelSteps);
        for (int step = 0; step < Math.Abs(wheelSteps); step++)
        {
            base.OnMouseWheel(new MouseWheelEventArgs(eventArgs.MouseDevice,
                eventArgs.Timestamp, direction * Mouse.MouseWheelDeltaForOneLine)
            {
                RoutedEvent = Mouse.MouseWheelEvent,
            });
        }

        eventArgs.Handled = true;
    }
}
