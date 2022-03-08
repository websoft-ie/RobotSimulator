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
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Paint.Align;
import android.graphics.RectF;
import android.util.AttributeSet;
import android.util.Log;
import android.view.View;

public class HistogramView extends View {
	
	private static final String TAG = "HistogramView";
	private float CANVAS_MARGIN = 5;
	private static final int NBINS = 128;
	private static final int HIST_MAX = 16;
	private static final float GOALPOST_MAX = 100;
	
	private Paint mPaintBlack;
	private Paint mPaintWhite;
	private Paint mPaintRed;
	private Paint mPaintGreen;
	private Paint mPaintBlue;
	private Paint mPaintGray;
	private Paint mPaintText;
	private int[] mRedHistogram;
	private int[] mGreenHistogram;
	private int[] mBlueHistogram;
	private int[] mLumaHistogram;

	private int mRedClip = 0;
	private int mGreenClip = 0;
	private int mBlueClip = 0;
	
	String mText;
	int mBottomClip = 0;
	int mTopClip = 0;
	RectF mBarRect;
	
    public HistogramView(Context context, AttributeSet attrs) {
		super(context, attrs);		
		init();		
	}
  
    public void setRedClip(int clip)
    {
    	mRedClip=clip;
    }
    public void setGreenClip(int clip)
    {
    	mGreenClip=clip;
    }
    public void setBlueClip(int clip)
    {
    	mBlueClip=clip;
    }
    public void setValues(int[] red, int[] green, int[] blue, int[] luma, int bottom_clip, int top_clip, String text) {
   	 
    	// Copy RGB to mRed,mGreen,mBlue..    	
    	System.arraycopy(red, 0, mRedHistogram, 0, red.length);
    	System.arraycopy(green, 0, mGreenHistogram, 0, green.length);
    	System.arraycopy(blue, 0, mBlueHistogram, 0, blue.length);
    	System.arraycopy(luma, 0, mLumaHistogram, 0, luma.length);
    	
    	if (text != null) mText = text;
    	
    	mBottomClip = bottom_clip;
    	mTopClip = top_clip;
    	
    	// Force histogram to be drawn
    	invalidate();
    	requestLayout();
    }
    
    @Override
    protected void onDraw (Canvas canvas) {
    	super.onDraw(canvas);
   	
    	int canvasWidth = canvas.getWidth();
    	int canvasHeight = canvas.getHeight();
    	
    	// Determine Box Sizes
    	float boxHeight = canvasHeight - CANVAS_MARGIN*2;
    	float boxStartX = CANVAS_MARGIN;
    	float boxStartY = CANVAS_MARGIN;
    	float boxEndX   = canvasWidth - CANVAS_MARGIN * 5;
    	float boxEndY	  = canvasHeight - CANVAS_MARGIN;
    	
    	
    	// Determine Goal Post sizes
    	float goalWidth = CANVAS_MARGIN * 2;
    	float bGoalX = boxStartX + goalWidth;
    	float tGoalX = boxEndX - goalWidth;
    	float goalScale = ((float) boxHeight) / GOALPOST_MAX;
    	
    	//float histWidth = boxWidth - goalWidth*2;
    	float histWidth = boxEndX-boxStartX+1 - goalWidth*2;
    	float histScale =((float) boxHeight) / HIST_MAX;
    	float barWidth = ((float)histWidth) / NBINS;
    	float histStartX = boxStartX + goalWidth;
    	
    	if (mRedClip == 1) {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, canvasHeight/4, CANVAS_MARGIN*(float)1.5, mPaintRed);
    	} else {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, canvasHeight/4, CANVAS_MARGIN*(float)1.5, mPaintBlack);
    	}
    	
    	if (mGreenClip == 1) {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, canvasHeight/2, CANVAS_MARGIN*(float)1.5, mPaintGreen);
    	} else {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, canvasHeight/2, CANVAS_MARGIN*(float)1.5, mPaintBlack);
    	}
    	
    	if (mBlueClip == 1) {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, (canvasHeight*3)/4, CANVAS_MARGIN*(float)1.5, mPaintBlue);
    	} else {
    		canvas.drawCircle(canvasWidth-CANVAS_MARGIN*2, (canvasHeight*3)/4, CANVAS_MARGIN*(float)1.5, mPaintBlack);
    	}
    	
    	//---------------------------------------------------------------------
    	// Draw bounding box   	
    	//---------------------------------------------------------------------
    	//Log.i(TAG, "Drawing bounding box");
    	canvas.drawRect(boxStartX, boxStartY, boxEndX, boxEndY, mPaintBlack);
    	canvas.drawRect(boxStartX, boxStartY, boxEndX, boxEndY, mPaintWhite);
 
    	//---------------------------------------------------------------------
    	// Draw goal posts   	
    	//---------------------------------------------------------------------

    	canvas.drawLine(bGoalX, boxStartY, bGoalX, boxEndY, mPaintWhite);
    	canvas.drawLine(tGoalX, boxStartY, tGoalX, boxEndY, mPaintWhite);
    	
    	if (mBottomClip > 0) {
	    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
	    	mBarRect.left = boxStartX + mPaintWhite.getStrokeWidth();
	    	mBarRect.right = bGoalX - mPaintWhite.getStrokeWidth();
	    	mBarRect.top = mBarRect.bottom - mBottomClip * goalScale + mPaintWhite.getStrokeWidth()*2;
	    	canvas.drawRect(mBarRect, mPaintRed);
    	}
    	if (mTopClip > 0) {
	    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
	    	mBarRect.left = tGoalX + mPaintWhite.getStrokeWidth();
	    	mBarRect.right = boxEndX - mPaintWhite.getStrokeWidth();
	    	mBarRect.top = mBarRect.bottom - mTopClip * goalScale + mPaintWhite.getStrokeWidth()*2;
	    	canvas.drawRect(mBarRect, mPaintRed);
    	}

    	//---------------------------------------------------------------------
    	// Draw red intensity histogram   	
    	//---------------------------------------------------------------------
    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
    	mBarRect.left = histStartX;
    	mBarRect.right = mBarRect.left + barWidth;
    	for (int bin = 0; bin < NBINS; bin++)
    	{
    		mBarRect.top = mBarRect.bottom - mRedHistogram[bin] * histScale;
    		canvas.drawRect(mBarRect, mPaintRed);
    		mBarRect.left += barWidth;
    		mBarRect.right += barWidth;
    	} 
    	
    	//---------------------------------------------------------------------
    	// Draw green intensity histogram   	
    	//---------------------------------------------------------------------
    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
    	mBarRect.left = histStartX;
    	mBarRect.right = mBarRect.left + barWidth;    	
    	for (int bin = 0; bin < NBINS; bin++)
    	{  		
    		mBarRect.top = mBarRect.bottom - mGreenHistogram[bin] * histScale;
    		canvas.drawRect(mBarRect, mPaintGreen);
    		mBarRect.left += barWidth;
    		mBarRect.right += barWidth;
    	} 
    	
    	//---------------------------------------------------------------------
    	// Draw blue intensity histogram   	
    	//---------------------------------------------------------------------
    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
    	mBarRect.left = histStartX;
    	mBarRect.right = mBarRect.left + barWidth;
    	for (int bin = 0; bin < NBINS; bin++)
    	{
    		mBarRect.top = mBarRect.bottom - mBlueHistogram[bin] * histScale;
    		canvas.drawRect(mBarRect, mPaintBlue);
    		mBarRect.left += barWidth;
    		mBarRect.right += barWidth;
    	}   	
    	
    	//---------------------------------------------------------------------
    	// Draw luma intensity histogram   	
    	//---------------------------------------------------------------------
    	mBarRect.bottom = canvasHeight - CANVAS_MARGIN - mPaintWhite.getStrokeWidth();
    	mBarRect.left = histStartX;
    	mBarRect.right = mBarRect.left + barWidth;
    	for (int bin = 0; bin < NBINS; bin++)
    	{
    		mBarRect.top = mBarRect.bottom - mLumaHistogram[bin] * histScale;
    		canvas.drawRect(mBarRect, mPaintGray);
    		mBarRect.left += barWidth;
    		mBarRect.right += barWidth;
    	}
    	
    	canvas.drawText(mText, boxEndX/2, CANVAS_MARGIN * 4, mPaintText);

    }
    
    @Override
    protected void onSizeChanged(int w, int h, int oldw, int oldh) {
    	super.onSizeChanged(w, h, oldw, oldh);
    	// Calculate sizes of things here based on the new and old size
    	
    }

    
    private void init() {

    	// use to initialize drawing objects (don't do this in onDraw(), expensive) 
    	
    	float densityMult = getContext().getResources().getDisplayMetrics().density;
    	
    	CANVAS_MARGIN = 5 * densityMult;
    	
    	mBarRect = new RectF();
    	
        mRedHistogram = new int[128];
        mGreenHistogram = new int[128];
        mBlueHistogram = new int[128];
        mLumaHistogram = new int[128];    	  	   	    	
       
        
        mText = new String(" ");
        
        mPaintText = new Paint();
        mPaintText.setTextAlign(Align.CENTER);
        mPaintText.setColor(Color.WHITE);
        
        mPaintText.setTextSize(15 * densityMult);
        
        Log.i(TAG, "density mult = " + Float.toString(densityMult));
        
        mPaintBlack = new Paint();
        mPaintBlack.setStyle(Paint.Style.FILL);
        mPaintBlack.setColor(Color.BLACK);
        mPaintBlack.setTextSize((float)12.5 * densityMult);
        
        mPaintWhite = new Paint();
        mPaintWhite.setStyle(Paint.Style.STROKE);
        mPaintWhite.setColor(Color.WHITE);
        mPaintWhite.setStrokeWidth(1 * densityMult);
        mPaintWhite.setTextSize((float)12.5 * densityMult);
        
        mPaintRed = new Paint();
        mPaintRed.setStyle(Paint.Style.FILL);
        mPaintRed.setColor(Color.RED);
        mPaintRed.setTextSize((float)12.5 * densityMult);
        mPaintRed.setAlpha(180);
        
        mPaintGreen = new Paint();
        mPaintGreen.setStyle(Paint.Style.FILL);
        mPaintGreen.setColor(Color.GREEN);
        mPaintGreen.setTextSize((float)12.5 * densityMult);
        mPaintGreen.setAlpha(160);
        
        mPaintBlue = new Paint();
        mPaintBlue.setStyle(Paint.Style.FILL);
        mPaintBlue.setColor(Color.BLUE);
        mPaintBlue.setTextSize((float)12.5 * densityMult);
        mPaintBlue.setAlpha(180);
        
        mPaintGray = new Paint();
        mPaintGray.setStyle(Paint.Style.FILL);
        mPaintGray.setColor(Color.LTGRAY);
        mPaintGray.setTextSize((float)12.5 * densityMult);
        mPaintGray.setAlpha(255);
        
    }
}
