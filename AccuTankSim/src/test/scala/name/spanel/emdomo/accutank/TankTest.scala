package name.spanel.emdomo.accutank

import org.scalactic.TolerantNumerics
import org.scalatest.{Matchers, FlatSpec}

class TankTest extends FlatSpec with Matchers {

  val eps = 0.0001f
  implicit val custom = TolerantNumerics.tolerantDoubleEquality(eps)

  "Tank" can "be created" in {
    val initTemp = 60.0f
    val tank = new Tank(1, initTemp)
    tank.levelCount shouldBe 1
    tank.topTemperature shouldBe initTemp
    tank.topTemperature shouldBe initTemp
  }

  it should "be stable" in {
    val initTemp = 60.0f
    var tank = new Tank(10, initTemp)
    val deltaT = 0.1f
    for (i <- 0 until 100) {
      tank = tank.simulate(deltaT)
    }
    tank.topTemperature shouldBe initTemp
    tank.botTemperature shouldBe initTemp
  }

  it should "be heated by a source" in {
    val initTemp = 30.0f
    val tank = new Tank(1, initTemp)

  }
}
