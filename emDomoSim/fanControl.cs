using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  class FanControl
  {
    bool fan_;
    float tempOn = 10;
    float tempOff = 12;

    public interface Input
    {
      float GetOutsideTemperature();
      float GetRoomTemperature();
    };
    public FanControl()
    {
      Reset();
    }
    public void Reset()
    {
      fan_ = false;
    }
    public void Simulate(float deltaT, Input input)
    {
      if (fan_)
      {
        if (input.GetOutsideTemperature() > tempOff)
        {
          fan_ = false;
        }
      }
      else
      {
        if (input.GetOutsideTemperature() < tempOn)
        {
          fan_ = true;
        }

      }
    }

    public bool FanStatus()
    {
      return fan_;
    }
  }
}
