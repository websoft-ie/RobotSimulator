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

rem @echo off
setlocal

set SCRIPT=%0
set DQUOTE="
@echo %SCRIPT:~0,1% | findstr /l %DQUOTE% > NUL
if %ERRORLEVEL% EQU 0 set PAUSE_ON_CLOSE=1

cd %~dp0

set PATH=C:\cygwin\bin;C:\cygwin64\bin;%PATH%
bash.exe build_doc.sh

if defined PAUSE_ON_CLOSE pause

exit /B 0
endlocal
