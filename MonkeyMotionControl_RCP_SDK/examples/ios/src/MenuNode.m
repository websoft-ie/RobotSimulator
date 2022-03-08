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

#import "MenuNode.h"
#import "RCPListItem.h"
#import "RCPUtil.h"

@implementation MenuNode

@synthesize type, filter, nodeID, parentID, title, paramID, args, hasPayload, payload, sendInt, sendUInt, sendStr;

- (MenuNode *)init
{
    if (self = [super init])
    {
        type = RCP_MENU_NODE_TYPE_BRANCH;
        filter = RCP_MENU_NODE_FILTER_NONE;
        nodeID = RCP_MENU_NODE_ID_NULL;
        parentID = RCP_MENU_NODE_ID_NULL;
        title = [[NSString alloc] init];
        
        paramID = RCP_PARAM_COUNT;
        args = [[NSMutableArray alloc] init];
        hasPayload = NO;
        payload = 0;
        
        sendInt = NO;
        sendUInt = NO;
        sendStr = NO;
    }
    
    return self;
}

- (MenuNode *)initWithInfo:(const rcp_menu_node_info_t *)info
{
    if (self = [super init])
    {
        type = info->type;
        filter = info->filter;
        nodeID = info->id;
        parentID = info->parent_id;
        title = [RCPUtil decodedStringFromString:info->title];
        
        paramID = info->param_id;
        args = [[NSMutableArray alloc] init];
        hasPayload = info->has_payload;
        payload = info->payload;
        
        sendInt = info->send_int;
        sendUInt = info->send_uint;
        sendStr = info->send_str;
        
        if (info->arg_c_list != NULL)
        {
            c_list_entry_t *cur = info->arg_c_list->head;
            
            while (cur)
            {
                RCPListItem *item = [[RCPListItem alloc] initWithNum:cur->num str:[NSString stringWithFormat:@"%s", cur->str]];
                [args addObject:item];
                
                cur = cur->next;
            }
        }
    }
    
    return self;
}

@end
