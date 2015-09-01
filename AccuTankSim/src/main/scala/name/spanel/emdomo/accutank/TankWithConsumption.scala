package name.spanel.emdomo.accutank

import name.spanel.emdomo.accutank.Tank.kcal

case class ConsumeTank(topMass: Float, topTemperature: Float, botMass: Float, botTemperature: Float) {
  def consumeMass(mass: Float) = {
    require(mass <= topMass + 1e-5)
    copy(topMass = topMass - mass, botMass = botMass + mass)
  }

  def consumePower(power: Float, time: Float) = {
    val mass = power * time / (kcal * (topTemperature - botTemperature))
    consumeMass(mass)
  }

  def consumableTime(power: Float, time: Float) = {
    if (topMass <= 0) {
      0f
    } else {
      val mass = power * time / (kcal * (topTemperature - botTemperature))
      if (mass <= topMass) {
        time
      } else {
        time * topMass / mass
      }
    }
  }
}

object ConsumeTank {
  def apply(bottomTemp: Float): ConsumeTank = ConsumeTank(0, 0, 0, bottomTemp)
}

case class TankWithConsumption(tank: Tank, consumeTank: ConsumeTank, consumption: () => Float)  extends Simulated {
  def simulate(implicit time: Float): TankWithConsumption = {
    // first draw from consume water, once this is not available, fill consume water from a tank
    val power = consumption()
    val canTime = consumeTank.consumableTime(power, time)
    if (canTime >= time) {
      val left = consumeTank.consumePower(power, time)
      copy(consumeTank = left)
    } else {
      val check = consumeTank.consumePower(power, canTime)
      assert(check.topMass <= 1e-6)
      val (pullWater, pullTank) = tank.pullTopLevel(consumeTank.botTemperature)
      // fill a new ConsumeTank
      val fractionTank = TankWithConsumption(pullTank, consumeTank.copy(topTemperature = pullWater, topMass = pullTank.levelMass, botMass = 0), consumption)
      fractionTank.simulate(time - canTime)
    }
  }
}
