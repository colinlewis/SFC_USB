using System;
using System.Collections.Generic;
using System.Text;

//Right now this is a static class, seems like commands should be first rate objects, rather than just the buffers,
// but think of this as a set of buffer-construction helpers.

namespace SFC_USB
{
    static public class MCTCommand
    {
   
         const byte MCT_CMD_JUMP_TO_APP = 0x02;
         const byte MCT_CMD_ERASE_APP = 0x03;
         const byte MCT_CMD_BLANK_CHECK = 0x04;
         const byte MCT_CMD_PROG_APP = 0x05;
         const byte MCT_CMD_READ_FLASH = 0x06;
         const byte MCT_CMD_FLASH_CKSUM = 0x07;
         const byte MCT_CMD_JUMP_TO_BOOT = 0x08;
         const byte MCT_CMD_SLAVE_MODE = 0x0D;
         const byte MCT_CMD_GET_STRING = 0x17;
         const byte MCT_CMD_POSN = 0x20;
         const byte MCT_CMD_TARGET = 0x21;
         const byte MCT_CMD_TRACK = 0x40;
         const byte MCT_CMD_GET_MIRRORS = 0x40;
         const byte MCT_CMD_PARAM = 0x41;
         const byte MCT_CMD_FWD_TO_SLAVE = 0x0E;

        public const byte MCT_RESP_JUMP_TO_APP = 0x82;
        public const byte MT_RESP_BLANK_CHECK = 0x84;
        public const byte MCT_RESP_PROG_APP = 0x85;
        public const byte MCT_RESP_READ_FLASH = 0x86;
        public const byte MCT_RESP_FLASH_CKSUM = 0x87;
        public const byte MCT_RESP_JUMP_TO_BOOT = 0x88;
        public const byte MCT_RESP_GET_STRING = 0x97;
        public const byte MCT_RESP_POSN = 0xA0;
        public const byte MCT_RESP_TARGET = 0xA1;
        public const byte MCT_RESP_GET_MIRRORS = 0xC0;
        public const byte MCT_RESP_PARAM = 0xC1;

        public const byte USB_CMD_GET_VSTRING = 0x17;
        public const byte USB_CMD_GET_CHAN = 0x33;
        public const byte USB_CMD_GET_MIRRORS = 0x42;
        public const byte USB_CMD_GET_STRING = 0x60;
        public const byte USB_CMD_FIELD_STATE = 0x61;
        public const byte USB_CMD_GET_FCE = 0x62;
        public const byte USB_CMD_GET_RTU = 0x63;
        public const byte USB_CMD_SEND_MCT485 = 0x64;
        public const byte USB_CMD_GET_MCT485 = 0x65;
        public const byte USB_CMD_RTC = 0x66;
        public const byte USB_CMD_DESICCANT = 0x68;
        public const byte USB_CMD_SFC_PARAM = 0x69;
        public const byte USB_CMD_MEMORY = 0x6A;
        public const byte USB_CMD_TEST = 0x6B;

        public const byte USB_RESP_GET_VSTRING = 0x97;
        public const byte USB_RESP_GET_CHAN = 0xB3;
        public const byte USB_RESP_GET_MIRRORS = 0xC2;
        public const byte USB_RESP_GET_STRING = 0xE0;
        public const byte USB_RESP_FIELD_STATE = 0xE1;
        public const byte USB_RESP_GET_FCE = 0xE2;
        public const byte USB_RESP_GET_RTU = 0xE3;
        public const byte USB_RESP_SEND_MCT485 = 0xE4;
        public const byte USB_RESP_GET_MCT485 = 0xE5;
        public const byte USB_RESP_RTC = 0xE6;
        public const byte USB_RESP_DESICCANT = 0xE8;
        public const byte USB_RESP_SFC_PARAM = 0xE9;
        public const byte USB_RESP_MEMORY = 0xEA;
        public const byte USB_RESP_TEST = 0xEB;

        // indexes into buffer for the first portion of the packet sent over USB
        public const byte USB_PACKET_STR = 0; // string we are talking to (if applicable)
        public const byte USB_PACKET_MCT = 1; // which MCT of the string (if applicable)
        public const byte USB_PACKET_LEN = 2; // length of the entire packet
        public const byte USB_PACKET_PID = 3;
        public const byte USB_PACKET_CMD = 4;
        public const byte USB_PACKET_DATA = 5; // beginning of variable length data

        public const byte BOX485_MSG_SLAVE = 0x7E;

        // address in MCT master. //TODO put in a common header or file for C and C#
        const int APP_CHECKSUM = 0xEDB8;


        static byte[] txBuffer = new byte[64];


        //TODO remove public version of TxCrcCalc() in frmMain
        static private uint TxCrcCalc(ref byte[] buffer, byte offset, byte len)
        {
            uint crc = 0x1D0F;
            for (byte i = 0; i < len; i++)
            {
                crc = (byte)(crc >> 8) | ((crc & 0xFF) << 8);
                crc ^= buffer[i + offset];
                crc ^= (crc & 0xFF) >> 4;
                crc ^= (crc << 8) << 4;
                crc ^= ((crc & 0xFF) << 4) << 1;
            }
            buffer[offset + len] = (byte)(crc >> 8);
            buffer[offset + len + 1] = (byte)crc;
            return crc;
        }


        static private byte Conv2Hex(char a, char b)
        {
            byte hex;

            hex = 0;
            if (a >= '0' && a <= '9')
                hex = (byte)(a - '0');
            else if (a >= 'A' && a <= 'F')
                hex = (byte)(10 + a - 'A');

            hex *= 16;
            if (b >= '0' && b <= '9')
                hex += (byte)(b - '0');
            else if (b >= 'A' && b <= 'F')
                hex += (byte)(10 + b - 'A');

            return hex;
        }


        static public byte [] SLAVE_MODE(byte mctNum, byte txPid, byte magic)
        {
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 7; //MCT packet length
            txBuffer[8] = MCT_CMD_SLAVE_MODE;
            txBuffer[9] = magic;// magic - sometimes x80 sometimes x1f

            TxCrcCalc(ref txBuffer, 5, 5);
 
            txBuffer[USB_PACKET_MCT] = mctNum;
            txBuffer[USB_PACKET_LEN] = 14;

            return txBuffer;
        }

        // Jump to Boot flash
        static public byte[] SLAVE_JUMP_TO_BOOT(byte mctNum, byte txPid)
        {
            txBuffer[9] = BOX485_MSG_SLAVE;
            txBuffer[10] = txPid;
            txBuffer[11] = 6;
            txBuffer[12] = MCT_CMD_JUMP_TO_BOOT;
            TxCrcCalc(ref txBuffer, 9, 4);
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 12;
            txBuffer[8] = MCT_CMD_FWD_TO_SLAVE;
            TxCrcCalc(ref txBuffer, 5, 10);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 19;
            return txBuffer;
        }

        static public byte[] SLAVE_ERASE_APP(byte mctNum, byte txPid)
        {

            txBuffer[9] = BOX485_MSG_SLAVE;
            txBuffer[10] = txPid;
            txBuffer[11] = 6;
            txBuffer[12] = MCT_CMD_ERASE_APP;
            TxCrcCalc(ref txBuffer, 9, 4);
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 12;
            txBuffer[8] = MCT_CMD_FWD_TO_SLAVE;
            TxCrcCalc(ref txBuffer, 5, 10);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 19;
            return txBuffer;
        }

        static public byte[] SLAVE_PROG_APP(byte mctNum, byte txPid, ref char[] record)
        {

            txBuffer[9] = BOX485_MSG_SLAVE;
            txBuffer[10] = txPid;

            byte len = (byte)(Conv2Hex(record[2], record[3]) - 3);
            txBuffer[11] = (byte)(len + 9);
            txBuffer[12] = MCT_CMD_PROG_APP;
            txBuffer[13] = Conv2Hex(record[4], record[5]); //address high byte
            txBuffer[14] = Conv2Hex(record[6], record[7]); //address low byte
            txBuffer[15] = len;
            for (byte i = 0; i < len; i++)
            {
                txBuffer[16 + i] = Conv2Hex(record[8 + i + i], record[9 + i + i]);
            }
            TxCrcCalc(ref txBuffer, 9, (byte)(7 + len)); // checksum of slave message

            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = (byte)(15 + len);
            txBuffer[8] = MCT_CMD_FWD_TO_SLAVE;
            TxCrcCalc(ref txBuffer, 5, (byte)(13 + len)); //checksum of MCT message, containing slave message
            txBuffer[ MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[ MCTCommand.USB_PACKET_LEN] = (byte)(6 + 7 + 9 + len);
            return txBuffer;
        }

        static public byte[] SLAVE_FLASH_CKSUM(byte txPid, byte mctNum)
        {

            txBuffer[9] = BOX485_MSG_SLAVE;
            txBuffer[10] = txPid;
            txBuffer[11] = 6;
            txBuffer[12] = MCT_CMD_FLASH_CKSUM;
            TxCrcCalc(ref txBuffer, 9, 4); // checksum of slave message
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 12;
            txBuffer[8] = MCT_CMD_FWD_TO_SLAVE;
            TxCrcCalc(ref txBuffer, 5, 10); //checksum of MCT message, including slave
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 19;

            return txBuffer;
        }

        static public byte[] SLAVE_JUMP_TO_APP(byte txPid, byte mctNum)
        {

            txBuffer[9] = BOX485_MSG_SLAVE;
            txBuffer[10] = txPid;
            txBuffer[11] = 6;
            txBuffer[12] = MCT_CMD_JUMP_TO_APP;
            TxCrcCalc(ref txBuffer, 9, 4);
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 12;
            txBuffer[8] = MCT_CMD_FWD_TO_SLAVE;
            TxCrcCalc(ref txBuffer, 5, 10);
            txBuffer[ MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[ MCTCommand.USB_PACKET_LEN] = 19;

            return txBuffer;
        }

        static public byte[] PROG_APP(byte mctNum, byte txPid, ref char[] record, ref byte[] mctFlashMem)
        {

            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            byte len = (byte)(Conv2Hex(record[2], record[3]) - 3);
            txBuffer[7] = (byte)(len + 9);
            txBuffer[8] = MCT_CMD_PROG_APP;
            txBuffer[9] = Conv2Hex(record[4], record[5]); //address high byte
            txBuffer[10] = Conv2Hex(record[6], record[7]);//address low byte
            int addr = (((int)txBuffer[9]) << 8) + ((int)txBuffer[10]);
            txBuffer[11] = len;
            for (byte i = 0; i < len; i++)
            {
                byte flash = Conv2Hex(record[8 + i + i], record[9 + i + i]);
                mctFlashMem[addr + i] = flash;
                txBuffer[12 + i] = flash;
            }
            TxCrcCalc(ref txBuffer, 5, (byte)(7 + len));
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = (byte)(7 + 9 + len);
            return txBuffer;
        }

        //write checksum value to MCT master
        static public byte[] PROG_APP_CHECKSUM(byte mctNum, byte txPid, UInt32 cksum)
        {

            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = (byte)(4 + 9);
            txBuffer[8] = MCT_CMD_PROG_APP;
            txBuffer[9] = (byte)(APP_CHECKSUM >> 8);
            txBuffer[10] = (byte)(APP_CHECKSUM & 0xFF);
            txBuffer[11] = 4;
            txBuffer[12] = (byte)(cksum >> 24);
            txBuffer[13] = (byte)(cksum >> 16);
            txBuffer[14] = (byte)(cksum >> 8);
            txBuffer[15] = (byte)(cksum);
            TxCrcCalc(ref txBuffer, 5, (byte)(7 + 4));
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = (byte)(7 + 9 + 4);
            return txBuffer;
        }

        // set position for mirror (indicated in mystery) - how is this different from target???
        static public byte[] POSN_WRITE(byte mctNum, byte txPid, byte mystery, UInt16 position)
        {
            txBuffer [MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[ MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[ MCTCommand.USB_PACKET_DATA + 2] = 11;
            txBuffer[ MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_POSN;
            txBuffer[ MCTCommand.USB_PACKET_DATA + 4] = mystery;
            txBuffer[ MCTCommand.USB_PACKET_DATA + 5] = (byte)(position >> 8);
            txBuffer[ MCTCommand.USB_PACKET_DATA + 6] = (byte)(position & 0xFF);
            txBuffer[ MCTCommand.USB_PACKET_DATA + 7] = (byte)(position >> 8);
            txBuffer[ MCTCommand.USB_PACKET_DATA + 8] = (byte)(position & 0xFF);
            TxCrcCalc(ref txBuffer, 5, 9);
            txBuffer[ MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[ MCTCommand.USB_PACKET_LEN] = 18;
            return txBuffer;
        }

        static public byte[] POSN_READ(byte mctNum, byte txPid, byte mystery)
        {
            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 7;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_POSN;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = mystery;
            TxCrcCalc(ref txBuffer, 5, 5);
            txBuffer[MCTCommand.USB_PACKET_LEN] = 14;
            return txBuffer;
        }

        //Set target position for mirror - mystery seems to indicate which mirror
        static public byte[] TARGET_WRITE(byte mctNum, byte txPid, byte mystery, UInt16 target)
        {
            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 11;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_TARGET;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = mystery;
            txBuffer[MCTCommand.USB_PACKET_DATA + 5] = (byte)(target >> 8);
            txBuffer[MCTCommand.USB_PACKET_DATA + 6] = (byte)(target & 0xFF);
            txBuffer[MCTCommand.USB_PACKET_DATA + 7] = (byte)(target >> 8);
            txBuffer[MCTCommand.USB_PACKET_DATA + 8] = (byte)(target & 0xFF);
            TxCrcCalc(ref txBuffer, 5, 9);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 18;
            return txBuffer;
        }

        static public byte[] TARGET_READ(byte mctNum, byte txPid, byte mystery)
        {
            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 7;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_TARGET;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = mystery;
            TxCrcCalc(ref txBuffer, 5, 5);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 14;
            return txBuffer;
        }

        static public byte[] TRACK(byte mctNum, byte txPid, byte whichMirror, byte trackNum)
        {
                txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 1] = txPid;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 2] = 8;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_TRACK;
                txBuffer[MCTCommand.USB_PACKET_DATA + 4] = whichMirror;
                txBuffer[MCTCommand.USB_PACKET_DATA + 5] = trackNum;
                TxCrcCalc(ref txBuffer, 5, 6);
                txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
                txBuffer[ MCTCommand.USB_PACKET_LEN] = 15;
                return txBuffer;
        }

        static public byte[] PARAM_WRITE (byte mctNum, byte txPid, byte mctMaxAddr, byte paramNum, Int16 param)
        {

            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 9;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_PARAM;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = paramNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 5] = (byte)(param >> 8);
            txBuffer[MCTCommand.USB_PACKET_DATA + 6] = (byte)(param);
            TxCrcCalc(ref txBuffer, 5, 7);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctMaxAddr;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 16;
            return txBuffer;
        }
        //read
        static public byte[] PARAM_READ(byte mctNum, byte txPid, byte paramNum)
        {

            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 7; //Magic!
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_PARAM;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = (byte)paramNum;
            TxCrcCalc(ref txBuffer, 5, 5);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 14;
            return txBuffer;
        }

        static public byte[] JUMP_TO_BOOT(byte mctNum, byte txPid)
        {
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 6;
            txBuffer[8] = MCT_CMD_JUMP_TO_BOOT;
            TxCrcCalc(ref txBuffer, 5, 4);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 13;
            return txBuffer;
        }

        static public byte[] ERASE_APP(byte mctNum, byte txPid)
        {
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 6;
            txBuffer[8] = MCT_CMD_ERASE_APP;
            TxCrcCalc(ref txBuffer, 5, 4);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 13;
            return txBuffer;
        }

        static public byte[] JUMP_TO_APP(byte mctNum, byte txPid)
        {
            txBuffer[5] = mctNum;
            txBuffer[6] = txPid;
            txBuffer[7] = 6;
            txBuffer[8] = MCT_CMD_JUMP_TO_APP;
            TxCrcCalc(ref txBuffer, 5, 4);
            txBuffer[MCTCommand.USB_PACKET_MCT] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_LEN] = 13;
            return txBuffer;
        }

        static public byte[] GET_STRING(byte mctNum, byte txPid)
        {

            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 7;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_GET_STRING;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = 0; //TODO rename MASTER
            TxCrcCalc(ref txBuffer, 5, 5);
            txBuffer[MCTCommand.USB_PACKET_LEN] = 14;
            return txBuffer;
        }

        static public byte[] SLAVE_GET_STRING(byte mctNum, byte txPid)
        {

            txBuffer[MCTCommand.USB_PACKET_DATA] = mctNum;
            txBuffer[MCTCommand.USB_PACKET_DATA + 1] = txPid;
            txBuffer[MCTCommand.USB_PACKET_DATA + 2] = 7;
            txBuffer[MCTCommand.USB_PACKET_DATA + 3] = MCT_CMD_GET_STRING;
            txBuffer[MCTCommand.USB_PACKET_DATA + 4] = 1; //TODO RENAME SLAVE
            TxCrcCalc(ref txBuffer, 5, 5);
            txBuffer[MCTCommand.USB_PACKET_LEN] = 14;
            return txBuffer;
        }

   }
}
           
