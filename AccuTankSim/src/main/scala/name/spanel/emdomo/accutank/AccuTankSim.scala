package name.spanel.emdomo.accutank

import java.awt.Dimension
import javax.swing.UIManager

import scala.swing._
import scala.swing.event.{EditDone, ButtonClicked}
import scala.reflect.runtime.universe._
import scala.language.implicitConversions

object AccuTankSim extends SimpleSwingApplication {
  implicit def pair2Dimension(pair: (Int, Int)): Dimension = new Dimension(pair._1, pair._2)

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

  val pars = new TankParameters

  private lazy val tankPanel = new TankPanel {
    minimumSize = (80, 150)
    preferredSize = (120, 300)
  }

  class HDOSwitch {
    var on = false
  }

  def fromParameters(pars: TankParameters, hdo: HDOSwitch) = {
    import pars._

    var tank = new Tank(tankVolume, slots, initTemp)

    class HDOHeater(temp: Float, power: Float, val hdo: HDOSwitch) extends Heater(temp, power) {
      override def apply(temp: Float) = {
        if (hdo.on) super.apply(temp)
        else 0
      }
    }

    val middleHeat = new HDOHeater(maxTemp, middlePower, hdo)
    val bottomHeat = new HDOHeater(maxTemp, bottomPower, hdo)
    tank = tank.addHeatSource(slots / 2, middleHeat)
    tank = tank.addHeatSource(tank.bottomLevel, bottomHeat)

    new TankWithConsumption(tank, ConsumeTank(retTemp), () => wantedPower, () => println("Out of power"))
  }

  class TankSimulator(pars: TankParameters) {
    val hdoSwitch = new HDOSwitch
    var tankConsume = fromParameters(pars, hdoSwitch)

    val step = 60f // a minute step is enough

    def hdo = hdoSwitch.on
    def hdo_=(on: Boolean) = hdoSwitch.on = on

    def simulateStep(time: Float): Unit = {
      tankConsume = tankConsume.simulateLongTime(time, step)
    }
  }

  val hdoTimes = List((0, 9), (13, 20))

  def checkHDO(hour: Float) = {
    hdoTimes.exists(r => r._1 <= hour && r._2 >= hour)
  }

  def simulateTank(pars: TankParameters) = {
    val simulator = new TankSimulator(pars)
    val hour = 3600f
    for (i <- 0 until 24) {
      simulator.hdo = checkHDO(i)
      simulator.simulateStep(hour)
      tankPanel.tank = simulator.tankConsume.tank
    }
  }


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
    title = "Accumulation Tank Simulator"
    contents = new ScrollPane() {
      contents = new BorderPanel {

        implicit class placeIntoLayout(c: BorderPanel#Constraints) {
          def @>(comp: Component) = comp -> c
        }

        import BorderPanel.Position._

        layout += Center @> new BoxPanel(Orientation.Vertical) {

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

        }

        layout += East @> tankPanel
        layout += South @> new Button {
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
