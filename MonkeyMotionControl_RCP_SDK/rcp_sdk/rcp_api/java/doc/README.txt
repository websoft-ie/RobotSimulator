--------------------------------------------------------------------------------
-- RCP SDK Java Wrapper: Overview
--------------------------------------------------------------------------------

The Java wrapper is intended to be used by programmers to interface their
Java code with a RED DSMC camera for status and control. This wrapper utilizes
the RCP C/C++ SDK for command and response parsing. The wrapper supports 
Java desktop applications (Windows & Linux) as well as Android applications.
Please review the RCP C/C++ SDK documentation for an overview on how
RCP operates.

The Java wrapper wraps the RCP C/C++ SDK with Java code utilizing the Java 
Native Interface (JNI). 

--------------------------------------------------------------------------------
-- RCP SDK Java Wrapper: Contents
--------------------------------------------------------------------------------

The wrapper consists of two Java packages located in the SRC folder, 
com.red.common & com.red.rcp, the JNI C/C++ code located in the JNI folder and a 
Java command line test application located in the TEST directory. A separate 
Android demo application is also available from RED. 

The com.red.common package contains the following source files:

1. HandleWrapper.java
2. Android_HandleWrapper.java
3. Java_HandleWrapper.java

The Android and Java files contain implementations that are specific to Android
and Java but they both provide a uniform interface for higher level software.
The only file that is used at compile time, is HandleWrapper.java. The build 
scripts determine which file should be used an copies it to HandleWrapper.java.

The HandleWrapper class provides a mechanism to send and process messages
and Runnable objects associated with a thread's MessageQueue, in a thread safe
manner. It is used inside other SDK classes and is not used by the programmer 
at the API level.

The com.red.rcp package contains the RCP API classes and contains the 
following source files:

1. RCPDiscoveryListener.java
2. RCPDiscovery.java
3. RCPCameraListener.java
4. RCPCamera.java

The RCPDiscoveryListener is an interface that is implemented by the RCPDiscovery
class. It defines the callback interface that will be called when a camera has
been discovered on the network. This interface must also be implemented by any
classes that instantiate a RCPDiscovery object, as the callback methods are 
delegated to the parent class.

The RCPDiscovery class provides the ability to discover RED cameras on a Local
Area Network (LAN) or via WiFi and provides the programmer with the necessary
camera information to connect to the camera.

The RCPCameraListener is an interface that is implemented by the RCPCamera 
class. It defines the callback interface that will be called when a camera has
sent data to the application. This interface must also be implemented by any
classes that instantiate a RCPCamera object, as the callback methods are 
delegated to the parent class.

The RCPCamera class provides the ability connect to a camera, receive status
from the camera and set parameters on the camera.

The JNI folder contains the necessary C/C++ wrapper code to interface with the 
RCP C/C++ SDK through the Java Native Interface (JNI). Ultimately this code
is compiled to a shared library, librcp.so on Android and Linux and rcp.dll 
on Windows. The JVM will load this shared library when either the RCPDiscovery 
class or the RCPCamera class is loaded.

The TEST folder contains a Java desktop command line test application. The files
are:

1. rcpTest.java
2. RcpTester.java

The rcpTest class implements the 'main' function and is invoked via the JVM at 
the command line. It provides the entry point for the program and starts a 
thread for the instantiated RCPTester class.

The RCPTester class implements the RCPDiscoveryListener and RCPCameraListener
interface and instantiates a RCPDiscovery object and a RCPCamera object. The
class discovers cameras on the network and allows the user to select a 
discovered camera or enter the IP address manually. Then the class performs
the following:

1. Connects to the camera
2. Displays camera status for 10 seconds including time code, temperature, etc.
3. At T=10 sec, it sets the SATURATION to 1.2
4. Displays camera status for another 10 seconds
5. Closes the connection and exits

--------------------------------------------------------------------------------
-- RCP SDK Java Wrapper: Building for Java Desktop
--------------------------------------------------------------------------------

1.	Install the required software tools.

	a.	Apache Ant - http://ant.apache.org/
	
	b.	CMake - http://cmake.org/
	
	c.	Java Development Kit (JDK) - http://www.oracle.com/technetwork/java/javase/downloads/index.html
	
		i.	If on windows and using MS Visual Studio, install a 32-bit JDK. 
		   CMake cannot recognize the 64-bit MS Visual Studio compilers
		   
	d.	Appropriate C/C++ compiler
	
		i.	GNU GCC for Linux or Cygwin (OR)
		
		ii.	Microsoft Visual Studio Express 2013 or equivalent for windows
		
2.	Add the Ant bin directory to your PATH. 

3.	Add the CMake bin directory to your PATH.

4.	Add the JDK bin directory to your PATH.

5.	Defined the required environment variables for a Java Desktop build

	a.	JAVA_HOME (e.g. JAVA_HOME=/opt/jdk1.6.0_45)
	
	b.	RCP_SDK (e.g. RCP_SDK=/disk1/rcp/rcp_sdk_X.YYY/rcp_sdk)
	
6.	Navigate to the location of the RCP SDK Java bindings via the command line

	a.	Example: >> cd ${RCP_SDK}/rcp_api/java
	
7.	Build the software using Apache Ant

	a.	Example: >> ant build
	
8.	Install the software using Apache Ant

	a.	Example: >> ant install
	
9.	Run the rcpTest program from the command line, output will be to the console

	a.	Example: >> java  -Djava.library.path=./bin -cp ./bin rcpTest
	b. NOTE: If running on 64-bit Windows with Microsoft Visual Studio compilers,
	   CMake cannot recognize the 64-bit compilers. Therefore you must use a
	   32-bit JDK. If you have a 32-bit JDK installed, you can do this from the
	   command line. The following is an example:
	   
	   >> C:/"Program Files (x86)"/Java/jdk1.8.0/bin/java  -Djava.library.path=./bin -cp ./bin rcpTest

--------------------------------------------------------------------------------
-- RCP SDK Java Wrapper: Building for Android
--------------------------------------------------------------------------------

The RCP SDK is shipped with a sample Android application called REDControl. This
app will provide a basic understanding of how to use the Java wrapper and 
RCP SDK in the Android environment. 

1. See the README_Android_App.txt file contained in the gradle project
   directory for REDControl. It will detail the build instructions.

--------------------------------------------------------------------------------
-- RCP SDK Java Wrapper: FAQ
--------------------------------------------------------------------------------

1. Why do I get the following error when I run the Java desktop demo app on 
   Windows?
   
   c:\babs\red\projects\Cyclone\vmshare\rcp\rcp_sdk_5.0.1\rcp_sdk\rcp_api\java>java  -Djava.library.path=./bin -cp ./bin rcpTest
Exception in thread "Thread-0" java.lang.UnsatisfiedLinkError: C:\babs\red\projects\Cyclone\vmshare\rcp\rcp_sdk_5.0.1\rcp_sdk\rcp_api\java\bin\rcp.dll: Can't load IA 32-bit .dll on a AMD 64-bit platform
        at java.lang.ClassLoader$NativeLibrary.load(Native Method)
        at java.lang.ClassLoader.loadLibrary0(Unknown Source)
        at java.lang.ClassLoader.loadLibrary(Unknown Source)
        at java.lang.Runtime.loadLibrary0(Unknown Source)
        at java.lang.System.loadLibrary(Unknown Source)
        at com.red.rcp.RCPDiscovery.<clinit>(Unknown Source)
        at RcpTester.run(Unknown Source)
        at java.lang.Thread.run(Unknown Source)


ANSWER: Since CMake cannot find the 64-bit MSVC compiler, it uses the 32-bit compiler
to generate the rcp.dll. Therefore this DLL is a 32-bit DLL. You must use a 32-bit JDK or JRE
to run the example. The Java wrapper can still be compiled as a 64-bit DLL in a
user defined MSVC project. This exercise is left up to the user.

The following is an example of invoking a 32-bit JRE on 64-bit Windows:

>>  C:/"Program Files (x86)"/Java/jdk1.8.0/bin/java  -Djava.library.path=./bin -cp ./bin rcpTest

Choose Connection:
  1. EPIC:102-4C6-95F:EPIC-M:For Development Use Only!!:10.2.67.14
  2. Enter manual IP Address

Enter Choice >> 1
RCP Java Test: open successful
RCP Java Test:  onUpdateString(): CameraId = 0x24c695f, state = 1
C/C++ RCP SDK: RCP_PARAM_RECORD_STATE: Not Recording

RCP Java Test:  onUpdate(): CameraId = 0x24c695f, param = RCP_PARAM_RECORD_STATE;RECORD;NONE;NONE
RCP Java Test:  onUpdateString(): CameraId = 0x24c695f, param = RCP_PARAM_TIMECODE;00:00:00:--;00:00:00;NONE
RCP Java Test:  onUpdate(): CameraId = 0x24c695f, param = RCP_PARAM_FAN_SPEED_FRONT;79 %;79;NONE
RCP Java Test:  onUpdate(): CameraId = 0x24c695f, param = RCP_PARAM_FAN_SPEED_TOP;---;---;NONE
RCP Java Test:  onUpdate(): CameraId = 0x24c695f, param = RCP_PARAM_CORE_TEMP;55 degC;55;NONE
RCP Java Test:  onUpdate(): CameraId = 0x24c695f, param = RCP_PARAM_SENSOR_TEMP;34 degC;34;NONE
...


