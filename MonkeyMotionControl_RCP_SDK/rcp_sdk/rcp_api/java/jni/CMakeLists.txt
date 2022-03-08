set(CMAKE_LEGACY_CYGWIN_WIN32 0)

cmake_minimum_required (VERSION 2.6)

set(RCP_CORE $ENV{RCP_SDK}/rcp_core)
set(RCP_API $ENV{RCP_SDK}/rcp_api)
set(JAVA_HOME $ENV{JAVA_HOME})

add_definitions(-DRCP_JAVA)

if (WIN32) 

	set(CMAKE_C_FLAGS " ")
	set(CMAKE_CXX_FLAGS " ")

else()

	set(CMAKE_C_FLAGS " ")
	set(CMAKE_CXX_FLAGS "-fpermissive ")

endif()


include_directories(${RCP_CORE}
					${RCP_API}
					${JAVA_HOME}/include
					${JAVA_HOME}/include/win32
					${JAVA_HOME}/include/linux
					.)

set(SRC_FILES 
	${RCP_CORE}/clist/clist.cpp
	${RCP_API}/rcp_api.c
	com_red_rcp_RCPCamera.cpp
	com_red_rcp_RCPDiscovery.cpp
	rcp_common.cpp
	rcp_camera.cpp
	rcp_discovery.cpp)
	
add_library(rcp SHARED ${SRC_FILES})
