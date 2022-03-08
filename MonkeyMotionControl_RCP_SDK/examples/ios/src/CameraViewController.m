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

#import "CameraViewController.h"
#import "SliderControl.h"
#import "TextInputView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Histogram.h"
#import "Logger.h"

@interface CameraViewController ()

@property (retain, nonatomic) PickerControl *pickerControl;
@property (retain, nonatomic) SliderControl *sliderControl;
@property (retain, nonatomic) TextControl *textControl;
@property (retain, nonatomic) TextInputView *textInputView;

@property (weak, nonatomic) IBOutlet UIButton *markFrameButton;
@property (weak, nonatomic) IBOutlet UIImageView *frameTaggedImage;
@property (weak, nonatomic) IBOutlet Histogram *histogram;
@property (weak, nonatomic) IBOutlet UILabel *mediaNameLabel;
@property (weak, nonatomic) IBOutlet UILabel *mediaValueLabel;
@property (weak, nonatomic) IBOutlet UILabel *timecodeLabel;
@property (weak, nonatomic) IBOutlet UIButton *recordButton;

- (void)pickerControlRequestedTextInputView:(PickerControl *)pickerControl;
- (void)textControlRequestedTextInputView:(TextControl *)textControl;

- (IBAction)markFramePressed:(UIButton *)sender;
- (IBAction)recordPressed:(UIButton *)sender;

@end


@implementation CameraViewController

- (void)setConnected:(BOOL)connected
{
    if (connected)
    {
        RCPHandler *handler = [RCPHandler instance];
        
        [handler rcpGetList:_pickerControl.paramID];
        [handler rcpGetList:_sliderControl.paramID];
        
        [handler rcpGet:_pickerControl.paramID];
        [handler rcpGet:_sliderControl.paramID];
        [handler rcpGet:_textControl.paramID];
        [handler rcpGet:RCP_PARAM_MEDIA_DISPLAY_LABEL];
        [handler rcpGet:RCP_PARAM_MEDIA_DISPLAY_VAL];
        [handler rcpGet:RCP_PARAM_TIMECODE];
        [handler rcpGet:RCP_PARAM_RECORD_STATE];
        
        [handler rcpGetStatus:_pickerControl.paramID];
        [handler rcpGetStatus:_sliderControl.paramID];
        [handler rcpGetStatus:_textControl.paramID];
        [handler rcpGetStatus:RCP_PARAM_MEDIA_DISPLAY_LABEL];
        [handler rcpGetStatus:RCP_PARAM_MEDIA_DISPLAY_VAL];
        [handler rcpGetStatus:RCP_PARAM_TIMECODE];
        [handler rcpGetStatus:RCP_PARAM_RECORD_STATE];
        
        [_pickerControl setTitle:[[handler rcpGetLabel:_pickerControl.paramID] uppercaseString]];
        [_sliderControl setTitle:[[handler rcpGetLabel:_sliderControl.paramID] uppercaseString]];
        [_textControl setTitle:[[handler rcpGetLabel:_textControl.paramID] uppercaseString]];
        
        rcp_param_properties_t properties = [handler rcpGetProperties:_textControl.paramID];
        
        if (properties.has_set_int)
        {
            _textControl.sendValueType = SEND_VALUE_TYPE_INT;
        }
        else if (properties.has_set_uint)
        {
            _textControl.sendValueType = SEND_VALUE_TYPE_UINT;
        }
        else if (properties.has_set_str)
        {
            _textControl.sendValueType = SEND_VALUE_TYPE_STR;
        }
    }
    else
    {
        [_pickerControl setEnabled:NO];
        [_sliderControl setEnabled:NO];
        [_textControl setEnabled:NO];
        
        [_mediaNameLabel setEnabled:NO];
        [_mediaValueLabel setEnabled:NO];
        [_timecodeLabel setEnabled:NO];
        [_recordButton setEnabled:NO];
    }
    
    [_markFrameButton setEnabled:connected];
    [self.view setUserInteractionEnabled:connected];
    [self.navigationController.tabBarItem setEnabled:connected];
}

- (void)updateParam:(rcp_param_t)paramID integer:(const int)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_int_edit_info_t)editInfo
{
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID)
    {
        [_pickerControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    if (paramID == _sliderControl.paramID && !_sliderControl.ignoreCameraNumUpdates)
    {
        [_sliderControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    if (paramID == _textControl.paramID)
    {
        [_textControl updateInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    switch (paramID)
    {
        case RCP_PARAM_RECORD_STATE:
            switch (val)
        {
            case RECORD_STATE_NOT_RECORDING:
                [_recordButton.layer setBackgroundColor:[[UIColor colorWithRed:240.0/255.0 green:240.0/255.0 blue:240.0/255.0 alpha:1.0] CGColor]];
                [_recordButton setTitle:@"RECORD" forState:UIControlStateNormal];
                [_recordButton setEnabled:YES];
                break;
                
            case RECORD_STATE_RECORDING:
                [_recordButton.layer setBackgroundColor:[[UIColor redColor] CGColor]];
                [_recordButton setTitle:@"RECORDING" forState:UIControlStateNormal];
                [_recordButton setEnabled:YES];
                break;
                
            case RECORD_STATE_FINALIZING:
                [_recordButton.layer setBackgroundColor:[[UIColor yellowColor] CGColor]];
                [_recordButton setTitle:@"FINALIZING" forState:UIControlStateNormal];
                [_recordButton setEnabled:NO];
                break;
                
            case RECORD_STATE_PRE_RECORDING:
                [_recordButton.layer setBackgroundColor:[[UIColor yellowColor] CGColor]];
                [_recordButton setTitle:@"PRE-RECORDING" forState:UIControlStateNormal];
                [_recordButton setEnabled:YES];
                break;
                
            default:
                [_recordButton setEnabled:YES];
                break;
        }
            break;
            
        case RCP_PARAM_RED_CLIP:
            [_histogram setRedClipOn:val];
            break;
            
        case RCP_PARAM_GREEN_CLIP:
            [_histogram setGreenClipOn:val];
            break;
            
        case RCP_PARAM_BLUE_CLIP:
            [_histogram setBlueClipOn:val];
            break;
            
        default:
            break;
    }
}

- (void)updateParam:(rcp_param_t)paramID uinteger:(const uint32_t)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_uint_edit_info_t)editInfo
{
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID)
    {
        [_pickerControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    if (paramID == _sliderControl.paramID && !_sliderControl.ignoreCameraNumUpdates)
    {
        [_sliderControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    if (paramID == _textControl.paramID)
    {
        [_textControl updateUInt:val editInfo:editInfo infoIsValid:infoIsValid];
    }
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status
{
    NSString *text = [NSString stringWithFormat:@"%s", str];
    UIColor *color = [RCPUtil colorFromStatus:status];
    
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID)
    {
        [_pickerControl setTextFieldTextColor:color];
        [_pickerControl updateStr:text];
    }
    
    if (paramID == _sliderControl.paramID)
    {
        [_sliderControl setTextFieldTextColor:color];
        [_sliderControl updateStr:text];
    }
    
    if (paramID == _textControl.paramID)
    {
        [_textControl setValueTextColor:color];
        [_textControl updateStr:text];
    }
    
    switch (paramID)
    {
        case RCP_PARAM_MEDIA_DISPLAY_LABEL:
            [_mediaNameLabel setTextColor:color];
            [_mediaNameLabel setText:text];
            break;
            
        case RCP_PARAM_MEDIA_DISPLAY_VAL:
            [_mediaValueLabel setTextColor:color];
            [_mediaValueLabel setText:text];
            break;
            
        case RCP_PARAM_TIMECODE:
            [_timecodeLabel setTextColor:color];
            [_timecodeLabel setText:text];
            break;
            
        default:
            break;
    }
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status infoIsValid:(BOOL)infoIsValid info:(rcp_cur_str_edit_info_t)editInfo
{
    NSString *text = [NSString stringWithFormat:@"%s", str];
    UIColor *color = [RCPUtil colorFromStatus:status];
    
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID)
    {
        [_pickerControl setTextFieldTextColor:color];
        [_pickerControl updateStr:text];
    }
    
    if (paramID == _sliderControl.paramID)
    {
        [_sliderControl setTextFieldTextColor:color];
        [_sliderControl updateStr:text];
    }
    
    if (paramID == _textControl.paramID)
    {
        [_textControl setValueTextColor:color];
        [_textControl updateStr:text editInfo:editInfo infoIsValid:infoIsValid];
    }
    
    switch (paramID)
    {
        case RCP_PARAM_MEDIA_DISPLAY_LABEL:
            [_mediaNameLabel setTextColor:color];
            [_mediaNameLabel setText:text];
            break;
            
        case RCP_PARAM_MEDIA_DISPLAY_VAL:
            [_mediaValueLabel setTextColor:color];
            [_mediaValueLabel setText:text];
            break;
            
        case RCP_PARAM_TIMECODE:
            [_timecodeLabel setTextColor:color];
            [_timecodeLabel setText:text];
            break;
            
        default:
            break;
    }
}

- (void)updateParam:(rcp_param_t)paramID list:(RCPList *)list
{
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID && !_pickerControl.ignoreCameraListUpdates)
    {
        [_pickerControl updateList:list];
    }
    
    if (paramID == _sliderControl.paramID && !_sliderControl.ignoreCameraListUpdates)
    {
        [_sliderControl updateList:list];
    }
}

- (void)updateParam:(rcp_param_t)paramID frameTagInfo:(tag_info_t)tagInfo
{
    if (tagInfo.type == TAG_INFO_TAG_TYPE_STILL)
    {
        [_frameTaggedImage setAlpha:1.0];
        [UIView animateWithDuration:0.3 delay:0 options:0 animations:^{ [_frameTaggedImage setAlpha:0.0]; } completion:nil];
    }
}

- (void)updateParam:(rcp_param_t)paramID enabled:(BOOL)enabled
{
    // These are laid out in multiple if statements rather than if/else if statements since you could technically assign the same parameter to multiple controls
    
    if (paramID == _pickerControl.paramID)
    {
        [_pickerControl setEnabled:enabled];
        [_pickerControl setUserInteractionEnabled:enabled];
    }
    
    if (paramID == _sliderControl.paramID)
    {
        [_sliderControl setEnabled:enabled];
        [_sliderControl setUserInteractionEnabled:enabled];
    }
    
    if (paramID == _textControl.paramID)
    {
        [_textControl setEnabled:enabled];
        [_textControl setUserInteractionEnabled:enabled];
    }

    switch (paramID)
    {
        case RCP_PARAM_MEDIA_DISPLAY_LABEL:
            [_mediaNameLabel setEnabled:enabled];
            break;
            
        case RCP_PARAM_MEDIA_DISPLAY_VAL:
            [_mediaValueLabel setEnabled:enabled];
            break;
            
        case RCP_PARAM_TIMECODE:
            [_timecodeLabel setEnabled:enabled];
            break;
            
        case RCP_PARAM_RECORD_STATE:
            [_recordButton setEnabled:enabled];
            break;
                            
        default:
            break;
    }
}

- (void)updateHistogram:(const rcp_cur_hist_cb_data_t *)data
{
    [_histogram setHistogramData:data];
    [_histogram setNeedsDisplay];
}

- (void)keyboardWillShow:(CGRect)keyboardFrame
{
    const CGFloat navbarHeight = 64.0;
    const CGFloat keyboardTop = keyboardFrame.origin.y - keyboardFrame.size.height;
    const CGFloat padding = 10.0;
    CGFloat viewBottom = navbarHeight;
    
    if ([_pickerControl isOpen])
    {
        _pickerControl.ignoreCameraListUpdates = YES;
        viewBottom += _pickerControl.frame.origin.y + _pickerControl.frame.size.height;
    }
    else if ([_sliderControl isOpen])
    {
        _sliderControl.ignoreCameraListUpdates = YES;
        viewBottom += _sliderControl.frame.origin.y + _sliderControl.frame.size.height;
    }
    
    
    if (viewBottom + padding > keyboardTop)
    {
        CGFloat offset = viewBottom + padding - keyboardTop;
        
        [UIView beginAnimations:nil context:NULL];
        [UIView setAnimationDuration:0.3];
        
        CGRect viewFrame = self.view.frame;
        viewFrame.size.height += keyboardFrame.size.height;
        viewFrame.origin.y -= offset;
        self.view.frame = viewFrame;
        
        [UIView commitAnimations];
    }
}

- (void)keyboardWillHide:(CGRect)keyboardFrame
{
    const CGFloat navbarHeight = 64.0;
    CGRect viewFrame = self.view.frame;
    
    if ([_pickerControl isOpen])
    {
        _pickerControl.ignoreCameraListUpdates = NO;
        [_pickerControl sendRCPValue];
    }
    else if ([_sliderControl isOpen])
    {
        _sliderControl.ignoreCameraListUpdates = NO;
        [_sliderControl sendRCPValue];
    }
    
    
    // Ensures that the view has actually been moved up
    if (viewFrame.origin.y < navbarHeight)
    {
        [UIView beginAnimations:nil context:NULL];
        [UIView setAnimationDuration:0.3];
        
        viewFrame.origin.y = navbarHeight;
        viewFrame.size.height -= keyboardFrame.size.height;
        self.view.frame = viewFrame;
        
        [UIView commitAnimations];
    }
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    _pickerControl = [[PickerControl alloc] initWithParam:RCP_PARAM_SENSOR_FRAME_RATE title:YES];
    _sliderControl = [[SliderControl alloc] initWithParam:RCP_PARAM_SATURATION title:YES];
    _textControl = [[TextControl alloc] initWithParam:RCP_PARAM_SLATE_SCENE title:YES];
    _textInputView = [[TextInputView alloc] init];
    
    
    [_frameTaggedImage setAlpha:0.0];
    
    [_markFrameButton.layer setCornerRadius:5.0];
    [_recordButton.layer setCornerRadius:5.0];
    
    
    [_pickerControl setFrame:CGRectMake(20.0, 4.0, 140.0, 55.0)];
    [_pickerControl setTextFieldBackgroundColor:[UIColor blackColor]];
    [_pickerControl setTextFieldFontSize:16.0];
    [_pickerControl setTextFieldAlignment:NSTextAlignmentLeft];
    [_pickerControl setTextFieldCornerRadius:5.0];
    [_pickerControl setDelegate:self];
    
    [_sliderControl setFrame:CGRectMake(20.0, 145.0, 280.0, 55.0)];
    [_sliderControl setTextFieldBackgroundColor:[UIColor blackColor]];
    [_sliderControl setTextFieldFontSize:16.0];
    [_sliderControl setTextFieldAlignment:NSTextAlignmentCenter];
    [_sliderControl setTextFieldCornerRadius:5.0];
    [_sliderControl setPickerDelegate:self];
    
    [_textControl setFrame:CGRectMake(20.0, 76.0, 140.0, 55.0)];
    [_textControl setValueBackgroundColor:[UIColor blackColor]];
    [_textControl setValueFontSize:16.0];
    [_textControl setValueAlignment:NSTextAlignmentLeft];
    [_textControl setValueCornerRadius:5.0];
    [_textControl setDelegate:self];
    
    [self.view addSubview:_pickerControl];
    [self.view addSubview:_textControl];
    [self.view addSubview:_sliderControl];
    
    [self setConnected:NO];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (void)pickerControlRequestedTextInputView:(PickerControl *)pickerControl
{
    if (_textInputView.superview == nil)
    {
        pickerControl.ignoreCameraListUpdates = NO;
        
        _textInputView.paramID = pickerControl.paramID;
        _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_TEXTFIELD;
        _textInputView.sendValueType = pickerControl.rcpList.sendValueType;
        _textInputView.textColor = pickerControl.textFieldTextColor;
        [_textInputView setText:pickerControl.baseValue];
        [_textInputView setDelegate:pickerControl];
        
        switch (pickerControl.rcpList.sendValueType)
        {
            case SEND_VALUE_TYPE_INT:
                [_textInputView setIntEditInfo:pickerControl.intEditInfo];
                break;
                
            case SEND_VALUE_TYPE_UINT:
                [_textInputView setUintEditInfo:pickerControl.uintEditInfo];
                break;
                
            case SEND_VALUE_TYPE_STR:
                [_textInputView setStrEditInfo:pickerControl.strEditInfo];
                break;
                
            default:
                break;
        }
        
        [_textInputView showInView:self.view];
    }
}

- (void)textControlRequestedTextInputView:(TextControl *)textControl
{
    if (_textInputView.superview == nil)
    {
        _textInputView.paramID = textControl.paramID;
        _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_TEXTFIELD;
        _textInputView.sendValueType = textControl.sendValueType;
        _textInputView.textColor = textControl.valueTextColor;
        [_textInputView setText:textControl.baseValue];
        [_textInputView setDelegate:textControl];
        
        switch (textControl.sendValueType)
        {
            case SEND_VALUE_TYPE_INT:
                _textInputView.intEditInfo = textControl.intEditInfo;
                break;
                
            case SEND_VALUE_TYPE_UINT:
                _textInputView.uintEditInfo = textControl.uintEditInfo;
                break;
                
            case SEND_VALUE_TYPE_STR:
                _textInputView.strEditInfo = textControl.strEditInfo;
                break;
                
            default:
                break;
        }
        
        [_textInputView showInView:self.view];
    }
}

- (IBAction)markFramePressed:(UIButton *)sender
{
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpSetString:RCP_PARAM_KEYACTION value:[NSString stringWithFormat:@"%d", KEY_ACTION_MARK_SNAPSHOT]];
}

- (IBAction)recordPressed:(UIButton *)sender
{
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpSetInt:RCP_PARAM_RECORD_STATE value:SET_RECORD_STATE_TOGGLE];
}

@end
