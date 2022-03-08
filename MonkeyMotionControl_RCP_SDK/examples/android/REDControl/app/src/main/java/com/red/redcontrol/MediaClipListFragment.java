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

import com.red.rcp.RCPCamera;

import android.app.Activity;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.ProgressBar;

public class MediaClipListFragment extends CameraUIFragment {
    
	private static final String TAG = "MediaClipListFragment";

	public MediaClipListFragment(RCPCamera camera) {
		super(camera);
	}

	private ListView mMediaClipListView;
	private ProgressBar mMediaClipListUpdateProgressBar;
	private ArrayList<MediaClip> mMediaClips;
	private MediaClipAdapter mMediaClipAdapter;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {

		super.onCreate(savedInstanceState);
		mMediaClips = new ArrayList<MediaClip>();
		mMediaClipAdapter = new MediaClipAdapter(super.getActivity(), R.layout.list_item_media_clip, mMediaClips);
	}
	
	@Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {

		ViewGroup rootView = (ViewGroup) inflater.inflate(R.layout.tab_media_clip_list, container, false);
        
        mMediaClipListView = (ListView) rootView.findViewById(R.id.media_clip_list_view);
        mMediaClipListView.setAdapter(mMediaClipAdapter);
        mMediaClipListUpdateProgressBar = (ProgressBar) rootView.findViewById(R.id.media_clip_progress_bar);
        
        return rootView;
    }

	public void setMediaClipListUpdateProgress(int status) {
		int v = (status == 100) ? View.INVISIBLE : View.VISIBLE;
		mMediaClipListUpdateProgressBar.setVisibility(v);
	}
	
	@Override
	public void onUpdateClipList(String id, int status, String clipList) {
		// Clip string format:
		// clips[index] = <index>|<clip name>|<clip date>|<clip time>|<sensor FPS>|<edge start timecode>|<edge end timecode>|<TOD start timecode>|<TOD end timecode>
		switch (status) {
		case RCPEnum.CLIP_LIST_LOADING:
			Log.i(TAG, "Clip List loading...");
			setMediaClipListUpdateProgress(0);
			break;
			
		case RCPEnum.CLIP_LIST_DONE:
			if (!clipList.isEmpty()) {
				String[] clips = clipList.split(";");		
				int nn = clips.length;
				Log.i(TAG, "Clip List done: nclips = " + Integer.toString(nn));
				Log.i(TAG, "Clip[0] = " + clips[0]);
				
				mMediaClips.clear();
				for(String clipString : clips) {
					mMediaClips.add(new MediaClip(clipString.split("\\|")));
				}
				mMediaClipAdapter.notifyDataSetChanged();					
				setMediaClipListUpdateProgress(100);
			}
			break;

		case RCPEnum.CLIP_LIST_BLOCKED:
			Log.i(TAG, "Clip List blocked...");
			setMediaClipListUpdateProgress(100);
			break;
		}
	}
}
