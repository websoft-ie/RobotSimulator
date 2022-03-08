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
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.TextView;

public class CameraAdapter extends ArrayAdapter<Camera> {

	Context context;
	int layoutResourceId;
	List<Camera> mCameras;
		
	public CameraAdapter(Context context, int layoutResourceId, List<Camera> items) {
		super(context, layoutResourceId, items);
		this.context = context;
		this.layoutResourceId = layoutResourceId;
		this.mCameras = items;
	}
	
	private class ArrowClicker implements View.OnClickListener {
		int position = 0;
		
		public ArrowClicker(int pos) {
			position = pos;
		}
		
		public void onClick(View v) {
			Intent intent = new Intent(context, CameraInfoActivity.class);
			intent.putExtra("CAMERA", mCameras.get(position));
			context.startActivity(intent);
		}
	}

	private class TextClicker implements View.OnClickListener {
		int position = 0;
		
		public TextClicker(int pos) {
			position = pos;
		}
		
		public void onClick(View v) {
			Intent intent = new Intent(context, ControllerActivity.class);
			intent.putExtra("CAMERA", mCameras.get(position));
			context.startActivity(intent);
		}
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		View rowView = convertView;
		CameraViewHolder holder = null;
		
		if(null == rowView) {
			LayoutInflater inflater = ((Activity)context).getLayoutInflater();
			rowView = inflater.inflate(layoutResourceId, parent, false);
			
			holder = new CameraViewHolder();
			holder.textButton 	= (Button) 		rowView.findViewById(R.id.itemTextButton);
			holder.textView 	= (TextView) 	rowView.findViewById(R.id.itemTextView);
			holder.arrowButton 	= (ImageButton) rowView.findViewById(R.id.arrowImageButton);
			
			holder.textButton.setOnClickListener(new TextClicker(position));
			holder.arrowButton.setOnClickListener(new ArrowClicker(position));
			
			rowView.setTag(holder);
		}
		else {
			holder = (CameraViewHolder) rowView.getTag();
		}
		
		Camera camera = mCameras.get(position);
		holder.textButton.setText(camera.id);
		holder.textView.setText(camera.ip + " | " + camera.pin);
		
		return rowView;
	}
	
	static class CameraViewHolder {
		Button		textButton;
		TextView	textView;
		ImageButton arrowButton;
	}
}
