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


#ifndef _VECTOR_H_
#define _VECTOR_H_

#include <cmath>
#include <limits>

template <typename T>
class vec3_t {
    
    public:
                
        vec3_t() : x1(T()), x2(T()), x3(T()) { 
        }

        vec3_t(T x,T y,T z) : x1(x), x2(y), x3(z) { 
        }

        vec3_t(const vec3_t &v) : x1(v.x1), x2(v.x2), x3(v.x3) { 
        }

        vec3_t(const T v[3]) : x1(v[0]), x2(v[1]), x3(v[2]) {
        }
        
        operator const T * const (void) const { 
            return t;
        }

        T const & operator [] (int i) const {
            return t[i];
        }

        T & operator [] (int i) { 
            return t[i];
        }
        
        vec3_t & operator = (const vec3_t &v) {
            if(&v != this) {
                x1 = v.x1;
                x2 = v.x2;
                x3 = v.x3;
            }
            return *this;
        };
 
        bool operator == (const vec3_t & v) const {
            return (x1 == v.x1 && x2 == v.x2 && x3 == v.x3);   
        }

        bool operator != (const vec3_t & v) const {
            return (x1 != v.x1 && x2 != v.x2 && x3 != v.x3);
        }

        vec3_t operator * (T s) const {
            return vec3_t(x1 * s,x2 * s,x3 * s);   
        }
        
        vec3_t operator / (T s) const {
            return vec3_t(x1 / s,x2 / s,x3 / s);
        }
        
        vec3_t operator + (const vec3_t &v) const {
            return vec3_t(x1 + v.x1,x2 + v.x2,x3 + v.x3);
        }

        vec3_t operator - (const vec3_t &v) const {
            return vec3_t(x1 - v.x1,x2 - v.x2,x3 - v.x3);
        }
        
        vec3_t & operator += (const vec3_t &v) {
            *this = *this + v;
            return *this;
        }

        vec3_t & operator -= (const vec3_t &v) {
            *this = *this - v;
            return *this;
        }

        vec3_t & operator *= (T s) {
            *this = *this * s;
            return *this;
        }
        
        void clear() {
            x1 = x2 = x3 = T();
        }

        void normalize() {
            T len = length();
            if(len == 0.0)
                len = 1.0;
            *this *= (1.0 / len);
        }

        T length() const {
            return sqrt(sqrdlength());
        }

        T sqrdlength() const {
            return (x1 * x1 + x2 * x2 + x3 * x3);    
        }

        static const vec3_t zero;

        union {
            struct {
                T x1;
                T x2;
                T x3;
            };
            T t[3];
        };
};
/*
template <>
const vec3_t<float> vec3_t<float>::zero(0.0f,0.0f,0.0f);

template <>
const vec3_t<double> vec3_t<double>::zero(0.0,0.0,0.0);
*/
template <typename T>
inline vec3_t<T> cross(const vec3_t<T> &v1,const vec3_t<T> &v2) {
    return vec3_t<T>(
        v1[1] * v2[2] - v1[2] * v2[1],
        v1[2] * v2[0] - v1[0] * v2[2],
        v1[0] * v2[1] - v1[1] * v2[0]
    );
}

template <typename T>
inline T dot(const vec3_t<T> &v1,const vec3_t<T> &v2) {
    return (v1.x1 * v2.x1 + v1.x2 * v2.x2 + v1.x3 * v2.x3);
}

template <typename T>
inline T deg_to_rad(T d) {
    return d * (M_PI / 180.0);
}

template <typename T>
inline T rad_to_deg(T r) {
    return r * (180.0 / M_PI);
}

typedef vec3_t<float> vec3f;
typedef vec3_t<double> vec3d;
typedef vec3_t<long double> vec3ld;

#endif // _VECTOR_H_


