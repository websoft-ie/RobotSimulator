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
#include <jni.h>
#ifdef RCP_ANDROID
#include <time.h>
#else // RCP_JAVA
#include <sys/time.h>
#endif
#include "rcp_api.h"
#include "clist/clist.h"
#include "decorated_string/decorated_string.h"
#include "rcp_common.h"
#include "alog.h"
#include "rcp_camera.h"
#include <string>
#include <zlib.h>

#if _MSC_VER
#define __func__ __FUNCTION__
#define snprintf _snprintf
#else
#include <pthread.h>
#endif

using namespace std;

#define PARAM_BUFFER_SIZE 			1024
#define LIST_HEADER_BUFFER_SIZE		128
#define LIST_FLAGS_BUFFER_SIZE		128
#define LIST_BUFFER_SIZE   			16384
#define IP_BUFFER_SIZE			24
#define MAX_PIN_LENGTH			12

#define PRINT_PARAM(s, ...) snprintf(camera->param, PARAM_BUFFER_SIZE, s, ##__VA_ARGS__)

typedef struct {
	char id[MAX_PIN_LENGTH];
} id_holder_t;
// RCPCamera_t - Provides the JNI context for each camera connection
typedef struct {

    // Cached JNI references
    JavaVM* javaVM;
    JNIEnv* env;
    jobject object;

    // Classes & Methods (efficient to cache and use later)
    jclass classRCPCamera;
	jmethodID methodOnUpdate;
	jmethodID methodOnUpdateInt;
	jmethodID methodOnUpdateString;
	jmethodID methodOnUpdateStringEditInfo;
	jmethodID methodOnUpdateEditInfo;
	jmethodID methodOnUpdateClipList;
	jmethodID methodOnArrayUpdateStrings;
	jmethodID methodOnHistogramUpdate;
	jmethodID methodOnConnectionError;
	jmethodID methodOnStateChange;
	jmethodID methodOnStatusUpdate;
	jmethodID methodOnNotificationUpdate;
	jmethodID methodOnAudioVuUpdate;
	jmethodID methodOnMenuNodeListUpdate;
	jmethodID methodOnMenuNodeStatusUpdate;
	jmethodID methodOnUpdateTag;
	jmethodID methodSendToCamera;
	jmethodID methodOnCameraInfo;
	jmethodID methodOnRftpStatusUpdate;
	
    // Parameters
    char param[PARAM_BUFFER_SIZE];
    char listHeader[LIST_HEADER_BUFFER_SIZE];
    // Dynamically allocated
    char *listStrings;
    int *listValues;
    char listFlags[LIST_FLAGS_BUFFER_SIZE];
    id_holder_t *idh;
    char id[MAX_PIN_LENGTH];
    jstring jid;
    bool connected;
    rcp_camera_connection_t *con;
} RCPCamera_t;

// Provides an association between a Java object (in JAVA land) and a camera's PIN by string
static map<std::string, RCPCamera_t*> gCameraPinMap;

// Provides an association between a camera and a connection in the RCP SDK
static map<rcp_camera_connection_t*, RCPCamera_t*> gConnections;

// Provides an association between a UUID and a local file.
static map<std::string, std::string> gRftpLocalPaths;

// Mutex to ensure rcp_process_data is thread safe (per RCP SDK)
// TODO: commented out for win32
#if _MSC_VER
#else
pthread_mutex_t gMutex[RCP_MUTEX_COUNT];
pthread_mutex_t gLocalMutex;
#endif

// Ensures the library performs common initialization only once
bool gLibraryLoaded = false;

void rcpEmitCallback(RCPCamera_t *camera);
void rcpEmitIntCallback(RCPCamera_t *camera, char *name, int value);
void rcpEmitListCallback(RCPCamera_t *camera, int numItems);
void rcpEmitStringCallback(RCPCamera_t *camera);
void rcpEmitStringEditInfoCallback(RCPCamera_t *camera, const char* rcp, const int *intParams, int numInts, int numStrings);
void rcpEmitEditInfoCallback(RCPCamera_t *camera);
void rcpEmitHistogramCallback(const rcp_cur_hist_cb_data_t * data);
void rcpEmitClipListCallback(RCPCamera_t *camera, int status, char *clipList);
void rcpEmitMenuNodeListCallback(RCPCamera_t *camera, int nodeId, char *childList, int numChildren, char *ancestorList, int numAncestors);
void rcpEmitTagCallback(RCPCamera_t *camera);
void rcpEmitRftpStatusCallback(RCPCamera_t *camera, JNIEnv *env, const rcp_cur_rftp_status_cb_data_t * data, jobjectArray dirList);

void * rcp_malloc(size_t NBYTES)
{
    void * addr = malloc(NBYTES);
    return addr;
}

void rcp_free(void * APTR)
{
    free(APTR);
}

void rcp_mutex_lock(rcp_mutex_t id)
{
	// TODO: commented out for win32
#if _MSC_VER
#else
    (void) pthread_mutex_lock(&gMutex[id]);
#endif
}

void rcp_mutex_unlock(rcp_mutex_t id)
{
	// TODO: commented out for win32
#if _MSC_VER
#else
    (void) pthread_mutex_unlock(&gMutex[id]);
#endif
}

void rcp_log(rcp_log_t severity, const rcp_camera_connection_t * con, const char * msg)
{
    switch (severity)
    {
        case RCP_LOG_ERROR:
            LOGE("RCP: error: %s", msg);
            break;

        case RCP_LOG_WARNING:
            LOGI("RCP: warning: %s", msg);
            break;

        case RCP_LOG_INFO:
            //LOGI("RCP: info: %s", msg);
            break;

        case RCP_LOG_DEBUG:
            //LOGI("RCP: debug: %s", msg);
            break;
    }
}

int rcp_rand(void)
{
	static int seed = 1;
	if (seed)
	{
        unsigned int t = time(NULL);
		srand(t);
		seed = 0;
	}
	return rand();
}

uint32_t rcp_timestamp(void)
{
	struct timeval now;
	gettimeofday(&now, NULL);
	return ((now.tv_sec * 1000) + (now.tv_usec / 1000));
}

static void rcpBuildCallbackString(char *param, rcp_camera_connection_t * con, rcp_param_t id, const char * str, const char * str_abbr, rcp_param_status_t status)
{
    const char * color = "";

    switch (status)
    {
        case RCP_PARAM_DISPLAY_STATUS_NORMAL:
        	color = "NONE";
            break;

        case RCP_PARAM_DISPLAY_STATUS_GOOD:
            color = "GREEN";
            break;

        case RCP_PARAM_DISPLAY_STATUS_WARNING:
        case RCP_PARAM_DISPLAY_STATUS_WARNING2:
        case RCP_PARAM_DISPLAY_STATUS_FINALIZING:
            color = "YELLOW";
            break;

        case RCP_PARAM_DISPLAY_STATUS_ERROR:
        case RCP_PARAM_DISPLAY_STATUS_RECORDING:
            color = "RED";
            break;

        case RCP_PARAM_DISPLAY_STATUS_DISABLED:
        	color = "DARK GREY";
        	break;
    }
    snprintf(param, PARAM_BUFFER_SIZE, "%s;%s;%s;%s", rcp_get_name(con, id), str, str_abbr, color);
}


static void cur_uint_display(const rcp_cur_uint_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}

	//LOGI("cur_uint_display: %s", rcp_get_name(data->con, data->id));
	//LOGI("cur_uint_display(): %s|%u|%s|%s|%s|%s|%d|%d|%d ", rcp_get_label(data->con, data->id),
	//	data->cur_val, data->display_str, data->display_str_abbr, data->display_str_abbr_decoded,
	//	data->display_str_decoded, data->display_str_in_list, data->display_str_status,
	//	data->edit_info_valid);

	if (data->display_str_valid)
    {
    	rcpBuildCallbackString(camera->param, data->con, data->id, data->display_str_decoded, data->display_str_abbr_decoded, data->display_str_status);
    	rcpEmitCallback(camera);
    }
	
    if (data->display_str_in_list)
    {
        rcp_get_list(data->con, data->id);
    }
    
    if (data->edit_info_valid)
    {
    	const rcp_cur_uint_edit_info_t * edit_info = &(data->edit_info);
    	PRINT_PARAM("%s;%u;%u;%u;%u;%u;%s;%s", rcp_get_name(data->con, data->id), edit_info->min, edit_info->max,
    			edit_info->divider, edit_info->digits, edit_info->step, edit_info->prefix_decoded, edit_info->suffix_decoded);
        rcpEmitEditInfoCallback(camera);
    }

    if (data->cur_val_valid)
    {
    	rcpEmitIntCallback(camera, (char *)rcp_get_name(data->con, data->id), data->cur_val);
    }    
}

static void cur_int_display(const rcp_cur_int_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}
	
	//LOGI("cur_int_display: %s", rcp_get_name(data->con, data->id));
	//LOGI("cur_int_display(): %s|%d|%d|%d|%d", rcp_get_label(data->con, data->id),
	//		data->display_str_valid, data->display_str_in_list, data->cur_val_valid, data->edit_info_valid);

    if (data->display_str_valid)
    {
    	//LOGI("rcpCamera: %s: data->id=%d, data_str=%s", __func__, data->id, data->display_str_decoded);
    	rcpBuildCallbackString(camera->param, data->con, data->id, data->display_str_decoded, data->display_str_abbr_decoded, data->display_str_status);
        rcpEmitCallback(camera);
    }

    if (data->display_str_in_list)
    {
        rcp_get_list(data->con, data->id);
    }

    if (data->edit_info_valid)
    {
    	const rcp_cur_int_edit_info_t * edit_info = &(data->edit_info);
    	PRINT_PARAM("%s;%d;%d;%d;%d;%d;%s;%s", rcp_get_name(data->con, data->id), edit_info->min, edit_info->max,
    			edit_info->divider, edit_info->digits, edit_info->step, edit_info->prefix_decoded, edit_info->suffix_decoded);
        rcpEmitEditInfoCallback(camera);
    }

    if (data->id == RCP_PARAM_RECORD_STATE && data->cur_val_valid)
    {
        switch (data->cur_val)
        {
            case RECORD_STATE_NOT_RECORDING:
                LOGI("%s: Not Recording\n", rcp_get_name((const rcp_camera_connection_t *)data->con, data->id));
            	rcpBuildCallbackString(camera->param, data->con, data->id, "RECORD", "NONE", RCP_PARAM_DISPLAY_STATUS_NORMAL);
                rcpEmitCallback(camera);
                break;

            case RECORD_STATE_RECORDING:
                LOGI("%s: Recording\n", rcp_get_name((const rcp_camera_connection_t *)data->con, data->id));
                rcpBuildCallbackString(camera->param, data->con, data->id, "RECORDING", "NONE", RCP_PARAM_DISPLAY_STATUS_RECORDING);
                rcpEmitCallback(camera);
                break;

            case RECORD_STATE_FINALIZING:
                LOGI("%s: Finalizing\n", rcp_get_name((const rcp_camera_connection_t *)data->con, data->id));
                rcpBuildCallbackString(camera->param, data->con, data->id, "FINALIZING", "NONE", RCP_PARAM_DISPLAY_STATUS_FINALIZING);
                rcpEmitCallback(camera);
                break;

            case RECORD_STATE_PRE_RECORDING:
                LOGI("%s: Pre-Recording\n", rcp_get_name((const rcp_camera_connection_t *)data->con, data->id));
                rcpBuildCallbackString(camera->param, data->con, data->id, "PRE-RECORDING", "NONE", RCP_PARAM_DISPLAY_STATUS_FINALIZING);
                rcpEmitCallback(camera);
                break;
        }
    }

    if (data->cur_val_valid)
    {
    	rcpEmitIntCallback(camera, (char *)rcp_get_name(data->con, data->id), data->cur_val);
    }
}

static void cur_list_display(const rcp_cur_list_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
	}
	else if (!data->list_string_valid) {
    	LOGE("cur_list_display(): Received list, but list invalid.");
	}
	else {
        // Get the list, current index, and decode the list
        cList list;
        list.importStringListAndDecode(data->list_string);
        const size_t len = list.length();
        size_t index = 0;
        if (cList::SUCCESS != list.getIndex(index))
            index = 0;

        // Now try to allocate memory for the lists
        camera->listStrings = (char *)malloc(len * cList::MaxStringLength);
        if (camera->listStrings == NULL)
        	return;

        camera->listValues = (int *)malloc(len * sizeof(int));
        if (camera->listValues == NULL) {
        	free(camera->listStrings);
        	return;
        }

        memset(camera->listHeader,'\0',LIST_HEADER_BUFFER_SIZE );
        //LOGI("cur_list_display(%s): data->display_str_in_list = %d data->list_string_valid = %d data->list_string = %s", 
		//	rcp_get_name(data->con, data->id), data->display_str_in_list, data->list_string_valid, data->list_string);

        // Populate the string header for Java: <ListName>:<Length>:<CurrentIndex>:<MinValid>:<MinValue>:<MaxValid>:<MaxValue>
        snprintf(camera->listHeader, LIST_HEADER_BUFFER_SIZE, "%s|%d|%d|%d|%d|%d|%d",
        		 rcp_get_name(data->con, data->id), (int)len, (int)index, data->min_val_valid, data->min_val, data->max_val_valid, data->max_val);

        snprintf(camera->listFlags, LIST_FLAGS_BUFFER_SIZE, "%s|%d|%d|%d|%d",
        		rcp_get_name(data->con, data->id), data->send_int, data->send_uint, data->send_str, data->update_list_only_on_close);

        // Attach the list item strings:
        char label[cList::MaxStringLength];
        for (size_t ii = 0; ii < len; ii++)
        {
            cList::NumType value;
            if (cList::SUCCESS == list.getStr(ii, &label))
            {
            	strncpy(&camera->listStrings[ii * cList::MaxStringLength], label, cList::MaxStringLength);
            	if (cList::SUCCESS == list.getNum(ii, value))
            	{
            		camera->listValues[ii] = (int)value;
            	}
            }
        }

        rcpEmitListCallback(camera, len);

    	free(camera->listStrings);
    	free(camera->listValues);

        if (data->display_str_in_list)
        {
            char display_str[cList::MaxStringLength];
            if (cList::SUCCESS == list.getCurrentStr(&display_str))
            {
                char display_str_decoded[cList::MaxStringLength];
                decorated_string_decode(display_str, display_str_decoded, sizeof(display_str_decoded));
				rcpBuildCallbackString(camera->param, data->con, data->id, display_str_decoded, display_str_decoded, RCP_PARAM_DISPLAY_STATUS_NORMAL);
				rcpEmitStringCallback(camera);
            }
        }
    }
}

static void cur_str_display(const rcp_cur_str_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}

	//LOGI("cur_str_display: data->id = %d", data->id);
    if (data->display_str)
    {
        //LOGI("cur_str_display(): %s|%s|%s|%s|%s|%d",
        //		rcp_get_label(data->con, data->id), data->display_str, data->display_str_abbr,
        //		data->display_str_abbr_decoded, data->display_str_decoded, data->display_str_status);
    	//LOGI("cur_str_display: CALLING CALLBACK: data->id = %d", data->id);
    	rcpBuildCallbackString(camera->param, data->con, data->id, data->display_str_decoded, data->display_str_abbr_decoded, data->display_str_status);
        rcpEmitStringCallback(camera);
    }
    if (data->edit_info_valid)
    {
    	const rcp_cur_str_edit_info_t * edit_info = &(data->edit_info);
    	int intParams[3];
    	intParams[0] = (int)edit_info->min_len;
    	intParams[1] = (int)edit_info->max_len;
    	intParams[2] = edit_info->is_password;
    	
    	// camera->param is a large buffer, we shouldn't overflow
    	char *pBuffer = camera->param;
    	// Allowed chars may be a NULL pointer, empty string, or valid
    	if (edit_info->allowed_characters == NULL) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else if (edit_info->allowed_characters[0] == 0x00) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else {
    		snprintf(pBuffer, PARAM_BUFFER_SIZE, "%s", edit_info->allowed_characters);
    		pBuffer += 1 + strlen(edit_info->allowed_characters);
    	}

    	// Prefix may be a NULL pointer, empty string, or valid
    	if (edit_info->prefix_decoded == NULL) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else if (edit_info->prefix_decoded[0] == 0x00) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else {
    		snprintf(pBuffer, PARAM_BUFFER_SIZE, "%s", edit_info->prefix_decoded);
    		pBuffer += 1 + strlen(edit_info->prefix_decoded);
    	}

    	// Suffix chars may be a NULL pointer, empty string, or valid
    	if (edit_info->suffix_decoded == NULL) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else if (edit_info->suffix_decoded[0] == 0x00) {
    		*pBuffer = 0x00;
    		pBuffer++;
    	}
    	else {
    		snprintf(pBuffer, PARAM_BUFFER_SIZE, "%s", edit_info->suffix_decoded);
    		pBuffer += 1 + strlen(edit_info->suffix_decoded);
    	}
    	
    	// camera->param should have three null-terminated strings now
    	rcpEmitStringEditInfoCallback(camera, rcp_get_name(data->con, data->id), intParams, 3, 3);
    }
}

static void cur_clip_list(const rcp_cur_clip_list_cb_data_t * data, void * user_data)
{
	int strsize;

	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}

    if (data)
    {
        switch (data->clip_list_status)
        {
            case CLIP_LIST_LOADING:
               	rcpEmitClipListCallback(camera, data->clip_list_status, NULL);
                break;

            case CLIP_LIST_BLOCKED:
                rcpEmitClipListCallback(camera, data->clip_list_status, NULL);
                break;

            case CLIP_LIST_DONE:
            {
                const rcp_clip_info_list_t * cur = data->clip_list;
                int count = 0;

                // Only way to get count a priori
                while (cur)
                {
                	count++;
                    const extended_clipinfo_t * clipinfo = &cur->info;
                    cur = cur->next;
                }

                int strsize = count * (sizeof(extended_clipinfo_t)+24);
                char *clipList = (char *)rcp_malloc(strsize);
                if (!clipList)
                {
                	LOGE("Error allocating clip list");
                	return;
                }

                *clipList='\0';
                count = 0;
                char cclip[512];
                cur = data->clip_list;

                while (cur)
                {
                    const extended_clipinfo_t * clipinfo = &cur->info;

                    snprintf(cclip, 512, "%d|%s|%s|%s|%d|%s|%s|%s|%s;",
                    		clipinfo->index,
                    		clipinfo->clip_name,
                    		clipinfo->clip_date,
                    		clipinfo->clip_time,
                    		clipinfo->sensor_fps,
                    		clipinfo->edge_start_timecode,
                    		clipinfo->edge_end_timecode,
                    		clipinfo->tod_start_timecode,
                    		clipinfo->tod_end_timecode);

                    strncat(clipList,cclip,strsize);

                    cur = cur->next;
                }

                rcpEmitClipListCallback(camera, data->clip_list_status, clipList);
                rcp_free(clipList);
            }
            break;
        }
    }
}

static void cur_tag_display(const rcp_cur_tag_info_cb_data_t * data, void * user_data)
{
	LOGI("IN CUR_TAG_DISPLAY");
	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}

	sprintf(camera->param, "%s;%d;%llu;", rcp_get_name(camera->con, data->id), data->tag_info.type, data->tag_info.frame);
	strncat(camera->param, data->tag_info.tod_timecode, 14);
	rcpEmitTagCallback(camera);
}

static void cur_hist_display(const rcp_cur_hist_cb_data_t * data, void * user_data)
{
	rcpEmitHistogramCallback(data);
}

static rcp_error_t send_to_camera(const char * data, size_t len, void * user_data)
{
	JNIEnv *env;
	id_holder_t *idh = (id_holder_t *) user_data;
	char const *id = idh->id;
	RCPCamera_t *camera = gCameraPinMap[id];
	
	if(camera != NULL) {
		env = getJNIEnv(camera->javaVM);
		if (env != NULL) {
			jbyteArray msg = env->NewByteArray(len);
			env->SetByteArrayRegion(msg, 0, len, reinterpret_cast<jbyte*>((char *)data));
			env->CallIntMethod(camera->object, camera->methodSendToCamera, msg);
			env->DeleteLocalRef(msg);
		}
	}
		
    return RCP_SUCCESS;
}

static void state_change(const rcp_state_data_t * data, void * user_data)
{
	JNIEnv *env;
	RCPCamera_t *camera = gConnections[data->con];
	char ifDesc[16];
	
	if(camera != NULL) {
		env = getJNIEnv(camera->javaVM);
		if (env != NULL) {

			//LOGI("state_change: RCP state change to camera->id = %d, state = %d", camera->id, data->state);
			//LOGI("state_change: id=%s, pin=%s, type=%s, version=%s, ip=%s", 
			//		data->cam_info->id, data->cam_info->pin, data->cam_info->type, data->cam_info->version, data->cam_info->ip_address);
			snprintf(camera->param, PARAM_BUFFER_SIZE, "%d;%d.%d;%d;%d", data->state, data->parameter_set_version_major, data->parameter_set_version_minor, data->parameter_set_newer, data->parameter_set_version_valid);
			jstring lValue = env->NewStringUTF(camera->param);
			env->CallVoidMethod(camera->object, camera->methodOnStateChange, camera->jid, lValue);

			// Convert rcp_interface_t enum to a string to display first
			rcp_interface_enum_to_string(data->cam_info->rcp_interface, ifDesc, sizeof(ifDesc));
			snprintf(camera->param, PARAM_BUFFER_SIZE, "%s;%s;%s;%s;%s", data->cam_info->id, data->cam_info->pin, data->cam_info->type, data->cam_info->version, ifDesc);
			lValue = env->NewStringUTF(camera->param);
			//LOGI("state_change: calling method onCameraInfo with %s", camera->param);
			env->CallVoidMethod(camera->object, camera->methodOnCameraInfo, camera->jid, lValue);

			if(env->ExceptionCheck()) {
				LOGE("state_change: Exception occurred.");
			}
			if (lValue != NULL) env->DeleteLocalRef(lValue);
		}
	}
}

void cur_status_update(const rcp_cur_status_cb_data_t * data, void * user_data)
{
	JNIEnv *env;
	RCPCamera_t *camera = gConnections[data->con];

	if(camera != NULL) {
		env = getJNIEnv(camera->javaVM);
		if (env != NULL) {

			//LOGI("cur_status_update: Status change to parameter %s, is_enabled = %d, is_enabled_valid = %d",
				//	rcp_get_name(camera->con, data->id), data->is_enabled, data->is_enabled_valid);
			if(data->is_enabled_valid || data->is_supported_valid) {
				snprintf(camera->param, PARAM_BUFFER_SIZE, "%s", rcp_get_name(camera->con, data->id));
				jstring lValue = env->NewStringUTF(camera->param);
				env->CallVoidMethod(camera->object, camera->methodOnStatusUpdate, camera->jid, lValue, 
									(jint)data->is_enabled, (jint)data->is_enabled_valid, (jint)data->is_supported, (jint)data->is_supported_valid);
				if(env->ExceptionCheck()) {
					LOGE("cur_status_update: Exception occurred.");
				}
				if (lValue != NULL) env->DeleteLocalRef(lValue);
			}
		}
	}
}

void notification_update(const rcp_notification_cb_data_t * data, void * user_data)
{
	JNIEnv *env;
	RCPCamera_t *camera = gConnections[data->con];

	if(camera != NULL) {
		env = getJNIEnv(camera->javaVM);
		if (env != NULL) {
			const rcp_notification_t * n = data->notification;
			// Seaparate ID, title, and message into their own parameters because these are strings
			// that may contain our separator character ';'
			snprintf(camera->param, PARAM_BUFFER_SIZE, "%d;%d;%d;%s;%d;%d",
					data->action, n->progress_type, n->progress_percent, n->response_list, n->timeout, n->type);

			jstring lID = env->NewStringUTF(n->id);
			jstring lTitle = env->NewStringUTF(n->title);
			jstring lMsg = env->NewStringUTF(n->message);
			jstring lValue = env->NewStringUTF(camera->param);
			env->CallVoidMethod(camera->object, camera->methodOnNotificationUpdate, camera->jid, lID, lTitle, lMsg, lValue);
			if(env->ExceptionCheck()) {
				LOGE("cur_status_update: Exception occurred.");
			}

			if (lID != NULL) env->DeleteLocalRef(lID);
			if (lTitle != NULL) env->DeleteLocalRef(lTitle);
			if (lMsg != NULL) env->DeleteLocalRef(lMsg);
			if (lValue != NULL) env->DeleteLocalRef(lValue);
		}
	}
}

void cur_audio_vu_update(const rcp_cur_audio_vu_cb_data_t * data, void * user_data)
{
	JNIEnv *env;
	RCPCamera_t *camera = gConnections[data->con];

	if(camera != NULL) {
		env = getJNIEnv(camera->javaVM);
		if (env != NULL) {
			snprintf(camera->param, PARAM_BUFFER_SIZE, "%d;%d;%d;%d;%d;%d;%d;%d;%d;%d",
					data->input_db[0], data->input_db[1], data->input_db[2], data->input_db[3],
					data->output_db[0], data->output_db[1], data->output_db[2], data->output_db[3],
					data->output_db[4], data->output_db[5]);
			jstring lValue = env->NewStringUTF(camera->param);
			env->CallVoidMethod(camera->object, camera->methodOnAudioVuUpdate, camera->jid, lValue);
			if(env->ExceptionCheck()) {
				LOGE("cur_audio_vu_update: Exception occurred.");
			}
			if (lValue != NULL) env->DeleteLocalRef(lValue);
		}
	}
}

static int count_c_list_entries(c_list_t * c_list)
{
	c_list_entry_t * cur = NULL;
	int count = 0;
	cur = c_list->head;
	while(cur) {
		cur = cur->next;
		count++;
	}
	return count;
}

static c_list_error_t arg_c_list_print(RCPCamera_t *camera, char * str, const int strLen, c_list_t * c_list)
{
    c_list_entry_t * cur = NULL;
    char buf[32];
    uint32_t flags;
    
    if (!c_list || !str)
    {
        return C_LIST_PARAM_ERROR;
    }

    int count = count_c_list_entries(c_list);
    snprintf(str, strLen, "|%d", count);

    cur = c_list->head;
    while (cur)
    {
    	flags = rcp_menu_get_multi_action_list_leaf_flags(camera->con, cur->user_defined.int32);
    	snprintf(buf, 32, "|%d|%s|%x", cur->num, cur->str, flags);
    	strcat(str, buf);
        cur = cur->next;
    }

    return C_LIST_SUCCESS;
}

#define ARG_C_LIST_SIZE		128
#define NODE_PARAM_ID_SIZE		16
#define NODE_PAYLOAD_SIZE		16
#define NODE_DESCRIPTOR_SIZE	64
#define NODE_INFO_MAX_SIZE		(ARG_C_LIST_SIZE + NODE_PARAM_ID_SIZE + NODE_PAYLOAD_SIZE + NODE_DESCRIPTOR_SIZE)

static void menu_node_list_print(RCPCamera_t *camera, char *nodeListBuffer, const int nodeSize, int numNodes, rcp_menu_node_list_t * menu_node_list)
{
	rcp_menu_node_list_t * cur = menu_node_list;
	rcp_menu_node_info_t * node;
	char argCList[ARG_C_LIST_SIZE];
	int offset = 0;
	char *pCurrNode;
	char *pCurrSegment;
	
	// Initialize with null char
	pCurrNode = nodeListBuffer;
	
	while (cur && numNodes) {
		node = cur->info;
		// If we get a NULL info field, just skip and keep traversing linked list.
		if (node != NULL) {

			argCList[0]    = '\0';
			pCurrSegment = pCurrNode;
			
			// snprintf returns number of bytes that would have been copied if buffer is big enough, not including null char
			// but it does add the null char at the end
			// So we'll build the node string by snprintf-ing each piece.
			// Then at the end we'll strncat the arg_c_list to the end.
			offset = snprintf(pCurrSegment, NODE_DESCRIPTOR_SIZE, "%d|%d|%d|%d|%d|%d|%d|%d|%s", node->type, node->filter, node->id, node->parent_id,
							node->is_enabled, node->is_enabled_valid, node->is_supported, node->is_supported_valid, node->title);
			if (offset > NODE_DESCRIPTOR_SIZE - 1)
				offset = NODE_DESCRIPTOR_SIZE - 1;
			
			pCurrSegment += offset;
			
			if(RCP_MENU_NODE_TYPE_BRANCH != node->type)
			{
				offset = snprintf(pCurrSegment, NODE_PARAM_ID_SIZE, "|%d", node->param_id);
				if (offset > NODE_PARAM_ID_SIZE - 1)
					offset = NODE_PARAM_ID_SIZE - 1;

				pCurrSegment += offset;
				
				switch(node->type)
				{
				case RCP_MENU_NODE_TYPE_ACTION_LEAF:
					offset = snprintf(pCurrSegment, NODE_PAYLOAD_SIZE, "|%d|%d", node->has_payload, node->payload);
					if (offset > NODE_PAYLOAD_SIZE - 1)
						offset = NODE_PAYLOAD_SIZE - 1;
					
					pCurrSegment += offset;
					break;
				case RCP_MENU_NODE_TYPE_NUMBER_LEAF:
					offset = snprintf(pCurrSegment, NODE_PAYLOAD_SIZE, "|%d|%d|%d", node->send_int, node->send_uint, node->send_str);
					if (offset > NODE_PAYLOAD_SIZE - 1)
						offset = NODE_PAYLOAD_SIZE - 1;

					pCurrSegment += offset;
					break;
				}

				arg_c_list_print(camera, argCList, ARG_C_LIST_SIZE, node->arg_c_list);
			}

			strncat(pCurrNode, argCList, ARG_C_LIST_SIZE);

			// Now go to the next node. We made sure the buffer is big enough already
			pCurrNode += nodeSize;
			numNodes--;
			//LOGI("cur_menu_update: node info = %s", menuNodeInfo1);
			//LOGI("cur_menu_update: arg_c_list = %s", argCList);
		}
		
		cur = cur->next;
	}
}

static int menu_node_list_count(rcp_menu_node_list_t * menu_node_list)
{
	rcp_menu_node_list_t * cur = menu_node_list;
	rcp_menu_node_info_t * node;
	int count = 0;
	
	while(cur) {
		node = cur->info;
		// If we get a NULL info field, just skip and keep traversing linked list.
		if (node != NULL) {
			count++;
		}
		cur = cur->next;
	}
	return count;
}

void cur_menu_update(const rcp_cur_menu_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	int childCount = 0;
	int ancestorCount = 0;
	char *pChildList = NULL;
	char *pAncestorList = NULL;

	if(camera == NULL)
		return;
	
	// first get the number of nodes in the child and ancestor lists
	childCount = menu_node_list_count(data->children_list);
	ancestorCount = menu_node_list_count(data->ancestor_list);

	// Now allocate enough memory to store the strings for these lists
	pChildList = (char *)malloc(childCount * NODE_INFO_MAX_SIZE);
	if (!pChildList)
		goto MENU_ERROR;
	
	pAncestorList = (char *)malloc(ancestorCount * NODE_INFO_MAX_SIZE);
	if (!pAncestorList)
		goto MENU_ERROR;

	menu_node_list_print(camera, pChildList, NODE_INFO_MAX_SIZE, childCount, data->children_list);
	menu_node_list_print(camera, pAncestorList, NODE_INFO_MAX_SIZE, ancestorCount, data->ancestor_list);

	rcpEmitMenuNodeListCallback(camera, data->id, pChildList, childCount, pAncestorList, ancestorCount);

	// Fall through to release memory
MENU_ERROR:
	if (pChildList) free(pChildList);
	if (pAncestorList) free(pAncestorList);
}

void cur_menu_node_status_update(const rcp_cur_menu_node_status_cb_data_t *data, void *user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if ((env != NULL) && (data->is_enabled_valid != 0)) {
		env->CallVoidMethod(camera->object, camera->methodOnMenuNodeStatusUpdate, camera->jid, (int)data->id,
				(int)data->is_enabled, (int)data->is_enabled_valid, (int)data->is_supported, (int)data->is_supported_valid);
	}
}

void rcpEmitCallback(RCPCamera_t *camera)
{	
	JNIEnv *env = getJNIEnv(camera->javaVM);
	
	if (env != NULL) {		
		jstring lValue = env->NewStringUTF(camera->param);
		env->CallVoidMethod(camera->object, camera->methodOnUpdate, camera->jid, lValue);
		if (lValue != NULL) env->DeleteLocalRef(lValue);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}

}

void rcpEmitIntCallback(RCPCamera_t *camera, char *name, int value)
{	

	JNIEnv *env = getJNIEnv(camera->javaVM);
	
	if (env != NULL) {		
		jstring lValue = env->NewStringUTF(name);		
		env->CallVoidMethod(camera->object, camera->methodOnUpdateInt, camera->jid, lValue, (jint)value);
		if (lValue != NULL) env->DeleteLocalRef(lValue);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}

}

void rcpEmitListCallback(RCPCamera_t *camera, int numListItems)
{
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if (env != NULL) {
		jobjectArray listStrings;
		jintArray listValues;

		// Allocate simple strings for the headers and flags
		jstring listHeader = env->NewStringUTF(camera->listHeader);
		jstring listFlags = env->NewStringUTF(camera->listFlags);
		
		// Allocate int array for the values
		listValues = env->NewIntArray(numListItems);
		env->SetIntArrayRegion(listValues, 0, numListItems, (const jint *)camera->listValues);
		
		// Allocate string array for the display strings
		jstring curStr = env->NewStringUTF("");
		listStrings = env->NewObjectArray(numListItems, env->FindClass("java/lang/String"), curStr);
		env->DeleteLocalRef(curStr);
		int i;
		char const *curPtr = camera->listStrings;
		for (i = 0; i < numListItems; i++) {
			curStr = env->NewStringUTF(curPtr);
			env->SetObjectArrayElement(listStrings, i, curStr);
			env->DeleteLocalRef(curStr);
			curPtr += cList::MaxStringLength;
		}
		// Call Java
		env->CallVoidMethod(camera->object, camera->methodOnArrayUpdateStrings, camera->jid, listHeader, listFlags, listStrings, listValues);
		
		if (listHeader != NULL) env->DeleteLocalRef(listHeader);
		if (listStrings != NULL) env->DeleteLocalRef(listStrings);
		if (listValues != NULL) env->DeleteLocalRef(listValues);
		if (listFlags != NULL) env->DeleteLocalRef(listFlags);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitStringCallback(RCPCamera_t *camera)
{
	JNIEnv *env = getJNIEnv(camera->javaVM);
	
	if (env != NULL) {
		jstring lValue = env->NewStringUTF(camera->param);
		env->CallVoidMethod(camera->object, camera->methodOnUpdateString, camera->jid, lValue);
		if (lValue != NULL) env->DeleteLocalRef(lValue);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitClipListCallback(RCPCamera_t *camera, int status, char *clipList)
{
	jstring lValue;


	JNIEnv *env = getJNIEnv(camera->javaVM);
	
	if (env != NULL) {
		if (clipList) {
			lValue = env->NewStringUTF(clipList);
		}
		else {
			lValue = env->NewStringUTF("");
		}
		env->CallVoidMethod(camera->object, camera->methodOnUpdateClipList, camera->jid, status, lValue);
		
		if (clipList) env->DeleteLocalRef(lValue);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitEditInfoCallback(RCPCamera_t *camera)
{
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if (env != NULL) {
		jstring lValue = env->NewStringUTF(camera->param);
		env->CallVoidMethod(camera->object, camera->methodOnUpdateEditInfo, camera->jid, lValue);
		
		if (lValue != NULL) env->DeleteLocalRef(lValue);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitStringEditInfoCallback(RCPCamera_t *camera, const char* rcp, const int *intParams, int numInts, int numStrings)
{
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if (env != NULL) {
		jobjectArray stringArray;
		jintArray intArray;
		jstring rcpParam;
		jstring curStr;
		
		// Allocate simple string for RCP parameter name
		rcpParam = env->NewStringUTF(rcp);
		
		// Create array for integer parameters and then fill in the array
		intArray = env->NewIntArray(numInts);
		env->SetIntArrayRegion(intArray, 0, numInts, (const jint *)intParams);
	
		// Create array for string parameters
		curStr = env->NewStringUTF("");
		stringArray = env->NewObjectArray(numStrings, env->FindClass("java/lang/String"), curStr);
		env->DeleteLocalRef(curStr);

		// Now fill in the string array
		int i;
		char const *curPtr = camera->param;
		for (i = 0; i < numStrings; i++) {
			if (curPtr[0] == 0x00) {
				curPtr++;
			}
			else {
				curStr = env->NewStringUTF(curPtr);
				env->SetObjectArrayElement(stringArray, i, curStr);
				env->DeleteLocalRef(curStr);
				curPtr += strlen(curPtr) + 1;
			}
		}

		// Call Java
		env->CallVoidMethod(camera->object, camera->methodOnUpdateStringEditInfo, camera->jid, rcpParam, intArray, stringArray);
		
		if (stringArray != NULL) env->DeleteLocalRef(stringArray);
		if (intArray != NULL) env->DeleteLocalRef(intArray);
		if (rcpParam != NULL) env->DeleteLocalRef(rcpParam);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitMenuNodeListCallback(RCPCamera_t *camera, int nodeId, char *childList, int numChildren, char *ancestorList, int numAncestors)
{
	jobjectArray jListChildren;
	jobjectArray jListancestors;
	jstring jCurStr;
	int i;
	char const *pCurr;
	
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if (env != NULL) {
		// Allocate string array for the children and ancestor lists
		jCurStr = env->NewStringUTF("");
		jListChildren = env->NewObjectArray(numChildren, env->FindClass("java/lang/String"), jCurStr);
		jListancestors = env->NewObjectArray(numAncestors, env->FindClass("java/lang/String"), jCurStr);
		env->DeleteLocalRef(jCurStr);

		// Now populate the lists
		pCurr = childList;
		for (i = 0; i < numChildren; i++) {
			jCurStr = env->NewStringUTF(pCurr);
			env->SetObjectArrayElement(jListChildren, i, jCurStr);
			env->DeleteLocalRef(jCurStr);
			pCurr += NODE_INFO_MAX_SIZE;
		}

		pCurr = ancestorList;
		for (i = 0; i < numAncestors; i++) {
			jCurStr = env->NewStringUTF(pCurr);
			env->SetObjectArrayElement(jListancestors, i, jCurStr);
			env->DeleteLocalRef(jCurStr);
			pCurr += NODE_INFO_MAX_SIZE;
		}

		env->CallVoidMethod(camera->object, camera->methodOnMenuNodeListUpdate, camera->jid, nodeId, jListChildren, jListancestors);

		if (jListChildren != NULL) env->DeleteLocalRef(jListChildren);
		if (jListancestors != NULL) env->DeleteLocalRef(jListancestors);
	}
	else {
		LOGE("%s: ERROR getting env!", __func__);
	}
}

void rcpEmitTagCallback(RCPCamera_t *camera)
{

	JNIEnv *env = getJNIEnv(camera->javaVM);
	
	if (env != NULL) {
		jstring paramName = env->NewStringUTF(camera->param);
		env->CallVoidMethod(camera->object, camera->methodOnUpdateTag, camera->jid, paramName);
		env->DeleteLocalRef(paramName);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}

}

void rcpEmitHistogramCallback(const rcp_cur_hist_cb_data_t * data)
{
	jintArray red;
	jintArray green;
	jintArray blue;
	jintArray luma;


	RCPCamera_t *camera = gConnections[data->con];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return;
	}
	
	JNIEnv *env = getJNIEnv(camera->javaVM);
	if (env != NULL) {
	
		jstring text = env->NewStringUTF(data->display_str_abbr);
	
		red = env->NewIntArray(128);
		env->SetIntArrayRegion(red, 0, 128, (const jint *)data->red);
	
		green = env->NewIntArray(128);
		env->SetIntArrayRegion(green, 0, 128, (const jint *)data->green);
	
		blue = env->NewIntArray(128);
		env->SetIntArrayRegion(blue, 0, 128, (const jint *)data->blue);
	
		luma = env->NewIntArray(128);
		env->SetIntArrayRegion(luma, 0, 128, (const jint *)data->luma);
	
		env->CallVoidMethod(camera->object, camera->methodOnHistogramUpdate,
				camera->jid, red, green, blue, luma, data->bottom_clip, data->top_clip, text);

		env->DeleteLocalRef(red);
		env->DeleteLocalRef(green);
		env->DeleteLocalRef(blue);
		env->DeleteLocalRef(luma);
		env->DeleteLocalRef(text);
	} else {
		LOGE("%s: ERROR getting env!", __func__);
	}

}

void handle_cur_rftp_store(RCPCamera_t *camera, JNIEnv *env, const rcp_cur_rftp_status_cb_data_t * data) {
    if ((data->error == RFTP_SUCCESS) || (data->error == RFTP_TRANSFER_COMPLETE)) {
    	if (data->percent_complete == 100)
    		free(data->data);
	}
	else {
		free(data->data);
	}
	rcpEmitRftpStatusCallback(camera, env, data, NULL);
}

void handle_cur_rftp_retrieve(RCPCamera_t *camera, JNIEnv *env, const rcp_cur_rftp_status_cb_data_t * data) {
    if (
    		((data->error == RFTP_SUCCESS) || (data->error == RFTP_TRANSFER_COMPLETE)) &&
    		(data->percent_complete == 100)
    	) {
		// Try to find the filename corresponding to this UUID
		std::map<std::string, std::string>::iterator it = gRftpLocalPaths.find(data->uuid.str);
		if (it != gRftpLocalPaths.end())
		{
            bool fileCreated = false;
			const char * const filename = it->second.c_str();

			if (data->is_compressed) {
				if (strncmp((char *) data->data, "REDZLIB0", 8) == 0) {
					unsigned long uncompressedSize = *((uint32_t *) (data->data + 8));
					uint8_t * uncompressedData = (uint8_t *) malloc(uncompressedSize);
					const int zlibRet = uncompress(uncompressedData, &uncompressedSize, data->data + 12, data->data_size - 12);
					
					if (zlibRet == Z_OK) {
						FILE * file = fopen(filename, "w");
						fwrite(uncompressedData, sizeof(uint8_t), uncompressedSize, file);
						fclose(file);
					
						fileCreated = true;
						//LOGI("Uncompressed %lu bytes of data to %d bytes of data\n", data->data_size - 12, uncompressedSize);
					}
					else {
						LOGE("error %d: unable to uncompress data\n", zlibRet);
					}
					
					free(uncompressedData);
				}
				else {
					LOGE("error: the compressed data is not valid\n");
				}
			}
			else {
				FILE * file = fopen(filename, "w");
				fwrite(data->data, sizeof(uint8_t), data->data_size, file);
				fclose(file);
				fileCreated = true;
			}

			if (fileCreated) {
				LOGI("Retrieve file transfer \"%s\" complete. Saved file locally as \"%s\"\n", data->uuid.str, filename);
			}
			gRftpLocalPaths.erase(data->uuid.str);
		}
	}
	rcpEmitRftpStatusCallback(camera, env, data, NULL);
}

void handle_cur_rftp_dir_list(RCPCamera_t *camera, JNIEnv *env, const rcp_cur_rftp_status_cb_data_t * data) {
	jstring jCurStr = NULL;
	jobjectArray jDirList = NULL;
	char temp[C_LIST_MAX_STRING_LEN + 7];
	
	if ((data->error == RFTP_SUCCESS) || (data->error == RFTP_TRANSFER_COMPLETE)) {
    	c_list_t * const list = c_list_create(malloc, free);

    	if (c_list_import_from_string(list, data->directory_list_string) == C_LIST_SUCCESS) {
    		size_t i;
			const size_t length = c_list_get_length(list);
			// Allocate string array for the children and ancestor lists
			jCurStr = env->NewStringUTF("");
			jDirList = env->NewObjectArray(length, env->FindClass("java/lang/String"), jCurStr);
			env->DeleteLocalRef(jCurStr);

			if (jDirList) {
				for (i = 0; i < length; i++) {
					c_list_entry_t entry;
				
					if (c_list_get_entry(list, i, &entry) == C_LIST_SUCCESS) {
						char permissions[4];
				
						permissions[0] = (entry.num & RFTP_PERMISSION_DIRECTORY) ? 'd' : '-';
						permissions[1] = (entry.num & RFTP_PERMISSION_READ) ? 'r' : '-';
						permissions[2] = (entry.num & RFTP_PERMISSION_WRITE) ? 'w' : '-';
						permissions[3] = '\0';
	
						snprintf(temp, sizeof(temp), "%s\t\t%s\n", permissions, entry.str);
						jCurStr = env->NewStringUTF(temp);
						env->SetObjectArrayElement(jDirList, i, jCurStr);
						env->DeleteLocalRef(jCurStr);
					}
					else {
						LOGE("handle_cur_rftp_dir_list: error getting list entry for index %u\n", i);
					}
				}
				rcpEmitRftpStatusCallback(camera, env, data, jDirList);
				env->DeleteLocalRef(jDirList);
			}
    	}
        else {
        	LOGE("handle_cur_rftp_dir_list: error importing list\n");
        }

    	if (c_list_delete(list) != C_LIST_SUCCESS) {
    		LOGE("handle_cur_rftp_dir_list: error deleting list\n");
        }
    }	
}

void cur_rftp_status_display(const rcp_cur_rftp_status_cb_data_t * data, void * user_data)
{
	RCPCamera_t *camera = gConnections[data->con];
	JNIEnv *env = getJNIEnv(camera->javaVM);

	if (env != NULL) {
		switch (data->rftp_type) {
		case RFTP_TYPE_STORE:
			handle_cur_rftp_store(camera, env, data);
			break;

		case RFTP_TYPE_RETRIEVE:
			handle_cur_rftp_retrieve(camera, env, data);
			break;

		case RFTP_TYPE_ABORT_STORE:
			free(data->data);
			rcpEmitRftpStatusCallback(camera, env, data, NULL);
			break;

		case RFTP_TYPE_ABORT_RETRIEVE:
			gRftpLocalPaths.erase(data->uuid.str);
			rcpEmitRftpStatusCallback(camera, env, data, NULL);
			break;

		case RFTP_TYPE_DELETE:
			rcpEmitRftpStatusCallback(camera, env, data, NULL);
			break;

		case RFTP_TYPE_LIST:
			handle_cur_rftp_dir_list(camera, env, data);
			break;

		case RFTP_TYPE_NONE:
		default:
    		LOGE("Unhandled RFTP status: %d\n", data->rftp_type);
			break;
		}
	}
}

void rcpEmitRftpStatusCallback(RCPCamera_t *camera, JNIEnv *env, const rcp_cur_rftp_status_cb_data_t * data, jobjectArray dirList) {
	jbyteArray uuidArray;
	
	uuidArray = env->NewByteArray(sizeof(rcp_uuid_t));
	if (!uuidArray) {
		return;
	}
	
    // Now fill in UUID array
    env->SetByteArrayRegion(uuidArray, 0 , sizeof(rcp_uuid_t), (const jbyte *)&data->uuid);

	env->CallVoidMethod(camera->object, camera->methodOnRftpStatusUpdate, camera->jid, uuidArray, (int)data->rftp_type,
						(int)data->error, (int)data->percent_complete, dirList);
	
	env->DeleteLocalRef(uuidArray);
}

static int rcpCamera_establishConnection(RCPCamera_t *camera, char const *name, char const *ver, char const *user)
{
	int status = 0;
    rcp_camera_connection_info_t info;

    // Initialize to NULL first;
    memset(&info, 0, sizeof(rcp_camera_connection_info_t));

    /* initialize client ID strings */
    info.client_name = name;
    info.client_version = ver;
    info.client_user = user;

	/* setup callbacks */
	info.send_data_to_camera_cb = send_to_camera;
	info.send_data_to_camera_cb_user_data = (void *)camera->idh;
	info.cur_int_cb = cur_int_display;
	info.cur_int_cb_user_data = NULL;
	info.cur_uint_cb = cur_uint_display;
	info.cur_uint_cb_user_data = NULL;
	info.cur_list_cb = cur_list_display;
	info.cur_list_cb_user_data = NULL;
	info.cur_hist_cb = cur_hist_display;
	info.cur_hist_cb_user_data = NULL;
	info.cur_str_cb = cur_str_display;
	info.cur_str_cb_user_data = NULL;
	info.clip_list_cb = cur_clip_list;
	info.clip_list_cb_user_data = NULL;
	info.cur_tag_cb = cur_tag_display;
	info.cur_tag_cb_user_data = NULL;
	info.state_cb = state_change;
	info.state_cb_user_data = NULL;
	info.cur_status_cb = cur_status_update;
	info.cur_status_cb_user_data = NULL;
	info.notification_cb = notification_update;
	info.notification_cb_user_data = NULL;
	info.cur_audio_vu_cb = cur_audio_vu_update;
	info.cur_audio_vu_cb_user_data = NULL;
	info.cur_menu_cb = cur_menu_update;
	info.cur_menu_cb_user_data = NULL;
    info.cur_menu_node_status_cb = cur_menu_node_status_update;
    info.cur_menu_node_status_cb_user_data = NULL;
    
    info.rftp_status_cb = cur_rftp_status_display;
    info.rftp_status_cb_user_data = NULL;
    
	camera->con = rcp_create_camera_connection(&info);
	if (camera->con == NULL) {
		LOGE("rcpCamera: Error creating camera connection");
		goto ERROR;
	}

	// Create a key / value pair for a connection and the camera
	gConnections[camera->con] = camera;

    // All is good, return
    return status;

ERROR:
	status = -1;
	return status;
}

int rcpCamera_initLibrary()
{
	if (gLibraryLoaded == false) {
		// Initialize the library, things that need to be done once
	    // initialize RCP mutexes
		// TODO: commented out for win32
#if _MSC_VER
#else
	    for(size_t ii = 0; ii < RCP_MUTEX_COUNT; ii++) {
	        pthread_mutexattr_t attr;
	        pthread_mutexattr_init(&attr);
	        pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
	        pthread_mutex_init(&(gMutex[ii]), &attr);
	    }
	    
        pthread_mutexattr_t lattr;
        pthread_mutexattr_init(&lattr);
        pthread_mutexattr_settype(&lattr, PTHREAD_MUTEX_RECURSIVE);
        pthread_mutex_init(&(gLocalMutex), &lattr);
        
#endif
        gLibraryLoaded = true;
	}
	return 0;
}

int rcpCamera_open(JNIEnv *pEnv, jobject pThis, char const *id, char const *name, char const *ver, char const *user)
{
	RCPCamera_t *camera;
	jstring jstr;
	rcp_mutex_lock(RCP_MUTEX_CONNECTION);
	
	LOGI("rcpCamera_open: %s", id);

	std::map<std::string, RCPCamera_t*>::iterator pos = gCameraPinMap.find(id);
	if ( pos == gCameraPinMap.end()) {
		// Object not found
		LOGI("rcpCamera_open: Object not found, creating camera instance...");
		camera = (RCPCamera_t *)malloc(sizeof(RCPCamera_t));
		if (camera == NULL) goto ERROR;
		camera->idh = (id_holder_t *)malloc(sizeof(id_holder_t));
		strncpy(camera->idh->id, id, sizeof(camera->idh->id));

		// Insert the camera object into the map
		gCameraPinMap[id] = camera;

	} else {
		// Object has been found
		camera = pos->second;
		std::string key = pos->first;
		LOGE("rcpCamera_open: Object found: %s", key.data());
		if (camera == NULL) {
			// Weird effect here in Android tablets. We disconnect the camera, call rcpCamera_close, and remove this camera
			// from the gCameraPinMap. If we then connect to the same camera again, when we end up in this function again we'll
			// properly not find the camera in the map and then proceed as expected. However, if we sleep and then wake the tablet before
			// reconnecting the camera, we somehow magically find the camera pin in the map, but the camera pointer is null, ending up here.
			LOGE("rcpCamera_open: but camera is NULL, recreating camera instance...");
			camera = (RCPCamera_t *)malloc(sizeof(RCPCamera_t));
			if (camera == NULL) goto ERROR;
			camera->idh = (id_holder_t *)malloc(sizeof(id_holder_t));
			strncpy(camera->idh->id, id, sizeof(camera->idh->id));

			// Insert the camera object into the map
			gCameraPinMap[id] = camera;
		}
	}

	strncpy(camera->id, id, sizeof(camera->id));
	jstr = pEnv->NewStringUTF(camera->id);

	camera->jid = (jstring) pEnv->NewGlobalRef(jstr);
	pEnv->DeleteLocalRef(jstr);

	// Cache the VM
	if (pEnv->GetJavaVM(&(camera->javaVM)) != JNI_OK) {
		LOGE("JNI: Error caching the VM");
		goto ERROR;
	}

	// Cache the Environment
	camera->env = pEnv;

	// Cache the Object
	camera->object = pEnv->NewGlobalRef(pThis);

	// Cache the classes (RCPCamera)
	camera->classRCPCamera = pEnv->FindClass("com/red/rcp/RCPCamera");
	makeGlobalRef(pEnv, (_jobject **)&(camera->classRCPCamera));
	if (camera->classRCPCamera == NULL) {
		LOGE("rcpCamera: Error caching the classes");
		goto ERROR;
	}

	// Cache the methods
	camera->methodOnUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onUpdate", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnUpdate == NULL) {
		LOGE("JNI: Error caching the methods: onUpdate()");
		goto ERROR;
	}

	camera->methodOnUpdateInt = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateInt", "(Ljava/lang/String;Ljava/lang/String;I)V");
	if (camera->methodOnUpdateInt == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateInt()");
		goto ERROR;
	}

	camera->methodOnUpdateString = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateString", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnUpdateString == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateString()");
		goto ERROR;
	}

	camera->methodOnUpdateStringEditInfo = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateStringEditInfo", "(Ljava/lang/String;Ljava/lang/String;[I[Ljava/lang/String;)V");
	if (camera->methodOnUpdateStringEditInfo == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateStringEditInfo()");
		goto ERROR;
	}
	
	camera->methodOnUpdateEditInfo = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateEditInfo", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnUpdateEditInfo == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateEditInfo()");
		goto ERROR;
	}

	camera->methodOnUpdateClipList = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateClipList", "(Ljava/lang/String;ILjava/lang/String;)V");
	if (camera->methodOnUpdateClipList == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateClipList()");
		goto ERROR;
	}
	
	camera->methodOnArrayUpdateStrings = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateListStrings", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;[I)V");
	if (camera->methodOnArrayUpdateStrings == NULL) {
		LOGE("JNI: Error caching the methods: onListUpdateStrings array()");
		goto ERROR;
	}
	
	camera->methodOnHistogramUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateHistogram", "(Ljava/lang/String;[I[I[I[IIILjava/lang/String;)V");
	if (camera->methodOnHistogramUpdate == NULL) {
		LOGE("JNI: Error caching the methods: onHistogramUpdate()");
		goto ERROR;
	}

	camera->methodOnConnectionError = pEnv->GetMethodID(camera->classRCPCamera, "onConnectionError", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnConnectionError == NULL) {
		LOGE("JNI: Error caching the methods: onConnectionError()");
		goto ERROR;
	}

	camera->methodOnStateChange = pEnv->GetMethodID(camera->classRCPCamera, "onStateChange", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnStateChange == NULL) {
		LOGE("JNI: Error caching the methods: onStateChange()");
		goto ERROR;
	}

	camera->methodOnStatusUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onStatusUpdate", "(Ljava/lang/String;Ljava/lang/String;IIII)V");
	if (camera->methodOnStatusUpdate == NULL) {
		LOGE("JNI: Error caching the methods: onStateChange()");
		goto ERROR;
	}

	camera->methodOnNotificationUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onNotificationUpdate", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnNotificationUpdate == NULL) {
		LOGE("JNI: Error caching the methods: methodOnNotificationUpdate()");
		goto ERROR;
	}

	camera->methodOnAudioVuUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onAudioVuUpdate", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnAudioVuUpdate == NULL) {
		LOGE("JNI: Error caching the methods: methodOnAudioVuUpdate()");
		goto ERROR;
	}

	camera->methodOnMenuNodeListUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onMenuNodeListUpdate", "(Ljava/lang/String;I[Ljava/lang/String;[Ljava/lang/String;)V");
	if (camera->methodOnMenuNodeListUpdate == NULL) {
		LOGE("JNI: Error caching the methods: methodOnMenuNodeListUpdate()");
		goto ERROR;
	}
	
	camera->methodOnMenuNodeStatusUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onMenuNodeStatusUpdate", "(Ljava/lang/String;IIIII)V");
	if (camera->methodOnMenuNodeStatusUpdate == NULL) {
		LOGE("JNI: Error caching the methods: methodOnMenuNodeStatusUpdate()");
		goto ERROR;
	}
	
	camera->methodOnUpdateTag = pEnv->GetMethodID(camera->classRCPCamera, "onUpdateTag", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnUpdateTag == NULL) {
		LOGE("JNI: Error caching the methods: onUpdateTag()");
		goto ERROR;
	}

	camera->methodSendToCamera = pEnv->GetMethodID(camera->classRCPCamera, "sendToCamera", "([B)I");
	if (camera->methodSendToCamera == NULL) {
		LOGE("JNI: Error caching the methods: sendToCamera()");
		goto ERROR;
	}

	camera->methodOnCameraInfo = pEnv->GetMethodID(camera->classRCPCamera, "onCameraInfo", "(Ljava/lang/String;Ljava/lang/String;)V");
	if (camera->methodOnCameraInfo == NULL) {
		LOGE("JNI: Error caching the methods: onCameraInfo()");
		goto ERROR;
	}
	
	camera->methodOnRftpStatusUpdate = pEnv->GetMethodID(camera->classRCPCamera, "onRftpStatusUpdate", "(Ljava/lang/String;[BIII[Ljava/lang/String;)V");
	if (camera->methodOnRftpStatusUpdate == NULL) {
		LOGE("JNI: Error caching the methods: methodOnRftpStatusUpdate()");
		goto ERROR;
	}

	if ( -1 == rcpCamera_establishConnection(camera, name, ver, user) ) {
		LOGE("rcpCamera: Error establishing camera connection");
		goto ERROR;
	}

	// All is good, return
	rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
	return (0);

ERROR:
	// There was an error, do cleanup
	if (camera) {
		if (camera->classRCPCamera) deleteGlobalRef(pEnv, (_jobject **)&camera->classRCPCamera);
		if (camera->object) pEnv->DeleteGlobalRef(camera->object);
		if (camera->jid) pEnv->DeleteGlobalRef(camera->jid);
		if (camera) free((void *)camera);
	}
	rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
	return(-1);
}

int rcpCamera_close(JNIEnv *pEnv, jobject pThis, char const *id)
{
	RCPCamera_t *camera;

	rcp_mutex_lock(RCP_MUTEX_CONNECTION);

	LOGI("rcpCamera_close: %s", id);

	// find the ID / camera pair
	std::map<std::string, RCPCamera_t*>::iterator pos1 = gCameraPinMap.find(id);
	if ( pos1 == gCameraPinMap.end()) {
		LOGE("rcpCamera_close: Unable to close connection. Camera object not found.");
		rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
		return -1;
	}
	camera = pos1->second;
	if (camera == NULL) {
		LOGE("rcpCamera_close: Unable to close connection. Camera object is NULL.");
		rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
		return -1;
	}

	// find the connection / camera pair
	std::map<rcp_camera_connection_t*, RCPCamera_t*>::iterator pos2 = gConnections.find(camera->con);
	if ( pos2 == gConnections.end()) {
		LOGE("rcpCamera_close: Unable to close connection. Camera Connection / Camera pair not found");
		rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
		return -1;
	}

	camera->connected = false;

	// The receive thread has exited, so now we can clean up the memory
	if (camera->classRCPCamera) deleteGlobalRef(pEnv, (_jobject **)&camera->classRCPCamera);
	if (camera->object) pEnv->DeleteGlobalRef(camera->object);
	if (camera->jid) pEnv->DeleteGlobalRef(camera->jid);

	gCameraPinMap.erase(pos1);
	gConnections.erase(pos2);
	free(camera->idh);
	free(camera);

	LOGI("rcpCamera: Connection closed.");
	rcp_mutex_unlock(RCP_MUTEX_CONNECTION);
	return 0;
}

int rcpCamera_processData(char const *id, char *data, int len) 
{	
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	else {
		if (camera->con != NULL) {
			//LOGI("rcpCamera: process len = %d", len);
			rcp_process_data(camera->con, data, len);
		}
	}
	
	return 0;	
}

int rcpCamera_getParameterID(char const *id, char *name) {

	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	return (int)rcp_get_id((const rcp_camera_connection_t *)camera->con, name);
}

int rcpCamera_getParameterName(char const *id, int paramId, const char **paramName) {

	RCPCamera_t *camera = gCameraPinMap[id];
	if ((camera == NULL) || (paramName == NULL)) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	*paramName = rcp_get_name((const rcp_camera_connection_t *)camera->con, (rcp_param_t)paramId);
	return 0;
}

static const char labelError[] = "ERROR\0";
int rcpCamera_getParameterLabel(char const *id, char *name, const char **label)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		*label = labelError;
		return -1;
	}
	*label = rcp_get_label(camera->con, rcp_get_id(camera->con,name) );
	return 0;
}

int rcpCamera_setValue(char const *id, int paramId, int value)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int)rcp_set_int(camera->con, (rcp_param_t)paramId, value);
}

int rcpCamera_setUnsignedValue(char const *id, int paramId, unsigned int value)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int)rcp_set_uint(camera->con, (rcp_param_t)paramId, value);
}

int rcpCamera_setStringValue(char const *id, int paramId, char const *value)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	//LOGI("SET STRING VALUE = %s", value);
	return (int)rcp_set_str((rcp_camera_connection_t *)camera->con, (rcp_param_t)paramId, value);
}
int rcpCamera_getValue(char const *id, int paramId)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	//LOGI("rcpCamera_getValue(): data id = %d", paramId);
	return (int)rcp_get(camera->con, (rcp_param_t)paramId);
}

int rcpCamera_getList(char const *id, int paramId)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	return (int)rcp_get_list(camera->con, (rcp_param_t)paramId);
}

static int splitString(const char * str, char ** tokens, const char * delim)
{
	int i = 0;
	char * s = strtok((char*)str, (char*)delim);
	while(s != NULL) {
//		LOGI("%s", s);
		strcpy(tokens[i++], s);
		s = strtok(NULL, (char*)delim);
	}
	return i;
}

#define NUM_STRINGS	32
#define NUM_VALUES	32
#define TOKEN_SIZE	32

static int splitString(const char * str, char (*tokens)[TOKEN_SIZE], const char * delim)
{
	int i = 0;
	char * s = strtok((char*)str, (char*)delim);
	while(s != NULL) {
		strcpy(tokens[i++], s);
		s = strtok(NULL, (char*)delim);
	}
	return i;
}

int rcpCamera_setList(char const *id, int paramId, const char * listStrings, const char * listValues)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	char delim[] = "|";
	user_defined_t user_data;
	user_data.int32 = 0;
    user_data.ptr = NULL;

	char exported_str[NUM_STRINGS*TOKEN_SIZE + NUM_VALUES*TOKEN_SIZE];
	char strings[NUM_STRINGS][TOKEN_SIZE];
	char values[NUM_VALUES][TOKEN_SIZE];

	int i = splitString((const char *)listStrings, (char (*)[TOKEN_SIZE]) strings, (const char *) delim);
	int j = splitString((const char *)listValues, (char (*)[TOKEN_SIZE]) values, (const char *) delim);
	if (i != j) {
		LOGE("rcpCamera_setList: Number of elements in listStrings different from that in listValues.");
		return -1;
	}

	c_list_t * c_list = c_list_create(malloc, free);
	for (int k = 0; k < i; ++k) {
		c_list_append(c_list, atoi(values[k]), strings[k], user_data);
	}

	c_list_export_to_string(c_list, exported_str, sizeof(exported_str));
	//LOGI("rcpCamera_setList: list = %s", exported_str);
	int status = rcp_set_list(camera->con, (rcp_param_t)paramId, exported_str);
	//LOGI("rcpCamera_setList: status = %d", status);
	c_list_delete(c_list);

	return status;
}

int rcpCamera_getIsSupported(char const *id, int paramId)
{
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	rcp_param_properties_t properties;
	return rcp_get_is_supported(camera->con, (rcp_param_t)paramId, &properties);
}

int rcpCamera_getPropertyIsSupported(char const *id, int paramId, int propertyType)
{
	int isSupported = 0;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	rcp_param_properties_t properties;
	isSupported = rcp_get_is_supported(camera->con, (rcp_param_t)paramId, &properties);
	switch(propertyType) {
		case 0: isSupported = properties.has_get; break;
		case 1: isSupported = properties.has_get_list; break;
		case 2: isSupported = properties.has_get_status; break;
		case 3: isSupported = properties.has_send; break;
		case 4: isSupported = properties.has_set_int; break;
		case 5: isSupported = properties.has_set_uint; break;
		case 6: isSupported = properties.has_set_str; break;
		case 7: isSupported = properties.has_set_list; break;
		case 8: isSupported = properties.has_edit_info; break;
		case 9: isSupported = properties.update_list_only_on_close; break;
	}
	return isSupported;
}

int rcpCamera_send(char const *id, int paramId) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	return (int)rcp_send(camera->con, (rcp_param_t)paramId);
}

int rcpCamera_getAPIVersion(const char **version) {
	*version = rcp_api_get_version();
	return 0;
}

int rcpCamera_menuIsSupported(char const *id) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_menu_is_supported(camera->con);
}

int rcpCamera_menuGetChildren(char const *id, int menuNodeId) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_menu_get_children(camera->con, (rcp_menu_node_id_t) menuNodeId);
}

int rcpCamera_menuNodeStatusIsSupported(char const *id) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return rcp_menu_node_status_is_supported(camera->con);
}

int rcpCamera_menuGetNodeStatus(char const *id, int menuNodeId) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_menu_get_node_status(camera->con, (rcp_menu_node_id_t) menuNodeId);
}

int rcpCamera_notificationGet(char const *id) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_notification_get(camera->con);
}

int rcpCamera_notificationTimeout(char const *id, char const *notificationId) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_notification_timeout(camera->con, notificationId);
}

int rcpCamera_notificationResponse(char const *id, char const *notificationId, int response) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return (int) rcp_notification_response(camera->con, notificationId, response);
}

int rcpCamera_getCameraConnectionStats(char const *id, int statsType) {
	int value = 0;
	rcp_camera_connection_stats_t stats;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	rcp_camera_connection_stats(camera->con, &stats);
	switch(statsType) {
	case 0: value = stats.tx_packets; break;
	case 1: value = stats.tx_bytes; break;
	case 2: value = stats.rx_packets; break;
	case 3: value = stats.rx_bytes; break;
	}
	return value;
}

int rcpCamera_getStatus(char const *id, int paramId) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

	return (int)rcp_get_status(camera->con, (rcp_param_t)paramId);
}

int rcpCamera_periodicEnable(char const *id, int paramId, int enable) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}

    if (enable) {
        return (int)rcp_get_periodic_on(camera->con, (rcp_param_t)paramId);
    }
    else {
        return (int)rcp_get_periodic_off(camera->con, (rcp_param_t)paramId);
    }
}

int rcpCamera_rftpGetIsSupported(char const *id) {
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return -1;
	}
	return rcp_rftp_is_supported(camera->con);

}

int rcpCamera_rftpSendFile(char const *id, char const *local_file, char const *cam_file, int compress_file, rcp_uuid_t *uuid) {
	int ret = -1;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return ret;
	}
	
    FILE * f = fopen(local_file, "rb");
    if (f) {
        fseek(f, 0, SEEK_END);
        unsigned long fsize = ftell(f);
        fseek(f, 0, SEEK_SET);

        uint8_t * data = (uint8_t *) malloc(fsize);
        fread(data, fsize, 1, f);
        fclose(f);

        if (compress_file) {
            unsigned long compressedLength = compressBound(fsize);
            uint8_t * compressedData = (uint8_t *) malloc(compressedLength + 12);

            if (Z_OK == compress(compressedData + 12, &compressedLength, data, fsize)) {
                strncpy((char *) compressedData, "REDZLIB0", 8);
                *((uint32_t *) (compressedData + 8)) = fsize;

                ret = (int)rcp_rftp_send_file(camera->con, cam_file, compressedData, fsize, compressedLength + 12, true, uuid);
            }
            else {
                free(compressedData);
            }

            free(data);
        }
        else {
            ret = (int)rcp_rftp_send_file(camera->con, cam_file, data, fsize, fsize, false, uuid);
        }
    }
    else {
    	LOGE("fopen error: %d\n", errno);
    	ret = (int)(-errno);
    }
    return ret;
}

int rcpCamera_rftpRetrieveFile(char const *id, char const *local_file, char const *cam_file, int maxFileSize, int compressionAllowed, rcp_uuid_t *uuid) {
	int ret = -1;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return ret;
	}

	ret = (int)rcp_rftp_retrieve_file(camera->con, cam_file, maxFileSize, compressionAllowed, uuid);
	
	if (ret == RCP_SUCCESS)
		gRftpLocalPaths[uuid->str] = local_file;
	
	return ret;
}

int rcpCamera_rftpDeleteFile(char const *id, char const *cam_file, rcp_uuid_t *uuid) {
	int ret = -1;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return ret;
	}
	
	ret = (int) rcp_rftp_delete_file(camera->con, cam_file, uuid);

	return ret;
}

int rcpCamera_rftpAbortTransfer(char const *id, rcp_uuid_t *uuid) {
	int ret = -1;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return ret;
	}

	ret = (int) rcp_rftp_abort_transfer(camera->con, uuid);

	return ret;
}

int rcpCamera_rftpDirList(char const *id, char const *cam_path, rcp_uuid_t *uuid) {
	int ret = -1;
	RCPCamera_t *camera = gCameraPinMap[id];
	if (camera == NULL) {
		LOGE("rcpCamera: %s: camera == NULL", __func__);
		return ret;
	}

	ret = (int)rcp_rftp_directory_listing(camera->con, cam_path, uuid);

	return ret;
}
