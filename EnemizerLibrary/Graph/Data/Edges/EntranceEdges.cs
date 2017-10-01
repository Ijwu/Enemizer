﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnemizerLibrary.Data
{
    public class EntranceEdges
    {
        OverworldNodes _overworldNodes;
        RoomNodes _roomNodes;
        public Dictionary<int, OverworldEntrance> _overworldEntrances;
        public Dictionary<string, Entrance> _entrances;

        public EntranceEdges(OverworldNodes overworldNodes, RoomNodes roomNodes)
        {
            this._overworldNodes = overworldNodes;
            this._roomNodes = roomNodes;
            FillEntranceEdges();
        }


        List<RawOverworldEntrance> _rawOverworldEntrances = new List<RawOverworldEntrance>()
        {
            { new RawOverworldEntrance(0xDB84C, "Skull Woods First Section Hole (North)", 0x040, "40-skull-woods", "0x76", "") },
            { new RawOverworldEntrance(0xDB84D, "Skull Woods First Section Hole (East)", 0x040, "40-skull-woods", "0x77", "") },
            { new RawOverworldEntrance(0xDB84F, "Skull Woods First Section Hole (West)", 0x040, "40-skull-woods", "0x78", "") },
            { new RawOverworldEntrance(0xDB851, "Skull Woods Second Section Hole", 0x040, "40-skull-woods", "0x79", "") },
            { new RawOverworldEntrance(0xDB853, "Thieves Forest Hideout Drop", 0x000, "00-lost-woods", "0x7A", "") },
            { new RawOverworldEntrance(0xDB854, "Pyramid Hole", 0x05B, "5B-pyramid", "0x7B", "<Agahnim 2>") },
            { new RawOverworldEntrance(0xDB857, "North Fairy Cave Drop", 0x015, "15-river-between-graveyard-witch", "0x7C", "") },
            { new RawOverworldEntrance(0xDB858, "Hyrule Castle Secret Entrance Drop", 0x01B, "1B-castle", "0x7D", "") },
            { new RawOverworldEntrance(0xDB859, "Bat Cave Drop", 0x022, "22-smithy", "0x7E", "Hammer;L2 Gloves,Mirror,Moon Pearl") },
            { new RawOverworldEntrance(0xDB85B, "Lumberjack Tree Drop", 0x002, "02-lumberjack-house", "0x7F", "<Agahnim 1>") },
            { new RawOverworldEntrance(0xDB85C, "Kakariko Well Drop", 0x018, "18-kakariko", "0x80", "") },
            { new RawOverworldEntrance(0xDB85E, "Sanctuary Grave", 0x014, "14-graveyard", "0x81", "Boots,L1 Gloves") },
            { new RawOverworldEntrance(0xDBB73, "Link's House", 0x02C, "2C-link-house", "0x01", "") },
            { new RawOverworldEntrance(0xDBB74, "Sanctuary", 0x013, "13-sanctuary", "0x02", "") },
            { new RawOverworldEntrance(0xDBB75, "Hyrule Castle Entrance (West)", 0x01B, "1B-castle", "0x03", "") },
            { new RawOverworldEntrance(0xDBB76, "Hyrule Castle Entrance (South)", 0x01B, "1B-castle", "0x04", "") },
            { new RawOverworldEntrance(0xDBB77, "Hyrule Castle Entrance (East)", 0x01B, "1B-castle", "0x05", "") },
            { new RawOverworldEntrance(0xDBB78, "Old Man Cave (West)", 0x00A, "0A-entrance-death-mountain", "0x06", "Lamp,L1 Gloves") },
            { new RawOverworldEntrance(0xDBB79, "Old Man Cave (East)", 0x003, "03-west-death-mountain-lower", "0x07", "Lamp") },
            { new RawOverworldEntrance(0xDBB7A, "Eastern Palace", 0x01E, "1E-eastern-palace", "0x08", "") },
            { new RawOverworldEntrance(0xDBB7B, "Desert Palace Entrance (South)", 0x030, "30-desert-palace-main-entrance", "0x09", "") },
            { new RawOverworldEntrance(0xDBB7C, "Desert Palace Entrance (East)", 0x030, "30-desert-palace-east-entrance", "0x0A", "") },
            { new RawOverworldEntrance(0xDBB7D, "Desert Palace Entrance (West)", 0x030, "30-desert-ledge", "0x0B", "") },
            { new RawOverworldEntrance(0xDBB7E, "Desert Palace Entrance (North)", 0x030, "30-desert-ledge-boss-entrance", "0x0C", "") },
            { new RawOverworldEntrance(0xDBB7F, "Elder House (West)", 0x018, "18-kakariko", "0x0D", "") },
            { new RawOverworldEntrance(0xDBB80, "Elder House (East)", 0x018, "18-kakariko", "0x0E", "") },
            { new RawOverworldEntrance(0xDBB81, "Two Brothers House (West)", 0x028, "28-kakariko-maze-race", "0x0F", "") },
            { new RawOverworldEntrance(0xDBB82, "Two Brothers House (East)", 0x029, "29-kakariko-library", "0x10", "") },
            { new RawOverworldEntrance(0xDBB83, "Bat Cave Cave", 0x022, "22-smithy", "0x11", "") },
            { new RawOverworldEntrance(0xDBB84, "Lumberjack Tree Cave", 0x002, "02-lumberjack-house", "0x12", "") },
            { new RawOverworldEntrance(0xDBB85, "Dark Death Mountain Ascend (Bottom)", 0x045, "45-dw-east-death-mountain", "0x13", "") },
            { new RawOverworldEntrance(0xDBB86, "Dark Death Mountain Ascend (Top)", 0x045, "45-dw-east-death-mountain", "0x14", "") },
            { new RawOverworldEntrance(0xDBB87, "Dark Death Mountain Turtle Rock Ledge (West)", 0x045, "45-dw-east-death-mountain-turtle-bridge", "0x15", "") },
            { new RawOverworldEntrance(0xDBB88, "Bumper Cave (Bottom)", 0x04A, "4A-bumper-cave", "0x16", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBB89, "Bumper Cave (Top)", 0x04A, "4A-bumper-cave", "0x17", "") },
            { new RawOverworldEntrance(0xDBB8A, "Turtle Rock Isolated Ledge Entrance", 0x045, "45-dw-east-death-mountain-turtle-isolated", "0x18", "") },
            { new RawOverworldEntrance(0xDBB8B, "Dark Death Mountain Turtle Rock Ledge (East)", 0x045, "45-dw-east-death-mountain-turtle-bridge", "0x19", "") },
            { new RawOverworldEntrance(0xDBB8C, "Fairy Ascension Cave (Bottom)", 0x005, "05-east-death-mountain", "0x1A", "") },
            { new RawOverworldEntrance(0xDBB8D, "Fairy Ascension Cave (Top)", 0x005, "05-east-death-mountain", "0x1B", "") },
            { new RawOverworldEntrance(0xDBB8E, "Spiral Cave (Bottom)", 0x005, "05-east-death-mountain", "0x1C", "") },
            { new RawOverworldEntrance(0xDBB8F, "Spiral Cave", 0x005, "05-east-death-mountain", "0x1D", "") },
            { new RawOverworldEntrance(0xDBB90, "7 Chest Cave (Bottom)", 0x005, "05-east-death-mountain", "0x1E", "") },
            { new RawOverworldEntrance(0xDBB91, "7 Chest Cave (Middle)", 0x005, "05-east-death-mountain", "0x1F", "") },
            { new RawOverworldEntrance(0xDBB92, "7 Chest Cave (Top)", 0x005, "05-east-death-mountain", "0x20", "") },
            { new RawOverworldEntrance(0xDBB93, "Spectacle Rock Cave (Bottom)", 0x003, "03-west-death-mountain-lower", "0x21", "") },
            { new RawOverworldEntrance(0xDBB94, "Spectacle Rock Cave", 0x003, "03-west-death-mountain-lower", "0x22", "") },
            { new RawOverworldEntrance(0xDBB95, "Spectacle Rock Cave Peak", 0x003, "03-west-death-mountain-lower", "0x23", "") },
            { new RawOverworldEntrance(0xDBB96, "Agahnims Tower", 0x01B, "1B-castle", "0x24", "L2 Sword;Cape") },
            { new RawOverworldEntrance(0xDBB97, "Swamp Palace", 0x07B, "7B-dw-southwest-swamp", "0x25", "") },
            { new RawOverworldEntrance(0xDBB98, "Palace of Darkness", 0x05E, "5E-palace-of-darkness", "0x26", "") },
            { new RawOverworldEntrance(0xDBB99, "Misery Mire", 0x070, "70-mire", "0x27", "<Misery Mire Token>") },
            { new RawOverworldEntrance(0xDBB9A, "Skull Woods Second Section Door (West)", 0x040, "40-skull-woods", "0x28", "") },
            { new RawOverworldEntrance(0xDBB9B, "Skull Woods Second Section Door (East)", 0x040, "40-skull-woods", "0x29", "") },
            { new RawOverworldEntrance(0xDBB9C, "Skull Woods First Section Door", 0x040, "40-skull-woods", "0x2A", "") },
            { new RawOverworldEntrance(0xDBB9D, "Skull Woods Final Section", 0x040, "40-skull-woods", "0x2B", "Fire Rod") },
            { new RawOverworldEntrance(0xDBB9E, "Thieves Forest Hideout Stump", 0x000, "00-lost-woods", "0x2C", "") },
            { new RawOverworldEntrance(0xDBB9F, "Ice Palace", 0x075, "75-dw-lake-hylia-ice-palace", "0x2D", "") },
            { new RawOverworldEntrance(0xDBBA0, "Death Mountain Return Cave (West)", 0x00A, "0A-entrance-death-mountain", "0x2E", "Lamp") },
            { new RawOverworldEntrance(0xDBBA1, "Death Mountain Return Cave (East)", 0x003, "03-west-death-mountain-lower", "0x2F", "Lamp") },
            { new RawOverworldEntrance(0xDBBA2, "Old Man House (Bottom)", 0x003, "03-west-death-mountain-lower", "0x30", "") },
            { new RawOverworldEntrance(0xDBBA3, "Old Man House (Top)", 0x003, "03-west-death-mountain-lower", "0x31", "") },
            { new RawOverworldEntrance(0xDBBA4, "Hyrule Castle Secret Entrance Stairs", 0x01B, "1B-castle", "0x32", "") },
            { new RawOverworldEntrance(0xDBBA5, "Tower of Hera", 0x003, "03-west-death-mountain-upper", "0x33", "") },
            { new RawOverworldEntrance(0xDBBA6, "Thieves Town", 0x058, "58-outcast-village", "0x34", "") },
            { new RawOverworldEntrance(0xDBBA7, "Turtle Rock", 0x047, "47-turtle-rock", "0x35", "<Turtle Rock Token>") },
            { new RawOverworldEntrance(0xDBBA8, "Pyramid Entrance", 0x05B, "5B-pyramid", "0x36", "") },
            { new RawOverworldEntrance(0xDBBA9, "Ganons Tower", 0x043, "43-dw-west-death-mountain-upper", "0x37", "<Crystal 1>,<Crystal 2>,<Crystal 3>,<Crystal 4>,<Crystal 5>,<Crystal 6>,<Crystal 7>") },
            { new RawOverworldEntrance(0xDBBAA, "North Fairy Cave", 0x015, "15-river-between-graveyard-witch", "0x38", "") },
            { new RawOverworldEntrance(0xDBBAB, "Kakariko Well Cave", 0x018, "18-kakariko", "0x39", "") },
            { new RawOverworldEntrance(0xDBBAC, "Hookshot Cave", 0x045, "45-dw-east-death-mountain", "0x3A", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBBAD, "Hookshot Cave Back Entrance", 0x045, "45-dw-east-death-mountain", "0x3B", "") },
            { new RawOverworldEntrance(0xDBBAE, "Lost Woods Gamble", 0x000, "00-lost-woods", "0x3C", "") },
            { new RawOverworldEntrance(0xDBBAF, "Dark Swamp Cave", 0x074, "74-dw-northeast-swamp", "0x3D", "") },
            { new RawOverworldEntrance(0xDBBB0, "Snitch Lady (East)", 0x018, "18-kakariko", "0x3E", "") },
            { new RawOverworldEntrance(0xDBBB1, "Snitch Lady (West)", 0x018, "18-kakariko", "0x3F", "") },
            { new RawOverworldEntrance(0xDBBB2, "Sick Kids House", 0x018, "18-kakariko", "0x40", "") },
            { new RawOverworldEntrance(0xDBBB3, "Spike Cave", 0x043, "43-dw-west-death-mountain-lower", "0x41", "") },
            { new RawOverworldEntrance(0xDBBB4, "Tavern (Front)", 0x018, "18-kakariko", "0x42", "") },
            { new RawOverworldEntrance(0xDBBB5, "Tavern North", 0x018, "18-kakariko", "0x43", "") },
            { new RawOverworldEntrance(0xDBBB6, "Bush Covered House", 0x018, "18-kakariko", "0x44", "") },
            { new RawOverworldEntrance(0xDBBB7, "Sahasrahlas Hut", 0x01E, "1E-eastern-palace", "0x45", "") },
            { new RawOverworldEntrance(0xDBBB8, "Kakariko Shop", 0x018, "18-kakariko", "0x46", "") },
            { new RawOverworldEntrance(0xDBBB9, "Chest Game", 0x058, "58-outcast-village", "0x47", "") },
            { new RawOverworldEntrance(0xDBBBA, "Doorless Hut", 0x058, "58-outcast-village", "0x48", "") },
            { new RawOverworldEntrance(0xDBBBB, "Library", 0x029, "29-kakariko-library", "0x49", "") },
            { new RawOverworldEntrance(0xDBBBC, "Light World Bomb Hut", 0x018, "18-kakariko", "0x4A", "") },
            { new RawOverworldEntrance(0xDBBBD, "Chicken House", 0x018, "18-kakariko", "0x4B", "") },
            { new RawOverworldEntrance(0xDBBBE, "Witch Hut", 0x016, "16-witch-hut", "0x4C", "") },
            //{ new RawOverworldEntrance(0xDBBBF, "Aginah's Cave", 0x030, "30-desert", "0x4D", "") }, // ER doesn't use this copy
            { new RawOverworldEntrance(0xDBBC0, "Dam", 0x03B, "3B-southwest-swamp", "0x4E", "") },
            { new RawOverworldEntrance(0xDBBC1, "Mimic Cave Mirror Spot", 0x005, "05-east-death-mountain", "0x4F", "") },
            { new RawOverworldEntrance(0xDBBC2, "Hookshot Fairy", 0x005, "05-east-death-mountain", "0x50", "") },
            { new RawOverworldEntrance(0xDBBC3, "Cave South of Haunted Grove", 0x032, "32-south-of-haunted-grove-ledge", "0x51", "") },
            { new RawOverworldEntrance(0xDBBC4, "Graveyard Cave", 0x014, "14-graveyard-ledge", "0x52", "") },
            { new RawOverworldEntrance(0xDBBC5, "Big Bomb Shop", 0x06C, "6C-bomb-shop", "0x53", "") },
            { new RawOverworldEntrance(0xDBBC6, "C-Shaped House", 0x058, "58-outcast-village", "0x54", "") },
            { new RawOverworldEntrance(0xDBBC7, "Long Fairy Cave", 0x02F, "2F-south-eastern-palace-right", "0x55", "") },
            { new RawOverworldEntrance(0xDBBC8, "Dark Desert Fairy", 0x070, "70-mire", "0x5Ea", "") },
            { new RawOverworldEntrance(0xDBBC9, "Dark World Lumberjack Shop", 0x042, "42-dw-lumberjack-house", "0x60a", "") },
            { new RawOverworldEntrance(0xDBBCA, "Cave Shop (Lake Hylia)", 0x035, "35-lake-hylia", "0x58a", "") },
            { new RawOverworldEntrance(0xDBBCB, "Archery Game", 0x069, "69-frog-smith", "0x59", "") },
            { new RawOverworldEntrance(0xDBBCC, "Dark Sanctuary Hint", 0x053, "53-dw-sanctuary", "0x5A", "") },
            { new RawOverworldEntrance(0xDBBCD, "Kings Grave", 0x014, "14-graveyard-kings-tomb", "0x5B", "Boots") },
            { new RawOverworldEntrance(0xDBBCE, "Waterfall of Wishing", 0x00F, "0F-entrance-zora-domain", "0x5C", "Flippers") },
            { new RawOverworldEntrance(0xDBBCF, "Capacity Upgrade", 0x035, "35-lake-hylia", "0x5D", "Flippers") },
            { new RawOverworldEntrance(0xDBBD0, "Lake Hylia Fairy", 0x02E, "2E-south-eastern-palace-left", "0x5Eb", "") },
            { new RawOverworldEntrance(0xDBBD1, "Dark Desert Cave", 0x070, "70-mire", "0x5F", "") },
            { new RawOverworldEntrance(0xDBBD2, "Dark World Shop", 0x058, "58-outcast-village", "0x60b", "") },
            { new RawOverworldEntrance(0xDBBD3, "Thiefs Hut", 0x018, "18-kakariko", "0x61", "") },
            { new RawOverworldEntrance(0xDBBD4, "Dark Desert Hint", 0x070, "70-mire", "0x62", "") },
            { new RawOverworldEntrance(0xDBBD5, "Pyramid Fairy", 0x05B, "5B-pyramid", "0x63", "<Big Bomb>") },
            { new RawOverworldEntrance(0xDBBD6, "Blacksmiths Hut", 0x022, "22-smithy", "0x64", "") },
            { new RawOverworldEntrance(0xDBBD7, "Fortune Teller (Light)", 0x011, "11-kakariko-fortune-teller", "0x65a", "") },
            { new RawOverworldEntrance(0xDBBD8, "Fortune Teller (Dark)", 0x051, "51-outcast-fortune-teller", "0x66", "") },
            { new RawOverworldEntrance(0xDBBD9, "Kakariko Gamble Game", 0x029, "29-kakariko-library", "0x67", "") },
            { new RawOverworldEntrance(0xDBBDA, "Palace of Darkness Hint", 0x05E, "5E-palace-of-darkness", "0x68", "") },
            { new RawOverworldEntrance(0xDBBDB, "East Dark World Hint", 0x06F, "6F-south-pod-right", "0x69", "") },
            { new RawOverworldEntrance(0xDBBDC, "Dark Lake Hylia Ledge Hint", 0x077, "77-dw-ice-cave", "0x6A", "") },
            { new RawOverworldEntrance(0xDBBDD, "Good Bee Cave", 0x037, "37-ice-cave", "0x56", "") },
            { new RawOverworldEntrance(0xDBBDE, "Swamp Fairy", 0x034, "34-northeast-swamp", "0x5Ec", "") },
            { new RawOverworldEntrance(0xDBBDF, "Dark Lake Hylia Fairy", 0x06E, "6E-south-pod-left", "0x5Ed", "") },
            { new RawOverworldEntrance(0xDBBE0, "Cave Shop (Dark Death Mountain)", 0x045, "45-dw-east-death-mountain", "0x58b", "") },
            { new RawOverworldEntrance(0xDBBE1, "Dark World Potion Shop", 0x056, "56-dw-witch-hut", "0x60c", "") },
            { new RawOverworldEntrance(0xDBBE2, "Dark Death Mountain Fairy", 0x043, "43-dw-west-death-mountain-lower", "0x5Ee", "") },
            { new RawOverworldEntrance(0xDBBE3, "Aginahs Cave", 0x030, "30-desert", "0x4D", "") },
            { new RawOverworldEntrance(0xDBBE4, "Desert Fairy", 0x03A, "3A-between-desert-swamp", "0x5Ef", "") },
            { new RawOverworldEntrance(0xDBBE5, "Lake Hylia Fortune Teller", 0x035, "35-lake-hylia", "0x65b", "") },
            { new RawOverworldEntrance(0xDBBE6, "Dark Lake Hylia Shop", 0x075, "75-dw-lake-hylia-upper", "0x60d", "") },
            { new RawOverworldEntrance(0xDBBE7, "Red Shield Shop", 0x05A, "5A-between-outcast-pyramid", "0x57", "") },
            { new RawOverworldEntrance(0xDBBE8, "Lumberjack House", 0x002, "02-lumberjack-house", "0x6B", "") },
            { new RawOverworldEntrance(0xDBBE9, "Bonk Fairy (Light)", 0x02B, "2B-between-haunted-link-house", "0x71a", "Boots") },
            { new RawOverworldEntrance(0xDBBEA, "Bonk Fairy (Dark)", 0x06B, "6B-between-grove-bomb-shop", "0x71b", "Boots") },
            { new RawOverworldEntrance(0xDBBEB, "50 Rupee Cave", 0x03A, "3A-between-desert-swamp", "0x6D", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBBEC, "Bonk Rock Cave", 0x013, "13-sanctuary", "0x6E", "Boots") },
            { new RawOverworldEntrance(0xDBBED, "20 Rupee Cave", 0x037, "37-ice-cave", "0x6F", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBBEE, "Dark Lake Hylia Ledge Spike Cave", 0x077, "77-dw-ice-cave", "0x70", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBBEF, "Mini Moldorm Cave", 0x035, "35-lake-hylia", "0x6C", "") },
            { new RawOverworldEntrance(0xDBBF0, "Checkerboard Cave", 0x030, "30-desert-checkerboard-ledge", "0x72", "L1 Gloves") },
            { new RawOverworldEntrance(0xDBBF1, "Dark World Hammer Peg Cave", 0x062, "62-dw-smith-house", "0x83", "Hammer") },
            { new RawOverworldEntrance(0xDBBF2, "Ice Cave", 0x037, "37-ice-cave", "0x84", "") },
            { new RawOverworldEntrance(0xDBBF3, "Dark Lake Hylia Ledge Fairy", 0x077, "77-dw-ice-cave", "0x5Eg", "") },
        };

        List<RawEntrance> _rawEntrances = new List<RawEntrance>()
        {
            { new RawEntrance("0x01", 0x01, "Link's House (Door)", 0x104, "Link's House", "cave-links-house") },
            { new RawEntrance("0x02", 0x02, "Sanctuary Exit (Main Entrance)", 0x012, "Sanctuary", "sanctuary") },
            { new RawEntrance("0x03", 0x03, "Hyrule Castle Exit (West) (Left Entrance)", 0x060, "Hyrule Castle (West Entrance Room)", "hyrule-west-entrance") },
            { new RawEntrance("0x04", 0x04, "Hyrule Castle Exit (South) (Main Entrance)", 0x061, "Hyrule Castle (Main Entrance Room)", "hyrule-entrance") },
            { new RawEntrance("0x05", 0x05, "Hyrule Castle Exit (East) (Right Entrance)", 0x062, "Hyrule Castle (East Entrance Room)", "hyrule-east-entrance") },
            { new RawEntrance("0x06", 0x06, "Old Man Cave Exit (West) (Lower Entrance)", 0x0F0, "Cave (Lost Old Man Starting Cave Entrance)", "cave-dm-entrance") },
            { new RawEntrance("0x07", 0x07, "Old Man Cave Exit (East) (Upper Entrance)", 0x0F1, "Cave (Lost Old Man Starting Cave Exit)", "cave-dm-entrance-exit") },
            { new RawEntrance("0x08", 0x08, "Eastern Palace Exit (Main Entrance)", 0x0C9, "Eastern Palace (Entrance Room)", "eastern-entrance") },
            { new RawEntrance("0x09", 0x09, "Desert Palace Exit (South) (Main Entrance)", 0x084, "Desert Palace (Main Entrance Room)", "desert-main-entrance") },
            { new RawEntrance("0x0A", 0x0A, "Desert Palace Exit (East) (Right Entrance)", 0x085, "Desert Palace (East Entrance Room)", "desert-east-entrance-hall") },
            { new RawEntrance("0x0B", 0x0B, "Desert Palace Exit (West) (Left Entrance)", 0x083, "Desert Palace (West Entrance Room)", "desert-west-entrance") },
            { new RawEntrance("0x0C", 0x0C, "Desert Palace Exit (North) (Boss Entrance)", 0x063, "Desert Palace (Final Section Entrance Room)", "desert-boss-entrance") },
            { new RawEntrance("0x0D", 0x0D, "Elder House Exit (West) (Left Entrance)", 0x0F2, "House (Old Woman Next Door)", "cave-old-lady-left") },
            { new RawEntrance("0x0E", 0x0E, "Elder House Exit (East) (Right Entrnace)", 0x0F3, "House (Old Woman (Sahasrahla's Wife?))", "cave-old-lady-right") },
            { new RawEntrance("0x0F", 0x0F, "Two Brothers House Exit (West) (Left Entrance)", 0x0F4, "House (Angry Brothers Exit to Maze Game)", "cave-angry-brothers-exit") },
            { new RawEntrance("0x10", 0x10, "Two Brothers House Exit (East) (Right Entrance)", 0x0F5, "House (Angry Brothers Entrance)", "cave-angry-brothers-entrance") },
            { new RawEntrance("0x11", 0x11, "Bat Cave Exit (Main Entrance)", 0x0E3, "Cave (1/2 Magic)", "cave-magic-bat") },
            { new RawEntrance("0x12", 0x12, "Lumberjack Tree Exit (Cave Entrance)", 0x0E2, "Cave (Lumberjack's Tree HP)", "cave-lumberjack-tree") },
            { new RawEntrance("0x13", 0x13, "Dark Death Mountain Ascend Exit (Bottom)", 0x0F8, "Super Bunny Cave Entrance and Chests", "cave-super-bunny-entrance") },
            { new RawEntrance("0x14", 0x14, "Dark Death Mountain Ascend Exit (Top)", 0x0E8, "Super Bunny Cave 'exit'", "cave-super-bunny-exit") },
            { new RawEntrance("0x15", 0x15, "Turtle Rock Ledge Exit (West) (Bomb Cave Entrance 5 Lazer Eyes)", 0x023, "Turtle Rock (West Exit to Balcony)", "turtle-lazer-exit") },
            { new RawEntrance("0x16", 0x16, "Bumper Cave Exit (Bottom)", 0x0FB, "Bumper Cave Entrance", "cave-bumper-bottom") },
            { new RawEntrance("0x17", 0x17, "Bumper Cave Exit (Top)", 0x0EB, "Bumper Cave Entrance", "cave-bumper-top") },
            { new RawEntrance("0x18", 0x18, "Turtle Rock Isolated Ledge Exit (Bomb Cave Lazer Bridge)", 0x0D5, "Turtle Rock (Laser Key Room)", "turtle-lazer-chests") },
            { new RawEntrance("0x19", 0x19, "Turtle Rock Ledge Exit (East) (Big Chest Entrance)", 0x024, "Turtle Rock (Double Hokku-Bokku / Big chest Room)", "turtle-big-chest") },
            { new RawEntrance("0x1A", 0x1A, "Fairy Ascension Cave Exit (Bottom) (Black Rocks Cave)", 0x0FD, "Black Rocks Cave Bottom", "cave-east-dm-rocks-bottom") },
            { new RawEntrance("0x1B", 0x1B, "Fairy Ascension Cave Exit (Top) (Black Rocks Cave Middle via Drop)", 0x0ED, "Black Rocks Cave Middle", "cave-east-dm-rocks-top") },
            { new RawEntrance("0x1C", 0x1C, "Spiral Cave Exit (Bottom)", 0x0FE, "Spiral Cave Exit after Falling", "cave-spiral-cave-exit") },
            { new RawEntrance("0x1D", 0x1D, "Spiral Cave Exit (Top) (Cave Entrance)", 0x0EE, "Cave (Spiral Cave)", "cave-spiral-cave-entrance") },
            { new RawEntrance("0x1E", 0x1E, "7 Chest Cave Exit (Bottom) (LW Upside-down Cave 2 Entrance Hold Entrance)", 0x0FF, "Upside-down Cave 'Middle' Entrance", "cave-upside-down-shop") },
            { new RawEntrance("0x1F", 0x1F, "7 Chest Cave Exit (Middle) (LW Upside-down Cave 'Bottom' Entrance", 0x0EF, "Cave (Crystal Switch / 5 Chests Room)", "cave-upside-down-5-chest") },
            { new RawEntrance("0x20", 0x20, "7 Chest Cave Exit (Top) (LW Upside-down Cave Top Exit)", 0x0DF, "'Top' of Backward Death-Mountain Cave", "cave-upside-down-top") },
            { new RawEntrance("0x21", 0x21, "Spectacle Rock Cave Exit (Top Left via Drop [kikiskip])", 0x0F9, "Spectical Rock Cave (Exit after falling from top entrance)", "cave-spectical-rock-exit") },
            { new RawEntrance("0x22", 0x22, "Spectacle Rock Cave Exit (Top) (Top Middle via Drop to Item)", 0x0FA, "Spectical Rock Cave (Entrance after jumping to get to HP)", "cave-spectical-rock-entrance-ledge") },
            { new RawEntrance("0x23", 0x23, "Spectacle Rock Cave Exit (Peak) (Top Middle Entrance)", 0x0EA, "Cave (Inside Spectacle Rock HP)", "cave-spectical-rock-upper-entrance") },
            { new RawEntrance("0x24", 0x24, "Agahnims Tower Exit", 0x0E0, "Agahnim's Tower (Entrance Room)", "agahnim-entrance") },
            { new RawEntrance("0x25", 0x25, "Swamp Palace Exit", 0x028, "Swamp Palace (Entrance Room)", "swamp-entrance") },
            { new RawEntrance("0x26", 0x26, "Dark Palace Exit", 0x04A, "Palace of Darkness (Entrance Room)", "pod-entrance") },
            { new RawEntrance("0x27", 0x27, "Misery Mire Exit", 0x098, "Misery Mire (Entrance Room)", "mire-entrance") },
            { new RawEntrance("0x28", 0x28, "Skull Woods Second Section Exit (West) (Entrance with Bumper / Key Pot)", 0x056, "Skull Woods (Key Pot / Trap Room)", "skull-pot-key-exit") },
            { new RawEntrance("0x29", 0x29, "Skull Woods Second Section Exit (East) (Statue Push Entrance)", 0x057, "Skull Woods (Big Key Room)", "skull-gibdo-chest") },
            { new RawEntrance("0x2A", 0x2A, "Skull Woods First Section Exit (Big Chest Entrance)", 0x058, "Skull Woods (Big Chest Room)", "skull-big-chest") },
            { new RawEntrance("0x2B", 0x2B, "Skull Woods Final Section Exit (Boss Entrance)", 0x059, "Skull Woods (Final Section Entrance Room)", "skull-boss-entrance") },
            { new RawEntrance("0x2C", 0x2C, "Thieves Forest Hideout Exit (Tree Entrance)", 0x0E1, "Cave (Lost Woods HP)", "cave-thief-hut") },
            { new RawEntrance("0x2D", 0x2D, "Ice Palace Exit", 0x00E, "Ice Palace (Entrance Room)", "ice-entrance") },
            { new RawEntrance("0x2E", 0x2E, "Death Mountain Return Cave Exit (West) (6 Holes Cave Exit)", 0x0E6, "Cave With a bunch of Keese", "cave-dm-exit") },
            { new RawEntrance("0x2F", 0x2F, "Death Mountain Return Cave Exit (East) (Long Ladder Cave)", 0x0E7, "Cave With a bunch of Keese 2", "cave-dm-exit-entrance") },
            { new RawEntrance("0x30", 0x30, "Old Man House Exit (Bottom)", 0x0E4, "Cave (Lost Old Man House Cave)", "cave-old-man-house-entrance") },
            { new RawEntrance("0x31", 0x31, "Old Man House Exit (Top)", 0x0E5, "Cave (Lost Old Man House Cave Back)", "cave-old-man-house-back-exit") },
            { new RawEntrance("0x32", 0x32, "Hyrule Castle Secret Entrance Exit (Courtyard Exit Door)", 0x055, "Castle Secret Entrance / Uncle Death Room", "cave-uncle-death") },
            { new RawEntrance("0x33", 0x33, "Tower of Hera Exit", 0x077, "Tower of Hera (Entrance Room)", "hera-entrance") },
            { new RawEntrance("0x34", 0x34, "Thieves Town Exit", 0x0DB, "Thieves Town (Main (South West) Entrance Room)", "thieves-entrance") },
            { new RawEntrance("0x35", 0x35, "Turtle Rock Exit (Front) (Main Entrance)", 0x0D6, "Turtle Rock (Entrance Room)", "turtle-entrance") },
            { new RawEntrance("0x36", 0x36, "Pyramid Exit", 0x010, "Ganon Evacuation Route", "ganon-fall") },
            { new RawEntrance("0x37", 0x37, "Ganons Tower Exit", 0x00C, "Ganon's Tower (Entrance Room)", "gt-entrance") },
            { new RawEntrance("0x38", 0x38, "North Fairy Cave Exit (Right of Cemetary Fairy Cave Entrance)", 0x008, "Cave (Healing Fairy)", "cave-north-fairy") },
            { new RawEntrance("0x39", 0x39, "Kakariko Well Exit (Cave Exit)", 0x02F, "Cave (Kakariko Well HP)", "cave-kakariko-well") },
            { new RawEntrance("0x3A", 0x3A, "Hookshot Cave Exit (South) (Under Rock Entrance)", 0x03C, "Hookshot Cave", "cave-hookshot-entrance") },
            { new RawEntrance("0x3B", 0x3B, "Hookshot Cave Exit (North) (Floating Island Exit)", 0x02C, "Hookshot Cave Backdoor (Big Fairy)", "cave-hookshot-backdoor") },
            { new RawEntrance("0x3C", 0x3C, "Lost Woods Gamble", 0x100, "Shop in Lost Woods 0x100", "cave-lost-woods-shop") },
            { new RawEntrance("0x3D", 0x3D, "Dark Swamp Cave (Hype Cave)", 0x11E, "Hype Cave", "cave-hype-cave") },
            { new RawEntrance("0x3E", 0x3E, "Snitch Lady (East) (Left side in Room Map [logically swapped])", 0x101, "Scared Ladies' Houses", "cave-lady-house-east") },
            { new RawEntrance("0x3F", 0x3F, "Snitch Lady (West) (Right side in Room Map [logically swapped])", 0x101, "Scared Ladies' Houses", "cave-lady-house-east") },
            { new RawEntrance("0x40", 0x40, "Sick Kids House", 0x102, "Sick Kid", "cave-sick-kid") },
            { new RawEntrance("0x41", 0x41, "Spike Cave", 0x117, "Spike Cave", "cave-spike-cave") },
            { new RawEntrance("0x42", 0x42, "Tavern (Front)", 0x103, "Inn / Bush House", "cave-inn-top") },
            { new RawEntrance("0x43", 0x43, "Tavern (Back Door)", 0x103, "Inn / Bush House", "cave-inn-top") },
            { new RawEntrance("0x44", 0x44, "Bush Covered House (Kakariko Bushes House)", 0x103, "Inn / Bush House", "cave-inn-top") },
            { new RawEntrance("0x45", 0x45, "Sahasrahlas Hut", 0x105, "Sahasrahla's House", "cave-shabadoo-house") },
            { new RawEntrance("0x46", 0x46, "Kakariko Shop", 0x11F, "Shop 0x11F", "cave-lumberjack-house") },
            { new RawEntrance("0x47", 0x47, "Chest Game (DW)", 0x106, "Chest Game / Outcast Village Bomb House", "cave-dw-chest-game") },
            { new RawEntrance("0x48", 0x48, "Doorless Hut (DW)", 0x106, "Chest Game / Outcast Village Bomb House", "cave-dw-chest-game") },
            { new RawEntrance("0x49", 0x49, "Library", 0x107, "Library / Bomb Farm Room", "cave-library") },
            { new RawEntrance("0x4A", 0x4A, "Light World Bomb Hut (Doorless Bomb House)", 0x107, "Library / Bomb Farm Room", "cave-library") },
            { new RawEntrance("0x4B", 0x4B, "Chicken House", 0x108, "Chicken House", "cave-chicken-hut") },
            { new RawEntrance("0x4C", 0x4C, "Witch Hut (Potion Shop)", 0x109, "Witch Hut?", "cave-witch-shop") },
            { new RawEntrance("0x4D", 0x4D, "Aginah's Cave", 0x10A, "Aginah's Cave", "cave-aginah") },
            { new RawEntrance("0x4E", 0x4E, "Dam", 0x10B, "Swamp Floodway Room", "cave-dam") },
            { new RawEntrance("0x4F", 0x4F, "Mimic Cave", 0x10C, "Mimic Cave", "cave-mimic-cave") },
            { new RawEntrance("0x50", 0x50, "Hookshot Fairy (East DM Next to Upside-down Cave)", 0x10C, "Mimic Cave", "cave-mimic-cave") },
            { new RawEntrance("0x51", 0x51, "Cave South of Haunted Grove", 0x11B, "Mirror Caves (South of Tree Boy / Above Kings Tomb)", "cave-haunted-mirror-cave") },
            { new RawEntrance("0x52", 0x52, "Graveyard Cave (Mirror Cave)", 0x11B, "Mirror Caves (South of Tree Boy / Above Kings Tomb)", "cave-haunted-mirror-cave") },
            { new RawEntrance("0x53", 0x53, "Big Bomb Shop", 0x11C, "Bomb Shop", "cave-big-bomb-shop") },
            { new RawEntrance("0x54", 0x54, "C-Shaped House", 0x11C, "Bomb Shop", "cave-big-bomb-shop") },
            { new RawEntrance("0x55", 0x55, "Long Fairy Cave (Near flute #5)", 0x11E, "Hype Cave", "cave-flute-5-fairy-cave") },
            { new RawEntrance("0x56", 0x56, "Good Bee Cave (Ice Rod Right Entrance)", 0x120, "Ice Rod Cave", "cave-good-bee") },
            { new RawEntrance("0x57", 0x57, "Red Shield Shop (Shop West of Pyramid)", 0x110, "Shop 0x110", "cave-fire-shield-shop") },
            { new RawEntrance("0x58a", 0x58, "Cave Shop (Lake Hylia Shop)", 0x112, "Cave / Shop 0x112", "cave-hylia-shop") },
            { new RawEntrance("0x58b", 0x58, "Cave Shop (DW East Death Mountain Shop)", 0x112, "Cave / Shop 0x112", "cave-dw-east-dm-shop") },
            { new RawEntrance("0x59", 0x59, "Archery Game", 0x111, "Archer Game", "cave-archer-game") },
            { new RawEntrance("0x5A", 0x5A, "Dark Sanctuary Hint", 0x112, "Cave / Shop 0x112", "cave-dw-sanctuary-hint") },
            { new RawEntrance("0x5B", 0x5B, "Kings Grave (Push Grave)", 0x113, "King's Tomb", "cave-kings-tomb") },
            { new RawEntrance("0x5C", 0x5C, "Waterfall of Wishing (Outside Zora's Domain)", 0x114, "Wishing Well / Cave 0x114", "cave-zora-waterfall-wishing") },
            { new RawEntrance("0x5D", 0x5D, "Capacity Upgrade (Lake Hylia Island Cave)", 0x115, "Wishing Well / Big Fairy", "cave-hylia-island-fairy") },
            { new RawEntrance("0x5Ea", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-mire-fairy") },
            { new RawEntrance("0x5Eb", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-south-eastern-fairy") },
            { new RawEntrance("0x5Ec", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-northeast-swamp-fairy") },
            { new RawEntrance("0x5Ed", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-south-pod-fairy") },
            { new RawEntrance("0x5Ee", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-dw-west-dm-fairy") },
            { new RawEntrance("0x5Ef", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-between-desert-swamp-fairy") },
            { new RawEntrance("0x5Eg", 0x5E, "Healer Fairy (South of Eastern Palace Area)", 0x115, "Wishing Well / Big Fairy", "cave-dw-ice-cave-fairy") },
            { new RawEntrance("0x5F", 0x5F, "Dark Desert Cave (Left of Mire Entrance 2 Chests)", 0x10D, "Cave outside Misery Mire", "cave-mire-chests") },
            { new RawEntrance("0x60a", 0x60, "Dark World Shop (Behind Pegs in Outcast Village)", 0x10F, "Shop 0x10F", "cave-dw-lumberjack-shop") },
            { new RawEntrance("0x60b", 0x60, "Dark World Shop (Behind Pegs in Outcast Village)", 0x10F, "Shop 0x10F", "cave-outcast-village-shop") },
            { new RawEntrance("0x60c", 0x60, "Dark World Shop (Behind Pegs in Outcast Village)", 0x10F, "Shop 0x10F", "cave-dw-witch-shop") },
            { new RawEntrance("0x60d", 0x60, "Dark World Shop (Behind Pegs in Outcast Village)", 0x10F, "Shop 0x10F", "cave-dw-hylia-shop") },
            { new RawEntrance("0x61", 0x61, "Thiefs Hut (Blind's House)", 0x119, "Blind's House", "cave-blinds-house") },
            { new RawEntrance("0x62", 0x62, "Dark Desert Hint (Cave in Mire Area)", 0x114, "Wishing Well / Cave 0x114", "cave-dw-mire-hint") },
            { new RawEntrance("0x63", 0x63, "Pyramid Fairy (Fat Fairy)", 0x116, "Fat Fairy", "cave-pyramid-fairy") },
            { new RawEntrance("0x64", 0x64, "Blacksmiths Hut", 0x121, "Smiths' House", "cave-smith-house") },
            { new RawEntrance("0x65a", 0x65, "Fortune Teller (Light)", 0x122, "Fortune Teller(s)", "cave-kakariko-fortune-teller") },
            { new RawEntrance("0x65b", 0x65, "Fortune Teller (Light)", 0x122, "Fortune Teller(s)", "cave-hylia-fortune-teller") },
            { new RawEntrance("0x66", 0x66, "Fortune Teller (Dark)", 0x122, "Fortune Teller(s)", "cave-fortune-teller-dw") },
            { new RawEntrance("0x67", 0x67, "Kakariko Gamble Game (3 Chest Game)", 0x118, "Shop 0x118", "cave-lw-chest-game") },
            { new RawEntrance("0x68", 0x68, "Palace of Darkness Hint", 0x11A, "Mutant Hut", "cave-pod-hint") },
            { new RawEntrance("0x69", 0x69, "East Dark World Hint (DW Flute #5 Cave)", 0x10E, "Cave 0x10E (2 Unknown Caves)", "cave-dw-hylia-hint") },
            { new RawEntrance("0x6A", 0x6A, "Dark Lake Hylia Ledge Hint (DW Ice Rod Cave)", 0x10E, "Cave 0x10E (2 Unknown Caves)", "cave-dw-hylia-hint") },
            { new RawEntrance("0x6B", 0x6B, "Lumberjack House", 0x11F, "Shop 0x11F", "cave-lumberjack-house") },
            { new RawEntrance("0x6C", 0x6C, "Mini Moldorm Cave", 0x123, "Mini-Moldorm Cave", "cave-mini-moldorm") },
            { new RawEntrance("0x6D", 0x6D, "50 Rupee Cave (Big Lift Rock Before Desert)", 0x124, "Unknown Cave / Bonk Cave", "cave-bonk-rocks") },
            { new RawEntrance("0x6E", 0x6E, "Bonk Rock Cave (Next to Sanctuary)", 0x124, "Unknown Cave / Bonk Cave", "cave-bonk-rocks") },
            { new RawEntrance("0x6F", 0x6F, "20 Rupee Cave (Big Lift Rock Outside Ice Rod Cave)", 0x125, "Cave 0x125", "cave-20-rupees") },
            { new RawEntrance("0x70", 0x70, "Dark Lake Hylia Ledge Spike Cave (DW Big Lift Rock Outside Ice Rod Cave)", 0x125, "Cave 0x125", "cave-dw-hylia-spike-cave") },
            { new RawEntrance("0x71a", 0x71, "Bonk Fairy (Bonk Rocks West of Link's House)", 0x126, "Checker Board Cave", "cave-bonk-fairy-lw") },
            { new RawEntrance("0x71b", 0x71, "Bonk Fairy (Bonk Rocks West of Link's House)", 0x126, "Checker Board Cave", "cave-bonk-fairy-dw") },
            { new RawEntrance("0x72", 0x72, "Checkerboard Cave", 0x126, "Checker Board Cave", "cave-checkerboard") },
            { new RawEntrance("0x73", 0x73, "Zeld's Jail Cell (Special Entrance)", 0x080, "Hyrule Castle (Jail Cell Room)", "hyrule-basement-jail") },
            { new RawEntrance("0x74", 0x74, "Hyrule Castle Entrance (Special Entrance)", 0x061, "Hyrule Castle (Main Entrance Room)", "hyrule-entrance") },
            { new RawEntrance("0x75", 0x75, "Agahnim Teleport Room (Special Entrance)", 0x030, "Agahnim's Tower (Maiden Sacrifice Chamber)", "agahnim-maiden-chamber") },
            { new RawEntrance("0x76", 0x76, "Skull Woods First Section (Top) (Drop Entrance)", 0x058, "Skull Woods (Big Chest Room)", "skull-big-chest") },
            { new RawEntrance("0x77", 0x77, "Skull Woods First Section (Left) (Drop Entrance)", 0x067, "Skull Woods (Compass Chest Room)", "skull-compass-chest") },
            { new RawEntrance("0x78", 0x78, "Skull Woods First Section (Right) (Drop Entrance)", 0x068, "Skull Woods (Key Chest / Trap Room)", "skull-wallmaster-chest") },
            { new RawEntrance("0x79", 0x79, "Skull Woods Second Section (Drop Entrance)", 0x056, "Skull Woods (Key Pot / Trap Room)", "skull-pot-key-exit") },
            { new RawEntrance("0x7A", 0x7A, "Thieves Forest Hideout (top) (Drop Entrance)", 0x0E1, "Cave (Lost Woods HP)", "cave-thief-hut") },
            { new RawEntrance("0x7B", 0x7B, "Pyramid (Drop Entrance)", 0x000, "Ganon", "ganon-fight") },
            { new RawEntrance("0x7C", 0x7C, "North Fairy Cave (Bush East of Cemetary) (Drop Entrance)", 0x018, "Big Fairy Drop Entrance Cave", "cave-north-fairy-drop") },
            { new RawEntrance("0x7D", 0x7D, "Hyrule Castle Secret Entrance (Bush Entrance) (Drop Entrance)", 0x055, "Castle Secret Entrance / Uncle Death Room", "cave-uncle-death") },
            { new RawEntrance("0x7E", 0x7E, "Bat Cave (right) (Drop Entrance)", 0x0E3, "Cave (1/2 Magic)", "cave-magic-bat") },
            { new RawEntrance("0x7F", 0x7F, "Lumberjack Tree (top) (Drop Entrance)", 0x0E2, "Cave (Lumberjack's Tree HP)", "cave-lumberjack-tree") },
            { new RawEntrance("0x80", 0x80, "Kakariko Well (top) (Drop Entrance)", 0x02F, "Cave (Kakariko Well HP)", "cave-kakariko-well") },
            { new RawEntrance("0x81", 0x81, "Sewer Drop (Graveyard Entrance) (Drop Entrance)", 0x011, "Hyrule Castle (Bombable Stock Room)", "hyrule-escape-bombable") },
            { new RawEntrance("0x82", 0x82, "Houlihan Room (Drop Entrance)", 0x003, "Houlihan Room", "cave-houlihan") },
            { new RawEntrance("0x83", 0x83, "Dark World Hammer Peg Cave", 0x127, "Hammer Peg Cave", "cave-hammer-pegs") },
            { new RawEntrance("0x84", 0x84, "Ice Cave (Ice Rod Cave Left Bomb Entrance)", 0x120, "Ice Rod Cave", "cave-ice-rod") },
        };

        public List<Edge> Edges { get; private set; }
        void FillEntranceEdges()
        {
            Edges = new List<Edge>();
            _overworldEntrances = new Dictionary<int, OverworldEntrance>();
            _entrances = new Dictionary<string, Entrance>();

            foreach(var r in _rawEntrances)
            {
                RoomNode room;
                if(!_roomNodes.Nodes.TryGetValue(r.LogicalRoomId, out room))
                {
                    throw new Exception($"FillEntranceEdges - Invalid roomId {r.LogicalRoomId}");
                }
                _entrances.Add(r.LogicalEntranceId, new Entrance(r.LogicalEntranceId, r.EntranceId, r.EntranceName, room));
            }

            foreach(var r in _rawOverworldEntrances)
            {
                OverworldAreaNode area;
                if(!_overworldNodes.Nodes.TryGetValue(r.LogicalAreaId, out area))
                {
                    throw new Exception($"FillEntranceEdges - Invalid areaId {r.LogicalAreaId}");
                }

                Entrance entrance;
                if(!_entrances.TryGetValue(r.LogicalEntranceId, out entrance))
                {
                    throw new Exception($"FillEntranceEdges - Invalid entranceId {r.LogicalEntranceId}");
                }
                _overworldEntrances.Add(r.EntranceAddress, new OverworldEntrance(r.EntranceAddress, r.EntranceName, area, entrance, r.Requirements));
            }

            foreach(var e in _overworldEntrances)
            {
                Edges.Add(new Edge(e.Value.Area, e.Value.Entrance.Room, e.Value.Requirements));

                if(e.Value.Entrance.Room.RoomId > 0xFF && e.Value.Entrance.Room.RoomId != RoomIdConstants.R260_LinksHouse) // exclude link's house because it has an exit
                {
                    Edges.Add(new Edge(e.Value.Entrance.Room, e.Value.Area, e.Value.Requirements));
                }
                // room -> area(?) only for fake ones
            }
        }
    }
}
