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

#import "RCPUtil.h"
#import "Logger.h"

#define PARAM_COUNT 27
static const rcp_param_t paramsWithSliders[PARAM_COUNT] =
{
    RCP_PARAM_SATURATION,
    RCP_PARAM_CONTRAST,
    RCP_PARAM_BRIGHTNESS,
    RCP_PARAM_EXPOSURE_COMPENSATION,
    RCP_PARAM_RED_GAIN,
    RCP_PARAM_GREEN_GAIN,
    RCP_PARAM_BLUE_GAIN,
    RCP_PARAM_ISO,
    RCP_PARAM_FLUT,
    RCP_PARAM_SHADOW,
    RCP_PARAM_COLOR_TEMPERATURE,
    RCP_PARAM_TINT,
    RCP_PARAM_LGG_LIFT_RED,
    RCP_PARAM_LGG_LIFT_GREEN,
    RCP_PARAM_LGG_LIFT_BLUE,
    RCP_PARAM_LGG_GAMMA_RED,
    RCP_PARAM_LGG_GAMMA_GREEN,
    RCP_PARAM_LGG_GAMMA_BLUE,
    RCP_PARAM_LGG_GAIN_RED,
    RCP_PARAM_LGG_GAIN_GREEN,
    RCP_PARAM_LGG_GAIN_BLUE,
    RCP_PARAM_MONITOR_BRIGHTNESS_LCD,
    RCP_PARAM_MONITOR_BRIGHTNESS_EVF,
    RCP_PARAM_MONITOR_BRIGHTNESS_REAR_LCD,
    RCP_PARAM_MONITOR_BRIGHTNESS_REAR_EVF,
    RCP_PARAM_MONITOR_BRIGHTNESS_SIDE_UI,
    RCP_PARAM_AE_EV_SHIFT
};

@implementation RCPUtil

+ (UIColor *)colorFromStatus:(rcp_param_status_t)status
{
    UIColor *color = nil;
    
    switch (status)
    {
        case RCP_PARAM_DISPLAY_STATUS_NORMAL:
            color = [UIColor whiteColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_GOOD:
            color = [UIColor greenColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_WARNING:
            color = [UIColor yellowColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_ERROR:
            color = [UIColor redColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_DISABLED:
            color = [UIColor grayColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_RECORDING:
            color = [UIColor redColor];
            break;
            
        case RCP_PARAM_DISPLAY_STATUS_FINALIZING:
            color = [UIColor yellowColor];
            break;
            
        default:
            color = [UIColor whiteColor];
            break;
    }
    
    return color;
}

+ (UITextRange *)selectedRangeForTextField:(UITextField *)textfield prefix:(NSString *)prefixToRemove suffix:(NSString *)suffixToRemove
{
    UITextRange *selectedRange = [textfield selectedTextRange];
    NSString *text = [textfield text];
    UITextPosition *start = nil;
    UITextPosition *end = nil;
    
    NSString *editableText = [text stringByReplacingOccurrencesOfString:prefixToRemove withString:@"" options:NSCaseInsensitiveSearch range:NSMakeRange(0, [text length])];
    editableText = [editableText stringByReplacingOccurrencesOfString:suffixToRemove withString:@"" options:NSBackwardsSearch range:NSMakeRange(0, [editableText length])];
    
    NSRange range = [text rangeOfString:editableText];
    
    start = [textfield positionFromPosition:selectedRange.start offset:range.location - [text length]];
    end = [textfield positionFromPosition:start offset:range.length];
    
    
    return [textfield textRangeFromPosition:start toPosition:end];
}

+ (CGSize)sizeOfText:(NSString *)text font:(UIFont *)font
{
    if (text == nil || font == nil)
    {
        return CGSizeZero;
    }
    
    NSAttributedString *str = [[NSAttributedString alloc] initWithString:text attributes:@{NSFontAttributeName:font}];
    CGRect rect = [str boundingRectWithSize:CGSizeMake(CGFLOAT_MAX, CGFLOAT_MAX) options:NSStringDrawingUsesLineFragmentOrigin context:nil];
    
    return rect.size;
}

+ (CGFloat)maxSystemFontSizeForContainer:(CGRect)container bold:(BOOL)bold
{
    CGFloat fontSize = 100.0; // initialized to max size
    CGFloat minFontSize = 6.0;
    
    while (fontSize > minFontSize)
    {
        UIFont *font = bold ? [UIFont boldSystemFontOfSize:fontSize] : [UIFont systemFontOfSize:fontSize];
        CGSize size = [RCPUtil sizeOfText:@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890`!@#$%^&*()~-=_+[]{};':\",./<>?|\\" font:font];
        
        if (size.height <= container.size.height)
        {
            break;
        }
        
        fontSize -= 1.0;
    }
    
    return fontSize;
}

+ (NSString *)decodedStringFromString:(const char *)str
{
    NSRange range;
    NSString *string = [NSString stringWithFormat:@"%s", str];
    
    while ((range = [string rangeOfString:@"&amp;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"&"];
    }
    
    while ((range = [string rangeOfString:@"&deg;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"\u00B0"];
    }
    
    while ((range = [string rangeOfString:@"&reg;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"\u00AE"];
    }
    
    while ((range = [string rangeOfString:@"&copy;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"\u00A9"];
    }
    
    while ((range = [string rangeOfString:@"&trade;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"\u2122"];
    }
    
    while ((range = [string rangeOfString:@"&redsub2;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"\u2082"];
    }
    
    while ((range = [string rangeOfString:@"&redana2;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" ANA 2"];
    }
    
    while ((range = [string rangeOfString:@"&redana13;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" ANA 1.3"];
    }
    
    while ((range = [string rangeOfString:@"&redana125;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" ANA 1.25"];
    }
    
    while ((range = [string rangeOfString:@"&redae;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"AE"];
    }
    
    while ((range = [string rangeOfString:@"&rediso;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"ISO "];
    }
    
    while ((range = [string rangeOfString:@"&redsec;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" sec"];
    }
    
    while ((range = [string rangeOfString:@"&redkelvin;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"K"];
    }
    
    while ((range = [string rangeOfString:@"&redformatk;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"K"];
    }
    
    while ((range = [string rangeOfString:@"&redfps;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" FPS"];
    }
    
    while ((range = [string rangeOfString:@"&red1over;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"1/"];
    }
    
    while ((range = [string rangeOfString:@"&redfover;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@"f/"];
    }
    
    while ((range = [string rangeOfString:@"&redav;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" Av"];
    }
    
    while ((range = [string rangeOfString:@"&redcheck;"]).location != NSNotFound)
    {
        string = [string stringByReplacingCharactersInRange:range withString:@" Check"];
    }
    
    return string;
}

+ (UIView *)decoratedViewForTextField:(UITextField *)textField prefix:(NSString *)decodedPrefix suffix:(NSString *)decodedSuffix
{
    // Trims the strings
    decodedPrefix = [decodedPrefix stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
    decodedSuffix = [decodedSuffix stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
    
    
    // textHeight is calculated separately from textWidth because if the text field is empty, the height will report back 0; however it's still necessary to give the prefix/suffix labels the correct height
    const CGFloat spacing = 2.0;
    const CGFloat textWidth = [RCPUtil sizeOfText:textField.text font:textField.font].width;
    const CGFloat textHeight = [RCPUtil sizeOfText:@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()-=_+[]\\{}|;':\",./<>?`~" font:textField.font].height;
    const CGFloat prefixWidth = [RCPUtil sizeOfText:decodedPrefix font:textField.font].width;
    const CGFloat suffixWidth = [RCPUtil sizeOfText:decodedSuffix font:textField.font].width;
    CGFloat x = ((textField.bounds.size.width - textWidth) / 2.0) - prefixWidth + spacing;
    
    UIView *decoratedView = [[UIView alloc] initWithFrame:CGRectMake(0.0, 0.0, textField.bounds.size.width, textField.bounds.size.height)];
    UILabel *prefixLabel = [[UILabel alloc] init];
    UILabel *suffixLabel = [[UILabel alloc] init];
    
    
    // Only works for centered text fields
    if (textField.textAlignment == NSTextAlignmentCenter)
    {
        // The -1.5 in the y coordinate is to account for the fact that text is not exactly vertically centered on text fields
        [prefixLabel setFrame:CGRectMake(x, ((textField.bounds.size.height - textHeight) / 2.0) - 1.5, prefixWidth, textHeight)];
        [prefixLabel setAdjustsFontSizeToFitWidth:NO];
        [prefixLabel setLineBreakMode:NSLineBreakByTruncatingTail];
        [prefixLabel setFont:textField.font];
        [prefixLabel setTextColor:textField.textColor];
        [prefixLabel setText:decodedPrefix];
        
        x += prefixWidth + textWidth;
        
        [suffixLabel setFrame:CGRectMake(x, ((textField.bounds.size.height - textHeight) / 2.0) - 1.5, suffixWidth, textHeight)];
        [suffixLabel setAdjustsFontSizeToFitWidth:NO];
        [suffixLabel setLineBreakMode:NSLineBreakByTruncatingTail];
        [suffixLabel setFont:textField.font];
        [suffixLabel setTextColor:textField.textColor];
        [suffixLabel setText:decodedSuffix];
        
        
        [decoratedView addSubview:prefixLabel];
        [decoratedView addSubview:suffixLabel];
    }
    
    return decoratedView;
}

+ (void)showDialogWithTitle:(NSString *)title message:(NSString *)message
{
    UIAlertView *dialog = [[UIAlertView alloc] initWithTitle:title message:message delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
    [dialog show];
}

+ (BOOL)menuParamHasSlider:(rcp_param_t)param
{
    int i;
    
    for (i = 0; i < PARAM_COUNT; i++)
    {
        if (param == paramsWithSliders[i])
        {
            return YES;
        }
    }
    
    return NO;
}

+ (NSString *)passwordStringFromString:(NSString *)str
{
    NSMutableString *passwordStr = [[NSMutableString alloc] init];
    const NSUInteger passwordLength = [str length];
    NSUInteger i;
    
    for (i = 0; i < passwordLength; i++)
    {
        [passwordStr appendString:@"*"];
    }
    
    return passwordStr;
}

@end
