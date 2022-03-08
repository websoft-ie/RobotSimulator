# Install script for directory: D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/out/install/x64-Debug")
endif()
string(REGEX REPLACE "/$" "" CMAKE_INSTALL_PREFIX "${CMAKE_INSTALL_PREFIX}")

# Set the install configuration name.
if(NOT DEFINED CMAKE_INSTALL_CONFIG_NAME)
  if(BUILD_TYPE)
    string(REGEX REPLACE "^[^A-Za-z0-9_]+" ""
           CMAKE_INSTALL_CONFIG_NAME "${BUILD_TYPE}")
  else()
    set(CMAKE_INSTALL_CONFIG_NAME "Debug")
  endif()
  message(STATUS "Install configuration: \"${CMAKE_INSTALL_CONFIG_NAME}\"")
endif()

# Set the component getting installed.
if(NOT CMAKE_INSTALL_COMPONENT)
  if(COMPONENT)
    message(STATUS "Install component: \"${COMPONENT}\"")
    set(CMAKE_INSTALL_COMPONENT "${COMPONENT}")
  else()
    set(CMAKE_INSTALL_COMPONENT)
  endif()
endif()

# Is this installation the result of a crosscompile?
if(NOT DEFINED CMAKE_CROSSCOMPILING)
  set(CMAKE_CROSSCOMPILING "FALSE")
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xdevelopmentx" OR NOT CMAKE_INSTALL_COMPONENT)
  list(APPEND CMAKE_ABSOLUTE_DESTINATION_FILES
   "/rl-/rl/math/Array.h;/rl-/rl/math/Circular.h;/rl-/rl/math/CircularVector2.h;/rl-/rl/math/CircularVector3.h;/rl-/rl/math/Function.h;/rl-/rl/math/GnatNearestNeighbors.h;/rl-/rl/math/Kalman.h;/rl-/rl/math/KdtreeBoundingBoxNearestNeighbors.h;/rl-/rl/math/KdtreeNearestNeighbors.h;/rl-/rl/math/LinearNearestNeighbors.h;/rl-/rl/math/LowPass.h;/rl-/rl/math/Matrix.h;/rl-/rl/math/MatrixBaseAddons.h;/rl-/rl/math/NestedFunction.h;/rl-/rl/math/Pid.h;/rl-/rl/math/Polynomial.h;/rl-/rl/math/PolynomialQuaternion.h;/rl-/rl/math/Quaternion.h;/rl-/rl/math/QuaternionBaseAddons.h;/rl-/rl/math/Real.h;/rl-/rl/math/Rotation.h;/rl-/rl/math/Spatial.h;/rl-/rl/math/Spline.h;/rl-/rl/math/SplineQuaternion.h;/rl-/rl/math/Transform.h;/rl-/rl/math/TransformAddons.h;/rl-/rl/math/TrapezoidalVelocity.h;/rl-/rl/math/TypeTraits.h;/rl-/rl/math/Unit.h;/rl-/rl/math/Vector.h")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "/rl-/rl/math" TYPE FILE FILES
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Array.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Circular.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/CircularVector2.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/CircularVector3.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Function.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/GnatNearestNeighbors.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Kalman.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/KdtreeBoundingBoxNearestNeighbors.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/KdtreeNearestNeighbors.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/LinearNearestNeighbors.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/LowPass.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Matrix.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/MatrixBaseAddons.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/NestedFunction.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Pid.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Polynomial.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/PolynomialQuaternion.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Quaternion.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/QuaternionBaseAddons.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Real.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Rotation.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Spatial.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Spline.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/SplineQuaternion.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Transform.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/TransformAddons.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/TrapezoidalVelocity.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/TypeTraits.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Unit.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/Vector.h"
    )
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xdevelopmentx" OR NOT CMAKE_INSTALL_COMPONENT)
  list(APPEND CMAKE_ABSOLUTE_DESTINATION_FILES
   "/rl-/rl/math/metrics/L2.h;/rl-/rl/math/metrics/L2Squared.h")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "/rl-/rl/math/metrics" TYPE FILE FILES
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/metrics/L2.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/metrics/L2Squared.h"
    )
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xdevelopmentx" OR NOT CMAKE_INSTALL_COMPONENT)
  list(APPEND CMAKE_ABSOLUTE_DESTINATION_FILES
   "/rl-/rl/math/spatial/ArticulatedBodyInertia.h;/rl-/rl/math/spatial/ArticulatedBodyInertia.hxx;/rl-/rl/math/spatial/ForceVector.h;/rl-/rl/math/spatial/ForceVector.hxx;/rl-/rl/math/spatial/MotionVector.h;/rl-/rl/math/spatial/MotionVector.hxx;/rl-/rl/math/spatial/PlueckerTransform.h;/rl-/rl/math/spatial/PlueckerTransform.hxx;/rl-/rl/math/spatial/RigidBodyInertia.h;/rl-/rl/math/spatial/RigidBodyInertia.hxx")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "/rl-/rl/math/spatial" TYPE FILE FILES
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/ArticulatedBodyInertia.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/ArticulatedBodyInertia.hxx"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/ForceVector.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/ForceVector.hxx"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/MotionVector.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/MotionVector.hxx"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/PlueckerTransform.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/PlueckerTransform.hxx"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/RigidBodyInertia.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/spatial/RigidBodyInertia.hxx"
    )
endif()

if(CMAKE_INSTALL_COMPONENT)
  set(CMAKE_INSTALL_MANIFEST "install_manifest_${CMAKE_INSTALL_COMPONENT}.txt")
else()
  set(CMAKE_INSTALL_MANIFEST "install_manifest.txt")
endif()

string(REPLACE ";" "\n" CMAKE_INSTALL_MANIFEST_CONTENT
       "${CMAKE_INSTALL_MANIFEST_FILES}")
file(WRITE "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/math/out/build/x64-Debug/${CMAKE_INSTALL_MANIFEST}"
     "${CMAKE_INSTALL_MANIFEST_CONTENT}")
