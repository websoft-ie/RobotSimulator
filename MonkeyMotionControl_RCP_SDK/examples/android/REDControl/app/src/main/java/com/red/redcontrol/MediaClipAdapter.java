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
import java.util.List;

import android.app.Activity;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class MediaClipAdapter extends ArrayAdapter<MediaClip> {

	Context context;
	int layoutResourceId;
	ArrayList<MediaClip> mMediaClips;
	

	public MediaClipAdapter(Context context, int layoutResourceId, List<MediaClip> items) {
		super(context, layoutResourceId, items);
		this.context = context;
		this.layoutResourceId = layoutResourceId;
		this.mMediaClips = (ArrayList<MediaClip>) items;
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		View rowView = convertView;
		MediaClipViewHolder holder = null;
		
		if(null == rowView) {
			LayoutInflater inflater = ((Activity)context).getLayoutInflater();
			rowView = inflater.inflate(layoutResourceId, parent, false);
			
			holder = new MediaClipViewHolder();
			holder.indexTextView = (TextView) rowView.findViewById(R.id.index_text_view_media_clip);
			holder.nameTextView = (TextView) rowView.findViewById(R.id.name_text_view_media_clip);
			holder.dateTimeTextView = (TextView) rowView.findViewById(R.id.date_time_text_view_media_clip);
			
			rowView.setTag(holder);
		}
		else {
			holder = (MediaClipViewHolder) rowView.getTag();
		}
		
		MediaClip mediaClip = mMediaClips.get(position);
		holder.indexTextView.setText(mediaClip.index);
		holder.nameTextView.setText(mediaClip.name);
		holder.dateTimeTextView.setText(mediaClip.date + " " + mediaClip.time);
				
		return rowView;
	}
	
	static class MediaClipViewHolder {
		TextView	indexTextView;
		TextView	nameTextView;
		TextView	dateTimeTextView;
	}
}
