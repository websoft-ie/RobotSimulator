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

#ifndef __POINT2_H__
#define __POINT2_H__

template <typename T>
class point2_t {
    
    public:
        point2_t() : x1(T()), x2(T()) {}
        point2_t(T x, T y) : x1(x), x2(y) {}
        point2_t(const point2_t &p) : x1(p.x1), x2(p.x2) {}
        point2_t & operator = (const point2_t &p) {
            if(&p != this) {
                x1 = p.x1;
                x2 = p.x2;
            }
            return *this;
        }
        const point2_t operator * (float r) const {
            return point2_t(x1 * r, x2 * r);
        }
        const point2_t operator / (float r) const {
            return point2_t(x1 / r, x2 / r);
        }
    
        T manhattanLength() const {
            return (x1 * x1 + x2 * x2);   
        }
    
        union {
            struct {
                T x1;
                T x2;
            };
            T t[2];
        };
};

template <typename T>
inline const point2_t<T> operator + (const point2_t<T> &p1, const point2_t<T> &p2) {
    return point2_t<T>(p1.x1 + p2.x1, p1.x2 + p2.x2);
}

template <typename T>
inline const point2_t<T> operator - (const point2_t<T> &p1, const point2_t<T> &p2) {
    return point2_t<T>(p1.x1 - p2.x1, p1.x2 - p2.x2);
}

typedef point2_t<float> point2f;
typedef point2_t<double> point2d;
typedef point2_t<long double> point2ld;

#endif
