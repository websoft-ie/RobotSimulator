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
#include "rcp_api.h"
#include "alog.h"
#include "rcp_common.h"
#include "rcp_discovery.h"

#if _MSC_VER 
#include <windows.h>
#define __func__ __FUNCTION__
#define snprintf _snprintf
#else
#include <unistd.h>
#endif

#define RCP_DISCOVERY_PORT	1112
#define CAM_BUFFER_SIZE 	2048

typedef struct {
    JavaVM* javaVM;
    JNIEnv* env;
    jobject object;
    jclass classRCPDiscovery;
	jmethodID methodOnCameraDiscoveryUpdate;
	jmethodID methodOnBroadcastRequest;
    char cameras[CAM_BUFFER_SIZE];
} RCPDiscovery_t;

static RCPDiscovery_t gRCPDiscovery;

static void* rcpDiscovery_handlerThread(void *pArgs);
static void rcpDiscovery_discover(void);

void rcpDiscovery_init(JNIEnv *pEnv, jobject pObject)
{
    //----------------------------------------------------------
    // Cache the VM
    //----------------------------------------------------------
    if (pEnv->GetJavaVM(&(gRCPDiscovery.javaVM)) != JNI_OK) {
    	LOGE("rcpDiscovery: Error caching the VM");
        goto ERROR_OUT;
    }

    //----------------------------------------------------------
    // Cache the Environment
    //----------------------------------------------------------
    gRCPDiscovery.env = pEnv;

    //----------------------------------------------------------
    // Cache the object (the 'this' pointer)
    //----------------------------------------------------------
    gRCPDiscovery.object = pEnv->NewGlobalRef(pObject);

    //----------------------------------------------------------
    // Cache the classes (RCPDiscovery)
    //----------------------------------------------------------
    gRCPDiscovery.classRCPDiscovery = pEnv->FindClass("com/red/rcp/RCPDiscovery");
    makeGlobalRef(pEnv, (_jobject **)&(gRCPDiscovery.classRCPDiscovery));
    if (gRCPDiscovery.classRCPDiscovery == NULL) {
    	LOGE("rcpDiscovery: Error caching the classes");
    	goto ERROR_OUT;
    }

    //----------------------------------------------------------
    // Cache the methods (onCameraDiscoveryUpdate())
    //----------------------------------------------------------
    gRCPDiscovery.methodOnCameraDiscoveryUpdate = pEnv->GetMethodID(gRCPDiscovery.classRCPDiscovery, "onCameraDiscoveryUpdate", "(Ljava/lang/String;)V");
    if (gRCPDiscovery.methodOnCameraDiscoveryUpdate == NULL) {
    	LOGE("JNI: Error caching the methods: onCameraDiscoveryUpdate()");
    	goto ERROR_OUT;
    }

    gRCPDiscovery.methodOnBroadcastRequest = pEnv->GetMethodID(gRCPDiscovery.classRCPDiscovery, "onBroadcastRequest", "([B)V");
    if (gRCPDiscovery.methodOnBroadcastRequest == NULL) {
    	LOGE("JNI: Error caching the methods: onBroadcastRequest()");
    	goto ERROR_OUT;
    }

    // All is good, return
    return;

ERROR_OUT:
	LOGE("rcpDiscovery: An error occurred while initializing the JNI environment.");
	if (gRCPDiscovery.classRCPDiscovery) deleteGlobalRef(gRCPDiscovery.env, (_jobject **)&(gRCPDiscovery.classRCPDiscovery));
	return;
}

int rcpDiscovery_start()
{
    rcpDiscovery_discover();
    
    return 0;
}

void rcpEmitDiscoveryCallback()
{
	JNIEnv *env = getJNIEnv(gRCPDiscovery.javaVM);

	jstring lValue = env->NewStringUTF(gRCPDiscovery.cameras);

	env->CallVoidMethod(gRCPDiscovery.object, gRCPDiscovery.methodOnCameraDiscoveryUpdate, lValue);
	env->DeleteLocalRef(lValue);
}

int rcpDiscovery_processResponse(unsigned char *buf, int len, char *ip_addr)
{
	rcp_discovery_process_data((const char *)buf, len, ip_addr);
	
	return 0;
}

static void broadcast_msg(const char * data, size_t len, void * user_data)
{
	JNIEnv *env = getJNIEnv(gRCPDiscovery.javaVM);

	jbyteArray msg = env->NewByteArray(len);

	env->SetByteArrayRegion(msg, 0, len, reinterpret_cast<jbyte*>((char *)data));

	env->CallVoidMethod(gRCPDiscovery.object, gRCPDiscovery.methodOnBroadcastRequest, msg);

}

static void rcpDiscovery_discover(void)
{
	rcp_discovery_cam_info_list_t * cam_list = NULL;
    char temp[128];
    char ifDesc[16];
    int lError;

    memset(gRCPDiscovery.cameras, 0, CAM_BUFFER_SIZE);
	rcp_discovery_start(broadcast_msg, NULL);
	for (size_t ii = 0; ii < RCP_DISCOVERY_STEP_LOOP_COUNT; ii++)
	{
#if _MSC_VER
		Sleep(RCP_DISCOVERY_STEP_SLEEP_MS);
#else
		usleep(RCP_DISCOVERY_STEP_SLEEP_MS * 1000);
#endif
		rcp_discovery_step();
	}

#if _MSC_VER
		Sleep(RCP_DISCOVERY_STEP_SLEEP_MS);
#else
		usleep(RCP_DISCOVERY_STEP_SLEEP_MS * 1000);
#endif
	cam_list = rcp_discovery_get_list();
	const rcp_discovery_cam_info_list_t * cur = cam_list;
	while (cur)
	{
		// Convert rcp_interface_t enum to a string to display first
		rcp_interface_enum_to_string(cur->info.rcp_interface, ifDesc, sizeof(ifDesc));
		// CameraParams="<ID>:<Pin>:<Type>:<Version>:<RCP Interface>:<IP Address>;
		snprintf(temp, 128, "%s:%s:%s:%s:%s:%s;", cur->info.id, cur->info.pin, cur->info.type, cur->info.version, ifDesc, cur->ip_address);
		// CameraList="<Camera 1 Info>;<Camera 2 Info>;...;<Camera N Info>
		strncat(gRCPDiscovery.cameras, temp, CAM_BUFFER_SIZE - strlen(gRCPDiscovery.cameras) - 1);
		cur = cur->next;
	}

	rcpEmitDiscoveryCallback();
	rcp_discovery_free_list(cam_list);
	rcp_discovery_end();
}




