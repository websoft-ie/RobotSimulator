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

import android.app.ActionBar;
import android.app.AlertDialog;
import android.app.FragmentTransaction;
import android.app.ActionBar.Tab;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v4.app.Fragment;
import android.support.v4.app.FragmentActivity;
import android.support.v4.view.PagerAdapter;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.content.pm.PackageManager.NameNotFoundException;

import com.red.rcp.RCPCameraListener;
import com.red.rcp.RCPCamera;

public class ControllerActivity extends FragmentActivity implements RCPCameraListener {

	private final static String TAG = "ControllerActivity";
	private final static int TIMER_DELAY = 10000;  // 10 seconds

	// The pager widget that handles animation and allows swiping horizontally to 
	// access previous and next control panels
	private ViewPager mPager;
	
	// The pager adapter that provides the pages to the view pager widget
	private PagerAdapter mPagerAdapter;
	
	// These are common and shared between the child fragments
	private RCPCamera mRCPCamera;
	private Camera mCamera;
	
	private HomeFragment homeFragment;
	private MediaClipListFragment mediaClipListFragment;
	private MenuTreeFragment menuTreeFragment;
	
	private Fragment[] mFragments;
	private int[] mFragmentStringIds = {
			R.string.tab_label_home,
			R.string.tab_label_media,
			R.string.tab_label_menu
	};

	private Handler mHandler;
	private AlertDialog.Builder mAlertBuilder;
	private AlertDialog mAlert;
	private boolean mRCPConnected;
	private RCPStateData mLastStateData;
	private int mCurrentMessageCount;

	public ControllerActivity () {
		
		mRCPCamera = new RCPCamera(this);

		homeFragment = new HomeFragment(mRCPCamera);
		mediaClipListFragment = new MediaClipListFragment(mRCPCamera);
		menuTreeFragment = new MenuTreeFragment(mRCPCamera);

		mFragments = new Fragment[3];
		mFragments[0] = homeFragment;
		mFragments[1] = mediaClipListFragment;
		mFragments[2] = menuTreeFragment;

		mRCPConnected = false;
		mCurrentMessageCount = 0;
	}
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		final ActionBar actionBar = getActionBar();
		
		// Specify that tabs should be displayed in the action bar.
		actionBar.setNavigationMode(ActionBar.NAVIGATION_MODE_TABS);
		
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_controller);
		
		// Instantiate a ViewPager and a PagerAdapter
		mPager = (ViewPager) findViewById(R.id.pager);
		mPagerAdapter = new ControllerTabPagerAdapter(getSupportFragmentManager(), mFragments);
		mPager.setAdapter(mPagerAdapter);
		
		mPager.setOnPageChangeListener(
				new ViewPager.SimpleOnPageChangeListener() {
					@Override
					public void onPageSelected(int position) {
						getActionBar().setSelectedNavigationItem(position);
					}
				});
		
		// Create a tab listener that is called when the user changes tabs.
		ActionBar.TabListener tabListener = new ActionBar.TabListener() {
			
			@Override
			public void onTabUnselected(Tab tab, FragmentTransaction ft) {
			}
			
			@Override
			public void onTabSelected(Tab tab, FragmentTransaction ft) {
				// When the tab is selected, switch to the corresponding
				// page in the ViewPager
				mPager.setCurrentItem(tab.getPosition());
			}
			
			@Override
			public void onTabReselected(Tab tab, FragmentTransaction ft) {
			}
		};
		
		if(mPagerAdapter.getCount() > mFragmentStringIds.length) {
			Log.e(TAG, "Number of tabs more than number of tab labels.");
		} else {
			// Add tabs, specifying the tab's text and TabListener
		    for (int i = 0; i < mPagerAdapter.getCount(); i++) {
		        actionBar.addTab(
		                actionBar.newTab()
		                		 .setText(getString(mFragmentStringIds[i]))
		                         .setTabListener(tabListener));
		    }
		}
		
		mHandler = new Handler() {
			@Override
			public void handleMessage(Message msg) {
				// Check every 10 seconds to see if we are receiving messages
				if (mRCPConnected && (mCurrentMessageCount == 0)) {
					Log.e(TAG, "The connection to " + mCamera.id + " has been lost. Timeout.");
					mAlert.show();
				} 
				else {
					mCurrentMessageCount = 0;
					mHandler.sendEmptyMessageDelayed(0, TIMER_DELAY);
				}
			}
		};
		
		// Set up camera connection if one is received (from another activity)
		Intent intent = this.getIntent();
		if(intent.hasExtra("CAMERA")) {
			Log.i(TAG, "Received intent with CAMERA info.");
			mCamera = (Camera) intent.getParcelableExtra("CAMERA");
		}
		else {
			Log.i(TAG, "Received intent without CAMERA info.");
		}
		
		mAlertBuilder = new AlertDialog.Builder(this);
	    mAlertBuilder.setMessage("The connection to " + mCamera.id + " has been lost. Timeout.")
	    	.setCancelable(false)
	    	.setPositiveButton("Ok", new DialogInterface.OnClickListener() {
	        public void onClick(DialogInterface dialog, int whichButton) {
	        	closeRCPConnection();
	        	finish();
	        }
	    });
	    mAlert = mAlertBuilder.create();
	}

	@Override
	public void onStart() {
		super.onStart();
		Log.i(TAG, "onStart");
		
		if((mRCPCamera != null) && (mCamera != null)) {
			if (mCamera.pin == null) {
				mCamera.pin = new String("102-000-001");
			}
			openRCPConnection(mCamera.pin, mCamera.ip);
		}
	}
	
	@Override
	public void onStop() {
		super.onStop();
		Log.i(TAG, "onStop");
		
		closeRCPConnection();
	}
	
	@Override
	public void onDestroy() {
		super.onDestroy();
		Log.i(TAG, "onDestroy");

		closeRCPConnection();
		mRCPCamera = null;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle presses on the action bar items
	    switch (item.getItemId()) {
	        case R.id.action_connect:
	        	closeRCPConnection();
	        	finish();
	            return true;
	        default:
	            return super.onOptionsItemSelected(item);
	    }
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.controller, menu);
		return true;
	}

	private void closeRCPConnection() {
		if((mRCPCamera != null) && mRCPConnected)
		{
			mRCPCamera.close();
			mRCPConnected = false;
		}
	}
	
	private void updateChildFragments() {
		for(Fragment f : mFragments) {
			((CameraUIFragment) f).onCameraConnectionChange(mLastStateData.state);
		}
	}

	private void openRCPConnection(String pin, String ip) {
		String appName = getApplicationInfo().loadLabel(getPackageManager()).toString();
		String appVer = "N/A";
		try
		{
			appVer = getPackageManager().getPackageInfo(getPackageName(), 0).versionName;
		}
		catch (NameNotFoundException e)
		{
			e.printStackTrace();
		}

		if ((mRCPCamera != null) && !mRCPConnected)
		{
			if (0 > mRCPCamera.open(pin, ip, appName, appVer, null)) {
				Log.i(TAG, "RCP open failed");
				mAlertBuilder.setMessage("Connection to Camera IP " + ip + " failed.");
				mAlertBuilder.create().show();
			}
			else {
				Log.i(TAG, "RCP open success");
				mRCPConnected = true;
				mCurrentMessageCount = 0;
				mHandler.sendEmptyMessageDelayed(0, TIMER_DELAY);
			}
		}
	}

	@Override
	public void onUpdateString(String id, String param) {

		String[] params = param.split(";");
		if (params[0].equals("RCP_PARAM_CAMERA_ID")) {
			mCamera.id = params[1];
			this.setTitle(mCamera.id);
		}

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateString(id, param);
			}
		}
	}

	@Override
	public void onUpdate(String id, String param) {

		String[] params = param.split(";");
		
		if (params[0].equals("RCP_PARAM_TIMECODE")) {
			mCurrentMessageCount++;
		}
		
		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdate(id, param);
			}
		}
	}

	@Override
	public void onConnectionError(String id, String msg) {
		Log.i(TAG, "Connection Error Received from SDK: " + msg);
		mAlertBuilder.setMessage("Connection ERROR from SDK: " + msg);
		mAlertBuilder.create().show();
	}

	@Override
	public void onStateChange(String id, String param) {
		mLastStateData = new RCPStateData(param);
	}

	@Override
	public void onCameraInfo(String id, String camInfo) {
		mCamera = new Camera(camInfo);
		
		switch (mLastStateData.state) {
		case RCPEnum.RCP_CONNECTION_STATE_INIT:
			// Do nothing
			break;
			
		case RCPEnum.RCP_CONNECTION_STATE_CONNECTED:
			break;
			
		case RCPEnum.RCP_CONNECTION_STATE_ERROR_RCP_VERSION_MISMATCH:
		case RCPEnum.RCP_CONNECTION_STATE_ERROR_RCP_PARAMETER_SET_VERSION_MISMATCH:
			closeRCPConnection();
			this.finish();
			break;
		
		case RCPEnum.RCP_CONNECTION_STATE_COMMUNICATION_ERROR:
		default:
			break;
		}
		updateChildFragments();
	}

	@Override
	public void onMenuNodeListUpdate(String id, int nodeId, String[] childList, String[] ancestorList) {

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onMenuNodeListUpdate(id, nodeId, childList,ancestorList);
			}
		}
	}

	@Override
	public void onMenuNodeStatusUpdate(String id, int nodeId, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onMenuNodeStatusUpdate(id, nodeId, isEnabled, isEnabledValid, isSupported, isSupportedValid);
			}
		}
	}

	@Override
	public void onStatusUpdate(String id, String paramName, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {
		
		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onStatusUpdate(id, paramName, isEnabled, isEnabledValid, isSupported, isSupportedValid);
			}
		}
	}

	@Override
	public void onUpdateClipList(String id, int status, String clipList) {

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateClipList(id, status, clipList);
			}
		}
	}

	@Override
	public void onUpdateEditInfo(String id, String editInfo) {

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateEditInfo(id, editInfo);
			}
		}
	}

	@Override
	public void onUpdateHistogram(String id, int[] red, int[] green, int[] blue, int[] luma,
			int bottom_clip, int top_clip, String text) {

		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateHistogram(id, red, green, blue, luma, bottom_clip, top_clip, text);
			}
		}
	}

	@Override
	public void onUpdateInt(String id, String paramName, int value) {
		
		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateInt(id, paramName, value);
			}
		}
	}

	@Override
	public void onUpdateListStrings(String id, String listHeader, String listFlags, String[] listStrings, int[] listValues) {
		
		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateListStrings(id, listHeader, listFlags, listStrings, listValues);
			}
		}
	}

	@Override
	public void onUpdateTag(String id, String param) {
		
		for(Fragment f : mFragments) {
			if(f.isResumed()) {
				((CameraUIFragment) f).onUpdateTag(id, param);
			}
		}
	}

	@Override
	public void onAudioVuUpdate(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onNotificationUpdate(String arg0, String arg1, String arg2,
			String arg3, String arg4) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onRftpStatusUpdate(String arg0, byte[] arg1, int arg2,
			int arg3, int arg4, String[] arg5) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateStringEditInfo(String arg0, String arg1, int[] arg2,
			String[] arg3) {
		// TODO Auto-generated method stub
		
	}
}
