namespace MonkeyMotionControl.Simulator
{
    public class JointDH
    {
        public double angle = 0;
        public double angleMin = -180;
        public double angleMax = 180;
        public int rotPointX = 0;
        public int rotPointY = 0;
        public int rotPointZ = 0;
        public int rotAxisX = 0;
        public int rotAxisY = 0;
        public int rotAxisZ = 0;

        public JointDH()
        {
            angle = 0;
            angleMax = 180;
            angleMin = -180;
            rotPointX = 0;
            rotPointY = 0;
            rotPointZ = 0;
            rotAxisX = 0;
            rotAxisY = 0;
            rotAxisZ = 0;
        }
    }
}
