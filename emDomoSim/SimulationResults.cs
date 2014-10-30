using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  public class SimulationResults
  {
    public class FanControlResults
    {
      public string Name { get; set; }
      public float AvgTemp {get; set;}
      public float TempOsc {get; set;}
      public float FanOn { get; set; }

      public FanControlResults(string name, float avgTemp, float tempOsc, float fanOn)
      {
        Name = name;
        AvgTemp = avgTemp;
        TempOsc = tempOsc;
        FanOn = fanOn;
      }
    };

    public ObservableCollection<FanControlResults> Results
    {
      get;
      private set;
    }

    public SimulationResults()
    {
      Results = new ObservableCollection<FanControlResults>();
    }
  }
}
