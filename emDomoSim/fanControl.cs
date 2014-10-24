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

    const float roomTempOn = 13;
    const float roomTempOff = 12;
    const float tempDiffOn = 5;
    const float tempDiffOff = 3;

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

    public bool FanStatus()
    {
      return fan_;
    }
  }
}
