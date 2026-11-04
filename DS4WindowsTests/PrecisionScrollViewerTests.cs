using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DS4WinWPF;
using DS4WinWPF.DS4Forms;

namespace DS4WindowsTests;

[TestClass]
public sealed class PrecisionScrollViewerTests
{
    [TestMethod]
    public void TouchpadFragmentsProduceOneScrollStepPerCompleteNotch()
    {
        OnSta(() =>
        {
            var scroll = new ScrollProbe();
            var viewer = new ViewerProbe(scroll);
            for (int fragment = 0; fragment < 7; fragment++)
                Assert.IsTrue(viewer.Wheel(-15).Handled);
            Assert.AreEqual(0, scroll.Steps.Count);

            Assert.IsTrue(viewer.Wheel(-15).Handled);
            CollectionAssert.AreEqual(new[] { -1 }, scroll.Steps);
        });
    }

    [TestMethod]
    public void MultipleNotchesKeepTheirRemainderForTheNextEvent()
    {
        OnSta(() =>
        {
            var scroll = new ScrollProbe();
            var viewer = new ViewerProbe(scroll);
            viewer.Wheel(-300);
            CollectionAssert.AreEqual(new[] { -1, -1 }, scroll.Steps);
            viewer.Wheel(-60);
            viewer.Wheel(120);
            CollectionAssert.AreEqual(new[] { -1, -1, -1, 1 }, scroll.Steps);
        });
    }

    [TestMethod]
    public void ReversingDirectionCancelsTheUnscrolledFraction()
    {
        OnSta(() =>
        {
            var scroll = new ScrollProbe();
            var viewer = new ViewerProbe(scroll);
            viewer.Wheel(-90);
            viewer.Wheel(30);
            viewer.Wheel(180);
            CollectionAssert.AreEqual(new[] { 1 }, scroll.Steps);
        });
    }

    [TestMethod]
    public void AlreadyHandledEventsDoNotConsumeAnotherScrollTargetsInput()
    {
        OnSta(() =>
        {
            var scroll = new ScrollProbe();
            var viewer = new ViewerProbe(scroll);
            viewer.Wheel(-120, handled: true);
            viewer.Wheel(0);
            Assert.AreEqual(0, scroll.Steps.Count);
        });
    }

    [TestMethod]
    public void ClosedDropdownForwardsPrecisionScrollWithoutChangingItsSelection()
    {
        OnSta(() =>
        {
            RuntimeHelpers.RunClassConstructor(typeof(App).TypeHandle);
            var scroll = new ScrollProbe();
            var viewer = new ViewerProbe(scroll);
            var panel = new StackPanel();
            var dropdown = new ComboBox { SelectedIndex = 0 };
            dropdown.Items.Add("First profile");
            dropdown.Items.Add("Second profile");
            dropdown.SelectedIndex = 0;
            panel.Children.Add(dropdown);
            viewer.Content = panel;

            for (int fragment = 0; fragment < 8; fragment++)
            {
                var wheel = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -15)
                {
                    RoutedEvent = UIElement.PreviewMouseWheelEvent,
                };
                dropdown.RaiseEvent(wheel);
                Assert.IsTrue(wheel.Handled);
            }

            Assert.AreEqual(0, dropdown.SelectedIndex);
            CollectionAssert.AreEqual(new[] { -1 }, scroll.Steps);
        });
    }

    private static void OnSta(Action action)
    {
        Exception failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(20)), "Precision scrolling did not finish.");
        if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private sealed class ViewerProbe : PrecisionScrollViewer
    {
        public ViewerProbe(ScrollProbe scroll)
        {
            ScrollInfo = scroll;
            scroll.ScrollOwner = this;
        }

        public MouseWheelEventArgs Wheel(int delta, bool handled = false)
        {
            var wheel = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
            {
                RoutedEvent = Mouse.MouseWheelEvent,
                Handled = handled,
            };
            OnMouseWheel(wheel);
            return wheel;
        }
    }

    private sealed class ScrollProbe : IScrollInfo
    {
        public List<int> Steps { get; } = new();
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; } = true;
        public double ExtentHeight => 2000;
        public double ExtentWidth => 200;
        public double HorizontalOffset => 0;
        public double VerticalOffset => 0;
        public double ViewportHeight => 100;
        public double ViewportWidth => 200;
        public ScrollViewer ScrollOwner { get; set; }
        public void MouseWheelDown() => Steps.Add(-1);
        public void MouseWheelUp() => Steps.Add(1);
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }
        public void LineDown() { }
        public void LineLeft() { }
        public void LineRight() { }
        public void LineUp() { }
        public void PageDown() { }
        public void PageLeft() { }
        public void PageRight() { }
        public void PageUp() { }
        public void SetHorizontalOffset(double offset) { }
        public void SetVerticalOffset(double offset) { }
        public Rect MakeVisible(System.Windows.Media.Visual visual, Rect rectangle) => rectangle;
    }
}
