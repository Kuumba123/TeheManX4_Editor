using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeheManX4
{
    static class Const
    {
        public static readonly string reproURL = "https://api.github.com/repos/Kuumba123/TeheManX4_Editor/releases/latest";
        public const string Version = "1.1.1";
        public static readonly string[] pastVersions =
        {
            "1.1.1",
            "1.1",
            "1.0"
        };
        public const byte FilesCount = 40;
        public const int MaxUndo = 512;
        public const long Crc = 0x8E9397EA;
        public static readonly int[] CordTabe =
        {
            0x1000140,0x10003C0,0x180,0x240,
            0x280,0x2C0,0xB00140,0x1000340,
            0x1000380,0x1C0,0x200
        };
        public static readonly byte[] MaxClutAnimes =
        {
            9,2,5,4,2,8,1,3,5,6,3,4,1,3,0xFF,2,0xFF,1,0,0xFF,2,0xFF,3,3,2,2,0
        };
        public static readonly byte[] MaxTriggers =
            {5,
            4,
            10,
            9,
            2,
            6,
            2,
            0,
            5,
            7,
            0xFF,
            0,
            0xFF,
            3,
            2,
            7,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            5,
            0xFF,
            0xFF,
            10,
            0,  //Refights
            0xFF};
        public const uint ClutInfoPointersAddress = 0x8010b3c0;
        public const uint ClutDestPointersAddress = 0x8010b354;
        public const uint ArcBufferAddress = 0x80178000;
        public const uint LayoutBufferAddress = 0x80141be8;
        public const int MaxCameraTriggerDataSize = 0x66C; //in bytes
        public const uint CameraTriggerFreeDataAddress = 0x8010a7a0; //Start of Data that should be Dumped at
        public const uint CameraTriggerPointersAddress = 0x8010ae0c;
        public const uint BorderSettingsAddress = 0x8010ae74;
        public const int MaxHeight = 1760;
        public const int MaxLayoutSize = 0x4004; //in bytes
        public const uint CheckPointPointersAddress = 0x800f42b4;
        public const uint MushroomSpecialSpawnTableAddress = 0x800f8b50;
        public const uint PeacockSpecialSpawnTableAddress = 0x800f8b60;
        public const uint RefightsSpecialSpawnTableAddress = 0x800f8b6c;
        public const int ObjectSlotPointersOffset = 0xE4B40;
        public static readonly byte[] MaxPoints =
        {
            2 , 2 , 4 , 5 , 4 , 3 , 3 , 3 , 1 , 3 , 0 , 1 , 5 , 1 , 2 , 3 , 2 , 3 , 0 , 0xFF , 2 , 0xFF , 1 , 2 , 18, 0 , 0 , 0 , 0 , 0, 0 , 0
        };
        public static int LayoutSizeOffset = 0x100864;
        public static int LayoutDataPointersOffset = 0x1007DC;
        public static uint UpdateClutAddress = 0x80166bb0;
        public static uint UpdateLayer1Address = 0x801419fc;
        public static uint ClearLevelAddress = 0x801721cf;
        public static uint CheckPointAddress = 0x801721dd;
        public static uint LayoutWidthAddress = 0x80172224;
        public static uint LayoutHeightAddress = 0x80173a28;
        public static uint LayoutSizeAddress = 0x8013bd48;
        public static uint TransSettingsAddress = 0x800f30d4;
        public static int EnemyDataPointersOffset = 0xE4BC8;
        public static int StartEnemyDataPointersOffset = 0xE4C30;
        public const uint BackgroundSettingsAddress = 0x800f3188;
        public const uint BackgroundTypeTableAddress = 0x800f32d4;
        public static readonly ushort[] MaxEnemies = new ushort[]
        {
            143,    //Intro
            144,    //Spider
            335,    //Walrus
            125,    //Mushroom
            102,    //Dragoon
            195,    //StingRay
            129,    //PeaCock
            121,    //Owl
            207,    //Beast
            2,      //Colonel
            60,
            78,     //Final Weapon
            46      //Refights
        };

        public static Dictionary<byte, (string name, string info)> mainObjInfo = new Dictionary<byte, (string name, string info)>()
        {
            {0, ("Dragon","") },
            {2, ("Item-Carrier","") },
            {3, ("Spike Marl","") },
            {5, ("Bees","") },
            {6, ("Mad Bull","") },
            {7, ("Trap Blast (Claws)","") },
            {8, ("Dragon (Boss)","") },
            {0xA, ("Tonboroid S","") },
            {0xB, ("Fly Guner","") },
            {0xC, ("Kill Fisher","") },
            {0xD, ("Yeti (E-AT)","") },
            {0xE, ("Ice Wing","") },
            {0xF, ("Mets","") },
            {0x10, ("King Poseidon","") },
            {0x11, ("Snowman (Yukidarubon)","") },
            {0x12,("Eyezard (mid-boss)","") },
            {0x13,("Octo Battery","") },
            {0x14,("Ice Blox (Blaster inside","") },
            {0x15,("Icicle","") },
            {0x16,("Bee Hive","") },
            {0x17,("Knot S","") },
            {0x18,("Dejira","") },
            {0x19,("Snake (Obiiru)","") },
            {0x1A,("Ice Block","") },
            {0x1B,("Guard","") },
            {0x1C,("Baby Spider (from Core)","") },
            {0x1D,("Spider Core","") },
            {0x1E,("Togerics (Bud)","") },
            {0x1F,("Togerics (Thorned)","") },
            {0x20, ("Bats","") },
            {0x21,("Tentoroid (Mid-Boss)","") },
            {0x22,("Knot (Train version)","") },
            {0x23,("Spikey","") },
            {0x24,("Miru Toraeru","") },
            {0x25,("Tentoroid RS/BS","") },
            {0x26,("Plasma Cannon","") },
            {0x27,("Tentoroid BS (Down)","") },
            {0x28,("Bustable Walls","") },
            {0x29,("Ball","") },
            {0x2A,("TriScan","") },
            {0x2B,("Web Spider","") },
            {0x2C,("Floating Spike","") },
            {0x2D,("DG-42L (Body)","") },
            {0x2E,("DG-42L (Cannon)","") },
            {0x2F,("DG-42L (Train Mid-Boss)","") },
            {0x30,("Hover Gunner","") },
            {0x31,("Knots","Use the same slot as the Train splitting Knot!\nRef ID: 22") },
            {0x32,("Boxes (in Slash Beast)","\nRef ID: 28") },
            {0x33,("Giga Death","") },
            {0x34,("Walk Shooter","") },
            {0x35,("Biker","") },
            {0x36,("Slash Beast","") },
            {0x37,("Jet StingRay (Flying)","") },
            {0x38,("Jet StingRay","Different Ref ID from the other version") },
            {0x39,("Frost Walrus","") },
            {0x3A,("Beam Cannon","") },
            {0x3B,("General Explosion?","") },
            {0x3C,("Storm Owl","") },
            {0x3D,("Split Mushroom","") },
            {0x3E,("Fire Inferno","") },
            {0x3F,("UFO","") },
            {0x40,("Cyber Peacock","") },
            {0x41,("Magma Dragoon","") },
            {0x42,("Iris","") },
            {0x43,("Metal Hawk (Blue Ship)","") },
            {0x44,("Sigma (1st/2nd Phase)","") },
            {0x45,("Colonel","") },
            {0x46,("Generaid Core","") },
            {0x47,("Raiden (Ride-Armor)","") },
            {0x49,("Double","") },
            {0x4A,("Gunner/Earth Sigma","") },
            {0x4B,("General","") },
            {0x4C,("MM1 Floor Enemies","") }
        };
        public static Dictionary<byte, (string name, string info)> itemObjInfo = new Dictionary<byte, (string name, string info)>()
        {
            {2,("Items","You get a different Item depending on the Var setting to.\n0 = Small-Health\n1 = Big-Health\n2 = Small-Ammo\n3 = Big-Ammo\n4 = 1UP\n5 = Full-Health\n6 = Full-Ammo\n7-E = Hearts\nF-10 = E-Tank\n11 = W-Tank\n12 = EX-Tank") },
            {8,("Boss Gate","\nRef ID: 80") },
            {0xD,("Vertical Door","\nRef ID: 81") },
            {0x1A, ("Capsule","You get a different Upgrade depending on the Var setting to.\nRef ID: A2\n0 = Helmet\n1 = Body\n2 = Arm (Multi Shot)\n3 = Arm (Plasma)\n4 = Leg") },
            {0x1B, ("Refights Teleporter","Ref ID: 88") }
        };
        public static Dictionary<byte, (string name, string info)> effectObjInfo = new Dictionary<byte, (string name, string info)>()
        {
            {0,("Border Change","") },
            {3,("Clut Anime","") },
            {5,("Slippery Slopes","Should be placed as an Start Enemy") },
            {6,("Checkpoint","Lower  4 bits is the new Checkpoint Id") },
            {0x1E, ("Ride Armor Spawner","Set the Var Const. to 1 for the Eagle Armor\nRef ID: 86") }
        };
        public const uint FileDataAddress = 0x800f0e18;
        public static readonly string CachName = "CACH.BIN";
        public static readonly uint[] c_edc_lut = new uint[]{
    0x00000000, 0x90910101, 0x91210201, 0x01b00300, 0x92410401, 0x02d00500, 0x03600600, 0x93f10701,
    0x94810801, 0x04100900, 0x05a00a00, 0x95310b01, 0x06c00c00, 0x96510d01, 0x97e10e01, 0x07700f00,
    0x99011001, 0x09901100, 0x08201200, 0x98b11301, 0x0b401400, 0x9bd11501, 0x9a611601, 0x0af01700,
    0x0d801800, 0x9d111901, 0x9ca11a01, 0x0c301b00, 0x9fc11c01, 0x0f501d00, 0x0ee01e00, 0x9e711f01,
    0x82012001, 0x12902100, 0x13202200, 0x83b12301, 0x10402400, 0x80d12501, 0x81612601, 0x11f02700,
    0x16802800, 0x86112901, 0x87a12a01, 0x17302b00, 0x84c12c01, 0x14502d00, 0x15e02e00, 0x85712f01,
    0x1b003000, 0x8b913101, 0x8a213201, 0x1ab03300, 0x89413401, 0x19d03500, 0x18603600, 0x88f13701,
    0x8f813801, 0x1f103900, 0x1ea03a00, 0x8e313b01, 0x1dc03c00, 0x8d513d01, 0x8ce13e01, 0x1c703f00,
    0xb4014001, 0x24904100, 0x25204200, 0xb5b14301, 0x26404400, 0xb6d14501, 0xb7614601, 0x27f04700,
    0x20804800, 0xb0114901, 0xb1a14a01, 0x21304b00, 0xb2c14c01, 0x22504d00, 0x23e04e00, 0xb3714f01,
    0x2d005000, 0xbd915101, 0xbc215201, 0x2cb05300, 0xbf415401, 0x2fd05500, 0x2e605600, 0xbef15701,
    0xb9815801, 0x29105900, 0x28a05a00, 0xb8315b01, 0x2bc05c00, 0xbb515d01, 0xbae15e01, 0x2a705f00,
    0x36006000, 0xa6916101, 0xa7216201, 0x37b06300, 0xa4416401, 0x34d06500, 0x35606600, 0xa5f16701,
    0xa2816801, 0x32106900, 0x33a06a00, 0xa3316b01, 0x30c06c00, 0xa0516d01, 0xa1e16e01, 0x31706f00,
    0xaf017001, 0x3f907100, 0x3e207200, 0xaeb17301, 0x3d407400, 0xadd17501, 0xac617601, 0x3cf07700,
    0x3b807800, 0xab117901, 0xaaa17a01, 0x3a307b00, 0xa9c17c01, 0x39507d00, 0x38e07e00, 0xa8717f01,
    0xd8018001, 0x48908100, 0x49208200, 0xd9b18301, 0x4a408400, 0xdad18501, 0xdb618601, 0x4bf08700,
    0x4c808800, 0xdc118901, 0xdda18a01, 0x4d308b00, 0xdec18c01, 0x4e508d00, 0x4fe08e00, 0xdf718f01,
    0x41009000, 0xd1919101, 0xd0219201, 0x40b09300, 0xd3419401, 0x43d09500, 0x42609600, 0xd2f19701,
    0xd5819801, 0x45109900, 0x44a09a00, 0xd4319b01, 0x47c09c00, 0xd7519d01, 0xd6e19e01, 0x46709f00,
    0x5a00a000, 0xca91a101, 0xcb21a201, 0x5bb0a300, 0xc841a401, 0x58d0a500, 0x5960a600, 0xc9f1a701,
    0xce81a801, 0x5e10a900, 0x5fa0aa00, 0xcf31ab01, 0x5cc0ac00, 0xcc51ad01, 0xcde1ae01, 0x5d70af00,
    0xc301b001, 0x5390b100, 0x5220b200, 0xc2b1b301, 0x5140b400, 0xc1d1b501, 0xc061b601, 0x50f0b700,
    0x5780b800, 0xc711b901, 0xc6a1ba01, 0x5630bb00, 0xc5c1bc01, 0x5550bd00, 0x54e0be00, 0xc471bf01,
    0x6c00c000, 0xfc91c101, 0xfd21c201, 0x6db0c300, 0xfe41c401, 0x6ed0c500, 0x6f60c600, 0xfff1c701,
    0xf881c801, 0x6810c900, 0x69a0ca00, 0xf931cb01, 0x6ac0cc00, 0xfa51cd01, 0xfbe1ce01, 0x6b70cf00,
    0xf501d001, 0x6590d100, 0x6420d200, 0xf4b1d301, 0x6740d400, 0xf7d1d501, 0xf661d601, 0x66f0d700,
    0x6180d800, 0xf111d901, 0xf0a1da01, 0x6030db00, 0xf3c1dc01, 0x6350dd00, 0x62e0de00, 0xf271df01,
    0xee01e001, 0x7e90e100, 0x7f20e200, 0xefb1e301, 0x7c40e400, 0xecd1e501, 0xed61e601, 0x7df0e700,
    0x7a80e800, 0xea11e901, 0xeba1ea01, 0x7b30eb00, 0xe8c1ec01, 0x7850ed00, 0x79e0ee00, 0xe971ef01,
    0x7700f000, 0xe791f101, 0xe621f201, 0x76b0f300, 0xe541f401, 0x75d0f500, 0x7460f600, 0xe4f1f701,
    0xe381f801, 0x7310f900, 0x72a0fa00, 0xe231fb01, 0x71c0fc00, 0xe151fd01, 0xe0e1fe01, 0x7070ff00,
};
        public static readonly byte[] c_ecc_f_lut = new byte[] {
    0x00, 0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e, 0x10, 0x12, 0x14, 0x16, 0x18, 0x1a, 0x1c, 0x1e,
    0x20, 0x22, 0x24, 0x26, 0x28, 0x2a, 0x2c, 0x2e, 0x30, 0x32, 0x34, 0x36, 0x38, 0x3a, 0x3c, 0x3e,
    0x40, 0x42, 0x44, 0x46, 0x48, 0x4a, 0x4c, 0x4e, 0x50, 0x52, 0x54, 0x56, 0x58, 0x5a, 0x5c, 0x5e,
    0x60, 0x62, 0x64, 0x66, 0x68, 0x6a, 0x6c, 0x6e, 0x70, 0x72, 0x74, 0x76, 0x78, 0x7a, 0x7c, 0x7e,
    0x80, 0x82, 0x84, 0x86, 0x88, 0x8a, 0x8c, 0x8e, 0x90, 0x92, 0x94, 0x96, 0x98, 0x9a, 0x9c, 0x9e,
    0xa0, 0xa2, 0xa4, 0xa6, 0xa8, 0xaa, 0xac, 0xae, 0xb0, 0xb2, 0xb4, 0xb6, 0xb8, 0xba, 0xbc, 0xbe,
    0xc0, 0xc2, 0xc4, 0xc6, 0xc8, 0xca, 0xcc, 0xce, 0xd0, 0xd2, 0xd4, 0xd6, 0xd8, 0xda, 0xdc, 0xde,
    0xe0, 0xe2, 0xe4, 0xe6, 0xe8, 0xea, 0xec, 0xee, 0xf0, 0xf2, 0xf4, 0xf6, 0xf8, 0xfa, 0xfc, 0xfe,
    0x1d, 0x1f, 0x19, 0x1b, 0x15, 0x17, 0x11, 0x13, 0x0d, 0x0f, 0x09, 0x0b, 0x05, 0x07, 0x01, 0x03,
    0x3d, 0x3f, 0x39, 0x3b, 0x35, 0x37, 0x31, 0x33, 0x2d, 0x2f, 0x29, 0x2b, 0x25, 0x27, 0x21, 0x23,
    0x5d, 0x5f, 0x59, 0x5b, 0x55, 0x57, 0x51, 0x53, 0x4d, 0x4f, 0x49, 0x4b, 0x45, 0x47, 0x41, 0x43,
    0x7d, 0x7f, 0x79, 0x7b, 0x75, 0x77, 0x71, 0x73, 0x6d, 0x6f, 0x69, 0x6b, 0x65, 0x67, 0x61, 0x63,
    0x9d, 0x9f, 0x99, 0x9b, 0x95, 0x97, 0x91, 0x93, 0x8d, 0x8f, 0x89, 0x8b, 0x85, 0x87, 0x81, 0x83,
    0xbd, 0xbf, 0xb9, 0xbb, 0xb5, 0xb7, 0xb1, 0xb3, 0xad, 0xaf, 0xa9, 0xab, 0xa5, 0xa7, 0xa1, 0xa3,
    0xdd, 0xdf, 0xd9, 0xdb, 0xd5, 0xd7, 0xd1, 0xd3, 0xcd, 0xcf, 0xc9, 0xcb, 0xc5, 0xc7, 0xc1, 0xc3,
    0xfd, 0xff, 0xf9, 0xfb, 0xf5, 0xf7, 0xf1, 0xf3, 0xed, 0xef, 0xe9, 0xeb, 0xe5, 0xe7, 0xe1, 0xe3,
};
        public static readonly byte[] c_ecc_b_lut = new byte[] {
        0x00, 0xf4, 0xf5, 0x01, 0xf7, 0x03, 0x02, 0xf6, 0xf3, 0x07, 0x06, 0xf2, 0x04, 0xf0, 0xf1, 0x05,
        0xfb, 0x0f, 0x0e, 0xfa, 0x0c, 0xf8, 0xf9, 0x0d, 0x08, 0xfc, 0xfd, 0x09, 0xff, 0x0b, 0x0a, 0xfe,
        0xeb, 0x1f, 0x1e, 0xea, 0x1c, 0xe8, 0xe9, 0x1d, 0x18, 0xec, 0xed, 0x19, 0xef, 0x1b, 0x1a, 0xee,
        0x10, 0xe4, 0xe5, 0x11, 0xe7, 0x13, 0x12, 0xe6, 0xe3, 0x17, 0x16, 0xe2, 0x14, 0xe0, 0xe1, 0x15,
        0xcb, 0x3f, 0x3e, 0xca, 0x3c, 0xc8, 0xc9, 0x3d, 0x38, 0xcc, 0xcd, 0x39, 0xcf, 0x3b, 0x3a, 0xce,
        0x30, 0xc4, 0xc5, 0x31, 0xc7, 0x33, 0x32, 0xc6, 0xc3, 0x37, 0x36, 0xc2, 0x34, 0xc0, 0xc1, 0x35,
        0x20, 0xd4, 0xd5, 0x21, 0xd7, 0x23, 0x22, 0xd6, 0xd3, 0x27, 0x26, 0xd2, 0x24, 0xd0, 0xd1, 0x25,
        0xdb, 0x2f, 0x2e, 0xda, 0x2c, 0xd8, 0xd9, 0x2d, 0x28, 0xdc, 0xdd, 0x29, 0xdf, 0x2b, 0x2a, 0xde,
        0x8b, 0x7f, 0x7e, 0x8a, 0x7c, 0x88, 0x89, 0x7d, 0x78, 0x8c, 0x8d, 0x79, 0x8f, 0x7b, 0x7a, 0x8e,
        0x70, 0x84, 0x85, 0x71, 0x87, 0x73, 0x72, 0x86, 0x83, 0x77, 0x76, 0x82, 0x74, 0x80, 0x81, 0x75,
        0x60, 0x94, 0x95, 0x61, 0x97, 0x63, 0x62, 0x96, 0x93, 0x67, 0x66, 0x92, 0x64, 0x90, 0x91, 0x65,
        0x9b, 0x6f, 0x6e, 0x9a, 0x6c, 0x98, 0x99, 0x6d, 0x68, 0x9c, 0x9d, 0x69, 0x9f, 0x6b, 0x6a, 0x9e,
        0x40, 0xb4, 0xb5, 0x41, 0xb7, 0x43, 0x42, 0xb6, 0xb3, 0x47, 0x46, 0xb2, 0x44, 0xb0, 0xb1, 0x45,
        0xbb, 0x4f, 0x4e, 0xba, 0x4c, 0xb8, 0xb9, 0x4d, 0x48, 0xbc, 0xbd, 0x49, 0xbf, 0x4b, 0x4a, 0xbe,
        0xab, 0x5f, 0x5e, 0xaa, 0x5c, 0xa8, 0xa9, 0x5d, 0x58, 0xac, 0xad, 0x59, 0xaf, 0x5b, 0x5a, 0xae,
        0x50, 0xa4, 0xa5, 0x51, 0xa7, 0x53, 0x52, 0xa6, 0xa3, 0x57, 0x56, 0xa2, 0x54, 0xa0, 0xa1, 0x55,
};
        public static readonly byte[] Sync = {
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
        public static readonly string[] DiscFiles = new string[]
        {
"LBA_CAPCOM_ARC",
"LBA_COL00_0X_ARC",
"LBA_COL00_0Z_ARC",
"LBA_COL00_1X_ARC",
"LBA_COL00_1Z_ARC",
"LBA_COL01_0X_ARC",
"LBA_COL01_0Z_ARC",
"LBA_COL01_1X_ARC",
"LBA_COL01_1Z_ARC",
"LBA_COL02_0X_ARC",
"LBA_COL02_0Z_ARC",
"LBA_COL02_1X_ARC",
"LBA_COL02_1Z_ARC",
"LBA_COL03_0X_ARC",
"LBA_COL03_0Z_ARC",
"LBA_COL03_1X_ARC",
"LBA_COL03_1Z_ARC",
"LBA_COL04_0X_ARC",
"LBA_COL04_0Z_ARC",
"LBA_COL04_1X_ARC",
"LBA_COL04_1Z_ARC",
"LBA_COL05_0X_ARC",
"LBA_COL05_0Z_ARC",
"LBA_COL05_1X_ARC",
"LBA_COL05_1Z_ARC",
"LBA_COL06_0X_ARC",
"LBA_COL06_0Z_ARC",
"LBA_COL06_1X_ARC",
"LBA_COL06_1Z_ARC",
"LBA_COL07_0X_ARC",
"LBA_COL07_0Z_ARC",
"LBA_COL07_1X_ARC",
"LBA_COL07_1Z_ARC",
"LBA_COL08_0X_ARC",
"LBA_COL08_0Z_ARC",
"LBA_COL08_1X_ARC",
"LBA_COL08_1Z_ARC",
"LBA_COL09_0X_ARC",
"LBA_COL09_0Z_ARC",
"LBA_COL0A_0X_ARC",
"LBA_COL0A_0Z_ARC",
"LBA_COL0B_0X_ARC",
"LBA_COL0B_0Z_ARC",
"LBA_COL0B_1X_ARC",
"LBA_COL0B_1Z_ARC",
"LBA_COL0C_0X_ARC",
"LBA_COL0C_0Z_ARC",
"LBA_COL0C_1X_ARC",
"LBA_COL0C_1Z_ARC",
"LBA_COL0D_0X_ARC",
"LBA_COL0D_0Z_ARC",
"LBA_COL0E_U0_ARC",
"LBA_COL0E_U1_ARC",
"LBA_COL0F_U0_ARC",
"LBA_COL0F_U1_ARC",
"LBA_COLD_1U1_ARC",
"LBA_COLD_1U2_ARC",
"LBA_COLD_1U3_ARC",
"LBA_COLD_1U4_ARC",
"LBA_COLD_1U5_ARC",
"LBA_COLD_1U6_ARC",
"LBA_COLD_1U7_ARC",
"LBA_COLD_1U8_ARC",
"LBA_FONT8X8_ARC",
"LBA_LOAD_U_ARC",
"LBA_MOJIPAT_ARC",
"LBA_ONPARE1_ARC",
"LBA_ONPARE2_ARC",
"LBA_ONPARE3_ARC",
"LBA_ONPARE4_ARC",
"LBA_ONPARE5_ARC",
"LBA_ONPARE6_ARC",
"LBA_ONPARE7_ARC",
"LBA_ONPARE8_ARC",
"LBA_PL00SEP_ARC",
"LBA_PL00_U_ARC",
"LBA_PL01SEP_ARC",
"LBA_PL01_U_ARC",
"LBA_PL02_U_ARC",
"LBA_PLDEMO_ARC",
"LBA_PLDEMO00_ARC",
"LBA_PLDEMO01_ARC",
"LBA_PLDEMO02_ARC",
"LBA_PLDEMO03_ARC",
"LBA_ST00_00_ARC",
"LBA_ST00_01_ARC",
"LBA_ST01_00_ARC",
"LBA_ST01_01_ARC",
"LBA_ST02_00_ARC",
"LBA_ST02_01_ARC",
"LBA_ST03_00_ARC",
"LBA_ST03_01_ARC",
"LBA_ST04_00_ARC",
"LBA_ST04_01_ARC",
"LBA_ST05_00_ARC",
"LBA_ST05_01_ARC",
"LBA_ST06_00_ARC",
"LBA_ST06_01_ARC",
"LBA_ST07_00_ARC",
"LBA_ST07_01_ARC",
"LBA_ST08_00_ARC",
"LBA_ST08_01_ARC",
"LBA_ST09_00_ARC",
"LBA_ST0A_00_ARC",
"LBA_ST0B_00_ARC",
"LBA_ST0B_01_ARC",
"LBA_ST0B_0X_ARC",
"LBA_ST0B_0Z_ARC",
"LBA_ST0C_00_ARC",
"LBA_ST0C_01_ARC",
"LBA_ST0C_U1_ARC",
"LBA_ST0D_0X_ARC",
"LBA_ST0D_0Z_ARC",
"LBA_ST0E_U0_ARC",
"LBA_ST0E_U1_ARC",
"LBA_ST0F_U1_ARC",
"LBA_ST0F_UX_ARC",
"LBA_ST0F_UZ_ARC",
"LBA_ST0_1_1_ARC",
"LBA_ST1_1_1_ARC",
"LBA_ST2_1_1_ARC",
"LBA_ST3_1_1_ARC",
"LBA_ST4_1_1_ARC",
"LBA_ST5_1_1_ARC",
"LBA_ST6_1_1_ARC",
"LBA_ST7_1_1_ARC",
"LBA_ST8_1_1_ARC",
"LBA_STA_0_1_ARC",
"LBA_STB_1_1_ARC",
"LBA_STC_1_1_ARC",
"LBA_STD_1_1U_ARC",
"LBA_STD_1_2U_ARC",
"LBA_STD_1_3U_ARC",
"LBA_STD_1_4U_ARC",
"LBA_STD_1_5U_ARC",
"LBA_STD_1_6U_ARC",
"LBA_STD_1_7U_ARC",
"LBA_STD_1_8U_ARC",
"LBA_CAPCOM20_STR",
"LBA_OP_U_STR",
"LBA_X1_U_STR",
"LBA_X2_U_STR",
"LBA_X3_U_STR",
"LBA_X4_U_STR",
"LBA_Z1_U_STR",
"LBA_Z2_U_STR",
"LBA_Z3_U_STR",
"LBA_Z4_U_STR",
"LBA_Z5_U_STR",
"LBA_BGM1_U_XA",
"LBA_BGM2_XA",
"LBA_BGM3_XA",
"LBA_BGM4_XA",
"LBA_BGM5_U_XA",
"LBA_BOSINT_U_XA",
"LBA_VOICE1_U_XA",
"LBA_VOICE2_U_XA",
"LBA_VOICE3_U_XA",
"LBA_VOICE4_U_XA",
"LBA_VOICE5_U_XA"
        };
        public static List<Color> GreyScale = new List<Color>()
                            {

                                Color.FromRgb(0,0,0),
                                Color.FromRgb(0x10,0x10,0x10),
                                Color.FromRgb(0x20,0x20,0x20),
                                Color.FromRgb(0x30,0x30,0x30),
                                Color.FromRgb(0x40,0x40,0x40),
                                Color.FromRgb(0x50,0x50,0x50),
                                Color.FromRgb(0x60,0x60,0x60),
                                Color.FromRgb(0x70,0x70,0x70),
                                Color.FromRgb(0x88,0x88,0x88),
                                Color.FromRgb(0x98,0x98,0x98),
                                Color.FromRgb(0xA8,0xA8,0xA8),
                                Color.FromRgb(0xB8,0xB8,0xB8),
                                Color.FromRgb(0xC8,0xC8,0xC8),
                                Color.FromRgb(0xD8,0xD8,0xD8),
                                Color.FromRgb(0xE8,0xE8,0xE8),
                                Color.FromRgb(0xF8,0xF8,0xF8)
                            };
        public static BitmapPalette GreyScalePallete = new BitmapPalette(GreyScale);
        public const int EnemyOffset = 8; //For Enemy Labels
        public static readonly Typeface FONT = new Typeface("Consolas");
    }
}
