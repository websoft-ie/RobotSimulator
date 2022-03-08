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

import android.os.Parcel;
import android.os.Parcelable;

public class Camera implements Parcelable {
	
	public String id;
	public String pin;
	public String type;
	public String version;
	public String ip;
	public String connectionIf;

    public static final Parcelable.Creator<Camera> CREATOR = new Parcelable.Creator<Camera>() {
        public Camera createFromParcel(Parcel in) {
            return new Camera(in);
        }

        public Camera[] newArray(int size) {
            return new Camera[size];
        }
    };

	public Camera(String id, String pin, String type, String version, String connectionIf, String ip) {
		this.id 			= id;
		this.pin 			= pin;
		this.type 			= type;
		this.version 		= version;
		this.ip 			= ip;
		this.connectionIf 	= connectionIf;
	}

	public Camera(String str) {
		String[] camInfo = str.split("[:;]");
		if(6 == camInfo.length) {
			this.id 		  = camInfo[0];
			this.pin 		  = camInfo[1];
			this.type 		  = camInfo[2];
			this.version 	  = camInfo[3];
			this.connectionIf = camInfo[4];
			this.ip 		  = camInfo[5];
		} else {
			this.id 		  = "N/A";
			this.pin 		  = "N/A";
			this.type 		  = "N/A";
			this.version 	  = "N/A";
			this.connectionIf = "N/A";
			this.ip 		  = "N/A";
		}
	}
	
	public Camera(Parcel in) {
		this.id 		= in.readString();
		this.pin 		= in.readString();
		this.type 		= in.readString();
		this.version	= in.readString();
		this.connectionIf = in.readString();
		this.ip 		= in.readString();
	}

	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel out, int flags) {
		out.writeString(id);
		out.writeString(pin);
		out.writeString(type);
		out.writeString(version);
		out.writeString(connectionIf);
		out.writeString(ip);
	}
	
	public boolean isComplete() {
		return !(null == this.id || null == this.pin || null == this.type || null == this.version || null == this.ip);
	}
}
