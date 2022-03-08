/****************************************************************************
** Meta object code from reading C++ file 'connection.h'
**
** Created by: The Qt Meta Object Compiler version 67 (Qt 5.14.2)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include <memory>
#include "../connection.h"
#include <QtCore/qbytearray.h>
#include <QtCore/qmetatype.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'connection.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 67
#error "This file was generated using the moc from 5.14.2. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
struct qt_meta_stringdata_Connection_t {
    QByteArrayData data[20];
    char stringdata0[327];
};
#define QT_MOC_LITERAL(idx, ofs, len) \
    Q_STATIC_BYTE_ARRAY_DATA_HEADER_INITIALIZER_WITH_OFFSET(len, \
    qptrdiff(offsetof(qt_meta_stringdata_Connection_t, stringdata0) + ofs \
        - idx * sizeof(QByteArrayData)) \
    )
static const qt_meta_stringdata_Connection_t qt_meta_stringdata_Connection = {
    {
QT_MOC_LITERAL(0, 0, 10), // "Connection"
QT_MOC_LITERAL(1, 11, 14), // "connectClicked"
QT_MOC_LITERAL(2, 26, 0), // ""
QT_MOC_LITERAL(3, 27, 7), // "connect"
QT_MOC_LITERAL(4, 35, 22), // "updateConnectionStatus"
QT_MOC_LITERAL(5, 58, 16), // "ConnectionStatus"
QT_MOC_LITERAL(6, 75, 6), // "status"
QT_MOC_LITERAL(7, 82, 20), // "updateConnectionType"
QT_MOC_LITERAL(8, 103, 5), // "index"
QT_MOC_LITERAL(9, 109, 15), // "setConnectionId"
QT_MOC_LITERAL(10, 125, 16), // "QListWidgetItem*"
QT_MOC_LITERAL(11, 142, 18), // "connectionListItem"
QT_MOC_LITERAL(12, 161, 15), // "connectFromList"
QT_MOC_LITERAL(13, 177, 16), // "searchForCameras"
QT_MOC_LITERAL(14, 194, 23), // "processPendingDatagrams"
QT_MOC_LITERAL(15, 218, 24), // "discoveryStepAndFinalize"
QT_MOC_LITERAL(16, 243, 24), // "incrementSessionDuration"
QT_MOC_LITERAL(17, 268, 28), // "enableOrDisableConnectButton"
QT_MOC_LITERAL(18, 297, 12), // "connectionId"
QT_MOC_LITERAL(19, 310, 16) // "toggleConnection"

    },
    "Connection\0connectClicked\0\0connect\0"
    "updateConnectionStatus\0ConnectionStatus\0"
    "status\0updateConnectionType\0index\0"
    "setConnectionId\0QListWidgetItem*\0"
    "connectionListItem\0connectFromList\0"
    "searchForCameras\0processPendingDatagrams\0"
    "discoveryStepAndFinalize\0"
    "incrementSessionDuration\0"
    "enableOrDisableConnectButton\0connectionId\0"
    "toggleConnection"
};
#undef QT_MOC_LITERAL

static const uint qt_meta_data_Connection[] = {

 // content:
       8,       // revision
       0,       // classname
       0,    0, // classinfo
      12,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       1,       // signalCount

 // signals: name, argc, parameters, tag, flags
       1,    1,   74,    2, 0x06 /* Public */,

 // slots: name, argc, parameters, tag, flags
       4,    1,   77,    2, 0x0a /* Public */,
       7,    1,   80,    2, 0x0a /* Public */,
       7,    0,   83,    2, 0x2a /* Public | MethodCloned */,
       9,    1,   84,    2, 0x08 /* Private */,
      12,    1,   87,    2, 0x08 /* Private */,
      13,    0,   90,    2, 0x08 /* Private */,
      14,    0,   91,    2, 0x08 /* Private */,
      15,    0,   92,    2, 0x08 /* Private */,
      16,    0,   93,    2, 0x08 /* Private */,
      17,    1,   94,    2, 0x08 /* Private */,
      19,    0,   97,    2, 0x08 /* Private */,

 // signals: parameters
    QMetaType::Void, QMetaType::Bool,    3,

 // slots: parameters
    QMetaType::Void, 0x80000000 | 5,    6,
    QMetaType::Void, QMetaType::Int,    8,
    QMetaType::Void,
    QMetaType::Void, 0x80000000 | 10,   11,
    QMetaType::Void, 0x80000000 | 10,   11,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void, QMetaType::QString,   18,
    QMetaType::Void,

       0        // eod
};

void Connection::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        auto *_t = static_cast<Connection *>(_o);
        Q_UNUSED(_t)
        switch (_id) {
        case 0: _t->connectClicked((*reinterpret_cast< const bool(*)>(_a[1]))); break;
        case 1: _t->updateConnectionStatus((*reinterpret_cast< ConnectionStatus(*)>(_a[1]))); break;
        case 2: _t->updateConnectionType((*reinterpret_cast< int(*)>(_a[1]))); break;
        case 3: _t->updateConnectionType(); break;
        case 4: _t->setConnectionId((*reinterpret_cast< QListWidgetItem*(*)>(_a[1]))); break;
        case 5: _t->connectFromList((*reinterpret_cast< QListWidgetItem*(*)>(_a[1]))); break;
        case 6: _t->searchForCameras(); break;
        case 7: _t->processPendingDatagrams(); break;
        case 8: _t->discoveryStepAndFinalize(); break;
        case 9: _t->incrementSessionDuration(); break;
        case 10: _t->enableOrDisableConnectButton((*reinterpret_cast< const QString(*)>(_a[1]))); break;
        case 11: _t->toggleConnection(); break;
        default: ;
        }
    } else if (_c == QMetaObject::IndexOfMethod) {
        int *result = reinterpret_cast<int *>(_a[0]);
        {
            using _t = void (Connection::*)(const bool );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&Connection::connectClicked)) {
                *result = 0;
                return;
            }
        }
    }
}

QT_INIT_METAOBJECT const QMetaObject Connection::staticMetaObject = { {
    QMetaObject::SuperData::link<QWidget::staticMetaObject>(),
    qt_meta_stringdata_Connection.data,
    qt_meta_data_Connection,
    qt_static_metacall,
    nullptr,
    nullptr
} };


const QMetaObject *Connection::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *Connection::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_meta_stringdata_Connection.stringdata0))
        return static_cast<void*>(this);
    return QWidget::qt_metacast(_clname);
}

int Connection::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QWidget::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 12)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 12;
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 12)
            *reinterpret_cast<int*>(_a[0]) = -1;
        _id -= 12;
    }
    return _id;
}

// SIGNAL 0
void Connection::connectClicked(const bool _t1)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))) };
    QMetaObject::activate(this, &staticMetaObject, 0, _a);
}
QT_WARNING_POP
QT_END_MOC_NAMESPACE
