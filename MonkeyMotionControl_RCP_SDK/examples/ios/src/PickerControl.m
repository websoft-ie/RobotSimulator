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

#import "PickerControl.h"
#import "TabViewController.h"
#import "RCPHandler.h"
#import "Logger.h"

@interface PickerControl()

@property (retain, nonatomic) UILabel *titleLabel;
@property (retain, nonatomic) PickerTextField *textField;
@property (retain, nonatomic) UIPickerView *pickerView;
@property (retain, nonatomic) UIActivityIndicatorView *activityIndicator;

@property (retain, nonatomic) RCPList *rcpList;
@property (retain, nonatomic) IntEditInfo *intEditInfo;
@property (retain, nonatomic) UIntEditInfo *uintEditInfo;
@property (retain, nonatomic) StrEditInfo *strEditInfo;
@property (retain, nonatomic) NSTimer *activityTimer;
@property (assign, nonatomic) BOOL showPickerOnListArrival;

@property (assign, nonatomic) int currentIntValue;
@property (assign, nonatomic) uint32_t currentUIntValue;
@property (retain, nonatomic) NSString *currentStrValue;


- (void)showPicker;
- (void)triggerActivityIndicator;
- (void)showActivityIndicator;
- (void)hideActivityIndicator;

- (void)donePressed;
- (void)advancedPressed;

- (void)pickerTextFieldTapped:(PickerTextField *)pickerTextField;
- (void)pickerTextFieldLongPressed:(PickerTextField *)pickerTextField;

- (void)textInputView:(TextInputView *)textInputView sentIntValue:(const int)val;
- (void)textInputView:(TextInputView *)textInputView sentUIntValue:(const uint32_t)val;
- (void)textInputView:(TextInputView *)textInputView sentStrValue:(NSString *)val;

- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component;
- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView;
- (NSString*)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component;
- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component;
- (NSAttributedString *)pickerView:(UIPickerView *)pickerView attributedTitleForRow:(NSInteger)row forComponent:(NSInteger)component;

@end


@implementation PickerControl

@synthesize delegate, paramID, ignoreCameraListUpdates, textFieldBackgroundColor, textFieldTextColor, textFieldCornerRadius, textFieldAlignment, textFieldFontSize, pickerBackgroundColor, pickerValidTextColor, pickerInvalidTextColor;

- (PickerControl *)initWithParam:(rcp_param_t)param title:(BOOL)hasTitle
{
    if (self = [super init])
    {
        delegate = nil;
        paramID = param;
        ignoreCameraListUpdates = NO;
        
        _titleLabel = hasTitle ? [[UILabel alloc] init] : nil;
        _textField = [[PickerTextField alloc] init];
        _pickerView = [[UIPickerView alloc] init];
        _activityIndicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhite];
        
        _rcpList = [[RCPList alloc] init];
        _intEditInfo = [[IntEditInfo alloc] init];
        _uintEditInfo = [[UIntEditInfo alloc] init];
        _strEditInfo = [[StrEditInfo alloc] init];
        _activityTimer = nil;
        _showPickerOnListArrival = NO;
        
        _currentIntValue = 0;
        _currentUIntValue = 0;
        _currentStrValue = [[NSString alloc] init];

        [_textField setDelegate:self];
        [_pickerView setDelegate:self];
        [_pickerView setDataSource:self];
        
        [_textField setInputView:_pickerView];
        
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
        
        [_textField addSubview:_activityIndicator];
        [self addSubview:_textField];
    }
    
    return self;
}

- (RCPList *)rcpList
{
    return _rcpList;
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
    if (_rcpList.sendValueType == SEND_VALUE_TYPE_INT && _intEditInfo.isValid)
    {
        return [NSString stringWithFormat:@"%.*f", _intEditInfo.digits, _currentIntValue / _intEditInfo.divider];
    }
    else if (_rcpList.sendValueType == SEND_VALUE_TYPE_UINT && _uintEditInfo.isValid)
    {
        return [NSString stringWithFormat:@"%.*f", _uintEditInfo.digits, _currentUIntValue / _uintEditInfo.divider];
    }
    else if (_rcpList.sendValueType == SEND_VALUE_TYPE_STR && _strEditInfo.isValid)
    {
        NSString *str = _currentStrValue;
        NSRange prefixRange = [str rangeOfString:_strEditInfo.prefixDecoded];
        
        if (prefixRange.location != NSNotFound)
        {
            str = [str stringByReplacingCharactersInRange:prefixRange withString:@""];
        }
        
        NSRange suffixRange = [str rangeOfString:_strEditInfo.suffixDecoded options:NSBackwardsSearch];
        
        if (suffixRange.location != NSNotFound)
        {
            str = [str stringByReplacingCharactersInRange:suffixRange withString:@""];
        }
        
        return str;
    }
    
    return [NSString stringWithFormat:@"%d", _currentIntValue];
}

- (BOOL)isEditable
{
    switch (_rcpList.sendValueType)
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

- (BOOL)isOpen
{
    return [_textField isFirstResponder];
}

- (int)selectedRow
{
    return (int)[_pickerView selectedRowInComponent:0];
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
    [_textField setText:str];
}

- (void)updateStr:(NSString *)str editInfo:(rcp_cur_str_edit_info_t)info infoIsValid:(BOOL)infoIsValid
{
    _currentStrValue = str;
    
    _strEditInfo.isValid = infoIsValid;
    
    if (_strEditInfo.isValid)
    {
        _strEditInfo.prefixEncoded = (info.prefix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix];
        _strEditInfo.prefixDecoded = (info.prefix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.prefix_decoded];
        _strEditInfo.suffixEncoded = (info.suffix == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix];
        _strEditInfo.suffixDecoded = (info.suffix_decoded == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.suffix_decoded];
        _strEditInfo.allowedChars = (info.allowed_characters == NULL) ? @"" : [NSString stringWithFormat:@"%s", info.allowed_characters];
        _strEditInfo.minLength = info.min_len;
        _strEditInfo.maxLength = info.max_len;
        _strEditInfo.isPassword = info.is_password;
    }
    
    if (_activityTimer != nil)
    {
        [_activityTimer invalidate];
        _activityTimer = nil;
    }
    
    [self hideActivityIndicator];
    [_textField setText:str];
}

- (void)updateList:(RCPList *)list
{
    _rcpList = list;
    
    if (_showPickerOnListArrival)
    {
        [self showPicker];
    }
}

- (void)setFrame:(CGRect)frame
{
    [super setFrame:frame];
    
    const CGFloat verticalSpacing = 3.0;
    const CGSize activityIndicatorSize = CGSizeMake(20.0, 20.0);
    CGRect titleContainer = CGRectZero;
    
    if (_titleLabel != nil)
    {
        titleContainer = CGRectMake(0.0, 0.0, self.bounds.size.width, (self.bounds.size.height / 3.25) + verticalSpacing);
        
        [_titleLabel setFrame:CGRectMake(titleContainer.origin.x, titleContainer.origin.y, titleContainer.size.width, titleContainer.size.height - verticalSpacing)];
        [_titleLabel setFont:[UIFont systemFontOfSize:[RCPUtil maxSystemFontSizeForContainer:_titleLabel.frame bold:NO]]];
    }
    
    const CGFloat remainingHeight = self.bounds.size.height - (titleContainer.origin.y + titleContainer.size.height);
    
    [_textField setFrame:CGRectMake(0.0, titleContainer.origin.y + titleContainer.size.height, self.bounds.size.width, remainingHeight)];
    
    const CGFloat activityIndicatorY = (_textField.bounds.size.height - activityIndicatorSize.height) / 2.0;
    
    if (_textField.textAlignment == NSTextAlignmentLeft)
    {
        [_activityIndicator setFrame:CGRectMake(_textField.leftView.frame.origin.x + _textField.leftView.bounds.size.width, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else if (_textField.textAlignment == NSTextAlignmentRight)
    {
        [_activityIndicator setFrame:CGRectMake(_textField.bounds.size.width - activityIndicatorSize.width - 5.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else // defaults to center alignment
    {
        [_activityIndicator setFrame:CGRectMake((_textField.bounds.size.width - activityIndicatorSize.width) / 2.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
}

- (void)setTextFieldBackgroundColor:(UIColor *)color
{
    textFieldBackgroundColor = color;
    [_textField setBackgroundColor:color];
}

- (void)setTextFieldTextColor:(UIColor *)color
{
    textFieldTextColor = color;
    [_textField setTextColor:color];
}

- (void)setTextFieldCornerRadius:(CGFloat)radius
{
    textFieldCornerRadius = radius;
    [_textField setCornerRadius:radius];
}

- (void)setTextFieldAlignment:(NSTextAlignment)alignment
{
    textFieldAlignment = alignment;
    [_textField setTextAlignment:alignment];
    
    if (alignment == NSTextAlignmentLeft)
    {
        if (_textField.leftView == nil)
        {
            UIView *leftView = [[UIView alloc] initWithFrame:CGRectMake(0.0, 0.0, 5.0, 0.0)];
            [_textField setLeftView:leftView];
            [_textField setLeftViewMode:UITextFieldViewModeAlways];
        }
    }
    else
    {
        _textField.leftView = nil;
        [_textField setLeftViewMode:UITextFieldViewModeNever];
    }
    
    const float activityIndicatorY = _activityIndicator.frame.origin.y;
    const CGSize activityIndicatorSize = _activityIndicator.bounds.size;
    
    if (_textField.textAlignment == NSTextAlignmentLeft)
    {
        [_activityIndicator setFrame:CGRectMake(_textField.leftView.frame.origin.x + _textField.leftView.bounds.size.width, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else if (_textField.textAlignment == NSTextAlignmentRight)
    {
        [_activityIndicator setFrame:CGRectMake(_textField.bounds.size.width - activityIndicatorSize.width - 5.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
    else // defaults to center alignment
    {
        [_activityIndicator setFrame:CGRectMake((_textField.bounds.size.width - activityIndicatorSize.width) / 2.0, activityIndicatorY, activityIndicatorSize.width, activityIndicatorSize.height)];
    }
}

- (void)setTextFieldFontSize:(CGFloat)size
{
    textFieldFontSize = size;
    [_textField setFont:[UIFont systemFontOfSize:size]];
}

- (void)setPickerBackgroundColor:(UIColor *)color
{
    pickerBackgroundColor = color;
    [_pickerView setBackgroundColor:color];
}

- (void)setPickerValidTextColor:(UIColor *)color
{
    pickerValidTextColor = color;
    [_pickerView reloadAllComponents];
}

- (void)setPickerInvalidTextColor:(UIColor *)color
{
    pickerInvalidTextColor = color;
    [_pickerView reloadAllComponents];
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
    // Ensures that the picker is closed, the value the user may have been entering at the time is not sent out, and that the value displayed is in sync with the camera's value
    if (!enabled && [self isOpen])
    {
        RCPHandler *handler = [RCPHandler instance];
        TabViewController *tabVC = [TabViewController instance];
        
        [tabVC enableIOSNotifications:NO];
        [_textField resignFirstResponder];
        [tabVC enableIOSNotifications:YES];
        
        [handler rcpGet:paramID];
    }
    
    
    const float alpha = enabled ? 1.0 : 0.5;
    
    if (_titleLabel != nil)
    {
        [_titleLabel setAlpha:alpha];
        [_titleLabel setEnabled:enabled];
    }
    
    [_textField setAlpha:alpha];
    [_textField setEnabled:enabled];
}

- (void)close
{
    [_textField resignFirstResponder];
}

// The reason this method doesn't actually show the picker view is because it first sends out a GET LIST to get the most recent data, waits for that list to come back, sets the data, and then displays the picker
- (void)requestPicker
{
    _showPickerOnListArrival = YES;
    
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpGetList:paramID];
}

- (void)requestTextInputView
{
    if (self.isEditable)
    {
        if (delegate != nil)
        {
            if ([delegate respondsToSelector:@selector(pickerControlSelected:)])
            {
                [delegate pickerControlSelected:self];
            }
            
            [delegate pickerControlRequestedTextInputView:self];
        }
    }
}

- (void)sendRCPValue
{
    RCPHandler *handler = [RCPHandler instance];
    RCPListItem *item = [_rcpList itemAtIndex:[self selectedRow]];
    
    int32_t intVal = item.num;
    uint32_t uintVal = item.num;
    NSString *strVal = item.str;
    
    switch (_rcpList.sendValueType)
    {
        case SEND_VALUE_TYPE_INT:
            [handler rcpSetInt:paramID value:intVal];
            
            if (intVal != _currentIntValue)
            {
                [self triggerActivityIndicator];
            }
            else
            {
                [handler rcpGet:paramID];
            }
            break;
            
        case SEND_VALUE_TYPE_UINT:
            [handler rcpSetUInt:paramID value:uintVal];
            
            if (intVal != _currentIntValue)
            {
                [self triggerActivityIndicator];
            }
            else
            {
                [handler rcpGet:paramID];
            }
            break;
            
        case SEND_VALUE_TYPE_STR:
            [handler rcpSetString:paramID value:strVal];
            
            if ([strVal compare:_currentStrValue] != NSOrderedSame)
            {
                [self triggerActivityIndicator];
            }
            else
            {
                [handler rcpGet:paramID];
            }
            break;
            
        default:
            break;
    }
}

- (void)showPicker
{
    if ([_rcpList count] > 0)
    {
        UIBarButtonItem *flexibleSpace = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
        UIBarButtonItem *doneButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemDone target:self action:@selector(donePressed)];
        
        UIToolbar *accessoryView = [[UIToolbar alloc] initWithFrame:CGRectMake(0.0, 0.0, [UIScreen mainScreen].bounds.size.width, 44.0)];
        accessoryView.barStyle = UIBarStyleBlackOpaque;
        accessoryView.tintColor = [UIColor colorWithRed:240.0/255.0 green:240.0/255.0 blue:240.0/255.0 alpha:1.0];
        
        if ([self isEditable])
        {
            UIBarButtonItem *advancedButton = [[UIBarButtonItem alloc] initWithTitle:@"Advanced..." style:UIBarButtonItemStylePlain target:self action:@selector(advancedPressed)];
            [accessoryView setItems:[NSArray arrayWithObjects:advancedButton, flexibleSpace, doneButton, nil]];
        }
        else
        {
            [accessoryView setItems:[NSArray arrayWithObjects:flexibleSpace, doneButton, nil]];
        }
        
        [_textField setInputAccessoryView:accessoryView];
        
        [_pickerView reloadAllComponents];
        [_pickerView selectRow:_rcpList.currentRow inComponent:0 animated:NO];
        
        [_textField becomeFirstResponder];
        
        _showPickerOnListArrival = NO;
    }
}

- (void)triggerActivityIndicator
{
    _activityTimer = [NSTimer scheduledTimerWithTimeInterval:ACTIVITY_INDICATOR_DELAY_S target:self selector:@selector(showActivityIndicator) userInfo:nil repeats:NO];
}

- (void)showActivityIndicator
{
    [_textField setText:@""];
    [_activityIndicator startAnimating];
    [_activityIndicator setHidden:NO];
}

- (void)hideActivityIndicator
{
    [_activityIndicator setHidden:YES];
    [_activityIndicator stopAnimating];
}

- (void)donePressed
{
    [_textField resignFirstResponder];
}

- (void)advancedPressed
{    
    if (delegate != nil)
    {
        [delegate pickerControlRequestedTextInputView:self];
    }
}

- (void)pickerTextFieldTapped:(PickerTextField *)pickerTextField
{
    [self requestPicker];
    
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(pickerControlSelected:)])
        {
            [delegate pickerControlSelected:self];
        }
    }
}

- (void)pickerTextFieldLongPressed:(PickerTextField *)pickerTextField
{
    [self requestTextInputView];
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

- (NSInteger)pickerView:(UIPickerView *)pickerView numberOfRowsInComponent:(NSInteger)component
{
    return [_rcpList count];
}

- (NSInteger)numberOfComponentsInPickerView:(UIPickerView *)pickerView
{
    return 1;
}

- (NSString*)pickerView:(UIPickerView *)pickerView titleForRow:(NSInteger)row forComponent:(NSInteger)component
{
    return [_rcpList itemAtIndex:row].str;
}

- (void)pickerView:(UIPickerView *)pickerView didSelectRow:(NSInteger)row inComponent:(NSInteger)component
{
    if (!_rcpList.updateOnlyOnClose)
    {
        [self sendRCPValue];
    }
}

- (NSAttributedString *)pickerView:(UIPickerView *)pickerView attributedTitleForRow:(NSInteger)row forComponent:(NSInteger)component
{
    RCPListItem *item = [_rcpList itemAtIndex:row];
    NSAttributedString *str;
    
    if ([_rcpList valueIsValid:item.num])
    {
        str = [[NSAttributedString alloc] initWithString:item.str attributes:@{NSForegroundColorAttributeName:pickerValidTextColor}];
    }
    else
    {
        str = [[NSAttributedString alloc] initWithString:item.str attributes:@{NSFontAttributeName:[UIFont boldSystemFontOfSize:[UIFont systemFontSize]], NSForegroundColorAttributeName:pickerInvalidTextColor}];
    }
    
    return str;
}

@end
