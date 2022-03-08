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

import java.util.ArrayList;

import android.content.Context;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.red.rcp.RCPCamera;

public class MenuTreeFragment extends CameraUIFragment implements MenuTreeNode.MenuTreeNodeListener {

	private static final String TAG = "MenuTreeFragment";
	
	private static final int MIN_NODE_PARAMETERS = MenuTreeNode.NODE_INDEX_TITLE + 1; // Branch nodes have no param id

	public enum CONTROL_MODE {
		INIT,
		RECORD,
		PLAYBACK,
	}
	private CONTROL_MODE mControlMode = CONTROL_MODE.INIT;
	private MenuTreeAdapter mAdapter;
	private boolean mMenuNodeStatusIsSupported;
	private Context mContext;
	private int mCurrentNodeId;
	private int mParentNodeId;
	private int mRequestedNodeId;

	private ArrayList<MenuTreeNode> mAncestorList;
	private ArrayList<MenuTreeNode> mChildList;

	// UI elements
	private ArrayList<TextView> mTitleBarViewList = new ArrayList<TextView> ();
	private TextView mTitleBarBackButton;
	private ListView mMenuListView;
	private RelativeLayout mTitleBarLayout;

	private OnClickListener mAncestorOnClickListener;
	
	public MenuTreeFragment(RCPCamera camera) {

		super(camera);
    	
		mMenuNodeStatusIsSupported = false;
		mCurrentNodeId = 0;
		mParentNodeId = -1;
		mRequestedNodeId = -1;

		mAncestorList = new ArrayList<MenuTreeNode>();
		mChildList = new ArrayList<MenuTreeNode>();
	}

	@Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState) {

		//Log.v(TAG, "onCreateView");
		ViewGroup rootView = (ViewGroup) inflater.inflate(R.layout.tab_menu_tree, container, false);
		bindUIElements (rootView);
    	mAdapter = new MenuTreeAdapter(super.getActivity());

		mContext = super.getActivity().getApplicationContext();

        return rootView;
    }
	
    @Override
    public void onResume() {
		//Log.v(TAG, "onResume");
    	super.onResume();
    	if (mRCP != null) {
    		if (mRCP.menuIsSupported() == 1) {
    			mRCP.getParameter("RCP_PARAM_PLAYBACK_STATE");
    			mMenuNodeStatusIsSupported = (mRCP.menuNodeStatusIsSupported() != 0);
    			getMenuChildren(0); // Get root node
    		}
    		else {
				Toast toast = Toast.makeText(mContext, "Menu Tree not supported on this camera", Toast.LENGTH_LONG);
				toast.show();
    		}
    	}
    }

	private void bindUIElements (ViewGroup rootView) {
		mTitleBarLayout = (RelativeLayout) rootView.findViewById(R.id.fragment_menu_tree_title_bar);
		mMenuListView = (ListView) rootView.findViewById(R.id.fragment_menu_tree_list_view);
		mTitleBarBackButton = (TextView) rootView.findViewById(R.id.fragment_menu_tree_back_button);
		mTitleBarBackButton.setClickable(true);
		mTitleBarBackButton.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick (View view) {
				handleBackButtonClicked(view);
			}
		});
		mMenuListView.setOnItemClickListener(new OnItemClickListener() {
			@Override
			public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
				handleListItemClicked(parent, view, position, id);
			}
    	});

		mAncestorOnClickListener = new OnClickListener () {
			@Override
			public void onClick (View view) {
				//Log.v(TAG, "onClick ancestor");
				int i = 0;
				TextView tv = (TextView) view;
				int count = mTitleBarViewList.size();
				for (i = 0; i < count; i++) {
					if (tv == mTitleBarViewList.get(i))
						break;
				}
				if (i < count) {
					MenuTreeNode node = mAncestorList.get(count - 1 - i);
					getMenuChildren(node.getNodeId());
				}
				else {
					Log.e(TAG, "onClick cannot find corresponding view");
				}
			}
		};
	}

	private void handleBackButtonClicked (View view) {
		//Log.v(TAG, "handleBackButtonClicked current node " + mCurrentNodeId);
		if (mCurrentNodeId == 0) {
			// Already in root node, exit out of Menu Tree
			// TBD
		}
		else {
			getMenuChildren(mParentNodeId);
		}
	}

	private void handleListItemClicked(AdapterView<?> parent, View view, int position, long id) {
		//Log.v(TAG, "Item clicked " + position + " current node " + mCurrentNodeId);
		
		MenuTreeNode item = mAdapter.getItem(position);
		switch (item.getNodeType()) {
		case MenuTreeNode.NODE_TYPE_BRANCH:
			getMenuChildren(item.getNodeId());
			break;
		case MenuTreeNode.NODE_TYPE_TEXT_LEAF:
		case MenuTreeNode.NODE_TYPE_NUMBER_LEAF:
		case MenuTreeNode.NODE_TYPE_IP_ADDRESS_LEAF:
		case MenuTreeNode.NODE_TYPE_ORDERED_LIST_LEAF:
		case MenuTreeNode.NODE_TYPE_CURVE_LEAF:
		case MenuTreeNode.NODE_TYPE_NOT_YET_SUPPORTED_LEAF:
		case MenuTreeNode.NODE_TYPE_MULTI_ACTION_LIST_LEAF:
			// Handle individual node types here when user clicks on the bar
			break;
		default:
			Log.e(TAG, "unsupported item click");
			break;
		}
	}

	private void getMenuChildren (int nodeID) {
		mRequestedNodeId = nodeID;
		if (mRCP != null) {
			mRCP.menuGetChildren(mRequestedNodeId);
		}
	}
	
	private void createMenuTree (int nodeId, String[] childList, String[] ancestorList) {

		// Check to see if we requested this node, if not, skip. This fixes the problem of when you
		// have multiple tablets connected to the same camera and navigating th menus.
		if ((mRequestedNodeId == -1) && (mRequestedNodeId != nodeId)) {
			return;
		}
		mRequestedNodeId = -1;
		
		// Clean up old stuff
		mAncestorList.clear();
		mChildList.clear();
		mAdapter.clear();
		clearTitleBar();

		if (mRCP != null) {
			createChildMenu(nodeId, childList);
			createAncestorMenu(ancestorList);
			
			mAdapter.addAll(mChildList);
			mMenuListView.setAdapter(mAdapter);
		}
	}

	private void createChildMenu (int nodeId, String[] childNodes) {

		if (childNodes != null) {
			int i = 0;
			int count = childNodes.length;
			
			mCurrentNodeId = nodeId;
			for (i = 0; i < count; i++) {
				createChildNode (childNodes[i], mControlMode);
			}
		}
	}

	private void createAncestorMenu (String[] ancestorNodes) {

		if ( ancestorNodes != null ) {
			int i = 0;
			int count = ancestorNodes.length;
			for (i = 0; i < count; i++) {
				//Log.v(TAG, "ancestor nodes[" + i + "] = " + ancestorNodes[i]);
				createAncestorNode(ancestorNodes[i]);
			}
			// First node is current node. Get its direct parent
			mParentNodeId = mAncestorList.get(0).getParentId();
			populateTitleBar();
		}
	}

	private boolean createAncestorNode (String nodeStr) {
		String [] params = nodeStr.split("\\|");
		
		if (params != null) {
			int count = params.length;
			/*
			for (int j = 0; j < count; j++) {
				Log.v(TAG, "params[" + j + "] = " + params[j]);
			}
			*/
			if (count >= MIN_NODE_PARAMETERS) {

				int nodeType = convertStringToInt(params[MenuTreeNode.NODE_INDEX_TYPE]);
				int nodeFilter = convertStringToInt(params[MenuTreeNode.NODE_INDEX_FILTER]);
				int nodeId = convertStringToInt(params[MenuTreeNode.NODE_INDEX_ID]);
				int parentId = convertStringToInt(params[MenuTreeNode.NODE_INDEX_PARENT_ID]);

				// Must be a branch node
				if (nodeType == MenuTreeNode.NODE_TYPE_BRANCH) {
					params[MenuTreeNode.NODE_INDEX_TITLE] = replaceWithSpecialCharacters(params[MenuTreeNode.NODE_INDEX_TITLE]);
					mAncestorList.add(new MenuTreeBranch (nodeFilter, nodeId, parentId, params[MenuTreeNode.NODE_INDEX_TITLE]));
				}
				else {
					Log.e(TAG, "createAncestorNode invalid node type" + nodeType);
				}
			}
			else {
				Log.e(TAG, "createAncestorNode invalid num params " + count);
			}
		}
		
		return true;
	}
	
	private boolean createChildNode (String nodeStr, CONTROL_MODE mode) {
		boolean result = false;
		String [] params = nodeStr.split("\\|");
		
		if (params != null) {
			int count = params.length;
			/*
			for (int j = 0; j < count; j++) {
				Log.v(TAG, "params[" + j + "] = " + params[j]);
			}
			*/
			if (count >= MIN_NODE_PARAMETERS) {

				int nodeType = convertStringToInt(params[MenuTreeNode.NODE_INDEX_TYPE]);
				int nodeFilter = convertStringToInt(params[MenuTreeNode.NODE_INDEX_FILTER]);
				int nodeId = convertStringToInt(params[MenuTreeNode.NODE_INDEX_ID]);
				int parentId = convertStringToInt(params[MenuTreeNode.NODE_INDEX_PARENT_ID]);
				int paramId = 0;
				String paramName = null;

				// Branches and Not Yet Supported leaves don't have RCP parameters
				// which is params[5]
				if ((nodeType != MenuTreeNode.NODE_TYPE_BRANCH) && (nodeType != MenuTreeNode.NODE_TYPE_NOT_YET_SUPPORTED_LEAF)) {
					if (params.length < 6) {
						// Error
						Log.e(TAG, "Leaf < 6 params!!");
						for (int k = 0; k < params.length; k++) {
							Log.e(TAG, "params[" + k + "]: " + params[k]);
						}
					}
					else {
						paramId = convertStringToInt(params[MenuTreeNode.NODE_INDEX_PARAM_ID]);
						paramName = mRCP.getParameterName(paramId);
					}
				}
				
				if (filterNode(nodeFilter, mode)) {
					params[MenuTreeNode.NODE_INDEX_TITLE] = replaceWithSpecialCharacters(params[MenuTreeNode.NODE_INDEX_TITLE]);

					switch (nodeType) {
					case MenuTreeNode.NODE_TYPE_BRANCH:
						result = createBranch(nodeFilter, nodeId, parentId, params);
						break;
						
					case MenuTreeNode.NODE_TYPE_ACTION_LEAF:
					case MenuTreeNode.NODE_TYPE_CURVE_LEAF:
					case MenuTreeNode.NODE_TYPE_ENABLE_LEAF:
					case MenuTreeNode.NODE_TYPE_IP_ADDRESS_LEAF:
					case MenuTreeNode.NODE_TYPE_LIST_LEAF:
					case MenuTreeNode.NODE_TYPE_NUMBER_LEAF:
					case MenuTreeNode.NODE_TYPE_TEXT_LEAF:
					case MenuTreeNode.NODE_TYPE_ORDERED_LIST_LEAF:
					case MenuTreeNode.NODE_TYPE_DATETIME_LEAF:
					case MenuTreeNode.NODE_TYPE_TIMECODE_LEAF:
					case MenuTreeNode.NODE_TYPE_STATUS_LEAF:
					case MenuTreeNode.NODE_TYPE_MULTI_ACTION_LIST_LEAF:
					case MenuTreeNode.NODE_TYPE_NOT_YET_SUPPORTED_LEAF:
						// Add specific leaf node UI creation here
						break;
						
					default:
						Log.e(TAG, "createMenuTreeNode invalid node type" + nodeType);
						break;
					} // switch
				} // filter node
			} // count >= min parameters
			else {
				Log.e(TAG, "createChildNode invalid num params " + count);
			}
		} // params != null
		
		return result;
	}

	private boolean createBranch (int nodeFilter, int nodeId, int parentId, String[] params) {
		MenuTreeBranch branch = new MenuTreeBranch (nodeFilter, nodeId, parentId, params[MenuTreeNode.NODE_INDEX_TITLE]);
		branch.setSettingsListener(this);
		mChildList.add(branch);
		return true;
	}

	private void populateTitleBar () {
		// Create a text view for each node in the ancestor list.
		// All of them should be clickable except for the current node.
		int id = 1;
		int i = 0, j = 0;
		TextView tv = null;
		int numAncestors = mAncestorList.size();
		int textSize = (int) mContext.getResources().getDimensionPixelSize(R.dimen.menu_tree_fragment_text_size_node);
		int leftPadding = (int) mContext.getResources().getDimensionPixelSize(R.dimen.menu_tree_fragment_text_padding);

		layoutBackButton();
		if (numAncestors > 1)
			mTitleBarBackButton.setText(R.string.text_menu_tree_button_back);
		else
			mTitleBarBackButton.setText(R.string.text_menu_tree_button_close);

		RelativeLayout.LayoutParams[] layoutParams = new RelativeLayout.LayoutParams[numAncestors];
		layoutParams[0] = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
		layoutParams[0].addRule(RelativeLayout.ALIGN_PARENT_LEFT, RelativeLayout.TRUE);
		layoutParams[0].addRule(RelativeLayout.CENTER_VERTICAL, RelativeLayout.TRUE);

		// Start with most distant ancestor and add it to left most of title bar
		// So first item of mTitleBarViewList is actually most distant ancestor.
		tv = new TextView (mContext);
		tv.setId(id++);
		tv.setLayoutParams(layoutParams[0]);
		tv.setGravity(Gravity.LEFT);
		tv.setPadding(leftPadding, 0, 0, 0);
		tv.setTextColor(Color.WHITE);
		tv.setTextSize(TypedValue.COMPLEX_UNIT_PX, textSize);
		tv.setBackground(null);
		tv.setText(mAncestorList.get(numAncestors - 1).getTitle().toString());
		tv.setClickable(true);
		tv.setOnClickListener(mAncestorOnClickListener);
		mTitleBarLayout.addView(tv);
		mTitleBarViewList.add(tv);

		for (i = numAncestors - 2, j = 1; i >= 0; i--, j++) {

			// Layout params go from left to right
			layoutParams[j] = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
			layoutParams[j].addRule(RelativeLayout.RIGHT_OF, mTitleBarViewList.get(j - 1).getId());
			layoutParams[j].addRule(RelativeLayout.CENTER_VERTICAL, RelativeLayout.TRUE);

			tv = new TextView (mContext);
			tv.setId(id++);
			tv.setLayoutParams(layoutParams[j]);
			tv.setGravity(Gravity.LEFT);
			tv.setPadding(0, 0, 0, 0);
			tv.setTextColor(Color.WHITE);
			tv.setTextSize(TypedValue.COMPLEX_UNIT_PX, textSize);
			tv.setBackground(null);
			String text = " > " + mAncestorList.get(i).getTitle();
			tv.setText(text.toString());
			// Last one is current node. No need to respond to clicks to get its children since we are already there
			if (i > 0) {
				tv.setClickable(true);
				tv.setOnClickListener(mAncestorOnClickListener);
			}
			else {
				tv.setClickable(false);
			}
			mTitleBarLayout.addView(tv);
			mTitleBarViewList.add(tv);
		}
	}

	private void layoutBackButton () {
		RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
		layoutParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT, RelativeLayout.TRUE);
		layoutParams.addRule(RelativeLayout.CENTER_VERTICAL, RelativeLayout.TRUE);
		mTitleBarLayout.addView(mTitleBarBackButton);
		
	}
	
	private void clearTitleBar () {
		mTitleBarLayout.removeAllViews();
		mTitleBarViewList.clear();
	}

	private void updateNodeStatus(MenuTreeNode node, int value) {
		if (node.isInflated()) {
			boolean state = (value == 0) ? false : true;
			node.setEnabled(state);
		}
	}

	private void getMenuNodeStatus (MenuTreeNode node) {
		if ((mMenuNodeStatusIsSupported) && (mRCP != null) && (node.getNodeType() == MenuTreeNode.NODE_TYPE_BRANCH)) {
			mRCP.menuNodeGetStatus(node.getNodeId());
		}
	}

	// Returns true if this node should be shown in current mode.
	private boolean filterNode (int nodeFilter, CONTROL_MODE mode) {
		boolean filter = true;
		
		switch (mode) {
		case PLAYBACK:
			if (nodeFilter == MenuTreeNode.NODE_FILTER_RECORD_ONLY)
				filter = false;
			break;
		
		case RECORD:		
			if (nodeFilter == MenuTreeNode.NODE_FILTER_PLAYBACK_ONLY)
				filter = false;
			break;
		default:
			filter = false;
			break;
		}
		return filter;
	}

	private String replaceWithSpecialCharacters (String in) {
		String out = in;
		
		if (in.contains("&reg;")) {
			out = in.replace("&reg;", ((char) 0x00AE)+"");
		}
		else if (in.contains("&reg")) {
			out = in.replace("&reg", ((char) 0x00AE)+"");
		}
		else if (in.contains("(R)")) {
			out = in.replace("(R)", ((char) 0x00AE)+"");
		}
		else if (in.contains("&trade;")) {
			out = in.replace("&trade;", ((char) 0x2122)+"");
		}
		else if (in.contains("(TM)")) {
			out = in.replace("(TM)", ((char) 0x2122)+"");
		}
		else if (in.contains(" deg")) {
			out = in.replace(" deg", ((char) 0x00B0)+"");
		}
		else if (in.contains("&deg;")) {
			out = in.replace("&deg;", ((char) 0x00B0)+"");
		}
		else if (in.contains("&redformatk")) {
			out = in.replace("&redformatk", "K");
		}
		
		return out;
	}

	private int convertStringToInt (String str) {
		int num = 0;
		try {
			num = Integer.parseInt(str);
		}
		catch (NumberFormatException nfe) {
			Log.e(TAG, "Cant convert to int: " + str);
			num = -1;
		}
		return num;
	}

	public void onCameraConnectionChange(int state) {
		if (state == RCPEnum.RCP_CONNECTION_STATE_CONNECTED) {
			mRCPConnected = true;
		}
		else {
			mRCPConnected = false;
			mControlMode = CONTROL_MODE.INIT;
		}
	}

	@Override
	public void onMenuNodeListUpdate(String id, int nodeId, String[] childList, String[] ancestorList) {

		/*
		Log.i(TAG, "onMenuNodeListUpdate: id = " + id + ", nodeId = " + nodeId);
		for (int i = 0; i < childList.length; i++) {
			Log.i(TAG, "child[" + i + "]: " + childList[i]);
		}
		for (int i = 0; i < ancestorList.length; i++) {
			Log.i(TAG, "ancestor[" + i + "]: " + ancestorList[i]);
		}
		*/
		createMenuTree(nodeId, childList, ancestorList);
	}

	@Override
	public void onMenuNodeStatusUpdate(String id, int nodeId, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {
		if (isEnabledValid != 0) {
			for (MenuTreeNode item : mChildList) {
				if ((nodeId == item.getNodeId()) && (item.getNodeType() == MenuTreeNode.NODE_TYPE_BRANCH)) {
					updateNodeStatus(item, isEnabled);
				}
			}
		}
	}

	@Override
	public void onUpdateInt(String id, String param, int value) {
		String[] params = param.split(";");
		String paramName = params[0];

		if (paramName.equals("RCP_PARAM_PLAYBACK_STATE")) {
			//Log.v(TAG, "onUpdateInt. RCP_PARAM_PLAYBACK_STATE value = " + value);
			switch(value) {
			case RCPEnum.SET_PLAYBACK_STATE_STOP:
				// Don't do anything if already in that mode
				mControlMode = CONTROL_MODE.RECORD;
				break;
				
			case RCPEnum.SET_PLAYBACK_STATE_START:
				mControlMode = CONTROL_MODE.PLAYBACK;
				break;
			}
		}
	}

	@Override
	public void onViewInflated(MenuTreeNode node, int position) {
		// This is a callback after the UI elements have been inflated, so now it's safe to get the RCP parameters
		getMenuNodeStatus(node);
		// Here is where you would call
		// mRCP.getParameter(), mRCP.getList(), and mRCP.getstatus() for the leaf RCP parameter
	}
}
