package name.spanel.emdomo.accutank

import name.spanel.emdomo.accutank.Tank.HeatSource
import org.scalactic.TolerantNumerics
import org.scalatest.{Matchers, FlatSpec}

class TankTest extends FlatSpec with Matchers {

  val eps = 0.0001f
  val minute = 60f
  val hour = 60*minute

  val initTemp = 30f
  val tgtTemp = 75f
  val retTemp = 40f

  implicit val custom = TolerantNumerics.tolerantDoubleEquality(eps)

  def simulateTank(tank: Tank, time: Float) = {
    tank.simulateLongTime(time)
  }

  "Tank" can "be created" in {
    val tank = new Tank(100, 1, initTemp)
    tank.levelCount shouldBe 1
    tank.bottomLevel shouldBe 0
    tank.topTemperature shouldBe initTemp
    tank.botTemperature shouldBe initTemp
  }

  it should "be stable" in {
    var tank = new Tank(100, 10, initTemp)
    tank = simulateTank(tank, 100)
    tank.topTemperature shouldBe initTemp
    tank.botTemperature shouldBe initTemp
  }

  it should "be heated by a bottom permanent source" in {
    var tank = new Tank(100, 10, initTemp)
    val heating: HeatSource = _ => 1000
    tank = tank.addHeatSource(tank.bottomLevel, heating)
    tank = simulateTank(tank, 1*hour)
    tank.botTemperature should be > initTemp
    tank.topTemperature should be > initTemp
  }

  "Bottom heater" should "stop heating once done" in {
    var tank = new Tank(100, 10, initTemp)
    val heating = new Heater(tgtTemp, 1000)
    tank = tank.addHeatSource(tank.bottomLevel, heating)
    tank = simulateTank(tank, 10*hour)
    tank.botTemperature should be > initTemp
    tank.topTemperature should be >= tgtTemp
    tank.botTemperature should be >= tgtTemp
    tank.topTemperature shouldBe tgtTemp +- 1f
  }

  def middleHeatedTank = {
    var tank = new Tank(100, 10, initTemp)
    val heating = new Heater(tgtTemp, 1000)
    tank = tank.addHeatSource(5, heating)
    tank = simulateTank(tank, 5*hour)
    tank
  }

  "Middle heater" should "not heat bottom, while heating top faster" in {
    var tank = middleHeatedTank

    tank.topTemperature should be > initTemp
    tank.topTemperature should be >= tgtTemp
    tank.botTemperature shouldBe initTemp
  }

  "Heated tank" should "provide warm water at top, but not infinitely" in {
    var tank = middleHeatedTank

    val (pullWater, pullTank) = tank.pullTopLevel(retTemp)
    tank = pullTank

    pullWater should be >= tgtTemp
    tank.botTemperature shouldBe retTemp

    for ( i <-0 until tank.levelCount) {
      val (_, pullTank) = tank.pullTopLevel(retTemp)
      tank = pullTank
    }

    tank.topTemperature shouldBe retTemp
    tank.botTemperature shouldBe retTemp
  }

  "Heated tank" should "provide power to consume, but not infinitely" in {

    var tank = middleHeatedTank

    val wantedPower = 1900f

    class NotEnoughPower extends Exception

    var consumableTank = TankWithConsumption(tank, ConsumeTank(retTemp), () => wantedPower , () => throw new NotEnoughPower)

    consumableTank = consumableTank.simulateLongTime(1*hour)

    consumableTank.tank.topTemperature should be >= tgtTemp
    consumableTank.tank.botTemperature should be <= retTemp

    intercept[NotEnoughPower] {
      consumableTank = consumableTank.simulateLongTime(10*hour)
    }

  }
}
