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

#import "TextControl.h"
#import "RCPHandler.h"
#import "TabViewController.h"
#import "Logger.h"

@interface TextControl()

@property (retain, nonatomic) UILabel *titleLabel;
@property (retain, nonatomic) UILabel *valueLabel;
@property (retain, nonatomic) UIActivityIndicatorView *activityIndicator;

@property (retain, nonatomic) IntEditInfo *intEditInfo;
@property (retain, nonatomic) UIntEditInfo *uintEditInfo;
@property (retain, nonatomic) StrEditInfo *strEditInfo;
@property (retain, nonatomic) NSTimer *activityTimer;

@property (assign, nonatomic) int32_t currentIntValue;
@property (assign, nonatomic) uint32_t currentUIntValue;
@property (retain, nonatomic) NSString *currentStrValue;

- (void)textInputView:(TextInputView *)textInputView sentIntValue:(const int)val;
- (void)textInputView:(TextInputView *)textInputView sentUIntValue:(const uint32_t)val;
- (void)textInputView:(TextInputView *)textInputView sentStrValue:(NSString *)val;

- (void)tapped;
- (void)triggerActivityIndicator;
- (void)showActivityIndicator;
- (void)hideActivityIndicator;

@end


@implementation TextControl

@synthesize delegate, paramID, sendValueType, valueBackgroundColor, valueTextColor, valueCornerRadius, valueAlignment, valueFontSize;

- (TextControl *)initWithParam:(rcp_param_t)param title:(BOOL)hasTitle
{
    if (self = [super init])
    {
        delegate = nil;
        paramID = param;
        sendValueType = SEND_VALUE_TYPE_INT;
        
        _titleLabel = hasTitle ? [[UILabel alloc] init] : nil;
        _valueLabel = [[UILabel alloc] init];
        _activityIndicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhite];
        
        _intEditInfo = [[IntEditInfo alloc] init];
        _uintEditInfo = [[UIntEditInfo alloc] init];
        _strEditInfo = [[StrEditInfo alloc] init];
        _activityTimer = nil;
        
        _currentIntValue = 0;
        _currentUIntValue = 0;
        _currentStrValue = [[NSString alloc] init];
        
        [self setValueBackgroundColor:[UIColor blackColor]];
        [self setValueTextColor:[UIColor whiteColor]];
        [self setValueCornerRadius:5.0];
        [self setValueAlignment:NSTextAlignmentCenter];
        [self setValueFontSize:16.0];
        
        UITapGestureRecognizer *singleTap = [[UITapGestureRecognizer alloc] init];
        singleTap.cancelsTouchesInView = NO;
        [singleTap addTarget:self action:@selector(tapped)];
        [_valueLabel setUserInteractionEnabled:YES];
        [_valueLabel addGestureRecognizer:singleTap];
        
        if (_titleLabel != nil)
        {
            [_titleLabel setTextColor:[UIColor whiteColor]];
            [_titleLabel setTextAlignment:NSTextAlignmentLeft];
            [_titleLabel setBackgroundColor:[UIColor clearColor]];
            
            [self addSubview:_titleLabel];
        }
        
        [_valueLabel addSubview:_activityIndicator];
        [self addSubview:_valueLabel];
    }
    
    return self;
}

- (IntEditInfo *)intEditInfo
{
    return _intEditInfo;
}

- (UIntEditInfo *)uintEditInfo
{
    return _uintEditInfo;
}

- (StrEditInfo *)strEditInfo
{
    return _strEditInfo;
}

- (NSString *)baseValue
{
    switch (sendValueType)
    {
        case SEND_VALUE_TYPE_INT:
            if (_intEditInfo.isValid)
            {
                return [NSString stringWithFormat:@"%.*f", _intEditInfo.digits, _currentIntValue / _intEditInfo.divider];
            }
            break;
            
        case SEND_VALUE_TYPE_UINT:
            if (_uintEditInfo.isValid)
            {
                return [NSString stringWithFormat:@"%.*f", _uintEditInfo.digits, _currentUIntValue / _uintEditInfo.divider];
            }
            break;
            
        case SEND_VALUE_TYPE_STR:
            if (_strEditInfo.isValid)
            {
                NSString *value = [_valueLabel text];
                value = [value stringByReplacingOccurrencesOfString:_strEditInfo.prefixDecoded withString:@"" options:NSCaseInsensitiveSearch range:NSMakeRange(0, [value length])];
                value = [value stringByReplacingOccurrencesOfString:_strEditInfo.suffixDecoded withString:@"" options:NSBackwardsSearch range:NSMakeRange(0, [value length])];
                
                return value;
            }
            break;
            
        default:
            break;
    }
    
    return [_valueLabel text];
}

- (BOOL)isEditable
{
    switch (sendValueType)
    {
        case SEND_VALUE_TYPE_INT:
            return _intEditInfo.isValid;
            
        case SEND_VALUE_TYPE_UINT:
            return _uintEditInfo.isValid;
            
        case SEND_VALUE_TYPE_STR:
            return _strEditInfo.isValid;
            
        default:
            break;
    }
    
    return NO;
}

- (void)updateInt:(const int)val editInfo:(rcp_cur_int_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    _currentIntValue = val;
    
    _intEditInfo.isValid = infoIsValid;
    
    if (_intEditInfo.isValid)
    {
        _intEditInfo.minValue = info.min;
        _intEditInfo.maxValue = info.max;
        _intEditInfo.divider = (float)info.divider;
        _intEditInfo.digits = info.digits;
        _intEditInfo.step = info.step;
        _intEditInfo.prefixEncoded = (info.prefix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix];
        _intEditInfo.prefixDecoded = (info.prefix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix_decoded];
        _intEditInfo.suffixEncoded = (info.suffix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix];
        _intEditInfo.suffixDecoded = (info.suffix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix_decoded];
    }
}

- (void)updateUInt:(const uint32_t)val editInfo:(rcp_cur_uint_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    _currentUIntValue = val;
    
    _uintEditInfo.isValid = infoIsValid;
    
    if (_uintEditInfo.isValid)
    {
        _uintEditInfo.minValue = info.min;
        _uintEditInfo.maxValue = info.max;
        _uintEditInfo.divider = (float)info.divider;
        _uintEditInfo.digits = info.digits;
        _uintEditInfo.step = info.step;
        _uintEditInfo.prefixEncoded = (info.prefix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix];
        _uintEditInfo.prefixDecoded = (info.prefix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix_decoded];
        _uintEditInfo.suffixEncoded = (info.suffix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix];
        _uintEditInfo.suffixDecoded = (info.suffix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix_decoded];
    }
}

- (void)updateStr:(NSString *)str
{
    _currentStrValue = str;
    
    if (_activityTimer != nil)
    {
        [_activityTimer invalidate];
        _activityTimer = nil;
    }
    
    [self hideActivityIndicator];
    [_valueLabel setText:str];
}

- (void)updateStr:(NSString *)str editInfo:(rcp_cur_str_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    _currentStrValue = str;
    
    _strEditInfo.isValid = infoIsValid;
    
    NSString *displayStr = _currentStrValue;
    
    if (_strEditInfo.isValid)
    {
        _strEditInfo.minLength = info.min_len;
        _strEditInfo.maxLength = info.max_len;
        _strEditInfo.isPassword = info.is_password;
        _strEditInfo.allowedChars = (info.allowed_characters == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.allowed_characters];
        _strEditInfo.prefixEncoded = (info.prefix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix];
        _strEditInfo.prefixDecoded = (info.prefix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix_decoded];
        _strEditInfo.suffixEncoded = (info.suffix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix];
        _strEditInfo.suffixDecoded = (info.suffix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix_decoded];
        
        if (_strEditInfo.isPassword)
        {
            displayStr = [RCPUtil passwordStringFromString:_currentStrValue];
        }
    }
    
    if (_activityTimer != nil)
    {
        [_activityTimer invalidate];
        _activityTimer = nil;
    }
    
    [self hideActivityIndicator];
    [_valueLabel setText:displayStr];
}

- (void)setFrame:(CGRect)frame
{
    const CGSize activityIndicatorSize = CGSizeMake(20.0, 20.0);
    const CGFloat activityIndicatorPadding = 5.0;
    
    const CGFloat verticalSpacing = 3.0;
    CGRect titleContainer = CGRectZero;
    
    
    [super setFrame:frame];
    
    if (_titleLabel != nil)
    {
        titleContainer = CGRectMake(0.0, 0.0, self.bounds.size.width, (self.bounds.size.height / 3.25) + verticalSpacing);
        
        [_titleLabel setFrame:CGRectMake(titleContainer.origin.x, titleContainer.origin.y, titleContainer.size.width, titleContainer.size.height - verticalSpacing)];
        [_titleLabel setFont:[UIFont systemFontOfSize:[RCPUtil maxSystemFontSizeForContainer:_titleLabel.frame bold:NO]]];
    }
    
    const CGFloat remainingHeight = self.bounds.size.height - (titleContainer.origin.y + titleContainer.size.height);
    
    [_valueLabel setFrame:CGRectMake(0.0, titleContainer.origin.y + titleContainer.size.height, self.bounds.size.width, remainingHeight)];
    
    
    const CGFloat activityIndicatorY = (_valueLabel.bounds.size.height - activityIndicatorSize.height) / 2.0;
    
    if (valueAlignment == NSTextAlignmentLeft)
    {
        [_activityIndicator setFrame:CGRectMake(activityIndicatorPadding, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else if (valueAlignment == NSTextAlignmentRight)
    {
        [_activityIndicator setFrame:CGRectMake(_valueLabel.bounds.size.width - activityIndicatorSize.width - activityIndicatorPadding, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else // defaults to center alignment
    {
        [_activityIndicator setFrame:CGRectMake((_valueLabel.bounds.size.width - activityIndicatorSize.width) / 2.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
}

- (void)setValueBackgroundColor:(UIColor *)color
{
    valueBackgroundColor = color;
    [_valueLabel setBackgroundColor:color];
}

- (void)setValueTextColor:(UIColor *)color
{
    valueTextColor = color;
    [_valueLabel setTextColor:color];
}

- (void)setValueCornerRadius:(CGFloat)radius
{
    valueCornerRadius = radius;
    
    if (radius > 0.0)
    {
        _valueLabel.clipsToBounds = YES;
        [_valueLabel.layer setCornerRadius:radius];
    }
    else
    {
        _valueLabel.clipsToBounds = NO;
        [_valueLabel.layer setCornerRadius:0.0];
    }
}

- (void)setValueAlignment:(NSTextAlignment)alignment
{
    valueAlignment = alignment;
    [_valueLabel setTextAlignment:alignment];
    
    const float activityIndicatorPadding = 5.0;
    const float activityIndicatorY = _activityIndicator.frame.origin.y;
    const CGSize activityIndicatorSize = _activityIndicator.bounds.size;
    
    if (valueAlignment == NSTextAlignmentLeft)
    {
        [_activityIndicator setFrame:CGRectMake(activityIndicatorPadding, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else if (valueAlignment == NSTextAlignmentRight)
    {
        [_activityIndicator setFrame:CGRectMake(_valueLabel.bounds.size.width - activityIndicatorSize.width - activityIndicatorPadding, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else // defaults to center alignment
    {
        [_activityIndicator setFrame:CGRectMake((_valueLabel.bounds.size.width - activityIndicatorSize.width) / 2.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
}

- (void)setValueFontSize:(CGFloat)size
{
    valueFontSize = size;
    [_valueLabel setFont:[UIFont systemFontOfSize:size]];
}

- (void)setTitle:(NSString *)title
{
    if (_titleLabel != nil)
    {
        [_titleLabel setText:title];
    }
}

- (void)setEnabled:(BOOL)enabled
{
    const float alpha = enabled ? 1.0 : 0.5;
    
    if (_titleLabel != nil)
    {
        [_titleLabel setAlpha:alpha];
        [_titleLabel setEnabled:enabled];
    }
    
    [_valueLabel setAlpha:alpha];
    [_valueLabel setEnabled:enabled];
}

- (void)requestTextInputView
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(textControlSelected:)])
        {
            [delegate textControlSelected:self];
        }
        
        [delegate textControlRequestedTextInputView:self];
    }
}

- (void)textInputView:(TextInputView *)textInputView sentIntValue:(const int)val
{
    if (val != _currentIntValue)
    {
        [self triggerActivityIndicator];
    }
    else
    {
        // This GET is followed by a SET in case an out-of-range value is sent multiple times in a row.
        // For example, if the user entered 1000 for color temperature (which has a minimum of 1700), then the CURRENT back will say 1700 and things will be fine (assuming it was not already at 1700).
        // However, if you sent out 1000 again, no CURRENT would be sent back because the value did not change (it was at 1700, and that "1000" was recognized as beyond the minimum boundary, so it remained at 1700).
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpGet:paramID];
    }
}

- (void)textInputView:(TextInputView *)textInputView sentUIntValue:(const uint32_t)val
{
    if (val != _currentUIntValue)
    {
        [self triggerActivityIndicator];
    }
    else
    {
        // This GET is followed by a SET in case an out-of-range value is sent multiple times in a row.
        // For example, if the user entered 1000 for color temperature (which has a minimum of 1700), then the CURRENT back will say 1700 and things will be fine (assuming it was not already at 1700).
        // However, if you sent out 1000 again, no CURRENT would be sent back because the value did not change (it was at 1700, and that "1000" was recognized as beyond the minimum boundary, so it remained at 1700).
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpGet:paramID];
    }
}

- (void)textInputView:(TextInputView *)textInputView sentStrValue:(NSString *)val
{
    if ([val compare:_currentStrValue] != NSOrderedSame)
    {
        [self triggerActivityIndicator];
    }
    else
    {
        // This GET is followed by a SET in case an out-of-range value is sent multiple times in a row.
        // For example, if the user entered 1000 for color temperature (which has a minimum of 1700), then the CURRENT back will say 1700 and things will be fine (assuming it was not already at 1700).
        // However, if you sent out 1000 again, no CURRENT would be sent back because the value did not change (it was at 1700, and that "1000" was recognized as beyond the minimum boundary, so it remained at 1700).
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpGet:paramID];
    }
}

- (void)tapped
{
    [self requestTextInputView];
}

- (void)triggerActivityIndicator
{
    _activityTimer = [NSTimer scheduledTimerWithTimeInterval:ACTIVITY_INDICATOR_DELAY_S target:self selector:@selector(showActivityIndicator) userInfo:nil repeats:NO];
}

- (void)showActivityIndicator
{
    [_valueLabel setText:@""];
    [_activityIndicator startAnimating];
    [_activityIndicator setHidden:NO];
}

- (void)hideActivityIndicator
{
    [_activityIndicator setHidden:YES];
    [_activityIndicator stopAnimating];
}

@end
