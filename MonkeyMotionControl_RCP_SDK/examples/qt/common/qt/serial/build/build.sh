#!/bin/bash

gunzip -c QtSerial.tar.gz | tar xopf -
cd qt-qtserialport/src
mkdir build
cd serialport
mv qt4support/include/private .
mv qt4support/include/QtCore .
mkdir QtSerialPort
mv qserialportglobal.h QtSerialPort
cp ../../../serialport.pro.user serialport.pro.user
cd ../build

/usr/bin/qmake -spec /usr/local/Qt4.7/mkspecs/macx-g++ CONFIG+=release CONFIG+=x86_64 -o Makefile ../serialport/serialport.pro

TAB=$'\t'
sed -i "" "s/-install_name${TAB}libQtSerialPort.1.dylib/-install_name${TAB}@executable_path\/..\/Frameworks\/libQtSerialPort.1.0.0.dylib/g" Makefile

make -w

install_name_tool -change "QtCore.framework/Versions/4/QtCore" "@executable_path/../Frameworks/QtCore.framework/Versions/4/QtCore" "libQtSerialPort.1.0.0.dylib"

open .
