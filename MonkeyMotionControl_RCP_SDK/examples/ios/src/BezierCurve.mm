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

#include "BezierCurve.h"

BezierCurve::BezierCurve(unsigned int num_points, float bound, float width) :
    data_points(num_points),
    control_points(num_points),
    division_points(2 * (num_points - 1)),
    m_points(num_points),
    m_bound(bound),
    m_width(width)
{
    reset();
}

void BezierCurve::reset() {
    float step = 1.0 / (float)(m_points - 1);
    float cur_stop = 0.0;
    for(unsigned int i = 0; i < m_points; ++i) {
        data_points[i] = point2f(cur_stop * m_bound,cur_stop * m_bound);
        control_points[i] = point2f(cur_stop * m_bound,cur_stop * m_bound);
        cur_stop += step;
    }    
	for(unsigned int i = 0; i < 2 * (m_points - 1); ++i)
	{
		division_points[i] = point2f(0,0); 
	}
}


void BezierCurve::update() {
    calcDataPoints();
    calcDivisionPoints();
}

void BezierCurve::setLineWidth(float width) {
    m_width = width;
}

void BezierCurve::setControlPoints(const point_vec_t &points) {
    for(unsigned int i = 0; i < m_points; ++i) {
        control_points[i] = points[i];
    }
    update();
}

void BezierCurve::setControlPointsX(const std::vector<float> &values) {
    unsigned int size = std::min(values.size(), control_points.size());
    for(unsigned int i = 0; i < size; ++i) {
        control_points[i].x1 = values[i];
    }
    update();
}

void BezierCurve::setControlPointsY(const std::vector<float> &values) {
    unsigned int size = std::min(values.size(), control_points.size());
    for(unsigned int i = 0; i < size; ++i) {
        control_points[i].x2 = values[i];
    }
    update();    
}

void BezierCurve::setControlPointX(int index, float value) {
    control_points[index].x1 = value;
    update();
}

void BezierCurve::setControlPointY(int index, float value) {
    control_points[index].x2 = value;
    update();
}

void BezierCurve::setControlPoint(int index, const point2f &point) {
    control_points[index] = point;
    update();
}

void BezierCurve::getControlPoints(point2f * points, unsigned int num_points) const {
    unsigned int size = std::min(m_points, num_points);
    for(unsigned int i = 0; i < size; ++i) {
        points[i] = control_points[i];
    }
}

void BezierCurve::getControlPoints(point_vec_t &points) const {
    points.resize(m_points);    
    for(unsigned int i = 0; i < m_points; ++i) {
        points[i] = control_points[i];
    }    
}

float BezierCurve::getControlPointX(int index) const {
    return control_points[index].x1;
}

float BezierCurve::getControlPointY(int index) const {
    return control_points[index].x2;
}

point2f BezierCurve::getControlPoint(int index) const {
    return control_points[index];
}


float BezierCurve::getDistanceToPoint(int index, const point2f &pos) const {
    float dx = pos.x1 - control_points[index].x1;
    float dy = pos.x2 - control_points[index].x2;
    return sqrt(dx * dx + dy * dy);
}

int BezierCurve::getClosestPointIndex(const point2f &pos, float pointSize) const {
    int point = -1;
    float distance = -1.0;
    for(unsigned int i = 0; i < m_points; ++i) {
        float dx = pos.x1 - control_points[i].x1;
        float dy = pos.x2 - control_points[i].x2;
        float d = sqrt(dx * dx + dy * dy);
        if((distance < 0.0 && d < pointSize) || d < distance) {
            distance = d;
            point = i;
        }
    }    
    return point;
}

bool BezierCurve::isCloseToPoint(const point2f &pos, float threshold) const {
    for(unsigned int i = 0; i < m_points; ++i) {
        float d = getDistanceToPoint(i,pos);
        if( d <= threshold ) 
            return true;
    }
    return false;
}

void BezierCurve::paintLine(CGContextRef context, const vec3f &color, float alpha, curve_color_t lineColor) const {
    UIBezierPath * path = [UIBezierPath bezierPath];
    [path setLineWidth:m_width];
    
    switch (lineColor)
    {
        case CURVE_COLOR_WHITE:
            CGContextSetStrokeColorWithColor(context, [UIColor whiteColor].CGColor);
            break;
            
        case CURVE_COLOR_RED:
            CGContextSetStrokeColorWithColor(context, [UIColor redColor].CGColor);
            break;
            
        case CURVE_COLOR_GREEN:
            CGContextSetStrokeColorWithColor(context, [UIColor greenColor].CGColor);
            break;
            
        case CURVE_COLOR_BLUE:
            CGContextSetStrokeColorWithColor(context, [UIColor blueColor].CGColor);
            break;
            
        default:
            CGContextSetStrokeColorWithColor(context, [UIColor whiteColor].CGColor); 
            break;
    }
    
    [path moveToPoint:CGPointMake(0.0, control_points[0].x2)];
    [path addLineToPoint:CGPointMake(control_points[0].x1, control_points[0].x2)];
    [path moveToPoint:CGPointMake(m_bound, control_points[4].x2)];
    [path addLineToPoint:CGPointMake(control_points[4].x1, control_points[4].x2)];    
    [path moveToPoint:CGPointMake(data_points[0].x1, data_points[0].x2)];
    for(unsigned int i = 0; i < (m_points - 1); ++i) {
        [path addCurveToPoint:CGPointMake(data_points[i+1].x1, data_points[i+1].x2) 
                controlPoint1:CGPointMake(division_points[i*2].x1, division_points[i*2].x2) 
                controlPoint2:CGPointMake(division_points[i*2+1].x1, division_points[i*2+1].x2)];
    }
    [path strokeWithBlendMode:kCGBlendModeNormal alpha:alpha];
}

void BezierCurve::paintControlPoints(CGContextRef context, float pointSize) const {      
    CGContextSetStrokeColorWithColor(context, [UIColor colorWithRed:0.13 green:0.13 blue:0.13 alpha:1.0].CGColor);    
    CGContextSetFillColorWithColor(context, [UIColor colorWithRed:1.0 green:0.5 blue:0.0 alpha:1.0].CGColor);    
    for(unsigned int i = 0; i < m_points; ++i) {
        UIBezierPath * path = [UIBezierPath bezierPathWithOvalInRect:CGRectMake(
                                    control_points[i].x1 - pointSize,
                                    control_points[i].x2 - pointSize,
                                    pointSize * 2.0, 
                                    pointSize * 2.0)];
        [path setLineWidth:0.2f];
        [path fill];
        [path stroke];
    }    
}

void BezierCurve::calcDivisionPoints() {
    for(unsigned int i = 0; i < (m_points - 1); ++i) {
        division_points[i*2+0] = control_points[i] * 0.666667 + control_points[i+1] * 0.333333;
        division_points[i*2+1] = control_points[i] * 0.333333 + control_points[i+1] * 0.666667;
    }    
}

void BezierCurve::calcDataPoints() {
    point2f t1 = control_points[1];
    point2f t2 = control_points[2];
    point2f t3 = control_points[3];

    vec3f tx(t1.x1,t2.x1,t3.x1);
    vec3f ty(t1.x2,t2.x2,t3.x2);

    vec3f r1(4.0f, 1.0f, 0.0f);
    vec3f r2(1.0f, 4.0f, 1.0f);
    vec3f r3(0.0f, 1.0f, 4.0f);

    vec3f dpx(dot(r1,tx),dot(r2,tx),dot(r3,tx));
    vec3f dpy(dot(r1,ty),dot(r2,ty),dot(r3,ty));

    data_points[0] = control_points[0];
    data_points[4] = control_points[4];
    data_points[1] = (point2f(dpx.x1,dpy.x1) + data_points[0]) / 6.0;
    data_points[2] = point2f(dpx.x2,dpy.x2) / 6.0;
    data_points[3] = (point2f(dpx.x3,dpy.x3) + data_points[4]) / 6.0;    
}
