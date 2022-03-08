using HelixToolkit.Wpf.SharpDX.Model.Scene;

namespace MonkeyMotionControl.Simulator
{
    public class RobotModel
    {
        public JointDH joint { get; set; }
        public SceneNode model { get; set; }

        public RobotModel()
        {
            joint = null;
            model = null;
        }

        public RobotModel(JointDH jointDHparams, SceneNode pModel, string name)
        {
            model = pModel;
            model.Name = name;
            joint = jointDHparams;
        }
    }
}
