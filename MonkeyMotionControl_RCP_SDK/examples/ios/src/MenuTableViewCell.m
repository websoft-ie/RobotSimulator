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

#import "MenuTableViewCell.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

@interface MenuTableViewCell()

@property (retain, nonatomic) MenuNode *node;
@property (retain, nonatomic) UIView *separator;
@property (retain, nonatomic) UILabel *titleLabel;
@property (retain, nonatomic) UILabel *valueLabel;
@property (retain, nonatomic) UIImageView *accessoryImage;
@property (retain, nonatomic) UIButton *button;
@property (retain, nonatomic) UISwitch *switchView;
@property (retain, nonatomic) PickerControl *pickerControl;
@property (retain, nonatomic) SliderControl *sliderControl;
@property (retain, nonatomic) TextControl *textControl;


- (void)pickerControlSelected:(PickerControl *)pickerControl;
- (void)pickerControlRequestedTextInputView:(PickerControl *)pickerControl;
- (void)sliderControlSliderSelected:(SliderControl *)sliderControl;
- (void)textControlSelected:(TextControl *)textControl;
- (void)textControlRequestedTextInputView:(TextControl *)textControl;

- (void)buttonPressed;
- (void)switchToggled;

- (void)layoutSubviews;
- (void)setupUI;

@end


@implementation MenuTableViewCell

@synthesize delegate, hasFocus, orderedListViewRequested, curveViewRequested, timecode, ipAddress;

- (MenuTableViewCell *)initWithReuseIdentifier:(NSString *)identifier
{
    if (self = [super initWithStyle:UITableViewCellStyleDefault reuseIdentifier:identifier])
    {
        delegate = nil;
        hasFocus = NO;
        orderedListViewRequested = NO;
        curveViewRequested = NO;
        timecode = 0;
        ipAddress = 0;
        
        _node = [[MenuNode alloc] init];
        _separator = [[UIView alloc] init];
        _titleLabel = [[UILabel alloc] init];
        _valueLabel = [[UILabel alloc] init];
        _accessoryImage = [[UIImageView alloc] init];
        _button = [[UIButton alloc] init];
        _switchView = [[UISwitch alloc] init];
        _pickerControl = [[PickerControl alloc] initWithParam:RCP_PARAM_COUNT title:NO];
        _sliderControl = [[SliderControl alloc] initWithParam:RCP_PARAM_COUNT title:NO];
        _textControl = [[TextControl alloc] initWithParam:RCP_PARAM_COUNT title:NO];
        
        [self setupUI];
    }
    
    return self;
}

- (rcp_param_t)paramID
{
    return _node.paramID;
}

- (rcp_menu_node_id_t)nodeID
{
    return _node.nodeID;
}

- (BOOL)hasOpenPicker
{
    if (![_pickerControl isHidden])
    {
        return [_pickerControl isOpen];
    }
    else if (![_sliderControl isHidden])
    {
        return [_sliderControl isOpen];
    }
    
    return NO;
}

- (void)setHasFocus:(BOOL)hasF
{
    hasFocus = hasF;
    
    if (!hasFocus)
    {
        if (![_pickerControl isHidden])
        {
            if ([_pickerControl isOpen])
            {
                [_pickerControl close];
            }
        }
        else if (![_sliderControl isHidden])
        {
            if ([_sliderControl isOpen])
            {
                [_sliderControl close];
            }
        }
    }
}

- (void)setNode:(MenuNode *)node
{
    _node = node;
    
    timecode = 0;
    ipAddress = 0;
    
    [_titleLabel setText:_node.title];
    
    switch (_node.type)
    {
        case RCP_MENU_NODE_TYPE_BRANCH:
            [_accessoryImage setImage:[UIImage imageNamed:@"disclosure_icon"]];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:NO];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_ACTION_LEAF:
            [_button setTitle:@"OK" forState:UIControlStateNormal];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleNone];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:YES];
            [_button setHidden:NO];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_CURVE_LEAF:
            [_accessoryImage setImage:[UIImage imageNamed:@"curve_icon"]];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:NO];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_ENABLE_LEAF:
            [self setSelectionStyle:UITableViewCellSelectionStyleNone];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:NO];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_IP_ADDRESS_LEAF:
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:NO];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_LIST_LEAF:
        {
            send_value_type_t sendValueType = SEND_VALUE_TYPE_INT;
            
            if (_node.sendUInt)
            {
                sendValueType = SEND_VALUE_TYPE_UINT;
            }
            else if (_node.sendStr)
            {
                sendValueType = SEND_VALUE_TYPE_STR;
            }
            
            if ([RCPUtil menuParamHasSlider:_node.paramID])
            {
                _sliderControl.paramID = _node.paramID;
                
                [self setSelectionStyle:UITableViewCellSelectionStyleNone];
                [_accessoryImage setHidden:YES];
                [_pickerControl setHidden:YES];
                [_sliderControl setHidden:NO];
            }
            else
            {
                if (_node.paramID == RCP_PARAM_COLOR_TEMPERATURE_PRESET)
                {
                    [_accessoryImage setImage:[UIImage imageNamed:@"list_icon"]];
                    [_accessoryImage setHidden:NO];
                    [_pickerControl setHidden:YES];
                }
                else
                {
                    [_accessoryImage setHidden:YES];
                    [_pickerControl setHidden:NO];
                }
                
                _pickerControl.paramID = _node.paramID;
                
                [self setSelectionStyle:UITableViewCellSelectionStyleGray];
                [_sliderControl setHidden:YES];
            }
            
            [_valueLabel setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_textControl setHidden:YES];
            break;
        }
            
        case RCP_MENU_NODE_TYPE_NUMBER_LEAF:
            _textControl.paramID = _node.paramID;
            
            if (_node.sendInt)
            {
                _textControl.sendValueType = SEND_VALUE_TYPE_INT;
            }
            else if (_node.sendUInt)
            {
                _textControl.sendValueType = SEND_VALUE_TYPE_UINT;
            }
            else if (_node.sendStr)
            {
                _textControl.sendValueType = SEND_VALUE_TYPE_STR;
            }
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:NO];
            break;
            
        case RCP_MENU_NODE_TYPE_TEXT_LEAF:
            _textControl.paramID = _node.paramID;
            _textControl.sendValueType = SEND_VALUE_TYPE_STR;
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:NO];
            break;
            
        case RCP_MENU_NODE_TYPE_ORDERED_LIST_LEAF:
            [_accessoryImage setImage:[UIImage imageNamed:@"list_icon"]];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:NO];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_DATETIME_LEAF:
            [_button setTitle:@"Use Device Time" forState:UIControlStateNormal];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleNone];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:YES];
            [_button setHidden:NO];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_TIMECODE_LEAF:
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:NO];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_STATUS_LEAF:
            [self setSelectionStyle:UITableViewCellSelectionStyleNone];
            [_valueLabel setHidden:NO];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_MULTI_ACTION_LIST_LEAF:
            [_accessoryImage setImage:[UIImage imageNamed:@"list_icon"]];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:YES];
            [_accessoryImage setHidden:NO];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        case RCP_MENU_NODE_TYPE_NOT_YET_SUPPORTED_LEAF:
            [_valueLabel setTextColor:[RCPUtil colorFromStatus:RCP_PARAM_DISPLAY_STATUS_NORMAL]];
            [_valueLabel setText:@"Not supported on iOS"];
            
            [self setSelectionStyle:UITableViewCellSelectionStyleGray];
            [_valueLabel setHidden:NO];
            [_accessoryImage setHidden:YES];
            [_button setHidden:YES];
            [_switchView setHidden:YES];
            [_pickerControl setHidden:YES];
            [_sliderControl setHidden:YES];
            [_textControl setHidden:YES];
            break;
            
        default:
            break;
    }
    
    [self layoutSubviews];
}

- (void)updateInt:(const int)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_int_edit_info_t)editInfo
{
    if (![_switchView isHidden])
    {
        if ([_node.args count] > 1)
        {
            RCPListItem *enableItem = [_node.args objectAtIndex:0];
            RCPListItem *disableItem = [_node.args objectAtIndex:1];
            BOOL setOn = NO;
            
            if (val == enableItem.num)
            {
                setOn = YES;
            }
            else if (val == disableItem.num)
            {
                setOn = NO;
            }
            
            [_switchView setOn:setOn animated:YES];
        }
    }
    else if (![_pickerControl isHidden])
    {
        [_pickerControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    else if (![_sliderControl isHidden])
    {
        if (!_sliderControl.ignoreCameraNumUpdates)
        {
            [_sliderControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
        }
    }
    else if (![_textControl isHidden])
    {
        [_textControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    else if (![_valueLabel isHidden])
    {
        if (_node.type == RCP_MENU_NODE_TYPE_TIMECODE_LEAF)
        {
            timecode = val;
        }
    }
}

- (void)updateUInt:(const uint32_t)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_uint_edit_info_t)editInfo
{
    if (![_pickerControl isHidden])
    {
        [_pickerControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    else if (![_sliderControl isHidden])
    {
        if (!_sliderControl.ignoreCameraNumUpdates)
        {
            [_sliderControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
        }
    }
    else if (![_textControl isHidden])
    {
        [_textControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    else if (![_valueLabel isHidden])
    {
        if (_node.type == RCP_MENU_NODE_TYPE_IP_ADDRESS_LEAF)
        {
            ipAddress = val;
        }
    }
}

- (void)updateStr:(const char *)str status:(rcp_param_status_t)status
{
    UIColor *color = [RCPUtil colorFromStatus:status];
    UIColor *modifiedNormalColor = [UIColor colorWithWhite:0.8 alpha:1.0];
    NSString *text = [[NSString alloc] initWithFormat:@"%s", str];
    
    if (![_pickerControl isHidden])
    {
        [_pickerControl setTextFieldTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        [_pickerControl updateStr:text];
    }
    else if (![_sliderControl isHidden])
    {
        [_sliderControl setTextFieldTextColor:color];
        [_sliderControl updateStr:text];
    }
    else if (![_textControl isHidden])
    {
        [_textControl setValueTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        [_textControl updateStr:text];
    }
    else if (![_valueLabel isHidden])
    {
        [_valueLabel setTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        [_valueLabel setText:text];
    }
}

- (void)updateStr:(const char *)str status:(rcp_param_status_t)status infoIsValid:(BOOL)infoIsValid info:(rcp_cur_str_edit_info_t)editInfo
{
    UIColor *color = [RCPUtil colorFromStatus:status];
    UIColor *modifiedNormalColor = [UIColor colorWithWhite:0.8 alpha:1.0];
    NSString *text = [[NSString alloc] initWithFormat:@"%s", str];
    
    if (![_pickerControl isHidden])
    {
        [_pickerControl setTextFieldTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        [_pickerControl updateStr:text];
    }
    else if (![_sliderControl isHidden])
    {
        [_sliderControl setTextFieldTextColor:color];
        [_sliderControl updateStr:text];
    }
    else if (![_textControl isHidden])
    {
        [_textControl setValueTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        [_textControl updateStr:text editInfo:editInfo infoIsValid:infoIsValid];
    }
    else if (![_valueLabel isHidden])
    {
        [_valueLabel setTextColor:((status == RCP_PARAM_DISPLAY_STATUS_NORMAL) ? modifiedNormalColor : color)];
        
        if (infoIsValid && editInfo.is_password)
        {
            [_valueLabel setText:[RCPUtil passwordStringFromString:text]];
        }
        else
        {
            [_valueLabel setText:text];
        }
    }
}

- (void)updateList:(RCPList *)list
{
    if (![_pickerControl isHidden] || _node.paramID == RCP_PARAM_COLOR_TEMPERATURE_PRESET)
    {
        if (!_pickerControl.ignoreCameraListUpdates)
        {
            [_pickerControl updateList:list];
        }
    }
    else if (![_sliderControl isHidden])
    {
        if (!_sliderControl.ignoreCameraListUpdates)
        {
            [_sliderControl updateList:list];
        }
    }
}

- (void)updateStatus:(BOOL)enabled
{
    [_accessoryImage setAlpha:(enabled ? 1.0 : 0.5)];
    
    [_titleLabel setEnabled:enabled];
    [_accessoryImage setUserInteractionEnabled:enabled];
    [_valueLabel setEnabled:enabled];
    [_button setEnabled:enabled];
    [_switchView setEnabled:enabled];
    [_pickerControl setEnabled:enabled];
    [_sliderControl setEnabled:enabled];
    [_textControl setEnabled:enabled];
    
    [self setUserInteractionEnabled:enabled];
}

- (void)keyboardWillShow
{
    // Nothing needs to be done within a UITableViewCell when the keyboard is shown.
}

- (void)keyboardWillHide
{
    if (![_pickerControl isHidden])
    {
        _pickerControl.ignoreCameraListUpdates = NO;
        [_pickerControl sendRCPValue];
    }
    else if (![_sliderControl isHidden])
    {
        _sliderControl.ignoreCameraListUpdates = NO;
        [_sliderControl sendRCPValue];
    }
}

- (void)requestPicker
{
    if (![_pickerControl isHidden])
    {
        [_pickerControl requestPicker];
    }
    else if (![_sliderControl isHidden])
    {
        [_sliderControl requestPicker];
    }
}

- (void)requestTextInputView
{
    if (![_pickerControl isHidden])
    {
        [_pickerControl requestTextInputView];
    }
    else if (![_sliderControl isHidden])
    {
        [_sliderControl requestTextInputView];
    }
    else if (![_textControl isHidden])
    {
        [_textControl requestTextInputView];
    }
}

- (void)pickerControlSelected:(PickerControl *)pickerControl
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCellSelected:)])
        {
            [delegate menuTableViewCellSelected:self];
        }
    }
}

- (void)pickerControlRequestedTextInputView:(PickerControl *)pickerControl
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCell:textInputViewRequestedFromPickerControl:)])
        {
            [delegate menuTableViewCell:self textInputViewRequestedFromPickerControl:pickerControl];
        }
    }
}

- (void)sliderControlSliderSelected:(SliderControl *)sliderControl
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCellSelected:)])
        {
            [delegate menuTableViewCellSelected:self];
        }
    }
}

- (void)textControlSelected:(TextControl *)textControl
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCellSelected:)])
        {
            [delegate menuTableViewCellSelected:self];
        }
    }
}

- (void)textControlRequestedTextInputView:(TextControl *)textControl
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCell:textInputViewRequestedFromTextControl:)])
        {
            [delegate menuTableViewCell:self textInputViewRequestedFromTextControl:textControl];
        }
    }
}

- (void)buttonPressed
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCellSelected:)])
        {
            [delegate menuTableViewCellSelected:self];
        }
    }
    
    RCPHandler *handler = [RCPHandler instance];
    
    if (_node.type == RCP_MENU_NODE_TYPE_ACTION_LEAF)
    {
        if (_node.hasPayload)
        {
            [handler rcpSetInt:_node.paramID value:_node.payload];
        }
        else
        {
            [handler rcpSend:_node.paramID];
        }
    }
    else if (_node.type == RCP_MENU_NODE_TYPE_DATETIME_LEAF)
    {
        time_t rawtime;
        time_t local;
        struct tm *timeinfo;
        
        time(&rawtime);
        timeinfo = localtime(&rawtime);
        local = timegm(timeinfo);
        
        [handler rcpSetInt:_node.paramID value:local];
    }
}

- (void)switchToggled
{
    if (delegate != nil)
    {
        if ([delegate respondsToSelector:@selector(menuTableViewCellSelected:)])
        {
            [delegate menuTableViewCellSelected:self];
        }
    }
    
    if ([_node.args count] > 1)
    {
        RCPHandler *handler = [RCPHandler instance];
        RCPListItem *enableItem = [_node.args objectAtIndex:0];
        RCPListItem *disableItem = [_node.args objectAtIndex:1];
        
        if (_switchView.on)
        {
            [handler rcpSetInt:_node.paramID value:disableItem.num];
        }
        else
        {
            [handler rcpSetInt:_node.paramID value:enableItem.num];
        }
    }
}

- (void)layoutSubviews
{
    const CGFloat width = self.contentView.bounds.size.width;
    const CGFloat height = self.contentView.bounds.size.height;
    const CGFloat spacing = 5.0;
    
    const CGSize separatorSize = CGSizeMake(width, 0.5);
    const CGFloat separatorY = height - separatorSize.height;
    
    const CGSize titleLabelSize = [RCPUtil sizeOfText:_titleLabel.text font:_titleLabel.font];
    const CGFloat titleLabelY = (height - titleLabelSize.height) / 2.0;
    
    const CGFloat minButtonWidth = 60.0;
    CGFloat buttonWidth = [RCPUtil sizeOfText:[_button.titleLabel text] font:_button.titleLabel.font].width + 15.0;
    if (buttonWidth < minButtonWidth)
    {
        buttonWidth = minButtonWidth;
    }
    const CGSize buttonSize = CGSizeMake(buttonWidth, 30.0);
    const CGFloat buttonY = (height - buttonSize.height) / 2.0;
    
    const CGSize accessorySize = (_accessoryImage.image == nil) ? CGSizeMake(20.0, 20.0) : _accessoryImage.image.size;
    const CGFloat accessoryY = (height - accessorySize.height) / 2.0;
    
    const CGSize valueLabelSize = CGSizeMake(width - titleLabelSize.width - (spacing * 3.0), height - 8.0);
    const CGFloat valueLabelY = (height - valueLabelSize.height) / 2.0;
    
    const CGSize switchSize = _switchView.bounds.size;
    const CGFloat switchY = (height - switchSize.height) / 2.0;
    
    const CGSize pickerControlSize = CGSizeMake(width - titleLabelSize.width - (spacing * 3.0), 30.0);
    const CGFloat pickerControlY = (height - pickerControlSize.height) / 2.0;
    
    const CGSize sliderControlSize = CGSizeMake(pickerControlSize.width, 31.0);
    const CGFloat sliderControlY = (height - sliderControlSize.height) / 2.0;
    
    const CGSize textControlSize = pickerControlSize;
    const CGFloat textControlY = pickerControlY;
    
    
    [_separator setFrame:CGRectMake(0.0, separatorY, separatorSize.width, separatorSize.height)];
    [_titleLabel setFrame:CGRectMake(spacing, titleLabelY, titleLabelSize.width, titleLabelSize.height)];
    [_valueLabel setFrame:CGRectMake(width - valueLabelSize.width - spacing, valueLabelY, valueLabelSize.width, valueLabelSize.height)];
    [_accessoryImage setFrame:CGRectMake(width - accessorySize.width - spacing, accessoryY, accessorySize.width, accessorySize.height)];
    [_button setFrame:CGRectMake(width - buttonSize.width - spacing, buttonY, buttonSize.width, buttonSize.height)];
    [_switchView setFrame:CGRectMake(width - switchSize.width - spacing, switchY, switchSize.width, switchSize.height)];
    [_pickerControl setFrame:CGRectMake(width - pickerControlSize.width - spacing, pickerControlY, pickerControlSize.width, pickerControlSize.height)];
    [_sliderControl setFrame:CGRectMake(width - sliderControlSize.width - spacing, sliderControlY, sliderControlSize.width, sliderControlSize.height)];
    [_textControl setFrame:CGRectMake(width - textControlSize.width - spacing, textControlY, textControlSize.width, textControlSize.height)];
    
    /* In case a value cannot be displayed (such as a RCP GET not being valid), this prevents an old value from being shown. */
    const rcp_cur_str_edit_info_t emptyEditInfo = {0};
    [_valueLabel setText:@""];
    [_pickerControl updateStr:@""];
    [_sliderControl updateStr:@""];
    [_textControl updateStr:@"" editInfo:emptyEditInfo infoIsValid:NO];
    
    RCPHandler *handler = [RCPHandler instance];
    
    if (_node.type != RCP_MENU_NODE_TYPE_BRANCH && _node.type != RCP_MENU_NODE_TYPE_ACTION_LEAF && _node.type != RCP_MENU_NODE_TYPE_DATETIME_LEAF && _node.type != RCP_MENU_NODE_TYPE_NOT_YET_SUPPORTED_LEAF)
    {
        if (_node.type == RCP_MENU_NODE_TYPE_LIST_LEAF && [RCPUtil menuParamHasSlider:_node.paramID])
        {
            [handler rcpGetList:_node.paramID];
        }
        
        [handler rcpGet:_node.paramID];
    }
    
    if (_node.type == RCP_MENU_NODE_TYPE_BRANCH)
    {
        if ([handler rcpMenuNodeStatusIsSupported])
        {
            [handler rcpGetMenuNodeStatus:_node.nodeID];
        }
        else
        {
            [self updateStatus:YES];
        }
    }
    else if (_node.type == RCP_MENU_NODE_TYPE_NOT_YET_SUPPORTED_LEAF)
    {
        [self updateStatus:YES];
    }
    else
    {
        [handler rcpGetStatus:_node.paramID];
    }
}

- (void)setupUI
{
    UIColor *textColor = [UIColor whiteColor];
    UIColor *textFieldBackgroundColor = [UIColor colorWithRed:0.2 green:0.2 blue:0.2 alpha:1.0];
    
    
    [self.contentView setFrame:CGRectMake(0.0, 0.0, [UIScreen mainScreen].bounds.size.width, 44.0)];
    
    [_separator setBackgroundColor:[UIColor whiteColor]];
    
    [_titleLabel setTextAlignment:NSTextAlignmentLeft];
    [_titleLabel setFont:[UIFont systemFontOfSize:16.0]];
    [_titleLabel setTextColor:textColor];
    
    [_accessoryImage setImage:[UIImage imageNamed:@"disclosure_icon"]];
    
    [_valueLabel setTextAlignment:NSTextAlignmentRight];
    [_valueLabel setFont:_titleLabel.font];
    
    [_button.titleLabel setFont:[UIFont systemFontOfSize:16.0]];
    [_button setTitleColor:textColor forState:UIControlStateNormal];
    [_button setTitleColor:[UIColor colorWithWhite:1.0 alpha:0.5] forState:UIControlStateHighlighted];
    [_button setTitleColor:[UIColor lightGrayColor] forState:UIControlStateDisabled];
    [_button.layer setBorderColor:[textColor CGColor]];
    [_button.layer setBorderWidth:0.5];
    [_button.layer setCornerRadius:5.0];
    [_button addTarget:self action:@selector(buttonPressed) forControlEvents:UIControlEventTouchUpInside];
    
    // Disabling the default recognizers in order to make it so the switch status only updates based on the camera's response rather than the user's touch
    for (UIGestureRecognizer *recognizer in _switchView.gestureRecognizers)
    {
        [recognizer setEnabled:NO];
    }
    UITapGestureRecognizer *tap = [[UITapGestureRecognizer alloc] init];
    UIView *switchCover = [[UIView alloc] initWithFrame:_switchView.frame];
    [tap addTarget:self action:@selector(switchToggled)];
    [switchCover addGestureRecognizer:tap];
    [_switchView addSubview:switchCover];
    
    [_pickerControl setUserInteractionEnabled:NO];
    [_pickerControl setTextFieldBackgroundColor:[UIColor clearColor]];
    [_pickerControl setTextFieldAlignment:NSTextAlignmentRight];
    [_pickerControl setTextFieldCornerRadius:3.0];
    [_pickerControl setDelegate:self];
    
    [_sliderControl setTextFieldBackgroundColor:textFieldBackgroundColor];
    [_sliderControl setTextFieldAlignment:NSTextAlignmentCenter];
    [_sliderControl setTextFieldCornerRadius:3.0];
    [_sliderControl setDelegate:self];
    [_sliderControl setPickerDelegate:self];
    
    [_textControl setUserInteractionEnabled:NO];
    [_textControl setValueBackgroundColor:[UIColor clearColor]];
    [_textControl setValueAlignment:NSTextAlignmentRight];
    [_textControl setValueCornerRadius:3.0];
    [_textControl setDelegate:self];
    
    
    [_accessoryImage setHidden:YES];
    [_valueLabel setHidden:YES];
    [_button setHidden:YES];
    [_switchView setHidden:YES];
    [_pickerControl setHidden:YES];
    [_sliderControl setHidden:YES];
    [_textControl setHidden:YES];
    
    
    [self.contentView addSubview:_separator];
    [self.contentView addSubview:_titleLabel];
    [self.contentView addSubview:_valueLabel];
    [self.contentView addSubview:_accessoryImage];
    [self.contentView addSubview:_button];
    [self.contentView addSubview:_switchView];
    [self.contentView addSubview:_pickerControl];
    [self.contentView addSubview:_sliderControl];
    [self.contentView addSubview:_textControl];
}

@end
