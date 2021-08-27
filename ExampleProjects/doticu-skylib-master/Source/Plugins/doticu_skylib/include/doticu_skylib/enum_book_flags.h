/*
    Copyright © 2020 r-neal-kelly, aka doticu
*/

#pragma once

#include "doticu_skylib/enum.h"

namespace doticu_skylib {

    class Book_Flags_e :
        public Enum_t<u8>
    {
    public:
        enum enum_type : value_type
        {
            CAN_TEACH_SKILL = static_cast<value_type>(1 << 0),
            CANT_TAKE       = static_cast<value_type>(1 << 1),
            CAN_TEACH_SPELL = static_cast<value_type>(1 << 2),
            HAS_BEEN_READ   = static_cast<value_type>(1 << 3),
        };

    public:
        using Enum_t::Enum_t;
    };

}
