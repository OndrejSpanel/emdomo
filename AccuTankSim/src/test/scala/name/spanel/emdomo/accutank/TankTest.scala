package name.spanel.emdomo.accutank

import org.scalactic.TolerantNumerics
import org.scalatest.{Matchers, FlatSpec}

class TankTest extends FlatSpec with Matchers {

  val eps = 0.0001f
  implicit val custom = TolerantNumerics.tolerantDoubleEquality(eps)

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
    tank = tank.addHeatSource(tank.bottomLevel, () => 1000)
    for (i <- 0 until 100) {
      tank = tank.simulate(deltaT)
    }
    tank.botTemperature should be > 30.0f
    //tank.botTemperature shouldBe 32.0f +- 1.0f
  }
}
