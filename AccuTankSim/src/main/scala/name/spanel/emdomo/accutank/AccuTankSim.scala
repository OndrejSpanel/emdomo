package name.spanel.emdomo.accutank

import javax.swing.UIManager

import scala.swing._
import scala.swing.event.{EditDone, ButtonClicked}
import scala.reflect.runtime.universe._

object AccuTankSim extends SimpleSwingApplication {

  class TankParameters {
    val slots = 50

    var tankVolume = 1000f

    var maxTemp = 75f
    var retTemp = 30f
    var initTemp = maxTemp

    var wantedPower = 8000f
    var middlePower = 6000f
    var bottomPower = 7500f
  }

  def simulateTank(pars: TankParameters) = {
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

  def enumValues[T: TypeTag : reflect.ClassTag, R](cls: T, process: (InstanceMirror, TermSymbol) => R): Iterable[R] = {
    val rm = runtimeMirror(getClass.getClassLoader)
    val im = rm.reflect(cls)
    val r = typeOf[T].members.sorted.collect {
      case s: TermSymbol if s.isVar =>
        process(im, s)
    }
    r
  }

  override def top = new MainFrame {
    title = "My Frame"
    contents = new ScrollPane() {
      contents = new BoxPanel(Orientation.Vertical) {

        contents ++= enumValues(pars, { (inst, sym) =>
          new FlowPanel(FlowPanel.Alignment.Left)() {
            val fName = sym.name.toString
            val fValue = inst.reflectField(sym).get
            contents += new Label(fName)
            val field = new TextField(fValue.toString, 7)
            contents += field
            listenTo(field)
            field.reactions += {
              case EditDone(x) =>
                // TODO: support other field types as well
                inst.reflectField(sym).set(x.text.toFloat)
            }
          }
        })

        contents += new Button {
          text = "Simulate!"
          reactions += {
            case ButtonClicked(_) => simulateTank(pars)
          }
        }
      }
    }
    size = new Dimension(300, 400)
  }
  override def startup(args: Array[String]) = {
    UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName)
    super.startup(args)
  }
}
