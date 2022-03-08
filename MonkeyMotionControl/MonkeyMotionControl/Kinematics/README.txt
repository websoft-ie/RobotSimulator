Inverse Kinematics Quaternion Main Functions

Initialize InverseKinematics in the following way:
    InverseKinematics [VARIABLE NAME] = new InverseKinematics([BOOL])
    (true for Degrees, false for Radians)

Functions:
    Position coordinates and Euler Angle input:
        Calculate(x, y, z, Rx, Ry, Rz)
    Rotation Quaternion and Position Quaternion input:
        Calculate(q1, q2)
    DualQuaternion input:
        Calculate(Q)