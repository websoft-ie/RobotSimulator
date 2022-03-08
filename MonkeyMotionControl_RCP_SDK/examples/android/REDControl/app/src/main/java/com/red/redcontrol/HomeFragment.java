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
import java.util.Collections;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Color;
import android.util.Log;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnFocusChangeListener;
import android.view.View.OnTouchListener;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputMethodManager;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.TextView.OnEditorActionListener;
import android.widget.Toast;

import com.red.rcp.RCPCamera;

public class HomeFragment extends CameraUIFragment {

	private static final String TAG = "HomeFragment";
	private static final String SLIDER_PARAM = "RCP_PARAM_SATURATION";
	private static final String SPINNER_PARAM = "RCP_PARAM_REDCODE";
	private static final String GENERIC_FIELD_2_PARAM = "RCP_PARAM_SLATE_SCENE";
	
	private final static int SLIDER_STOP_TIMEOUT_MS = 500;
	
	private LinearLayout mRootView;
	
	private TextView 		mtvTimecode;
	private Button 			mRecordButton;
	private CustomSpinner	mSpinner;
	private HistogramView 	mHistogram;
	private SeekBar			mSeekBar;
	private EditText		mSeekBarEditText;
	private TextView 		mtvSliderLabel;
	private TextView		mtvSpinnerLabel;
	private TextView		mtvMediaLabel;
	private TextView		mtvMediaValue;
	private EditText		mGenericField2EditText;
	private Button			mSnapshotButton;
	
	private List<String> mSpinnerList;
	private List<String> mSpinnerParamValuesList;
	
	private CustomSpinnerAdapter mSpinnerAdapter;
	
	private int mSliderValuesNum;
	private List<String> mSliderList;
	private List<Integer> mSliderParamValuesList;
	private boolean mIsSliding;
	private int mSliderCurrentIndex;
	private TimerTask mSliderTask = null;
	private Timer mSliderTimer = null;
	
	private Context mContext;
	private boolean mSliderTextChanged = false;
	
	private EditInfo mSliderEditInfo = null;

	public HomeFragment(RCPCamera camera) {
		super(camera);
	}
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {

		super.onCreate(savedInstanceState);
		mRootView = (LinearLayout) inflater.inflate(R.layout.tab_home, container, false);

		// Create member variables
		mContext = super.getActivity().getApplicationContext();
		
		// Bind members to UI components
		mtvTimecode 			= (TextView) mRootView.findViewById(R.id.timecodeTextView);
		mRecordButton			= (Button) mRootView.findViewById(R.id.recButtonMain);
		mSpinner 				= (CustomSpinner) mRootView.findViewById(R.id.rcpParamSpinner);
		mHistogram				= (HistogramView) mRootView.findViewById(R.id.histogramView);
		mSeekBar				= (SeekBar) mRootView.findViewById(R.id.rcpParamSlider);
		mSeekBarEditText 		= (EditText) mRootView.findViewById(R.id.sliderValueEditText);
		mtvSliderLabel      	= (TextView) mRootView.findViewById(R.id.sliderLabelTextView);
		mtvSpinnerLabel			= (TextView) mRootView.findViewById(R.id.spinnerLabel);
		mtvMediaLabel			= (TextView) mRootView.findViewById(R.id.mediaLabel);
		mtvMediaValue			= (TextView) mRootView.findViewById(R.id.mediaValue);
		mGenericField2EditText 	= (EditText) mRootView.findViewById(R.id.generic_field_2_edit_text_main);
		mSnapshotButton			= (Button) mRootView.findViewById(R.id.snapshot_button_main);
		
		initSnapshotButton();
		initRecButton();
		initSpinner();
		initGenericField2();
		initSeekBar();
		
	    return mRootView;
	}
	
	@Override
	public void onResume() {
		super.onResume();
		if (mRCPConnected)
			updateUI();
	}

	@Override
	public void onPause() {
		super.onPause();
		mSliderEditInfo = null;
	}

	private void initSpinner() {
		mSpinnerList 			= new ArrayList<String>();
		mSpinnerParamValuesList = new ArrayList<String>();
		
		mSpinnerAdapter = new CustomSpinnerAdapter(super.getActivity(), R.layout.list_item_spinner_param, mSpinnerParamValuesList, mSpinnerList);
		mSpinnerAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
		mSpinner.setAdapter(mSpinnerAdapter);
		
		mSpinner.setOnItemSelectedListener(new OnItemSelectedListener() {

			@Override
			public void onItemSelected(AdapterView<?> parentView, View view, int pos, long id) {
				if (!mSpinnerParamValuesList.isEmpty() && mSpinner.isLockPeriodOver()) {
					mRCP.setParameter(SPINNER_PARAM, Integer.parseInt(mSpinnerParamValuesList.get(pos)));
				}
			}

			@Override
			public void onNothingSelected(AdapterView<?> arg0) {
				// nothing
			}
		});
		
		mSpinner.setOnTouchListener(new OnTouchListener() {
			@Override
			public boolean onTouch(View view, MotionEvent event) {
				switch(event.getAction()) {
				case MotionEvent.ACTION_DOWN:
					if(null != mRCP) {
						mRCP.getList(SPINNER_PARAM);
					}
					break;
				case MotionEvent.ACTION_UP:
					break;
				}
				return false;
			}
		});
	}
	
	private void initGenericField2() {
		mGenericField2EditText.setOnEditorActionListener(new OnEditorActionListener() {
		    @Override
		    public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
		        boolean handled = false;
		        Log.i(TAG, "actionId = " + actionId);
		        if (actionId == EditorInfo.IME_ACTION_DONE) {
		        	mRCP.setParameterString(GENERIC_FIELD_2_PARAM, v.getText().toString());
		        	mRCP.getParameter(GENERIC_FIELD_2_PARAM);
		        	mSeekBarEditText.clearFocus();

		        	// Dismiss the keyboard
			        InputMethodManager imm = (InputMethodManager)mContext.getSystemService(Activity.INPUT_METHOD_SERVICE);
			        imm.hideSoftInputFromWindow(mGenericField2EditText.getWindowToken(), 0);
		            handled = true;
		        }
		        return handled;
		    }
		});
	}
	
	private void initRecButton() {
		mRecordButton.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View view) {
				mRCP.toggleRecordState();
			}
		});
	}

	private void initSnapshotButton() {
		mSnapshotButton.setOnTouchListener(new OnTouchListener() {
			@Override
			public boolean onTouch(View view, MotionEvent event) {
				switch(event.getAction()) {
				case MotionEvent.ACTION_DOWN:
					view.setBackgroundColor(Color.RED);
					break;
				case MotionEvent.ACTION_UP:
					view.setBackgroundColor(Color.GRAY);
					// TODO: Create a script that generates a Java class that includes the enums from RCP SDK
					mRCP.setParameterString("RCP_PARAM_KEYACTION", "137");	// 137 - KEY_ACTION_MARK_SNAPSHOT
					break;
				}
				return false;
			}
		});
	}
	
	private void initSeekBar() {
		mSliderValuesNum = 0;
		mSliderList = new ArrayList<String>();
		mSliderParamValuesList = new ArrayList<Integer>();
		mIsSliding = false;
		mSliderCurrentIndex = 0;
		
		mSeekBarEditText.setOnEditorActionListener(new OnEditorActionListener() {
		    @Override
		    public boolean onEditorAction(TextView v, int actionId, KeyEvent event) {
		        boolean handled = false;
		        if (actionId == EditorInfo.IME_ACTION_DONE) {	            
		        	mSliderTextChanged = true;
		        	mSeekBarEditText.clearFocus();

		        	// Dismiss the keyboard
			        InputMethodManager imm = (InputMethodManager)mContext.getSystemService(Activity.INPUT_METHOD_SERVICE);
			        imm.hideSoftInputFromWindow(mSeekBarEditText.getWindowToken(), 0);
		            handled = true;
		        }
		        return handled;
		    }

		});
		
		mSeekBarEditText.setOnFocusChangeListener(new OnFocusChangeListener() {

			@Override
			public void onFocusChange(View view, boolean hasFocus) {
				if(hasFocus) {
					// do nothing
				} else {
					if(true == mSliderTextChanged) {
						mSliderTextChanged = false;
			        	mRCP.setParameter(SLIDER_PARAM, (int) (1000*Float.parseFloat(((TextView) view).getText().toString())) );
					}
					mRCP.getParameter(SLIDER_PARAM);
				}
			}
		});
		
		mSeekBar.setOnSeekBarChangeListener(new OnSeekBarChangeListener() {
			@Override
			public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
				
				mSliderCurrentIndex = progress;
				if(fromUser) {
					int value;
					if (mSliderEditInfo != null) {
						// If this is a custom editable list
						value = mSliderEditInfo.min + (mSliderCurrentIndex * mSliderEditInfo.step);
						float result = (float) value / (float) mSliderEditInfo.divider;
						String form = "%." + mSliderEditInfo.digits + "f";
						String str = String.format(form, result);
						mSeekBarEditText.setText(str);
					}
					else {
						// If we're simply sliding through choices in the list
						value = mSliderParamValuesList.get(mSliderCurrentIndex).intValue();
						mSeekBarEditText.setText(mSliderList.get(mSliderCurrentIndex));
					}
					mRCP.setParameter(SLIDER_PARAM, value);
				}
			}

			@Override
			public void onStartTrackingTouch(SeekBar seekBar) {
				mIsSliding = true;
			}

			@Override
			public void onStopTrackingTouch(SeekBar seekBar) {
				// Cancel old task if any
				if ((mSliderTask != null) && (mSliderTimer != null)) {
					mSliderTask.cancel();
					mSliderTimer.cancel();
					mSliderTimer.purge();
				}
				// As we slide the slider, we're sending set messages to the camera and getting current messages back. If we update the slider position
				// and display value right then, you would see them jump around too much. Instead, we ignore all current messages until we stop sliding
				// for a bit. This is the timer that detects that.
				mSliderTask = new TimerTask() {
					@Override
					public void run() {
						mIsSliding = false;
						mRCP.getParameter(SLIDER_PARAM);
					}
				};
				mSliderTimer = new Timer();
				try {
					mSliderTimer.schedule(mSliderTask , SLIDER_STOP_TIMEOUT_MS);
				}
				catch (IllegalStateException e) {
					Log.v(TAG, "timer illegal state");
				}
			}
		});
	}

	public void onCameraConnectionChange(int state) {
		if (state == RCPEnum.RCP_CONNECTION_STATE_CONNECTED) {
			mRCPConnected = true;
			updateUI();
		}
		else {
			mRCPConnected = false;
		}
	}

	private String replaceSpecialCharacters(String s) {
		return s.replace("&reg;", ((char) 0x00AE)+"");
	}

	private void updateUI() {
		
		mRCP.getList(SLIDER_PARAM);
		mRCP.getParameter(SLIDER_PARAM);
		mRCP.getstatus(SLIDER_PARAM);

		mRCP.getParameter(SPINNER_PARAM); // don't get the list until the user presses the spinner to expand it.
		mRCP.getstatus(SPINNER_PARAM);
		
		mRCP.getParameter("RCP_PARAM_CAMERA_ID");
		mRCP.getParameter("RCP_PARAM_CAMERA_TYPE");
		mRCP.getParameter("RCP_PARAM_CAMERA_PIN");
		mRCP.getParameter("RCP_PARAM_CAMERA_FIRMWARE_VERSION");
		mRCP.getParameter("RCP_PARAM_RED_CLIP");
		mRCP.getParameter("RCP_PARAM_GREEN_CLIP");
		mRCP.getParameter("RCP_PARAM_BLUE_CLIP");
		mRCP.getParameter(GENERIC_FIELD_2_PARAM);
		
		mRCP.getParameter("RCP_PARAM_MEDIA_DISPLAY_LABEL");
		mRCP.getstatus("RCP_PARAM_MEDIA_DISPLAY_LABEL");
		mRCP.getParameter("RCP_PARAM_MEDIA_DISPLAY_VAL");
		mRCP.getstatus("RCP_PARAM_MEDIA_DISPLAY_VAL");
		
		mtvSpinnerLabel.setText(replaceSpecialCharacters(mRCP.getParameterLabel(SPINNER_PARAM)));
		mtvSliderLabel.setText(replaceSpecialCharacters(mRCP.getParameterLabel(SLIDER_PARAM)));
		//mtvMediaLabel.setText(mRCP.getParameterLabel("RCP_PARAM_MEDIA_DISPLAY_LABEL"));
	}
	
	@Override
	public void onUpdateInt(String id, String param, int value) {
		if (param.equals("RCP_PARAM_RED_CLIP")) {
			mHistogram.setRedClip(value);
		}
		else if (param.equals("RCP_PARAM_GREEN_CLIP")) {
			mHistogram.setGreenClip(value);
		}
		else if (param.equals("RCP_PARAM_BLUE_CLIP")) {
			mHistogram.setBlueClip(value);
		}
		else if (param.equals(SLIDER_PARAM) && !mIsSliding) {
			if (mSliderEditInfo != null) {
				// We have custom editable values
				if ((value >= mSliderEditInfo.min) && (value <= mSliderEditInfo.max)) {
					mSliderCurrentIndex = (value - mSliderEditInfo.min)/mSliderEditInfo.step;
					mSeekBar.setProgress(mSliderCurrentIndex);
				}
			}
			else {
				// only select from values in the list
				Integer iVal = Integer.valueOf(value);
				mSeekBar.setProgress(mSliderParamValuesList.indexOf(iVal));
			}
		}
	}
	
	@Override
	public void onUpdate(String id, String param) {
		
		// ParamSet = "<Parameter Enum Name>;<Parameter Value>;<Parameter Value Abbreviated>;<Color>
		String[] params = param.split(";");
		
		// Assign parameter to the corresponding widget
		if (params[0].equals("RCP_PARAM_TIMECODE")) {
			mtvTimecode.setText(params[1]);
			setColor(mtvTimecode, params[3]);
		}
		else if (params[0].equals(SLIDER_PARAM) && !mSeekBarEditText.isFocused() && !mIsSliding) {
			mSeekBarEditText.setText(params[1]);
		}
		else if (params[0].equals(SPINNER_PARAM)) {

			mSpinnerAdapter.setCurrentDisplay(params[1], params[3]);
			// Only add an item to the list if none are there currently. This takes care of the case when we simply
			// want to display the current value in the spinner anchor, but the user hasn't pressed it to expand the list yet.
			if (mSpinnerList.isEmpty())
				mSpinnerList.add(params[2]);
			mSpinnerAdapter.notifyDataSetChanged();
		}
		else if (params[0].equals("RCP_PARAM_RECORD_STATE")) {
			mRecordButton.setText(params[1]);
			if(params[1].equals("RECORDING")) {
				mRecordButton.setBackgroundColor(Color.RED);
			}
			else if(params[1].equals("FINALIZING")) {
				mRecordButton.setBackgroundColor(Color.YELLOW);
			}
			else if(params[1].equals("PRE-RECORDING")) {
				mRecordButton.setBackgroundColor(Color.YELLOW);
			}
			else if(params[1].equals("RECORD")) {
				mRecordButton.setBackgroundColor(Color.GRAY);
			}
		}
	}

	@Override
	public void onUpdateHistogram(String id, int[] red, int[] green, int[] blue, int[] luma,
			int bottom_clip, int top_clip, String text) {
		mHistogram.setValues(red, green, blue, luma, bottom_clip, top_clip, text);
	}

	@Override
	public void onUpdateListStrings(String id, String listHeader, String listFlags, String[] listStrings, int[] listValues) {

		String[] headers = listHeader.split("\\|");
		String[] flags = listFlags.split("\\|");
		String paramName = headers[0];

		int length 		= Integer.parseInt(headers[1]);
		int index 		= Integer.parseInt(headers[2]);
		int minValid 	= Integer.parseInt(headers[3]);
		int minValue 	= Integer.parseInt(headers[4]);
		int maxValid 	= Integer.parseInt(headers[5]);
		int maxValue	= Integer.parseInt(headers[6]);

		if(paramName.equals(SPINNER_PARAM)) {
			/*
			Log.d(TAG, "REDCODE: l=" + length + ", i=" + index);
			for (int i = 0; i < listStrings.length; i++) {
				Log.d(TAG, "strings[" + i + "] = " + listStrings[i]);
			}
			for (int i = 0; i < listValues.length; i++) {
				Log.d(TAG, "values[" + i + "] = " + listValues[i]);
			}*/
			// Update spinner data
			mSpinnerAdapter.updateLimits(minValid, minValue, maxValid, maxValue);
			mSpinnerList.clear();
			Collections.addAll(mSpinnerList, listStrings);
			
			String[] strValues = new String[listValues.length];
			for (int i = 0; i < listValues.length; i++) {
				strValues[i] = String.valueOf(listValues[i]);
			}

			mSpinnerParamValuesList.clear();
			Collections.addAll(mSpinnerParamValuesList, strValues);
			mSpinnerAdapter.setLastSelection(index);
			mSpinnerAdapter.notifyDataSetChanged();
		}
		else if(paramName.equals(SLIDER_PARAM)) {

			mSliderValuesNum = length;

			// Update spinner data
			mSliderList.clear();
			Collections.addAll(mSliderList, listStrings);

			Integer[] intValues = new Integer[listValues.length];
			for (int i = 0; i < listValues.length; i++) {
				intValues[i] = Integer.valueOf(listValues[i]);
			}

			mSliderParamValuesList.clear();
			Collections.addAll(mSliderParamValuesList, intValues);
			// Don't update current index and max if we've already got the edit info, since its done htere
			if (mSliderEditInfo == null) {
				mSliderCurrentIndex = index;
				mSeekBar.setMax(mSliderValuesNum);
			}
		}
	}

	public void setColor(TextView tv, String color)
	{	
		if (color.equals("RED")) {
			tv.setTextColor(Color.RED);
		} else if (color.equals("YELLOW")) {
			tv.setTextColor(Color.YELLOW);
		} else if (color.equals("GREEN")) {
			tv.setTextColor(Color.GREEN);
		} else if (color.equals("DARK GREY")) {
			tv.setTextColor(Color.DKGRAY);
		} else {
			tv.setTextColor(Color.WHITE);
		}
	}
	
	@Override
	public void onUpdateString(String id, String param) {
		
		String[] params = param.split(";");
		
		if (params[0].equals("RCP_PARAM_MEDIA_DISPLAY_LABEL")) {
			mtvMediaLabel.setText(params[1]);
		}
		else if (params[0].equals("RCP_PARAM_MEDIA_DISPLAY_VAL")) {
			mtvMediaValue.setText(params[1]);
			setColor(mtvMediaValue, params[3]);
		}
		else if (params[0].equals(GENERIC_FIELD_2_PARAM)) {
			mGenericField2EditText.setText(params[1]);
		}
	}

	@Override
	public void onUpdateTag(String id, String param) {
		
		// param: <Param id>;<tag type>;<tag frame number>;<tod timecode>
		String[] params = param.split(";");
		
		if (params[0].equals("RCP_PARAM_FRAME_TAG")) {
			if(1 == Integer.parseInt(params[1])) {		// TAG_INFO_TAG_TYPE_STILL
				Toast toast = Toast.makeText(mContext, "Snapshot taken at frame #" + params[2], Toast.LENGTH_SHORT);
				toast.show();
			}
		}
	}

	@Override
	public void onUpdateEditInfo (String id, String editInfo) {
		// paramName;min;max;divider;digits;step;prefix;suffix
    	String[] info = editInfo.split(";");

    	if (info.length >= 6) {
    		if (info[0].equals(SLIDER_PARAM) && (mSliderEditInfo == null)) {
    			int min = Integer.parseInt(info[1]);
    			int max = Integer.parseInt(info[2]);
    			int divider = Integer.parseInt(info[3]);
    			int digits = Integer.parseInt(info[4]);
    			int step = Integer.parseInt(info[5]);
    			Log.d(TAG, "editinfo: min=" + min + ", max=" + max + ", div=" + divider + ", digits=" + digits + ", step=" + step);
    			mSliderEditInfo = new EditInfo(min, max, divider, digits, step);

    			mSeekBar.setMax((mSliderEditInfo.max - mSliderEditInfo.min)/mSliderEditInfo.step);
    			int listStep = (mSliderEditInfo.max - mSliderEditInfo.min)/(mSliderValuesNum - 1);
    			mSliderCurrentIndex *= (listStep / mSliderEditInfo.step);
    		}
    	}
    	else
    		Log.e(TAG, "Invalid num edit info params: " + info.length);
	}

	@Override
	public void onStatusUpdate(String id, String paramName, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {
		if (paramName.equals(SPINNER_PARAM)) {
			if (isSupportedValid != 0) {
				// hide/unhide control for this parameter based on this flag
				int visibility = (isSupported == 0) ? View.INVISIBLE : View.VISIBLE;
				mSpinner.setVisibility(visibility);
				mtvSpinnerLabel.setVisibility(visibility);
			}
			
			if (isEnabledValid != 0) {
				// enable/disable control for this parameter based on this flag
				boolean enable = (isEnabled == 0) ? false : true;
				mSpinner.setEnabled(enable);
				mtvSpinnerLabel.setEnabled(enable);
			}
		}
		else if (paramName.equals(SLIDER_PARAM)) {
			if (isSupportedValid != 0) {
				// hide/unhide control for this parameter based on this flag
				int visibility = (isSupported == 0) ? View.INVISIBLE : View.VISIBLE;
				mSeekBar.setVisibility(visibility);
				mSeekBarEditText.setVisibility(visibility);
				mtvSliderLabel.setVisibility(visibility);
			}
			
			if (isEnabledValid != 0) {
				// enable/disable control for this parameter based on this flag
				boolean enable = (isEnabled == 0) ? false : true;
				mSeekBar.setEnabled(enable);
				mSeekBarEditText.setEnabled(enable);
				mtvSliderLabel.setEnabled(enable);
			}
		}
		// ... TBD status for all other RCP parameters in this activity need to be handled as well.
	}
}
