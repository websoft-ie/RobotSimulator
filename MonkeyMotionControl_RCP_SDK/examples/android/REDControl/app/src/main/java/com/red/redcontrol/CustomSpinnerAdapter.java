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

import java.util.List;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class CustomSpinnerAdapter extends ArrayAdapter<String>{

	private static final String TAG = "CustomSpinnerAdapter";
	Context context;
	int layoutResourceId;
	List<String> mParamStrings;
	List<String> mParamValues;
	int minValid, minValue, maxValid, maxValue, mLastAttemptedValue;
	String mCurrDisplayValue, mCurrDisplayColor, mLastSelectedString;
	
	private static enum CALLER {
		GET_VIEW,
		GET_DROP_DOWN_VIEW
	}
	
	public CustomSpinnerAdapter(Context context, int layoutResourceId, List<String> values, List<String> items) {
		super(context, layoutResourceId, items);
		this.context = context;
		this.layoutResourceId = layoutResourceId;
		this.mParamStrings = items;
		this.mParamValues = values;
		this.minValid = 0;
		this.minValue = 0;
		this.maxValid = 0;
		this.maxValue = 0;
		this.mLastAttemptedValue = 0;
		this.mCurrDisplayValue = "";
		this.mCurrDisplayColor = "";
		this.mLastSelectedString = ""; 
	}
	
	public void setCurrentDisplay(String currDisplayValue, String currDisplayColor) {
		this.mCurrDisplayValue = currDisplayValue;
		this.mCurrDisplayColor = currDisplayColor;
	}
	
	public void setLastSelection(int position) {
		this.mLastSelectedString = mParamStrings.get(position);
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent) {
		return getCustomView(position, convertView, parent, CALLER.GET_VIEW);
	}
	
	@Override
	public View getDropDownView(int position, View convertView, ViewGroup parent) {
		return getCustomView(position, convertView, parent, CALLER.GET_DROP_DOWN_VIEW);
	}
	
	private View getCustomView(int position, View convertView, ViewGroup parent, CALLER caller) {
		View rowView = convertView;
		ParamValueViewHolder holder = null;
		
		if(null == rowView) {
			LayoutInflater inflater = ((Activity)context).getLayoutInflater();
			rowView = inflater.inflate(layoutResourceId, parent, false);
			
			holder = new ParamValueViewHolder();
			holder.textView		 = (TextView) rowView.findViewById(R.id.spinnerItemTextView);
			
			rowView.setTag(holder);
		}
		else {
			holder = (ParamValueViewHolder) rowView.getTag();
		}
		
		switch(caller) {
		case GET_VIEW:
			holder.textView.setText(mCurrDisplayValue);
			if(mCurrDisplayColor.equals("YELLOW")) {
				holder.textView.setTextColor(Color.YELLOW);
			}
			else {
				holder.textView.setTextColor(Color.WHITE);
			}
			break;
		case GET_DROP_DOWN_VIEW:
			if(position < mParamStrings.size() && position < mParamValues.size()) {
				String paramString = mParamStrings.get(position);
				String paramValue  = mParamValues.get(position);
	
				try {
					int value = Integer.parseInt(paramValue);
					if ((1 == minValid && value < minValue) || (1 == maxValid && value > maxValue)) {
						holder.textView.setTextColor(Color.GRAY);
					}
					else {
						holder.textView.setTextColor(Color.WHITE);
					}
				}
				catch(NumberFormatException e) {
					Log.e(TAG, "Invalid value received.");
				}

				if(paramString.equals(mLastSelectedString)) {
					holder.textView.setText(paramString + " <");
				}
				else {
					holder.textView.setText(paramString);
				}
			}
			break;
		}
		
		return rowView;
	}
	
	public void updateLimits(int minValid, int minValue, int maxValid, int maxValue) {
		this.minValid = minValid;
		this.minValue = minValue;
		this.maxValid = maxValid;
		this.maxValue = maxValue;
	}

	static class ParamValueViewHolder {
		TextView		textView;
	}
}
