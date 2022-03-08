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
import java.util.regex.Matcher;
import android.os.Bundle;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.util.Patterns;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.TextView.OnEditorActionListener;
import com.red.rcp.RCPDiscoveryListener;
import com.red.rcp.RCPDiscovery;

public class ConnectActivity extends Activity implements RCPDiscoveryListener {

	final static String TAG = "ConnectActivity";
	
	private ListView 	mCameraListView;
	private EditText 	mCameraIpUserInput;
	private Button		mConnectButton;
	
	private RCPDiscovery mRCP;
	private CameraAdapter mCameraAdapter;
	private ArrayList<Camera> mCameras;

	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_connect);
		
		// Initiate member variables
		mRCP = new RCPDiscovery(this);
		mCameras = new ArrayList<Camera>();
		
		
		// Bind UI elements to handles
		mCameraListView = (ListView) findViewById(R.id.camera_list_view_connect);

		// Bind adapter to the camera list view
		mCameraAdapter = new CameraAdapter(this, R.layout.list_item_camera, mCameras);
		mCameraListView.setAdapter(mCameraAdapter);
		
		initConnectButton();

		initCameraIpUserInputEditText();
		
		// Start finding cameras
		mRCP.startDiscovery();
		
	}

	private void initConnectButton() {
		mConnectButton = (Button) findViewById(R.id.connect_button_connect);
		
		mConnectButton.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View view) {
				Context context = getApplicationContext();
				String usrInputStr = mCameraIpUserInput.getText().toString();
				
				Matcher matcher = Patterns.IP_ADDRESS.matcher(usrInputStr);
				
				if(matcher.matches()) {
					Log.i(TAG, "IP valid.");
					Intent intent = new Intent(context, ControllerActivity.class);
					intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
					intent.putExtra("CAMERA", new Camera(null, null, null, null, null, usrInputStr));
					context.startActivity(intent);
				}
				else {
					Log.i(TAG, "IP invalid.");
					Toast toast = Toast.makeText(context, "IP address invalid", Toast.LENGTH_SHORT);
					toast.show();
				}
			}
		});
	}
	
	private void initCameraIpUserInputEditText() {
		mCameraIpUserInput = (EditText) findViewById(R.id.camera_ip_edit_text_connect);
		
		mCameraIpUserInput.setOnEditorActionListener(new OnEditorActionListener() {
		    @Override
		    public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
		        boolean handled = false;
		        if (actionId == EditorInfo.IME_ACTION_DONE) {	            
		        	// Dismiss the keyboard
			        InputMethodManager imm = (InputMethodManager)getSystemService(INPUT_METHOD_SERVICE);
			        imm.hideSoftInputFromWindow(mCameraIpUserInput.getWindowToken(), 0);
		            handled = true;
		        }
		        return handled;
		    }
		});		
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.connect, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle presses on the action bar items
	    switch (item.getItemId()) {
	        case R.id.action_refresh:
	        	if(null != mRCP) {
	        		mRCP.startDiscovery();
	        	}
	        	return true;
	        default:
	            return super.onOptionsItemSelected(item);
	    }
	}
	

	private void updateCameraListFromRCPCameraString(String str) {

		mCameras.clear();
		
		if(!str.isEmpty()) {
			String[] cameraList = str.split(";");
			for(String cameraStr : cameraList) {
				mCameras.add(new Camera(cameraStr));
			}
		}
		
		mCameraAdapter.notifyDataSetChanged();
	}
	
	@Override
	public void onCameraDiscoveryUpdate(String cameras) {
		Log.i(TAG, "onCameraDiscoveryUpdate");
		
		updateCameraListFromRCPCameraString(cameras);
	}
}
