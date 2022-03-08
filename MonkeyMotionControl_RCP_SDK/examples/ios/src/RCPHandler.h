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

#import <Foundation/Foundation.h>
#import "RCPList.h"
#import "Camera.h"
#import "rcp_api.h"

@interface RCPHandler : NSObject<NSStreamDelegate>

@property (nonatomic, retain) NSMutableDictionary *lookDictionary; // key = rcp_param_t, value = string
@property (nonatomic, retain) NSTimer *connectionTimer;
@property (nonatomic, retain) Camera *connectedCamera;
@property (nonatomic, retain) NSString *ipAddress;
@property (nonatomic, assign) BOOL connected;
@property (nonatomic, assign) BOOL appOutOfDate;
@property (nonatomic, assign) int paramMajor;
@property (nonatomic, assign) int paramMinor;

+ (RCPHandler *)instance;
- (void)connectToCameraWithIP:(NSString *)ipAddr;
- (void)dropConnection;
- (rcp_error_t)sendRCP:(const char *)data withLength:(size_t)len;
- (void)rcpGet:(rcp_param_t)paramID;
- (void)rcpGetDelayed:(NSTimer *)timer;
- (void)rcpGetList:(rcp_param_t)paramID;
- (void)rcpGetStatus:(rcp_param_t)paramID;
- (void)rcpGetMenu:(rcp_menu_node_id_t)nodeID;
- (void)rcpGetMenuNodeStatus:(rcp_menu_action_node_t)nodeID;
- (NSString *)rcpGetLabel:(rcp_param_t)paramID;
- (BOOL)rcpGetIsSupported:(rcp_param_t)paramID;
- (rcp_param_properties_t)rcpGetProperties:(rcp_param_t)paramID;
- (BOOL)rcpMenuIsSupported;
- (BOOL)rcpMenuNodeStatusIsSupported;
- (void)rcpSend:(rcp_param_t)paramID;
- (void)rcpSetInt:(rcp_param_t)paramID value:(const int)value;
- (void)rcpSetUInt:(rcp_param_t)paramID value:(const uint32_t)value;
- (void)rcpSetString:(rcp_param_t)paramID value:(NSString *)value;
- (void)rcpSetList:(RCPList *)rcpList;
- (rcp_error_t)rcpNotificationTimeout:(const char *)notificationID;
- (rcp_error_t)rcpNotificationResponse:(const char *)notificationID response:(int32_t)response;
- (void)loadListData:(const rcp_cur_list_cb_data_t *)data;

@end
