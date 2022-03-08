using MonkeyMotionControl.Simulator;
using System;

namespace MonkeyMotionControl.UI
{

    public class StreamTableRowEventArgs : EventArgs
    {
        public int RowID { get; set; }
        public double[] JointValues { get; set; }
        public double Vel { get; set; }
        public double Acc { get; set; }
        public double Dec { get; set; }
        public double Leave { get; set; }
        public double Reach { get; set; }
        public double DistToTarget { get; set; }

        public StreamTableRowEventArgs(int id)
        {
            this.RowID = id;
        }

        public StreamTableRowEventArgs(double[] jointVals, double vel, double acc, double dec, double leave, double reach, double distToTarget)
        {
            this.JointValues = jointVals;
            this.Vel = vel;
            this.Acc = acc;
            this.Dec = dec;
            this.Leave = leave;
            this.Reach = reach;
            this.DistToTarget = distToTarget;
        }
    }

    public class CommandsTableRowEventArgs : EventArgs
    {
        public CommandsMoveType MoveType { get; set; }
        public CartesianPos FirstPoint { get; set; }
        public CartesianPos MidPoint { get; set; }
        public CartesianPos LastPoint { get; set; }
        public double Velocity { get; set; } = 100;
        public double Acceleration { get; set; } = 100;
        public double Deceleration { get; set; } = 100;
        public double Focus { get; set; } = 0;
        public double Iris { get; set; } = 0;
        public double Zoom { get; set; } = 0;
        public double AuxMotor { get; set; } = 0;
        public double MidFocus { get; set; } = 0;
        public double MidIris { get; set; } = 0;
        public double MidZoom { get; set; } = 0;
        public double MidAuxMotor { get; set; } = 0;

        public CommandsTableRowEventArgs(CommandsMoveType curveType, CartesianPos firstPt, CartesianPos lastPt, CartesianPos midPt, double vel, double acc, double dec, double focusPos, double irisPos, double zoomPos, double auxMotPos)
        {
            this.MoveType = curveType;
            this.FirstPoint = firstPt;
            this.LastPoint = lastPt;
            this.MidPoint = midPt;
            this.Velocity = vel;
            this.Acceleration = acc;
            this.Deceleration = dec;
            this.Focus = focusPos;
            this.Iris = irisPos;
            this.Zoom = zoomPos;
            this.AuxMotor = auxMotPos;
        }

        public CommandsTableRowEventArgs(CommandsMoveType curveType, CartesianPos firstPt, CartesianPos lastPt, CartesianPos midPt, double vel, double acc, double dec, double focusPos, double irisPos, double zoomPos, double auxMotPos, double focusMidPos, double irisMidPos, double zoomMidPos, double auxMotMidPos)
        {
            this.MoveType = curveType;
            this.FirstPoint = firstPt;
            this.LastPoint = lastPt;
            this.MidPoint = midPt;
            this.Velocity = vel;
            this.Acceleration = acc;
            this.Deceleration = dec;
            this.Focus = focusPos;
            this.Iris = irisPos;
            this.Zoom = zoomPos;
            this.AuxMotor = auxMotPos;
            this.MidFocus = focusMidPos;
            this.MidIris = irisMidPos;
            this.MidZoom = zoomMidPos;
            this.MidAuxMotor = auxMotMidPos;
        }
    }

}
