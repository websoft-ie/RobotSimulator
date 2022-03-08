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

#include "qtutil.h"
#include <QRegExp>
#include <QApplication>
#include <QDesktopWidget>
#include <QFile>
#include <iostream>
#include <fstream>

Qt::GlobalColor QtUtil::qtColor(int color)
{
    switch (color)
    {
        case BLACK:
            return Qt::black;

        case RED:
            return Qt::red;

        case BLUE:
            return Qt::blue;

        case GREEN:
            return Qt::green;

        case YELLOW:
            return Qt::yellow;

        case MAGENTA:
            return Qt::magenta;

        case CYAN:
            return Qt::cyan;

        case DARK_GRAY:
            return Qt::darkGray;

        case WHITE:
            return Qt::white;

        default:
            return Qt::black;
    }
}

Qt::PenStyle QtUtil::qtPenStyle(int lineStyle)
{
    switch (lineStyle)
    {
        case SOLID:
            return Qt::SolidLine;

        case DASHED:
            return Qt::DotLine;

        case BRACKET:
            return Qt::CustomDashLine;

        default:
            return Qt::SolidLine;
    }
}

tRect QtUtil::tRectFromQRect(const QRect &qRect)
{
    tRect rect;

    rect.x = qRect.x();
    rect.y = qRect.y();
    rect.width = qRect.width();
    rect.height = qRect.height();

    return rect;
}

QPoint QtUtil::desktopCenter(const QSize &windowSize)
{
    QDesktopWidget *desktop = QApplication::desktop();
    QRect available = desktop->availableGeometry();

    return QPoint((available.width() / 2) - (windowSize.width() / 2), (available.height() / 2) - (windowSize.height() / 2));
}

bool QtUtil::filenameIsValid(const QString &filename)
{
    bool isValid = true;

    if (!filename.endsWith(".preset"))
    {
        isValid = false;
    }
    else
    {
        QString basename = baseFilename(filename);

        if (basename.trimmed().isEmpty())
        {
            isValid = false;
        }
        else
        {
            // File names can only have alphanumeric characters, spaces, underscores, and hyphens
            QRegExp fileRegex("[^A-Za-z0-9 _\\-]");

            // If a match exists
            if (fileRegex.indexIn(basename) != -1)
            {
                isValid = false;
            }
        }
    }

    return isValid;
}

// Extracts just the base file name without the path
QString QtUtil::baseFilename(const QString &filename, bool keepExtension /*= false*/)
{
    QString basename(filename);

    basename.remove(0, basename.lastIndexOf('/') + 1);

    if (!keepExtension)
    {
        int index = basename.lastIndexOf('.');
        basename.remove(index, basename.length() - index);
    }

    return basename;
}

QString QtUtil::pathFromFile(const QString &filename)
{
    QString path = filename;

    int index = path.lastIndexOf('/');

    path = path.remove(index, path.length() - index);

    return path;
}

bool QtUtil::fileExists(const QString &filename)
{
    FILE *file = fopen(filename.toUtf8().constData(), "r");

    if (file)
    {
        fclose(file);
        return true;
    }

    return false;
}

// Copies the contents of the source file to the destination file. If the destination file does not exist, it gets created. If the source file doesn't exist, false is returned and nothing is copied.
void QtUtil::copyFile(const QString &destination, const QString &source)
{
    std::ifstream src(source.toUtf8().constData(), std::ios::in | std::ios::binary);
    std::ofstream dest(destination.toUtf8().constData(), std::ios::out | std::ios::binary);
    dest << src.rdbuf();
}

int QtUtil::base64index(char c)
{
    int ascii = (int)c;

    // A - Z
    if (ascii >= 65 && ascii <= 90)
    {
        return ascii - 65;
    }
    // a - z
    else if (ascii >= 97 && ascii <= 122)
    {
        return ascii - 71;
    }
    // 0 - 9
    else if (ascii >= 48 && ascii <= 57)
    {
        return ascii + 4;
    }
    else if (c == '+')
    {
        return 62;
    }
    else if (c == '/')
    {
        return 63;
    }

    return -1;
}

QString QtUtil::generateStyleSheet(const QStringList &files)
{
	QString styleString = "";
    QStringList::const_iterator i;

    for (i = files.begin(); i != files.end(); i++)
    {
        QFile stylesheet(*i);
        stylesheet.open(QFile::ReadOnly);

        if (stylesheet.isOpen())
        {
            styleString.append(QLatin1String(stylesheet.readAll()));
            stylesheet.close();
        }
    }
	
	return styleString;
}

QString QtUtil::decodedStringFromString(const QString &str)
{
    QString decodedString = str;

    decodedString.replace("&amp;", "&");
    decodedString.replace("&deg;", QString::fromWCharArray(L"\u00B0"));
    decodedString.replace("&reg;", QString::fromWCharArray(L"\u00AE"));
    decodedString.replace("&copy;", QString::fromWCharArray(L"\u00A9"));
    decodedString.replace("&trade;", QString::fromWCharArray(L"\u2122"));
    decodedString.replace("&redana2;", " ANA 2");
    decodedString.replace("&redana13;", " ANA 1.3");
    decodedString.replace("&redae;", "AE");
    decodedString.replace("&rediso;", "ISO ");
    decodedString.replace("&redsec;", " sec");
    decodedString.replace("&redkelvin;", "K");
    decodedString.replace("&redformatk;", "K");
    decodedString.replace("&redfps;", " FPS");
    decodedString.replace("&red1over;", "1/");
    decodedString.replace("&redfover;", "f/");
    decodedString.replace("&redav;", " Av");
    decodedString.replace("&redcheck;", " Check");

    return decodedString;
}

QString QtUtil::encodedStringFromString(const QString &str)
{
    QString encodedString = str;

    encodedString.replace("&", "&amp;");
    encodedString.replace(QString::fromWCharArray(L"\u00B0"), "&deg;");
    encodedString.replace(QString::fromWCharArray(L"\u00AE"), "&reg;");
    encodedString.replace(QString::fromWCharArray(L"\u00A9"), "&copy;");
    encodedString.replace(QString::fromWCharArray(L"\u2122"), "&trade;");
    encodedString.replace(" ANA 2", "&redana2;");
    encodedString.replace(" ANA 1.3", "&redana13;");
    encodedString.replace("AE", "&redae;");
    encodedString.replace("ISO ", "&rediso;");
    encodedString.replace(" sec", "&redsec;");
    encodedString.replace("K", "&redkelvin;");
    encodedString.replace("K", "&redformatk;");
    encodedString.replace(" FPS", "&redfps;");
    encodedString.replace("1/", "&red1over;");
    encodedString.replace("f/", "&redfover;");
    encodedString.replace(" Av", "&redav;");
    encodedString.replace(" Check", "&redcheck;");

    return encodedString;
}
