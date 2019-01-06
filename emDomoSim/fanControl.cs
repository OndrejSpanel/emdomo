using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  public interface FanControlInput
  {
    float GetOutsideTemperature();
    float GetRoomTemperature();
  };

  public interface FanControl
  {
    void Simulate(float deltaT, FanControlInput input);
    bool FanStatus();
    string Name();
  }

  namespace FanControlPrograms
  {

    public class FanControlAlwaysOn : FanControl
    {
      public void Simulate(float deltaT, FanControlInput input) { }
      public bool FanStatus() { return true; }
      public string Name() { return "Always On";}
    }

    public class FanControlAlwaysOff : FanControl
    {
      public void Simulate(float deltaT, FanControlInput input) { }
      public bool FanStatus() { return false; }
      public string Name() { return "Always Off"; }
    }

    public class FanControlThermostat : FanControl
    {
      public string Name() { return "Thermostat"; }

      private bool fan_;

      const float roomTempOn = 13;
      const float roomTempOff = 12;
      const float tempDiffOn = 5;
      const float tempDiffOff = 3;

      public FanControlThermostat()
      {
        Reset();
      }
      public void Reset()
      {
        fan_ = false;
      }
      public void Simulate(float deltaT, FanControlInput input)
      {
        float roomTemp = input.GetRoomTemperature();
        float outTemp = input.GetOutsideTemperature();
        if (fan_)
        {
          if (roomTemp < roomTempOff || roomTemp - outTemp < tempDiffOff)
          {
            fan_ = false;
          }
        }
        else
        {
          if (roomTemp > roomTempOn && roomTemp - outTemp > tempDiffOn)
          {
            fan_ = true;
          }

        }
      }

      public bool FanStatus() { return fan_; }
    }
  }
}
