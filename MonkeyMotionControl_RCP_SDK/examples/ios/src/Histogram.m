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

#import "Histogram.h"
#import "Logger.h"

@interface Histogram()

@property (nonatomic, assign) rcp_cur_hist_cb_data_t *data;
@property (nonatomic, retain) NSString *displayString;

- (id)initWithCoder:(NSCoder *)aDecoder;
- (void)drawRect:(CGRect)rect;

@end


@implementation Histogram

- (void)setHistogramData:(const rcp_cur_hist_cb_data_t*)histogramData
{
    memcpy(_data, histogramData, sizeof(rcp_cur_hist_cb_data_t));
    _displayString = [NSString stringWithFormat:@"%s", histogramData->display_str_decoded];
}

- (id)initWithCoder:(NSCoder *)aDecoder
{
    if (self = [super initWithCoder:aDecoder])
    {
        _data = (rcp_cur_hist_cb_data_t*)malloc(sizeof(rcp_cur_hist_cb_data_t));
        _displayString = [[NSString alloc] init];
        _redClipOn = NO;
        _greenClipOn = NO;
        _blueClipOn = NO;
    }
    
    return self;
}

- (void)drawRect:(CGRect)rect
{
    [super drawRect:rect];
    
    CGContextRef context = UIGraphicsGetCurrentContext();
    
    int i, j;
    CGRect botClipRect = CGRectMake(rect.origin.x + 1.0, rect.origin.y + 1.0, 10.0, rect.size.height - 2.0);
    const int widthScale = (rect.size.width - (botClipRect.size.width * 2) - 20) / 128;
    const int heightScale = rect.size.height / 16;
    const int colCount = sizeof(_data->red) / sizeof(unsigned int);
    CGRect histogramRect = CGRectMake(botClipRect.origin.x + botClipRect.size.width, rect.origin.y + 1.0, colCount * widthScale, rect.size.height - 1.0);
    CGRect topClipRect = CGRectMake(histogramRect.origin.x + histogramRect.size.width, rect.origin.y + 1.0, botClipRect.size.width, rect.size.height - 2.0);
    CGRect rectWithoutLights = CGRectMake(rect.origin.x, rect.origin.y, botClipRect.size.width + histogramRect.size.width + topClipRect.size.width, rect.size.height);
    CGRect lightRect = CGRectMake(botClipRect.origin.x + botClipRect.size.width + histogramRect.size.width + topClipRect.size.width + 1.0, rect.origin.y, rect.size.width - topClipRect.size.width - histogramRect.size.width - topClipRect.size.width - 4.0, rect.size.height - 1.0);
    CGPoint redClip = CGPointMake(lightRect.origin.x + (lightRect.size.width / 2.0) + 1.0, lightRect.origin.y + 12.0);
    CGPoint greenClip = CGPointMake(redClip.x, redClip.y + 20.0);
    CGPoint blueClip = CGPointMake(redClip.x, greenClip.y + 20.0);
    

    // Set up line cap/width and make the rectangle black
    CGContextSetLineCap(context, kCGLineCapRound);
    CGContextSetLineJoin(context, kCGLineJoinRound);
    CGContextSetLineWidth(context, 1.0);
    CGContextSetRGBFillColor(context, 0.0, 0.0, 0.0, 1.0);
    CGContextFillRect(context, rectWithoutLights);
    
    
    // Draw the clipping bars
    CGContextSetRGBStrokeColor(context, 1.0, 1.0, 1.0, 1.0);
    CGContextStrokeRect(context, botClipRect);
    CGContextStrokeRect(context, topClipRect);
    
    
    // Fill the clipping bars
    CGContextSetRGBFillColor(context, 1.0, 0.0, 0.0, 1.0);

    CGFloat width = botClipRect.size.width - 2.0;
    CGFloat height = (botClipRect.size.height - 2.0) * (_data->bottom_clip * 0.01);
    CGFloat x = botClipRect.origin.x + 1.0;
    CGFloat y = botClipRect.origin.y + botClipRect.size.height - height - 1.0;
    CGContextFillRect(context, CGRectMake(x, y, width, height));
    
    width = topClipRect.size.width - 2.0;
    height = (topClipRect.size.height - 2.0) * (_data->top_clip * 0.01);
    x = topClipRect.origin.x + 1.0;
    y = topClipRect.origin.y + topClipRect.size.height - height - 1.0;
    CGContextFillRect(context, CGRectMake(x, y, width, height));
    
    
    // Draw histogram
    
    // Red
    CGContextSetRGBStrokeColor(context, 1.0, 0.0, 0.0, 1.0);
    CGContextSetLineWidth(context, 1.0);
    for (i = 0; i < 128; i++)
    {
        CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height);
        CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height - (_data->red[i] * heightScale));
        CGContextStrokePath(context);
        
        if (i < 127)
        {
            for (j = 1; j < widthScale; j++)
            {
                y = histogramRect.origin.y + histogramRect.size.height - (((_data->red[i] + _data->red[i + 1]) / 2) * heightScale);

                CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale) + j, histogramRect.origin.y + histogramRect.size.height);
                CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale) + j, y);
                CGContextStrokePath(context);
            }
        }
    }
    
    // Green
    CGContextSetRGBStrokeColor(context, 0.0, 1.0, 0.0, 0.70);
    for (i = 0; i < colCount; i++)
    {
        CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height);
        CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height - (_data->green[i] * heightScale));
        CGContextStrokePath(context);
        
        if (i < 127)
        {
            for (j = 1; j < widthScale; j++)
            {
                y = histogramRect.origin.y + histogramRect.size.height - (((_data->green[i] + _data->green[i + 1]) / 2) * heightScale);
                
                CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale) + j, histogramRect.origin.y + histogramRect.size.height);
                CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale) + j, y);
                CGContextStrokePath(context);
            }
        }
    }
    
    // Blue
    CGContextSetRGBStrokeColor(context, 0.0, 0.0, 1.0, 0.75);
    for (i = 0; i < colCount; i++)
    {
        CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height);
        CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height - (_data->blue[i] * heightScale));
        CGContextStrokePath(context);
        
        if (i < 127)
        {
            for (j = 1; j < widthScale; j++)
            {
                y = histogramRect.origin.y + histogramRect.size.height - (((_data->blue[i] + _data->blue[i + 1]) / 2) * heightScale);
                
                CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale) + j, histogramRect.origin.y + histogramRect.size.height);
                CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale) + j, y);
                CGContextStrokePath(context);
            }
        }
    }
    
    // Luma
    CGContextSetRGBStrokeColor(context, 0.35, 0.35, 0.35, 1.0 );
    for (i = 0; i < colCount; i++)
    {
        CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height);
        CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale), histogramRect.origin.y + histogramRect.size.height - (_data->luma[i] * heightScale));
        CGContextStrokePath(context);
        
        if (i < 127)
        {
            for (j = 1; j < widthScale; j++)
            {
                y = histogramRect.origin.y + histogramRect.size.height - (((_data->luma[i] + _data->luma[i + 1]) / 2) * heightScale);
                
                CGContextMoveToPoint(context, histogramRect.origin.x + (i * widthScale) + j, histogramRect.origin.y + histogramRect.size.height);
                CGContextAddLineToPoint(context, histogramRect.origin.x + (i * widthScale) + j, y);
                CGContextStrokePath(context);
            }
        }
    }
    
    
    // Draw traffic lights
    if (_redClipOn)
    {
        CGContextSetRGBFillColor(context, 0.95, 0.0, 0.0, 1.0);
        CGContextSetRGBStrokeColor(context, 0.95, 0.0, 0.0, 1.0);
    }
    else
    {
        CGContextSetRGBFillColor(context, 0.15, 0.15, 0.15, 1.0);
        CGContextSetRGBStrokeColor(context, 0.15, 0.15, 0.15, 1.0);
    }
    
    CGContextBeginPath(context);
    CGContextAddArc(context, redClip.x, redClip.y, 5.0, 0.0, 2 * M_PI, YES);
    CGContextClosePath(context);
    CGContextDrawPath(context, kCGPathFillStroke);
    
    if (_greenClipOn)
    {
        CGContextSetRGBFillColor(context, 0.0, 0.95, 0.0, 1.0);
        CGContextSetRGBStrokeColor(context, 0.0, 0.95, 0.0, 1.0);
    }
    else
    {
        CGContextSetRGBFillColor(context, 0.15, 0.15, 0.15, 1.0);
        CGContextSetRGBStrokeColor(context, 0.15, 0.15, 0.15, 1.0);
    }
    
    CGContextBeginPath(context);
    CGContextAddArc(context, greenClip.x, greenClip.y, 5.0, 0.0, 2 * M_PI, YES);
    CGContextClosePath(context);
    CGContextDrawPath(context, kCGPathFillStroke);
    
    if (_blueClipOn)
    {
        CGContextSetRGBFillColor(context, 0.0, 0.0, 0.95, 1.0);
        CGContextSetRGBStrokeColor(context, 0.0, 0.0, 0.95, 1.0);
    }
    else
    {
        CGContextSetRGBFillColor(context, 0.15, 0.15, 0.15, 1.0);
        CGContextSetRGBStrokeColor(context, 0.15, 0.15, 0.15, 1.0);
    }
    
    CGContextBeginPath(context);
    CGContextAddArc(context, blueClip.x, blueClip.y, 5.0, 0.0, 2 * M_PI, YES);
    CGContextClosePath(context);
    CGContextDrawPath(context, kCGPathFillStroke);
    
    
    // Draw text
    NSMutableParagraphStyle *style = [[NSParagraphStyle defaultParagraphStyle] mutableCopy];
    [style setAlignment:NSTextAlignmentCenter];
    
    NSDictionary *attributes = [NSDictionary dictionaryWithObjectsAndKeys:
                                style, NSParagraphStyleAttributeName,
                                [UIFont fontWithName:@"Helvetica" size:12], NSFontAttributeName,
                                [UIColor whiteColor], NSForegroundColorAttributeName,
                                [UIColor clearColor], NSBackgroundColorAttributeName,
                                nil];
    
    NSAttributedString *str = [[NSAttributedString alloc] initWithString:[NSString stringWithFormat:@"%@", _displayString] attributes:attributes];
    [str drawInRect:CGRectMake(histogramRect.origin.x, histogramRect.origin.y, histogramRect.size.width, 12.0)];
}

@end
