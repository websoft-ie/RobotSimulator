# Install script for directory: D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/out/install/x64-Debug")
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
   "/rl-/rl/std/chrono;/rl-/rl/std/condition_variable;/rl-/rl/std/future;/rl-/rl/std/iterator;/rl-/rl/std/mutex;/rl-/rl/std/random;/rl-/rl/std/regex;/rl-/rl/std/string;/rl-/rl/std/thread")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "/rl-/rl/std" TYPE FILE FILES
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/chrono"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/condition_variable"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/future"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/iterator"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/mutex"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/random"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/regex"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/string"
    "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/thread"
    )
endif()

if(CMAKE_INSTALL_COMPONENT)
  set(CMAKE_INSTALL_MANIFEST "install_manifest_${CMAKE_INSTALL_COMPONENT}.txt")
else()
  set(CMAKE_INSTALL_MANIFEST "install_manifest.txt")
endif()

string(REPLACE ";" "\n" CMAKE_INSTALL_MANIFEST_CONTENT
       "${CMAKE_INSTALL_MANIFEST_FILES}")
file(WRITE "D:/MonkeyMotionV0.98.979_RenewTimeline/MonkeyMotionControl/lib/DHKinematics/include/RoboticsLibrary/rl/std/out/build/x64-Debug/${CMAKE_INSTALL_MANIFEST}"
     "${CMAKE_INSTALL_MANIFEST_CONTENT}")
