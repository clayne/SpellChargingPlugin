/*
    Copyright © 2020 r-neal-kelly, aka doticu
*/

#pragma once

#include "doticu_skylib/enum.h"

namespace doticu_skylib {

    class Global_Type_e : public Enum_t<s8>
    {
    public:
        enum : value_type
        {
            _NONE_  = 0,

            FLOAT   = 'f',
            SHORT   = 's',
            LONG    = 'l',
        };
        using Enum_t::Enum_t;
    };

}
