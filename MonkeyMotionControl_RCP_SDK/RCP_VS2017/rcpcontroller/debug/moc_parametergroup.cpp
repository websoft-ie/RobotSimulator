/****************************************************************************
** Meta object code from reading C++ file 'parametergroup.h'
**
** Created by: The Qt Meta Object Compiler version 67 (Qt 5.14.2)
**
** WARNING! All changes made in this file will be lost!
*****************************************************************************/

#include <memory>
#include "../parametergroup.h"
#include <QtCore/qbytearray.h>
#include <QtCore/qmetatype.h>
#if !defined(Q_MOC_OUTPUT_REVISION)
#error "The header file 'parametergroup.h' doesn't include <QObject>."
#elif Q_MOC_OUTPUT_REVISION != 67
#error "This file was generated using the moc from 5.14.2. It"
#error "cannot be used with the include files from this version of Qt."
#error "(The moc has changed too much.)"
#endif

QT_BEGIN_MOC_NAMESPACE
QT_WARNING_PUSH
QT_WARNING_DISABLE_DEPRECATED
struct qt_meta_stringdata_ParameterGroup_t {
    QByteArrayData data[17];
    char stringdata0[177];
};
#define QT_MOC_LITERAL(idx, ofs, len) \
    Q_STATIC_BYTE_ARRAY_DATA_HEADER_INITIALIZER_WITH_OFFSET(len, \
    qptrdiff(offsetof(qt_meta_stringdata_ParameterGroup_t, stringdata0) + ofs \
        - idx * sizeof(QByteArrayData)) \
    )
static const qt_meta_stringdata_ParameterGroup_t qt_meta_stringdata_ParameterGroup = {
    {
QT_MOC_LITERAL(0, 0, 14), // "ParameterGroup"
QT_MOC_LITERAL(1, 15, 10), // "stringSent"
QT_MOC_LITERAL(2, 26, 0), // ""
QT_MOC_LITERAL(3, 27, 11), // "rcp_param_t"
QT_MOC_LITERAL(4, 39, 7), // "paramID"
QT_MOC_LITERAL(5, 47, 5), // "value"
QT_MOC_LITERAL(6, 53, 7), // "intSent"
QT_MOC_LITERAL(7, 61, 10), // "listNeeded"
QT_MOC_LITERAL(8, 72, 12), // "stringNeeded"
QT_MOC_LITERAL(9, 85, 16), // "sendSliderString"
QT_MOC_LITERAL(10, 102, 10), // "sendString"
QT_MOC_LITERAL(11, 113, 7), // "sendInt"
QT_MOC_LITERAL(12, 121, 5), // "index"
QT_MOC_LITERAL(13, 127, 9), // "openCombo"
QT_MOC_LITERAL(14, 137, 10), // "closeCombo"
QT_MOC_LITERAL(15, 148, 14), // "disableUpdates"
QT_MOC_LITERAL(16, 163, 13) // "enableUpdates"

    },
    "ParameterGroup\0stringSent\0\0rcp_param_t\0"
    "paramID\0value\0intSent\0listNeeded\0"
    "stringNeeded\0sendSliderString\0sendString\0"
    "sendInt\0index\0openCombo\0closeCombo\0"
    "disableUpdates\0enableUpdates"
};
#undef QT_MOC_LITERAL

static const uint qt_meta_data_ParameterGroup[] = {

 // content:
       8,       // revision
       0,       // classname
       0,    0, // classinfo
      11,   14, // methods
       0,    0, // properties
       0,    0, // enums/sets
       0,    0, // constructors
       0,       // flags
       4,       // signalCount

 // signals: name, argc, parameters, tag, flags
       1,    2,   69,    2, 0x06 /* Public */,
       6,    2,   74,    2, 0x06 /* Public */,
       7,    1,   79,    2, 0x06 /* Public */,
       8,    1,   82,    2, 0x06 /* Public */,

 // slots: name, argc, parameters, tag, flags
       9,    1,   85,    2, 0x08 /* Private */,
      10,    1,   88,    2, 0x08 /* Private */,
      11,    1,   91,    2, 0x08 /* Private */,
      13,    0,   94,    2, 0x08 /* Private */,
      14,    0,   95,    2, 0x08 /* Private */,
      15,    0,   96,    2, 0x08 /* Private */,
      16,    0,   97,    2, 0x08 /* Private */,

 // signals: parameters
    QMetaType::Void, 0x80000000 | 3, QMetaType::QString,    4,    5,
    QMetaType::Void, 0x80000000 | 3, QMetaType::Int,    4,    5,
    QMetaType::Void, 0x80000000 | 3,    4,
    QMetaType::Void, 0x80000000 | 3,    4,

 // slots: parameters
    QMetaType::Void, QMetaType::QString,    5,
    QMetaType::Void, QMetaType::QString,    5,
    QMetaType::Void, QMetaType::Int,   12,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,
    QMetaType::Void,

       0        // eod
};

void ParameterGroup::qt_static_metacall(QObject *_o, QMetaObject::Call _c, int _id, void **_a)
{
    if (_c == QMetaObject::InvokeMetaMethod) {
        auto *_t = static_cast<ParameterGroup *>(_o);
        Q_UNUSED(_t)
        switch (_id) {
        case 0: _t->stringSent((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const QString(*)>(_a[2]))); break;
        case 1: _t->intSent((*reinterpret_cast< rcp_param_t(*)>(_a[1])),(*reinterpret_cast< const int(*)>(_a[2]))); break;
        case 2: _t->listNeeded((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        case 3: _t->stringNeeded((*reinterpret_cast< rcp_param_t(*)>(_a[1]))); break;
        case 4: _t->sendSliderString((*reinterpret_cast< const QString(*)>(_a[1]))); break;
        case 5: _t->sendString((*reinterpret_cast< const QString(*)>(_a[1]))); break;
        case 6: _t->sendInt((*reinterpret_cast< const int(*)>(_a[1]))); break;
        case 7: _t->openCombo(); break;
        case 8: _t->closeCombo(); break;
        case 9: _t->disableUpdates(); break;
        case 10: _t->enableUpdates(); break;
        default: ;
        }
    } else if (_c == QMetaObject::IndexOfMethod) {
        int *result = reinterpret_cast<int *>(_a[0]);
        {
            using _t = void (ParameterGroup::*)(rcp_param_t , const QString & );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&ParameterGroup::stringSent)) {
                *result = 0;
                return;
            }
        }
        {
            using _t = void (ParameterGroup::*)(rcp_param_t , const int );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&ParameterGroup::intSent)) {
                *result = 1;
                return;
            }
        }
        {
            using _t = void (ParameterGroup::*)(rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&ParameterGroup::listNeeded)) {
                *result = 2;
                return;
            }
        }
        {
            using _t = void (ParameterGroup::*)(rcp_param_t );
            if (*reinterpret_cast<_t *>(_a[1]) == static_cast<_t>(&ParameterGroup::stringNeeded)) {
                *result = 3;
                return;
            }
        }
    }
}

QT_INIT_METAOBJECT const QMetaObject ParameterGroup::staticMetaObject = { {
    QMetaObject::SuperData::link<QWidget::staticMetaObject>(),
    qt_meta_stringdata_ParameterGroup.data,
    qt_meta_data_ParameterGroup,
    qt_static_metacall,
    nullptr,
    nullptr
} };


const QMetaObject *ParameterGroup::metaObject() const
{
    return QObject::d_ptr->metaObject ? QObject::d_ptr->dynamicMetaObject() : &staticMetaObject;
}

void *ParameterGroup::qt_metacast(const char *_clname)
{
    if (!_clname) return nullptr;
    if (!strcmp(_clname, qt_meta_stringdata_ParameterGroup.stringdata0))
        return static_cast<void*>(this);
    return QWidget::qt_metacast(_clname);
}

int ParameterGroup::qt_metacall(QMetaObject::Call _c, int _id, void **_a)
{
    _id = QWidget::qt_metacall(_c, _id, _a);
    if (_id < 0)
        return _id;
    if (_c == QMetaObject::InvokeMetaMethod) {
        if (_id < 11)
            qt_static_metacall(this, _c, _id, _a);
        _id -= 11;
    } else if (_c == QMetaObject::RegisterMethodArgumentMetaType) {
        if (_id < 11)
            *reinterpret_cast<int*>(_a[0]) = -1;
        _id -= 11;
    }
    return _id;
}

// SIGNAL 0
void ParameterGroup::stringSent(rcp_param_t _t1, const QString & _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 0, _a);
}

// SIGNAL 1
void ParameterGroup::intSent(rcp_param_t _t1, const int _t2)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))), const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t2))) };
    QMetaObject::activate(this, &staticMetaObject, 1, _a);
}

// SIGNAL 2
void ParameterGroup::listNeeded(rcp_param_t _t1)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))) };
    QMetaObject::activate(this, &staticMetaObject, 2, _a);
}

// SIGNAL 3
void ParameterGroup::stringNeeded(rcp_param_t _t1)
{
    void *_a[] = { nullptr, const_cast<void*>(reinterpret_cast<const void*>(std::addressof(_t1))) };
    QMetaObject::activate(this, &staticMetaObject, 3, _a);
}
QT_WARNING_POP
QT_END_MOC_NAMESPACE
