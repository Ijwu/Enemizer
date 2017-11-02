﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
//using System.Windows.Forms;

namespace EnemizerLibrary
{
    public partial class Randomization
    {
        Random rand;
        RomData ROM_DATA;

        //StreamWriter spoilerfile;

        OptionFlags optionFlags;

        public RomData MakeRandomization(int seed, OptionFlags optionflags, RomData romData, string skin = "") //Initialization of the randomization
        {
            this.optionFlags = optionflags;

            this.ROM_DATA = romData;
            if(this.ROM_DATA.IsEnemizerRom)
            {
                seed = ResetEnemizerRom();
            }
            this.ROM_DATA.ExpandRom();
            this.ROM_DATA.SetCharacterSelectScreenVersion();
            this.ROM_DATA.EnemizerSeed = seed;
            this.ROM_DATA.SetRomInfoOptionFlags(this.optionFlags);

            // make sure we have a randomizer rom
            if (this.ROM_DATA.IsRandomizerRom == false)
            {
                throw new Exception("Enemizer only supports randomizer roms for input.");
            }

            // check that we are not trying to feed in a race rom
            if (this.ROM_DATA.IsRaceRom)
            {
                throw new Exception("Enemizer does not support race roms.");
            }

            // patch in our assembly binary data
            // TODO: figure out if this should be done first or after some other code below
            // TODO: and really this should all be modified to add patches onto this and then just write everything to the rom at once if possible (but there are some reads from the rom I need to look into first)
            Patch patch = new Patch("patchData.json");
            patch.PatchRom(this.ROM_DATA);

            GeneralPatches.MoveRoomHeaders(this.ROM_DATA);

            rand = new Random(seed);

            Graph graph = new Graph(new GraphData(this.ROM_DATA, this.optionFlags));

            if (skin != "Unchanged" && skin != "")
            {
                ChangeSkin(skin);
            }

            if(skin == "Random")
            {
                this.ROM_DATA.RandomizeSprites = true;
                BuildRandomLinkSpriteTable(new Random(seed));
            }
            else
            {
                this.ROM_DATA.RandomizeSprites = false;
            }

            if (optionFlags.RandomizeLinkSpritePalette)
            {
                MakeRandomLinkSpritePalette();
            }


            if (optionFlags.GenerateSpoilers)
            {
                //spoilerfile = new StreamWriter($"{seed.ToString()} Spoiler.txt");
                this.ROM_DATA.Spoiler.AppendLine($"Enemizer Spoiler Log Seed: {seed}");
                //spoilerfile.WriteLine("Spoiler Log Seed : " + seed.ToString());
            }

            //create_subset_gfx();
            var spriteRequirements = new SpriteRequirementCollection();

            var spriteGroupCollection = new SpriteGroupCollection(this.ROM_DATA, rand, spriteRequirements);
            spriteGroupCollection.LoadSpriteGroups();

            // -----bosses---------------------
            if (optionFlags.RandomizeBosses)
            {
                this.ROM_DATA.CloseBlindDoor = true;

                BossRandomizer br;

                switch (optionFlags.RandomizeBossesType)
                {
                    case RandomizeBossesType.Basic:
                        br = new BossRandomizer(rand, optionFlags, this.ROM_DATA.Spoiler, graph);
                        break;
                    case RandomizeBossesType.Normal:
                        br = new NormalBossRandomizer(rand, optionflags, this.ROM_DATA.Spoiler, graph);
                        break;
                    case RandomizeBossesType.Chaos:
                        br = new ChaosBossRandomizer(rand, optionflags, this.ROM_DATA.Spoiler, graph);
                        break;
                    default:
                        throw new Exception("Unknown Boss Randomization Type");
                }
                br.RandomizeRom(this.ROM_DATA, spriteGroupCollection, spriteRequirements);
            }

            // -----sprites---------------------

            if(optionFlags.RandomizeEnemies)
            {
                this.ROM_DATA.RandomizeHiddenEnemies = true;
                if (optionflags.RandomizeBushEnemyChance)
                {
                    this.ROM_DATA.RandomizeHiddenEnemyChancePool();
                }
            }

            //dungeons
            if (optionFlags.RandomizeEnemies) // random sprites dungeons
            {
                spriteGroupCollection.SetupRequiredDungeonGroups();
                DungeonEnemyRandomizer der = new DungeonEnemyRandomizer(this.ROM_DATA, this.rand, spriteGroupCollection, spriteRequirements);
                der.RandomizeDungeonEnemies(optionFlags);
            }

            //random sprite overworld
            if (optionFlags.RandomizeEnemies)
            {
                spriteGroupCollection.SetupRequiredOverworldGroups();
                OverworldEnemyRandomizer oer = new OverworldEnemyRandomizer(this.ROM_DATA, this.rand, spriteGroupCollection, spriteRequirements);
                oer.RandomizeOverworldEnemies(optionFlags);
            }

            if (optionflags.RandomizeBosses || optionflags.RandomizeEnemies)
            {
                spriteGroupCollection.UpdateRom();
            }


            if (optionFlags.RandomizeEnemyHealthRange)
            {
                Randomize_Sprites_HP(optionFlags.RandomizeEnemyHealthRangeAmount);
            }

            if (optionFlags.RandomizeEnemyDamage)
            {
                Randomize_Sprites_DMG(optionFlags.AllowEnemyZeroDamage);
            }

            
            if (optionFlags.RandomizePots)
            {
                randomizePots(); //default on for now
            }

            if (optionFlags.ShuffleEnemyDamageGroups)
            {
                ShuffleDamageGroups();
            }

            //reset seed for all these values so they can be optional
            rand = new Random(seed);
            if (optionFlags.RandomizeDungeonPalettes)
            {
                Randomize_Dungeons_Palettes();
            }

            rand = new Random(seed);
            if (optionFlags.RandomizeSpritePalettes)
            {
                Randomize_Sprites_Palettes();
            }

            rand = new Random(seed);
            if (optionFlags.RandomizeOverworldPalettes)
            {
                Randomize_Overworld_Palettes();
            }

            rand = new Random(seed);
            if(optionFlags.ShuffleMusic)
            {
                // TODO: disable this for now because it crashes stuff sometimes
                shuffle_music();
            }

            if(optionFlags.SetBlackoutMode)
            {
                black_all_dungeons();
            }

            if(optionFlags.PukeMode)
            {
                GeneratePukeModePalettes(new Random(seed));
            }

            if (optionFlags.NegativeMode)
            {
                GenerateNegativeModePalettes();
            }

            if (optionFlags.GrayscaleMode)
            {
                grayscale_all_dungeons();
            }

            SetSwordGfx(optionFlags.SwordGraphics);
            SetShieldGfx(optionFlags.ShieldGraphics);

            rand = new Random(seed);
            if (optionflags.BootlegMagic)
            {
                // TODO: move this to its own class
                byte numberOfMoldormEyes = (byte)rand.Next(0, 8);
                this.ROM_DATA[AddressConstants.MoldormEyeCountAddressVanilla] = numberOfMoldormEyes;
                this.ROM_DATA[AddressConstants.MoldormEyeCountAddressEnemizer] = numberOfMoldormEyes;

                if(rand.Next(0, 100) == 1)
                {
                    // break link's water transition so he turns invisible and always gets fake flippers when he doesn't have flippers
                    // discovered on accident by using the SNES address instead of PC address when trying to expand the rom
                    // TODO: leave this out for now
                    //this.ROM_DATA[0xFFD7] = 0x0C;
                }
            }



            //Remove Trinexx Ice Floor : 
            this.ROM_DATA[0x04B37E] = AssemblyConstants.NoOp;
            this.ROM_DATA[0x04B37E+1] = AssemblyConstants.NoOp;
            this.ROM_DATA[0x04B37E+2] = AssemblyConstants.NoOp;
            this.ROM_DATA[0x04B37E+3] = AssemblyConstants.NoOp;

            /*this.ROM_DATA[0x5033 + 0x5E] = 0x24;
            this.ROM_DATA[0x5112 + 0x5E] = 0x93;
            this.ROM_DATA[0x51F1 + 0x5E] = 0x57;

            FileStream fsxx = new FileStream("weapons/mace.bin", FileMode.Open, FileAccess.Read);
            byte[] weapon_data = new byte[fsxx.Length];
            fsxx.Read(weapon_data, 0, (int)fsxx.Length);
            fsxx.Close();
            for (int i = 0; i < (int)weapon_data.Length; i++)
            {
                ROM_DATA[0x0121357 + i] = weapon_data[i];
            }*/

            //if (spoilerfile != null)
            //{
            //    spoilerfile.Flush();
            //    spoilerfile.Close();
            //}

            if (optionFlags.AlternateGfx)
            {
                SetBossGfx();
            }

            SetHeartBeepSpeed(optionflags.HeartBeepSpeed);

            if (optionFlags.AndyMode)
            {
                SetAndyMode();
            }

            if (optionFlags.DebugMode)
            {
                // put the room id in the rupee slot
                this.ROM_DATA[0x1017A9] = 0xA0;
                this.ROM_DATA[0x1017A9 + 1] = 0x00;
                this.ROM_DATA[0x1017A9 + 2] = 0x7E;
            }

            return this.ROM_DATA;

        }

        private int ResetEnemizerRom()
        {
            // loaded an enemizer rom. let's just reset the bosses and load the saved options so we can try to debug roms
            int seed = this.ROM_DATA.EnemizerSeed;

            this.ROM_DATA[AddressConstants.RoomHeaderBankLocation] = 0x04; // put it back

            // reset the room pointers
            byte[] originalPointers = { 0x62, 0xF4, 0x6C, 0xF4, 0x7A, 0xF4, 0xDD, 0xF5, 0x85, 0xF4, 0x90, 0xF4, 0x90, 0xF4, 0x97, 0xF4, 0xA2, 0xF4, 0xA9, 0xF4, 0xB5, 0xF4, 0xC0, 0xF4, 0xCB, 0xF4, 0xD8, 0xF4, 0xDF, 0xF4, 0xEA, 0xF4, 0xEA, 0xF4, 0xF1, 0xF4, 0xFC, 0xF4, 0x03, 0xF5, 0x11, 0xF5, 0x18, 0xF5, 0x23, 0xF5, 0x2E, 0xF5, 0x73, 0xFC, 0x3A, 0xF5, 0x41, 0xF5, 0x4D, 0xF5, 0x58, 0xF5, 0x63, 0xF5, 0x6E, 0xF5, 0x79, 0xF5, 0x84, 0xF5, 0x8B, 0xF5, 0x8B, 0xF5, 0x03, 0xF5, 0x92, 0xF5, 0x99, 0xF5, 0x99, 0xF5, 0xA6, 0xF5, 0xB2, 0xF5, 0xBD, 0xF5, 0xC4, 0xF5, 0xCB, 0xF5, 0x73, 0xFC, 0xD6, 0xF5, 0xD6, 0xF5, 0xDD, 0xF5, 0xE4, 0xF5, 0xEF, 0xF5, 0xFB, 0xF5, 0x06, 0xF6, 0x0D, 0xF6, 0x18, 0xF6, 0x1F, 0xF6, 0x18, 0xF6, 0x26, 0xF6, 0x31, 0xF6, 0x3B, 0xF6, 0x46, 0xF6, 0x51, 0xF6, 0x58, 0xF6, 0x63, 0xF6, 0x6E, 0xF6, 0x7A, 0xF6, 0x86, 0xF6, 0x91, 0xF6, 0x9D, 0xF6, 0xA4, 0xF6, 0xAB, 0xF6, 0xB6, 0xF6, 0xBD, 0xF6, 0xBD, 0xF6, 0xBD, 0xF6, 0xC4, 0xF6, 0xD0, 0xF6, 0xDA, 0xF6, 0xE5, 0xF6, 0xF0, 0xF6, 0xFB, 0xF6, 0x05, 0xF7, 0x13, 0xF7, 0x1E, 0xF7, 0x2C, 0xF7, 0x37, 0xF7, 0x42, 0xF7, 0x49, 0xF7, 0x50, 0xF7, 0x57, 0xF7, 0x5E, 0xF7, 0x65, 0xF7, 0x6C, 0xF7, 0x73, 0xF7, 0x7E, 0xF7, 0x89, 0xF7, 0x94, 0xF7, 0xA0, 0xF7, 0xA7, 0xF7, 0xA0, 0xF7, 0xB2, 0xF7, 0xBD, 0xF7, 0xC8, 0xF7, 0xD2, 0xF7, 0xDD, 0xF7, 0xE4, 0xF7, 0xEB, 0xF7, 0xEB, 0xF7, 0xF7, 0xF7, 0x02, 0xF8, 0x0D, 0xF8, 0x14, 0xF8, 0x1F, 0xF8, 0x1F, 0xF8, 0x2B, 0xF8, 0x36, 0xF8, 0x41, 0xF8, 0x48, 0xF8, 0x4F, 0xF8, 0x56, 0xF8, 0x63, 0xF8, 0x70, 0xF8, 0x70, 0xF8, 0x70, 0xF8, 0x70, 0xF8, 0x7A, 0xF8, 0x81, 0xF8, 0x8B, 0xF8, 0x96, 0xF8, 0xA1, 0xF8, 0xAC, 0xF8, 0xAC, 0xF8, 0xB3, 0xF8, 0xBA, 0xF8, 0xC1, 0xF8, 0xC8, 0xF8, 0xC8, 0xF8, 0xD4, 0xF8, 0xD4, 0xF8, 0xDE, 0xF8, 0xDE, 0xF8, 0xE5, 0xF8, 0xF2, 0xF8, 0xF9, 0xF8, 0x04, 0xF9, 0x04, 0xF9, 0x0B, 0xF9, 0x16, 0xF9, 0x1D, 0xF9, 0x28, 0xF9, 0x28, 0xF9, 0x2F, 0xF9, 0x3A, 0xF9, 0x45, 0xF9, 0x50, 0xF9, 0x5B, 0xF9, 0x5B, 0xF9, 0x65, 0xF9, 0x6C, 0xF9, 0x76, 0xF9, 0x81, 0xF9, 0x88, 0xF9, 0x93, 0xF9, 0x9A, 0xF9, 0x93, 0xF9, 0xA5, 0xF9, 0xAC, 0xF9, 0xB7, 0xF9, 0xC2, 0xF9, 0xCC, 0xF9, 0xD3, 0xF9, 0xDD, 0xF9, 0xE4, 0xF9, 0xEF, 0xF9, 0xF6, 0xF9, 0xF6, 0xF9, 0x01, 0xFA, 0x08, 0xFA, 0x14, 0xFA, 0x1E, 0xFA, 0x25, 0xFA, 0x2C, 0xFA, 0x37, 0xFA, 0x42, 0xFA, 0x0A, 0xF5, 0x4D, 0xFA, 0x54, 0xFA, 0x5B, 0xFA, 0x62, 0xFA, 0x69, 0xFA, 0x74, 0xFA, 0x74, 0xFA, 0x7F, 0xFA, 0x86, 0xFA, 0x92, 0xFA, 0x99, 0xFA, 0xA0, 0xFA, 0xA7, 0xFA, 0xB2, 0xFA, 0x0A, 0xF5, 0xB9, 0xFA, 0xC0, 0xFA, 0xC7, 0xFA, 0xCE, 0xFA, 0xCE, 0xFA, 0xCE, 0xFA, 0xD5, 0xFA, 0xD5, 0xFA, 0xDF, 0xFA, 0xDF, 0xFA, 0xEB, 0xFA, 0xF6, 0xFA, 0x01, 0xFB, 0x01, 0xFB, 0xB2, 0xFA, 0x0A, 0xF5, 0x01, 0xFB, 0x01, 0xFB, 0x08, 0xFB, 0x0F, 0xFB, 0xCE, 0xFA, 0xCE, 0xFA, 0x1A, 0xFB, 0x1A, 0xFB, 0x21, 0xFB, 0x2C, 0xFB, 0x37, 0xFB, 0x3E, 0xFB, 0x45, 0xFB, 0x4C, 0xFB, 0x4C, 0xFB, 0x53, 0xFB, 0x53, 0xFB, 0x5A, 0xFB, 0x68, 0xFB, 0x68, 0xFB, 0x73, 0xFB, 0x7E, 0xFB, 0x7E, 0xFB, 0x8A, 0xFB, 0x94, 0xFB, 0x53, 0xFB, 0x53, 0xFB, 0xA0, 0xFB, 0xA0, 0xFB, 0xA5, 0xFB, 0xA5, 0xFB, 0xAC, 0xFB, 0xAC, 0xFB, 0xAC, 0xFB, 0xBA, 0xFB, 0xC1, 0xFB, 0xCC, 0xFB, 0xD7, 0xFB, 0xD7, 0xFB, 0xBA, 0xFB, 0xE3, 0xFB, 0xEE, 0xFB, 0xFC, 0xFB, 0x03, 0xFC, 0x0A, 0xFC, 0x11, 0xFC, 0x18, 0xFC, 0x1F, 0xFC, 0x26, 0xFC, 0x2D, 0xFC, 0x34, 0xFC, 0x3B, 0xFC, 0x42, 0xFC, 0x49, 0xFC, 0x50, 0xFC, 0x57, 0xFC, 0xF5, 0xFB, 0xF5, 0xFB, 0x5E, 0xFC, 0x65, 0xFC, 0x6C, 0xFC, 0x73, 0xFC, 0x73, 0xFC, 0x7A, 0xFC, 0x81, 0xFC, 0x0A, 0xFC, 0x88, 0xFC, 0x93, 0xFC, 0x9A, 0xFC, 0xF5, 0xFB, 0xA1, 0xFC, 0xAC, 0xFC, 0xB3, 0xFC, 0xBA, 0xFC, 0x5E, 0xFC, 0x5E, 0xFC, 0xC1, 0xFC, 0xC8, 0xFC, 0xC8, 0xFC, 0xC8, 0xFC, 0xAC, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC, 0xCF, 0xFC };
            this.ROM_DATA.WriteDataChunk(AddressConstants.dungeonHeaderPointerTableBaseAddress, originalPointers);

            // reset room graphics blocks
            byte[] originalRoomBlocks = { 0x00, 0x49, 0x00, 0x00, 0x46, 0x49, 0x0C, 0x1D, 0x48, 0x49, 0x13, 0x1D, 0x46, 0x49, 0x13, 0x0E, 0x48, 0x49, 0x0C, 0x11, 0x48, 0x49, 0x0C, 0x10, 0x4F, 0x49, 0x4A, 0x50, 0x0E, 0x49, 0x4A, 0x11, 0x46, 0x49, 0x12, 0x00, 0x00, 0x49, 0x00, 0x50, 0x00, 0x49, 0x00, 0x11, 0x48, 0x49, 0x0C, 0x00, 0x00, 0x00, 0x37, 0x36, 0x48, 0x49, 0x4C, 0x11, 0x5D, 0x2C, 0x0C, 0x44, 0x00, 0x00, 0x4E, 0x00, 0x0F, 0x00, 0x12, 0x10, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x0D, 0x17, 0x00, 0x16, 0x0D, 0x17, 0x1B, 0x16, 0x0D, 0x17, 0x14, 0x15, 0x0D, 0x17, 0x15, 0x16, 0x0D, 0x18, 0x19, 0x16, 0x0D, 0x17, 0x19, 0x16, 0x0D, 0x00, 0x00, 0x16, 0x0D, 0x18, 0x1B, 0x0F, 0x49, 0x4A, 0x11, 0x4B, 0x2A, 0x5C, 0x15, 0x16, 0x49, 0x17, 0x1D, 0x00, 0x00, 0x00, 0x15, 0x16, 0x0D, 0x17, 0x10, 0x16, 0x49, 0x12, 0x00, 0x16, 0x49, 0x0C, 0x11, 0x00, 0x00, 0x12, 0x10, 0x16, 0x0D, 0x00, 0x11, 0x16, 0x49, 0x0C, 0x00, 0x16, 0x0D, 0x4C, 0x11, 0x0E, 0x0D, 0x4A, 0x11, 0x16, 0x1A, 0x17, 0x1B, 0x4F, 0x34, 0x4A, 0x50, 0x35, 0x4D, 0x65, 0x36, 0x4A, 0x34, 0x4E, 0x00, 0x0E, 0x34, 0x4A, 0x11, 0x51, 0x34, 0x5D, 0x59, 0x4B, 0x49, 0x4C, 0x11, 0x2D, 0x00, 0x00, 0x00, 0x5D, 0x00, 0x12, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x47, 0x49, 0x2B, 0x2D, 0x46, 0x49, 0x1C, 0x52, 0x00, 0x49, 0x1C, 0x52, 0x5D, 0x49, 0x00, 0x52, 0x46, 0x49, 0x13, 0x52, 0x4B, 0x4D, 0x4A, 0x5A, 0x47, 0x49, 0x1C, 0x52, 0x4B, 0x4D, 0x39, 0x36, 0x1F, 0x2C, 0x2E, 0x52, 0x1F, 0x2C, 0x2E, 0x1D, 0x2F, 0x2C, 0x2E, 0x52, 0x2F, 0x2C, 0x2E, 0x31, 0x1F, 0x1E, 0x30, 0x52, 0x51, 0x49, 0x13, 0x00, 0x4F, 0x49, 0x13, 0x50, 0x4F, 0x4D, 0x4A, 0x50, 0x4B, 0x49, 0x4C, 0x2B, 0x1F, 0x20, 0x22, 0x53, 0x55, 0x3D, 0x42, 0x43, 0x1F, 0x1E, 0x23, 0x52, 0x1F, 0x1E, 0x39, 0x3A, 0x1F, 0x1E, 0x3A, 0x3E, 0x1F, 0x1E, 0x3C, 0x3D, 0x40, 0x1E, 0x27, 0x3F, 0x55, 0x1A, 0x42, 0x43, 0x1F, 0x1E, 0x2A, 0x52, 0x1F, 0x1E, 0x38, 0x52, 0x1F, 0x20, 0x28, 0x52, 0x1F, 0x20, 0x26, 0x52, 0x1F, 0x2C, 0x25, 0x52, 0x1F, 0x20, 0x27, 0x52, 0x1F, 0x1E, 0x29, 0x52, 0x1F, 0x2C, 0x3B, 0x52, 0x46, 0x49, 0x24, 0x52, 0x21, 0x41, 0x45, 0x33, 0x1F, 0x2C, 0x28, 0x31, 0x1F, 0x0D, 0x29, 0x52, 0x1F, 0x1E, 0x27, 0x52, 0x1F, 0x20, 0x27, 0x53, 0x48, 0x49, 0x13, 0x52, 0x0E, 0x1E, 0x4A, 0x50, 0x1F, 0x20, 0x26, 0x53, 0x15, 0x00, 0x00, 0x00, 0x1F, 0x00, 0x2A, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x5D, 0x49, 0x00, 0x52, 0x55, 0x49, 0x42, 0x43, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x56, 0x57, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x56, 0x57, 0x50, 0x61, 0x56, 0x63, 0x50, 0x61, 0x56, 0x57, 0x50, 0x61, 0x56, 0x33, 0x50, 0x61, 0x56, 0x57, 0x50, 0x61, 0x62, 0x63, 0x50, 0x61, 0x62, 0x63, 0x50 };
            this.ROM_DATA.WriteDataChunk(0x5B97, originalRoomBlocks);

            // reset overworld blocks
            byte[] originalOverworldBlocks = { 0x07, 0x07, 0x07, 0x10, 0x10, 0x10, 0x10, 0x10, 0x07, 0x07, 0x07, 0x10, 0x10, 0x10, 0x10, 0x04, 0x06, 0x06, 0x00, 0x03, 0x03, 0x00, 0x0D, 0x0A, 0x06, 0x06, 0x01, 0x01, 0x01, 0x04, 0x05, 0x05, 0x06, 0x06, 0x06, 0x01, 0x01, 0x04, 0x05, 0x05, 0x06, 0x09, 0x0F, 0x00, 0x00, 0x0B, 0x0B, 0x05, 0x08, 0x08, 0x0A, 0x04, 0x04, 0x04, 0x04, 0x04, 0x08, 0x08, 0x0A, 0x04, 0x04, 0x04, 0x04, 0x04, 0x07, 0x07, 0x1A, 0x10, 0x10, 0x10, 0x10, 0x10, 0x07, 0x07, 0x1A, 0x10, 0x10, 0x10, 0x10, 0x04, 0x06, 0x06, 0x00, 0x03, 0x03, 0x00, 0x0D, 0x0A, 0x06, 0x06, 0x1C, 0x1C, 0x1C, 0x02, 0x05, 0x05, 0x06, 0x06, 0x06, 0x1C, 0x1C, 0x00, 0x05, 0x05, 0x06, 0x00, 0x0F, 0x00, 0x00, 0x23, 0x23, 0x05, 0x1F, 0x1F, 0x0A, 0x20, 0x20, 0x20, 0x20, 0x20, 0x1F, 0x1F, 0x0A, 0x20, 0x20, 0x20, 0x20, 0x20, 0x13, 0x13, 0x17, 0x14, 0x14, 0x14, 0x14, 0x14, 0x13, 0x13, 0x17, 0x14, 0x14, 0x14, 0x14, 0x16, 0x15, 0x15, 0x12, 0x13, 0x13, 0x18, 0x16, 0x16, 0x15, 0x15, 0x13, 0x26, 0x26, 0x13, 0x17, 0x17, 0x15, 0x15, 0x15, 0x26, 0x26, 0x13, 0x17, 0x17, 0x1B, 0x1D, 0x11, 0x13, 0x13, 0x18, 0x18, 0x17, 0x16, 0x16, 0x13, 0x13, 0x13, 0x19, 0x19, 0x19, 0x16, 0x16, 0x18, 0x13, 0x18, 0x19, 0x19, 0x19, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x05, 0x05, 0x06, 0x09, 0x09, 0x09, 0x09, 0x09, 0x05, 0x05, 0x06, 0x09, 0x09, 0x09, 0x09, 0x03 };
            this.ROM_DATA.WriteDataChunk(AddressConstants.OverworldAreaGraphicsBlockBaseAddress, originalOverworldBlocks);

            // reset bosses
            VanillaBossResetter reset = new VanillaBossResetter();
            reset.ResetRom(this.ROM_DATA);

            // load the option flags from the ROM and use them
            this.optionFlags = optionFlags = this.ROM_DATA.GetOptionFlagsFromRom();
            return seed;
        }

        void SetSwordGfx(SwordTypes swordType)
        {
            string filename = swordType.ToString().Replace(" ", "").Trim() + ".gfx";
            FileStream f = new FileStream("sword_gfx\\" + filename, FileMode.Open, FileAccess.Read);
            f.Read(this.ROM_DATA.romData, XkasSymbols.Instance.Symbols["swordgfx"], (int)f.Length);
            f.Close();
        }

        void SetShieldGfx(ShieldTypes shieldType)
        {
            string filename = shieldType.ToString().Replace(" ", "").Trim() + ".gfx";
            FileStream f = new FileStream("shield_gfx\\" + filename, FileMode.Open, FileAccess.Read);
            f.Read(this.ROM_DATA.romData, XkasSymbols.Instance.Symbols["shieldgfx"], (int)f.Length);
            f.Close();
        }

        void ShuffleDamageGroups()
        {
            //for 10 groups, 3 damage by groups, green mail, blue mail, red mail
            //example vanilla group will do 4,2,1, 8 = 1 heart
            for(int i = 0; i < 10; i++)
            {
                int minRand = 4; // min half heart for green
                if(optionFlags.AllowEnemyZeroDamage)
                {
                    minRand = 0;
                }

                int maxRand = 32;
                if (optionFlags.EnemyDamageChaosMode)
                {
                    maxRand = 64;
                }

                byte greenmail = (byte)rand.Next(minRand, maxRand);
                byte bluemail;
                byte redmail;

                if (optionFlags.EnemyDamageChaosMode)
                {
                    bluemail = (byte)rand.Next(minRand, maxRand);
                    redmail = (byte)rand.Next(minRand, maxRand);
                }
                else
                {
                    bluemail = (byte)(greenmail * 3 / 4);
                    redmail = (byte)(greenmail * 3 / 8);
                }
                this.ROM_DATA[0x3742D + 0 + (i * 3)] = greenmail; //green mail
                this.ROM_DATA[0x3742D + 1 + (i * 3)] = bluemail; //blue mail
                this.ROM_DATA[0x3742D + 2 + (i * 3)] = redmail; //red mail
            }
            
        }

        void SetBossGfx()
        {
            //they all must need to be at the same place since they generate new addresses/pointers
            int newGfxPosition = AddressConstants.NewBossGraphicsBaseAddress;
            byte[] bossgfxindex =
            {
                0x8D,
                0xB5,
                0xC8,
                0xB6,
                0x90,
                0x94,
                0xA3,
                0xA4,
                0xA6,
                0xAB,
                0xAC,
                0xAD,
                0xAE,
                0xAF,
                0xB0,
                0xB1,
                0xB2,
                0xB3,
                0xB4,
                0xB8
            };
            string[] bossgfxfiles =
            {
                "agahnim1.bin",
                "agahnim2.bin",
                "agahnim3.bin",
                "agahnim4.bin",
                "armosknight.bin",
                "ganon1.bin",
                "moldorm.bin",
                "lanmola.bin",
                "ganon2.bin",
                "mothula.bin",
                "arrghus.bin",
                "helmasaure1.bin",
                "blind.bin",
                "kholdstare.bin",
                "vitreous.bin",
                "helmasaure2.bin",
                "trinexx1.bin",
                "trinexx2.bin",
                "ganon3.bin",
                "ganon4.bin"
            };

            for(int i = 0; i < bossgfxindex.Length; i++)
            {
                FileStream f = new FileStream("bosses_gfx\\"+bossgfxfiles[i], FileMode.Open, FileAccess.Read);
                f.Read(this.ROM_DATA.romData, newGfxPosition, (int)f.Length);
                byte[] address = Utilities.PCAddressToSnesByteArray(newGfxPosition);
                this.ROM_DATA[0x4FC0 + bossgfxindex[i]] = address[0]; //bank
                this.ROM_DATA[0x509F + bossgfxindex[i]] = address[1];  //highbyte
                this.ROM_DATA[0x517E + bossgfxindex[i]] = address[2];  //lowbyte
                newGfxPosition += (int)f.Length;
                f.Close();
            }
            

        }

    

        void SetAndyMode()
        {
            this.ROM_DATA[0xD0D58] = 0x00; // set soundfx3 background note 00
            this.ROM_DATA[0xD0D97] = 0x00; // set soundfx3 background note 2(?) 00

            byte[] newSoundInstrument = { 0xE0, 0x19, 0x7F, 0x97, 0x00 }; // set instrument 19, length 7F, play note 97 (B oct2), end
            this.ROM_DATA.WriteDataChunk(0xD1869, newSoundInstrument); // set soundfx3 background note 00
        }

        void SetHeartBeepSpeed(HeartBeepSpeed beepSpeed)
        {
            this.ROM_DATA.HeartBeep = beepSpeed;
        }

        private void MakeRandomLinkSpritePalette()
        {
            Color c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (1 * 2), c, 0);

            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (2 * 2), c, 2);
            setColor(0xDD308 + (3 * 2), c, 0);
            setColor(0xDD308 + (12 * 2), c, 0);

            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (5 * 2), c, 0);

            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (6 * 2), c, 0);

            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (7 * 2), c, 0);


            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (8 * 2), c, 2);
            setColor(0xDD308 + (9 * 2), c, 0);

            c = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            setColor(0xDD308 + (10 * 2), c, 2);
            setColor(0xDD308 + (11 * 2), c, 0);
        }

        private void ChangeSkin(string skin)
        {
            if (skin == "Random")
            {
                string[] skins = Directory.GetFiles("sprites\\");
                skin = skins[rand.Next(skins.Length)];
            }
            FileStream fsx = new FileStream(skin, FileMode.Open, FileAccess.Read);
            byte[] skin_data = new byte[0x7078];
            fsx.Read(skin_data, 0, 0x7078);
            fsx.Close();
            for (int i = 0; i < 0x7000; i++)
            {
                this.ROM_DATA[0x80000 + i] = skin_data[i];
            }
            for (int i = 0; i < 0x78; i++)
            {
                this.ROM_DATA[0x0DD308 + i] = skin_data[0x7000 + i];
            }
            // gloves color
            this.ROM_DATA[0xDEDF5] = skin_data[0x7036];
            this.ROM_DATA[0xDEDF6] = skin_data[0x7037];
            this.ROM_DATA[0xDEDF7] = skin_data[0x7054];
            this.ROM_DATA[0xDEDF8] = skin_data[0x7055];

        }


        public void Randomize_Dungeons_Palettes()
        {
            for (int i = 0; i < 20; i++)
            {
                randomize_wall(i);
                randomize_floors(i);
            }
            //grayscale_all_dungeons();
        }

        public void black_all_dungeons()
        {
            for (int i = 0; i < 3600; i++)
            {
                int j = (i / 180);
                if (((i - (j * 180)) < 120) || ((i - (j * 180)) >= 136))
                {
                    if (((i - (j * 180)) < 38) || ((i - (j * 180)) >= 44))
                    {
                        ROM_DATA[0xDD734 + i] = 0x00;
                    }
                }


            }

            //Remove Dark Room
            int[] dark_rooms = { 11, 25, 33, 34, 50, 65, 66, 106, 146, 147, 153, 181, 186, 192, 208, 228, 229, 230, 231, 240, 241 };
            for(int i = 0;i<dark_rooms.Length;i++)
            {
                ROM_DATA[AddressConstants.dungeonHeaderBaseAddress + ((dark_rooms[i] * 14))] = (byte)((ROM_DATA[AddressConstants.dungeonHeaderBaseAddress + ((dark_rooms[i] * 14))] & 0xFE));
            }

        }

        public Color getColor(short c)
        {
            return Color.FromArgb(((c & 0x1F) * 8), (((c & 0x3E0) >> 5) * 8), (((c & 0x7C00) >> 10) * 8));
        }

        public void grayscale_all_dungeons()
        {
            for (int i = 0; i < 3600; i+=2)
            {

                Color c = getColor((byte)((ROM_DATA[0xDD734 + i+1] << 8) + ROM_DATA[0xDD734 + i]));
                if (c.R == 40 && c.G == 40 && c.B == 40)
                {
                    //6,6,3
                    //48,48,24
                }
                else
                {

                    int sum = ((c.R + c.R + c.G + c.G + c.G + c.B) / 6);
                    setColor(0xDD734 + i, Color.FromArgb(sum, sum, sum), 0);
                }
            }
        }

        public void set_weird_color()
        {
            byte[] ppp = new byte[] { 0x00, 0x00, 0x0E, 0xFA, 0x7D, 0xD1, 0x00, 0x00, 0x7F, 0x1A, 0x00, 0x00, 0x7F, 0x1A, 0x71, 0x6E, 0x7D, 0xD1, 0x40, 0xA7, 0x7D, 0xD1, 0x40, 0xA7, 0x48, 0xE9, 0x50, 0xCF, 0x7F, 0xFF };
            int posppp = 0;
            for (int i = 0; i < 3600; i += 1)
            {
                ROM_DATA[0xDD734 + i] = ppp[posppp];
                posppp++;
                if (posppp >= ppp.Length)
                {
                    posppp = 0;
                }
            }
        }

        public void randomize_wall(int dungeon)
        {

            
            Color wall_color = Color.FromArgb(60+rand.Next(180), 60+rand.Next(180), 60+rand.Next(180));
            //byte shade = (byte)(6 + rand.Next(8) -((wall_color.R+wall_color.G+wall_color.B)/80));

            for (int i = 0; i < 5; i++)
            {
                //166
                byte shadex = (byte)(10 - (i * 2));
                setColor((0x0DD734 + (0xB4 * dungeon)) + (i * 2), wall_color, shadex);
                setColor((0x0DD770 + (0xB4 * dungeon)) + (i * 2), wall_color, shadex);
                setColor((0x0DD744 + (0xB4 * dungeon)) + (i * 2), wall_color, shadex);
               
                if (dungeon == 0)
                {
                    setColor((0x0DD7CA + (0xB4 * dungeon)) + (i * 2), wall_color, shadex);
                }
                /*setColor(0x0DD74C - (i * 2), Color.LimeGreen, (byte)(i * 2));
                setColor(0x0DD778 - (i * 2), Color.LimeGreen, (byte)(i * 2));
                setColor(0x0DD73C - (i * 2), Color.LimeGreen, (byte)(i * 2));*/
            }

            if (dungeon == 2)
            {
                setColor((0x0DD74E + (0xB4 * dungeon)), wall_color, 3);
                setColor((0x0DD74E+2 + (0xB4 * dungeon)), wall_color, 7);
                setColor((0x0DD73E + (0xB4 * dungeon)), wall_color, 3);
                setColor((0x0DD73E + 2 + (0xB4 * dungeon)), wall_color, 7);
                
            }

            //setColor(0x0DD76A + (0xB4 * dungeon), wall_color, (byte)(shade - 6));

            //Ceiling
            setColor(0x0DD7E4 + (0xB4 * dungeon), wall_color, (byte)(2)); //outer wall darker
            setColor(0x0DD7E6 + (0xB4 * dungeon), wall_color, (byte)(4)); //outter wall brighter

            //pits walls
            setColor(0x0DD7DA + (0xB4 * dungeon), wall_color, (byte)(6));
            setColor(0x0DD7DC + (0xB4 * dungeon), wall_color, (byte)(4));

            Color pot_color = Color.FromArgb(60 + rand.Next(180), 60 + rand.Next(180), 60 + rand.Next(180));
            //Pots
            setColor(0x0DD75A + (0xB4 * dungeon), pot_color, 6);
            setColor(0x0DD75C + (0xB4 * dungeon), pot_color, 1);
            setColor(0x0DD75E + (0xB4 * dungeon), pot_color, 3);

            //Wall Contour?
            //f,c,m
            setColor(0x0DD76A + (0xB4 * dungeon), wall_color, 7);
            setColor(0x0DD76C + (0xB4 * dungeon), wall_color, 3);
            setColor(0x0DD76E + (0xB4 * dungeon), wall_color, 5);

            //Decoration?


            //WHAT ARE THOSE !!
            //setColor((0x0DD7DA + (0xB4 * dungeon)), wall_color, (byte)(shade - (0 * 4)));
            //setColor((0x0DD7DA + 2 + (0xB4 * dungeon)), wall_color, (byte)(shade - (1 * 4)));
        }

        public void randomize_floors(int dungeon)
        {


            Color floor_color1 = Color.FromArgb(60 + rand.Next(180), 60 + rand.Next(180), 60 + rand.Next(180));
            Color floor_color2 = Color.FromArgb(60 + rand.Next(180), 60 + rand.Next(180), 60 + rand.Next(180));
            Color floor_color3 = Color.FromArgb(60 + rand.Next(180), 60 + rand.Next(180), 60 + rand.Next(180));

            /*if (dungeon == 7)
            {
                Console.WriteLine("Dungeon = 7");
                for (int i = 0; i < 3; i++)
                {
                    setColor(0x0DD764 + (0xB4 * dungeon) + (i * 2), floor_color1, (byte)((shade1-1 ) - (i * 3)));
                    setColor(0x0DD782 + (0xB4 * dungeon) + (i * 2), floor_color1, (byte)((shade1) - (i * 3)));
                    setColor(0x0DD7A0 + (0xB4 * dungeon) + (i * 2), floor_color1, (byte)((shade2-1 ) - (i * 3)));
                    setColor(0x0DD7BE + (0xB4 * dungeon) + (i * 2), floor_color1, (byte)((shade2) - (i * 3)));

                    if (i <= 1)
                    {
                        setColor((0x0DD764 + (0xB4 * dungeon) + 8) + (i * 2), floor_color1, (byte)((shade1-1 ) - (i * 3)));
                        setColor((0x0DD782 + (0xB4 * dungeon) + 8) + (i * 2), floor_color1, (byte)((shade1) - (i * 3)));
                        setColor((0x0DD7A0 + (0xB4 * dungeon) + 8) + (i * 2), floor_color1, (byte)((shade2-1 ) - (i * 3)));
                        setColor((0x0DD7BE + (0xB4 * dungeon) + 8) + (i * 2), floor_color1, (byte)((shade2) - (i * 3)));
                    }
                }
                setColor(0x0DD7E2 + (0xB4 * dungeon), floor_color1, 3);
                setColor(0x0DD796 + (0xB4 * dungeon), floor_color1, 3);
            }
            else
            {*/
                for (int i = 0; i < 3; i++)
                {
                byte shadex = (byte)(6 - (i * 2));
                    setColor(0x0DD764 + (0xB4 * dungeon) + (i * 2), floor_color1, shadex);
                    setColor(0x0DD782 + (0xB4 * dungeon) + (i * 2), floor_color1, (byte)(shadex+3));

                setColor(0x0DD7A0 + (0xB4 * dungeon) + (i * 2), floor_color2, shadex);
                setColor(0x0DD7BE + (0xB4 * dungeon) + (i * 2), floor_color2, (byte)(shadex + 3));
                //setColor(0x0DD7A0 + (0xB4 * dungeon) + (i * 2), floor_color2, (byte)((shade2-1 ) - (i * 3)));
                //setColor(0x0DD7BE + (0xB4 * dungeon) + (i * 2), floor_color2, (byte)((shade2) - (i * 3)));

                /*if (i <= 1)
                {
                    setColor((0x0DD764 + (0xB4 * dungeon) + 8) + (i * 2), floor_color3, (byte)((shade3-1 ) - (i * 3)));
                    setColor((0x0DD782 + (0xB4 * dungeon) + 8) + (i * 2), floor_color3, (byte)((shade3) - (i * 3)));
                    setColor((0x0DD7A0 + (0xB4 * dungeon) + 8) + (i * 2), floor_color3, (byte)((shade3-1 ) - (i * 3)));
                    setColor((0x0DD7BE + (0xB4 * dungeon) + 8) + (i * 2), floor_color3, (byte)((shade3 ) - (i * 3)));
                }*/
            }

                setColor(0x0DD7E2 + (0xB4 * dungeon), floor_color3, 3);
                setColor(0x0DD796 + (0xB4 * dungeon), floor_color3, 4);
            //}
        }

        public void setColor(int address, Color col, byte shade)
        {

            int r = col.R;
            int g = col.G;
            int b = col.B;

            for (int i = 0; i < shade; i++)
            {
                r = (r - (r / 5));
                g = (g - (g/ 5));
                b = (b - (b / 5));
            }
            r = (r / 8);
            g = (g / 8);
            b = (b / 8);
            short s = (short)(((b) << 10) | ((g) << 5) | ((r) << 0));

            ROM_DATA[address] = (byte)(s & 0x00FF);
            ROM_DATA[address+1] = (byte)((s >> 8) & 0x00FF);
           

        }
        
        public void Randomize_Sprites_Palettes()
        {
            //Do not change color of collectible items
            for (int j = 0; j < 0xF3; j++)
            {
                if (j <= 0xD7 || j >= 0xE7)
                {
                    if (optionFlags.AlternateGfx == true)
                    {
                        ROM_DATA[0x6B359 + j] = (byte)((ROM_DATA[0x6B359 + j] & 0xF1) + (rand.Next(15) & 0x0E));
                    }
                }
            }
            //sprite_palette_new();

        }

        public void Randomize_Overworld_Palettes()
        {
            Color grass = Color.FromArgb(60 + (rand.Next(155)), 60 + rand.Next(155), 60 + rand.Next(155));
            Color grass2 = Color.FromArgb(60 + (rand.Next(155)), 60 + rand.Next(155), 60 + rand.Next(155));
            Color grass3 = Color.FromArgb(60 + (rand.Next(155) ), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dirt = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dirt2 = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            //Color grass = Color.FromArgb(230, 230, 230);
            //Color dirt = Color.FromArgb(140,120,64);

            // TODO: unused?
            Color wall = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));

            // TODO: unused?
            Color roof = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));


            Color btreetrunk = Color.FromArgb(172, 144, 96);

            // TODO: unused?
            Color treetrunk = Color.FromArgb(btreetrunk.R - 40 + rand.Next(80), btreetrunk.G - 20 + rand.Next(30), btreetrunk.B - 30 + rand.Next(60));


            Color treeleaf = Color.FromArgb(grass.R-20 + rand.Next(30), grass.G-20+rand.Next(30), grass.B-20+rand.Next(30));

            // TODO: unused?
            Color bridge = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));


            setColor(0x05FEA9, grass, 0);//hardcoded grass palette LW

            setColor(0x0DD4AC, grass, 2); //desert shadow
            setColor(0x0DE6DE, grass2, 2);
            setColor(0x0DE75C, grass2, 2);
            setColor(0x0DE786, grass2, 2);
            setColor(0x0DE794, grass2, 2);
            setColor(0x0DE99A, grass2, 2);

            setColor(0x0DE6E0 , grass2, 1);
            setColor(0x0DE6E2 , grass2, 0);

            setColor(0x0DD4AE , grass2, 1);
            setColor(0x0DE6E0 , grass2, 1);
            setColor(0x0DE9FA , grass2, 1);
            setColor(0x0DEA0E , grass2, 1);

            setColor(0x0DE9FE, grass2, 0);


            setColor(0x0DD3D2, grass2, 2);
            setColor(0x0DE88C, grass2, 2);
            setColor(0x0DE8A8, grass2, 2);
            setColor(0x0DE9F8, grass2, 2);
            setColor(0x0DEA4E, grass2, 2);
            setColor(0x0DEAF6, grass2, 2);
            setColor(0x0DEB2E, grass2, 2);
            setColor(0x0DEB4A, grass2, 2);







            int i = 0;
                setColor(0x0DE892 + (i * 70), grass, 1);
                setColor(0x0DE886 + (i * 70), grass, 0);

                setColor(0x0DE6D0 + (i * 70), grass, 1);//grass shade
                setColor(0x0DE6D2 + (i * 70), grass, 0); //grass



                setColor(0x0DE6FA + (i * 70), grass, 3);
                setColor(0x0DE6FC + (i * 70), grass, 0);//grass shade2
                setColor(0x0DE6FE + (i * 70), grass, 0);//??

                setColor(0x0DE884 + (i * 70), grass, 4);//tree shadow
                

                setColor(0x0DE70A + (i * 70), grass, 0); //grass?
                setColor(0x0DE708 + (i * 70), grass, 2); //bush?

                setColor(0x0DE70C + (i * 70), grass, 1); //bush?

                //Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
                setColor(0x0DE6D4 + (i * 70), dirt, 2);

                setColor(0x0DE6CA + (i * 70), dirt, 5);
                setColor(0x0DE6CC + (i * 70), dirt, 4);
                setColor(0x0DE6CE + (i * 70), dirt, 3);
                setColor(0x0DE6E2 + (i * 70), dirt, 2);

                setColor(0x0DE6D8 + (i * 70), dirt, 5);
                setColor(0x0DE6DA + (i * 70), dirt, 4);
                setColor(0x0DE6DC + (i * 70), dirt, 2);
                setColor(0x0DE6F0 + (i * 70), dirt, 2);

                setColor(0x0DE6E6 + (i * 70), dirt, 5);
                setColor(0x0DE6E8 + (i * 70), dirt, 4);
                setColor(0x0DE6EA + (i * 70), dirt, 2);
                setColor(0x0DE6EC + (i * 70), dirt, 4);
                setColor(0x0DE6EE + (i * 70), dirt, 2);
                setColor(0x0DE6F0 + (i * 70), dirt, 2);


            //lake borders
            setColor(0x0DE91E, grass, 0);
            setColor(0x0DE920, dirt, 2);
            setColor(0x0DE916, dirt, 3);

            setColor(0x0DE92C, grass, 0);
            setColor(0x0DE93A, grass, 0);
            setColor(0x0DE93C, dirt, 2);


            setColor(0x0DE91C, grass, 1);

            setColor(0x0DE92A, grass, 1);
            setColor(0x0DE938, grass, 1);//darker?

            //zora domain
            setColor(0x0DEA1C, grass, 0);
            setColor(0x0DEA2A, grass, 0);
            setColor(0x0DEA30, grass, 0);

            setColor(0x0DEA2E, dirt, 5);
            setColor(0X067FE1, grass, 3); //Zora Domain Shadow

            setColor(0X0DE6D0, grass, 3); //Test2
            setColor(0x0DE884, grass, 3);
            setColor(0x0DE8AE, grass, 3);
            setColor(0x0DE8BE, grass, 3);
            setColor(0x0DE8E4, grass, 3);
            setColor(0x0DE938, grass, 3);
            setColor(0x0DE9C4, grass, 3);
            //Nothing Happen : 0x01E0F8,0x04E2DB,0x05FE75,0X067FAF
            //map changed : 0x0216B8

            setColor(0x0DE6D0, grass, 4);//tree shadow
                                         /*setColor(0x0DE87C, bridge, 6);
                                             setColor(0x0DE87E, bridge, 4);
                                             setColor(0x0DE880, bridge, 2);
                                             setColor(0x0DE882, bridge, 0);

                                             setColor(0x0DE86E, wall, 6);
                                             setColor(0x0DE870, wall, 4);
                                             setColor(0x0DE872, wall, 2);
                                             setColor(0x0DE878, wall, 0);


                                             */
            /*setColor(0x0DE88A, treetrunk, 2);
            setColor(0x0DE88C, treetrunk, 1);
            setColor(0x0DE88E, treetrunk, 0);
            */
            setColor(0x0DE890, treeleaf, 1);
            setColor(0x0DE894, treeleaf, 0);

            /*setColor(0x0DE874, roof, 4);
            setColor(0x0DE876, roof, 0);*/


            Color water = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            setColor(0x0DE924, water, 3);//water dark
            setColor(0x0DE668, water, 3);//water dark
            setColor(0x0DE66A, water, 2);//water light
            setColor(0x0DE670, water, 1); // water light
            setColor(0x0DE918, water, 1);// water light
            setColor(0x0DE66C, water, 0); //water lighter
            setColor(0x0DE91A, water, 0); //water lighter
            setColor(0x0DE92E, water, 1);// water light

            setColor(0x0DE66E, dirt, 3); //ground dark

            setColor(0x0DE672, dirt, 2);  // ground light


            setColor(0x0DE932, dirt, 4);  //ground darker
            setColor(0x0DE934, dirt, 3);  //ground dark
            setColor(0x0DE936, dirt, 2);  // ground light
            setColor(0x0DE93C, dirt, 1);  // ground lighter

            setColor(0x0DE756, dirt2, 4);
            setColor(0x0DE764, dirt2, 4);
            setColor(0x0DE772, dirt2, 4);
            setColor(0x0DE994, dirt2, 4);
            setColor(0x0DE9A2, dirt2, 4);

            setColor(0x0DE758, dirt2, 3);
            setColor(0x0DE766, dirt2, 3);
            setColor(0x0DE774, dirt2, 3);
            setColor(0x0DE996, dirt2, 3);
            setColor(0x0DE9A4, dirt2, 3);


            setColor(0x0DE75A, dirt2, 2);
            setColor(0x0DE768, dirt2, 2);
            setColor(0x0DE776, dirt2, 2);
            setColor(0x0DE778, dirt2, 2);
            setColor(0x0DE998, dirt2, 2);
            setColor(0x0DE9A6, dirt2, 2);


            setColor(0x0DE9AC, dirt2, 1);
            setColor(0x0DE99E, dirt2, 1);
            setColor(0x0DE760, dirt2, 1);
            setColor(0x0DE77A, dirt2, 1);
            setColor(0x0DE77C, dirt2, 1);
            setColor(0x0DE798, dirt2, 1);
            setColor(0x0DE664, dirt2, 1);
            setColor(0x0DE980, dirt2, 1);



            setColor(0x0DE75C, grass3, 2);
            setColor(0x0DE786, grass3, 2);
            setColor(0x0DE794, grass3, 2);
            setColor(0x0DE99A, grass3, 2);

            setColor(0x0DE75E, grass3, 1);
            setColor(0x0DE788, grass3, 1);
            setColor(0x0DE796, grass3, 1); 
            setColor(0x0DE99C, grass3, 1);


            Color clouds = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            setColor(0x0DE76A, clouds,2);
            setColor(0x0DE9A8, clouds,2);

            setColor(0x0DE76E, clouds,0);
            setColor(0x0DE9AA, clouds,0);
            //setColor(0x0DE8E8, clouds,0);
            setColor(0x0DE8DA, clouds,0);
            setColor(0x0DE8D8, clouds,0);
            setColor(0x0DE8D0, clouds,0);

            setColor(0x0DE98C, clouds, 2);
            setColor(0x0DE990, clouds, 0);



            //DW
            Color dwdirt = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dwgrass = Color.FromArgb(60 + (rand.Next(155)), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dwwater = Color.FromArgb(60 + (rand.Next(155)), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dwtree = Color.FromArgb(dwgrass.R - 20 + rand.Next(30), dwgrass.G - 20 + rand.Next(30), dwgrass.B - 20 + rand.Next(30));


            setColor(0x05FEB3, dwgrass, 1);//hardcoded grass color in dw


            setColor(0x0DEB34, dwtree, 4);
            setColor(0x0DEB30, dwtree, 3);
            setColor(0x0DEB32, dwtree, 1);

            //dwdirt - dark to light
            setColor(0x0DE710, dwdirt, 5);
            setColor(0x0DE71E, dwdirt, 5);
            setColor(0x0DE72C, dwdirt, 5);
            setColor(0x0DEAD6, dwdirt, 5);

            setColor(0x0DE712, dwdirt, 4);
            setColor(0x0DE720, dwdirt, 4);
            setColor(0x0DE72E, dwdirt, 4);
            setColor(0x0DE660, dwdirt, 4);
            setColor(0x0DEAD8, dwdirt, 4);

            setColor(0x0DEADA, dwdirt, 3);
            setColor(0x0DE714, dwdirt, 3);
            setColor(0x0DE722, dwdirt, 3);
            setColor(0x0DE730, dwdirt, 3);
            setColor(0x0DE732, dwdirt, 3);

            setColor(0x0DE734, dwdirt, 2);
            setColor(0x0DE736, dwdirt, 2);
            setColor(0x0DE728, dwdirt, 2);
            setColor(0x0DE71A, dwdirt, 2);
            setColor(0x0DE664, dwdirt, 2);
            setColor(0x0DEAE0, dwdirt, 2);


            //grass
            setColor(0x0DE716, dwgrass, 3);
            setColor(0x0DE740, dwgrass, 3);
            setColor(0x0DE74E, dwgrass, 3);
            setColor(0x0DEAC0, dwgrass, 3);
            setColor(0x0DEACE, dwgrass, 3);
            setColor(0x0DEADC, dwgrass, 3);
            setColor(0x0DEB24, dwgrass, 3);

            setColor(0x0DE752, dwgrass, 2);

            setColor(0x0DE718, dwgrass, 1);
            setColor(0x0DE742, dwgrass, 1);
            setColor(0x0DE750, dwgrass, 1);
            setColor(0x0DEB26, dwgrass, 1);
            setColor(0x0DEAC2, dwgrass, 1);
            setColor(0x0DEAD0, dwgrass, 1);
            setColor(0x0DEADE, dwgrass, 1);



            //water

            setColor(0x0DE65A, dwwater, 5); //very dark water

            setColor(0x0DE65C, dwwater, 3); //main water color
            setColor(0x0DEAC8, dwwater, 3); //main water color
            setColor(0x0DEAD2, dwwater, 2); //main water color
            setColor(0x0DEABC, dwwater, 2);//light
            setColor(0x0DE662, dwwater, 2); //light
            setColor(0x0DE65E, dwwater, 1); //lighter
            setColor(0x0DEABE, dwwater, 1);//lighter


            //Death Mountain



            //dw dm
            //dirt
            Color dwdmdirt = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dwdmgrass = Color.FromArgb(60 + (rand.Next(155)), 60 + rand.Next(155), 60 + rand.Next(155));
            setColor(0x0DE79A, dwdmdirt, 6); //super dark (6)
            setColor(0x0DE7A8, dwdmdirt, 6);
            setColor(0x0DE7B6, dwdmdirt, 6);
            setColor(0x0DEB60, dwdmdirt, 6);
            setColor(0x0DEB6E, dwdmdirt, 6);
            setColor(0x0DE93E, dwdmdirt, 6);
            setColor(0x0DE94C, dwdmdirt, 6);
            setColor(0x0DEBA6, dwdmdirt, 6);

            setColor(0x0DE79C, dwdmdirt, 4); //dark (4)
            setColor(0x0DE7AA, dwdmdirt, 4);
            setColor(0x0DE7B8, dwdmdirt, 4);
            setColor(0x0DE7BE, dwdmdirt, 4);
            setColor(0x0DE7CC, dwdmdirt, 4);
            setColor(0x0DE7DA, dwdmdirt, 4);
            setColor(0x0DEB70, dwdmdirt, 4);
            setColor(0x0DEBA8, dwdmdirt, 4);
            setColor(0x0DEB72, dwdmdirt, 3);
            setColor(0x0DEB74, dwdmdirt, 3);
            //light (3)
            setColor(0x0DE79E, dwdmdirt, 3);
            setColor(0x0DE7AC, dwdmdirt, 3);
            setColor(0x0DEB6A, dwdmdirt, 3);
            setColor(0x0DE948, dwdmdirt, 3);
            setColor(0x0DE956, dwdmdirt, 3);
            setColor(0x0DE964, dwdmdirt, 3);
            setColor(0x0DEBAA, dwdmdirt, 3);
            setColor(0x0DE7A0, dwdmdirt, 3);
            setColor(0x0DE7BC, dwdmgrass, 3);

            //lighter (2)
            setColor(0x0DEBAC, dwdmdirt, 2);
            
            setColor(0x0DE7AE, dwdmdirt, 2);
            setColor(0x0DE7C2, dwdmdirt, 2);
            setColor(0x0DE7A6, dwdmdirt, 2);
            setColor(0x0DEB7A, dwdmdirt, 2);
            setColor(0x0DEB6C, dwdmdirt, 2);
            setColor(0x0DE7C0, dwdmdirt, 2);

            //grass
            setColor(0x0DE7A2, dwdmgrass, 3);
            setColor(0x0DE7BE, dwdmgrass, 3);
            setColor(0x0DE7CC, dwdmgrass, 3);
            setColor(0x0DE7DA, dwdmgrass, 3);
            setColor(0x0DEB6A, dwdmgrass, 3);
            setColor(0x0DE948, dwdmgrass, 3);
            setColor(0x0DE956, dwdmgrass, 3);
            setColor(0x0DE964, dwdmgrass, 3);

            
            setColor(0x0DE7CE, dwdmgrass, 1);
            setColor(0x0DE7A4, dwdmgrass, 1);
            setColor(0x0DEBA2, dwdmgrass, 1);
            setColor(0x0DEBB0, dwdmgrass, 1);

            Color dwdmclouds1 = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            Color dwdmclouds2 = Color.FromArgb(60 + rand.Next(155), 60 + rand.Next(155), 60 + rand.Next(155));
            //clouds 1
            setColor(0x0DE644, dwdmclouds1, 2); //dark
            setColor(0x0DEB84, dwdmclouds1, 2);

             setColor(0x0DE648, dwdmclouds1, 1); //light dark
             setColor(0x0DEB88, dwdmclouds1, 1);

            //clouds2
            setColor(0x0DEBAE,dwdmclouds2, 2); //dark
            setColor(0x0DE7B0, dwdmclouds2, 2);


            setColor(0x0DE7B4, dwdmclouds2, 0);//light dark
            setColor(0x0DEB78, dwdmclouds2, 0);
            setColor(0x0DEBB2, dwdmclouds2, 0);













        }

        public void GeneratePukeModePalettes(Random random)
        {
            //overworld
            for (int i = 0; i < 1456; i += 2)
            {
                setColor(0xDE604 + i, Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), 0);
            }


            // indoors
            for (int i = 0; i < 3600; i += 2)
            {
                setColor(0xDD734 + i, Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)), 0);
            }


        }

        public void GenerateNegativeModePalettes()
        {
            //overworld

            Color c = getColor((short)((ROM_DATA[0x05FEA9 + 1] << 8) + ROM_DATA[0x05FEA9]));
            Color c2 = Color.FromArgb((c.R ^ 0xFF), (c.G ^ 0xFF), (c.B ^ 0xFF));
            setColor(0x05FEA9, c2, 0);

            for (int i = 0; i < 1456; i += 2)
            {
                c = getColor((short)((ROM_DATA[0xDE604 + i + 1] << 8) + ROM_DATA[0xDE604 + i]));
                c2 = Color.FromArgb((c.R ^ 0xFF), (c.G ^ 0xFF), (c.B ^ 0xFF));
                setColor(0xDE604 + i, c2, 0);
            }



            // indoors
            for (int i = 0; i < 3600; i += 2)
            {
                c = getColor((short)((ROM_DATA[0xDD734 + i + 1] << 8) + ROM_DATA[0xDD734 + i]));
                c2 = Color.FromArgb((c.R ^ 0xFF), (c.G ^ 0xFF), (c.B ^ 0xFF));
                setColor(0xDD734 + i, c2, 0);
            }


            //misc
            /*for (int i = 0; i < 1308; i += 2)
            {
                c = getColor((short)((ROM_DATA[0xDD218 + i + 1] << 8) + ROM_DATA[0xDD218 + i]));
                c2 = Color.FromArgb((c.R ^ 0xFF), (c.G ^ 0xFF), (c.B ^ 0xFF));
                setColor(0xDD218 + i, c2, 0);
            }
            */


        }

        public void Randomize_Sprites_HP(int rangeValue)
        {
            for (int j = 0; j < 0xF3; j++)
            {
                if (ROM_DATA[0x6B173 + j] != 0xFF)
                {
                    if (j != 0x54 && j != 0x09 && j != 0x53 && j != 0x88 && j != 0x89 && j != 0x53 && j != 0x8C && j != 0x92
                        && j != 0x70 && j != 0xBD && j != 0xBE && j != 0xBF && j != 0xCB && j != 0xCE && j != 0xA2 && j != 0xA3
                       && j != 0x8D && j != 0x7A && j != 0x7B && j != 0xCC && j != 0xCD && j != 0xA4 && j != 0xD6 && j != 0xD7)
                    {
                        int new_hp = ROM_DATA[0x6B173 + j] + rand.Next(-rangeValue, rangeValue);
                        if (new_hp >= 0xFF)
                        {
                            new_hp = 0xFF;
                        }
                        if (new_hp <= 0)
                        {
                            new_hp = 1;
                        }
                        ROM_DATA[0x6B173 + j] = (byte)new_hp;
                    }
                }
            }
        }

        public void shuffle_music()
        {
            for (int i = 0; i < 0x70; i++)
            {
                byte[] musics = { 0x03, 0x07, 0x0B, 0x0E, 0x10, 0x11, 0x12, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x22, 0x23};
                byte m = (byte)rand.Next(musics.Length);
                m = musics[m];
                ROM_DATA[0x015592+i] = m;
            }
            byte[] originalmusicafter0x85 = { 0x12, 0x1B, 0x12, 0xFF, 0xFF, 0xFF, 0x11, 0x11, 0x11, 0x11 };
            for (int i = 0; i < 0x0A; i++)
            {
                ROM_DATA[0x015602 + i] = originalmusicafter0x85[i];
            }
        }

        // TODO: unused?
        byte[] original_damage = {2,4,0,8,8,16,32,32,24,64, 32, 32, 32, 32, 32, 32 };


        public void Randomize_Sprites_DMG(bool allowZeroDamage)
        {
            for (int j = 0; j < 0xF3; j++)
            {
                if (j != 0x54 && j != 0x09 && j != 0x53 && j != 0x88 && j != 0x89 && j != 0x53 && j != 0x8C && j != 0x92
                    && j != 0x70 && j != 0xBD && j != 0xBE && j != 0xBF && j != 0xCB && j != 0xCE && j != 0xA2 && j != 0xA3 && j != 0x8D
                    && j != 0x7A && j != 0x7B && j != 0xCC && j != 0xCD && j != 0xA4 && j != 0xD6 && j != 0xD7)
                {
                    //New DMG CODE NOT WORKING
                    //ROM_DATA[0x6B266 + j] = (byte)((ROM_DATA[0x6B266 + j] & 0xF8) + (byte)(rand.Next(8)));
                    byte newDmg = (byte)(rand.Next(8));
                    if (allowZeroDamage == false)
                    {
                        if (newDmg == 2)
                        {
                            continue;
                        }
                    }
                    ROM_DATA[0x6B266 + j] = newDmg;
                }
            }
        }

        public void Set_Sprites_ZeroHP()
        {
            for (int j = 0; j < 0xF3; j++)
            {
                if (ROM_DATA[0x6B173 + j] != 0xFF)
                {
                    if (j != 0x54 && j != 0x09 && j != 0x53 && j != 0x88 && j != 0x89 && j != 0x53 && j != 0x8C && j != 0x92
                        && j != 0x70 && j != 0xBD && j != 0xBE && j != 0xBF && j != 0xCB && j != 0xCE && j != 0xA2 && j != 0xA3
                       && j != 0x8D && j != 0x7A && j != 0x7B && j != 0xCC && j != 0xCD && j != 0xA4 && j != 0xD6 && j != 0xD7)
                    {
                        int new_hp = 1;
                        ROM_DATA[0x6B173 + j] = (byte)new_hp;
                    }
                }
            }
        }

        private void BuildRandomLinkSpriteTable(Random random)
        {
            List<string> skins = Directory.GetFiles("sprites\\").ToList();
            int totalSprites = 32;
            //if(totalSprites > skins.Count)
            //{
            //    totalSprites = skins.Count;
            //}

            int i = 0;
            FileStream fsx;
            int r;

            if (optionFlags.AndyMode)
            {
                // force pug sprite
                r = skins.IndexOf(skins.Where(x => x.Contains("pug.spr")).FirstOrDefault());
                fsx = new FileStream(skins[r], FileMode.Open, FileAccess.Read);
                fsx.Read(this.ROM_DATA.romData, AddressConstants.RandomSpriteGraphicsBaseAddress + (i * 0x8000), 0x7078);
                fsx.Close();
                skins.RemoveAt(r);
                i++;
            }

            for (; i < totalSprites; i++)
            {
                r = random.Next(skins.Count);
                fsx = new FileStream(skins[r], FileMode.Open, FileAccess.Read);
                fsx.Read(this.ROM_DATA.romData, AddressConstants.RandomSpriteGraphicsBaseAddress + (i * 0x8000), 0x7078);
                fsx.Close();
                skins.RemoveAt(r);
            }
        }
    }
}
