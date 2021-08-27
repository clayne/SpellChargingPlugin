/*
    Copyright © 2020 r-neal-kelly, aka doticu
*/

#pragma once

#include "doticu_skylib/enum.h"

namespace doticu_skylib {

    class Weapon_State_e :
        public Enum_t<u8>
    {
    public:
        enum enum_type : value_type
        {
            SHEATHED            = 0x0,
            WANTS_TO_DRAW       = 0x1,
            DRAWING             = 0x2,
            DRAWN               = 0x3,
            WANTS_TO_SHEATHE    = 0x4,
            SHEATHING           = 0x5,
        };
        using Enum_t::Enum_t;
    };

}
