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
#import "IntEditInfo.h"
#import "UIntEditInfo.h"
#import "StrEditInfo.h"
#import "rcp_api.h"

@interface MenuNode : NSObject

@property (assign, nonatomic) rcp_menu_node_type_t type;
@property (assign, nonatomic) rcp_menu_node_filter_t filter;
@property (assign, nonatomic) rcp_menu_node_id_t nodeID;
@property (assign, nonatomic) rcp_menu_node_id_t parentID;
@property (retain, nonatomic) NSString *title;

// The following properties are only valid for leaf nodes
@property (assign, nonatomic) rcp_param_t paramID;
@property (retain, nonatomic) NSMutableArray *args;     // holds RCPListItems

// only valid for RCP_MENU_NODE_TYPE_ACTION_LEAF
@property (assign, nonatomic) BOOL hasPayload;
@property (assign, nonatomic) int payload;

// only valid for RCP_MENU_NODE_TYPE_NUMBER_LEAF and RCP_MENU_NODE_TYPE_LIST_LEAF
@property (assign, nonatomic) BOOL sendInt;
@property (assign, nonatomic) BOOL sendUInt;
@property (assign, nonatomic) BOOL sendStr;


- (MenuNode *)init;
- (MenuNode *)initWithInfo:(const rcp_menu_node_info_t *)info;

@end
