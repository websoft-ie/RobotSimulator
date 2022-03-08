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

#import <UIKit/UIKit.h>
#import "RCPList.h"
#import "Notification.h"

@interface TabViewController : UITabBarController

+ (TabViewController *)instance;

- (void)setConnected:(BOOL)connected;
- (void)updateParam:(rcp_param_t)paramID integer:(const int)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_int_edit_info_t)editInfo;
- (void)updateParam:(rcp_param_t)paramID uinteger:(const uint32_t)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_uint_edit_info_t)editInfo;
- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status;
- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status infoIsValid:(BOOL)infoIsValid info:(rcp_cur_str_edit_info_t)editInfo;
- (void)updateParam:(rcp_param_t)paramID list:(RCPList *)list;
- (void)updateParam:(rcp_param_t)paramID frameTagInfo:(tag_info_t)tagInfo;
- (void)updateParam:(rcp_param_t)paramID enabled:(BOOL)enabled;
- (void)updateNode:(rcp_menu_node_id_t)nodeID enabled:(BOOL)enabled;
- (void)updateClipList:(rcp_clip_info_list_t *)clipList status:(rcp_clip_list_status_t)status;
- (void)updateHistogram:(const rcp_cur_hist_cb_data_t *)data;
- (void)updateAudioVU:(const rcp_cur_audio_vu_cb_data_t *)data;
- (void)updateMenuTree:(const rcp_cur_menu_cb_data_t *)data;

- (void)showNotificationView:(Notification *)notification;
- (void)updateNotificationView:(Notification *)notification;
- (void)hideNotificationView;

- (void)enableIOSNotifications:(BOOL)enable;

@end
