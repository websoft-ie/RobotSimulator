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


#ifndef _BEZIER_CURVE_H_
#define _BEZIER_CURVE_H_

#include <vector>
#include "vec3.h"
#include "point2.h"

typedef enum
{
    CURVE_COLOR_WHITE,
    CURVE_COLOR_RED,
    CURVE_COLOR_GREEN,
    CURVE_COLOR_BLUE
} curve_color_t;

typedef std::vector<point2f> point_vec_t;

class BezierCurve {
        
    public:
        
        BezierCurve(unsigned int num_points = 5, float bound = 100.0, float width = 1.0);
        
        void setLineWidth(float width);
    
        void setControlPoints(const point_vec_t &points);
        void setControlPointsX(const std::vector<float> &values);
        void setControlPointsY(const std::vector<float> &values);
        void setControlPointX(int index, float value);
        void setControlPointY(int index, float value);
        void setControlPoint(int index, const point2f &point);
        
        void getControlPoints(point2f * points, unsigned int num_points) const;
        void getControlPoints(point_vec_t &points) const;
        float getControlPointX(int index) const;
        float getControlPointY(int index) const;
        point2f getControlPoint(int index) const;
        
        float getDistanceToPoint(int index, const point2f &pos) const;
        
        int getClosestPointIndex(const point2f &pos, float pointSize) const;
        
        bool isCloseToPoint(const point2f &pos, float threshold) const;
        
        void reset();
        
        void paintLine(CGContextRef context, const vec3f &color, float alpha, curve_color_t lineColor) const;
        void paintControlPoints(CGContextRef context, float pointSize) const;
        
        void update();
        
    private:
    
        void calcDivisionPoints();
        void calcDataPoints();
        
        point_vec_t data_points;        
        point_vec_t control_points;
        point_vec_t division_points;
                
        unsigned int m_points;
        float m_bound;
        float m_width;
};

#endif 
