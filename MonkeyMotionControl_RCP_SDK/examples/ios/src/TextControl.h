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

#import <Foundation/Foundation.h>
#import "TextInputView.h"
#import "IntEditInfo.h"
#import "UIntEditInfo.h"
#import "StrEditInfo.h"
#import "RCPUtil.h"
#import "rcp_api.h"

@class TextControl;
@protocol TextControlDelegate<NSObject>
@optional
- (void)textControlSelected:(TextControl *)textControl;
@required
- (void)textControlRequestedTextInputView:(TextControl *)textControl;
@end

@interface TextControl : UIView<TextInputViewDelegate>

@property (weak, nonatomic) id<TextControlDelegate> delegate;
@property (assign, nonatomic) rcp_param_t paramID;
@property (assign, nonatomic) send_value_type_t sendValueType;

@property (retain, nonatomic) UIColor *valueBackgroundColor;
@property (retain, nonatomic) UIColor *valueTextColor;
@property (assign, nonatomic) CGFloat valueCornerRadius;
@property (assign, nonatomic) NSTextAlignment valueAlignment;
@property (assign, nonatomic) CGFloat valueFontSize;


- (TextControl *)initWithParam:(rcp_param_t)param title:(BOOL)hasTitle;
- (IntEditInfo *)intEditInfo;
- (UIntEditInfo *)uintEditInfo;
- (StrEditInfo *)strEditInfo;
- (NSString *)baseValue;
- (BOOL)isEditable;

- (void)updateInt:(const int)val editInfo:(rcp_cur_int_edit_info_t)info infoIsValid:(BOOL)infoIsValid;
- (void)updateUInt:(const uint32_t)val editInfo:(rcp_cur_uint_edit_info_t)info infoIsValid:(BOOL)infoIsValid;
- (void)updateStr:(NSString *)str;
- (void)updateStr:(NSString *)str editInfo:(rcp_cur_str_edit_info_t)info infoIsValid:(BOOL)infoIsValid;

- (void)setFrame:(CGRect)frame;

- (void)setValueBackgroundColor:(UIColor *)color;
- (void)setValueTextColor:(UIColor *)color;
- (void)setValueCornerRadius:(CGFloat)radius;
- (void)setValueAlignment:(NSTextAlignment)alignment;
- (void)setValueFontSize:(CGFloat)size;

- (void)setTitle:(NSString *)title;
- (void)setEnabled:(BOOL)enabled;

- (void)requestTextInputView;

@end
