/*
    Copyright � 2020 r-neal-kelly, aka doticu
*/

#pragma once

#include "doticu_skylib/enum.h"

namespace doticu_skylib {

    class Magic_Target_Flags_e :
        public Enum_t<u8>
    {
    public:
        enum enum_type : value_type
        {
        };

    public:
        using Enum_t::Enum_t;
    };

}
