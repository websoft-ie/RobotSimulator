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

/* DO NOT EDIT THIS FILE - it is machine generated */
#include <jni.h>
/* Header for class com_red_rcp_RCPCamera */

#ifndef _Included_com_red_rcp_RCPCamera
#define _Included_com_red_rcp_RCPCamera
#ifdef __cplusplus
extern "C" {
#endif

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_init
 * Signature: ()V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1init
  (JNIEnv *, jobject);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_open
 * Signature: (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1open
  (JNIEnv *, jobject, jstring, jstring, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_close
 * Signature: (Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1close
  (JNIEnv *, jobject, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_processData
 * Signature: (Ljava/lang/String;[BI)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1processData
  (JNIEnv *, jobject, jstring, jbyteArray, jint);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_toggleRecordState
 * Signature: (Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1toggleRecordState
  (JNIEnv *, jobject, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_setParameter
 * Signature: (Ljava/lang/String;Ljava/lang/String;I)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameter
  (JNIEnv *, jobject, jstring, jstring, jint);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_setParameterString
 * Signature: (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameterString
  (JNIEnv *, jobject, jstring, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getParameter
 * Signature: (Ljava/lang/String;Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameter
  (JNIEnv *, jobject, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getList
 * Signature: (Ljava/lang/String;Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1getList
  (JNIEnv *, jobject, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_setList
 * Signature: (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setList
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jstring listStrings, jstring listValues);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getParameterLabel
 * Signature: (Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;
 */
JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameterLabel
  (JNIEnv *, jobject, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getIsSupported
 * Signature: (Ljava/lang/String;Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getIsSupported
  (JNIEnv *, jobject, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getPropertyIsSupported
 * Signature: (Ljava/lang/String;Ljava/lang/String;I)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getPropertyIsSupported
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jint propertyType);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_setParameterUnsigned
 * Signature: (Ljava/lang/String;Ljava/lang/String;J)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1setParameterUnsigned
  (JNIEnv *, jobject, jstring, jstring, jlong);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_send
 * Signature: (Ljava/lang/String;Ljava/lang/String;)V
 */
JNIEXPORT void JNICALL Java_com_red_rcp_RCPCamera_jni_1send
  (JNIEnv *, jobject, jstring, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getAPIVersion
 * Signature: ()Ljava/lang/String;
 */
JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getAPIVersion
  (JNIEnv *, jobject);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_menuIsSupported
 * Signature: (Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuIsSupported
  (JNIEnv *, jobject, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_menuGetChildren
 * Signature: (Ljava/lang/String;I)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuGetChildren
  (JNIEnv *, jobject, jstring, jint);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_menuNodeStatusIsSupported
 * Signature: (Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuNodeStatusIsSupported
  (JNIEnv *, jobject, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_1menuGetNodeStatus
 * Signature: (Ljava/lang/String;I)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1menuGetNodeStatus
  (JNIEnv *, jobject, jstring, jint);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_notificationGet
 * Signature: (Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationGet
  (JNIEnv *pEnv, jobject pThis, jstring id);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_notificationTimeout
 * Signature: (Ljava/lang/String;Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationTimeout
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring notificationId);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_notificationResponse
 * Signature: (Ljava/lang/String;Ljava/lang/String;I)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1notificationResponse
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring notificationId, jint response);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getCameraConnectionStats
 * Signature: (Ljava/lang/String;I)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getCameraConnectionStats
  (JNIEnv *pEnv, jobject pThis, jstring id, jint statsType);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getStatus
 * Signature: (Ljava/lang/String;Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1getStatus
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_getParameterName
 * Signature: (Ljava/lang/String;I)Ljava/lang/String;
 */
JNIEXPORT jstring JNICALL Java_com_red_rcp_RCPCamera_jni_1getParameterName
  (JNIEnv *, jobject, jstring, jint);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_setPeriodicEnable
 * Signature: (Ljava/lang/String;Ljava/lang/String;Z)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1setPeriodicEnable
  (JNIEnv *pEnv, jobject pThis, jstring id, jstring paramName, jboolean enable);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpIsSupported
 * Signature: (Ljava/lang/String;)I
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpIsSupported
  (JNIEnv *, jobject, jstring);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpSendFile
 * Signature: (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Z[B)I;
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpSendFile
  (JNIEnv *, jobject, jstring, jstring, jstring, jboolean, jbyteArray);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpRetrieveFile
 * Signature: (Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;IZ[B)I;
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpRetrieveFile
  (JNIEnv *, jobject, jstring, jstring, jstring, jint, jboolean, jbyteArray);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpAbortTransfer
 * Signature: (Ljava/lang/String;[B)I;
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpAbortTransfer
  (JNIEnv *, jobject, jstring, jbyteArray);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpDeleteFile
 * Signature: (Ljava/lang/String;Ljava/lang/String;[B)I;
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpDeleteFile
  (JNIEnv *, jobject, jstring, jstring, jbyteArray);

/*
 * Class:     com_red_rcp_RCPCamera
 * Method:    jni_rftpDirList
 * Signature: (Ljava/lang/String;Ljava/lang/String;[B)I;
 */
JNIEXPORT jint JNICALL Java_com_red_rcp_RCPCamera_jni_1rftpDirList
  (JNIEnv *, jobject, jstring, jstring, jbyteArray);

#ifdef __cplusplus
}
#endif
#endif
