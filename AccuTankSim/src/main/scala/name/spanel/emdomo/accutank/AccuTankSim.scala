package name.spanel.emdomo.accutank

object AccuTankSim extends App {
  val slots = 50

  val wantedPower = 8000f
  val maxTemp = 75f
  val retTemp = 30f
  val initTemp = maxTemp

  val middlePower = 6000f
  val bottomPower = 7500f
  val tankVolume = 1000f

  var tank = new Tank(tankVolume, slots, initTemp)

  val hdo = new HDOSwitch
  val middleHeat = new HDOHeater(maxTemp, middlePower, hdo)
  val bottomHeat = new HDOHeater(maxTemp, bottomPower, hdo)
  tank = tank.addHeatSource(slots/2, middleHeat)
  tank = tank.addHeatSource(tank.bottomLevel, bottomHeat)

  var tankConsume = new TankWithConsumption(tank, ConsumeTank(retTemp), () => wantedPower, () => println("Out of power") )
  class HDOSwitch {
    var on = false
  }
  class HDOHeater(temp: Float, power: Float, val hdo: HDOSwitch) extends Heater(temp, power) {
    override def apply(temp: Float) = {
      if (hdo.on) super.apply(temp)
      else 0
    }
  }

  val hour = 3600f
  val step = 60f // a minute step is enough
  for (i <- 0 until 10) {
    hdo.on = true
    tankConsume = tankConsume.simulateLongTime(9 * hour, step)

    hdo.on = false
    tankConsume = tankConsume.simulateLongTime(4 * hour, step)

    hdo.on = true
    tankConsume = tankConsume.simulateLongTime(7 * hour, step)

    hdo.on = false
    tankConsume = tankConsume.simulateLongTime(3 * hour, step)
  }

}
