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

#import "CurveGraphView.h"
#import "BezierCurve.h"
#import "Logger.h"
#import "RCPUtil.h"

#define POINT_SIZE 2.0

#define BOUND 100.0

#define SELECTED_CURVE_WIDTH 1.0
#define DESELECTED_CURVE_WIDTH 0.3

#define SELECTED_CURVE_ALPHA 1.0
#define DESELECTED_CURVE_ALPHA 0.3

@interface CurveGraphView()

@property (nonatomic) BezierCurve *lumaCurve;
@property (nonatomic) BezierCurve *redCurve;
@property (nonatomic) BezierCurve *greenCurve;
@property (nonatomic) BezierCurve *blueCurve;
@property (nonatomic, assign) rcp_param_t selectedCurve;

@property (nonatomic, assign) BOOL enabled;
@property (nonatomic, assign) BOOL drawCurves;
@property (nonatomic, assign) BOOL touchDown;
@property (nonatomic, assign) BOOL touchDrag;
@property (nonatomic, assign) point2f touchLocation;
@property (nonatomic, assign) point_type_t selectedPoint;


- (void)initialize;
- (void)checkBounds:(const point2f &)pos activePoint:(int)n withDir:(int)dir;
- (void)drawRect:(CGRect)rect;
- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event;
- (void)touchesEnded:(NSSet *)touches withEvent:(UIEvent *)event;
- (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event;
- (void)enableGraphUpdates;

@end


@implementation CurveGraphView

@synthesize delegate, ignoreUpdates;

- (CurveGraphView *)init
{
    if (self = [super init])
    {
        [self initialize];
    }
    
    return self;
}

- (void)resetCurves
{
    _lumaCurve->reset();
    _redCurve->reset();
    _greenCurve->reset();
    _blueCurve->reset();
    
    [self setNeedsDisplay];
}

- (void)setSelectedCurve:(rcp_param_t)curveID
{
    switch (curveID)
    {
        case RCP_PARAM_LUMA_CURVE:
            _selectedCurve = curveID;
            _lumaCurve->setLineWidth(SELECTED_CURVE_WIDTH);
            _redCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _greenCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _blueCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            break;
            
        case RCP_PARAM_RED_CURVE:
            _selectedCurve = curveID;
            _lumaCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _redCurve->setLineWidth(SELECTED_CURVE_WIDTH);
            _greenCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _blueCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            break;
            
        case RCP_PARAM_GREEN_CURVE:
            _selectedCurve = curveID;
            _lumaCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _redCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _greenCurve->setLineWidth(SELECTED_CURVE_WIDTH);
            _blueCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            break;
            
        case RCP_PARAM_BLUE_CURVE:
            _selectedCurve = curveID;
            _lumaCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _redCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _greenCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _blueCurve->setLineWidth(SELECTED_CURVE_WIDTH);
            break;
            
        default:
            _selectedCurve = RCP_PARAM_LUMA_CURVE;
            _lumaCurve->setLineWidth(SELECTED_CURVE_WIDTH);
            _redCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _greenCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            _blueCurve->setLineWidth(DESELECTED_CURVE_WIDTH);
            break;
    }
    
    [self setNeedsDisplay];
}

- (void)setPointForCurve:(rcp_param_t)curveID pointID:(point_type_t)pointID point:(CGPoint)point
{
    if (_touchDrag)
    {
        return;
    }
    
    switch (curveID)
    {
        case RCP_PARAM_LUMA_CURVE:
            _lumaCurve->setControlPointX(pointID, point.x);
            _lumaCurve->setControlPointY(pointID, point.y);
            break;
            
        case RCP_PARAM_RED_CURVE:
            _redCurve->setControlPointX(pointID, point.x);
            _redCurve->setControlPointY(pointID, point.y);
            break;
            
        case RCP_PARAM_GREEN_CURVE:
            _greenCurve->setControlPointX(pointID, point.x);
            _greenCurve->setControlPointY(pointID, point.y);
            break;
            
        case RCP_PARAM_BLUE_CURVE:
            _blueCurve->setControlPointX(pointID, point.x);
            _blueCurve->setControlPointY(pointID, point.y);
            break;
            
        default:
            break;
    }
    
    [self setNeedsDisplay];
}

- (void)drawCurves:(BOOL)draw
{
    _drawCurves = draw;
    [self setNeedsDisplay];
}

- (void)setEnabled:(BOOL)enabled
{
    _enabled = enabled;
    
    [self setUserInteractionEnabled:enabled];
    [self setNeedsDisplay];
}

- (void)initialize
{
    ignoreUpdates = NO;
    
    _lumaCurve = new BezierCurve(5, BOUND, SELECTED_CURVE_WIDTH);
    _redCurve = new BezierCurve(5, BOUND, DESELECTED_CURVE_WIDTH);
    _greenCurve = new BezierCurve(5, BOUND, DESELECTED_CURVE_WIDTH);
    _blueCurve = new BezierCurve(5, BOUND, DESELECTED_CURVE_WIDTH);
    _selectedCurve = RCP_PARAM_LUMA_CURVE;
    
    _enabled = NO;
    _drawCurves = NO;
    _touchDown = NO;
    _touchDrag = NO;
    _touchLocation = point2f(0.0, 0.0);
    _selectedPoint = POINT_NONE;
}

- (void)checkBounds:(const point2f &)pos activePoint:(int)n withDir:(int)dir
{
    BezierCurve *curve = NULL;
    
    switch (_selectedCurve)
    {
        case RCP_PARAM_LUMA_CURVE:
            curve = _lumaCurve;
            break;
            
        case RCP_PARAM_RED_CURVE:
            curve = _redCurve;
            break;
            
        case RCP_PARAM_GREEN_CURVE:
            curve = _greenCurve;
            break;
            
        case RCP_PARAM_BLUE_CURVE:
            curve = _blueCurve;
            break;
            
        default:
            return;
    }
    
    float x = pos.x1;
    float y = pos.x2;
    
    // First check extreme bound conditions
    if (x < 0.0)
    {
        x = 0.0;
    }
    else if (x > BOUND)
    {
        x = BOUND;
    }
    
    if (y < 0.0)
    {
        y = 0.0;
    }
    else if (y > BOUND)
    {
        y = BOUND;
    }
    
    // Now get positions of neighboring points for comparison
    if (n > 0 && dir <= 0)
    {
        point2f p = curve->getControlPoint(n - 1);
        
        if (x <= p.x1 && p.x1 > 0.0)
        {
            p.x1 = x - 0.01;
            curve->setControlPoint(n - 1, p);
            [self checkBounds:p activePoint:n - 1 withDir:-1];
        }
    }
    
    if (n < 4 && dir >= 0)
    {
        point2f p = curve->getControlPoint(n + 1);
    
        if (x >= p.x1 && p.x1 < BOUND)
        {
            p.x1 = x + 0.01;
            curve->setControlPoint(n + 1, p);
            [self checkBounds:p activePoint:n + 1 withDir:1];
        }
    }
    
    if (n == 4)
    {
        if (x > BOUND)
        {
            x = BOUND;
        }
    }
    
    curve->setControlPoint(n, point2f(x, y));
}

- (void)drawRect:(CGRect)rect
{
    if (_drawCurves)
    {
        CGContextRef context = UIGraphicsGetCurrentContext();
        CGRect frame = self.bounds;
        const CGFloat selectedAlpha = _enabled ? SELECTED_CURVE_ALPHA : (SELECTED_CURVE_ALPHA / 2.0);
        
        CGContextSetLineWidth(context, 1);
        CGRectInset(frame, 10, 10);
        
        // Scale and translate coordinate system
        CGContextScaleCTM(context, [self bounds].size.width / BOUND, -[self bounds].size.height / BOUND);
        CGContextTranslateCTM(context, 0.0, -BOUND);
        
        switch (_selectedCurve)
        {
            case RCP_PARAM_LUMA_CURVE:
                _lumaCurve->paintLine(context, vec3f(1.0, 1.0, 1.0), selectedAlpha, CURVE_COLOR_WHITE);
                _lumaCurve->paintControlPoints(context, POINT_SIZE);
                break;
                
            case RCP_PARAM_RED_CURVE:
                _redCurve->paintLine(context, vec3f(1.0, 1.0, 1.0), selectedAlpha, CURVE_COLOR_RED);
                _redCurve->paintControlPoints(context, POINT_SIZE);
                break;
                
            case RCP_PARAM_GREEN_CURVE:
                _greenCurve->paintLine(context, vec3f(1.0, 1.0, 1.0), selectedAlpha, CURVE_COLOR_GREEN);
                _greenCurve->paintControlPoints(context, POINT_SIZE);
                break;
                
            case RCP_PARAM_BLUE_CURVE:
                _blueCurve->paintLine(context, vec3f(1.0, 1.0, 1.0), selectedAlpha, CURVE_COLOR_BLUE);
                _blueCurve->paintControlPoints(context, POINT_SIZE);
                break;
                
            default:
                return;
        }
    }
}

- (void)touchesBegan:(NSSet *)touches withEvent:(UIEvent *)event
{
    BezierCurve *curve = NULL;
    
    switch (_selectedCurve)
    {
        case RCP_PARAM_LUMA_CURVE:
            curve = _lumaCurve;
            break;
            
        case RCP_PARAM_RED_CURVE:
            curve = _redCurve;
            break;
            
        case RCP_PARAM_GREEN_CURVE:
            curve = _greenCurve;
            break;
            
        case RCP_PARAM_BLUE_CURVE:
            curve = _blueCurve;
            break;
            
        default:
            return;
    }
    
    ignoreUpdates = YES;
    
    CGPoint pt = [[touches anyObject] locationInView:self];
    
    pt.x = pt.x * (BOUND / [self bounds].size.width);
    pt.y = pt.y * (BOUND / [self bounds].size.height);
    pt.y = BOUND - pt.y;
    
    _touchLocation = point2f(pt.x, pt.y);
    _touchDown = YES;
    
    _selectedPoint = (point_type_t)curve->getClosestPointIndex(_touchLocation, 4.0 * POINT_SIZE);
}

- (void)touchesEnded:(NSSet *)touches withEvent:(UIEvent *)event
{
    _selectedPoint = POINT_NONE;
    _touchDrag = NO;
    _touchDown = NO;
    
    [NSTimer scheduledTimerWithTimeInterval:SLIDER_UPDATE_DELAY_S target:self selector:@selector(enableGraphUpdates) userInfo:nil repeats:NO];
}

- (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event
{
    ignoreUpdates = YES;
    
    CGPoint pt = [[touches anyObject] locationInView:self];
    
    pt.x = pt.x * (BOUND / [self bounds].size.width);
    pt.y = pt.y * (BOUND / [self bounds].size.height);
    pt.y = BOUND - pt.y;
    
    point2f touchMove(pt.x, pt.y);
    float manLength = (_touchLocation - touchMove).manhattanLength();
    
    if (_touchDown && !_touchDrag && manLength > 0.3)
    {
        _touchDrag = YES;
    }
    
    if (_touchDrag && _selectedPoint >= 0 && _selectedPoint < 5)
    {
        [self checkBounds:touchMove activePoint:_selectedPoint withDir:0];
        [self setNeedsDisplay];
        
        if (delegate != nil)
        {
            if ([delegate respondsToSelector:@selector(curveGraphView:didUpdatePoint:withValue:)])
            {
                [delegate curveGraphView:self didUpdatePoint:_selectedPoint withValue:CGPointMake(pt.x, pt.y)];
            }
        }
    }
}

- (void)enableGraphUpdates
{
    ignoreUpdates = NO;
}

@end
