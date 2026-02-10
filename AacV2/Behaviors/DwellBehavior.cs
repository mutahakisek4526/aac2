using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace AacV2.Behaviors;

public static class DwellBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DwellBehavior), new PropertyMetadata(false, OnChanged));

    public static readonly DependencyProperty DwellTimeMsProperty =
        DependencyProperty.RegisterAttached("DwellTimeMs", typeof(int), typeof(DwellBehavior), new PropertyMetadata(800));

    public static readonly DependencyProperty CooldownMsProperty =
        DependencyProperty.RegisterAttached("CooldownMs", typeof(int), typeof(DwellBehavior), new PropertyMetadata(300));

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.RegisterAttached("Progress", typeof(double), typeof(DwellBehavior), new PropertyMetadata(0.0));

    private static readonly Dictionary<UIElement, DwellState> States = new();

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static int GetDwellTimeMs(DependencyObject obj) => (int)obj.GetValue(DwellTimeMsProperty);
    public static void SetDwellTimeMs(DependencyObject obj, int value) => obj.SetValue(DwellTimeMsProperty, value);

    public static int GetCooldownMs(DependencyObject obj) => (int)obj.GetValue(CooldownMsProperty);
    public static void SetCooldownMs(DependencyObject obj, int value) => obj.SetValue(CooldownMsProperty, value);

    public static double GetProgress(DependencyObject obj) => (double)obj.GetValue(ProgressProperty);
    public static void SetProgress(DependencyObject obj, double value) => obj.SetValue(ProgressProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.MouseMove += OnMouseMove;
            States[element] = new DwellState();
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.MouseMove -= OnMouseMove;
            if (States.Remove(element, out var state))
            {
                state.Timer.Stop();
            }
        }
    }

    private static void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is UIElement element && States.TryGetValue(element, out var state) && state.StartedAt != default)
        {
            UpdateProgress(element, state);
        }
    }

    private static void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not UIElement element || !States.TryGetValue(element, out var state))
        {
            return;
        }

        if (DateTime.UtcNow < state.CooldownUntil)
        {
            return;
        }

        state.StartedAt = DateTime.UtcNow;
        state.Timer.Stop();
        state.Timer.Tick -= state.TickHandler;
        state.TickHandler = (_, _) => Tick(element, state);
        state.Timer.Tick += state.TickHandler;
        state.Timer.Start();
        SetProgress(element, 0);
    }

    private static void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not UIElement element || !States.TryGetValue(element, out var state))
        {
            return;
        }

        state.Timer.Stop();
        state.StartedAt = default;
        SetProgress(element, 0);
    }

    private static void Tick(UIElement element, DwellState state)
    {
        UpdateProgress(element, state);
        var dwell = Math.Max(100, GetDwellTimeMs(element));
        var elapsed = (DateTime.UtcNow - state.StartedAt).TotalMilliseconds;
        if (elapsed < dwell)
        {
            return;
        }

        state.Timer.Stop();
        state.CooldownUntil = DateTime.UtcNow.AddMilliseconds(Math.Max(0, GetCooldownMs(element)));
        state.StartedAt = default;
        SetProgress(element, 1);
        if (element is ButtonBase b && b.IsEnabled)
        {
            b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        _ = Task.Delay(120).ContinueWith(_ =>
        {
            element.Dispatcher.Invoke(() => SetProgress(element, 0));
        });
    }

    private static void UpdateProgress(UIElement element, DwellState state)
    {
        var dwell = Math.Max(100, GetDwellTimeMs(element));
        var elapsed = (DateTime.UtcNow - state.StartedAt).TotalMilliseconds;
        SetProgress(element, Math.Clamp(elapsed / dwell, 0, 1));
    }

    private sealed class DwellState
    {
        public DispatcherTimer Timer { get; } = new() { Interval = TimeSpan.FromMilliseconds(30) };
        public DateTime StartedAt { get; set; }
        public DateTime CooldownUntil { get; set; }
        public EventHandler? TickHandler { get; set; }
    }
}
