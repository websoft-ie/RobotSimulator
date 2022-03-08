/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 * Copyright (C) 2009-2018 RED.COM, LLC.  All rights reserved.
 *
 * For technical support please email rcpsdk@red.com.
 *
 * "Source Code" means the accompanying software in any form preferred for making
 * modifications. "Source Code" does not include the accompanying strlcat.c and
 * strlcpy.c software and examples/qt/common/qt/serial software.
 * 
 * "Binary Code" means machine-readable Source Code in binary form.
 * 
 * "Approved Recipients" means only those recipients of the Source Code who have
 * entered into the RCP SDK License Agreement with RED.COM, LLC. All
 * other recipients are not authorized to possess, modify, use, or distribute the
 * Source Code.
 *
 * RED.COM, LLC hereby grants Approved Recipients the rights to modify this
 * Source Code, create derivative works based on this Source Code, and distribute
 * the modified/derivative works only as Binary Code in its binary form. Approved
 * Recipients may not distribute the Source Code or any modification or derivative
 * work of the Source Code. Redistributions of Binary Code must reproduce this
 * copyright notice, this list of conditions, and the following disclaimer in the
 * documentation or other materials provided with the distribution. RED.COM, LLC
 * may not be used to endorse or promote Binary Code redistributions without
 * specific prior written consent from RED.COM, LLC. 
 *
 * The only exception to the above licensing requirements is any recipient may use,
 * copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
 * in accordance with the provisions required by the license associated therewith;
 * provided, however, that the modifications are solely to the strlcat.c and
 * strlcpy.c software and do not include any other portion of the Source Code.
 * 
 * THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
 * IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
 * DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
 * IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 ********************************************************************************/

#ifndef _RCP_CAMERA_H
#define _RCP_CAMERA_H 

#include <jni.h>

int rcpCamera_initLibrary();
int rcpCamera_open					(JNIEnv *pEnv, jobject pThis, char const *id, char const *name, char const *ver, char const *user);
int rcpCamera_close					(JNIEnv *pEnv, jobject pThis, char const *id);
int rcpCamera_processData			(char const *id, char *data, int len);
int rcpCamera_getList				(char const *id, int paramId);
int rcpCamera_setList				(char const *id, int paramId, const char * listStrings, const char *listValues);
int rcpCamera_getParameterID		(char const *id, char *name);
int rcpCamera_getParameterName		(char const *id, int paramId, const char **paramName);
int rcpCamera_getValue				(char const *id, int paramId);
int rcpCamera_getParameterLabel		(char const *id, char *name, const char **label);
int rcpCamera_setStringValue		(char const *id, int paramId, char const *value);
int rcpCamera_setValue				(char const *id, int paramId, int value);
int rcpCamera_getIsSupported		(char const *id, int paramId);
int rcpCamera_getPropertyIsSupported(char const *id, int paramId, int propertyType);
int rcpCamera_setUnsignedValue		(char const *id, int paramId, unsigned int value);
int rcpCamera_send					(char const *id, int paramId);
int rcpCamera_getAPIVersion			(const char **version);
int rcpCamera_menuIsSupported		(char const *id);
int rcpCamera_menuGetChildren		(char const *id, int menuNodeId);
int rcpCamera_menuNodeStatusIsSupported	(char const *id);
int rcpCamera_menuGetNodeStatus		(char const *id, int menuNodeId);
int rcpCamera_notificationGet		(char const *id);
int rcpCamera_notificationTimeout	(char const *id, char const *notificationId);
int rcpCamera_notificationResponse	(char const *id, char const *notificationId, int response);
int rcpCamera_getCameraConnectionStats	(char const *id, int statsType);
int rcpCamera_getStatus				(char const *id, int paramId);
int rcpCamera_periodicEnable        (char const *id, int paramId, int enable);

int rcpCamera_rftpGetIsSupported	(char const *id);
int rcpCamera_rftpSendFile			(char const *id, char const *local_file, char const *cam_file, int compress_file, rcp_uuid_t *uuid);
int rcpCamera_rftpRetrieveFile		(char const *id, char const *local_file, char const *cam_file, int maxFileSize, int compressionAllowed, rcp_uuid_t *uuid);
int rcpCamera_rftpDeleteFile		(char const *id, char const *cam_path, rcp_uuid_t *uuid);
int rcpCamera_rftpAbortTransfer		(char const *id, rcp_uuid_t *uuid);
int rcpCamera_rftpDirList			(char const *id, char const *cam_path, rcp_uuid_t *uuid);
#endif // _RCP_CAMERA_H
