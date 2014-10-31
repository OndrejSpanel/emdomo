using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  namespace FanControlPrograms
  {
    public class FanControlPredictDay: FanControl
    {
      public void Simulate(float deltaT, FanControlInput input) { }
      public bool FanStatus() { return true; }
      public string Name() { return "Predict Day"; }
    }

  }
}
