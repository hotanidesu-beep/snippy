using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace snippy
{
    public partial class KnobControl : UserControl
    {
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(KnobControl), new PropertyMetadata(0.0, OnAngleChanged));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public event EventHandler<double> AngleChangedEvent;

        public KnobControl()
        {
            InitializeComponent();
        }

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as KnobControl;
            if (control != null && control.knobRotation != null)
            {
                control.knobRotation.Angle = (double)e.NewValue;
                control.AngleChangedEvent?.Invoke(control, (double)e.NewValue);
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double step = 30;
            if (e.Delta > 0)
            {
                Angle += step;
            }
            else
            {
                Angle -= step;
            }
            e.Handled = true;
        }
    }
}
