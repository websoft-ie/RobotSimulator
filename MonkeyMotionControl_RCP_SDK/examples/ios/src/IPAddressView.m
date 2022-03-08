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

#import "IPAddressView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

@interface IPAddressView()

@property (retain, nonatomic) UITextField *textField1;
@property (retain, nonatomic) UITextField *textField2;
@property (retain, nonatomic) UITextField *textField3;
@property (retain, nonatomic) UITextField *textField4;

@property (retain, nonatomic) UILabel *label1;
@property (retain, nonatomic) UILabel *label2;
@property (retain, nonatomic) UILabel *label3;


- (BOOL)textFieldShouldReturn:(UITextField *)textField;
- (BOOL)textField:(UITextField *)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string;

@end


@implementation IPAddressView

@synthesize delegate, paramID;

- (IPAddressView *)init
{
    if (self = [super init])
    {
        delegate = nil;
        paramID = RCP_PARAM_COUNT;
        
        _textField1 = [[UITextField alloc] init];
        _textField2 = [[UITextField alloc] init];
        _textField3 = [[UITextField alloc] init];
        _textField4 = [[UITextField alloc] init];
        
        _label1 = [[UILabel alloc] init];
        _label2 = [[UILabel alloc] init];
        _label3 = [[UILabel alloc] init];
        
        
        [_textField1 setTextAlignment:NSTextAlignmentCenter];
        [_textField2 setTextAlignment:NSTextAlignmentCenter];
        [_textField3 setTextAlignment:NSTextAlignmentCenter];
        [_textField4 setTextAlignment:NSTextAlignmentCenter];
        
        [_textField1 setBorderStyle:UITextBorderStyleNone];
        [_textField2 setBorderStyle:UITextBorderStyleNone];
        [_textField3 setBorderStyle:UITextBorderStyleNone];
        [_textField4 setBorderStyle:UITextBorderStyleNone];
        
        [_textField1 setKeyboardAppearance:UIKeyboardAppearanceDark];
        [_textField2 setKeyboardAppearance:UIKeyboardAppearanceDark];
        [_textField3 setKeyboardAppearance:UIKeyboardAppearanceDark];
        [_textField4 setKeyboardAppearance:UIKeyboardAppearanceDark];
        
        [_textField1 setKeyboardType:UIKeyboardTypeNumbersAndPunctuation];
        [_textField2 setKeyboardType:UIKeyboardTypeNumbersAndPunctuation];
        [_textField3 setKeyboardType:UIKeyboardTypeNumbersAndPunctuation];
        [_textField4 setKeyboardType:UIKeyboardTypeNumbersAndPunctuation];
        
        [_textField1 setReturnKeyType:UIReturnKeyNext];
        [_textField2 setReturnKeyType:UIReturnKeyNext];
        [_textField3 setReturnKeyType:UIReturnKeyNext];
        [_textField4 setReturnKeyType:UIReturnKeyDone];
        
        [_textField1 setTag:1];
        [_textField2 setTag:2];
        [_textField3 setTag:3];
        [_textField4 setTag:4];
        
        [_textField1 setDelegate:self];
        [_textField2 setDelegate:self];
        [_textField3 setDelegate:self];
        [_textField4 setDelegate:self];
        
        
        [_label1 setText:@"."];
        [_label2 setText:@"."];
        [_label3 setText:@"."];
        
        [_label1 setTextAlignment:NSTextAlignmentCenter];
        [_label2 setTextAlignment:NSTextAlignmentCenter];
        [_label3 setTextAlignment:NSTextAlignmentCenter];
        
        
        [self addSubview:_textField1];
        [self addSubview:_textField2];
        [self addSubview:_textField3];
        [self addSubview:_textField4];
        
        [self addSubview:_label1];
        [self addSubview:_label2];
        [self addSubview:_label3];
    }
    
    return self;
}

- (BOOL)isOpen
{
    return [_textField1 isFirstResponder] || [_textField2 isFirstResponder] || [_textField3 isFirstResponder] || [_textField4 isFirstResponder];
}

- (uint32_t)ipAddress
{
    const uint32_t first = ([[_textField1 text] intValue]) << 24;
    const uint32_t second = ([[_textField2 text] intValue]) << 16;
    const uint32_t third = ([[_textField3 text] intValue]) << 8;
    const uint32_t fourth = [[_textField4 text] intValue];
    
    const uint32_t ipAddress = first | second | third | fourth;
    
    return ipAddress;
}

- (void)setFrame:(CGRect)frame
{
    const CGFloat labelWidth = 7.0;
    const CGFloat labelHeight = frame.size.height;
    
    const CGFloat textFieldWidth = (frame.size.width - (labelWidth * 3.0)) / 4.0;
    const CGFloat textFieldHeight = frame.size.height;
    
    CGFloat x = 0.0;
    
    
    [super setFrame:frame];
    
    [_textField1 setFrame:CGRectMake(x, 0.0, textFieldWidth, textFieldHeight)];
    x += _textField1.frame.size.width;
    
    [_label1 setFrame:CGRectMake(x, 0.0, labelWidth, labelHeight)];
    x += _label1.frame.size.width;
    
    [_textField2 setFrame:CGRectMake(x, 0.0, textFieldWidth, textFieldHeight)];
    x += _textField2.frame.size.width;
    
    [_label2 setFrame:CGRectMake(x, 0.0, labelWidth, labelHeight)];
    x += _label2.frame.size.width;
    
    [_textField3 setFrame:CGRectMake(x, 0.0, textFieldWidth, textFieldHeight)];
    x += _textField3.frame.size.width;
    
    [_label3 setFrame:CGRectMake(x, 0.0, labelWidth, labelHeight)];
    x += _label3.frame.size.width;
    
    [_textField4 setFrame:CGRectMake(x, 0.0, textFieldWidth, textFieldHeight)];
}

- (void)setTextColor:(UIColor *)color
{
    [_textField1 setTextColor:color];
    [_textField2 setTextColor:color];
    [_textField3 setTextColor:color];
    [_textField4 setTextColor:color];
    
    [_textField1 setTintColor:color];
    [_textField2 setTintColor:color];
    [_textField3 setTintColor:color];
    [_textField4 setTintColor:color];
    
    [_label1 setTextColor:color];
    [_label2 setTextColor:color];
    [_label3 setTextColor:color];
}

- (void)setColor:(UIColor *)color
{
    [_textField1 setBackgroundColor:color];
    [_textField2 setBackgroundColor:color];
    [_textField3 setBackgroundColor:color];
    [_textField4 setBackgroundColor:color];
}

- (void)setEnabled:(BOOL)enabled
{
    const CGFloat alpha = enabled ? 1.0 : 0.5;
    
    [_textField1 setAlpha:alpha];
    [_textField2 setAlpha:alpha];
    [_textField3 setAlpha:alpha];
    [_textField4 setAlpha:alpha];
    
    [_textField1 setEnabled:enabled];
    [_textField2 setEnabled:enabled];
    [_textField3 setEnabled:enabled];
    [_textField4 setEnabled:enabled];
    
    [_label1 setEnabled:enabled];
    [_label2 setEnabled:enabled];
    [_label3 setEnabled:enabled];
    
    [self setUserInteractionEnabled:enabled];
}

- (void)setIPAddress:(const uint32_t)ipAddress
{
    const uint32_t first = (ipAddress >> 24) & 255;
    const uint32_t second = (ipAddress >> 16) & 255;
    const uint32_t third = (ipAddress >> 8) & 255;
    const uint32_t fourth = ipAddress & 255;
    
    [_textField1 setText:[NSString stringWithFormat:@"%d", first]];
    [_textField2 setText:[NSString stringWithFormat:@"%d", second]];
    [_textField3 setText:[NSString stringWithFormat:@"%d", third]];
    [_textField4 setText:[NSString stringWithFormat:@"%d", fourth]];
}

- (void)open
{
    [_textField1 becomeFirstResponder];
}

- (void)close
{
    if ([_textField1 isFirstResponder])
    {
        [_textField1 resignFirstResponder];
    }
    else if ([_textField2 isFirstResponder])
    {
        [_textField2 resignFirstResponder];
    }
    else if ([_textField3 isFirstResponder])
    {
        [_textField3 resignFirstResponder];
    }
    else if ([_textField4 isFirstResponder])
    {
        [_textField4 resignFirstResponder];
    }
}

- (void)selectText:(BOOL)select
{
    UITextField *textField = nil;
    
    if ([_textField1 isFirstResponder])
    {
        textField = _textField1;
    }
    else if ([_textField2 isFirstResponder])
    {
        textField = _textField2;
    }
    else if ([_textField3 isFirstResponder])
    {
        textField = _textField3;
    }
    else if ([_textField4 isFirstResponder])
    {
        textField = _textField4;
    }
    
    if (textField != nil)
    {
        if (select)
        {
            [textField setSelectedTextRange:[RCPUtil selectedRangeForTextField:textField prefix:@"" suffix:@""]];
        }
        else
        {
            [textField setSelectedTextRange:nil];
        }
    }
}

- (void)sendRCPValue
{
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpSetUInt:paramID value:[self ipAddress]];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField
{
    switch (textField.tag)
    {
        case 1:
            [_textField2 becomeFirstResponder];
            [self selectText:YES];
            return NO;
            
        case 2:
            [_textField3 becomeFirstResponder];
            [self selectText:YES];
            return NO;
            
        case 3:
            [_textField4 becomeFirstResponder];
            [self selectText:YES];
            return NO;
            
        case 4:
            [_textField4 resignFirstResponder];
            
            if (delegate != nil)
            {
                if ([delegate respondsToSelector:@selector(ipAddressViewDoneEditing:)])
                {
                    [delegate ipAddressViewDoneEditing:self];
                }
            }
            return YES;
            
        default:
            break;
    }
    
    return YES;
}

- (BOOL)textField:(UITextField *)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string
{
    NSUInteger i;
    NSUInteger length = [string length];
    NSCharacterSet *characterSet = [NSCharacterSet characterSetWithCharactersInString:@"0123456789"];
    
    for (i = 0; i < length; i++)
    {
        NSString *character = [NSString stringWithFormat:@"%c", [string characterAtIndex:i]];
        NSRange r = [character rangeOfCharacterFromSet:characterSet];
        
        if (r.location == NSNotFound)
        {
            return NO;
        }
    }
    
    
    NSMutableString *text = [NSMutableString stringWithString:[textField text]];

    [text replaceCharactersInRange:range withString:string];
    
    const int value = [text intValue];
    
    if (value < 0 || value > 255)
    {
        return NO;
    }
    
    return YES;
}

@end
