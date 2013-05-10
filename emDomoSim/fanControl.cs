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
    public void Simulate(float deltaT)
    {
    }

    public bool FanStatus()
    {
      return fan_;
    }
  }
}
