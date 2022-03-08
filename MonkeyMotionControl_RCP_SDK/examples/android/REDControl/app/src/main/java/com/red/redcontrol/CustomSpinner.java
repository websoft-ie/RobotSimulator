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

import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.widget.Spinner;

public class CustomSpinner extends Spinner {
	private long mTimeWhenEnabled = 0;
    private static final long SPINNER_CONTROL_LOCK_PERIOD_MS = 3000;
    
	public CustomSpinner(Context context) {
		super(context);
	}

    public CustomSpinner(Context context, AttributeSet attrs) {
    	super(context, attrs);
    }

    public CustomSpinner(Context context, AttributeSet attrs, int defStyle) {
    	super(context, attrs, defStyle);
    }
	
    @Override public void
    setSelection(int position, boolean animate) {
   		boolean sameSelected = position == getSelectedItemPosition();
   		super.setSelection(position, animate);
   		if (sameSelected) {
   			// Spinner does not call the OnItemSelectedListener if the same item is selected, so do it manually now
   			getOnItemSelectedListener().onItemSelected(this, getSelectedView(), position, getSelectedItemId());
   		}
    }

    @Override public void
    setSelection(int position) {
    	boolean sameSelected = position == getSelectedItemPosition();
    	super.setSelection(position);
    	if (sameSelected) {
    		// Spinner does not call the OnItemSelectedListener if the same item is selected, so do it manually now
    		getOnItemSelectedListener().onItemSelected(this, getSelectedView(), position, getSelectedItemId());
    	}
    }
    
    @Override
    public void setEnabled(boolean enabled) {
    	super.setEnabled(enabled);
    	mTimeWhenEnabled = System.currentTimeMillis();
    }
    
    // During the lock period, erroneous onItemSelected() callbacks from the Android framework shall be ignored.
    public boolean isLockPeriodOver() {
    	return (System.currentTimeMillis() - mTimeWhenEnabled > SPINNER_CONTROL_LOCK_PERIOD_MS);
    }
}
