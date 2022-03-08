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

import android.support.v4.app.FragmentActivity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

public class MenuTreeBranch extends MenuTreeNode {

	private ImageView mButton;
	
	public MenuTreeBranch (int nodeFilter, int nodeId, int parentId, String title) {
		super (MenuTreeNode.NODE_TYPE_BRANCH, nodeFilter, nodeId, parentId, title, null);
	}

	@Override
	public View onInflateView(FragmentActivity activity, LayoutInflater inflater, int position, ViewGroup parent) {
		View convertView = inflater.inflate(R.layout.list_item_menu_tree_branch, parent, false);
		// Indicates that all UI elements have been initialized in this leaf node and that it is now
		// safe to send/receive get/set commands to the camera
		mTitleView = (TextView) convertView.findViewById(R.id.id_menu_tree_node_title_textview);
		mButton = (ImageView) convertView.findViewById(R.id.id_menu_tree_branch_image);
		mButton.setFocusable(false); // Allow user to click on the entire menu bar
		mButton.setClickable(false);

		notifyViewInflated();
		return convertView;
	}
		
	@Override
	public void setEnabled (boolean enable) {
		// Only do something if a new state is requested
		if ((mEnabled != enable) && (mTitleView != null)) {
			if (enable) {
				mTitleView.setTextColor(DEFAULT_ENABLED_TITLE_COLOR);
				mButton.setImageAlpha(255);
				mButton.setEnabled(true);
			}
			else {
				mTitleView.setTextColor(DEFAULT_DISABLED_TITLE_COLOR);
				mButton.setImageAlpha(128);
				mButton.setEnabled(false);
			}
			mEnabled = enable; 
		}
	}
}
