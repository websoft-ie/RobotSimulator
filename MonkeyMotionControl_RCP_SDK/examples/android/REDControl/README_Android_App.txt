-------------------------------------------------------------------------------
-- REDLINK Android App: System Requirements
-------------------------------------------------------------------------------
The following minimum requirements indicate the environment in which the 
application was tested. Other system specs may work but have not been
tested by RED.

1. Windows 7 64-bit

2. Java SE Development Kit

	a. http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html

	b. version 1.8.0_77 or greater

3. Android Studio 2.0 or greater

	a. Gradle 2.10 or greater (shipped with Studio)

	b. Install Android SDK along with Android Studio available at https://developer.android.com/studio/index.html

4. Launch SDK Manager from Android Studio (Tools->Android->SDK Manager) and install the following:

	a. Android 4.4 (KitKat), API Level 19 (this is the target version for this app)

	b. Android NDK

	c. Android SDK Platform-tools

	d. Android Support Library
	

-------------------------------------------------------------------------------
-- REDLINK Android App: Building the Application
-------------------------------------------------------------------------------

REDControl is the project and Android application name. This Android app is 
intended to provide the developer with the basic understanding of the RCP
SDK and its use in an Android app.


1. Install the required software as noted above
   
2. Define the following environment variables

	a. JAVA_HOME
		- Location of the Java Development Kit (JDK)
		- Example: JAVA_HOME=C:\Program Files\Java\jdk1.8.0_77
		
	b.	ANDROID_HOME
		- Location of the Android SDK
		- Example: ANDROID_HOME=C:\AndroidSDK
		
	c.	ANDROID_NDK_HOME
		- Location of the Android NDK, for native C/C++ builds
		- Example: ANDROID_NDK_HOME=%ANDROID_HOME%\ndk-bundle

	d.	ANDROID_PLATFORM
		- Location of the current target platform (this must match 'compileSdkVersion' in the app module's build.gradle)
		- Example: ANDROID_PLATFORM=%ANDROID_HOME%\platforms\android-19

	e.	RCP_SDK
		- Location of the RCP SDK
		- Example: RCP_SDK=C:\redlink_sdk_X.YY.ZZ\redlink_sdk

	f.	Add the following paths to your PATH environment variable
		- Gradle bin directory, example: C:\Program Files\Android Studio\gradle\gradle-2.10\bin
		- SDK platform tools, example: %ANDROID_HOME%\platform-tools

3. Build from the command line

	a. Open a command window and navigate to this directory.

	b. gradlew assembleDebug

	c. Output is in .\app\build\outputs\apk\app-debug.apk


4. Alternatively, build from Android Studio

	a. Launch Android Studio.
	
	b. From the home screen, select "Open an existing Android Studio project"
	
	c. Point to this directory (REDControl)
	
	d. Click on Run->Run.. and then select the 'app' configuration.

	e. This will build the .apk and then install and run it if you have a tablet connected on USB.
	
-------------------------------------------------------------------------------
-- REDLINK Android App: Gradle build scripts
-------------------------------------------------------------------------------

1. The file %RCP_SDK%\rcp_api\java\build_android.gradle contains tasks and dependencies that will build the RCP Java jar and native C/C++ libraries.

	a. A configuration called 'rcpBuild' is defined with build artifacts produced by the 'jar' task.

	b. The REDControl app's build.gradle file will contain a dependency to this build configuration (see below for more details).

	c. The 'jar' task compiles the classes in %RCP_SDK%\rcp_api\java\src and assembles into an rcp.jar file, which is included in the app's classpath.

	d. The 'jar' task is finalized by the 'rcpNativeInstall' task, which will launch ndk-build to build the native libraries
	   for all the ABI's supported by Android. It will then install it to REDControl\app\src\main\jniLibs.


2. How the RCP SDK is included in REDControl.

	a. In the root settings.gradle, we define an ':rcpSDK' module and include it, along with the 'app' module.

		- REDControl\settings.gradle:
			include ':app', ':rcpSDK'
			project(':rcpSDK').projectDir = new File("$System.env.RCP_SDK/rcp_api/java")
			project(':rcpSDK').buildFileName = 'build_android.gradle'

	b. Then in the app module's gradle build script, we add a dependency to the 'rcpBuild' configuration of the ':rcpSDK' module.

		REDControl\app\build.gradle:
			dependencies {
				...
				compile project (path: ':rcpSDK', configuration: 'rcpBuild')
			}

	c. If you want to include RCP SDK into your own app, you only need to add the above lines to your gradle scripts.
