namespace ActionGameEngine.Enum
{
    //only really used in the movelist, it tells what the character can cancel into
    //labels are arbitrary, you can make them mean anything you want
    public enum CancelConditions : int
    {

        GROUNDED = 1 << 0,
        AIRBORNE = 1 << 1,
        JUMP = 1 << 2,
        DASH = 1 << 3,
        GUARD = 1 << 4,
        GRAB = 1 << 5,
        INSTALLED = 1 << 6,
        STANCED = 1 << 7,
        BURST = 1 << 8,
        //For any unique mechanic that a character/game might have
        UNIQUE_COND_1 = 1 << 9,
        UNIQUE_COND_2 = 1 << 10,
        UNIQUE_COND_3 = 1 << 11,
        SUPER_LV1 = 1 << 12,
        SUPER_LV2 = 1 << 13,
        SPCL_LV1 = 1 << 14,
        SPCL_LV2 = 1 << 15,
        SPCL_LV3 = 1 << 16,
        SPCL_LV4 = 1 << 17,
        SPCL_LV5 = 1 << 18,
        SPCL_LV6 = 1 << 19,
        CMDNORM_LV1 = 1 << 20,
        CMDNORM_LV2 = 1 << 21,
        CMDNORM_LV3 = 1 << 22,
        CMDNORM_LV4 = 1 << 23,
        CMDNORM_LV5 = 1 << 24,
        CMDNORM_LV6 = 1 << 25,
        NORM_LV1 = 1 << 26,
        NORM_LV2 = 1 << 27,
        NORM_LV3 = 1 << 28,
        NORM_LV4 = 1 << 29,
        NORM_LV5 = 1 << 30,
        NORM_LV6 = 1 << 31,
        UNIQUE = UNIQUE_COND_1 | UNIQUE_COND_2 | UNIQUE_COND_3,
        SUPER = SUPER_LV1 | SUPER_LV2,
        SPECIAL = SPCL_LV1 | SPCL_LV2 | SPCL_LV3 | SPCL_LV4 | SPCL_LV5 | SPCL_LV6,
        COMMAND_NORMAL = CMDNORM_LV1 | CMDNORM_LV2 | CMDNORM_LV3 | CMDNORM_LV4 | CMDNORM_LV5 | CMDNORM_LV6,
        NORMAL = NORM_LV1 | NORM_LV2 | NORM_LV3 | NORM_LV4 | NORM_LV5 | NORM_LV6,
    }

}
