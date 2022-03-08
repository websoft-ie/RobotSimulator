REM This file is part of the RCP SDK Release 6.62.0
REM Copyright (C) 2009-2018 RED.COM, LLC.  All Rights Reserved.
REM
REM For technical support please email rcpsdk@red.com.
REM
REM "Source Code" means the accompanying software in any form preferred for making
REM modifications. "Source Code" does not include the accompanying strlcat.c and
REM strlcpy.c software and examples/qt/common/at/serial software.
REM 
REM "Binary Code" means machine-readable Source Code in binary form.
REM 
REM "Approved Recipients" means only those recipients of the Source Code who have
REM entered into the RCP SDK License Agreement with RED.COM, LLC. All
REM other recipients are not authorized to possess, modify, use, or distribute the
REM Source Code.
REM
REM RED.COM, LLC. hereby grants Approved Recipients the rights to modify this
REM Source Code, create derivative works based on this Source Code, and distribute
REM the modified/derivative works only as Binary Code in its binary form. Approved
REM Recipients may not distribute the Source Code or any modification or derivative
REM work of the Source Code. Redistributions of Binary Code must reproduce this
REM copyright notice, this list of conditions, and the following disclaimer in the
REM documentation or other materials provided with the distribution. RED.COM, LLC
REM may not be used to endorse or promote Binary Code redistributions without
REM specific prior written consent from RED.COM, LLC. 
REM
REM The only exception to the above licensing requirements is any recipient may use,
REM copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
REM in accordance with the provisions required by the license associated therewith;
REM provided, however, that the modifications are solely to the strlcat.c and
REM strlcpy.c software and do not include any other portion of the Source Code.
REM 
REM THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
REM EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
REM MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
REM IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
REM DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
REM IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
REM SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

@if "%DEBUG%" == "" @echo off
@rem ##########################################################################
@rem
@rem  Gradle startup script for Windows
@rem
@rem ##########################################################################

@rem Set local scope for the variables with windows NT shell
if "%OS%"=="Windows_NT" setlocal

@rem Add default JVM options here. You can also use JAVA_OPTS and GRADLE_OPTS to pass JVM options to this script.
set DEFAULT_JVM_OPTS=

set DIRNAME=%~dp0
if "%DIRNAME%" == "" set DIRNAME=.
set APP_BASE_NAME=%~n0
set APP_HOME=%DIRNAME%

@rem Find java.exe
if defined JAVA_HOME goto findJavaFromJavaHome

set JAVA_EXE=java.exe
%JAVA_EXE% -version >NUL 2>&1
if "%ERRORLEVEL%" == "0" goto init

echo.
echo ERROR: JAVA_HOME is not set and no 'java' command could be found in your PATH.
echo.
echo Please set the JAVA_HOME variable in your environment to match the
echo location of your Java installation.

goto fail

:findJavaFromJavaHome
set JAVA_HOME=%JAVA_HOME:"=%
set JAVA_EXE=%JAVA_HOME%/bin/java.exe

if exist "%JAVA_EXE%" goto init

echo.
echo ERROR: JAVA_HOME is set to an invalid directory: %JAVA_HOME%
echo.
echo Please set the JAVA_HOME variable in your environment to match the
echo location of your Java installation.

goto fail

:init
@rem Get command-line arguments, handling Windowz variants

if not "%OS%" == "Windows_NT" goto win9xME_args
if "%@eval[2+2]" == "4" goto 4NT_args

:win9xME_args
@rem Slurp the command line arguments.
set CMD_LINE_ARGS=
set _SKIP=2

:win9xME_args_slurp
if "x%~1" == "x" goto execute

set CMD_LINE_ARGS=%*
goto execute

:4NT_args
@rem Get arguments from the 4NT Shell from JP Software
set CMD_LINE_ARGS=%$

:execute
@rem Setup the command line

set CLASSPATH=%APP_HOME%\gradle\wrapper\gradle-wrapper.jar

@rem Execute Gradle
"%JAVA_EXE%" %DEFAULT_JVM_OPTS% %JAVA_OPTS% %GRADLE_OPTS% "-Dorg.gradle.appname=%APP_BASE_NAME%" -classpath "%CLASSPATH%" org.gradle.wrapper.GradleWrapperMain %CMD_LINE_ARGS%

:end
@rem End local scope for the variables with windows NT shell
if "%ERRORLEVEL%"=="0" goto mainEnd

:fail
rem Set variable GRADLE_EXIT_CONSOLE if you need the _script_ return code instead of
rem the _cmd.exe /c_ return code!
if  not "" == "%GRADLE_EXIT_CONSOLE%" exit 1
exit /b 1

:mainEnd
if "%OS%"=="Windows_NT" endlocal

:omega
