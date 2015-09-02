package name.spanel.emdomo.accutank

import scala.swing._
import scala.swing.event.ButtonClicked
import scala.reflect.runtime.universe._

object AccuTankSim extends SimpleSwingApplication {

  class TankParameters {
    val wantedPower = 8000f
    val maxTemp = 75f
    val retTemp = 30f
    val initTemp = maxTemp

    val middlePower = 6000f
    val bottomPower = 7500f
    val tankVolume = 1000f
  }

  def simulateTank() = {
    val slots = 50
    val pars = new TankParameters

    import pars._

    var tank = new Tank(tankVolume, slots, initTemp)

    class HDOSwitch {
      var on = false
    }
    class HDOHeater(temp: Float, power: Float, val hdo: HDOSwitch) extends Heater(temp, power) {
      override def apply(temp: Float) = {
        if (hdo.on) super.apply(temp)
        else 0
      }
    }

    val hdo = new HDOSwitch
    val middleHeat = new HDOHeater(maxTemp, middlePower, hdo)
    val bottomHeat = new HDOHeater(maxTemp, bottomPower, hdo)
    tank = tank.addHeatSource(slots / 2, middleHeat)
    tank = tank.addHeatSource(tank.bottomLevel, bottomHeat)

    var tankConsume = new TankWithConsumption(tank, ConsumeTank(retTemp), () => wantedPower, () => println("Out of power"))


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

  val pars = new TankParameters

  def enumValues[T: TypeTag : reflect.ClassTag, R](cls: T, process: (String, Any) => R): Iterable[R] = {
    val rm = runtimeMirror(getClass.getClassLoader)
    val im = rm.reflect(cls)
    val r = typeOf[T].members.collect {
      case s: TermSymbol if s.isVal || s.isVar =>
        val name = s.name.toString
        val value = im.reflectField(s).get
        process(name, value)
    }
    r
  }

  override def top = new MainFrame {
    title = "My Frame"
    contents = new BoxPanel(Orientation.Vertical) {

      contents ++= enumValues(pars, { (fName, fValue) =>
        new FlowPanel(FlowPanel.Alignment.Left)() {
          contents += new Label(fName)
          contents += new TextField(fValue.toString, 4)
        }
      })

      contents += new Button {
        text = "Simulate!"
        reactions += {
          case ButtonClicked(_) => simulateTank()
        }
      }
    }
    size = new Dimension(300, 400)
  }
}
