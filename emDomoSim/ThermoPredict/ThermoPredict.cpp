// ThermoPredict.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ThermoPredict.h"

#include <math.h>
#include <limits.h>

#include <array>
#include <algorithm>
#include <numeric>

/// daytime (in hours)
float currDayTime;

float sumTemp = 0;
float sumDuration = 0;

float lastTemp;

static const float houseTemp = 19.0f;
static const float targetTemp = 10.0f;

/// assume house-room temperature influence linear to the temperature difference, measure the factor
float houseInfluence = 1.0f;
float fanInfluence = 0.5f;

class History {
  static const int nValues = 48;
  float temp[nValues];
  int curr;

public:
  History() {
    for (int i=0; i<nValues; i++) temp[i] = 0;
    curr = 0;
  }

  void AddTemp(float t) {
    temp[curr++] = t;
    if (curr>=nValues) curr = 0;
  }

  float TempAtTime(float time) {
    int index = (int)floor(time);
    float frac = time-index;

    float pTemp = temp[(curr+nValues+index-1)%nValues];
    float nTemp = temp[(curr+nValues+index)%nValues];
    return pTemp*(1-frac)+nTemp*frac;
  }

  float Forecast(float currTemp, float time) {
    float tempNowMinus24h = TempAtTime(0);
    float tempTimeMinus24h = TempAtTime(time);
    return currTemp+tempTimeMinus24h-tempNowMinus24h;

  }
};

class WatchThermalFlow {
  float timeElapsed;
  float thermalFlow;
  float lastTemp;
  bool first;

public:
  WatchThermalFlow() {
    Reset();
  }

  void Reset() {
    thermalFlow = 0;
    timeElapsed = 0;
    first = true;
    lastTemp = 0;
  }

  void AddSample(float deltaT, float currTemp, float flowDrive) {
    if (first)
    {
      lastTemp = currTemp;
      first = false;
    }
    thermalFlow += (currTemp-lastTemp)/flowDrive;
    timeElapsed += deltaT;
  }

  float TimeElapsed() const {return timeElapsed;}

  float Flow() const {return thermalFlow/timeElapsed;}
};

static History THistory;
static WatchThermalFlow WatchFlow;
static bool WatchingFan;

static float Lerp(float y0, float y1, float x) {return y0+(y1-y0)*x;}


bool DecideFan(float outTempCurr, float roomTemp) {
  if (roomTemp<=targetTemp) return false;
  // build a histogram describing out temperature in the next 24 hours
  float minTemp = FLT_MAX, maxTemp = FLT_MIN;
  for (float time = 0; time<24; time += 0.1f) {
    float outTemp = THistory.Forecast(outTempCurr,time);
    minTemp = std::min(minTemp,outTemp);
    maxTemp = std::max(maxTemp,outTemp);

  }
  const int nHistogram = 100;
  int histogram[nHistogram];
  for (int i=0; i<nHistogram; i++) histogram[i] = 0;
  float deltaT = 0.1f;
  for (float time = 0; time<24; time += deltaT) {
    float outTemp = THistory.Forecast(outTempCurr,time);
    int percentil = int((outTemp-minTemp)*nHistogram/(maxTemp-minTemp));
    percentil = std::max(0,std::min(percentil,nHistogram-1));
    histogram[percentil]++;
  }
  // find limit temperature needed to turn on to reach the target temperature
  float simulatedTemp = roomTemp;
  for (int i=0; i<100; i++) {
    float temp = (i+1)*(maxTemp-minTemp)/100+minTemp;
    float time = histogram[i]*deltaT;
    simulatedTemp += (temp-roomTemp)*time*fanInfluence;
    if (simulatedTemp<=targetTemp)
    {
      return outTempCurr<=temp;
    }
  }

  return outTempCurr<targetTemp;
}

THERMOPREDICT_API
bool ThermoPredictSimulate(float deltaT, float outTemp, float roomTemp) {
  // plot a curve, try to fit the history
  if (sumDuration + deltaT >= 1)
  {
    float deltaTIn = 1-sumDuration;
    float deltaTOut = sumDuration + deltaT - 1;
    sumTemp += outTemp * deltaTIn;
    float avgTemp = sumTemp;
    lastTemp = avgTemp;
    sumTemp = deltaTOut;
    sumDuration = 0;
    THistory.AddTemp(avgTemp);
  }
  else
  {
    sumTemp += outTemp * deltaT;
    sumDuration += deltaT;

  }

  bool fan = DecideFan(outTemp,roomTemp);

  // measure room / air influence
  // assume temperature can be modeled by the following equation
  // (beware: this is intentional simplification, in reality the out temperature is also influencing the room even with fan off, however that would lead to too complex system to solve)
  // tRoomNew = tRoomOld + deltaT*(fanOnOff*(outTemp-roomTemp)*fanInfluence+houseInfluence*(houseTemp-roomTemp))
  // measure response with fan on / off and try to estimate the factors
  // with fan off the response is:
  // tRoomNew = tRoomOld + deltaT*houseInfluence*(houseTemp-roomTemp)
  // v v v 
  // houseInfluence = (tRoomNew-tRoomOld)/(deltaT*(houseTemp-roomTemp))
  if (fan!=WatchingFan) {
    // evaluate last slope
    if (WatchFlow.TimeElapsed()>2) {
      float newInfluence = WatchFlow.Flow();
      if (!WatchingFan) {
        houseInfluence = Lerp(houseInfluence,newInfluence,0.2f);
      } else {
        fanInfluence = Lerp(fanInfluence,newInfluence,0.2f);
      }

    }
    WatchingFan = fan;
    WatchFlow.Reset();
  }
  if (!WatchingFan) {
    WatchFlow.AddSample(deltaT,roomTemp,houseTemp-roomTemp);
  } else {
    // TODO: subtract result of house thermal flow
    WatchFlow.AddSample(deltaT,roomTemp,outTemp-roomTemp);
  }
  return fan;
}
