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

import android.support.v4.app.Fragment;

import com.red.rcp.RCPCamera;
import com.red.rcp.RCPCameraListener;

public abstract class CameraUIFragment extends Fragment implements RCPCameraListener {

	protected RCPCamera mRCP = null;
	protected boolean mRCPConnected = false;
	
	public CameraUIFragment (RCPCamera camera) {
		mRCP = camera;
	}
	
	public void onCameraConnectionChange(int state) {
	}
	
	@Override
	public void onAudioVuUpdate(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onCameraInfo(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onConnectionError(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onMenuNodeListUpdate(String arg0, int arg1, String[] arg2,
			String[] arg3) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onMenuNodeStatusUpdate(String arg0, int arg1, int arg2,
			int arg3, int arg4, int arg5) {
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
	public void onStateChange(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onStatusUpdate(String arg0, String arg1, int arg2, int arg3,
			int arg4, int arg5) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdate(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateClipList(String arg0, int arg1, String arg2) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateEditInfo(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateHistogram(String arg0, int[] arg1, int[] arg2,
			int[] arg3, int[] arg4, int arg5, int arg6, String arg7) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateInt(String arg0, String arg1, int arg2) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateListStrings(String arg0, String arg1, String arg2,
			String[] arg3, int[] arg4) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateString(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateStringEditInfo(String arg0, String arg1, int[] arg2,
			String[] arg3) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpdateTag(String arg0, String arg1) {
		// TODO Auto-generated method stub
		
	}
}
