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

#import "RCPHandler.h"
#import "TabViewController.h"
#import "RCPUtil.h"
#import "Logger.h"
#import "clist.h"

@interface RCPHandler()

@property (atomic, assign) rcp_camera_connection_t *rcpConnection;
@property (nonatomic, retain) NSInputStream *inputStream;
@property (nonatomic, retain) NSOutputStream *outputStream;
@property (nonatomic, retain) NSTimer *readTimer;

- (id)init;
- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode;
- (void)setupRCPConnection:(id)obj;
- (void)createRCPConnection:(const rcp_camera_connection_info_t *)info;
- (void)rcpConnectionError;
- (void)externalControlError;

// RCP API callbacks
rcp_error_t sendData(const char *data, size_t len, void *user_data);
void intReceived(const rcp_cur_int_cb_data_t *data, void *user_data);
void uintReceived(const rcp_cur_uint_cb_data_t *data, void *user_data);
void histogramReceived(const rcp_cur_hist_cb_data_t *data, void *user_data);
void listReceived(const rcp_cur_list_cb_data_t *data, void *user_data);
void stringReceived(const rcp_cur_str_cb_data_t *data, void *user_data);
void clipListUpdated(const rcp_cur_clip_list_cb_data_t *data, void *user_data);
void frameTagged(const rcp_cur_tag_info_cb_data_t *data, void *user_data);
void paramStatusUpdated(const rcp_cur_status_cb_data_t *data, void *user_data);
void notificationReceived(const rcp_notification_cb_data_t *data, void *user_data);
void audiovuReceived(const rcp_cur_audio_vu_cb_data_t *data, void *user_data);
void menuTreeUpdated(const rcp_cur_menu_cb_data_t *data, void *user_data);
void menuNodeStatusUpdated(const rcp_cur_menu_node_status_cb_data_t *data, void *user_data);
void rftpStatusUpdated(const rcp_cur_rftp_status_cb_data_t * data, void *user_data);
void connectionStateUpdated(const rcp_state_data_t *data, void *user_data);

@end

@implementation RCPHandler

@synthesize lookDictionary, connectionTimer, connectedCamera, ipAddress, connected, appOutOfDate, paramMajor, paramMinor;

+ (RCPHandler *)instance
{
    static RCPHandler *handler;
    
    @synchronized(self)
    {
        if (!handler)
        {
            handler = [[RCPHandler alloc] init];
        }
        
        return handler;
    }
}

- (void)connectToCameraWithIP:(NSString *)ipAddr
{
    [self dropConnection]; // remove existing connection (if one exists)
    
    ipAddress = ipAddr;
    
    LogInfo("Connecting to %@", ipAddress);
    
    CFReadStreamRef readStream;
    CFWriteStreamRef writeStream;
    CFStreamCreatePairWithSocketToHost(NULL, (__bridge CFStringRef)ipAddress, TCP_PORT, &readStream, &writeStream);
    _inputStream = (__bridge NSInputStream *)readStream;
    _outputStream = (__bridge NSOutputStream *)writeStream;
    
    [_inputStream setDelegate:self];
    [_outputStream setDelegate:self];
    
    [_inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    [_outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
    
    [_inputStream open];
    [_outputStream open];
    
    [NSThread detachNewThreadSelector:@selector(setupRCPConnection:) toTarget:self withObject:self];
    connectionTimer = [NSTimer scheduledTimerWithTimeInterval:CONNECTION_ERROR_TIMEOUT_S target:self selector:@selector(rcpConnectionError) userInfo:nil repeats:NO];
}

- (void)dropConnection
{
    LogInfo("Dropping connection to %@", connectedCamera.ipAddress);
    
    [_readTimer invalidate];
    [connectionTimer invalidate];
    
    _readTimer = nil;
    connectionTimer = nil;
    
    [connectedCamera clear];
    ipAddress = @"";
    connected = NO;
    appOutOfDate = NO;
    paramMajor = -1;
    paramMinor = -1;
    
    if (_rcpConnection != NULL)
    {
        rcp_delete_camera_connection(_rcpConnection);
        _rcpConnection = NULL;
    }
    
    if (_inputStream != nil)
    {
        [_inputStream close];
        _inputStream = nil;
    }
    
    if (_outputStream != nil)
    {
        [_outputStream close];
        _outputStream = nil;
    }
    
    TabViewController *tabVC = [TabViewController instance];
    [tabVC setConnected:NO];
}

- (rcp_error_t)sendRCP:(const char *)data withLength:(size_t)len
{
    NSData *nsdata = [[NSData alloc] initWithBytes:data length:len];
    
    if ([_outputStream write:(const uint8_t*)[nsdata bytes] maxLength:[nsdata length]] <= 0)
    {
        LogError("Failed to write to output stream");
        return RCP_ERROR_SEND_DATA_TO_CAM_FAILED;
    }
    
    return RCP_SUCCESS;
}

- (void)rcpGet:(rcp_param_t)paramID
{
    rcp_get(_rcpConnection, paramID);
}

// This method is meant to be called from a selector of a NSTimer since there are times when it makes sense to wait before doing a GET
// (i.e. right after setting a repeated value to ensure that the value is valid and that the CURRENT response from the GET came after the one from the SET (if there even was one))
- (void)rcpGetDelayed:(NSTimer *)timer
{
    NSNumber *param = [timer userInfo];
    rcp_param_t paramID = (rcp_param_t)[param intValue];
    
    rcp_get(_rcpConnection, paramID);
}

- (void)rcpGetList:(rcp_param_t)paramID
{
    rcp_get_list(_rcpConnection, paramID);
}

- (void)rcpGetStatus:(rcp_param_t)paramID
{
    rcp_get_status(_rcpConnection, paramID);
}

- (void)rcpGetMenu:(rcp_menu_node_id_t)nodeID
{
    rcp_menu_get_children(_rcpConnection, nodeID);
}

- (void)rcpGetMenuNodeStatus:(rcp_menu_action_node_t)nodeID
{
    rcp_menu_get_node_status(_rcpConnection, nodeID);
}

- (NSString *)rcpGetLabel:(rcp_param_t)paramID
{
    return [NSString stringWithFormat:@"%s", rcp_get_label(_rcpConnection, paramID)];
}

- (BOOL)rcpGetIsSupported:(rcp_param_t)paramID
{
    return rcp_get_is_supported(_rcpConnection, paramID, NULL);
}

- (rcp_param_properties_t)rcpGetProperties:(rcp_param_t)paramID
{
    rcp_param_properties_t properties;
    
    rcp_get_is_supported(_rcpConnection, paramID, &properties);
    
    return properties;
}

- (BOOL)rcpMenuIsSupported
{
    return rcp_menu_is_supported(_rcpConnection);
}

- (BOOL)rcpMenuNodeStatusIsSupported
{
    return rcp_menu_node_status_is_supported(_rcpConnection);
}

- (void)rcpSend:(rcp_param_t)paramID
{
    rcp_send(_rcpConnection, paramID);
}

- (void)rcpSetInt:(rcp_param_t)paramID value:(const int)value
{
    rcp_set_int(_rcpConnection, paramID, value);
}

- (void)rcpSetUInt:(rcp_param_t)paramID value:(const uint32_t)value
{
    rcp_set_uint(_rcpConnection, paramID, value);
}

- (void)rcpSetString:(rcp_param_t)paramID value:(NSString *)value
{
    rcp_set_str(_rcpConnection, paramID, [value UTF8String]);
}

- (void)rcpSetList:(RCPList *)rcpList
{
    int i;
    const NSUInteger count = [rcpList count];
    cList *clist = new cList;
    
    for (i = 0; i < count; i++)
    {
        RCPListItem *item = [rcpList itemAtIndex:i];
        cList::Entry entry;
        
        entry.num = item.num;
        strncpy(entry.str, [item.str UTF8String], sizeof(entry.str) / sizeof(char));
        
        clist->append(entry);
    }
    
    char listString[4096];
    if (clist->exportStringList(listString, 4096) == cList::SUCCESS)
    {
        if (rcp_get_is_supported(_rcpConnection, rcpList.paramID, NULL))
        {
            rcp_set_list(_rcpConnection, rcpList.paramID, listString);
        }
    }
    
    delete clist;
}

- (rcp_error_t)rcpNotificationTimeout:(const char *)notificationID
{
    return rcp_notification_timeout(_rcpConnection, notificationID);
}

- (rcp_error_t)rcpNotificationResponse:(const char *)notificationID response:(int32_t)response
{
    return rcp_notification_response(_rcpConnection, notificationID, response);
}

- (void)loadListData:(const rcp_cur_list_cb_data_t *)data
{
    cList *list = new cList;
    
    if (list->importStringListAndDecode(data->list_string) == cList::SUCCESS)
    {
        RCPList *rcpList = [[RCPList alloc] init];
        size_t i, currentRow;
        size_t length = list->length();
        
        list->getIndex(currentRow);
        
        
        rcpList.paramID = data->id;
        rcpList.currentRow = currentRow;
        rcpList.minValue = data->min_val;
        rcpList.maxValue = data->max_val;
        rcpList.minValueValid = data->min_val_valid;
        rcpList.maxValueValid = data->max_val_valid;
        rcpList.updateOnlyOnClose = data->update_list_only_on_close;
        
        if (data->send_int)
        {
            rcpList.sendValueType = SEND_VALUE_TYPE_INT;
        }
        else if (data->send_uint)
        {
            rcpList.sendValueType = SEND_VALUE_TYPE_UINT;
        }
        else if (data->send_str)
        {
            rcpList.sendValueType = SEND_VALUE_TYPE_STR;
        }
        
        
        for (i = 0; i < length; i++)
        {
            cList::Entry entry;
            
            if (list->getData(i, entry) == cList::SUCCESS)
            {
                RCPListItem *item = [[RCPListItem alloc] initWithNum:entry.num str:[NSString stringWithFormat:@"%s", entry.str]];
                [rcpList addItem:item];
            }
        }
        
        TabViewController *tabVC = [TabViewController instance];
        [tabVC updateParam:data->id list:rcpList];
        
        if (data->display_str_in_list)
        {
            cList::Entry entry;
            if (list->getData(currentRow, entry) == cList::SUCCESS)
            {
                [tabVC updateParam:data->id string:entry.str status:RCP_PARAM_DISPLAY_STATUS_NORMAL];
            }
        }
    }
    else
    {
        LogError("Failed to import list from %s", data->list_string);
    }
    
    delete list;
}

- (id)init
{
    if (self = [super init])
    {
        lookDictionary = [[NSMutableDictionary alloc] init];
        connectionTimer = nil;
        connectedCamera = [[Camera alloc] init];
        ipAddress = [[NSString alloc] init];
        connected = NO;
        appOutOfDate = NO;
        paramMajor = -1;
        paramMinor = -1;
        _rcpConnection = NULL;
        _inputStream = nil;
        _outputStream = nil;
        _readTimer = nil;
    }
    
    return self;
}

- (void)stream:(NSStream *)aStream handleEvent:(NSStreamEvent)eventCode
{
    switch (eventCode)
    {
        case NSStreamEventNone:
            // No stream event occurred
            break;
            
        case NSStreamEventOpenCompleted:
            LogInfo("%@ opened", (aStream == _inputStream) ? @"Input stream" : @"Output stream");
            break;
            
        case NSStreamEventHasBytesAvailable:
            // Reset the read timer
            [_readTimer invalidate];
            _readTimer = [NSTimer scheduledTimerWithTimeInterval:READ_TIMEOUT_S target:self selector:@selector(dropConnection) userInfo:nil repeats:NO];
            
            // Read the data
            if (aStream == _inputStream)
            {
                uint8_t buf_uint8[1024];
                char buf_char[1024];
                NSInteger len;
                
                while ([_inputStream hasBytesAvailable])
                {
                    len = [_inputStream read:buf_uint8 maxLength:sizeof(buf_uint8)];
                    if (len > 0)
                    {
                        memcpy(buf_char, buf_uint8, len);
                        
                        if (_rcpConnection)
                        {
                            rcp_process_data(_rcpConnection, buf_char, len);
                        }
                    }
                }
            }
            break;
            
        case NSStreamEventHasSpaceAvailable:
            // The stream can accept bytes for writing
            break;
            
        case NSStreamEventErrorOccurred:
            LogError("An error has occurred on the %@ stream", (aStream == _inputStream) ? @"input" : @"output");
            break;
            
        case NSStreamEventEndEncountered:
            LogError("The end of the %@ stream has been reached", (aStream == _inputStream) ? @"input" : @"output");
            break;
            
        default:
            break;
    }
}

- (void)setupRCPConnection:(id)obj
{
    LogInfo("Initializing RCP Connection");
    NSString * appVersion = (NSString *)[[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleShortVersionString"];
    
    rcp_camera_connection_info_t info =
    {
        "RCP Demo iOS",
        [appVersion UTF8String],
        NULL,
        sendData, (__bridge void*)self,
        intReceived, (__bridge void*)self,
        uintReceived, (__bridge void*)self,
        listReceived, (__bridge void*)self,
        histogramReceived, (__bridge void*)self,
        stringReceived, (__bridge void*)self,
        clipListUpdated, (__bridge void*)self,
        frameTagged, (__bridge void*)self,
        paramStatusUpdated, (__bridge void*)self,
        notificationReceived, (__bridge void*)self,
        audiovuReceived, (__bridge void*)self,
        menuTreeUpdated, (__bridge void*)self,
        menuNodeStatusUpdated, (__bridge void*)self,
        rftpStatusUpdated, (__bridge void*)self,
        NULL, NULL, /* User set */
        NULL, NULL, /* User get */
        NULL, NULL, /* User current */
        NULL, NULL, /* User metadata */
        NULL, NULL, /* Default int */
        NULL, NULL, /* Default uint */
        NULL, NULL, /* Key mapping action list */
        NULL, NULL, /* Current key mapping */
        connectionStateUpdated, (__bridge void*)self
    };
    
    RCPHandler *handler = (RCPHandler *)obj; // refers to "self"
    [handler createRCPConnection:&info];
}

- (void)createRCPConnection:(const rcp_camera_connection_info_t *)info
{
    // The reason this is in a separate function is to avoid having to expose _rcpConnection as a public property since it needs to be created in its own thread due to the blocking nature of rcp_create_camera_connection()
    // Having it in its own thread allows a reasonable timeout to be in place if the connection fails
    _rcpConnection = rcp_create_camera_connection(info);
}

// This method is called if the RCP connection state is not set to RCP_CONNECTION_STATE_CONNECTED in the connectionStateUpdated callback
- (void)rcpConnectionError
{
    LogInfo("An error occurred while trying to connect to %@", connectedCamera.ipAddress);
    
    [self dropConnection];
    [RCPUtil showDialogWithTitle:@"Connection Error" message:@"Please ensure that the specified IP address belongs to your camera."];
}

// This method is called if the camera does not receive a non-connection-related RCP message (such as record status) upon establishing a RCP connection, implying that External Control on the camera is disabled
- (void)externalControlError
{
    LogError("External control is disabled");
    
    [self dropConnection];
    [RCPUtil showDialogWithTitle:@"Connection Error" message:@"Please ensure that external control is enabled on your camera.\nMenu > Settings > Setup > Communication > Ethernet"];
}

rcp_error_t sendData(const char *data, size_t len, void *user_data)
{
    RCPHandler *handler = (__bridge RCPHandler*)user_data; // refers to "self"
    return [handler sendRCP:data withLength:len];
}

void intReceived(const rcp_cur_int_cb_data_t *data, void *user_data)
{
    RCPHandler *handler = (__bridge RCPHandler*)user_data; // refers to "self"
    TabViewController *tabVC = [TabViewController instance];
    
    if (data->cur_val_valid)
    {
        // RECORD_STATE is used here to verify if external control is enabled; this method would not be entered with the RECORD_STATE parameter if it was disabled
        if (!handler.connected && data->id == RCP_PARAM_RECORD_STATE)
        {
            [handler.connectionTimer invalidate];
            handler.connectionTimer = nil;
            
            handler.connected = YES;
            
            if (handler.appOutOfDate)
            {
                [RCPUtil showDialogWithTitle:@"Warning" message:@"This version of the application is not fully supported by your camera's firmware. The app will still work but there may be features on the camera that the app cannot support."];
            }
            
            [tabVC setConnected:YES];
        }
        
        [tabVC updateParam:data->id integer:data->cur_val infoIsValid:data->edit_info_valid info:data->edit_info];
    }
    
    if (data->display_str_valid)
    {
        [tabVC updateParam:data->id string:data->display_str_decoded status:data->display_str_status];
    }
    
    if (data->display_str_in_list)
    {
        [handler rcpGetList:data->id];
    }
}

void uintReceived(const rcp_cur_uint_cb_data_t *data, void *user_data)
{
    RCPHandler *handler = (__bridge RCPHandler*)user_data; // refers to "self"
    TabViewController *tabVC = [TabViewController instance];
    
    if (data->cur_val_valid)
    {
        [tabVC updateParam:data->id uinteger:data->cur_val infoIsValid:data->edit_info_valid info:data->edit_info];
    }
    
    if (data->display_str_valid)
    {
        [tabVC updateParam:data->id  string:data->display_str_decoded status:data->display_str_status];
    }
    
    if (data->display_str_in_list)
    {
        [handler rcpGetList:data->id];
    }
}

void histogramReceived(const rcp_cur_hist_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateHistogram:data];
}

void listReceived(const rcp_cur_list_cb_data_t *data, void *user_data)
{
    RCPHandler *handler = (__bridge RCPHandler*)user_data; // refers to "self"
    
    if (data->list_string_valid)
    {
        [handler loadListData:data];
    }
}

void stringReceived(const rcp_cur_str_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateParam:data->id string:data->display_str_decoded status:data->display_str_status infoIsValid:data->edit_info_valid info:data->edit_info];
}

void clipListUpdated(const rcp_cur_clip_list_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateClipList:data->clip_list status:data->clip_list_status];
}

void frameTagged(const rcp_cur_tag_info_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateParam:data->id frameTagInfo:data->tag_info];
}

void paramStatusUpdated(const rcp_cur_status_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    const BOOL enabled = (data->is_supported_valid ? data->is_supported : YES) && (data->is_enabled_valid ? data->is_enabled : YES);
    
    [tabVC updateParam:data->id enabled:enabled];
}

void notificationReceived(const rcp_notification_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    
    if (data->action == NOTIFICATION_ACTION_OPEN || data->action == NOTIFICATION_ACTION_UPDATE)
    {
        Notification *notification = [[Notification alloc] init];
        
        notification.notificationID = [NSString stringWithFormat:@"%s", data->notification->id];
        notification.title = [NSString stringWithFormat:@"%s", data->notification->title];
        notification.message = [NSString stringWithFormat:@"%s", data->notification->message];
        notification.progressType = data->notification->progress_type;
        notification.progressPercent = data->notification->progress_percent;
        notification.timeout = data->notification->timeout;
        notification.type = data->notification->type;
        
        cList *responses = new cList;
        
        if (responses->importStringListAndDecode(data->notification->response_list) == cList::SUCCESS)
        {
            size_t i;
            size_t length = responses->length();
            
            for (i = 0; i < length; i++)
            {
                cList::Entry entry;
                
                if (responses->getData(i, entry) == cList::SUCCESS)
                {
                    [notification addButtonWithName:[NSString stringWithFormat:@"%s", entry.str] value:entry.num];
                }
            }
        }
        else
        {
            LogError("Failed to import responses list from %s", data->notification->response_list);
        }
        
        delete responses;
        
        if (data->action == NOTIFICATION_ACTION_OPEN)
        {
            [tabVC showNotificationView:notification];
        }
        else
        {
            [tabVC updateNotificationView:notification];
        }
    }
    else if (data->action == NOTIFICATION_ACTION_CLOSE)
    {
        [tabVC hideNotificationView];
    }
}

void audiovuReceived(const rcp_cur_audio_vu_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateAudioVU:data];
}

void menuTreeUpdated(const rcp_cur_menu_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    [tabVC updateMenuTree:data];
}

void menuNodeStatusUpdated(const rcp_cur_menu_node_status_cb_data_t *data, void *user_data)
{
    TabViewController *tabVC = [TabViewController instance];
    const BOOL enabled = (data->is_supported_valid ? data->is_supported : YES) && (data->is_enabled_valid ? data->is_enabled : YES);
    
    [tabVC updateNode:data->id enabled:enabled];
}

void rftpStatusUpdated(const rcp_cur_rftp_status_cb_data_t * data, void *user_data)
{
    /* Handles file transfer information */
}

void connectionStateUpdated(const rcp_state_data_t *data, void *user_data)
{
    RCPHandler *handler = (__bridge RCPHandler*)user_data; // refers to "self"
    
    switch (data->state)
    {
        case RCP_CONNECTION_STATE_INIT:
        case RCP_CONNECTION_STATE_GET_REQUIRED_PARAMS:
            // Nothing to do
            break;
            
        case RCP_CONNECTION_STATE_CONNECTED:
            LogInfo("RCP connection established");
            
            [handler.connectionTimer invalidate];
            handler.connectionTimer = nil;
            
            handler.appOutOfDate = data->parameter_set_version_valid && data->parameter_set_newer;
            
            handler.paramMajor = data->parameter_set_version_major;
            handler.paramMinor = data->parameter_set_version_minor;
            
            handler.connectedCamera.cameraID = [NSString stringWithFormat:@"%s", data->cam_info->id];
            handler.connectedCamera.pin = [NSString stringWithFormat:@"%s", data->cam_info->pin];
            handler.connectedCamera.type = [NSString stringWithFormat:@"%s", data->cam_info->type];
            handler.connectedCamera.version = [NSString stringWithFormat:@"%s", data->cam_info->version];
            handler.connectedCamera.ipAddress = handler.ipAddress;
            handler.connectedCamera.interface = data->cam_info->rcp_interface;
            
            [handler rcpGet:RCP_PARAM_RECORD_STATE]; // used to check if external control is enabled
            
            if (handler.connectedCamera.interface == RCP_INTERFACE_BRAIN_GIGABIT_ETHERNET)
            {
                handler.connectionTimer = [NSTimer scheduledTimerWithTimeInterval:CONNECTION_ERROR_TIMEOUT_S target:handler selector:@selector(externalControlError) userInfo:nil repeats:NO];
            }
            break;
            
        case RCP_CONNECTION_STATE_ERROR_RCP_VERSION_MISMATCH:
            LogError("The RCP version is invalid");
            [handler dropConnection];
            [RCPUtil showDialogWithTitle:@"Connection Error" message:@"The camera's RCP version is invalid."];
            break;
            
        case RCP_CONNECTION_STATE_ERROR_RCP_PARAMETER_SET_VERSION_MISMATCH:
            LogError("The RCP parameter set version is invalid");
            [handler dropConnection];
            [RCPUtil showDialogWithTitle:@"Connection Error" message:@"The camera's RCP parameter set version is invalid."];
            break;
            
        case RCP_CONNECTION_STATE_RCP_DISABLED_ON_INTERFACE:
            LogError("External control is disabled");
            [handler dropConnection];
            [RCPUtil showDialogWithTitle:@"Connection Error" message:@"Please ensure that external control is enabled on your camera.\nMenu > Settings > Setup > Communication > Ethernet"];
            break;
            
        case RCP_CONNECTION_STATE_COMMUNICATION_ERROR:
            LogError("Communication with camera was lost");
            [handler dropConnection];
            [RCPUtil showDialogWithTitle:@"Connection Error" message:@"Communication with your camera was lost."];
            break;
            
        default:
            break;
    }
}

@end
