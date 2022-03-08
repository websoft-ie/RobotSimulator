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

#import "SliderControl.h"
#import "Slider.h"
#import "IntEditInfo.h"
#import "RCPHandler.h"

@interface SliderControl()

@property (retain, nonatomic) UILabel *titleLabel;
@property (retain, nonatomic) Slider *slider;
@property (retain, nonatomic) PickerControl *pickerControl;

@property (retain, nonatomic) NSTimer *sliderTimer;
@property (assign, nonatomic) BOOL ignoreCameraNumUpdates;


- (void)sliderDown;
- (void)sliderUp;
- (void)sliderMoved;
- (void)acceptCameraNumUpdates;

@end


@implementation SliderControl

@synthesize delegate, paramID, textFieldBackgroundColor, textFieldTextColor, textFieldCornerRadius, textFieldAlignment, textFieldFontSize, pickerBackgroundColor, pickerValidTextColor, pickerInvalidTextColor;

- (SliderControl *)initWithParam:(rcp_param_t)param title:(BOOL)hasTitle
{
    if (self = [super init])
    {
        delegate = nil;
        paramID = param;
        
        _titleLabel = hasTitle ? [[UILabel alloc] init] : nil;
        _slider = [[Slider alloc] init];
        _pickerControl = [[PickerControl alloc] initWithParam:paramID title:NO];
        
        _sliderTimer = nil;
        _ignoreCameraNumUpdates = NO;
        
        
        [_slider addTarget:self action:@selector(sliderDown) forControlEvents:UIControlEventTouchDown];
        [_slider addTarget:self action:@selector(sliderUp) forControlEvents:UIControlEventTouchUpInside];
        [_slider addTarget:self action:@selector(sliderUp) forControlEvents:UIControlEventTouchUpOutside];
        [_slider addTarget:self action:@selector(sliderMoved) forControlEvents:UIControlEventValueChanged];
        
        [self setTextFieldBackgroundColor:[UIColor blackColor]];
        [self setTextFieldTextColor:[UIColor whiteColor]];
        [self setTextFieldCornerRadius:5.0];
        [self setTextFieldAlignment:NSTextAlignmentCenter];
        [self setTextFieldFontSize:16.0];
        [self setPickerBackgroundColor:[UIColor colorWithRed:43.0/255.0 green:43.0/255.0 blue:43.0/255.0 alpha:1.0]];
        [self setPickerValidTextColor:[UIColor whiteColor]];
        [self setPickerInvalidTextColor:[UIColor colorWithRed:20.0/255.0 green:20.0/255.0 blue:20.0/255.0 alpha:1.0]];
        
        if (_titleLabel != nil)
        {
            [_titleLabel setTextColor:[UIColor whiteColor]];
            [_titleLabel setTextAlignment:NSTextAlignmentLeft];
            [_titleLabel setBackgroundColor:[UIColor clearColor]];
            
            [self addSubview:_titleLabel];
        }
        
        [self addSubview:_slider];
        [self addSubview:_pickerControl];
    }
    
    return self;
}

- (BOOL)ignoreCameraNumUpdates
{
    return _ignoreCameraNumUpdates;
}

- (BOOL)ignoreCameraListUpdates
{
    return _pickerControl.ignoreCameraListUpdates;
}

- (BOOL)isEditable
{
    return [_pickerControl isEditable];
}

- (BOOL)isOpen
{
    return [_pickerControl isOpen];
}

- (int)selectedRow
{
    return [_pickerControl selectedRow];
}

- (void)updateInt:(const int)val editInfo:(rcp_cur_int_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    [_pickerControl updateInt:val editInfo:info infoIsValid:infoIsValid];
    [_slider setValue:[_pickerControl.rcpList indexFromInt:val]];
}

- (void)updateUInt:(const uint32_t)val editInfo:(rcp_cur_uint_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    [_pickerControl updateUInt:val editInfo:info infoIsValid:infoIsValid];
    [_slider setValue:[_pickerControl.rcpList indexFromUInt:val]];
}

- (void)updateStr:(NSString *)str
{
    [_pickerControl updateStr:str];
    
    if (_pickerControl.rcpList.sendValueType == SEND_VALUE_TYPE_STR)
    {
        [_slider setValue:[_pickerControl.rcpList indexFromStr:str]];
    }
}

- (void)updateStr:(NSString *)str editInfo:(rcp_cur_str_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    [_pickerControl updateStr:str editInfo:info infoIsValid:infoIsValid];
    
    if (_pickerControl.rcpList.sendValueType == SEND_VALUE_TYPE_STR)
    {
        [_slider setValue:[_pickerControl.rcpList indexFromStr:str]];
    }
}

- (void)updateList:(RCPList *)list
{
    [_pickerControl updateList:list];
    
    [_slider setMinimumValue:0.0];
    [_slider setMaximumValue:([_pickerControl.rcpList count] - 1)];
    
    if (!_ignoreCameraNumUpdates)
    {
        [_slider setValue:_pickerControl.rcpList.currentRow];
    }
}

- (void)setFrame:(CGRect)frame
{
    [super setFrame:frame];
    
    const CGFloat horizontalSpacing = 5.0;
    const CGFloat verticalSpacing = 3.0;
    CGRect titleContainer = CGRectZero;
    
    if (_titleLabel != nil)
    {
        titleContainer = CGRectMake(0.0, 0.0, self.bounds.size.width, (self.bounds.size.height / 3.25) + verticalSpacing);
        
        [_titleLabel setFrame:CGRectMake(titleContainer.origin.x, titleContainer.origin.y, titleContainer.size.width, titleContainer.size.height - verticalSpacing)];
        [_titleLabel setFont:[UIFont systemFontOfSize:[RCPUtil maxSystemFontSizeForContainer:_titleLabel.frame bold:NO]]];
    }
    
    const CGFloat remainingHeight = self.bounds.size.height - (titleContainer.origin.y + titleContainer.size.height);
    const CGSize sliderSize = CGSizeMake((2.0 * (self.bounds.size.width / 3.0)), 31.0);
    const CGSize pickerSize = CGSizeMake(self.bounds.size.width - sliderSize.width - horizontalSpacing, remainingHeight);
    
    [_slider setFrame:CGRectMake(0.0, titleContainer.origin.y + titleContainer.size.height + ((remainingHeight - sliderSize.height) / 2.0), sliderSize.width, sliderSize.height)];
    [_pickerControl setFrame:CGRectMake(_slider.frame.origin.x + sliderSize.width + horizontalSpacing, titleContainer.origin.y + titleContainer.size.height, pickerSize.width, pickerSize.height)];
}

- (void)setPickerDelegate:(id<PickerControlDelegate>)del
{
    _pickerControl.delegate = del;
}

- (void)setIgnoreCameraListUpdates:(BOOL)ignoreCameraListUpdates
{
    _pickerControl.ignoreCameraListUpdates = ignoreCameraListUpdates;
}

- (void)setTextFieldBackgroundColor:(UIColor *)color
{
    textFieldBackgroundColor = color;
    _pickerControl.textFieldBackgroundColor = color;
}

- (void)setTextFieldTextColor:(UIColor *)color
{
    textFieldTextColor = color;
    _pickerControl.textFieldTextColor = color;
}

- (void)setTextFieldCornerRadius:(CGFloat)radius
{
    textFieldCornerRadius = radius;
    _pickerControl.textFieldCornerRadius = radius;
}

- (void)setTextFieldAlignment:(NSTextAlignment)alignment
{
    textFieldAlignment = alignment;
    _pickerControl.textFieldAlignment = alignment;
}

- (void)setTextFieldFontSize:(CGFloat)size
{
    textFieldFontSize = size;
    _pickerControl.textFieldFontSize = size;
}

- (void)setPickerBackgroundColor:(UIColor *)color
{
    pickerBackgroundColor = color;
    _pickerControl.pickerBackgroundColor = color;
}

- (void)setPickerValidTextColor:(UIColor *)color
{
    pickerValidTextColor = color;
    _pickerControl.pickerValidTextColor = color;
}

- (void)setPickerInvalidTextColor:(UIColor *)color
{
    pickerInvalidTextColor = color;
    _pickerControl.pickerInvalidTextColor = color;
}

- (void)setParamID:(rcp_param_t)param
{
    paramID = param;
    _pickerControl.paramID = param;
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
    
    [_slider setAlpha:alpha];
    [_slider setEnabled:enabled];
    [_pickerControl setEnabled:enabled];
}

- (void)close
{
    [_pickerControl close];
}

// The reason this method doesn't actually show the picker view is because it first sends out a GET LIST to get the most recent data, waits for that list to come back, sets the data, and then displays the picker
- (void)requestPicker
{
    [_pickerControl requestPicker];
}

- (void)requestTextInputView
{
    [_pickerControl requestTextInputView];
}

- (void)sendRCPValue
{
    if ([self isOpen])
    {
        [_pickerControl sendRCPValue];
    }
    else
    {
        RCPHandler *handler = [RCPHandler instance];
        const int index = (int)_slider.value;
        RCPListItem *item = [_pickerControl.rcpList itemAtIndex:index];
        
        int32_t intVal = item.num;
        uint32_t uintVal = item.num;
        NSString *strVal = item.str;
        
        switch (_pickerControl.rcpList.sendValueType)
        {
            case SEND_VALUE_TYPE_INT:
                [handler rcpSetInt:paramID value:intVal];
                break;
                
            case SEND_VALUE_TYPE_UINT:
                [handler rcpSetUInt:paramID value:uintVal];
                break;
                
            case SEND_VALUE_TYPE_STR:
                [handler rcpSetString:paramID value:strVal];
                break;
                
            default:
                break;
        }
        
        // This GET is followed by a SET in case an out-of-range value is sent multiple times in a row.
        // For example, if the user entered 1000 for color temperature (which has a minimum of 1700), then the CURRENT back will say 1700 and things will be fine (assuming it was not already at 1700).
        // However, if you sent out 1000 again, no CURRENT would be sent back because the value did not change (it was at 1700, and that "1000" was recognized as beyond the minimum boundary, so it remained at 1700).
        [handler rcpGet:paramID];
    }
}

- (void)sliderDown
{
    if (_sliderTimer != nil)
    {
        [_sliderTimer invalidate];
        _sliderTimer = nil;
    }
    
    _ignoreCameraNumUpdates = YES;
    
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(sliderControlSliderSelected:)])
        {
            [delegate sliderControlSliderSelected:self];
        }
    }
}

- (void)sliderUp
{
    _sliderTimer = [NSTimer scheduledTimerWithTimeInterval:SLIDER_UPDATE_DELAY_S target:self selector:@selector(acceptCameraNumUpdates) userInfo:nil repeats:NO];
}

- (void)sliderMoved
{
    _ignoreCameraNumUpdates = YES;
    [self sendRCPValue];
}

- (void)acceptCameraNumUpdates
{
    [_sliderTimer invalidate];
    _sliderTimer = nil;
    
    _ignoreCameraNumUpdates = NO;
    
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpGet:paramID];
}

@end
