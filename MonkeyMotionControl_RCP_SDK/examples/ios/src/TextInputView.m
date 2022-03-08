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

#import "TextInputView.h"
#import "TextControl.h"
#import "TabViewController.h"
#import "RCPHandler.h"
#import "Logger.h"

@interface TextInputView()

@property (retain, nonatomic) UIView *container;
@property (retain, nonatomic) UILabel *titleLabel;
@property (retain, nonatomic) UITextField *textField;
@property (retain, nonatomic) UIView *textFieldView;
@property (retain, nonatomic) TimecodeView *timecodeView;
@property (retain, nonatomic) IPAddressView *ipAddressView;
@property (retain, nonatomic) UIButton *cancelButton;
@property (retain, nonatomic) UIButton *doneButton;
@property (retain, nonatomic) UIView *buttonSeparator;


- (BOOL)textField:(UITextField *)txt shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string;
- (BOOL)textFieldShouldReturn:(UITextField *)txt;
- (void)textFieldValueChanged;

- (void)timecodeViewDoneEditing:(TimecodeView *)timecodeView;
- (void)ipAddressViewDoneEditing:(IPAddressView *)ipAddressView;

- (void)cancelPressed;
- (void)donePressed;

- (void)sendRCPValue;
- (void)drawView;

@end


@implementation TextInputView

@synthesize delegate, paramID, textInputViewType, sendValueType, intEditInfo, uintEditInfo, strEditInfo, textColor;

- (TextInputView *)init
{
    if (self = [super init])
    {
        delegate = nil;
        paramID = RCP_PARAM_COUNT;
        [self setTextInputViewType:TEXT_INPUT_VIEW_TYPE_TEXTFIELD];
        [self setSendValueType:SEND_VALUE_TYPE_INT];
        intEditInfo = [[IntEditInfo alloc] init];
        uintEditInfo = [[UIntEditInfo alloc] init];
        strEditInfo = [[StrEditInfo alloc] init];
        [self setTextColor:[UIColor whiteColor]];
        
        _container = [[UIView alloc] init];
        _titleLabel = [[UILabel alloc] init];
        _textField = [[UITextField alloc] init];
        _textFieldView = [[UIView alloc] init];
        _timecodeView = [[TimecodeView alloc] init];
        _ipAddressView = [[IPAddressView alloc] init];
        _cancelButton = [[UIButton alloc] init];
        _doneButton = [[UIButton alloc] init];
        _buttonSeparator = [[UIView alloc] init];
        
        
        [self setBackgroundColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.4]];
        
        [_container setBackgroundColor:[UIColor colorWithRed:0.15 green:0.15 blue:0.15 alpha:1.0]];
        [_container.layer setBorderColor:[[UIColor blackColor] CGColor]];
        [_container.layer setBorderWidth:1.0];
        
        [_titleLabel setTextColor:[UIColor whiteColor]];
        [_titleLabel setFont:[UIFont boldSystemFontOfSize:20.0]];
        [_titleLabel setTextAlignment:NSTextAlignmentCenter];
        
        [_textField setBackgroundColor:[UIColor blackColor]];
        [_textField setTintColor:[UIColor whiteColor]];
        [_textField setTextAlignment:NSTextAlignmentCenter];
        [_textField setReturnKeyType:UIReturnKeySend];
        [_textField setKeyboardAppearance:UIKeyboardAppearanceDark];
        [_textField setLeftView:[[UIView alloc] initWithFrame:CGRectMake(0.0, 0.0, 5.0, 0.0)]];
        [_textField setLeftViewMode:UITextFieldViewModeAlways];
        [_textField.layer setCornerRadius:5.0];
        [_textField setDelegate:self];
        [_textField addTarget:self action:@selector(textFieldValueChanged) forControlEvents:UIControlEventEditingChanged];
        
        [_timecodeView setColor:[UIColor blackColor]];
        [_timecodeView setTextColor:[UIColor whiteColor]];
        [_timecodeView setDelegate:self];
        
        [_ipAddressView setColor:[UIColor blackColor]];
        [_ipAddressView setTextColor:[UIColor whiteColor]];
        [_ipAddressView setDelegate:self];
        
        [_cancelButton setBackgroundColor:[UIColor colorWithRed:0.3 green:0.3 blue:0.3 alpha:1.0]];
        [_cancelButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
        [_cancelButton setTitleColor:[UIColor lightGrayColor] forState:UIControlStateHighlighted];
        [_cancelButton.titleLabel setFont:[UIFont systemFontOfSize:18.0]];
        [_cancelButton setTitle:@"Cancel" forState:UIControlStateNormal];
        [_cancelButton addTarget:self action:@selector(cancelPressed) forControlEvents:UIControlEventTouchUpInside];
        
        [_doneButton setBackgroundColor:[UIColor colorWithRed:0.3 green:0.3 blue:0.3 alpha:1.0]];
        [_doneButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
        [_doneButton setTitleColor:[UIColor lightGrayColor] forState:UIControlStateHighlighted];
        [_doneButton.titleLabel setFont:[UIFont boldSystemFontOfSize:18.0]];
        [_doneButton setTitle:@"Done" forState:UIControlStateNormal];
        [_doneButton addTarget:self action:@selector(donePressed) forControlEvents:UIControlEventTouchUpInside];
        
        [_buttonSeparator setBackgroundColor:[UIColor blackColor]];
        
        
        [_textField addSubview:_textFieldView];
        [_container addSubview:_titleLabel];
        [_container addSubview:_textField];
        [_container addSubview:_timecodeView];
        [_container addSubview:_ipAddressView];
        [_container addSubview:_cancelButton];
        [_container addSubview:_doneButton];
        [_container addSubview:_buttonSeparator];
        [self addSubview:_container];
    }
    
    return self;
}

- (void)showInView:(UIView *)view
{
    [self setFrame:view.bounds];
    
    _container.alpha = 0.0;
    _container.transform = CGAffineTransformMakeScale(1.2, 1.2);
    [view addSubview:self];
    
    TabViewController *tabVC = [TabViewController instance];

    [tabVC enableIOSNotifications:NO];
    
    switch (textInputViewType)
    {
        case TEXT_INPUT_VIEW_TYPE_TEXTFIELD:
            [self drawView];
            [_textField becomeFirstResponder];
            [_textField setSelectedTextRange:[RCPUtil selectedRangeForTextField:_textField prefix:@"" suffix:@""]];
            break;
            
        case TEXT_INPUT_VIEW_TYPE_TIMECODE:
            [_timecodeView open];
            [_timecodeView selectText:YES];
            break;
            
        case TEXT_INPUT_VIEW_TYPE_IP_ADDRESS:
            [_ipAddressView open];
            [_ipAddressView selectText:YES];
            break;
            
        default:
            break;
    }
    
    [tabVC enableIOSNotifications:YES];
    
    [UIView animateWithDuration:0.3 delay:0 options:UIViewAnimationOptionCurveEaseOut animations:^{
        _container.alpha = 1.0;
        _container.transform = CGAffineTransformIdentity;
    } completion:^(BOOL finished){
        
    }];
}

- (void)hide
{
    if (self.superview != nil)
    {
        TabViewController *tabVC = [TabViewController instance];
        
        [tabVC enableIOSNotifications:NO];
        
        switch (textInputViewType)
        {
            case TEXT_INPUT_VIEW_TYPE_TEXTFIELD:
                [_textField resignFirstResponder];
                break;
                
            case TEXT_INPUT_VIEW_TYPE_TIMECODE:
                [_timecodeView close];
                break;
                
            case TEXT_INPUT_VIEW_TYPE_IP_ADDRESS:
                [_ipAddressView close];
                break;
                
            default:
                break;
        }
        
        [tabVC enableIOSNotifications:YES];
        
        const CGFloat prevAlpha = self.alpha;
        
        [UIView animateWithDuration:0.3 delay:0 options:UIViewAnimationOptionCurveEaseOut animations:^{
            _container.alpha = 0.0;
            _container.transform = CGAffineTransformMakeScale(0.9, 0.9);
            self.alpha = 0.0;
        } completion:^(BOOL finished){
            [self removeFromSuperview];
            
            self.alpha = prevAlpha;
            _container.alpha = 1.0;
            _container.transform = CGAffineTransformIdentity;
        }];
    }
}

- (void)setFrame:(CGRect)frame
{
    [super setFrame:frame];
    
    
    const CGFloat verticalSpacing = 10.0;
    
    const CGFloat containerWidth = 300.0;
    
    const CGFloat titleWidth = containerWidth;
    const CGFloat titleHeight = [RCPUtil sizeOfText:@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890" font:[_titleLabel font]].height;
    const CGFloat titleY = verticalSpacing;
    
    const CGFloat inputViewWidth = containerWidth - 30.0;
    const CGRect inputViewFrame = CGRectMake((containerWidth - inputViewWidth) / 2.0, titleY + titleHeight + (verticalSpacing * 2.0), inputViewWidth, 30.0);
    
    const CGFloat buttonWidth = containerWidth / 2.0;
    const CGFloat buttonHeight = [RCPUtil sizeOfText:[_cancelButton.titleLabel text] font:[_cancelButton.titleLabel font]].height + 20.0;
    
    const CGFloat buttonSeparatorWidth = 1.0;
    const CGFloat buttonSeparatorHeight = buttonHeight;
    const CGFloat buttonY = inputViewFrame.origin.y + inputViewFrame.size.height + (verticalSpacing * 2.0);
    
    const CGFloat containerHeight = buttonY + buttonHeight;
    
    
    [_container setFrame:CGRectMake((self.bounds.size.width - containerWidth) / 2.0, (self.bounds.size.height / 3.0) - (containerHeight / 2.0), containerWidth, containerHeight)];
    [_titleLabel setFrame:CGRectMake(0.0, titleY, titleWidth, titleHeight)];
    [_textField setFrame:inputViewFrame];
    [_textFieldView setFrame:_textField.bounds];
    [_timecodeView setFrame:inputViewFrame];
    [_ipAddressView setFrame:inputViewFrame];
    [_cancelButton setFrame:CGRectMake(0.0, buttonY, buttonWidth, buttonHeight)];
    [_doneButton setFrame:CGRectMake(_cancelButton.frame.origin.x + buttonWidth, buttonY, buttonWidth, buttonHeight)];
    [_buttonSeparator setFrame:CGRectMake(_doneButton.frame.origin.x - (buttonSeparatorWidth / 2.0), buttonY, buttonSeparatorWidth, buttonSeparatorHeight)];
}

- (void)setParamID:(rcp_param_t)param
{
    paramID = param;
    
    _timecodeView.paramID = param;
    _ipAddressView.paramID = param;
    
    RCPHandler *handler = [RCPHandler instance];
    [_titleLabel setText:[handler rcpGetLabel:paramID]];
}

- (void)setTextInputViewType:(text_input_view_type_t)type
{
    textInputViewType = type;
    
    switch (textInputViewType)
    {
        case TEXT_INPUT_VIEW_TYPE_TEXTFIELD:
            [_textField setHidden:NO];
            [_timecodeView setHidden:YES];
            [_ipAddressView setHidden:YES];
            break;
            
        case TEXT_INPUT_VIEW_TYPE_TIMECODE:
            [_textField setHidden:YES];
            [_timecodeView setHidden:NO];
            [_ipAddressView setHidden:YES];
            break;
            
        case TEXT_INPUT_VIEW_TYPE_IP_ADDRESS:
            [_textField setHidden:YES];
            [_timecodeView setHidden:YES];
            [_ipAddressView setHidden:NO];
            break;
            
        default:
            break;
    }
}

- (void)setSendValueType:(send_value_type_t)type
{
    sendValueType = type;
    
    if (sendValueType == SEND_VALUE_TYPE_INT || sendValueType == SEND_VALUE_TYPE_UINT)
    {
        [_textField setKeyboardType:UIKeyboardTypeNumbersAndPunctuation];
    }
    else if (sendValueType == SEND_VALUE_TYPE_STR)
    {
        [_textField setKeyboardType:UIKeyboardTypeAlphabet];
    }
}

- (void)setTextColor:(UIColor *)color
{
    textColor = color;
    [_textField setTextColor:color];
    [_timecodeView setTextColor:color];
    [_ipAddressView setTextColor:color];
}

- (void)setText:(NSString *)text
{
    [_textField setText:text];
    [self drawView];
}

- (void)setTimecode:(const int)timecode
{
    [_timecodeView setTimecode:timecode];
}

- (void)setIPAddress:(const uint32_t)ipAddress
{
    [_ipAddressView setIPAddress:ipAddress];
}

- (BOOL)textField:(UITextField *)txt shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string
{
    NSMutableString *charSetString = [[NSMutableString alloc] init];
    size_t maxLength = 9999;
    
    if (sendValueType == SEND_VALUE_TYPE_INT && intEditInfo.isValid)
    {
        [charSetString appendString:@"0123456789"];
        
        if (intEditInfo.minValue < 0)
        {
            [charSetString appendString:@"-"];
        }
        
        if (intEditInfo.divider > 1.0)
        {
            [charSetString appendString:@"."];
        }
    }
    else if (sendValueType == SEND_VALUE_TYPE_UINT && uintEditInfo.isValid)
    {
        [charSetString appendString:@"0123456789"];
        
        if (uintEditInfo.divider > 1.0)
        {
            [charSetString appendString:@"."];
        }
    }
    else if (sendValueType == SEND_VALUE_TYPE_STR && strEditInfo.isValid)
    {
        [charSetString appendString:strEditInfo.allowedChars];
        maxLength = strEditInfo.maxLength;
    }
    else
    {
        return NO;
    }
    
    if ([[txt.text stringByReplacingCharactersInRange:range withString:string] length] <= maxLength)
    {
        if ([charSetString length] > 0)
        {
            NSUInteger i;
            NSUInteger length = [string length];
            NSCharacterSet *characterSet = [NSCharacterSet characterSetWithCharactersInString:charSetString];
            
            for (i = 0; i < length; i++)
            {
                NSString *character = [NSString stringWithFormat:@"%c", [string characterAtIndex:i]];
                NSRange r = [character rangeOfCharacterFromSet:characterSet];
                
                // If the character at index i of the text the user is typing/pasting is not in the allowedCharacters string, do not put it in the text field
                if (r.location == NSNotFound)
                {
                    return NO;
                }
            }
        }
    }
    else
    {
        return NO;
    }
    
    return YES;
}

- (BOOL)textFieldShouldReturn:(UITextField *)txt
{
    [self hide];
    [self sendRCPValue];
    return YES;
}

- (void)textFieldValueChanged
{
    [self drawView];
}

- (void)timecodeViewDoneEditing:(TimecodeView *)timecodeView
{
    [self hide];
    [self sendRCPValue];
}

- (void)ipAddressViewDoneEditing:(IPAddressView *)ipAddressView
{
    [self hide];
    [self sendRCPValue];
}

- (void)cancelPressed
{
    RCPHandler *handler = [RCPHandler instance];
    
    [self hide];
    [handler rcpGet:paramID];
}

- (void)donePressed
{
    [self hide];
    [self sendRCPValue];
}

- (void)sendRCPValue
{
    RCPHandler *handler = [RCPHandler instance];
    
    switch (textInputViewType)
    {
        case TEXT_INPUT_VIEW_TYPE_TEXTFIELD:
            switch (sendValueType)
            {
                case SEND_VALUE_TYPE_INT:
                {
                    int val = roundf([[_textField text] floatValue] * intEditInfo.divider);
                    val = intEditInfo.minValue + roundf((val - intEditInfo.minValue) / (float)intEditInfo.step) * intEditInfo.step;
                    
                    [handler rcpSetInt:paramID value:val];
                    
                    if (delegate != nil)
                    {
                        if ([delegate respondsToSelector:@selector(textInputView:sentIntValue:)])
                        {
                            [delegate textInputView:self sentIntValue:val];
                        }
                        else
                        {
                            [handler rcpGet:paramID];
                        }
                    }
                    else
                    {
                        [handler rcpGet:paramID];
                    }
                    break;
                }
                    
                case SEND_VALUE_TYPE_UINT:
                {
                    uint32_t val = roundf([[_textField text] floatValue] * uintEditInfo.divider);
                    val = uintEditInfo.minValue + roundf((val - uintEditInfo.minValue) / (float)uintEditInfo.step) * uintEditInfo.step;
                    
                    [handler rcpSetUInt:paramID value:val];
                    
                    if (delegate != nil)
                    {
                        if ([delegate respondsToSelector:@selector(textInputView:sentUIntValue:)])
                        {
                            [delegate textInputView:self sentUIntValue:val];
                        }
                        else
                        {
                            [handler rcpGet:paramID];
                        }
                    }
                    else
                    {
                        [handler rcpGet:paramID];
                    }
                    break;
                }
                    
                case SEND_VALUE_TYPE_STR:
                {
                    NSString *val = [_textField text];
                    
                    [handler rcpSetString:paramID value:val];
                    
                    if (delegate != nil)
                    {
                        if ([delegate respondsToSelector:@selector(textInputView:sentStrValue:)])
                        {
                            [delegate textInputView:self sentStrValue:val];
                        }
                        else
                        {
                            [handler rcpGet:paramID];
                        }
                    }
                    else
                    {
                        [handler rcpGet:paramID];
                    }
                    break;
                }
                    
                default:
                    break;
            }
            break;
            
        case TEXT_INPUT_VIEW_TYPE_TIMECODE:
            [_timecodeView sendRCPValue];
            [handler rcpGet:paramID];
            break;
            
        case TEXT_INPUT_VIEW_TYPE_IP_ADDRESS:
            [_ipAddressView sendRCPValue];
            [handler rcpGet:paramID];
            break;
            
        default:
            break;
    }
}

- (void)drawView
{
    [_textField setSecureTextEntry:(sendValueType == SEND_VALUE_TYPE_STR && strEditInfo.isValid && strEditInfo.isPassword)];
    
    for (UIView *v in _textFieldView.subviews)
    {
        [v removeFromSuperview];
    }
    
    UIView *decoratedView;
    
    if (sendValueType == SEND_VALUE_TYPE_INT && intEditInfo.isValid)
    {
        decoratedView = [RCPUtil decoratedViewForTextField:_textField prefix:intEditInfo.prefixDecoded suffix:intEditInfo.suffixDecoded];
        [decoratedView setFrame:_textFieldView.bounds];
        [_textFieldView addSubview:decoratedView];
    }
    else if (sendValueType == SEND_VALUE_TYPE_UINT && uintEditInfo.isValid)
    {
        decoratedView = [RCPUtil decoratedViewForTextField:_textField prefix:uintEditInfo.prefixDecoded suffix:uintEditInfo.suffixDecoded];
        [decoratedView setFrame:_textFieldView.bounds];
        [_textFieldView addSubview:decoratedView];
    }
    else if (sendValueType == SEND_VALUE_TYPE_STR && strEditInfo.isValid)
    {
        decoratedView = [RCPUtil decoratedViewForTextField:_textField prefix:strEditInfo.prefixDecoded suffix:strEditInfo.suffixDecoded];
        [decoratedView setFrame:_textFieldView.bounds];
        [_textFieldView addSubview:decoratedView];
    }
}

@end
