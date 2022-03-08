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

package com.red.redcontrol;

public final class RCPEnum {

	// From rcp_connection_state_t in rcp_api.h
	public static final int RCP_CONNECTION_STATE_INIT = 0;
	public static final int RCP_CONNECTION_STATE_GET_REQUIRED_PARAMS = 1;
	public static final int RCP_CONNECTION_STATE_CONNECTED = 2;
	public static final int RCP_CONNECTION_STATE_ERROR_RCP_VERSION_MISMATCH = 3;
	public static final int RCP_CONNECTION_STATE_ERROR_RCP_PARAMETER_SET_VERSION_MISMATCH = 4;
	public static final int RCP_CONNECTION_STATE_COMMUNICATION_ERROR = 5;
	
	// From rcp_clip_list_status_t in rcp_api.h
	public static final int CLIP_LIST_LOADING = 0;
	public static final int CLIP_LIST_DONE = 1;
	public static final int CLIP_LIST_BLOCKED = 2;
	
	// From types/rcp_types_public.h
	public static final int KEY_ACTION_MARK_SNAPSHOT = 137;
	
	public static final int SET_PLAYBACK_STATE_STOP = 0;
	public static final int SET_PLAYBACK_STATE_START = 1;
	public static final int SET_PLAYBACK_STATE_TOGGLE = 2;
	
	public static final int KEY_ACTION_NEXT_TAGGED_FRAME = 147;
	public static final int KEY_ACTION_PREV_TAGGED_FRAME = 148;

    public static final long MULTI_ACTION_LIST_LEAF_PROPERTY_CLOSE_ON_ACTION = 0x10000;
}
