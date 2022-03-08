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

import android.graphics.Color;
import android.support.v4.app.FragmentActivity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

public abstract class MenuTreeNode {

	protected static final String TAG = "MenuTreeNode";
	protected int mPosition; // Set in the subclass' onInflateView() method
	
	/* The following matches
		typedef enum {
		    RCP_MENU_NODE_TYPE_BRANCH,
		    RCP_MENU_NODE_TYPE_ACTION_LEAF,
		    RCP_MENU_NODE_TYPE_CURVE_LEAF,
		    RCP_MENU_NODE_TYPE_ENABLE_LEAF,
		    RCP_MENU_NODE_TYPE_IP_ADDRESS_LEAF,
		    RCP_MENU_NODE_TYPE_LIST_LEAF,
		    RCP_MENU_NODE_TYPE_NUMBER_LEAF,
		    RCP_MENU_NODE_TYPE_TEXT_LEAF,
		    RCP_MENU_NODE_TYPE_ORDERED_LIST_LEAF,
		    RCP_MENU_NODE_TYPE_DATETIME_LEAF,
		    RCP_MENU_NODE_TYPE_TIMECODE_LEAF,
		    RCP_MENU_NODE_TYPE_STATUS_LEAF
		} rcp_menu_node_type_t;
	 */
	public static final int NODE_TYPE_BRANCH 					= 0;
	public static final int NODE_TYPE_ACTION_LEAF 				= 1;
	public static final int NODE_TYPE_CURVE_LEAF 				= 2;
	public static final int NODE_TYPE_ENABLE_LEAF 				= 3;
	public static final int NODE_TYPE_IP_ADDRESS_LEAF 			= 4;
	public static final int NODE_TYPE_LIST_LEAF 				= 5;
	public static final int NODE_TYPE_NUMBER_LEAF 				= 6;
	public static final int NODE_TYPE_TEXT_LEAF 				= 7;
	public static final int NODE_TYPE_ORDERED_LIST_LEAF 		= 8;
	public static final int NODE_TYPE_DATETIME_LEAF 			= 9;
    public static final int NODE_TYPE_TIMECODE_LEAF				= 10;
	public static final int NODE_TYPE_STATUS_LEAF 				= 11;
    public static final int NODE_TYPE_MULTI_ACTION_LIST_LEAF 	= 12;
    public static final int NODE_TYPE_NOT_YET_SUPPORTED_LEAF	= 13;	
	public static final int NODE_TYPE_MAX 						= 14; // should be last

    /*
	typedef enum {
    	RCP_MENU_NODE_FILTER_NONE = 0,
    	RCP_MENU_NODE_FILTER_RECORD_ONLY = 1,
    	RCP_MENU_NODE_FILTER_PLAYBACK_ONLY = 2,
    	RCP_MENU_NODE_FILTER_RECORD_AND_PLAYBACK = RCP_MENU_NODE_FILTER_RECORD_ONLY | RCP_MENU_NODE_FILTER_PLAYBACK_ONLY
	} rcp_menu_node_filter_t;
	 */
	public static final int NODE_FILTER_NONE = 0;
	public static final int NODE_FILTER_RECORD_ONLY = 1;
	public static final int NODE_FILTER_PLAYBACK_ONLY = 2;
	public static final int NODE_FILTER_RECORD_AND_PLAYBACK = 3;


	/*
	 * From this line in rcp_camera.cpp in the RCP Java SDK
	 * 			offset = snprintf(pCurrSegment, NODE_DESCRIPTOR_SIZE, "%d|%d|%d|%d|%d|%d|%d|%d|%s", node->type, node->filter, node->id, node->parent_id,
	 *					node->is_enabled, node->is_enabled_valid, node->is_supported, node->is_supported_valid, node->title);
	 * 
	 * These are the indices into the array of strings describing the menu node passed into onMenuNodeListUpdate
     *
	 */
	public static final int NODE_INDEX_TYPE				= 0;
	public static final int NODE_INDEX_FILTER			= 1;
	public static final int NODE_INDEX_ID				= 2;
	public static final int NODE_INDEX_PARENT_ID		= 3;
	public static final int NODE_INDEX_ENABLED			= 4;
	public static final int NODE_INDEX_ENABLED_VALID	= 5;
	public static final int NODE_INDEX_SUPPORTED		= 6;
	public static final int NODE_INDEX_SUPPORTED_VALID	= 7;
	public static final int NODE_INDEX_TITLE			= 8;
	public static final int NODE_INDEX_PARAM_ID			= 9;

	protected int mNodeType;
	protected int mNodeFilter;
	protected int mNodeId;
	protected int mNodeParentId;
	protected String mTitle;
	protected boolean mEnabled;
	protected boolean mViewInflated;
	//protected String mParamName;
	protected MenuTreeNodeListener mDelegateListener;
	protected TextView mTitleView;
	//protected RCPParam mRcpParam;
	protected int mDisplayStrColor;
	protected static final int DEFAULT_ENABLED_TITLE_COLOR = Color.WHITE;
	protected static final int DEFAULT_DISABLED_TITLE_COLOR = Color.GRAY;
	
	public MenuTreeNode (int nodeType, int nodeFilter, int nodeId, int parentId, String title, String paramName) {
		this.mNodeType = nodeType;
		this.mNodeFilter = nodeFilter;
		this.mNodeId = nodeId;
		this.mNodeParentId = parentId;
		this.mTitle = title;
		this.mEnabled = true;
		this.mViewInflated = false;
		//this.mParamName = paramName;
		//this.mRcpParam = new RCPParam(paramName);
		this.mDisplayStrColor = Color.WHITE;
	}
	
	public interface MenuTreeNodeListener {
		public void onViewInflated (MenuTreeNode node, int position);
	};
	
	public void setSettingsListener (MenuTreeNodeListener delegateListener) {
		mDelegateListener = delegateListener;
	}

	public int getNodeType () {
		return mNodeType;
	}

	public int getNodeId () {
		return mNodeId;
	}

	public int getParentId () {
		return mNodeParentId;
	}

	public boolean isEnabled () {
		return mEnabled;
	}

	public String getTitle () {
		return mTitle;
	}
	
	public void setEnabled (boolean enable) {
		// Only do something if a new state is requested
		if ((mEnabled != enable) && (mTitleView != null)) {
			if (enable) {
				mTitleView.setTextColor(DEFAULT_ENABLED_TITLE_COLOR);
			}
			else {
				mTitleView.setTextColor(DEFAULT_DISABLED_TITLE_COLOR);
			}
			mEnabled = enable; 
		}
	}
	
	public boolean isInflated () {
		return this.mViewInflated;
	}
	
	public void handleNodeSelected () {
		// Does nothing. Override to allow users to press the entire menu bar to select item
	}

	public abstract View onInflateView (FragmentActivity activity, LayoutInflater inflater, int position, ViewGroup parent);
	
	protected void notifyViewInflated () {
		// UI is always enabled right after inflating.
		mEnabled = true;
		if (mDelegateListener != null) {
			mViewInflated = true;
			mDelegateListener.onViewInflated(this, mPosition);
		}
	}

}
