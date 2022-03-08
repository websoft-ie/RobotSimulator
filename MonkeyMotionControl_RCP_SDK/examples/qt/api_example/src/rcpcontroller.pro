#-------------------------------------------------
#
# Project created by QtCreator 2013-10-08T17:59:58
#
#-------------------------------------------------

QT       += core gui network

TARGET = RCPController
TEMPLATE = app

isEmpty(RCP_SDK) {
    RCP_SDK = "../../../common"
}

RCP_CORE = $$RCP_SDK
RCP_API = $$RCP_SDK/rcp_api/amalgamation

RESOURCES_PATH = "../resource/redlink"

DEFINES += \
    VER_STR=\\\"${VERSION_STRING}\\\" \
    VER_NO=\\\"${BUILD_NUMBER}\\\" \
    LOG_INFO=1 \
    LOG_WARNINGS=1 \
    LOG_ERRORS=1 \
    LOG_DEBUG=1

SOURCES += \
    $$RCP_API/rcp_api.c \
    $$RCP_CORE/clist/clist.cpp \
    ../../common/qt/src/logger.cpp \
    ../../common/qt/src/qtutil.cpp \
    ../../common/qt/src/listwidget.cpp \
    combobox.cpp \
    main.cpp \
    mainwindow.cpp \
    slideredit.cpp \
    slider.cpp \
    connection.cpp \
    controller.cpp \
    parametergroup.cpp \
    listdata.cpp \
    combodelegate.cpp \
    strtok_r.c

HEADERS += \
    $$RCP_API/rcp_api.h \
    $$RCP_CORE/clist/clist.h \
    ../../common/qt/src/logger.h \
    ../../common/qt/src/qtutil.h \
    ../../common/qt/src/listwidget.h \
    combobox.h \
    mainwindow.h \
    slideredit.h \
    slider.h \
    connection.h \
    controller.h \
    parametergroup.h \
    listdata.h \
    combodelegate.h

INCLUDEPATH += \
    $$RCP_SDK \
    $$RCP_CORE \
    ../../common/qt/src \
    ../../common/qt/serial/include

win32:LIBS += -L../../common/qt/serial/lib -lQtSerialPort
unix:LIBS += -L../../common/qt/serial/lib -lQtSerialPort.1.0.0

RESOURCES += ../resource/resources.qrc

ICON = $$RESOURCES_PATH/redlink.icns

RC_FILE = $$RESOURCES_PATH/resources.rc
