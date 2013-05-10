using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace emDomoSim
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public class RoomSimulator : FanControl.Input
    {
      int dayInYear_;
      float timeOfDay_;

      WeatherSim weatherSim_;
      State state_;

      public RoomSimulator()
      {
        weatherSim_ = new WeatherSim();
      }

      public class State: WeatherSim.Weather
      {
        public float roomTemperature_;

        public State()
        {
          roomTemperature_ = 15;
        }
        public State(WeatherSim.Weather w) : base(w)
        {
          roomTemperature_ = 15;
        }

        public void SetBase(WeatherSim.Weather w) {}
      }

      public void SetTime(int dayInYear, float timeOfDay) { dayInYear_ = dayInYear; timeOfDay_ = timeOfDay; }
      public State Simulate()
      {
        WeatherSim.Weather weather = weatherSim_.Simulate(dayInYear_, timeOfDay_);
        State result = new State(weather);
        state_ = result;
        return result;
      }

      public float GetOutsideTemperature() { return state_.curTemp; }
      public float GetRoomTemperature() { return state_.roomTemperature_; }
    };

    FanControl fanControl_;

    RoomSimulator room_;

    public MainWindow()
    {
      InitializeComponent();
      fanControl_ = new FanControl();
      room_ = new RoomSimulator();
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {

      // The Work to perform on another thread
      ThreadStart start = delegate()
      {
        // move time from left to right
        for (double f = 0; f < 24; f += 0.05)
        {
          Dispatcher.Invoke(delegate()
          {
            time.Value = f;
          });
          System.Threading.Thread.Sleep(20);
        }
      };

      // Create the thread and kick it started!
      new Thread(start).Start();
    }
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Simulate_Click(object sender, RoutedEventArgs e)
    {
      Run_Click(sender, e); //
    }

    private void Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      float time = (float)e.NewValue;
      int dayInYear = 180;
      try
      {
        DateTime date = new DateTime(2000, Convert.ToInt16(month.Text), Convert.ToInt16(dayInMonth.Text));
        dayInYear = date.DayOfYear;
      }
      catch
      {
      }

      room_.SetTime(dayInYear, time);
      RoomSimulator.State roomState = room_.Simulate();

      TimeSpan ts= TimeSpan.FromHours(time);
      curTime.Text = ts.ToString(@"hh\:mm");
      curTemp.Text = roomState.curTemp.ToString("0.0");
    }

    private void SetWeather_Click(object sender, RoutedEventArgs e)
    {
    }
  }
}
