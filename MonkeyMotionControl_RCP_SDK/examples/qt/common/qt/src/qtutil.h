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

#ifndef QTUTIL_H
#define QTUTIL_H

#include <QRect>
#include <QString>

typedef struct
{
    int x;
    int y;
    int width;
    int height;
} tRect;

enum AspectRatios
{
    ASPECT_RATIO_ABSOLUTE = -3,
    ASPECT_RATIO_USER = -2,
    ASPECT_RATIO_OFF = -1,
    ASPECT_RATIO_FULL = 0
};

enum Guides
{
    GUIDE_OFF = -1,
    FRAME_GUIDE = 0,
    ACTION_GUIDE = 1,
    TITLE_GUIDE = 2,
    RECORDING_AREA = 3
};

enum Colors
{
    BLACK = 0,
    RED = 1,
    BLUE = 2,
    GREEN = 3,
    YELLOW = 4,
    MAGENTA = 5,
    CYAN = 6,
    DARK_GRAY = 7,
    WHITE = 8
};

enum LineStyles
{
    SOLID = 0,
    DASHED = 1,
    BRACKET = 2
};

/* NOTE: If this enum ever changes, update MainWindow in the RED Keymapper project */
typedef enum
{
    KEYMAPPER_CAMERA_EPIC_SCARLET,
    KEYMAPPER_CAMERA_WEAPON,
    KEYMAPPER_CAMERA_COUNT
} keymapper_camera_t;

class QtUtil
{
    public:
        static Qt::GlobalColor qtColor(int color);
        static Qt::PenStyle qtPenStyle(int lineStyle);
        static tRect tRectFromQRect(const QRect &qRect);
        static QPoint desktopCenter(const QSize &windowSize);
        static bool filenameIsValid(const QString &filename);
        static QString baseFilename(const QString &filename, bool keepExtension = false);
        static QString pathFromFile(const QString &filename);
        static bool fileExists(const QString &filename);
        static void copyFile(const QString &destination, const QString &source);
        static int base64index(char c);
        static QString generateStyleSheet(const QStringList &files);
        static QString decodedStringFromString(const QString &str);
        static QString encodedStringFromString(const QString &str);
};

#endif // QTUTIL_H
