using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace WPF_Player
{
    /// <summary>
    /// Interaction logic for UCProcessBar.xaml
    /// </summary>
    public partial class UCProcessBar : UserControl,INotifyPropertyChanged
    {
        public UCProcessBar()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private double minValue = 0;

        public double MinValue
        {
            get { return minValue; }
            set { minValue = value; }
        }
        private double maxValue = 100;

        public double MaxValue
        {
            get { return maxValue; }
            set { maxValue = value; }
        }
        private double currentValue = 0;

        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; RealValue = this.ActualWidth / (MaxValue - minValue) * value; }
        }

        private double realValue;//经过转换后的值

        public double RealValue
        {
            get { return realValue; }
            set { realValue = value; OnPropertyChanged("RealValue"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        public event Action<double> PlayProcessChanged;
        private bool canMove = false;
        private double x = 0;
        //private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    canMove = true;
        //    Rectangle r=sender as Rectangle;
        //    x = e.GetPosition(r.Parent as Canvas).X;
        //}

        //private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!canMove)
        //        return;
        //    Rectangle r=sender as Rectangle;
        //    double originalLeft=Canvas.GetLeft(r);
        //    double x1 = e.GetPosition(r.Parent as Canvas).X;
        //    double change = x1 - x;
        //    double afterLeft = originalLeft + change;
        //    Canvas.SetLeft(r,afterLeft);
        //    x = x1;
        //}

        //private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    canMove = false;
        //    x = 0;
        //    if (PlayProcessChanged != null)
        //    {
        //        PlayProcessChanged(RealValue * (MaxValue - MinValue) / 200);
        //    }
        //}

        //private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (canMove)
        //    {
        //        canMove = false;
        //        x = 0;
        //        if (PlayProcessChanged != null)
        //        {
        //            PlayProcessChanged(RealValue * (MaxValue - MinValue) / 200);
        //        }
        //    }
        //}

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;

            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (PlayProcessChanged != null)
            {
                PlayProcessChanged(RealValue * (MaxValue - MinValue) / 200);
            }
        }
    }
    public class ValueConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = (double)value;
            string para = parameter.ToString();
            switch (para)
            {
                case "Button": return v + 1;
                case "Rectangle": return v;
                default: return v;                   
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = (double)value;
            switch (parameter.ToString())
            {
                case "Button": return v - 1;

                default: return v;
                    
            }
        }
    }
}
