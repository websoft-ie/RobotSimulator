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
import java.util.Arrays;

public class RCPList {

	String mName;
	ArrayList<String> mStrings;
	ArrayList<Integer> mValues;
	int mLength;
	int mCurrentIndex;
	int mMinValid;
	int mMinValue;
	int mMaxValid;
	int mMaxValue;
	
	public RCPList() {
		
	}

	public void setAllStrings(String listHeader, String listStrings, String listValues) {
		
		// Populate member values from the listHeader: 
		// <NAME>|<LENGTH>|<CURRENT_INDEX>|<MIN_VALID>|<MIN_VALUE>|<MAX_VALID>|<MAX_VALUE>
		String[] listParms = listHeader.split("\\|");
		if (listParms.length == 7) {
			mName = listParms[0];
			mLength = Integer.parseInt(listParms[1]);		
			mCurrentIndex = Integer.parseInt(listParms[2]);
			mMinValid = Integer.parseInt(listParms[3]);
			mMinValue = Integer.parseInt(listParms[4]);
			mMaxValid = Integer.parseInt(listParms[5]);
			mMaxValue = Integer.parseInt(listParms[6]);
		}
		// Populate the strings array list
		mStrings = new ArrayList<String> (Arrays.asList(listStrings.split("\\|")));
		
		// Populate the values array list
		String[] numberStrs = listValues.split("\\|");
		mValues = new ArrayList<Integer>();
		for (int ii = 0; ii < numberStrs.length; ii++) {
			mValues.add(Integer.parseInt(numberStrs[ii]));
		}

	}
	
	public void setName(String name) { 
		this.mName = name; 
	}
	
	public String getName()	 { 
		return this.mName; 
	}
	public int getLength()	{ 
		return this.mLength;
	}
	
	public int getIndex() {
		return this.mCurrentIndex;
	}

	public ArrayList<String> getStrings() {
		return mStrings;
	}
	
	public int getValue(int index) {
		return (mValues.get(index));
	}	
}
