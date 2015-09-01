package name.spanel.emdomo.accutank

import name.spanel.emdomo.accutank.Tank.HeatSource
import org.scalactic.TolerantNumerics
import org.scalatest.{Matchers, FlatSpec}
import scala.util.control.Breaks._

class TankTest extends FlatSpec with Matchers {

  val eps = 0.0001f
  implicit val custom = TolerantNumerics.tolerantDoubleEquality(eps)

  def simulateTank(tank: Tank, time: Float, deltaT: Float = 1.0f) = {
    var ret = tank
    var timeLeft = time
    breakable {
      while (true) {
        if (timeLeft>deltaT) {
          ret = ret.simulate(deltaT)
          timeLeft = timeLeft - deltaT
        }
        else {
          ret = ret.simulate(timeLeft)
          break
        }
      }
    }
    ret
  }

  "Tank" can "be created" in {
    val initTemp = 60.0f
    val tank = new Tank(100, 1, initTemp)
    tank.levelCount shouldBe 1
    tank.bottomLevel shouldBe 0
    tank.topTemperature shouldBe initTemp
    tank.botTemperature shouldBe initTemp
  }

  it should "be stable" in {
    val initTemp = 60.0f
    val deltaT = 0.1f

    var tank = new Tank(100, 10, initTemp)
    for (i <- 0 until 100) {
      tank = tank.simulate(deltaT)
    }
    tank.topTemperature shouldBe initTemp
    tank.botTemperature shouldBe initTemp
  }

  it should "be heated by a bottom permanent source" in {
    val initTemp = 30.0f
    val deltaT = 0.1f

    var tank = new Tank(100, 10, initTemp)
    val heating: HeatSource = _ => 1000
    tank = tank.addHeatSource(tank.bottomLevel, heating)
    tank = simulateTank(tank, 3600)
    tank.botTemperature should be > initTemp
    tank.topTemperature should be > initTemp
  }
}
