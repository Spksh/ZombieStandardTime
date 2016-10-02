using System;
using System.Windows;
using System.Windows.Controls;

namespace ZombieStandardTime.Controls
{
    /// <summary>
    /// http://jobijoy.blogspot.de/2007/10/time-picker-user-control.html
    /// </summary>
    public partial class TimeControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeControl), new UIPropertyMetadata(DateTime.Now.TimeOfDay, OnValueChanged));

        public static readonly DependencyProperty DaysProperty = DependencyProperty.Register("Days", typeof(int), typeof(TimeControl), new UIPropertyMetadata(0, OnTimeChanged, CoerceDays));

        public static readonly DependencyProperty HoursProperty = DependencyProperty.Register("Hours", typeof(int), typeof(TimeControl), new UIPropertyMetadata(0, OnTimeChanged, CoerceHours));

        public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register("Minutes", typeof(int), typeof(TimeControl), new UIPropertyMetadata(0, OnTimeChanged, CoerceMinutes));

        public static readonly DependencyProperty SecondsProperty = DependencyProperty.Register("Seconds", typeof(int), typeof(TimeControl), new UIPropertyMetadata(0, OnTimeChanged, CoerceSeconds));

        public TimeSpan Value
        {
            get
            {
                return (TimeSpan)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public int Days
        {
            get
            {
                return (int)GetValue(DaysProperty);
            }
            set
            {
                SetValue(DaysProperty, value);
            }
        }
        

        public int Hours
        {
            get
            {
                return (int)GetValue(HoursProperty);
            }
            set
            {
                SetValue(HoursProperty, value);
            }
        }


        public int Minutes
        {
            get
            {
                return (int)GetValue(MinutesProperty);
            }
            set
            {
                SetValue(MinutesProperty, value);
            }
        }


        public int Seconds
        {
            get
            {
                return (int)GetValue(SecondsProperty);
            }
            set
            {
                SetValue(SecondsProperty, value);
            }
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = obj as TimeControl;

            if (control != null)
            {
                control.Days = ((TimeSpan)e.NewValue).Days;
                control.Hours = ((TimeSpan)e.NewValue).Hours;
                control.Minutes = ((TimeSpan)e.NewValue).Minutes;
                control.Seconds = ((TimeSpan)e.NewValue).Seconds;
            }
        }

        private static void OnTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TimeControl control = obj as TimeControl;
            
            if (control != null)
            {
                control.Value = new TimeSpan(control.Days, control.Hours, control.Minutes, control.Seconds);
            }
        }

        private static object CoerceDays(DependencyObject d, object basevalue)
        {
            if (basevalue is int)
            {
                int days = (int)basevalue;

                if (days < 0)
                {
                    return 0;
                }

                return basevalue;
            }

            return 0;
        }

        private static object CoerceHours(DependencyObject d, object basevalue)
        {
            if (basevalue is int)
            {
                int hours = (int)basevalue;

                if (hours < 0)
                {
                    return 0;
                }

                if (hours > 23)
                {
                    return 23;
                }

                return basevalue;
            }

            return 0;
        }

        private static object CoerceMinutes(DependencyObject d, object basevalue)
        {
            if (basevalue is int)
            {
                int minutes = (int)basevalue;

                if (minutes < 0)
                {
                    return 0;
                }

                if (minutes > 59)
                {
                    return 59;
                }

                return basevalue;
            }

            return 0;
        }

        private static object CoerceSeconds(DependencyObject d, object basevalue)
        {
            if (basevalue is int)
            {
                int seconds = (int)basevalue;

                if (seconds < 0)
                {
                    return 0;
                }

                if (seconds > 59)
                {
                    return 59;
                }

                return basevalue;
            }

            return 0;
        }
    } 
}
