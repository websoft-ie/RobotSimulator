/****************************************************************************
** Meta object code from reading C++ file 'controller.h'
**
** Created by: The Qt Meta Object Compiler version 67 (Qt 5.14.2)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include <memory>
#include "../controller.h"
#include <QtCore/qbytearray.h>
#include <QtCore/qmetatype.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'controller.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 67
#error "This file was generated using the moc from 5.14.2. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
struct qt_meta_stringdata_Controller_t {
    QByteArrayData data[19];
    char stringdata0[211];
};
#define QT_MOC_LITERAL(idx, ofs, len) \
    Q_STATIC_BYTE_ARRAY_DATA_HEADER_INITIALIZER_WITH_OFFSET(len, \
    qptrdiff(offsetof(qt_meta_stringdata_Controller_t, stringdata0) + ofs \
        - idx * sizeof(QByteArrayData)) \
    )
static const qt_meta_stringdata_Controller_t qt_meta_stringdata_Controller = {
    {
QT_MOC_LITERAL(0, 0, 10), // "Controller"
QT_MOC_LITERAL(1, 11, 10), // "stringSent"
QT_MOC_LITERAL(2, 22, 0), // ""
QT_MOC_LITERAL(3, 23, 11), // "rcp_param_t"
QT_MOC_LITERAL(4, 35, 7), // "paramID"
QT_MOC_LITERAL(5, 43, 5), // "value"
QT_MOC_LITERAL(6, 49, 7), // "intSent"
QT_MOC_LITERAL(7, 57, 10), // "listNeeded"
QT_MOC_LITERAL(8, 68, 12), // "stringNeeded"
QT_MOC_LITERAL(9, 81, 9), // "updateInt"
QT_MOC_LITERAL(10, 91, 12), // "updateString"
QT_MOC_LITERAL(11, 104, 3), // "str"
QT_MOC_LITERAL(12, 108, 18), // "rcp_param_status_t"
QT_MOC_LITERAL(13, 127, 6), // "status"
QT_MOC_LITERAL(14, 134, 10), // "updateList"
QT_MOC_LITERAL(15, 145, 29), // "const rcp_cur_list_cb_data_t*"
QT_MOC_LITERAL(16, 175, 4), // "data"
QT_MOC_LITERAL(17, 180, 17), // "updateOnlyOnClose"
QT_MOC_LITERAL(18, 198, 12) // "toggleRecord"

    },
    "Controller\0stringSent\0\0rcp_param_t\0"
    "paramID\0value\0intSent\0listNeeded\0"
    "stringNeeded\0updateInt\0updateString\0"
    "str\0rcp_param_status_t\0status\0updateList\0"
    "const rcp_cur_list_cb_data_t*\0data\0"
    "updateOnlyOnClose\0toggleRecord"
};
#undef QT_MOC_LITERAL

static const uint qt_meta_data_Controller[] = {

 // content:
       8,       // revision
       0,       // classname
       0,    0, // classinfo
       8,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       4,       // signalCount

 // signals: name, argc, parameters, tag, flags
       1,    2,   54,    2, 0x06 /* Public */,
       6,    2,   59,    2, 0x06 /* Public */,
       7,    1,   64,    2, 0x06 /* Public */,
       8,    1,   67,    2, 0x06 /* Public */,

 // slots: name, argc, parameters, tag, flags
       9,    2,   70,    2, 0x08 /* Private */,
      10,    3,   75,    2, 0x08 /* Private */,
      14,    2,   82,    2, 0x08 /* Private */,
      18,    0,   87,    2, 0x08 /* Private */,

 // signals: parameters
    QMetaType::Void, 0x80000000 | 3, QMetaType::QString,    4,    5,
    QMetaType::Void, 0x80000000 | 3, QMetaType::Int,    4,    5,
    QMetaType::Void, 0x80000000 | 3,    4,
    QMetaType::Void, 0x80000000 | 3,    4,

 // slots: parameters
    QMetaType::Void, QMetaType::Int, 0x80000000 | 3,    5,    4,
    QMetaType::Void, QMetaType::QString, 0x80000000 | 12, 0x80000000 | 3,   11,   13,    4,
    QMetaType::Void, 0x80000000 | 15, QMetaType::Bool,   16,   17,
    QMetaType::Void,

       0        // eod
};

void Controller::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        auto *_t = static_cast<Controller *>(_o);
        Q_UNUSED(_t)
        switch (_id) {
        case 0: _t->stringSent((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const QString(*)>(_a[2]))); break;
        case 1: _t->intSent((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const int(*)>(_a[2]))); break;
        case 2: _t->listNeeded((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        case 3: _t->stringNeeded((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        case 4: _t->updateInt((*reinterpret_cast< const int(*)>(_a[1])),(*reinterpret_cast< const rcp_param_t(*)>(_a[2]))); break;
        case 5: _t->updateString((*reinterpret_cast< const QString(*)>(_a[1])),(*reinterpret_cast< const rcp_param_status_t(*)>(_a[2])),(*reinterpret_cast< const rcp_param_t(*)>(_a[3]))); break;
        case 6: _t->updateList((*reinterpret_cast< const rcp_cur_list_cb_data_t*(*)>(_a[1])),(*reinterpret_cast< const bool(*)>(_a[2]))); break;
        case 7: _t->toggleRecord(); break;
        default: ;
        }
    } else if (_c == QMetaObject::IndexOfMethod) {
        int *result = reinterpret_cast<int *>(_a[0]);
        {
            using _t = void (Controller::*)(rcp_param_t , const QString & );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&Controller::stringSent)) {
                *result = 0;
                return;
            }
        }
        {
            using _t = void (Controller::*)(rcp_param_t , const int );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&Controller::intSent)) {
                *result = 1;
                return;
            }
        }
        {
            using _t = void (Controller::*)(rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&Controller::listNeeded)) {
                *result = 2;
                return;
            }
        }
        {
            using _t = void (Controller::*)(rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&Controller::stringNeeded)) {
                *result = 3;
                return;
            }
        }
    }
}

QT_INIT_METAOBJECT const QMetaObject Controller::staticMetaObject = { {
    QMetaObject::SuperData::link<QWidget::staticMetaObject>(),
    qt_meta_stringdata_Controller.data,
    qt_meta_data_Controller,
    qt_static_metacall,
    nullptr,
    nullptr
} };


const QMetaObject *Controller::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *Controller::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_meta_stringdata_Controller.stringdata0))
        return static_cast<void*>(this);
    return QWidget::qt_metacast(_clname);
}

int Controller::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QWidget::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 8)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 8;
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 8)
            *reinterpret_cast<int*>(_a[0]) = -1;
        _id -= 8;
    }
    return _id;
}

// SIGNAL 0
void Controller::stringSent(rcp_param_t _t1, const QString & _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 0, _a);
}

// SIGNAL 1
void Controller::intSent(rcp_param_t _t1, const int _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 1, _a);
}

// SIGNAL 2
void Controller::listNeeded(rcp_param_t _t1)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))) };
    QMetaObject::activate(this, &staticMetaObject, 2, _a);
}

// SIGNAL 3
void Controller::stringNeeded(rcp_param_t _t1)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))) };
    QMetaObject::activate(this, &staticMetaObject, 3, _a);
}
QT_WARNING_POP
QT_END_MOC_NAMESPACE
