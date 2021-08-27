/*
    Copyright © 2020 r-neal-kelly, aka doticu
*/

#pragma once

#include "doticu_skylib/bound_object.h"
#include "doticu_skylib/component_destructible.h"
#include "doticu_skylib/component_grab_sounds.h"
#include "doticu_skylib/component_icon.h"
#include "doticu_skylib/component_keywords.h"
#include "doticu_skylib/component_message_icon.h"
#include "doticu_skylib/component_model_alternates.h"
#include "doticu_skylib/component_name.h"
#include "doticu_skylib/component_value.h"
#include "doticu_skylib/component_weight.h"
#include "doticu_skylib/enum_script_type.h"

namespace doticu_skylib {

    class Misc_t :                  // TESObjectMISC
        public Bound_Object_t,      // 000
        public Name_c,              // 030
        public Model_Alternates_c,  // 040
        public Icon_c,              // 078
        public Value_c,             // 088
        public Weight_c,            // 098
        public Destructible_c,      // 0A8
        public Message_Icon_c,      // 0B8
        public Grab_Sounds_c,       // 0D0
        public Keywords_c           // 0E8
    {
    public:
        enum
        {
            SCRIPT_TYPE = Script_Type_e::MISC,
        };

        static constexpr const char* SCRIPT_NAME = "MiscObject";

    public:
        class Offset_e :
            public Enum_t<Word_t>
        {
        public:
            enum enum_type : value_type
            {
                RTTI = 0x01E14BB8, // 513921
            };

        public:
            using Enum_t::Enum_t;
        };

    public:
        virtual ~Misc_t(); // 00

    public:
        Bool_t  Is_Animal_Hide() const;
        Bool_t  Is_Animal_Part() const;
        Bool_t  Is_Child_Clothing() const;
        Bool_t  Is_Firewood() const;
        Bool_t  Is_Gem() const;
        Bool_t  Is_Ore_Or_Ingot() const;
        Bool_t  Is_Tool() const;

        Bool_t  Is_Daedric_Artifact() const;

    public:
        void Log(std::string indent = "");
    };
    STATIC_ASSERT(sizeof(Misc_t) == 0x100);

}
