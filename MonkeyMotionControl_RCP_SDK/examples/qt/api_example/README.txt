=================
BUILDING THE CODE
=================
In order to build and run the source code, you must first download Qt.
http://qt-project.org/downloads

The code was built using Qt 4.7.3 and developed in the Qt Creator IDE.

Once the Qt libraries and Qt Creator are installed, open src/rcpdemo.pro. Here you can specify build folders for debug and release.

The following environment variable(s) will modify the build settings:
  - RCP_SDK - location of RCP SDK (if not defined, the version included at the top level redlink_sdk directory in this package will be used.)

Build the application.

After building for the first time on Mac:
    -You must copy examples/qt/common/qt/serial/lib/libQtSerialPort.1.0.0.dylib [your build directory]/RCPController.app/Contents/Frameworks
        *Note: If the Frameworks directory does not exist, you must create it.

This build step only needs to be performed once after the initial build or after deleting your local build folder.
You can automate this step by adding a Custom Process Step in the Build Steps section of the Projects window.

At this point, the code should be ready to run.


=====================
PREPARING YOUR CAMERA
=====================
1) Enable external control of your camera:
	-Navigate to Menu > Settings > Setup > Communication
	-Select the Network tab and check the boxes for "Enable DHCP" and "Enable External Control"

2) Set the Serial Protocol:
	-Navigate to Menu > Settings > Setup > Communication
	-Select the Serial tab and set the Serial Protocol to "REDLINK™ Command Protocol"


==========================
INSTALLING THE APPLICATION
==========================
If you want to run the RCP Controller without building the code, you can use the prebuilt binaries:
	-On Windows, run RCPController.exe and follow the installation instructions, then launch the application once the installation has finished
	-On Mac, open RCPController.dmg and drag the app into the Applications shortcut folder, then launch the application once the files have been copied over

	
=====================
USING THE APPLICATION
=====================
Once the application has been launched, use the Connection panel at the top to connect to your camera
	-If you select "GigE Ethernet (TCP)," a list of RED cameras on your network will be displayed. Double click your camera to connect to it.
	-If you select "Serial Port," a list of available serial ports will be displayed. Double click the serial port that corresponds to your camera.
	-Optionally, you can manually enter your camera's IP address or the serial port name and then click "Connect."
	
Once connected, you can issue commands to your camera using the Controller panel:
	-Press the RECORD button to toggle recording
	-Choose entries from the combo boxes to update their corresponding camera values
	-Slide any of the sliders or manually type values into their text fields to update their corresponding camera values
	-Enter text into the text field to update the camera value
