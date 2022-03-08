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

#import "CurveView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

static const char *default_curve = "0,0,250,250,500,500,750,750,1000,1000";
static const float min_value = 0.0;
static const float max_value = 1000.0;

@interface CurveView()

@property (assign, nonatomic) rcp_param_t paramID;
@property (assign, nonatomic) CGRect containerFrame;

@property (retain, nonatomic) UIView *container;
@property (retain, nonatomic) CurveGraphView *curveGraph;
@property (retain, nonatomic) UILabel *xLabel;
@property (retain, nonatomic) UILabel *yLabel;
@property (retain, nonatomic) UIButton *resetButton;
@property (retain, nonatomic) UIButton *doneButton;

@property (assign, nonatomic) int shadowX;
@property (assign, nonatomic) int shadowY;
@property (assign, nonatomic) int darkX;
@property (assign, nonatomic) int darkY;
@property (assign, nonatomic) int midtoneX;
@property (assign, nonatomic) int midtoneY;
@property (assign, nonatomic) int lightX;
@property (assign, nonatomic) int lightY;
@property (assign, nonatomic) int highlightX;
@property (assign, nonatomic) int highlightY;


- (void)buttonUpInside:(UIButton *)sender;
- (void)curveGraphView:(CurveGraphView *)curveGraphView didUpdatePoint:(point_type_t)pointID withValue:(CGPoint)value;
- (void)sendCurve:(BOOL)decreasing;

@end


@implementation CurveView

@synthesize delegate;

- (CurveView *)init
{
    if (self = [super init])
    {
        delegate = nil;
        
        _paramID = RCP_PARAM_COUNT;
        _containerFrame = CGRectZero;
        
        _curveGraph = [[CurveGraphView alloc] init];
        _xLabel = [[UILabel alloc] init];
        _yLabel = [[UILabel alloc] init];
        _resetButton = [[UIButton alloc] init];
        
        [_curveGraph setDelegate:self];
        [_curveGraph setBackgroundColor:[UIColor colorWithRed:18.0/255.0 green:18.0/255.0 blue:18.0/255.0 alpha:1.0]];
        
        [_xLabel setTextColor:[UIColor whiteColor]];
        [_xLabel setTextAlignment:NSTextAlignmentCenter];
        [_xLabel setText:@"X"];
        
        [_yLabel setTextColor:[UIColor whiteColor]];
        [_yLabel setTextAlignment:NSTextAlignmentCenter];
        [_yLabel setText:@"Y"];
        
        [_resetButton.titleLabel setFont:[UIFont systemFontOfSize:14.0]];
        [_resetButton setTitleColor:[UIColor lightGrayColor] forState:UIControlStateHighlighted];
        [_resetButton setTitle:@"RESET" forState:UIControlStateNormal];
        [_resetButton addTarget:self action:@selector(buttonUpInside:) forControlEvents:UIControlEventTouchUpInside];
        
        
        _container = [[UIView alloc] init];
        [_container setBackgroundColor:[UIColor colorWithRed:0.1 green:0.1 blue:0.1 alpha:1.0]];
        
        _doneButton = [[UIButton alloc] init];
        [_doneButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
        [_doneButton setTitleColor:[UIColor lightGrayColor] forState:UIControlStateHighlighted];
        [_doneButton setBackgroundColor:[UIColor colorWithRed:0.2 green:0.2 blue:0.2 alpha:1.0]];
        [_doneButton setTitle:@"Done" forState:UIControlStateNormal];
        [_doneButton addTarget:self action:@selector(buttonUpInside:) forControlEvents:UIControlEventTouchUpInside];
        
        [self setBackgroundColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.4]];
        
        [self addSubview:_container];
        [_container addSubview:_curveGraph];
        [_container addSubview:_xLabel];
        [_container addSubview:_yLabel];
        [_container addSubview:_resetButton];
        [_container addSubview:_doneButton];
        
        _shadowX = 0;
        _shadowY = 0;
        _darkX = 0;
        _darkY = 0;
        _midtoneX = 0;
        _midtoneY = 0;
        _lightX = 0;
        _lightY = 0;
        _highlightX = 0;
        _highlightY = 0;
    }
    
    return self;
}

- (rcp_param_t)paramID
{
    return _paramID;
}

- (void)setParam:(rcp_param_t)param
{
    _paramID = param;
    
    switch (_paramID)
    {
        case RCP_PARAM_LUMA_CURVE:
            [_resetButton setBackgroundColor:[UIColor darkGrayColor]];
            [_resetButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
            break;
            
        case RCP_PARAM_RED_CURVE:
            [_resetButton setBackgroundColor:[UIColor redColor]];
            [_resetButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
            break;
            
        case RCP_PARAM_GREEN_CURVE:
            [_resetButton setBackgroundColor:[UIColor greenColor]];
            [_resetButton setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
            break;
            
        case RCP_PARAM_BLUE_CURVE:
            [_resetButton setBackgroundColor:[UIColor blueColor]];
            [_resetButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
            break;
            
        default:
            [_resetButton setBackgroundColor:[UIColor darkGrayColor]];
            [_resetButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
            break;
    }
    
    [_curveGraph setSelectedCurve:_paramID];
}

- (void)setCurve:(const char *)curve status:(rcp_param_status_t)status param:(rcp_param_t)paramID
{
    NSArray *values = [[NSString stringWithFormat:@"%s", curve] componentsSeparatedByString:@","];
    
    if ([values count] > 9)
    {
        _shadowX = [[values objectAtIndex:0] intValue];
        _shadowY = [[values objectAtIndex:1] intValue];
        _darkX = [[values objectAtIndex:2] intValue];
        _darkY = [[values objectAtIndex:3] intValue];
        _midtoneX = [[values objectAtIndex:4] intValue];
        _midtoneY = [[values objectAtIndex:5] intValue];
        _lightX = [[values objectAtIndex:6] intValue];
        _lightY = [[values objectAtIndex:7] intValue];
        _highlightX = [[values objectAtIndex:8] intValue];
        _highlightY = [[values objectAtIndex:9] intValue];
        
        if (!_curveGraph.ignoreUpdates)
        {
            const float graphDivider = 10.0;
            
            [_curveGraph setPointForCurve:paramID pointID:POINT_SHADOW point:CGPointMake(_shadowX / graphDivider, _shadowY / graphDivider)];
            [_curveGraph setPointForCurve:paramID pointID:POINT_DARK point:CGPointMake(_darkX / graphDivider, _darkY / graphDivider)];
            [_curveGraph setPointForCurve:paramID pointID:POINT_MIDTONE point:CGPointMake(_midtoneX / graphDivider, _midtoneY / graphDivider)];
            [_curveGraph setPointForCurve:paramID pointID:POINT_LIGHT point:CGPointMake(_lightX / graphDivider, _lightY / graphDivider)];
            [_curveGraph setPointForCurve:paramID pointID:POINT_HIGHLIGHT point:CGPointMake(_highlightX / graphDivider, _highlightY / graphDivider)];
        }
    }
}

- (void)setFrame:(CGRect)frame
{
    const CGFloat verticalSpacing = 20.0;
    
    const CGFloat labelSize = 20.0;
    
    const CGFloat resetButtonWidth = 110.0;
    const CGFloat resetButtonHeight = 40.0;
    
    CGFloat graphSize;
    CGFloat containerWidth;
    CGFloat containerHeight;
    CGFloat doneButtonHeight;
    
    
    [super setFrame:frame];

    graphSize = 250.0;
    
    doneButtonHeight = 45.0;
    
    containerWidth = labelSize + graphSize + labelSize;
    containerHeight = (labelSize / 2.0) + graphSize + labelSize + verticalSpacing + resetButtonHeight + verticalSpacing + doneButtonHeight;
    
    
    [_container setFrame:CGRectMake((frame.size.width - containerWidth) / 2.0, (frame.size.height - containerHeight) / 2.0, containerWidth, containerHeight)];
    
    [_yLabel setFrame:CGRectMake(labelSize / 2.0, labelSize / 2.0, labelSize, graphSize)];
    [_xLabel setFrame:CGRectMake(_yLabel.frame.origin.x + labelSize, _yLabel.frame.origin.y + graphSize, graphSize, labelSize)];
    [_curveGraph setFrame:CGRectMake(_xLabel.frame.origin.x, _yLabel.frame.origin.y, graphSize, graphSize)];
    
    [_resetButton setFrame:CGRectMake(_curveGraph.frame.origin.x + ((graphSize - resetButtonWidth) / 2.0), _xLabel.frame.origin.y + labelSize + verticalSpacing, resetButtonWidth, resetButtonHeight)];
    
    [_doneButton setFrame:CGRectMake(0.0, _resetButton.frame.origin.y + resetButtonHeight + verticalSpacing, containerWidth, doneButtonHeight)];
}

- (void)showCurves:(BOOL)show
{
    [_curveGraph drawCurves:show];
}

- (void)setEnabled:(BOOL)enabled
{
    const float alpha = enabled ? 1.0 : 0.5;
    
    [_curveGraph setAlpha:alpha];
    [_xLabel setAlpha:alpha];
    [_yLabel setAlpha:alpha];
    [_resetButton setAlpha:alpha];
    
    [_curveGraph setEnabled:enabled];
    [_xLabel setEnabled:enabled];
    [_yLabel setEnabled:enabled];
    [_resetButton setEnabled:enabled];
    
    [self setUserInteractionEnabled:enabled];
}

- (void)buttonUpInside:(UIButton *)sender
{
    if (sender == _resetButton)
    {
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpSetString:_paramID value:[NSString stringWithFormat:@"%s", default_curve]];
    }
    else if (sender == _doneButton)
    {
        if (delegate != nil)
        {
            [delegate curveViewFinishedEditing:self];
        }
    }
}

- (void)curveGraphView:(CurveGraphView *)curveGraphView didUpdatePoint:(point_type_t)pointID withValue:(CGPoint)value
{
    const float graphMultiplier = 10.0;
    BOOL decreasing = NO;
    
    value = CGPointMake(value.x * graphMultiplier, value.y * graphMultiplier);
    
    if (value.x < min_value)
    {
        value.x = min_value;
    }
    else if (value.x > max_value)
    {
        value.x = max_value;
    }
    
    if (value.y < min_value)
    {
        value.y = min_value;
    }
    else if (value.y > max_value)
    {
        value.y = max_value;
    }
    
    switch (pointID)
    {
        case POINT_SHADOW:
            decreasing = value.x < _shadowX;
            _shadowX = value.x;
            _shadowY = value.y;
            break;
            
        case POINT_DARK:
            decreasing = value.x < _darkX;
            _darkX = value.x;
            _darkY = value.y;
            break;
            
        case POINT_MIDTONE:
            decreasing = value.x < _midtoneX;
            _midtoneX = value.x;
            _midtoneY = value.y;
            break;
            
        case POINT_LIGHT:
            decreasing = value.x < _lightX;
            _lightX = value.x;
            _lightY = value.y;
            break;
            
        case POINT_HIGHLIGHT:
            decreasing = value.x < _highlightX;
            _highlightX = value.x;
            _highlightY = value.y;
            break;
            
        default:
            return;
    }
    
    [self sendCurve:decreasing];
}

- (void)sendCurve:(BOOL)decreasing
{
    RCPHandler *handler = [RCPHandler instance];
    NSMutableString *curve = [[NSMutableString alloc] init];
    
    if (decreasing)
    {
        if (_lightX > _highlightX)
        {
            _lightX = _highlightX;
        }
        
        if (_midtoneX > _lightX)
        {
            _midtoneX = _lightX;
        }
        
        if (_darkX > _midtoneX)
        {
            _darkX = _midtoneX;
        }
        
        if (_shadowX > _darkX)
        {
            _shadowX = _darkX;
        }
    }
    
    [curve appendFormat:@"%d,", _shadowX];
    [curve appendFormat:@"%d,", _shadowY];
    [curve appendFormat:@"%d,", _darkX];
    [curve appendFormat:@"%d,", _darkY];
    [curve appendFormat:@"%d,", _midtoneX];
    [curve appendFormat:@"%d,", _midtoneY];
    [curve appendFormat:@"%d,", _lightX];
    [curve appendFormat:@"%d,", _lightY];
    [curve appendFormat:@"%d,", _highlightX];
    [curve appendFormat:@"%d", _highlightY];
    
    [handler rcpSetString:_paramID value:curve];
}

@end
