/****************************************************************************
** Meta object code from reading C++ file 'mainwindow.h'
**
** Created by: The Qt Meta Object Compiler version 67 (Qt 5.14.2)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include <memory>
#include "../mainwindow.h"
#include <QtCore/qbytearray.h>
#include <QtCore/qmetatype.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'mainwindow.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 67
#error "This file was generated using the moc from 5.14.2. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
struct qt_meta_stringdata_MainWindow_t {
    QByteArrayData data[33];
    char stringdata0[392];
};
#define QT_MOC_LITERAL(idx, ofs, len) \
    Q_STATIC_BYTE_ARRAY_DATA_HEADER_INITIALIZER_WITH_OFFSET(len, \
    qptrdiff(offsetof(qt_meta_stringdata_MainWindow_t, stringdata0) + ofs \
        - idx * sizeof(QByteArrayData)) \
    )
static const qt_meta_stringdata_MainWindow_t qt_meta_stringdata_MainWindow = {
    {
QT_MOC_LITERAL(0, 0, 10), // "MainWindow"
QT_MOC_LITERAL(1, 11, 10), // "intUpdated"
QT_MOC_LITERAL(2, 22, 0), // ""
QT_MOC_LITERAL(3, 23, 5), // "value"
QT_MOC_LITERAL(4, 29, 11), // "rcp_param_t"
QT_MOC_LITERAL(5, 41, 7), // "paramID"
QT_MOC_LITERAL(6, 49, 11), // "listUpdated"
QT_MOC_LITERAL(7, 61, 29), // "const rcp_cur_list_cb_data_t*"
QT_MOC_LITERAL(8, 91, 4), // "data"
QT_MOC_LITERAL(9, 96, 17), // "updateOnlyOnClose"
QT_MOC_LITERAL(10, 114, 10), // "strUpdated"
QT_MOC_LITERAL(11, 125, 3), // "str"
QT_MOC_LITERAL(12, 129, 18), // "rcp_param_status_t"
QT_MOC_LITERAL(13, 148, 6), // "status"
QT_MOC_LITERAL(14, 155, 7), // "sendRCP"
QT_MOC_LITERAL(15, 163, 11), // "rcp_error_t"
QT_MOC_LITERAL(16, 175, 11), // "const char*"
QT_MOC_LITERAL(17, 187, 6), // "size_t"
QT_MOC_LITERAL(18, 194, 3), // "len"
QT_MOC_LITERAL(19, 198, 14), // "dropConnection"
QT_MOC_LITERAL(20, 213, 6), // "rcpGet"
QT_MOC_LITERAL(21, 220, 16), // "toggleConnection"
QT_MOC_LITERAL(22, 237, 7), // "connect"
QT_MOC_LITERAL(23, 245, 15), // "connectToCamera"
QT_MOC_LITERAL(24, 261, 7), // "readRCP"
QT_MOC_LITERAL(25, 269, 11), // "socketError"
QT_MOC_LITERAL(26, 281, 28), // "QAbstractSocket::SocketError"
QT_MOC_LITERAL(27, 310, 9), // "errorCode"
QT_MOC_LITERAL(28, 320, 19), // "serialProtocolError"
QT_MOC_LITERAL(29, 340, 20), // "externalControlError"
QT_MOC_LITERAL(30, 361, 9), // "rcpSetInt"
QT_MOC_LITERAL(31, 371, 9), // "rcpSetStr"
QT_MOC_LITERAL(32, 381, 10) // "rcpGetList"

    },
    "MainWindow\0intUpdated\0\0value\0rcp_param_t\0"
    "paramID\0listUpdated\0const rcp_cur_list_cb_data_t*\0"
    "data\0updateOnlyOnClose\0strUpdated\0str\0"
    "rcp_param_status_t\0status\0sendRCP\0"
    "rcp_error_t\0const char*\0size_t\0len\0"
    "dropConnection\0rcpGet\0toggleConnection\0"
    "connect\0connectToCamera\0readRCP\0"
    "socketError\0QAbstractSocket::SocketError\0"
    "errorCode\0serialProtocolError\0"
    "externalControlError\0rcpSetInt\0rcpSetStr\0"
    "rcpGetList"
};
#undef QT_MOC_LITERAL

static const uint qt_meta_data_MainWindow[] = {

 // content:
       8,       // revision
       0,       // classname
       0,    0, // classinfo
      15,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       3,       // signalCount

 // signals: name, argc, parameters, tag, flags
       1,    2,   89,    2, 0x06 /* Public */,
       6,    2,   94,    2, 0x06 /* Public */,
      10,    3,   99,    2, 0x06 /* Public */,

 // slots: name, argc, parameters, tag, flags
      14,    2,  106,    2, 0x0a /* Public */,
      19,    0,  111,    2, 0x0a /* Public */,
      20,    1,  112,    2, 0x0a /* Public */,
      21,    1,  115,    2, 0x08 /* Private */,
      23,    0,  118,    2, 0x08 /* Private */,
      24,    0,  119,    2, 0x08 /* Private */,
      25,    1,  120,    2, 0x08 /* Private */,
      28,    0,  123,    2, 0x08 /* Private */,
      29,    0,  124,    2, 0x08 /* Private */,
      30,    2,  125,    2, 0x08 /* Private */,
      31,    2,  130,    2, 0x08 /* Private */,
      32,    1,  135,    2, 0x08 /* Private */,

 // signals: parameters
    QMetaType::Void, QMetaType::Int, 0x80000000 | 4,    3,    5,
    QMetaType::Void, 0x80000000 | 7, QMetaType::Bool,    8,    9,
    QMetaType::Void, QMetaType::QString, 0x80000000 | 12, 0x80000000 | 4,   11,   13,    5,

 // slots: parameters
    0x80000000 | 15, 0x80000000 | 16, 0x80000000 | 17,    8,   18,
    QMetaType::Void,
    QMetaType::Void, 0x80000000 | 4,    5,
    QMetaType::Void, QMetaType::Bool,   22,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void, 0x80000000 | 26,   27,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void, 0x80000000 | 4, QMetaType::Int,    5,    3,
    QMetaType::Void, 0x80000000 | 4, QMetaType::QString,    5,   11,
    QMetaType::Void, 0x80000000 | 4,    5,

       0        // eod
};

void MainWindow::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        auto *_t = static_cast<MainWindow *>(_o);
        Q_UNUSED(_t)
        switch (_id) {
        case 0: _t->intUpdated((*reinterpret_cast< const int(*)>(_a[1])),(*reinterpret_cast< const rcp_param_t(*)>(_a[2]))); break;
        case 1: _t->listUpdated((*reinterpret_cast< const rcp_cur_list_cb_data_t*(*)>(_a[1])),(*reinterpret_cast< const bool(*)>(_a[2]))); break;
        case 2: _t->strUpdated((*reinterpret_cast< const QString(*)>(_a[1])),(*reinterpret_cast< const rcp_param_status_t(*)>(_a[2])),(*reinterpret_cast< const rcp_param_t(*)>(_a[3]))); break;
        case 3: { rcp_error_t _r = _t->sendRCP((*reinterpret_cast< const char*(*)>(_a[1])),(*reinterpret_cast< size_t(*)>(_a[2])));
            if (_a[0]) *reinterpret_cast< rcp_error_t*>(_a[0]) = std::move(_r); }  break;
        case 4: _t->dropConnection(); break;
        case 5: _t->rcpGet((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        case 6: _t->toggleConnection((*reinterpret_cast< const bool(*)>(_a[1]))); break;
        case 7: _t->connectToCamera(); break;
        case 8: _t->readRCP(); break;
        case 9: _t->socketError((*reinterpret_cast< QAbstractSocket::SocketError(*)>(_a[1]))); break;
        case 10: _t->serialProtocolError(); break;
        case 11: _t->externalControlError(); break;
        case 12: _t->rcpSetInt((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const int(*)>(_a[2]))); break;
        case 13: _t->rcpSetStr((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const QString(*)>(_a[2]))); break;
        case 14: _t->rcpGetList((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        default: ;
        }
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        switch (_id) {
        default: *reinterpret_cast<int*>(_a[0]) = -1; break;
        case 9:
            switch (*reinterpret_cast<int*>(_a[1])) {
            default: *reinterpret_cast<int*>(_a[0]) = -1; break;
            case 0:
                *reinterpret_cast<int*>(_a[0]) = qRegisterMetaType< QAbstractSocket::SocketError >(); break;
            }
            break;
        }
    } else if (_c == QMetaObject::IndexOfMethod) {
        int *result = reinterpret_cast<int *>(_a[0]);
        {
            using _t = void (MainWindow::*)(const int , const rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&MainWindow::intUpdated)) {
                *result = 0;
                return;
            }
        }
        {
            using _t = void (MainWindow::*)(const rcp_cur_list_cb_data_t * , const bool );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&MainWindow::listUpdated)) {
                *result = 1;
                return;
            }
        }
        {
            using _t = void (MainWindow::*)(const QString & , const rcp_param_status_t , const rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&MainWindow::strUpdated)) {
                *result = 2;
                return;
            }
        }
    }
}

QT_INIT_METAOBJECT const QMetaObject MainWindow::staticMetaObject = { {
    QMetaObject::SuperData::link<QMainWindow::staticMetaObject>(),
    qt_meta_stringdata_MainWindow.data,
    qt_meta_data_MainWindow,
    qt_static_metacall,
    nullptr,
    nullptr
} };


const QMetaObject *MainWindow::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *MainWindow::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_meta_stringdata_MainWindow.stringdata0))
        return static_cast<void*>(this);
    return QMainWindow::qt_metacast(_clname);
}

int MainWindow::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QMainWindow::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 15)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 15;
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 15)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 15;
    }
    return _id;
}

// SIGNAL 0
void MainWindow::intUpdated(const int _t1, const rcp_param_t _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 0, _a);
}

// SIGNAL 1
void MainWindow::listUpdated(const rcp_cur_list_cb_data_t * _t1, const bool _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 1, _a);
}

// SIGNAL 2
void MainWindow::strUpdated(const QString & _t1, const rcp_param_status_t _t2, const rcp_param_t _t3)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t3))) };
    QMetaObject::activate(this, &staticMetaObject, 2, _a);
}
QT_WARNING_POP
QT_END_MOC_NAMESPACE
