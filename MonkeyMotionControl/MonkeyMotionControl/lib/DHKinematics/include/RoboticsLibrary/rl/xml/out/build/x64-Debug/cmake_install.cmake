# Install script for directory: D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/out/install/x64-Debug")
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
   "/rl-/rl/xml/Attribute.h;/rl-/rl/xml/Document.h;/rl-/rl/xml/DomParser.h;/rl-/rl/xml/Exception.h;/rl-/rl/xml/Node.h;/rl-/rl/xml/NodeSet.h;/rl-/rl/xml/Namespace.h;/rl-/rl/xml/Object.h;/rl-/rl/xml/Path.h;/rl-/rl/xml/Schema.h;/rl-/rl/xml/Stylesheet.h")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "/rl-/rl/xml" TYPE FILE FILES
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Attribute.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Document.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/DomParser.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Exception.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Node.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/NodeSet.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Namespace.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Object.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Path.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Schema.h"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/Stylesheet.h"
    )
endif()

if(CMAKE_INSTALL_COMPONENT)
  set(CMAKE_INSTALL_MANIFEST "install_manifest_${CMAKE_INSTALL_COMPONENT}.txt")
else()
  set(CMAKE_INSTALL_MANIFEST "install_manifest.txt")
endif()

string(REPLACE ";" "\n" CMAKE_INSTALL_MANIFEST_CONTENT
       "${CMAKE_INSTALL_MANIFEST_FILES}")
file(WRITE "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/xml/out/build/x64-Debug/${CMAKE_INSTALL_MANIFEST}"
     "${CMAKE_INSTALL_MANIFEST_CONTENT}")
