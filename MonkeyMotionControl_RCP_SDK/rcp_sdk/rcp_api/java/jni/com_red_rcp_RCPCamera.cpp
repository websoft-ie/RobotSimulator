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

#include <stdio.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <signal.h>
#include <map>
#include "com_red_rcp_RCPCamera.h"
#include <jni.h>
#include "rcp_api.h"
#include "rcp_camera.h"
#include "alog.h"

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1init
  (JNIEnv *, jobject)
{
	rcpCamera_initLibrary();
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1open
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring client_name, jstring client_version, jstring client_user)
{
	int ret = -1;
	if (id != NULL) {
		char const *name = NULL;
		char const *ver  = NULL;
		char const *user = NULL;

		char const *pin  = pEnv->GetStringUTFChars(id, 0);
		if (client_name != NULL) name = pEnv->GetStringUTFChars(client_name, 0);
		if (client_version != NULL) ver  = pEnv->GetStringUTFChars(client_version, 0);
		if (client_user != NULL) user = pEnv->GetStringUTFChars(client_user, 0);

		if (pin != NULL) {
			ret = rcpCamera_open(pEnv, pThis, pin, name, ver, user);
		}
    	
        if (pin)  pEnv->ReleaseStringUTFChars(id, pin);
        if (name) pEnv->ReleaseStringUTFChars(client_name, name);
        if (ver)  pEnv->ReleaseStringUTFChars(client_version, ver);
        if (user) pEnv->ReleaseStringUTFChars(client_user, user);
	}
	return ret;
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1close
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	if (id != NULL) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin != NULL) {
			rcpCamera_close(pEnv, pThis, pin);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1processData
  (JNIEnv *pEnv, jobject pThis, jstring id, jbyteArray msg, jint nbytes)
{
	int len = pEnv->GetArrayLength(msg);
	int status = -1;
	
	if (id != NULL) {
		unsigned char* buf = new unsigned char[nbytes+1];
		if (buf != NULL) {
			char const *pin = pEnv->GetStringUTFChars(id, 0);
			if (pin != NULL) {
				pEnv->GetByteArrayRegion(msg, 0, nbytes, reinterpret_cast<jbyte *>(buf));
				status = rcpCamera_processData(pin, (char *)buf, nbytes);
				pEnv->ReleaseStringUTFChars(id, pin);
			}
			free(buf);
		}
	}
	
	return (status);
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1getList
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int parameter = -1;

	if ((id != NULL) && (paramName != NULL)) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		if ((pin) && (name)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_getList(pin, (rcp_param_t)parameter);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setList
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jstring listStrings, jstring listValues)
{
	int parameter = -1;

	if ((id) && (paramName) && (listStrings) && (listValues)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *strings = pEnv->GetStringUTFChars(listStrings, 0);
		char const *values = pEnv->GetStringUTFChars(listValues, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (strings) && (values) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_setList(pin, (rcp_param_t)parameter, (char*)strings, (char*)values);
		}
		
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (strings) pEnv->ReleaseStringUTFChars(listStrings, strings);
		if (values) pEnv->ReleaseStringUTFChars(listValues, values);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameter
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int parameter = -1;

	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_getValue(pin, (rcp_param_t)parameter);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameterLabel
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int parameter = -1;
	const char *label;
	jstring jstr = NULL;

	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);
	
		if ((name) && (pin)) {
			rcpCamera_getParameterLabel(pin, (char*)name, &label);
			jstr = pEnv->NewStringUTF(label);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
	return jstr;
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameter
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jint value)
{
	int parameter = -1;

	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);
	
		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_setValue(pin, (rcp_param_t)parameter, (int)value);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameterString
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jstring valueIn)
{
	int parameter = -1;

	if ((id) && (paramName) && (valueIn)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *value = pEnv->GetStringUTFChars(valueIn, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin) && (value)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_setStringValue(pin, (rcp_param_t)parameter, value);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (value) pEnv->ReleaseStringUTFChars(valueIn, value);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1toggleRecordState
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			rcpCamera_setValue(pin, RCP_PARAM_RECORD_STATE, SET_RECORD_STATE_TOGGLE);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int parameter = -1;
	int ret = -1;
	
	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		
		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			ret = rcpCamera_getIsSupported(pin, (rcp_param_t)parameter);
		}
	
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getPropertyIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jint propertyType)
{
	int parameter = -1;
	int ret = -1;
	
	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			ret = rcpCamera_getPropertyIsSupported(pin, (rcp_param_t)parameter, propertyType);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
	return ret;
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameterUnsigned
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jlong value)
{
	int parameter = -1;

	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_setUnsignedValue(pin, (rcp_param_t)parameter, (unsigned int)value);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1send
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int parameter = -1;

	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			rcpCamera_send(pin, (rcp_param_t) parameter);
		}
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
}

JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getAPIVersion
  (JNIEnv *pEnv, jobject pThis)
{
	const char *version;
	jstring jstr;

	rcpCamera_getAPIVersion(&version);
	jstr = pEnv->NewStringUTF(version);

	return jstr;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	int ret = 0;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_menuIsSupported(pin);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuGetChildren
  (JNIEnv *pEnv, jobject pThis, jstring id, jint menuNodeId)
{
	int ret = -1;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_menuGetChildren(pin, menuNodeId);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuNodeStatusIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	int ret = 0;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_menuNodeStatusIsSupported(pin);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuGetNodeStatus
  (JNIEnv *pEnv, jobject pThis, jstring id, jint menuNodeId)
{
	int ret = -1;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_menuGetNodeStatus(pin, menuNodeId);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}


JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationGet
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	int ret = -1;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_notificationGet(pin);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationTimeout
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring notificationId)
{
	int status = -1;
	if ((id) && (notificationId)) {
		char const *nId = pEnv->GetStringUTFChars(notificationId, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((nId) && (pin))
			status = rcpCamera_notificationTimeout(pin, nId);

		if (nId) pEnv->ReleaseStringUTFChars(notificationId, nId);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationResponse
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring notificationId, jint response)
{
	int status = -1;
	if ((id) && (notificationId)) {
		char const *nId = pEnv->GetStringUTFChars(notificationId, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((nId) && (pin))
			status = rcpCamera_notificationResponse(pin, nId, response);
	
		if (nId) pEnv->ReleaseStringUTFChars(notificationId, nId);
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getCameraConnectionStats
  (JNIEnv *pEnv, jobject pThis, jstring id, jint statsType)
{
	int ret = -1;
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			ret = rcpCamera_getCameraConnectionStats(pin, statsType);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return ret;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getStatus
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName)
{
	int status = -1;
	int parameter = -1;
	
	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
			status = rcpCamera_getStatus(pin, (rcp_param_t)parameter);
		}
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
	}
	return (status);
}

JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameterName
  (JNIEnv *pEnv, jobject pThis, jstring id, jint parameterId)
{
	const char *name;
	jstring jstr = NULL;
	
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		if (pin) {
			if (0 == rcpCamera_getParameterName(pin, parameterId, &name))
			{
				jstr = pEnv->NewStringUTF(name);
			}
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return jstr;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1setPeriodicEnable
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jboolean enable)
{
	int status = -1;
	int parameter = -1;
	
	if ((id) && (paramName)) {
		char const *name = pEnv->GetStringUTFChars(paramName, 0);
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if ((name) && (pin)) {
			parameter = rcpCamera_getParameterID(pin, (char*)name);
            status = rcpCamera_periodicEnable(pin, parameter, (int)enable);
		}
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (name) pEnv->ReleaseStringUTFChars(paramName, name);
	}
	return (status);
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id)
{
	int status = -1;
	
	if (id) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);

		if (pin) {
			status = rcpCamera_rftpGetIsSupported(pin);
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpSendFile
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring localFile, jstring cameraFile, jboolean compress, jbyteArray uuid_out)
{
	int status = -1;
	
	if (id && localFile && cameraFile && uuid_out && (pEnv->GetArrayLength(uuid_out) == sizeof(rcp_uuid_t))) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		char const *localPath = pEnv->GetStringUTFChars(localFile, 0);
		char const *cameraPath = pEnv->GetStringUTFChars(cameraFile, 0);
		rcp_uuid_t temp_uuid;
		
		if (pin && localPath && cameraPath) {
			status = rcpCamera_rftpSendFile(pin, localPath, cameraPath, (int)compress, &temp_uuid);
		}
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (localPath) pEnv->ReleaseStringUTFChars(localFile, localPath);
		if (cameraPath) pEnv->ReleaseStringUTFChars(cameraFile, cameraPath);
		
		// If successful
		if (status == RCP_SUCCESS)
			pEnv->SetByteArrayRegion(uuid_out, 0, sizeof(rcp_uuid_t), (const jbyte*)&temp_uuid);
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpRetrieveFile
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring localFile, jstring cameraFile, jint maxFileSize, jboolean compressionAllowed, jbyteArray uuid_out)
{
	int status = -1;
	
	if (id && localFile && cameraFile && uuid_out && (pEnv->GetArrayLength(uuid_out) == sizeof(rcp_uuid_t))) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		char const *localPath = pEnv->GetStringUTFChars(localFile, 0);
		char const *cameraPath = pEnv->GetStringUTFChars(cameraFile, 0);
		rcp_uuid_t temp_uuid;
		
		if (pin && localPath && cameraPath) {
			status = rcpCamera_rftpRetrieveFile(pin, localPath, cameraPath, (int)maxFileSize, (int)compressionAllowed, &temp_uuid);
		}
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (localPath) pEnv->ReleaseStringUTFChars(localFile, localPath);
		if (cameraPath) pEnv->ReleaseStringUTFChars(cameraFile, cameraPath);
		
		// If successful
		if (status == RCP_SUCCESS)
			pEnv->SetByteArrayRegion(uuid_out, 0, sizeof(rcp_uuid_t), (const jbyte*)&temp_uuid);
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpAbortTransfer
  (JNIEnv *pEnv, jobject pThis, jstring id, jbyteArray uuid_in)
{
	int status = -1;
	rcp_uuid_t temp_uuid;
	
	if (id && uuid_in && (pEnv->GetArrayLength(uuid_in) == sizeof(rcp_uuid_t))) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		pEnv->GetByteArrayRegion(uuid_in, 0, sizeof(rcp_uuid_t), (jbyte *)(&temp_uuid));

		if (pin) {
			status = rcpCamera_rftpAbortTransfer(pin, &temp_uuid);;
			pEnv->ReleaseStringUTFChars(id, pin);
		}
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpDeleteFile
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring cameraFile, jbyteArray uuid_out)
{
	int status = -1;
	
	if (id && cameraFile && uuid_out && (pEnv->GetArrayLength(uuid_out) == sizeof(rcp_uuid_t))) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		char const *cameraPath = pEnv->GetStringUTFChars(cameraFile, 0);
		rcp_uuid_t temp_uuid;

		if (pin && cameraPath) {
			status = rcpCamera_rftpDeleteFile(pin, cameraPath, &temp_uuid);
		}

		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (cameraPath) pEnv->ReleaseStringUTFChars(cameraFile, cameraPath);

		// If successful
		if (status == RCP_SUCCESS)
			pEnv->SetByteArrayRegion(uuid_out, 0, sizeof(rcp_uuid_t), (const jbyte*)&temp_uuid);
	}
	return status;
}

JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpDirList
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring path, jbyteArray uuid_out)
{
	int status = -1;
	
	if (id && path && uuid_out && (pEnv->GetArrayLength(uuid_out) == sizeof(rcp_uuid_t))) {
		char const *pin = pEnv->GetStringUTFChars(id, 0);
		char const *cameraPath = pEnv->GetStringUTFChars(path, 0);
		rcp_uuid_t temp_uuid;

		if (pin && cameraPath) {
			status = rcpCamera_rftpDirList(pin, cameraPath, &temp_uuid);
		}
		if (pin) pEnv->ReleaseStringUTFChars(id, pin);
		if (cameraPath) pEnv->ReleaseStringUTFChars(path, cameraPath);
		
		// If successful
		if (status == RCP_SUCCESS)
			pEnv->SetByteArrayRegion(uuid_out, 0, sizeof(rcp_uuid_t), (const jbyte*)&temp_uuid);
	}
	return status;
}
