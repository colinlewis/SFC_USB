/// <summary>
/// Project: SFC_USB_cs
/// 
/// ***********************************************************************
/// Software License Agreement
///
/// Licensor grants any person obtaining a copy of this software ("You") 
/// a worldwide, royalty-free, non-exclusive license, for the duration of 
/// the copyright, free of charge, to store and execute the Software in a 
/// computer system and to incorporate the Software or any portion of it 
/// in computer programs You write.   
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// ***********************************************************************
/// 
/// Author               Date        Version
/// Jan Axelson          6/3/08     1.3
///                      8/8/08     1.3.1 Because WinUsb_QueryDeviceInformation appears
///                                       unreliable for detecting device speed, I
///                                       commented out the call to obtain device speed. 
///                      10/1/08    1.4   Minor edits     
///                      10/29/08   1.5   Minor edits mainly for 64-bit compatibility
///                      11/1/08    1.6   Minor edits  
///                      11/9/08    1.7   Minor edits    
///                      2/10/09    1.8   Changes to WinUsb_ReadPipe parameters.
///                      2/11/09    1.81  Moved Free_ and similar to Finally blocks
///                                       to ensure they execute.
///                      8/15/10    2.0   Modified for Chromasun SFC
/// 
/// 
/// This software was created using Visual Studio 2008 Standard Edition with .NET Framework 2.0.
/// 
/// Purpose: 
/// Demonstrates USB communications using the Microsoft WinUSB driver.
/// 
/// Requirements:
/// Windows XP or later and an attached USB device that uses the WinUSB driver.
/// 
/// Description:
/// Finds an attached device whose INF file contains a specific device interface GUID.
/// Enables sending and receiving data via bulk, interrupt, and control transfers.
/// 
/// Uses RegisterDeviceNotification() and WM_DEVICE_CHANGE messages
/// to detect when a device is attached or removed.
/// 
/// For bulk and interrupt transfers, the application uses a Delegate and the BeginInvoke 
/// and EndInvoke methods to read data asynchronously, so the application's main thread 
/// doesn't have to wait for the device to return data. A callback routine uses 
/// marshaling to send data to the form, whose code runs in a different thread. 
/// </summary>

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SFC_USB
{
    /// <summary>
    /// The application's form. Buttons enable detecting a device
    /// and initiating transfers.
    /// </summary>
    /// 
    internal class frmMain : System.Windows.Forms.Form
    {
        #region '"Windows Form Designer generated code "'
        private CheckBox[,] checkMctStore = new System.Windows.Forms.CheckBox[4, 10];

        public StreamWriter[,] mctFile = new StreamWriter[4, 10];
        public StreamWriter sfcFile;
        public DateTime dateTimeMct;
        public DateTime dateTimeResetSFC = DateTime.MaxValue; // forever in the future.
        public int intervalMct;
        public DateTime dateTimeSfc;
        public int intervalSfc;

        public byte fieldState;
        public byte fceInputState;
        public byte fceOutputState;
        public int fieldDNI;
        public int fceRtd;
        public int[] rtu = new int[4];
        public int[] fceAD = new int[8];
        public byte[] stringState = new byte[4];
        public byte[] mctMaxAddr = new byte[4];
        public int[, ,] mctChan = new int[4, 10, 16];
        public int[, ,] mctPosn = new int[4, 10, 4];
        public byte[, ,] mctTrack = new byte[4, 10, 2];
        public byte[, ,] mctSensor = new byte[4, 10, 2];
        const byte MIRROR1 = 0;
        const byte MIRROR2 = 1;
        const byte POSN1A = 0;
        const byte POSN1B = 1;
        const byte POSN2A = 2;
        const byte POSN2B = 3;
        public Boolean pollDataValid;
        public byte pollState;
        public byte pollStringNum;
        public byte pollMctNum;

        const int NUM_STRING_BOXES = 16;
        private Label[] strMctLabel = new System.Windows.Forms.Label[10];
        private ListBox lstResults;
        private TextBox[,] strMctText = new System.Windows.Forms.TextBox[10, NUM_STRING_BOXES];
        const int PARAM_NUM_VALUES = 14;
        private Label[] paramLabel = new System.Windows.Forms.Label[PARAM_NUM_VALUES];
        private Label[] paramMctLabel = new System.Windows.Forms.Label[10];
        private TextBox[,] paramText = new System.Windows.Forms.TextBox[10, PARAM_NUM_VALUES];
        private TextBox[] paramDefaultText = new System.Windows.Forms.TextBox[PARAM_NUM_VALUES];
        private CheckBox[] paramEnable = new System.Windows.Forms.CheckBox[PARAM_NUM_VALUES];
        private Label[] labelMctVer = new System.Windows.Forms.Label[10];
        private TextBox[] textMctMver = new System.Windows.Forms.TextBox[10];
        private TextBox[] textMctSver = new System.Windows.Forms.TextBox[10];
        private CheckBox[] checkMctMvalid = new System.Windows.Forms.CheckBox[10];
        private CheckBox[] checkMctSvalid = new System.Windows.Forms.CheckBox[10];

        private string FwFileName;
        public Boolean cmdComplete;
        public Boolean cmdFailed;
        public Boolean updatingMctFlash;
        public int cmdTries;
        public Byte[] txBuffer = new Byte[64];
        public Byte[] rxBuffer = new Byte[64];
        public Byte[] mctFlashMem = new Byte[65536];
        public int rxLen;
        public byte txPid;
        public long packetWrCount;
        public long packetRdCount;
        public int numParamEntries;
        public byte[] paramNum = new Byte[128];
        public String[] paramName = new String[128];
        public int[] paramDefault = new int[128];
        public int numTrackEntries;
        public byte[] trackNum = new Byte[256];
        public String[] trackName = new String[256];
        public int numErrorEntries;
        public byte[] errorNum = new Byte[256];
        public String[] errorName = new String[256];


        const byte MCT_RTD_TRACKLEFT_1A = 0;
        const byte MCT_RTD_TRACKRIGHT_1A = 1;
        const byte MCT_RTD_TRACKLEFT_1B = 2;
        const byte MCT_RTD_TRACKRIGHT_1B = 3;
        const byte MCT_RTD_MANIFOLD_1 = 4;
        const byte MCT_HUMIDITY1 = 5;
        const byte MCT_LOCAL_TEMPA = 6;
        const byte MCT_BUSVA = 7;
        const byte MCT_RTD_TRACKLEFT_2A = 8;
        const byte MCT_RTD_TRACKRIGHT_2A = 9;
        const byte MCT_RTD_TRACKLEFT_2B = 10;
        const byte MCT_RTD_TRACKRIGHT_2B = 11;
        const byte MCT_RTD_MANIFOLD_2 = 12;
        const byte MCT_HUMIDITY2 = 13;
        const byte MCT_LOCAL_TEMPB = 14;
        const byte MCT_BUSVB = 15;

        //const byte MCT_STAT_BOOT = 0x80;
        //const byte MCT_STAT_LOG = 0x40;
        const byte MCT_STAT_APP_VALID = 0x20;
        //const byte MCT_STAT_SBOOT = 0x10;
        const byte MCT_STAT_SAPP_VALID = 0x08;

        // indices for strMctText
        const byte TXT_TRACK1 = 0;
        const byte TXT_POSN1A = 1;
        const byte TXT_RTD1AL = 2;
        const byte TXT_RTD1AR = 3;
        const byte TXT_POSN1B = 4;
        const byte TXT_RTD1BL = 5;
        const byte TXT_RTD1BR = 6;
        const byte TXT_RTD1MAN = 7;
        const byte TXT_TRACK2 = 8;
        const byte TXT_POSN2A = 9;
        const byte TXT_RTD2AL = 10;
        const byte TXT_RTD2AR = 11;
        const byte TXT_POSN2B = 12;
        const byte TXT_RTD2BL = 13;
        const byte TXT_RTD2BR = 14;
        const byte TXT_RTD2MAN = 15;

        const byte STRING_POWER_OFF = 0;
        const byte STRING_POWER_UP = 1;
        const byte STRING_INIT_PING0 = 2;
        const byte STRING_INIT_SET_ADDR = 3;
        const byte STRING_INIT_SET_DIR = 4;
        const byte STRING_INIT_PING_ADDR = 5;
        const byte STRING_GET_INFO = 6;
        const byte STRING_ACTIVE = 7;

        const int APP_START_VECTOR = 0xEDFC;
        const int BOOT_START = 0xEE00;


        const byte FIELD_OFF = 0;
        const byte FIELD_PUMP_ON = 1;
        const byte FIELD_GO_ON_SUN = 2;
        const byte FIELD_OPERATE = 3;
        const byte FIELD_END_OF_DAY = 4;
        const byte FIELD_TEST = 5;
        const byte FIELD_TEST_ON_SUN = 6;
        const byte FIELD_TEST_OPERATE = 7;
        const byte FIELD_TEST_UPDATE = 8;
        const byte FIELD_TEST_END_OF_DAY = 9;
        const byte FIELD_TEST_OFF = 10;
        const byte FIELD_LOGGING = 11;
        const byte FIELD_LOGGING_OFF = 12;

        public Boolean flagRamDump;
        public Boolean flagSetClock;
        public Boolean flagGetString;
        public Boolean flagSetFieldOff;
        public Boolean flagSetFieldTest;
        public Boolean flagSetFieldTestOff;
        public Boolean flagSetFieldShutdown;
        public Boolean flagSetFieldTestOnSun;
        public Boolean flagSetFieldTestShutdown;
        public Boolean flagSetFieldLogging;
        public Boolean flagSetFieldLoggingOff;
        public Boolean flagHomeString;
        public Boolean flagStowString;
        public Boolean flagMctChangeTrack1;
        public Boolean flagMctHome1;
        public Boolean flagMctStow1;
        public Boolean flagMctMoveSteps1;
        public Boolean flagMctMoveDeg1;
        public Boolean flagMctChangeTrack2;
        public Boolean flagMctHome2;
        public Boolean flagMctStow2;
        public Boolean flagMctMoveSteps2;
        public Boolean flagMctMoveDeg2;
        public Boolean flagWriteAllParam;
        public int flagWriteParam;
        public Boolean flagReadAllParam;
        public Boolean flagGetMctVersions;
        public int countGetMctVersions;
        public Boolean flagSetDesiccant;
        public Boolean flagDesiccantResp;
        public Boolean flagTestBtn;
        public int testBtnValue;
        public byte desiccantNewState;
        public byte desiccantNewOutputs;
        public int sfcPcbTemp;
        public int sfcThermistor;
        public int sfcHumidity;
        public int paramStartRead;
        public int paramStopRead;
        public int paramStartWrite;
        public int paramStopWrite;
        public ulong[] sfcParam = new ulong[128];
        const int PARAM_DESICCANT_T1 = 0;
        const int PARAM_DESICCANT_T2 = 1;
        const int PARAM_DESICCANT_T3 = 2;
        const int PARAM_DESICCANT_MODE = 3;
        const int PARAM_DESICCANT_MAX_TEMP = 4;
        const int PARAM_DESICCANT_TEMP_HYST = 5;
        const int PARAM_RTD_ZERO = 6;
        const int PARAM_RTD_SPAN = 7;
        const int PARAM_DESICCANT_REGEN_HUMIDITY = 8;
        const int PARAM_DESICCANT_DUTY1_TEMP = 9;
        const int PARAM_DESICCANT_DUTY2_TEMP = 10;
        const int PARAM_DESICCANT_DUTY3_TEMP = 11;
        const int PARAM_DESICCANT_MIN_FAN_TEMP = 12;
        const byte TRACK_OFF = 0;
        const byte TRACK_HOME = 1;
        const byte TRACK_SETTLE = 2;
        const byte TRACK_FIND_EDGE_FWD = 3;
        const byte TRACK_FIND_EDGE_REV = 4;
        const byte TRACK_GO_TO_MIDDLE = 5;
        const byte TRACK_MIDDLE = 6;
        const byte TRACK_END = 7;

        const byte DESICCANT_OFF = 0;
        const byte DESICCANT_DRYING = 1;
        const byte DESICCANT_REGEN = 2;
        const byte DESICCANT_CLOSED = 3;
        const byte DESICCANT_MANUAL = 4;

        const byte FCE_OUT_PUMP = 0x01;
        const byte FCE_OUT_FAN     =0x02;
        const byte FCE_OUT_VALVE   =0x04;
        const byte FCE_OUT_HEAT    =0x08;
        public byte pollMct;
        public int countdownPollLog;
        const byte POLL_CHANNELS = 0;
        const byte POLL_MIRRORS = 1;
        const byte POLL_POSN1 = 2;
        const byte POLL_POSN2 = 3;
        const byte POLL_TARGET1 = 4;
        const byte POLL_TARGET2 = 5;
        const byte POLL_ERROR1 = 6;
        const byte POLL_ERROR2 = 7;
        const byte POLL_SFC_VERSION = 100;
        const byte POLL_FCE = 101;
        const byte POLL_RTU = 102;
        const byte POLL_RTC = 103;
        const byte POLL_DESICCANT = 104;
        const byte POLL_LOG_FIELD_STATE = 0;
        const byte POLL_LOG_FCE = 1;
        const byte POLL_LOG_RTU = 2;
        const byte POLL_LOG_SFC_TEMP = 3;
        const byte POLL_LOG_STRING = 4;
        const byte POLL_LOG_MCT_POSN = 5;
        const byte POLL_LOG_MCT_CHAN = 6;
        private Timer timer1;
        public int countdownPollString;
        private TextBox txtStringState;
        private Label label12;
        private Label label4;
        private TextBox txtNumMct;
        private TextBox txtFieldState;
        private Label label20;
        private Button btnFieldTest;
        private Button btnFieldOff;
        private Button btnFieldTestOnSun;
        private Button btnFieldShutdown;
        private Label label23;
        private Label label22;
        private Label label21;
        private TextBox txtFceDNI;
        private TextBox txtFceFlowSw;
        private TextBox txtFceRtdAD;
        private TextBox txtFceIn5;
        private TextBox txtFceIn4;
        private TextBox txtFceIn3;
        private TextBox txtFceIn2;
        private TextBox txtFceIn1;
        private TextBox txtFceBusIn;
        private TextBox txtFceBusOut;
        private TextBox txtFceInStates;
        private Label label31;
        private Label label30;
        private Label label29;
        private Label label28;
        private Label label27;
        private Label label26;
        private Label label25;
        private Label label24;
        private TabPage tabMCTfw;
        private ComboBox cbFwStartString;
        private Label label32;
        private ComboBox cbFwStopMct;
        private Label label35;
        private ComboBox cbFwStartMct;
        private Label label34;
        private ComboBox cbFwStopString;
        private Label label33;
        private Button btnUpdateMctMaster;
        private OpenFileDialog openFileDialog1;
        private Label label36;
        private TextBox txtFwMaster;
        private Label label37;
        private TextBox txtFwSlave;
        private Button btnUpdateMctSlave;
        private Button btnStowString;
        private Button btnHomeString;
        private TabPage tabMctParam;
        private TextBox txtFceOut4;
        private TextBox txtFceOut3;
        private TextBox txtFceOut2;
        private TextBox txtFceOut1;
        private Label label41;
        private Label label40;
        private Label label39;
        private Label label38;
        private TabPage tabMctControl;
        private Label label50;
        private Label label51;
        private Label label52;
        private Label label53;
        private Label label54;
        private Label label55;
        private Label label56;
        private Label label57;
        private TextBox txtMctPosn1A;
        private TextBox txtMctRtd1AL;
        private TextBox txtMctRtd1AR;
        private TextBox txtMctMan1;
        private TextBox txtMctRtd1BR;
        private TextBox txtMctRtd1BL;
        private TextBox txtMctPosn1B;
        private TextBox txtMctSteps1A;
        private Label label65;
        private TextBox txtMctTarg1A;
        private Label label64;
        private TextBox txtMctSteps1B;
        private Label label59;
        private TextBox txtMctTarg1B;
        private Label label58;
        private ComboBox cbMctTrack1;
        private CheckBox checkMctHome1A;
        private CheckBox checkMctStow1A;
        private CheckBox checkMctHome1B;
        private CheckBox checkMctStow1B;
        private Button btnMctStow1;
        private Button btnMctHome1;
        private Button btnMctMoveSteps1;
        private Button btnMctMoveDeg1;
        private TextBox txtMctMoveSteps1;
        private TextBox txtMctMoveDeg1;
        private Button btnMctMoveSteps2;
        private Button btnMctMoveDeg2;
        private TextBox txtMctMoveSteps2;
        private TextBox txtMctMoveDeg2;
        private Button btnMctStow2;
        private Button btnMctHome2;
        private CheckBox checkMctHome2B;
        private CheckBox checkMctStow2B;
        private CheckBox checkMctHome2A;
        private CheckBox checkMctStow2A;
        private ComboBox cbMctTrack2;
        private TextBox txtMctSteps2A;
        private Label label42;
        private TextBox txtMctTarg2A;
        private Label label43;
        private TextBox txtMctSteps2B;
        private Label label44;
        private TextBox txtMctTarg2B;
        private Label label45;
        private TextBox txtMctMan2;
        private TextBox txtMctRtd2BR;
        private TextBox txtMctRtd2BL;
        private TextBox txtMctPosn2B;
        private TextBox txtMctRtd2AR;
        private TextBox txtMctRtd2AL;
        private TextBox txtMctPosn2A;
        private Label label46;
        private Label label47;
        private Label label48;
        private Label label49;
        private Label label60;
        private Label label61;
        private Label label62;
        private Label label63;
        private Label label67;
        private ComboBox cbMctPollAddr;
        private TextBox txtMctLocalTempA;
        private Label label66;
        private TextBox txtMctLocalTempB;
        private Label label68;
        private TextBox txtMctHumid2;
        private Label label69;
        private TextBox txtMctHumid1;
        private Label label70;
        private TextBox txtMctError1;
        private Label label71;
        private TextBox txtMctError2;
        private Label label72;
        public byte pollType;
        private TextBox txtSys30Return;
        private Label label78;
        private TextBox txtSys30Supply;
        private TextBox txtSys30W;
        private TextBox txtSys30lph;
        private Label label75;
        private Label label76;
        private Label label77;
        private Button btnRealAllParam;
        private Button btnWriteAllParam;
        private Button btnGetMctVersions;
        private Label label74;
        private Label label79;
        private Label label80;
        private Label label81;
        private Button btnSetClock;
        private TabPage tabDataLog;
        private Label label82;
        private Label label87;
        private Label label83;
        private Label label84;
        private Label label85;
        private Label label86;
        private Label label88;
        private Button btnFieldTestOff;
        private Button btnFieldTestShutdown;
        private TextBox txtRTC;
        private GroupBox groupBox3;
        private RadioButton radioDesManual;
        private RadioButton radioDesClosed;
        private RadioButton radioDesRegen;
        private RadioButton radioDesDrying;
        private RadioButton radioDesOff;
        private CheckBox cbHeat;
        private CheckBox cbValve;
        private CheckBox cbFan;
        private ComboBox cbDryingHr;
        private ComboBox cbDryingMin;
        private ComboBox cbClosedMin;
        private ComboBox cbClosedHr;
        private ComboBox cbRegenMin;
        private ComboBox cbRegenHr;
        private Button btnDesWriteTimes;
        private Button btnDesReadTimes;
        private Label label89;
        private TextBox txtFceRtd;
        private Label label90;
        private Label label91;
        private Label label92;
        private TextBox txtSfcHumidity;
        private TextBox txtSfcThermistor;
        private TextBox txtSfcPcbTemp;
        private Label label93;
        private Button btnWriteTimes;
        private Button btnReadTemps;
        private TextBox txtDesTempDuty3;
        private TextBox txtDesTempDuty2;
        private TextBox txtDesTempDuty1;
        private Label label94;
        private Label label95;
        private Label label96;
        private TextBox txtDesRegenHumid;
        private Label label97;
        private TextBox txtTempMinFan;
        private Label label98;
        private Button btnWriteParam10;
        private Button btnWriteParam9;
        private Button btnWriteParam8;
        private Button btnWriteParam7;
        private Button btnWriteParam6;
        private Button btnWriteParam5;
        private Button btnWriteParam4;
        private Button btnWriteParam3;
        private Button btnWriteParam2;
        private Button btnWriteParam1;
        private TabPage tabTesting;
        private Button btnRamDump;
        private Button btnTestSoftReset;
        private Button btnTestField;
        private Button btnTestString;
        private Button btnTestMct485;
        private CheckBox cbLogNightMode;
        private CheckBox resetAtMidnightCheckbox;
        private Label label73;


        public frmMain()
            : base()
        {


            // This call is required by the Windows Form Designer.
            cmdComplete = true;
            updatingMctFlash = false;
            packetWrCount = 0;
            packetRdCount = 0;

            numParamEntries = 0;
            numTrackEntries = 0;
            numErrorEntries = 0;
            paramStartRead = 0;
            paramStopRead = 128;
            paramStartWrite = 128;
            paramStopWrite = 0;
            string mctFileName = "Files\\" + "MCTvalues.txt";
            if (File.Exists(mctFileName))
            {
                String line;
                char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                StreamReader MCTvalues = File.OpenText(mctFileName);
                while ((line = MCTvalues.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                    {
                    }
                    else
                    {
                        string[] parse = line.Split(delimiterChars);
                        if (line.StartsWith("PARAM"))
                        {
                            paramName[numParamEntries] = parse[0].Substring(6, parse[0].Length - 6);
                            paramNum[numParamEntries] = Convert.ToByte(parse[1]);
                            paramDefault[numParamEntries] = Convert.ToInt16(parse[2]);
                            numParamEntries++;
                        }
                        else if (line.StartsWith("TRACK"))
                        {
                            trackName[numTrackEntries] = parse[0].Substring(6, parse[0].Length - 6);
                            trackNum[numTrackEntries] = Convert.ToByte(parse[1]);
                            numTrackEntries++;
                        }
                        else if (line.StartsWith("LOG"))
                        {
                            errorName[numErrorEntries] = parse[0].Substring(4, parse[0].Length - 4);
                            errorNum[numErrorEntries] = Convert.ToByte(parse[1]);
                            numErrorEntries++;
                        }
                    }
                }
                MCTvalues.Close();
            }

            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                strMctLabel[i] = new System.Windows.Forms.Label();
                strMctLabel[i].AutoSize = true;
                strMctLabel[i].Location = new System.Drawing.Point(178 + i * 60, 3);
                strMctLabel[i].Name = "strMctLabel" + i.ToString();
                strMctLabel[i].Size = new System.Drawing.Size(40, 14);
                strMctLabel[i].TabStop = false;
                strMctLabel[i].Text = "MCT" + (i + 1).ToString();
                this.tabString.Controls.Add(this.strMctLabel[i]);

                for (int j = 0; j < NUM_STRING_BOXES; j++)
                {
                    strMctText[i, j] = new System.Windows.Forms.TextBox();
                    strMctText[i, j].Location = new System.Drawing.Point(170 + i * 60, 21 + j * 24);
                    strMctText[i, j].Name = "strMctText" + i.ToString() + "_" + j.ToString();
                    strMctText[i, j].Size = new System.Drawing.Size(50, 20);
                    strMctText[i, j].TabIndex = i * NUM_STRING_BOXES + j + 5;
                    strMctText[i, j].TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
                    this.tabString.Controls.Add(this.strMctText[i, j]);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                paramMctLabel[i] = new System.Windows.Forms.Label();
                paramMctLabel[i].AutoSize = true;
                paramMctLabel[i].Location = new System.Drawing.Point(320 + i * 45, 3);
                paramMctLabel[i].Name = "strMctLabel" + i.ToString();
                paramMctLabel[i].Size = new System.Drawing.Size(42, 14);
                paramMctLabel[i].TabStop = false;
                paramMctLabel[i].Text = "MCT" + (i + 1).ToString();
                this.tabMctParam.Controls.Add(this.paramMctLabel[i]);
            }
            for (int i = 0; i < PARAM_NUM_VALUES; i++)
            {
                paramLabel[i] = new System.Windows.Forms.Label();
                paramLabel[i].AutoSize = true;
                paramLabel[i].Location = new System.Drawing.Point(57, 24 + i * 24);
                paramLabel[i].Name = "paramLabel" + i.ToString();
                paramLabel[i].Size = new System.Drawing.Size(40, 14);
                paramLabel[i].TabStop = false;
                if (i < numParamEntries)
                {
                    paramLabel[i].Text = paramName[i];
                }
                else
                {
                    paramLabel[i].Text = "not used";
                }
                this.tabMctParam.Controls.Add(this.paramLabel[i]);

                for (int j = 0; j < 10; j++)
                {
                    paramText[j, i] = new System.Windows.Forms.TextBox();
                    paramText[j, i].Location = new System.Drawing.Point(315 + j * 45, 21 + i * 24);
                    paramText[j, i].Name = "paramText" + i.ToString() + "_" + j.ToString();
                    paramText[j, i].Size = new System.Drawing.Size(35, 20);
                    paramText[j, i].TabIndex = i * NUM_STRING_BOXES + j + 5;
                    paramText[j, i].TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
                    this.tabMctParam.Controls.Add(this.paramText[j, i]);
                }

                paramDefaultText[i] = new System.Windows.Forms.TextBox();
                paramDefaultText[i].Location = new System.Drawing.Point(7, 21 + i * 24);
                paramDefaultText[i].Name = "paramDefaultText" + i.ToString();
                paramDefaultText[i].Size = new System.Drawing.Size(50, 20);
                paramDefaultText[i].TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
                paramDefaultText[i].Text = paramDefault[i].ToString();
                this.tabMctParam.Controls.Add(this.paramDefaultText[i]);
                paramEnable[i] = new System.Windows.Forms.CheckBox();
                paramEnable[i].AutoSize = true;
                paramEnable[i].Location = new System.Drawing.Point(285, 24 + i * 24);
                paramEnable[i].Name = "checkParamEnable" + i.ToString();
                paramEnable[i].Size = new System.Drawing.Size(18, 18);
                paramEnable[i].Text = "";
                paramEnable[i].TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                paramEnable[i].UseVisualStyleBackColor = true;
                this.tabMctParam.Controls.Add(this.paramEnable[i]);
                if (i < numParamEntries)
                {
                    paramEnable[i].Checked = true;
                }
                else
                {
                    paramEnable[i].Checked = false;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                textMctMver[i] = new System.Windows.Forms.TextBox();
                textMctMver[i].Location = new System.Drawing.Point(205, 21 + i * 24);
                textMctMver[i].Name = "textMctMver" + i.ToString();
                textMctMver[i].Size = new System.Drawing.Size(250, 20);
                textMctMver[i].TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
                this.tabMCTfw.Controls.Add(this.textMctMver[i]);
                textMctSver[i] = new System.Windows.Forms.TextBox();
                textMctSver[i].Location = new System.Drawing.Point(510, 21 + i * 24);
                textMctSver[i].Name = "textMctSver" + i.ToString();
                textMctSver[i].Size = new System.Drawing.Size(250, 20);
                textMctSver[i].TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
                this.tabMCTfw.Controls.Add(this.textMctSver[i]);
                labelMctVer[i] = new System.Windows.Forms.Label();
                labelMctVer[i].AutoSize = true;
                labelMctVer[i].Location = new System.Drawing.Point(140, 24 + i * 24);
                labelMctVer[i].Name = "paramLabel" + i.ToString();
                labelMctVer[i].Size = new System.Drawing.Size(30, 14);
                labelMctVer[i].TabStop = false;
                labelMctVer[i].Text = "MCT" + (i + 1).ToString();
                this.tabMCTfw.Controls.Add(this.labelMctVer[i]);
                checkMctMvalid[i] = new System.Windows.Forms.CheckBox();
                checkMctMvalid[i].AutoSize = true;
                checkMctMvalid[i].Location = new System.Drawing.Point(180, 24 + i * 24);
                checkMctMvalid[i].Name = "checkMctMvalid" + i.ToString();
                checkMctMvalid[i].Size = new System.Drawing.Size(18, 18);
                checkMctMvalid[i].Text = "";
                checkMctMvalid[i].TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                checkMctMvalid[i].UseVisualStyleBackColor = true;
                this.tabMCTfw.Controls.Add(this.checkMctMvalid[i]);
                checkMctSvalid[i] = new System.Windows.Forms.CheckBox();
                checkMctSvalid[i].AutoSize = true;
                checkMctSvalid[i].Location = new System.Drawing.Point(485, 24 + i * 24);
                checkMctSvalid[i].Name = "checkMctSvalid" + i.ToString();
                checkMctSvalid[i].Size = new System.Drawing.Size(18, 18);
                checkMctSvalid[i].Text = "";
                checkMctSvalid[i].TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                checkMctSvalid[i].UseVisualStyleBackColor = true;
                this.tabMCTfw.Controls.Add(this.checkMctSvalid[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                char StrNum = (char)((byte)'A' + i);
                for (int j = 0; j < 10; j++)
                {
                    checkMctStore[i, j] = new System.Windows.Forms.CheckBox();
                    checkMctStore[i, j].AutoSize = true;
                    checkMctStore[i, j].Location = new System.Drawing.Point(18 + i * 42, 52 + j * 24);
                    checkMctStore[i, j].Name = "checkMctStore" + StrNum + j.ToString();
                    checkMctStore[i, j].Size = new System.Drawing.Size(18, 18);
                    checkMctStore[i, j].Text = (j + 1).ToString();
                    checkMctStore[i, j].TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                    checkMctStore[i, j].UseVisualStyleBackColor = true;
                    this.tabDataLog.Controls.Add(this.checkMctStore[i, j]);
                }
            }
            checkMctStore[0, 0].Checked = true;
            cbFwStartString.SelectedIndex = 0;
            cbFwStopString.SelectedIndex = 3;
            cbFwStartMct.SelectedIndex = 1;
            cbFwStopMct.SelectedIndex = 1;
            cbDryingHr.SelectedIndex = 15;
            cbDryingMin.SelectedIndex = 0;
            cbRegenHr.SelectedIndex = 7;
            cbRegenMin.SelectedIndex = 0;
            cbClosedHr.SelectedIndex = 8;
            cbClosedMin.SelectedIndex = 0;
        }
        // Form overrides dispose to clean up the component list.
        protected override void Dispose(Boolean Disposing)
        {
            if (Disposing)
            {
                if (!(components == null))
                {
                    components.Dispose();
                }
            }
            base.Dispose(Disposing);
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;
        private TabControl tabCtrl;
        private TabPage tabSFC;
        private TabPage tabString;
        private TextBox txtVersString;
        private Label label1;
        private Label label2;
        private RadioButton radioStringD;
        private RadioButton radioStringC;
        private RadioButton radioStringB;
        private RadioButton radioStringA;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label3;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private Label label18;
        private Label label19;
        private NumericUpDown numMctLogRate;
        private RadioButton radioMctSeconds;
        private RadioButton radioMctMinutes;
        private RadioButton radioSfcMinutes;
        private RadioButton radioSfcSeconds;
        private NumericUpDown numSfcLogRate;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label101;
        private Label label102;
        private Label label103;
        private Label label104;
        private Label label105;
        private Button btnSetA;
        private Button btnClrA;
        private Button btnClrB;
        private Button btnSetB;
        private Button btnClrD;
        private Button btnSetD;
        private Button btnClrC;
        private Button btnSetC;
        private CheckBox checkRecPosn;
        private CheckBox checkRecTrackRTD;
        private CheckBox checkRecPCBTemp;
        private CheckBox checkRecManRTD;
        private CheckBox checkRecHumidity;
        private CheckBox checkRecTrackState;
        private Label label106;
        private Label label107;
        private CheckBox checkRecSys30;
        private CheckBox checkDNI;
        private CheckBox checkRecFCE;
        private CheckBox checkRecSensors;
        private CheckBox checkRecSFCTemp;
        private Button btnStartStop;
        public System.Windows.Forms.ToolTip ToolTip1;
        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.

        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabSFC = new System.Windows.Forms.TabPage();
            this.label93 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.label91 = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.txtSfcHumidity = new System.Windows.Forms.TextBox();
            this.txtSfcThermistor = new System.Windows.Forms.TextBox();
            this.txtSfcPcbTemp = new System.Windows.Forms.TextBox();
            this.label89 = new System.Windows.Forms.Label();
            this.txtFceRtd = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtTempMinFan = new System.Windows.Forms.TextBox();
            this.label98 = new System.Windows.Forms.Label();
            this.txtDesRegenHumid = new System.Windows.Forms.TextBox();
            this.label97 = new System.Windows.Forms.Label();
            this.btnWriteTimes = new System.Windows.Forms.Button();
            this.btnReadTemps = new System.Windows.Forms.Button();
            this.txtDesTempDuty3 = new System.Windows.Forms.TextBox();
            this.btnDesWriteTimes = new System.Windows.Forms.Button();
            this.txtDesTempDuty2 = new System.Windows.Forms.TextBox();
            this.btnDesReadTimes = new System.Windows.Forms.Button();
            this.txtDesTempDuty1 = new System.Windows.Forms.TextBox();
            this.label94 = new System.Windows.Forms.Label();
            this.cbClosedMin = new System.Windows.Forms.ComboBox();
            this.label95 = new System.Windows.Forms.Label();
            this.cbClosedHr = new System.Windows.Forms.ComboBox();
            this.label96 = new System.Windows.Forms.Label();
            this.cbRegenMin = new System.Windows.Forms.ComboBox();
            this.cbRegenHr = new System.Windows.Forms.ComboBox();
            this.cbDryingMin = new System.Windows.Forms.ComboBox();
            this.cbDryingHr = new System.Windows.Forms.ComboBox();
            this.cbHeat = new System.Windows.Forms.CheckBox();
            this.cbValve = new System.Windows.Forms.CheckBox();
            this.cbFan = new System.Windows.Forms.CheckBox();
            this.radioDesManual = new System.Windows.Forms.RadioButton();
            this.radioDesClosed = new System.Windows.Forms.RadioButton();
            this.radioDesRegen = new System.Windows.Forms.RadioButton();
            this.radioDesDrying = new System.Windows.Forms.RadioButton();
            this.radioDesOff = new System.Windows.Forms.RadioButton();
            this.txtRTC = new System.Windows.Forms.TextBox();
            this.btnSetClock = new System.Windows.Forms.Button();
            this.txtSys30Return = new System.Windows.Forms.TextBox();
            this.label78 = new System.Windows.Forms.Label();
            this.txtSys30Supply = new System.Windows.Forms.TextBox();
            this.txtSys30W = new System.Windows.Forms.TextBox();
            this.txtSys30lph = new System.Windows.Forms.TextBox();
            this.label75 = new System.Windows.Forms.Label();
            this.label76 = new System.Windows.Forms.Label();
            this.label77 = new System.Windows.Forms.Label();
            this.txtFceOut4 = new System.Windows.Forms.TextBox();
            this.txtFceOut3 = new System.Windows.Forms.TextBox();
            this.txtFceOut2 = new System.Windows.Forms.TextBox();
            this.txtFceOut1 = new System.Windows.Forms.TextBox();
            this.label41 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.txtFceDNI = new System.Windows.Forms.TextBox();
            this.txtFceFlowSw = new System.Windows.Forms.TextBox();
            this.txtFceRtdAD = new System.Windows.Forms.TextBox();
            this.txtFceIn5 = new System.Windows.Forms.TextBox();
            this.txtFceIn4 = new System.Windows.Forms.TextBox();
            this.txtFceIn3 = new System.Windows.Forms.TextBox();
            this.txtFceIn2 = new System.Windows.Forms.TextBox();
            this.txtFceIn1 = new System.Windows.Forms.TextBox();
            this.txtFceBusIn = new System.Windows.Forms.TextBox();
            this.txtFceBusOut = new System.Windows.Forms.TextBox();
            this.txtFceInStates = new System.Windows.Forms.TextBox();
            this.txtVersString = new System.Windows.Forms.TextBox();
            this.tabString = new System.Windows.Forms.TabPage();
            this.label87 = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.label84 = new System.Windows.Forms.Label();
            this.label85 = new System.Windows.Forms.Label();
            this.label86 = new System.Windows.Forms.Label();
            this.btnStowString = new System.Windows.Forms.Button();
            this.btnHomeString = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.txtNumMct = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabMCTfw = new System.Windows.Forms.TabPage();
            this.label80 = new System.Windows.Forms.Label();
            this.label81 = new System.Windows.Forms.Label();
            this.label79 = new System.Windows.Forms.Label();
            this.label74 = new System.Windows.Forms.Label();
            this.btnGetMctVersions = new System.Windows.Forms.Button();
            this.label37 = new System.Windows.Forms.Label();
            this.txtFwSlave = new System.Windows.Forms.TextBox();
            this.btnUpdateMctSlave = new System.Windows.Forms.Button();
            this.label36 = new System.Windows.Forms.Label();
            this.txtFwMaster = new System.Windows.Forms.TextBox();
            this.btnUpdateMctMaster = new System.Windows.Forms.Button();
            this.cbFwStopMct = new System.Windows.Forms.ComboBox();
            this.label35 = new System.Windows.Forms.Label();
            this.cbFwStartMct = new System.Windows.Forms.ComboBox();
            this.label34 = new System.Windows.Forms.Label();
            this.cbFwStopString = new System.Windows.Forms.ComboBox();
            this.label33 = new System.Windows.Forms.Label();
            this.cbFwStartString = new System.Windows.Forms.ComboBox();
            this.label32 = new System.Windows.Forms.Label();
            this.tabMctParam = new System.Windows.Forms.TabPage();
            this.btnWriteParam10 = new System.Windows.Forms.Button();
            this.btnWriteParam9 = new System.Windows.Forms.Button();
            this.btnWriteParam8 = new System.Windows.Forms.Button();
            this.btnWriteParam7 = new System.Windows.Forms.Button();
            this.btnWriteParam6 = new System.Windows.Forms.Button();
            this.btnWriteParam5 = new System.Windows.Forms.Button();
            this.btnWriteParam4 = new System.Windows.Forms.Button();
            this.btnWriteParam3 = new System.Windows.Forms.Button();
            this.btnWriteParam2 = new System.Windows.Forms.Button();
            this.btnWriteParam1 = new System.Windows.Forms.Button();
            this.label73 = new System.Windows.Forms.Label();
            this.btnRealAllParam = new System.Windows.Forms.Button();
            this.btnWriteAllParam = new System.Windows.Forms.Button();
            this.tabMctControl = new System.Windows.Forms.TabPage();
            this.txtMctError1 = new System.Windows.Forms.TextBox();
            this.label71 = new System.Windows.Forms.Label();
            this.txtMctError2 = new System.Windows.Forms.TextBox();
            this.label72 = new System.Windows.Forms.Label();
            this.txtMctHumid2 = new System.Windows.Forms.TextBox();
            this.label69 = new System.Windows.Forms.Label();
            this.txtMctHumid1 = new System.Windows.Forms.TextBox();
            this.label70 = new System.Windows.Forms.Label();
            this.txtMctLocalTempA = new System.Windows.Forms.TextBox();
            this.label66 = new System.Windows.Forms.Label();
            this.txtMctLocalTempB = new System.Windows.Forms.TextBox();
            this.label68 = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.cbMctPollAddr = new System.Windows.Forms.ComboBox();
            this.btnMctMoveSteps2 = new System.Windows.Forms.Button();
            this.btnMctMoveDeg2 = new System.Windows.Forms.Button();
            this.txtMctMoveSteps2 = new System.Windows.Forms.TextBox();
            this.txtMctMoveDeg2 = new System.Windows.Forms.TextBox();
            this.btnMctStow2 = new System.Windows.Forms.Button();
            this.btnMctHome2 = new System.Windows.Forms.Button();
            this.checkMctHome2B = new System.Windows.Forms.CheckBox();
            this.checkMctStow2B = new System.Windows.Forms.CheckBox();
            this.checkMctHome2A = new System.Windows.Forms.CheckBox();
            this.checkMctStow2A = new System.Windows.Forms.CheckBox();
            this.cbMctTrack2 = new System.Windows.Forms.ComboBox();
            this.txtMctSteps2A = new System.Windows.Forms.TextBox();
            this.label42 = new System.Windows.Forms.Label();
            this.txtMctTarg2A = new System.Windows.Forms.TextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.txtMctSteps2B = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.txtMctTarg2B = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.txtMctMan2 = new System.Windows.Forms.TextBox();
            this.txtMctRtd2BR = new System.Windows.Forms.TextBox();
            this.txtMctRtd2BL = new System.Windows.Forms.TextBox();
            this.txtMctPosn2B = new System.Windows.Forms.TextBox();
            this.txtMctRtd2AR = new System.Windows.Forms.TextBox();
            this.txtMctRtd2AL = new System.Windows.Forms.TextBox();
            this.txtMctPosn2A = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.label60 = new System.Windows.Forms.Label();
            this.label61 = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.btnMctMoveSteps1 = new System.Windows.Forms.Button();
            this.btnMctMoveDeg1 = new System.Windows.Forms.Button();
            this.txtMctMoveSteps1 = new System.Windows.Forms.TextBox();
            this.txtMctMoveDeg1 = new System.Windows.Forms.TextBox();
            this.btnMctStow1 = new System.Windows.Forms.Button();
            this.btnMctHome1 = new System.Windows.Forms.Button();
            this.checkMctHome1B = new System.Windows.Forms.CheckBox();
            this.checkMctStow1B = new System.Windows.Forms.CheckBox();
            this.checkMctHome1A = new System.Windows.Forms.CheckBox();
            this.checkMctStow1A = new System.Windows.Forms.CheckBox();
            this.cbMctTrack1 = new System.Windows.Forms.ComboBox();
            this.txtMctSteps1A = new System.Windows.Forms.TextBox();
            this.label65 = new System.Windows.Forms.Label();
            this.txtMctTarg1A = new System.Windows.Forms.TextBox();
            this.label64 = new System.Windows.Forms.Label();
            this.txtMctSteps1B = new System.Windows.Forms.TextBox();
            this.label59 = new System.Windows.Forms.Label();
            this.txtMctTarg1B = new System.Windows.Forms.TextBox();
            this.label58 = new System.Windows.Forms.Label();
            this.txtMctMan1 = new System.Windows.Forms.TextBox();
            this.txtMctRtd1BR = new System.Windows.Forms.TextBox();
            this.txtMctRtd1BL = new System.Windows.Forms.TextBox();
            this.txtMctPosn1B = new System.Windows.Forms.TextBox();
            this.txtMctRtd1AR = new System.Windows.Forms.TextBox();
            this.txtMctRtd1AL = new System.Windows.Forms.TextBox();
            this.txtMctPosn1A = new System.Windows.Forms.TextBox();
            this.label50 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.label57 = new System.Windows.Forms.Label();
            this.tabDataLog = new System.Windows.Forms.TabPage();
            this.cbLogNightMode = new System.Windows.Forms.CheckBox();
            this.label82 = new System.Windows.Forms.Label();
            this.checkRecSFCTemp = new System.Windows.Forms.CheckBox();
            this.checkRecSensors = new System.Windows.Forms.CheckBox();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.checkRecSys30 = new System.Windows.Forms.CheckBox();
            this.checkDNI = new System.Windows.Forms.CheckBox();
            this.checkRecFCE = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkRecTrackState = new System.Windows.Forms.CheckBox();
            this.checkRecHumidity = new System.Windows.Forms.CheckBox();
            this.checkRecPCBTemp = new System.Windows.Forms.CheckBox();
            this.checkRecManRTD = new System.Windows.Forms.CheckBox();
            this.checkRecTrackRTD = new System.Windows.Forms.CheckBox();
            this.checkRecPosn = new System.Windows.Forms.CheckBox();
            this.btnClrD = new System.Windows.Forms.Button();
            this.btnSetD = new System.Windows.Forms.Button();
            this.btnClrC = new System.Windows.Forms.Button();
            this.btnSetC = new System.Windows.Forms.Button();
            this.btnClrB = new System.Windows.Forms.Button();
            this.btnSetB = new System.Windows.Forms.Button();
            this.btnClrA = new System.Windows.Forms.Button();
            this.btnSetA = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numSfcLogRate = new System.Windows.Forms.NumericUpDown();
            this.radioSfcSeconds = new System.Windows.Forms.RadioButton();
            this.radioSfcMinutes = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numMctLogRate = new System.Windows.Forms.NumericUpDown();
            this.radioMctSeconds = new System.Windows.Forms.RadioButton();
            this.radioMctMinutes = new System.Windows.Forms.RadioButton();
            this.tabTesting = new System.Windows.Forms.TabPage();
            this.resetAtMidnightCheckbox = new System.Windows.Forms.CheckBox();
            this.btnTestSoftReset = new System.Windows.Forms.Button();
            this.btnTestField = new System.Windows.Forms.Button();
            this.btnTestString = new System.Windows.Forms.Button();
            this.btnTestMct485 = new System.Windows.Forms.Button();
            this.btnRamDump = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.btnFieldTestOnSun = new System.Windows.Forms.Button();
            this.btnFieldShutdown = new System.Windows.Forms.Button();
            this.btnFieldTest = new System.Windows.Forms.Button();
            this.btnFieldOff = new System.Windows.Forms.Button();
            this.txtFieldState = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.txtStringState = new System.Windows.Forms.TextBox();
            this.radioStringD = new System.Windows.Forms.RadioButton();
            this.radioStringC = new System.Windows.Forms.RadioButton();
            this.radioStringB = new System.Windows.Forms.RadioButton();
            this.radioStringA = new System.Windows.Forms.RadioButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label101 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.label103 = new System.Windows.Forms.Label();
            this.label104 = new System.Windows.Forms.Label();
            this.label105 = new System.Windows.Forms.Label();
            this.label106 = new System.Windows.Forms.Label();
            this.label107 = new System.Windows.Forms.Label();
            this.label88 = new System.Windows.Forms.Label();
            this.btnFieldTestOff = new System.Windows.Forms.Button();
            this.btnFieldTestShutdown = new System.Windows.Forms.Button();
            this.tabCtrl.SuspendLayout();
            this.tabSFC.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabString.SuspendLayout();
            this.tabMCTfw.SuspendLayout();
            this.tabMctParam.SuspendLayout();
            this.tabMctControl.SuspendLayout();
            this.tabDataLog.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSfcLogRate)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMctLogRate)).BeginInit();
            this.tabTesting.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCtrl
            // 
            this.tabCtrl.Controls.Add(this.tabSFC);
            this.tabCtrl.Controls.Add(this.tabString);
            this.tabCtrl.Controls.Add(this.tabMCTfw);
            this.tabCtrl.Controls.Add(this.tabMctParam);
            this.tabCtrl.Controls.Add(this.tabMctControl);
            this.tabCtrl.Controls.Add(this.tabDataLog);
            this.tabCtrl.Controls.Add(this.tabTesting);
            this.tabCtrl.Location = new System.Drawing.Point(8, 0);
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(768, 436);
            this.tabCtrl.TabIndex = 0;
            this.tabCtrl.SelectedIndexChanged += new System.EventHandler(this.tabCtrl_SelectedIndexChanged);
            // 
            // tabSFC
            // 
            this.tabSFC.Controls.Add(this.label93);
            this.tabSFC.Controls.Add(this.label90);
            this.tabSFC.Controls.Add(this.label91);
            this.tabSFC.Controls.Add(this.label92);
            this.tabSFC.Controls.Add(this.txtSfcHumidity);
            this.tabSFC.Controls.Add(this.txtSfcThermistor);
            this.tabSFC.Controls.Add(this.txtSfcPcbTemp);
            this.tabSFC.Controls.Add(this.label89);
            this.tabSFC.Controls.Add(this.txtFceRtd);
            this.tabSFC.Controls.Add(this.groupBox3);
            this.tabSFC.Controls.Add(this.txtRTC);
            this.tabSFC.Controls.Add(this.btnSetClock);
            this.tabSFC.Controls.Add(this.txtSys30Return);
            this.tabSFC.Controls.Add(this.label78);
            this.tabSFC.Controls.Add(this.txtSys30Supply);
            this.tabSFC.Controls.Add(this.txtSys30W);
            this.tabSFC.Controls.Add(this.txtSys30lph);
            this.tabSFC.Controls.Add(this.label75);
            this.tabSFC.Controls.Add(this.label76);
            this.tabSFC.Controls.Add(this.label77);
            this.tabSFC.Controls.Add(this.txtFceOut4);
            this.tabSFC.Controls.Add(this.txtFceOut3);
            this.tabSFC.Controls.Add(this.txtFceOut2);
            this.tabSFC.Controls.Add(this.txtFceOut1);
            this.tabSFC.Controls.Add(this.label41);
            this.tabSFC.Controls.Add(this.label40);
            this.tabSFC.Controls.Add(this.label39);
            this.tabSFC.Controls.Add(this.label38);
            this.tabSFC.Controls.Add(this.label31);
            this.tabSFC.Controls.Add(this.label30);
            this.tabSFC.Controls.Add(this.label29);
            this.tabSFC.Controls.Add(this.label28);
            this.tabSFC.Controls.Add(this.label27);
            this.tabSFC.Controls.Add(this.label26);
            this.tabSFC.Controls.Add(this.label25);
            this.tabSFC.Controls.Add(this.label24);
            this.tabSFC.Controls.Add(this.label23);
            this.tabSFC.Controls.Add(this.label22);
            this.tabSFC.Controls.Add(this.label21);
            this.tabSFC.Controls.Add(this.txtFceDNI);
            this.tabSFC.Controls.Add(this.txtFceFlowSw);
            this.tabSFC.Controls.Add(this.txtFceRtdAD);
            this.tabSFC.Controls.Add(this.txtFceIn5);
            this.tabSFC.Controls.Add(this.txtFceIn4);
            this.tabSFC.Controls.Add(this.txtFceIn3);
            this.tabSFC.Controls.Add(this.txtFceIn2);
            this.tabSFC.Controls.Add(this.txtFceIn1);
            this.tabSFC.Controls.Add(this.txtFceBusIn);
            this.tabSFC.Controls.Add(this.txtFceBusOut);
            this.tabSFC.Controls.Add(this.txtFceInStates);
            this.tabSFC.Controls.Add(this.txtVersString);
            this.tabSFC.Location = new System.Drawing.Point(4, 23);
            this.tabSFC.Name = "tabSFC";
            this.tabSFC.Padding = new System.Windows.Forms.Padding(3);
            this.tabSFC.Size = new System.Drawing.Size(760, 409);
            this.tabSFC.TabIndex = 0;
            this.tabSFC.Text = "SFC";
            this.tabSFC.UseVisualStyleBackColor = true;
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Location = new System.Drawing.Point(142, 352);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(56, 14);
            this.label93.TabIndex = 51;
            this.label93.Text = "SFC Clock";
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.Location = new System.Drawing.Point(7, 185);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(81, 14);
            this.label90.TabIndex = 50;
            this.label90.Text = "SFC Thermistor";
            // 
            // label91
            // 
            this.label91.AutoSize = true;
            this.label91.Location = new System.Drawing.Point(7, 211);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(70, 14);
            this.label91.TabIndex = 49;
            this.label91.Text = "SFC Humidity";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.Location = new System.Drawing.Point(6, 159);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(78, 14);
            this.label92.TabIndex = 48;
            this.label92.Text = "SFC PCB Temp";
            // 
            // txtSfcHumidity
            // 
            this.txtSfcHumidity.Location = new System.Drawing.Point(94, 208);
            this.txtSfcHumidity.Name = "txtSfcHumidity";
            this.txtSfcHumidity.Size = new System.Drawing.Size(44, 20);
            this.txtSfcHumidity.TabIndex = 47;
            this.txtSfcHumidity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSfcThermistor
            // 
            this.txtSfcThermistor.Location = new System.Drawing.Point(94, 182);
            this.txtSfcThermistor.Name = "txtSfcThermistor";
            this.txtSfcThermistor.Size = new System.Drawing.Size(44, 20);
            this.txtSfcThermistor.TabIndex = 46;
            this.txtSfcThermistor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSfcPcbTemp
            // 
            this.txtSfcPcbTemp.Location = new System.Drawing.Point(94, 156);
            this.txtSfcPcbTemp.Name = "txtSfcPcbTemp";
            this.txtSfcPcbTemp.Size = new System.Drawing.Size(44, 20);
            this.txtSfcPcbTemp.TabIndex = 45;
            this.txtSfcPcbTemp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Location = new System.Drawing.Point(179, 277);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(49, 14);
            this.label89.TabIndex = 44;
            this.label89.Text = "FCE RTD";
            // 
            // txtFceRtd
            // 
            this.txtFceRtd.Location = new System.Drawing.Point(235, 274);
            this.txtFceRtd.Name = "txtFceRtd";
            this.txtFceRtd.Size = new System.Drawing.Size(42, 20);
            this.txtFceRtd.TabIndex = 43;
            this.txtFceRtd.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtTempMinFan);
            this.groupBox3.Controls.Add(this.label98);
            this.groupBox3.Controls.Add(this.txtDesRegenHumid);
            this.groupBox3.Controls.Add(this.label97);
            this.groupBox3.Controls.Add(this.btnWriteTimes);
            this.groupBox3.Controls.Add(this.btnReadTemps);
            this.groupBox3.Controls.Add(this.txtDesTempDuty3);
            this.groupBox3.Controls.Add(this.btnDesWriteTimes);
            this.groupBox3.Controls.Add(this.txtDesTempDuty2);
            this.groupBox3.Controls.Add(this.btnDesReadTimes);
            this.groupBox3.Controls.Add(this.txtDesTempDuty1);
            this.groupBox3.Controls.Add(this.label94);
            this.groupBox3.Controls.Add(this.cbClosedMin);
            this.groupBox3.Controls.Add(this.label95);
            this.groupBox3.Controls.Add(this.cbClosedHr);
            this.groupBox3.Controls.Add(this.label96);
            this.groupBox3.Controls.Add(this.cbRegenMin);
            this.groupBox3.Controls.Add(this.cbRegenHr);
            this.groupBox3.Controls.Add(this.cbDryingMin);
            this.groupBox3.Controls.Add(this.cbDryingHr);
            this.groupBox3.Controls.Add(this.cbHeat);
            this.groupBox3.Controls.Add(this.cbValve);
            this.groupBox3.Controls.Add(this.cbFan);
            this.groupBox3.Controls.Add(this.radioDesManual);
            this.groupBox3.Controls.Add(this.radioDesClosed);
            this.groupBox3.Controls.Add(this.radioDesRegen);
            this.groupBox3.Controls.Add(this.radioDesDrying);
            this.groupBox3.Controls.Add(this.radioDesOff);
            this.groupBox3.Location = new System.Drawing.Point(498, 107);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(256, 296);
            this.groupBox3.TabIndex = 42;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Desiccant";
            // 
            // txtTempMinFan
            // 
            this.txtTempMinFan.Location = new System.Drawing.Point(213, 263);
            this.txtTempMinFan.Name = "txtTempMinFan";
            this.txtTempMinFan.Size = new System.Drawing.Size(37, 20);
            this.txtTempMinFan.TabIndex = 63;
            this.txtTempMinFan.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label98
            // 
            this.label98.AutoSize = true;
            this.label98.Location = new System.Drawing.Point(137, 266);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(72, 14);
            this.label98.TabIndex = 62;
            this.label98.Text = "Temp Min Fan";
            // 
            // txtDesRegenHumid
            // 
            this.txtDesRegenHumid.Location = new System.Drawing.Point(213, 161);
            this.txtDesRegenHumid.Name = "txtDesRegenHumid";
            this.txtDesRegenHumid.Size = new System.Drawing.Size(37, 20);
            this.txtDesRegenHumid.TabIndex = 61;
            this.txtDesRegenHumid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.Location = new System.Drawing.Point(113, 164);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(94, 14);
            this.label97.TabIndex = 60;
            this.label97.Text = "Regen Humidity %";
            // 
            // btnWriteTimes
            // 
            this.btnWriteTimes.Location = new System.Drawing.Point(67, 216);
            this.btnWriteTimes.Name = "btnWriteTimes";
            this.btnWriteTimes.Size = new System.Drawing.Size(75, 23);
            this.btnWriteTimes.TabIndex = 59;
            this.btnWriteTimes.Text = "Write Temps";
            this.btnWriteTimes.UseVisualStyleBackColor = true;
            this.btnWriteTimes.Click += new System.EventHandler(this.btnWriteTimes_Click);
            // 
            // btnReadTemps
            // 
            this.btnReadTemps.Location = new System.Drawing.Point(67, 187);
            this.btnReadTemps.Name = "btnReadTemps";
            this.btnReadTemps.Size = new System.Drawing.Size(75, 23);
            this.btnReadTemps.TabIndex = 58;
            this.btnReadTemps.Text = "Read Temps";
            this.btnReadTemps.UseVisualStyleBackColor = true;
            this.btnReadTemps.Click += new System.EventHandler(this.btnReadTemps_Click);
            // 
            // txtDesTempDuty3
            // 
            this.txtDesTempDuty3.Location = new System.Drawing.Point(213, 239);
            this.txtDesTempDuty3.Name = "txtDesTempDuty3";
            this.txtDesTempDuty3.Size = new System.Drawing.Size(37, 20);
            this.txtDesTempDuty3.TabIndex = 57;
            this.txtDesTempDuty3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnDesWriteTimes
            // 
            this.btnDesWriteTimes.Location = new System.Drawing.Point(156, 113);
            this.btnDesWriteTimes.Name = "btnDesWriteTimes";
            this.btnDesWriteTimes.Size = new System.Drawing.Size(75, 23);
            this.btnDesWriteTimes.TabIndex = 44;
            this.btnDesWriteTimes.Text = "Write Times";
            this.btnDesWriteTimes.UseVisualStyleBackColor = true;
            this.btnDesWriteTimes.Click += new System.EventHandler(this.btnDesWriteTimes_Click);
            // 
            // txtDesTempDuty2
            // 
            this.txtDesTempDuty2.Location = new System.Drawing.Point(213, 213);
            this.txtDesTempDuty2.Name = "txtDesTempDuty2";
            this.txtDesTempDuty2.Size = new System.Drawing.Size(37, 20);
            this.txtDesTempDuty2.TabIndex = 56;
            this.txtDesTempDuty2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnDesReadTimes
            // 
            this.btnDesReadTimes.Location = new System.Drawing.Point(156, 17);
            this.btnDesReadTimes.Name = "btnDesReadTimes";
            this.btnDesReadTimes.Size = new System.Drawing.Size(75, 23);
            this.btnDesReadTimes.TabIndex = 43;
            this.btnDesReadTimes.Text = "Read Times";
            this.btnDesReadTimes.UseVisualStyleBackColor = true;
            this.btnDesReadTimes.Click += new System.EventHandler(this.btnDesReadTimes_Click);
            // 
            // txtDesTempDuty1
            // 
            this.txtDesTempDuty1.Location = new System.Drawing.Point(213, 187);
            this.txtDesTempDuty1.Name = "txtDesTempDuty1";
            this.txtDesTempDuty1.Size = new System.Drawing.Size(37, 20);
            this.txtDesTempDuty1.TabIndex = 55;
            this.txtDesTempDuty1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.Location = new System.Drawing.Point(148, 242);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(63, 14);
            this.label94.TabIndex = 54;
            this.label94.Text = "Temp Duty3";
            // 
            // cbClosedMin
            // 
            this.cbClosedMin.FormattingEnabled = true;
            this.cbClosedMin.Items.AddRange(new object[] {
            ":00",
            ":05",
            ":10",
            ":15",
            ":20",
            ":25",
            ":30",
            ":35",
            ":40",
            ":45",
            ":50",
            ":55"});
            this.cbClosedMin.Location = new System.Drawing.Point(206, 87);
            this.cbClosedMin.Name = "cbClosedMin";
            this.cbClosedMin.Size = new System.Drawing.Size(44, 22);
            this.cbClosedMin.TabIndex = 13;
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Location = new System.Drawing.Point(148, 216);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(63, 14);
            this.label95.TabIndex = 53;
            this.label95.Text = "Temp Duty2";
            // 
            // cbClosedHr
            // 
            this.cbClosedHr.FormattingEnabled = true;
            this.cbClosedHr.Items.AddRange(new object[] {
            "12am",
            "1am",
            "2am",
            "3am",
            "4am",
            "5am",
            "6am",
            "7am",
            "8am",
            "9am",
            "10am",
            "11am",
            "12pm",
            "1pm",
            "2pm",
            "3pm",
            "4pm",
            "5pm",
            "6pm",
            "7pm",
            "8pm",
            "9pm",
            "10pm",
            "11pm"});
            this.cbClosedHr.Location = new System.Drawing.Point(142, 87);
            this.cbClosedHr.Name = "cbClosedHr";
            this.cbClosedHr.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbClosedHr.Size = new System.Drawing.Size(58, 22);
            this.cbClosedHr.TabIndex = 12;
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Location = new System.Drawing.Point(148, 190);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(63, 14);
            this.label96.TabIndex = 52;
            this.label96.Text = "Temp Duty1";
            // 
            // cbRegenMin
            // 
            this.cbRegenMin.FormattingEnabled = true;
            this.cbRegenMin.Items.AddRange(new object[] {
            ":00",
            ":05",
            ":10",
            ":15",
            ":20",
            ":25",
            ":30",
            ":35",
            ":40",
            ":45",
            ":50",
            ":55"});
            this.cbRegenMin.Location = new System.Drawing.Point(206, 65);
            this.cbRegenMin.Name = "cbRegenMin";
            this.cbRegenMin.Size = new System.Drawing.Size(44, 22);
            this.cbRegenMin.TabIndex = 11;
            // 
            // cbRegenHr
            // 
            this.cbRegenHr.FormattingEnabled = true;
            this.cbRegenHr.Items.AddRange(new object[] {
            "12am",
            "1am",
            "2am",
            "3am",
            "4am",
            "5am",
            "6am",
            "7am",
            "8am",
            "9am",
            "10am",
            "11am",
            "12pm",
            "1pm",
            "2pm",
            "3pm",
            "4pm",
            "5pm",
            "6pm",
            "7pm",
            "8pm",
            "9pm",
            "10pm",
            "11pm"});
            this.cbRegenHr.Location = new System.Drawing.Point(142, 65);
            this.cbRegenHr.Name = "cbRegenHr";
            this.cbRegenHr.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbRegenHr.Size = new System.Drawing.Size(58, 22);
            this.cbRegenHr.TabIndex = 10;
            // 
            // cbDryingMin
            // 
            this.cbDryingMin.FormattingEnabled = true;
            this.cbDryingMin.Items.AddRange(new object[] {
            ":00",
            ":05",
            ":10",
            ":15",
            ":20",
            ":25",
            ":30",
            ":35",
            ":40",
            ":45",
            ":50",
            ":55"});
            this.cbDryingMin.Location = new System.Drawing.Point(206, 43);
            this.cbDryingMin.Name = "cbDryingMin";
            this.cbDryingMin.Size = new System.Drawing.Size(44, 22);
            this.cbDryingMin.TabIndex = 9;
            // 
            // cbDryingHr
            // 
            this.cbDryingHr.FormattingEnabled = true;
            this.cbDryingHr.Items.AddRange(new object[] {
            "12am",
            "1am",
            "2am",
            "3am",
            "4am",
            "5am",
            "6am",
            "7am",
            "8am",
            "9am",
            "10am",
            "11am",
            "12pm",
            "1pm",
            "2pm",
            "3pm",
            "4pm",
            "5pm",
            "6pm",
            "7pm",
            "8pm",
            "9pm",
            "10pm",
            "11pm"});
            this.cbDryingHr.Location = new System.Drawing.Point(142, 43);
            this.cbDryingHr.Name = "cbDryingHr";
            this.cbDryingHr.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cbDryingHr.Size = new System.Drawing.Size(58, 22);
            this.cbDryingHr.TabIndex = 8;
            // 
            // cbHeat
            // 
            this.cbHeat.AutoSize = true;
            this.cbHeat.Location = new System.Drawing.Point(6, 91);
            this.cbHeat.Name = "cbHeat";
            this.cbHeat.Size = new System.Drawing.Size(49, 17);
            this.cbHeat.TabIndex = 7;
            this.cbHeat.Text = "Heat";
            this.cbHeat.UseVisualStyleBackColor = true;
            this.cbHeat.CheckedChanged += new System.EventHandler(this.cbHeat_CheckedChanged);
            // 
            // cbValve
            // 
            this.cbValve.AutoSize = true;
            this.cbValve.Location = new System.Drawing.Point(6, 67);
            this.cbValve.Name = "cbValve";
            this.cbValve.Size = new System.Drawing.Size(53, 17);
            this.cbValve.TabIndex = 6;
            this.cbValve.Text = "Valve";
            this.cbValve.UseVisualStyleBackColor = true;
            this.cbValve.CheckedChanged += new System.EventHandler(this.cbValve_CheckedChanged);
            // 
            // cbFan
            // 
            this.cbFan.AutoSize = true;
            this.cbFan.Location = new System.Drawing.Point(6, 43);
            this.cbFan.Name = "cbFan";
            this.cbFan.Size = new System.Drawing.Size(44, 17);
            this.cbFan.TabIndex = 5;
            this.cbFan.Text = "Fan";
            this.cbFan.UseVisualStyleBackColor = true;
            this.cbFan.CheckedChanged += new System.EventHandler(this.cbFan_CheckedChanged);
            // 
            // radioDesManual
            // 
            this.radioDesManual.AutoSize = true;
            this.radioDesManual.Location = new System.Drawing.Point(80, 115);
            this.radioDesManual.Name = "radioDesManual";
            this.radioDesManual.Size = new System.Drawing.Size(60, 17);
            this.radioDesManual.TabIndex = 4;
            this.radioDesManual.TabStop = true;
            this.radioDesManual.Text = "Manual";
            this.radioDesManual.UseVisualStyleBackColor = true;
            this.radioDesManual.CheckedChanged += new System.EventHandler(this.radioDesManual_CheckedChanged);
            // 
            // radioDesClosed
            // 
            this.radioDesClosed.AutoSize = true;
            this.radioDesClosed.Location = new System.Drawing.Point(80, 91);
            this.radioDesClosed.Name = "radioDesClosed";
            this.radioDesClosed.Size = new System.Drawing.Size(57, 17);
            this.radioDesClosed.TabIndex = 3;
            this.radioDesClosed.TabStop = true;
            this.radioDesClosed.Text = "Closed";
            this.radioDesClosed.UseVisualStyleBackColor = true;
            this.radioDesClosed.CheckedChanged += new System.EventHandler(this.radioDesClosed_CheckedChanged);
            // 
            // radioDesRegen
            // 
            this.radioDesRegen.AutoSize = true;
            this.radioDesRegen.Location = new System.Drawing.Point(80, 67);
            this.radioDesRegen.Name = "radioDesRegen";
            this.radioDesRegen.Size = new System.Drawing.Size(57, 17);
            this.radioDesRegen.TabIndex = 2;
            this.radioDesRegen.TabStop = true;
            this.radioDesRegen.Text = "Regen";
            this.radioDesRegen.UseVisualStyleBackColor = true;
            this.radioDesRegen.CheckedChanged += new System.EventHandler(this.radioDesRegen_CheckedChanged);
            // 
            // radioDesDrying
            // 
            this.radioDesDrying.AutoSize = true;
            this.radioDesDrying.Location = new System.Drawing.Point(80, 43);
            this.radioDesDrying.Name = "radioDesDrying";
            this.radioDesDrying.Size = new System.Drawing.Size(55, 17);
            this.radioDesDrying.TabIndex = 1;
            this.radioDesDrying.TabStop = true;
            this.radioDesDrying.Text = "Drying";
            this.radioDesDrying.UseVisualStyleBackColor = true;
            this.radioDesDrying.CheckedChanged += new System.EventHandler(this.radioDesDrying_CheckedChanged);
            // 
            // radioDesOff
            // 
            this.radioDesOff.AutoSize = true;
            this.radioDesOff.Location = new System.Drawing.Point(80, 19);
            this.radioDesOff.Name = "radioDesOff";
            this.radioDesOff.Size = new System.Drawing.Size(39, 17);
            this.radioDesOff.TabIndex = 0;
            this.radioDesOff.TabStop = true;
            this.radioDesOff.Text = "Off";
            this.radioDesOff.UseVisualStyleBackColor = true;
            this.radioDesOff.CheckedChanged += new System.EventHandler(this.radioDesOff_CheckedChanged);
            // 
            // txtRTC
            // 
            this.txtRTC.Location = new System.Drawing.Point(110, 370);
            this.txtRTC.Name = "txtRTC";
            this.txtRTC.Size = new System.Drawing.Size(123, 20);
            this.txtRTC.TabIndex = 41;
            // 
            // btnSetClock
            // 
            this.btnSetClock.Location = new System.Drawing.Point(17, 369);
            this.btnSetClock.Name = "btnSetClock";
            this.btnSetClock.Size = new System.Drawing.Size(75, 23);
            this.btnSetClock.TabIndex = 40;
            this.btnSetClock.Text = "Set Clock";
            this.btnSetClock.UseVisualStyleBackColor = true;
            this.btnSetClock.Click += new System.EventHandler(this.btnSetClock_Click);
            // 
            // txtSys30Return
            // 
            this.txtSys30Return.Location = new System.Drawing.Point(394, 260);
            this.txtSys30Return.Name = "txtSys30Return";
            this.txtSys30Return.Size = new System.Drawing.Size(44, 20);
            this.txtSys30Return.TabIndex = 39;
            this.txtSys30Return.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Location = new System.Drawing.Point(315, 263);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(73, 14);
            this.label78.TabIndex = 38;
            this.label78.Text = "Sys30 Return";
            // 
            // txtSys30Supply
            // 
            this.txtSys30Supply.Location = new System.Drawing.Point(394, 234);
            this.txtSys30Supply.Name = "txtSys30Supply";
            this.txtSys30Supply.Size = new System.Drawing.Size(44, 20);
            this.txtSys30Supply.TabIndex = 37;
            this.txtSys30Supply.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSys30W
            // 
            this.txtSys30W.Location = new System.Drawing.Point(394, 208);
            this.txtSys30W.Name = "txtSys30W";
            this.txtSys30W.Size = new System.Drawing.Size(44, 20);
            this.txtSys30W.TabIndex = 36;
            this.txtSys30W.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSys30lph
            // 
            this.txtSys30lph.Location = new System.Drawing.Point(394, 182);
            this.txtSys30lph.Name = "txtSys30lph";
            this.txtSys30lph.Size = new System.Drawing.Size(44, 20);
            this.txtSys30lph.TabIndex = 35;
            this.txtSys30lph.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Location = new System.Drawing.Point(315, 237);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(74, 14);
            this.label75.TabIndex = 34;
            this.label75.Text = "Sys30 Supply";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Location = new System.Drawing.Point(315, 211);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(51, 14);
            this.label76.TabIndex = 33;
            this.label76.Text = "Sys30 W";
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Location = new System.Drawing.Point(315, 185);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(57, 14);
            this.label77.TabIndex = 32;
            this.label77.Text = "Sys30 l/Hr";
            // 
            // txtFceOut4
            // 
            this.txtFceOut4.Location = new System.Drawing.Point(401, 130);
            this.txtFceOut4.Name = "txtFceOut4";
            this.txtFceOut4.Size = new System.Drawing.Size(37, 20);
            this.txtFceOut4.TabIndex = 31;
            // 
            // txtFceOut3
            // 
            this.txtFceOut3.Location = new System.Drawing.Point(401, 104);
            this.txtFceOut3.Name = "txtFceOut3";
            this.txtFceOut3.Size = new System.Drawing.Size(37, 20);
            this.txtFceOut3.TabIndex = 30;
            // 
            // txtFceOut2
            // 
            this.txtFceOut2.Location = new System.Drawing.Point(401, 78);
            this.txtFceOut2.Name = "txtFceOut2";
            this.txtFceOut2.Size = new System.Drawing.Size(37, 20);
            this.txtFceOut2.TabIndex = 29;
            // 
            // txtFceOut1
            // 
            this.txtFceOut1.Location = new System.Drawing.Point(401, 52);
            this.txtFceOut1.Name = "txtFceOut1";
            this.txtFceOut1.Size = new System.Drawing.Size(37, 20);
            this.txtFceOut1.TabIndex = 28;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(336, 133);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(59, 14);
            this.label41.TabIndex = 27;
            this.label41.Text = "FCE OUT 4";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(336, 107);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(59, 14);
            this.label40.TabIndex = 26;
            this.label40.Text = "FCE OUT 3";
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(336, 81);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(59, 14);
            this.label39.TabIndex = 25;
            this.label39.Text = "FCE OUT 2";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(336, 55);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(59, 14);
            this.label38.TabIndex = 24;
            this.label38.Text = "FCE OUT 1";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(168, 251);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(61, 14);
            this.label31.TabIndex = 23;
            this.label31.Text = "DNI (W/m2)";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(182, 211);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(51, 14);
            this.label30.TabIndex = 22;
            this.label30.Text = "Flow Sw";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(182, 185);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(47, 14);
            this.label29.TabIndex = 21;
            this.label29.Text = "RTD A/D";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(182, 159);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(47, 14);
            this.label28.TabIndex = 20;
            this.label28.Text = "FCE IN 5";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(182, 133);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(47, 14);
            this.label27.TabIndex = 19;
            this.label27.Text = "FCE IN 4";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(182, 107);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(47, 14);
            this.label26.TabIndex = 18;
            this.label26.Text = "FCE IN 3";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(182, 81);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(47, 14);
            this.label25.TabIndex = 17;
            this.label25.Text = "FCE IN 2";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(182, 55);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(47, 14);
            this.label24.TabIndex = 16;
            this.label24.Text = "FCE IN 1";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(7, 81);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(71, 14);
            this.label23.TabIndex = 15;
            this.label23.Text = "FCE bus OUT";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(7, 107);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(59, 14);
            this.label22.TabIndex = 14;
            this.label22.Text = "FCE bus IN";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 55);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(86, 14);
            this.label21.TabIndex = 13;
            this.label21.Text = "FCE Input States";
            // 
            // txtFceDNI
            // 
            this.txtFceDNI.Location = new System.Drawing.Point(235, 248);
            this.txtFceDNI.Name = "txtFceDNI";
            this.txtFceDNI.Size = new System.Drawing.Size(42, 20);
            this.txtFceDNI.TabIndex = 12;
            this.txtFceDNI.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceFlowSw
            // 
            this.txtFceFlowSw.Location = new System.Drawing.Point(235, 208);
            this.txtFceFlowSw.Name = "txtFceFlowSw";
            this.txtFceFlowSw.Size = new System.Drawing.Size(42, 20);
            this.txtFceFlowSw.TabIndex = 11;
            this.txtFceFlowSw.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceRtdAD
            // 
            this.txtFceRtdAD.Location = new System.Drawing.Point(235, 182);
            this.txtFceRtdAD.Name = "txtFceRtdAD";
            this.txtFceRtdAD.Size = new System.Drawing.Size(42, 20);
            this.txtFceRtdAD.TabIndex = 10;
            this.txtFceRtdAD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceIn5
            // 
            this.txtFceIn5.Location = new System.Drawing.Point(235, 156);
            this.txtFceIn5.Name = "txtFceIn5";
            this.txtFceIn5.Size = new System.Drawing.Size(42, 20);
            this.txtFceIn5.TabIndex = 9;
            this.txtFceIn5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceIn4
            // 
            this.txtFceIn4.Location = new System.Drawing.Point(235, 130);
            this.txtFceIn4.Name = "txtFceIn4";
            this.txtFceIn4.Size = new System.Drawing.Size(42, 20);
            this.txtFceIn4.TabIndex = 8;
            this.txtFceIn4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceIn3
            // 
            this.txtFceIn3.Location = new System.Drawing.Point(235, 104);
            this.txtFceIn3.Name = "txtFceIn3";
            this.txtFceIn3.Size = new System.Drawing.Size(42, 20);
            this.txtFceIn3.TabIndex = 7;
            this.txtFceIn3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceIn2
            // 
            this.txtFceIn2.Location = new System.Drawing.Point(235, 78);
            this.txtFceIn2.Name = "txtFceIn2";
            this.txtFceIn2.Size = new System.Drawing.Size(42, 20);
            this.txtFceIn2.TabIndex = 6;
            this.txtFceIn2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceIn1
            // 
            this.txtFceIn1.Location = new System.Drawing.Point(235, 52);
            this.txtFceIn1.Name = "txtFceIn1";
            this.txtFceIn1.Size = new System.Drawing.Size(42, 20);
            this.txtFceIn1.TabIndex = 5;
            this.txtFceIn1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtFceBusIn
            // 
            this.txtFceBusIn.Location = new System.Drawing.Point(94, 104);
            this.txtFceBusIn.Name = "txtFceBusIn";
            this.txtFceBusIn.Size = new System.Drawing.Size(39, 20);
            this.txtFceBusIn.TabIndex = 4;
            // 
            // txtFceBusOut
            // 
            this.txtFceBusOut.Location = new System.Drawing.Point(94, 78);
            this.txtFceBusOut.Name = "txtFceBusOut";
            this.txtFceBusOut.Size = new System.Drawing.Size(39, 20);
            this.txtFceBusOut.TabIndex = 3;
            // 
            // txtFceInStates
            // 
            this.txtFceInStates.Location = new System.Drawing.Point(94, 52);
            this.txtFceInStates.Name = "txtFceInStates";
            this.txtFceInStates.Size = new System.Drawing.Size(39, 20);
            this.txtFceInStates.TabIndex = 2;
            // 
            // txtVersString
            // 
            this.txtVersString.Location = new System.Drawing.Point(57, 6);
            this.txtVersString.Name = "txtVersString";
            this.txtVersString.Size = new System.Drawing.Size(153, 20);
            this.txtVersString.TabIndex = 1;
            // 
            // tabString
            // 
            this.tabString.Controls.Add(this.label87);
            this.tabString.Controls.Add(this.label83);
            this.tabString.Controls.Add(this.label84);
            this.tabString.Controls.Add(this.label85);
            this.tabString.Controls.Add(this.label86);
            this.tabString.Controls.Add(this.btnStowString);
            this.tabString.Controls.Add(this.btnHomeString);
            this.tabString.Controls.Add(this.label12);
            this.tabString.Controls.Add(this.txtNumMct);
            this.tabString.Controls.Add(this.label13);
            this.tabString.Controls.Add(this.label14);
            this.tabString.Controls.Add(this.label15);
            this.tabString.Controls.Add(this.label16);
            this.tabString.Controls.Add(this.label17);
            this.tabString.Controls.Add(this.label18);
            this.tabString.Controls.Add(this.label19);
            this.tabString.Controls.Add(this.label11);
            this.tabString.Controls.Add(this.label10);
            this.tabString.Controls.Add(this.label9);
            this.tabString.Controls.Add(this.label8);
            this.tabString.Location = new System.Drawing.Point(4, 22);
            this.tabString.Name = "tabString";
            this.tabString.Padding = new System.Windows.Forms.Padding(3);
            this.tabString.Size = new System.Drawing.Size(760, 410);
            this.tabString.TabIndex = 1;
            this.tabString.Text = "Strings";
            this.tabString.UseVisualStyleBackColor = true;
            // 
            // label87
            // 
            this.label87.AutoSize = true;
            this.label87.Location = new System.Drawing.Point(117, 384);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(36, 14);
            this.label87.TabIndex = 62;
            this.label87.Text = "MAN2";
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.Location = new System.Drawing.Point(117, 96);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(51, 14);
            this.label83.TabIndex = 61;
            this.label83.Text = "RTD 1AR";
            // 
            // label84
            // 
            this.label84.AutoSize = true;
            this.label84.Location = new System.Drawing.Point(117, 72);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(50, 14);
            this.label84.TabIndex = 60;
            this.label84.Text = "RTD 1AL";
            // 
            // label85
            // 
            this.label85.AutoSize = true;
            this.label85.Location = new System.Drawing.Point(117, 48);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(48, 14);
            this.label85.TabIndex = 59;
            this.label85.Text = "Posn 1A";
            // 
            // label86
            // 
            this.label86.AutoSize = true;
            this.label86.Location = new System.Drawing.Point(117, 24);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(43, 14);
            this.label86.TabIndex = 58;
            this.label86.Text = "Track 1";
            // 
            // btnStowString
            // 
            this.btnStowString.Location = new System.Drawing.Point(19, 375);
            this.btnStowString.Name = "btnStowString";
            this.btnStowString.Size = new System.Drawing.Size(81, 23);
            this.btnStowString.TabIndex = 57;
            this.btnStowString.Text = "Stow String";
            this.btnStowString.UseVisualStyleBackColor = true;
            this.btnStowString.Click += new System.EventHandler(this.btnStowString_Click);
            // 
            // btnHomeString
            // 
            this.btnHomeString.Location = new System.Drawing.Point(20, 346);
            this.btnHomeString.Name = "btnHomeString";
            this.btnHomeString.Size = new System.Drawing.Size(81, 23);
            this.btnHomeString.TabIndex = 56;
            this.btnHomeString.Text = "Home String";
            this.btnHomeString.UseVisualStyleBackColor = true;
            this.btnHomeString.Click += new System.EventHandler(this.btnHomeString_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 21);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(36, 14);
            this.label12.TabIndex = 48;
            this.label12.Text = "MCT\'s";
            // 
            // txtNumMct
            // 
            this.txtNumMct.Location = new System.Drawing.Point(53, 18);
            this.txtNumMct.Name = "txtNumMct";
            this.txtNumMct.Size = new System.Drawing.Size(29, 20);
            this.txtNumMct.TabIndex = 46;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(117, 360);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(50, 14);
            this.label13.TabIndex = 43;
            this.label13.Text = "RTD 2BR";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(117, 336);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(49, 14);
            this.label14.TabIndex = 41;
            this.label14.Text = "RTD 2BL";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(117, 312);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(47, 14);
            this.label15.TabIndex = 39;
            this.label15.Text = "Posn 2B";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(117, 288);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(51, 14);
            this.label16.TabIndex = 37;
            this.label16.Text = "RTD 2AR";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(117, 264);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(50, 14);
            this.label17.TabIndex = 35;
            this.label17.Text = "RTD 2AL";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(117, 240);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(48, 14);
            this.label18.TabIndex = 33;
            this.label18.Text = "Posn 2A";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(117, 216);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(43, 14);
            this.label19.TabIndex = 31;
            this.label19.Text = "Track 2";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(117, 192);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(36, 14);
            this.label11.TabIndex = 20;
            this.label11.Text = "MAN1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(117, 168);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 14);
            this.label10.TabIndex = 18;
            this.label10.Text = "RTD 1BR";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(117, 144);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(49, 14);
            this.label9.TabIndex = 16;
            this.label9.Text = "RTD 1BL";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(117, 120);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 14);
            this.label8.TabIndex = 14;
            this.label8.Text = "Posn 1B";
            // 
            // tabMCTfw
            // 
            this.tabMCTfw.Controls.Add(this.label80);
            this.tabMCTfw.Controls.Add(this.label81);
            this.tabMCTfw.Controls.Add(this.label79);
            this.tabMCTfw.Controls.Add(this.label74);
            this.tabMCTfw.Controls.Add(this.btnGetMctVersions);
            this.tabMCTfw.Controls.Add(this.label37);
            this.tabMCTfw.Controls.Add(this.txtFwSlave);
            this.tabMCTfw.Controls.Add(this.btnUpdateMctSlave);
            this.tabMCTfw.Controls.Add(this.label36);
            this.tabMCTfw.Controls.Add(this.txtFwMaster);
            this.tabMCTfw.Controls.Add(this.btnUpdateMctMaster);
            this.tabMCTfw.Controls.Add(this.cbFwStopMct);
            this.tabMCTfw.Controls.Add(this.label35);
            this.tabMCTfw.Controls.Add(this.cbFwStartMct);
            this.tabMCTfw.Controls.Add(this.label34);
            this.tabMCTfw.Controls.Add(this.cbFwStopString);
            this.tabMCTfw.Controls.Add(this.label33);
            this.tabMCTfw.Controls.Add(this.cbFwStartString);
            this.tabMCTfw.Controls.Add(this.label32);
            this.tabMCTfw.Location = new System.Drawing.Point(4, 22);
            this.tabMCTfw.Name = "tabMCTfw";
            this.tabMCTfw.Size = new System.Drawing.Size(760, 410);
            this.tabMCTfw.TabIndex = 2;
            this.tabMCTfw.Text = "MCT Firmware";
            this.tabMCTfw.UseVisualStyleBackColor = true;
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.Location = new System.Drawing.Point(522, 3);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(98, 14);
            this.label80.TabIndex = 18;
            this.label80.Text = "MCT Slave Version";
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.Location = new System.Drawing.Point(217, 3);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(104, 14);
            this.label81.TabIndex = 17;
            this.label81.Text = "MCT Master Version";
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.Location = new System.Drawing.Point(475, 3);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(30, 14);
            this.label79.TabIndex = 16;
            this.label79.Text = "Valid";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Location = new System.Drawing.Point(170, 3);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(30, 14);
            this.label74.TabIndex = 15;
            this.label74.Text = "Valid";
            // 
            // btnGetMctVersions
            // 
            this.btnGetMctVersions.Location = new System.Drawing.Point(397, 285);
            this.btnGetMctVersions.Name = "btnGetMctVersions";
            this.btnGetMctVersions.Size = new System.Drawing.Size(85, 23);
            this.btnGetMctVersions.TabIndex = 14;
            this.btnGetMctVersions.Text = "Get Versions";
            this.btnGetMctVersions.UseVisualStyleBackColor = true;
            this.btnGetMctVersions.Click += new System.EventHandler(this.btnGetMctVersions_Click);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(196, 307);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(122, 14);
            this.label37.TabIndex = 13;
            this.label37.Text = "MCT Slave FW Filename";
            // 
            // txtFwSlave
            // 
            this.txtFwSlave.Location = new System.Drawing.Point(199, 324);
            this.txtFwSlave.Name = "txtFwSlave";
            this.txtFwSlave.Size = new System.Drawing.Size(125, 20);
            this.txtFwSlave.TabIndex = 12;
            this.txtFwSlave.Text = "MCTslave.abs.s19";
            // 
            // btnUpdateMctSlave
            // 
            this.btnUpdateMctSlave.Location = new System.Drawing.Point(199, 350);
            this.btnUpdateMctSlave.Name = "btnUpdateMctSlave";
            this.btnUpdateMctSlave.Size = new System.Drawing.Size(75, 46);
            this.btnUpdateMctSlave.TabIndex = 11;
            this.btnUpdateMctSlave.Text = "Update MCT Slave FW";
            this.btnUpdateMctSlave.UseVisualStyleBackColor = true;
            this.btnUpdateMctSlave.Click += new System.EventHandler(this.btnUpdateMctSlave_Click);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(20, 307);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(128, 14);
            this.label36.TabIndex = 10;
            this.label36.Text = "MCT Master FW Filename";
            // 
            // txtFwMaster
            // 
            this.txtFwMaster.Location = new System.Drawing.Point(23, 324);
            this.txtFwMaster.Name = "txtFwMaster";
            this.txtFwMaster.Size = new System.Drawing.Size(125, 20);
            this.txtFwMaster.TabIndex = 9;
            this.txtFwMaster.Text = "MCTmaster.abs.s19";
            // 
            // btnUpdateMctMaster
            // 
            this.btnUpdateMctMaster.Location = new System.Drawing.Point(23, 350);
            this.btnUpdateMctMaster.Name = "btnUpdateMctMaster";
            this.btnUpdateMctMaster.Size = new System.Drawing.Size(75, 46);
            this.btnUpdateMctMaster.TabIndex = 8;
            this.btnUpdateMctMaster.Text = "Update MCT Master FW";
            this.btnUpdateMctMaster.UseVisualStyleBackColor = true;
            this.btnUpdateMctMaster.Click += new System.EventHandler(this.btnUpdateMctMaster_Click);
            // 
            // cbFwStopMct
            // 
            this.cbFwStopMct.FormattingEnabled = true;
            this.cbFwStopMct.Items.AddRange(new object[] {
            "ALL MCT\'s",
            "MCT 1",
            "MCT 2",
            "MCT 3",
            "MCT 4",
            "MCT 5",
            "MCT 6",
            "MCT 7",
            "MCT 8",
            "MCT 9",
            "MCT 10"});
            this.cbFwStopMct.Location = new System.Drawing.Point(30, 209);
            this.cbFwStopMct.Name = "cbFwStopMct";
            this.cbFwStopMct.Size = new System.Drawing.Size(79, 22);
            this.cbFwStopMct.TabIndex = 7;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(27, 192);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(89, 14);
            this.label35.TabIndex = 6;
            this.label35.Text = "Stop MCT Master";
            // 
            // cbFwStartMct
            // 
            this.cbFwStartMct.FormattingEnabled = true;
            this.cbFwStartMct.Items.AddRange(new object[] {
            "ALL MCT\'s",
            "MCT 1",
            "MCT 2",
            "MCT 3",
            "MCT 4",
            "MCT 5",
            "MCT 6",
            "MCT 7",
            "MCT 8",
            "MCT 9",
            "MCT 10"});
            this.cbFwStartMct.Location = new System.Drawing.Point(30, 163);
            this.cbFwStartMct.Name = "cbFwStartMct";
            this.cbFwStartMct.Size = new System.Drawing.Size(79, 22);
            this.cbFwStartMct.TabIndex = 5;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(27, 146);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(90, 14);
            this.label34.TabIndex = 4;
            this.label34.Text = "Start MCT Master";
            // 
            // cbFwStopString
            // 
            this.cbFwStopString.FormattingEnabled = true;
            this.cbFwStopString.Items.AddRange(new object[] {
            "String A",
            "String B",
            "String C",
            "String D"});
            this.cbFwStopString.Location = new System.Drawing.Point(30, 88);
            this.cbFwStopString.Name = "cbFwStopString";
            this.cbFwStopString.Size = new System.Drawing.Size(79, 22);
            this.cbFwStopString.TabIndex = 3;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(27, 71);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(60, 14);
            this.label33.TabIndex = 2;
            this.label33.Text = "Stop String";
            // 
            // cbFwStartString
            // 
            this.cbFwStartString.FormattingEnabled = true;
            this.cbFwStartString.Items.AddRange(new object[] {
            "String A",
            "String B",
            "String C",
            "String D"});
            this.cbFwStartString.Location = new System.Drawing.Point(30, 37);
            this.cbFwStartString.Name = "cbFwStartString";
            this.cbFwStartString.Size = new System.Drawing.Size(79, 22);
            this.cbFwStartString.TabIndex = 1;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(27, 20);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(61, 14);
            this.label32.TabIndex = 0;
            this.label32.Text = "Start String";
            // 
            // tabMctParam
            // 
            this.tabMctParam.Controls.Add(this.btnWriteParam10);
            this.tabMctParam.Controls.Add(this.btnWriteParam9);
            this.tabMctParam.Controls.Add(this.btnWriteParam8);
            this.tabMctParam.Controls.Add(this.btnWriteParam7);
            this.tabMctParam.Controls.Add(this.btnWriteParam6);
            this.tabMctParam.Controls.Add(this.btnWriteParam5);
            this.tabMctParam.Controls.Add(this.btnWriteParam4);
            this.tabMctParam.Controls.Add(this.btnWriteParam3);
            this.tabMctParam.Controls.Add(this.btnWriteParam2);
            this.tabMctParam.Controls.Add(this.btnWriteParam1);
            this.tabMctParam.Controls.Add(this.label73);
            this.tabMctParam.Controls.Add(this.btnRealAllParam);
            this.tabMctParam.Controls.Add(this.btnWriteAllParam);
            this.tabMctParam.Location = new System.Drawing.Point(4, 22);
            this.tabMctParam.Name = "tabMctParam";
            this.tabMctParam.Size = new System.Drawing.Size(760, 410);
            this.tabMctParam.TabIndex = 3;
            this.tabMctParam.Text = "MCT Parameters";
            this.tabMctParam.UseVisualStyleBackColor = true;
            // 
            // btnWriteParam10
            // 
            this.btnWriteParam10.Location = new System.Drawing.Point(720, 383);
            this.btnWriteParam10.Name = "btnWriteParam10";
            this.btnWriteParam10.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam10.TabIndex = 12;
            this.btnWriteParam10.Text = "Wr10";
            this.btnWriteParam10.UseVisualStyleBackColor = true;
            this.btnWriteParam10.Click += new System.EventHandler(this.btnWriteParam10_Click);
            // 
            // btnWriteParam9
            // 
            this.btnWriteParam9.Location = new System.Drawing.Point(675, 383);
            this.btnWriteParam9.Name = "btnWriteParam9";
            this.btnWriteParam9.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam9.TabIndex = 11;
            this.btnWriteParam9.Text = "Wr9";
            this.btnWriteParam9.UseVisualStyleBackColor = true;
            this.btnWriteParam9.Click += new System.EventHandler(this.btnWriteParam9_Click);
            // 
            // btnWriteParam8
            // 
            this.btnWriteParam8.Location = new System.Drawing.Point(630, 383);
            this.btnWriteParam8.Name = "btnWriteParam8";
            this.btnWriteParam8.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam8.TabIndex = 10;
            this.btnWriteParam8.Text = "Wr8";
            this.btnWriteParam8.UseVisualStyleBackColor = true;
            this.btnWriteParam8.Click += new System.EventHandler(this.btnWriteParam8_Click);
            // 
            // btnWriteParam7
            // 
            this.btnWriteParam7.Location = new System.Drawing.Point(585, 383);
            this.btnWriteParam7.Name = "btnWriteParam7";
            this.btnWriteParam7.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam7.TabIndex = 9;
            this.btnWriteParam7.Text = "Wr7";
            this.btnWriteParam7.UseVisualStyleBackColor = true;
            this.btnWriteParam7.Click += new System.EventHandler(this.btnWriteParam7_Click);
            // 
            // btnWriteParam6
            // 
            this.btnWriteParam6.Location = new System.Drawing.Point(540, 383);
            this.btnWriteParam6.Name = "btnWriteParam6";
            this.btnWriteParam6.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam6.TabIndex = 8;
            this.btnWriteParam6.Text = "Wr6";
            this.btnWriteParam6.UseVisualStyleBackColor = true;
            this.btnWriteParam6.Click += new System.EventHandler(this.btnWriteParam6_Click);
            // 
            // btnWriteParam5
            // 
            this.btnWriteParam5.Location = new System.Drawing.Point(495, 383);
            this.btnWriteParam5.Name = "btnWriteParam5";
            this.btnWriteParam5.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam5.TabIndex = 7;
            this.btnWriteParam5.Text = "Wr5";
            this.btnWriteParam5.UseVisualStyleBackColor = true;
            this.btnWriteParam5.Click += new System.EventHandler(this.btnWriteParam5_Click);
            // 
            // btnWriteParam4
            // 
            this.btnWriteParam4.Location = new System.Drawing.Point(450, 383);
            this.btnWriteParam4.Name = "btnWriteParam4";
            this.btnWriteParam4.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam4.TabIndex = 6;
            this.btnWriteParam4.Text = "Wr4";
            this.btnWriteParam4.UseVisualStyleBackColor = true;
            this.btnWriteParam4.Click += new System.EventHandler(this.btnWriteParam4_Click);
            // 
            // btnWriteParam3
            // 
            this.btnWriteParam3.Location = new System.Drawing.Point(405, 383);
            this.btnWriteParam3.Name = "btnWriteParam3";
            this.btnWriteParam3.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam3.TabIndex = 5;
            this.btnWriteParam3.Text = "Wr3";
            this.btnWriteParam3.UseVisualStyleBackColor = true;
            this.btnWriteParam3.Click += new System.EventHandler(this.btnWriteParam3_Click);
            // 
            // btnWriteParam2
            // 
            this.btnWriteParam2.Location = new System.Drawing.Point(360, 383);
            this.btnWriteParam2.Name = "btnWriteParam2";
            this.btnWriteParam2.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam2.TabIndex = 4;
            this.btnWriteParam2.Text = "Wr2";
            this.btnWriteParam2.UseVisualStyleBackColor = true;
            this.btnWriteParam2.Click += new System.EventHandler(this.btnWriteParam2_Click);
            // 
            // btnWriteParam1
            // 
            this.btnWriteParam1.Location = new System.Drawing.Point(315, 383);
            this.btnWriteParam1.Name = "btnWriteParam1";
            this.btnWriteParam1.Size = new System.Drawing.Size(40, 23);
            this.btnWriteParam1.TabIndex = 3;
            this.btnWriteParam1.Text = "Wr1";
            this.btnWriteParam1.UseVisualStyleBackColor = true;
            this.btnWriteParam1.Click += new System.EventHandler(this.btnWriteParam1_Click);
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Location = new System.Drawing.Point(18, 3);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(41, 14);
            this.label73.TabIndex = 2;
            this.label73.Text = "Default";
            // 
            // btnRealAllParam
            // 
            this.btnRealAllParam.Location = new System.Drawing.Point(123, 383);
            this.btnRealAllParam.Name = "btnRealAllParam";
            this.btnRealAllParam.Size = new System.Drawing.Size(75, 23);
            this.btnRealAllParam.TabIndex = 1;
            this.btnRealAllParam.Text = "Read All";
            this.btnRealAllParam.UseVisualStyleBackColor = true;
            this.btnRealAllParam.Click += new System.EventHandler(this.btnRealAllParam_Click);
            // 
            // btnWriteAllParam
            // 
            this.btnWriteAllParam.Location = new System.Drawing.Point(3, 383);
            this.btnWriteAllParam.Name = "btnWriteAllParam";
            this.btnWriteAllParam.Size = new System.Drawing.Size(75, 23);
            this.btnWriteAllParam.TabIndex = 0;
            this.btnWriteAllParam.Text = "Write All";
            this.btnWriteAllParam.UseVisualStyleBackColor = true;
            this.btnWriteAllParam.Click += new System.EventHandler(this.btnWriteAllParam_Click);
            // 
            // tabMctControl
            // 
            this.tabMctControl.Controls.Add(this.txtMctError1);
            this.tabMctControl.Controls.Add(this.label71);
            this.tabMctControl.Controls.Add(this.txtMctError2);
            this.tabMctControl.Controls.Add(this.label72);
            this.tabMctControl.Controls.Add(this.txtMctHumid2);
            this.tabMctControl.Controls.Add(this.label69);
            this.tabMctControl.Controls.Add(this.txtMctHumid1);
            this.tabMctControl.Controls.Add(this.label70);
            this.tabMctControl.Controls.Add(this.txtMctLocalTempA);
            this.tabMctControl.Controls.Add(this.label66);
            this.tabMctControl.Controls.Add(this.txtMctLocalTempB);
            this.tabMctControl.Controls.Add(this.label68);
            this.tabMctControl.Controls.Add(this.label67);
            this.tabMctControl.Controls.Add(this.cbMctPollAddr);
            this.tabMctControl.Controls.Add(this.btnMctMoveSteps2);
            this.tabMctControl.Controls.Add(this.btnMctMoveDeg2);
            this.tabMctControl.Controls.Add(this.txtMctMoveSteps2);
            this.tabMctControl.Controls.Add(this.txtMctMoveDeg2);
            this.tabMctControl.Controls.Add(this.btnMctStow2);
            this.tabMctControl.Controls.Add(this.btnMctHome2);
            this.tabMctControl.Controls.Add(this.checkMctHome2B);
            this.tabMctControl.Controls.Add(this.checkMctStow2B);
            this.tabMctControl.Controls.Add(this.checkMctHome2A);
            this.tabMctControl.Controls.Add(this.checkMctStow2A);
            this.tabMctControl.Controls.Add(this.cbMctTrack2);
            this.tabMctControl.Controls.Add(this.txtMctSteps2A);
            this.tabMctControl.Controls.Add(this.label42);
            this.tabMctControl.Controls.Add(this.txtMctTarg2A);
            this.tabMctControl.Controls.Add(this.label43);
            this.tabMctControl.Controls.Add(this.txtMctSteps2B);
            this.tabMctControl.Controls.Add(this.label44);
            this.tabMctControl.Controls.Add(this.txtMctTarg2B);
            this.tabMctControl.Controls.Add(this.label45);
            this.tabMctControl.Controls.Add(this.txtMctMan2);
            this.tabMctControl.Controls.Add(this.txtMctRtd2BR);
            this.tabMctControl.Controls.Add(this.txtMctRtd2BL);
            this.tabMctControl.Controls.Add(this.txtMctPosn2B);
            this.tabMctControl.Controls.Add(this.txtMctRtd2AR);
            this.tabMctControl.Controls.Add(this.txtMctRtd2AL);
            this.tabMctControl.Controls.Add(this.txtMctPosn2A);
            this.tabMctControl.Controls.Add(this.label46);
            this.tabMctControl.Controls.Add(this.label47);
            this.tabMctControl.Controls.Add(this.label48);
            this.tabMctControl.Controls.Add(this.label49);
            this.tabMctControl.Controls.Add(this.label60);
            this.tabMctControl.Controls.Add(this.label61);
            this.tabMctControl.Controls.Add(this.label62);
            this.tabMctControl.Controls.Add(this.label63);
            this.tabMctControl.Controls.Add(this.btnMctMoveSteps1);
            this.tabMctControl.Controls.Add(this.btnMctMoveDeg1);
            this.tabMctControl.Controls.Add(this.txtMctMoveSteps1);
            this.tabMctControl.Controls.Add(this.txtMctMoveDeg1);
            this.tabMctControl.Controls.Add(this.btnMctStow1);
            this.tabMctControl.Controls.Add(this.btnMctHome1);
            this.tabMctControl.Controls.Add(this.checkMctHome1B);
            this.tabMctControl.Controls.Add(this.checkMctStow1B);
            this.tabMctControl.Controls.Add(this.checkMctHome1A);
            this.tabMctControl.Controls.Add(this.checkMctStow1A);
            this.tabMctControl.Controls.Add(this.cbMctTrack1);
            this.tabMctControl.Controls.Add(this.txtMctSteps1A);
            this.tabMctControl.Controls.Add(this.label65);
            this.tabMctControl.Controls.Add(this.txtMctTarg1A);
            this.tabMctControl.Controls.Add(this.label64);
            this.tabMctControl.Controls.Add(this.txtMctSteps1B);
            this.tabMctControl.Controls.Add(this.label59);
            this.tabMctControl.Controls.Add(this.txtMctTarg1B);
            this.tabMctControl.Controls.Add(this.label58);
            this.tabMctControl.Controls.Add(this.txtMctMan1);
            this.tabMctControl.Controls.Add(this.txtMctRtd1BR);
            this.tabMctControl.Controls.Add(this.txtMctRtd1BL);
            this.tabMctControl.Controls.Add(this.txtMctPosn1B);
            this.tabMctControl.Controls.Add(this.txtMctRtd1AR);
            this.tabMctControl.Controls.Add(this.txtMctRtd1AL);
            this.tabMctControl.Controls.Add(this.txtMctPosn1A);
            this.tabMctControl.Controls.Add(this.label50);
            this.tabMctControl.Controls.Add(this.label51);
            this.tabMctControl.Controls.Add(this.label52);
            this.tabMctControl.Controls.Add(this.label53);
            this.tabMctControl.Controls.Add(this.label54);
            this.tabMctControl.Controls.Add(this.label55);
            this.tabMctControl.Controls.Add(this.label56);
            this.tabMctControl.Controls.Add(this.label57);
            this.tabMctControl.Location = new System.Drawing.Point(4, 22);
            this.tabMctControl.Name = "tabMctControl";
            this.tabMctControl.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tabMctControl.Size = new System.Drawing.Size(760, 410);
            this.tabMctControl.TabIndex = 4;
            this.tabMctControl.Text = "MCT Control";
            this.tabMctControl.UseVisualStyleBackColor = true;
            // 
            // txtMctError1
            // 
            this.txtMctError1.Location = new System.Drawing.Point(69, 172);
            this.txtMctError1.Name = "txtMctError1";
            this.txtMctError1.Size = new System.Drawing.Size(127, 20);
            this.txtMctError1.TabIndex = 142;
            this.txtMctError1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.Location = new System.Drawing.Point(11, 175);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(54, 14);
            this.label71.TabIndex = 141;
            this.label71.Text = "Error Mir1";
            // 
            // txtMctError2
            // 
            this.txtMctError2.Location = new System.Drawing.Point(69, 148);
            this.txtMctError2.Name = "txtMctError2";
            this.txtMctError2.Size = new System.Drawing.Size(127, 20);
            this.txtMctError2.TabIndex = 140;
            this.txtMctError2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.Location = new System.Drawing.Point(11, 151);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(54, 14);
            this.label72.TabIndex = 139;
            this.label72.Text = "Error Mir2";
            // 
            // txtMctHumid2
            // 
            this.txtMctHumid2.Location = new System.Drawing.Point(551, 18);
            this.txtMctHumid2.Name = "txtMctHumid2";
            this.txtMctHumid2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctHumid2.Size = new System.Drawing.Size(46, 20);
            this.txtMctHumid2.TabIndex = 138;
            this.txtMctHumid2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.Location = new System.Drawing.Point(497, 21);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(53, 14);
            this.label69.TabIndex = 137;
            this.label69.Text = "Humidity2";
            // 
            // txtMctHumid1
            // 
            this.txtMctHumid1.Location = new System.Drawing.Point(279, 18);
            this.txtMctHumid1.Name = "txtMctHumid1";
            this.txtMctHumid1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctHumid1.Size = new System.Drawing.Size(46, 20);
            this.txtMctHumid1.TabIndex = 136;
            this.txtMctHumid1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.Location = new System.Drawing.Point(225, 21);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(53, 14);
            this.label70.TabIndex = 135;
            this.label70.Text = "Humidity1";
            // 
            // txtMctLocalTempA
            // 
            this.txtMctLocalTempA.Location = new System.Drawing.Point(79, 94);
            this.txtMctLocalTempA.Name = "txtMctLocalTempA";
            this.txtMctLocalTempA.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctLocalTempA.Size = new System.Drawing.Size(46, 20);
            this.txtMctLocalTempA.TabIndex = 134;
            this.txtMctLocalTempA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Location = new System.Drawing.Point(11, 97);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(65, 14);
            this.label66.TabIndex = 133;
            this.label66.Text = "PCB Temp A";
            // 
            // txtMctLocalTempB
            // 
            this.txtMctLocalTempB.Location = new System.Drawing.Point(79, 70);
            this.txtMctLocalTempB.Name = "txtMctLocalTempB";
            this.txtMctLocalTempB.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctLocalTempB.Size = new System.Drawing.Size(46, 20);
            this.txtMctLocalTempB.TabIndex = 132;
            this.txtMctLocalTempB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.Location = new System.Drawing.Point(11, 73);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(65, 14);
            this.label68.TabIndex = 131;
            this.label68.Text = "PCB Temp B";
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.Location = new System.Drawing.Point(12, 18);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(72, 14);
            this.label67.TabIndex = 130;
            this.label67.Text = "MCT Address";
            // 
            // cbMctPollAddr
            // 
            this.cbMctPollAddr.FormattingEnabled = true;
            this.cbMctPollAddr.Location = new System.Drawing.Point(91, 13);
            this.cbMctPollAddr.Name = "cbMctPollAddr";
            this.cbMctPollAddr.Size = new System.Drawing.Size(55, 22);
            this.cbMctPollAddr.TabIndex = 129;
            this.cbMctPollAddr.SelectedIndexChanged += new System.EventHandler(this.cbMctPollAddr_SelectedIndexChanged);
            // 
            // btnMctMoveSteps2
            // 
            this.btnMctMoveSteps2.Location = new System.Drawing.Point(486, 373);
            this.btnMctMoveSteps2.Name = "btnMctMoveSteps2";
            this.btnMctMoveSteps2.Size = new System.Drawing.Size(75, 23);
            this.btnMctMoveSteps2.TabIndex = 126;
            this.btnMctMoveSteps2.Text = "Move Steps";
            this.btnMctMoveSteps2.UseVisualStyleBackColor = true;
            this.btnMctMoveSteps2.Click += new System.EventHandler(this.btnMctMoveSteps2_Click);
            // 
            // btnMctMoveDeg2
            // 
            this.btnMctMoveDeg2.Location = new System.Drawing.Point(486, 344);
            this.btnMctMoveDeg2.Name = "btnMctMoveDeg2";
            this.btnMctMoveDeg2.Size = new System.Drawing.Size(75, 23);
            this.btnMctMoveDeg2.TabIndex = 125;
            this.btnMctMoveDeg2.Text = "Move Deg";
            this.btnMctMoveDeg2.UseVisualStyleBackColor = true;
            this.btnMctMoveDeg2.Click += new System.EventHandler(this.btnMctMoveDeg2_Click);
            // 
            // txtMctMoveSteps2
            // 
            this.txtMctMoveSteps2.Location = new System.Drawing.Point(576, 374);
            this.txtMctMoveSteps2.Name = "txtMctMoveSteps2";
            this.txtMctMoveSteps2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMoveSteps2.Size = new System.Drawing.Size(46, 20);
            this.txtMctMoveSteps2.TabIndex = 124;
            this.txtMctMoveSteps2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctMoveDeg2
            // 
            this.txtMctMoveDeg2.Location = new System.Drawing.Point(576, 345);
            this.txtMctMoveDeg2.Name = "txtMctMoveDeg2";
            this.txtMctMoveDeg2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMoveDeg2.Size = new System.Drawing.Size(46, 20);
            this.txtMctMoveDeg2.TabIndex = 123;
            this.txtMctMoveDeg2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnMctStow2
            // 
            this.btnMctStow2.Location = new System.Drawing.Point(486, 315);
            this.btnMctStow2.Name = "btnMctStow2";
            this.btnMctStow2.Size = new System.Drawing.Size(75, 23);
            this.btnMctStow2.TabIndex = 122;
            this.btnMctStow2.Text = "Stow";
            this.btnMctStow2.UseVisualStyleBackColor = true;
            this.btnMctStow2.Click += new System.EventHandler(this.btnMctStow2_Click);
            // 
            // btnMctHome2
            // 
            this.btnMctHome2.Location = new System.Drawing.Point(636, 315);
            this.btnMctHome2.Name = "btnMctHome2";
            this.btnMctHome2.Size = new System.Drawing.Size(75, 23);
            this.btnMctHome2.TabIndex = 121;
            this.btnMctHome2.Text = "Home";
            this.btnMctHome2.UseVisualStyleBackColor = true;
            this.btnMctHome2.Click += new System.EventHandler(this.btnMctHome2_Click);
            // 
            // checkMctHome2B
            // 
            this.checkMctHome2B.AutoSize = true;
            this.checkMctHome2B.Location = new System.Drawing.Point(641, 75);
            this.checkMctHome2B.Name = "checkMctHome2B";
            this.checkMctHome2B.Size = new System.Drawing.Size(70, 17);
            this.checkMctHome2B.TabIndex = 120;
            this.checkMctHome2B.Text = "Home 2B";
            this.checkMctHome2B.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctHome2B.UseVisualStyleBackColor = true;
            // 
            // checkMctStow2B
            // 
            this.checkMctStow2B.AutoSize = true;
            this.checkMctStow2B.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctStow2B.Location = new System.Drawing.Point(486, 75);
            this.checkMctStow2B.Name = "checkMctStow2B";
            this.checkMctStow2B.Size = new System.Drawing.Size(66, 17);
            this.checkMctStow2B.TabIndex = 119;
            this.checkMctStow2B.Text = "Stow 2B";
            this.checkMctStow2B.UseVisualStyleBackColor = true;
            // 
            // checkMctHome2A
            // 
            this.checkMctHome2A.AutoSize = true;
            this.checkMctHome2A.Location = new System.Drawing.Point(641, 291);
            this.checkMctHome2A.Name = "checkMctHome2A";
            this.checkMctHome2A.Size = new System.Drawing.Size(70, 17);
            this.checkMctHome2A.TabIndex = 118;
            this.checkMctHome2A.Text = "Home 2A";
            this.checkMctHome2A.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctHome2A.UseVisualStyleBackColor = true;
            // 
            // checkMctStow2A
            // 
            this.checkMctStow2A.AutoSize = true;
            this.checkMctStow2A.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctStow2A.Location = new System.Drawing.Point(486, 291);
            this.checkMctStow2A.Name = "checkMctStow2A";
            this.checkMctStow2A.Size = new System.Drawing.Size(66, 17);
            this.checkMctStow2A.TabIndex = 117;
            this.checkMctStow2A.Text = "Stow 2A";
            this.checkMctStow2A.UseVisualStyleBackColor = true;
            // 
            // cbMctTrack2
            // 
            this.cbMctTrack2.FormattingEnabled = true;
            this.cbMctTrack2.Location = new System.Drawing.Point(551, 44);
            this.cbMctTrack2.Name = "cbMctTrack2";
            this.cbMctTrack2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbMctTrack2.Size = new System.Drawing.Size(160, 22);
            this.cbMctTrack2.TabIndex = 116;
            this.cbMctTrack2.SelectedIndexChanged += new System.EventHandler(this.cbMctTrack2_SelectedIndexChanged);
            // 
            // txtMctSteps2A
            // 
            this.txtMctSteps2A.Location = new System.Drawing.Point(576, 241);
            this.txtMctSteps2A.Name = "txtMctSteps2A";
            this.txtMctSteps2A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctSteps2A.Size = new System.Drawing.Size(46, 20);
            this.txtMctSteps2A.TabIndex = 115;
            this.txtMctSteps2A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(518, 244);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(52, 14);
            this.label42.TabIndex = 114;
            this.label42.Text = "Steps 2A";
            // 
            // txtMctTarg2A
            // 
            this.txtMctTarg2A.Location = new System.Drawing.Point(576, 265);
            this.txtMctTarg2A.Name = "txtMctTarg2A";
            this.txtMctTarg2A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctTarg2A.Size = new System.Drawing.Size(46, 20);
            this.txtMctTarg2A.TabIndex = 113;
            this.txtMctTarg2A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(518, 268);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(54, 14);
            this.label43.TabIndex = 112;
            this.label43.Text = "Target 2A";
            // 
            // txtMctSteps2B
            // 
            this.txtMctSteps2B.Location = new System.Drawing.Point(576, 97);
            this.txtMctSteps2B.Name = "txtMctSteps2B";
            this.txtMctSteps2B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctSteps2B.Size = new System.Drawing.Size(46, 20);
            this.txtMctSteps2B.TabIndex = 111;
            this.txtMctSteps2B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(518, 100);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(51, 14);
            this.label44.TabIndex = 110;
            this.label44.Text = "Steps 2B";
            // 
            // txtMctTarg2B
            // 
            this.txtMctTarg2B.Location = new System.Drawing.Point(576, 121);
            this.txtMctTarg2B.Name = "txtMctTarg2B";
            this.txtMctTarg2B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctTarg2B.Size = new System.Drawing.Size(46, 20);
            this.txtMctTarg2B.TabIndex = 109;
            this.txtMctTarg2B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(518, 124);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(53, 14);
            this.label45.TabIndex = 108;
            this.label45.Text = "Target 2B";
            // 
            // txtMctMan2
            // 
            this.txtMctMan2.Location = new System.Drawing.Point(665, 18);
            this.txtMctMan2.Name = "txtMctMan2";
            this.txtMctMan2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMan2.Size = new System.Drawing.Size(46, 20);
            this.txtMctMan2.TabIndex = 107;
            this.txtMctMan2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd2BR
            // 
            this.txtMctRtd2BR.Location = new System.Drawing.Point(603, 169);
            this.txtMctRtd2BR.Name = "txtMctRtd2BR";
            this.txtMctRtd2BR.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd2BR.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd2BR.TabIndex = 106;
            this.txtMctRtd2BR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd2BL
            // 
            this.txtMctRtd2BL.Location = new System.Drawing.Point(551, 169);
            this.txtMctRtd2BL.Name = "txtMctRtd2BL";
            this.txtMctRtd2BL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd2BL.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd2BL.TabIndex = 105;
            this.txtMctRtd2BL.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctPosn2B
            // 
            this.txtMctPosn2B.Location = new System.Drawing.Point(576, 145);
            this.txtMctPosn2B.Name = "txtMctPosn2B";
            this.txtMctPosn2B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctPosn2B.Size = new System.Drawing.Size(46, 20);
            this.txtMctPosn2B.TabIndex = 104;
            this.txtMctPosn2B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd2AR
            // 
            this.txtMctRtd2AR.Location = new System.Drawing.Point(603, 193);
            this.txtMctRtd2AR.Name = "txtMctRtd2AR";
            this.txtMctRtd2AR.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd2AR.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd2AR.TabIndex = 103;
            this.txtMctRtd2AR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd2AL
            // 
            this.txtMctRtd2AL.Location = new System.Drawing.Point(551, 193);
            this.txtMctRtd2AL.Name = "txtMctRtd2AL";
            this.txtMctRtd2AL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd2AL.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd2AL.TabIndex = 102;
            this.txtMctRtd2AL.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctPosn2A
            // 
            this.txtMctPosn2A.Location = new System.Drawing.Point(576, 217);
            this.txtMctPosn2A.Name = "txtMctPosn2A";
            this.txtMctPosn2A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctPosn2A.Size = new System.Drawing.Size(46, 20);
            this.txtMctPosn2A.TabIndex = 101;
            this.txtMctPosn2A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(628, 21);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(36, 14);
            this.label46.TabIndex = 100;
            this.label46.Text = "MAN2";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(655, 172);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(50, 14);
            this.label47.TabIndex = 99;
            this.label47.Text = "RTD 2BR";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(493, 172);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(49, 14);
            this.label48.TabIndex = 98;
            this.label48.Text = "RTD 2BL";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(518, 148);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(47, 14);
            this.label49.TabIndex = 97;
            this.label49.Text = "Posn 2B";
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Location = new System.Drawing.Point(655, 196);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(51, 14);
            this.label60.TabIndex = 96;
            this.label60.Text = "RTD 2AR";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Location = new System.Drawing.Point(493, 196);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(50, 14);
            this.label61.TabIndex = 95;
            this.label61.Text = "RTD 2AL";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Location = new System.Drawing.Point(518, 220);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(48, 14);
            this.label62.TabIndex = 94;
            this.label62.Text = "Posn 2A";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Location = new System.Drawing.Point(507, 47);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(43, 14);
            this.label63.TabIndex = 93;
            this.label63.Text = "Track 2";
            // 
            // btnMctMoveSteps1
            // 
            this.btnMctMoveSteps1.Location = new System.Drawing.Point(214, 373);
            this.btnMctMoveSteps1.Name = "btnMctMoveSteps1";
            this.btnMctMoveSteps1.Size = new System.Drawing.Size(75, 23);
            this.btnMctMoveSteps1.TabIndex = 92;
            this.btnMctMoveSteps1.Text = "Move Steps";
            this.btnMctMoveSteps1.UseVisualStyleBackColor = true;
            this.btnMctMoveSteps1.Click += new System.EventHandler(this.btnMctMoveSteps1_Click);
            // 
            // btnMctMoveDeg1
            // 
            this.btnMctMoveDeg1.Location = new System.Drawing.Point(214, 344);
            this.btnMctMoveDeg1.Name = "btnMctMoveDeg1";
            this.btnMctMoveDeg1.Size = new System.Drawing.Size(75, 23);
            this.btnMctMoveDeg1.TabIndex = 91;
            this.btnMctMoveDeg1.Text = "Move Deg";
            this.btnMctMoveDeg1.UseVisualStyleBackColor = true;
            this.btnMctMoveDeg1.Click += new System.EventHandler(this.btnMctMoveDeg1_Click);
            // 
            // txtMctMoveSteps1
            // 
            this.txtMctMoveSteps1.Location = new System.Drawing.Point(304, 374);
            this.txtMctMoveSteps1.Name = "txtMctMoveSteps1";
            this.txtMctMoveSteps1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMoveSteps1.Size = new System.Drawing.Size(46, 20);
            this.txtMctMoveSteps1.TabIndex = 90;
            this.txtMctMoveSteps1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctMoveDeg1
            // 
            this.txtMctMoveDeg1.Location = new System.Drawing.Point(304, 345);
            this.txtMctMoveDeg1.Name = "txtMctMoveDeg1";
            this.txtMctMoveDeg1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMoveDeg1.Size = new System.Drawing.Size(46, 20);
            this.txtMctMoveDeg1.TabIndex = 89;
            this.txtMctMoveDeg1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnMctStow1
            // 
            this.btnMctStow1.Location = new System.Drawing.Point(214, 315);
            this.btnMctStow1.Name = "btnMctStow1";
            this.btnMctStow1.Size = new System.Drawing.Size(75, 23);
            this.btnMctStow1.TabIndex = 88;
            this.btnMctStow1.Text = "Stow";
            this.btnMctStow1.UseVisualStyleBackColor = true;
            this.btnMctStow1.Click += new System.EventHandler(this.btnMctStow1_Click);
            // 
            // btnMctHome1
            // 
            this.btnMctHome1.Location = new System.Drawing.Point(364, 315);
            this.btnMctHome1.Name = "btnMctHome1";
            this.btnMctHome1.Size = new System.Drawing.Size(75, 23);
            this.btnMctHome1.TabIndex = 87;
            this.btnMctHome1.Text = "Home";
            this.btnMctHome1.UseVisualStyleBackColor = true;
            this.btnMctHome1.Click += new System.EventHandler(this.btnMctHome1_Click);
            // 
            // checkMctHome1B
            // 
            this.checkMctHome1B.AutoSize = true;
            this.checkMctHome1B.Location = new System.Drawing.Point(370, 75);
            this.checkMctHome1B.Name = "checkMctHome1B";
            this.checkMctHome1B.Size = new System.Drawing.Size(70, 17);
            this.checkMctHome1B.TabIndex = 86;
            this.checkMctHome1B.Text = "Home 1B";
            this.checkMctHome1B.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctHome1B.UseVisualStyleBackColor = true;
            // 
            // checkMctStow1B
            // 
            this.checkMctStow1B.AutoSize = true;
            this.checkMctStow1B.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctStow1B.Location = new System.Drawing.Point(215, 75);
            this.checkMctStow1B.Name = "checkMctStow1B";
            this.checkMctStow1B.Size = new System.Drawing.Size(66, 17);
            this.checkMctStow1B.TabIndex = 85;
            this.checkMctStow1B.Text = "Stow 1B";
            this.checkMctStow1B.UseVisualStyleBackColor = true;
            // 
            // checkMctHome1A
            // 
            this.checkMctHome1A.AutoSize = true;
            this.checkMctHome1A.Location = new System.Drawing.Point(370, 291);
            this.checkMctHome1A.Name = "checkMctHome1A";
            this.checkMctHome1A.Size = new System.Drawing.Size(70, 17);
            this.checkMctHome1A.TabIndex = 84;
            this.checkMctHome1A.Text = "Home 1A";
            this.checkMctHome1A.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctHome1A.UseVisualStyleBackColor = true;
            // 
            // checkMctStow1A
            // 
            this.checkMctStow1A.AutoSize = true;
            this.checkMctStow1A.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkMctStow1A.Location = new System.Drawing.Point(215, 291);
            this.checkMctStow1A.Name = "checkMctStow1A";
            this.checkMctStow1A.Size = new System.Drawing.Size(66, 17);
            this.checkMctStow1A.TabIndex = 83;
            this.checkMctStow1A.Text = "Stow 1A";
            this.checkMctStow1A.UseVisualStyleBackColor = true;
            // 
            // cbMctTrack1
            // 
            this.cbMctTrack1.FormattingEnabled = true;
            this.cbMctTrack1.Location = new System.Drawing.Point(279, 44);
            this.cbMctTrack1.Name = "cbMctTrack1";
            this.cbMctTrack1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cbMctTrack1.Size = new System.Drawing.Size(160, 22);
            this.cbMctTrack1.TabIndex = 82;
            this.cbMctTrack1.SelectedIndexChanged += new System.EventHandler(this.cbMctTrack1_SelectedIndexChanged);
            // 
            // txtMctSteps1A
            // 
            this.txtMctSteps1A.Location = new System.Drawing.Point(304, 241);
            this.txtMctSteps1A.Name = "txtMctSteps1A";
            this.txtMctSteps1A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctSteps1A.Size = new System.Drawing.Size(46, 20);
            this.txtMctSteps1A.TabIndex = 81;
            this.txtMctSteps1A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Location = new System.Drawing.Point(246, 244);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(52, 14);
            this.label65.TabIndex = 80;
            this.label65.Text = "Steps 1A";
            // 
            // txtMctTarg1A
            // 
            this.txtMctTarg1A.Location = new System.Drawing.Point(304, 265);
            this.txtMctTarg1A.Name = "txtMctTarg1A";
            this.txtMctTarg1A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctTarg1A.Size = new System.Drawing.Size(46, 20);
            this.txtMctTarg1A.TabIndex = 79;
            this.txtMctTarg1A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Location = new System.Drawing.Point(246, 268);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(54, 14);
            this.label64.TabIndex = 78;
            this.label64.Text = "Target 1A";
            // 
            // txtMctSteps1B
            // 
            this.txtMctSteps1B.Location = new System.Drawing.Point(304, 97);
            this.txtMctSteps1B.Name = "txtMctSteps1B";
            this.txtMctSteps1B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctSteps1B.Size = new System.Drawing.Size(46, 20);
            this.txtMctSteps1B.TabIndex = 73;
            this.txtMctSteps1B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Location = new System.Drawing.Point(246, 100);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(51, 14);
            this.label59.TabIndex = 72;
            this.label59.Text = "Steps 1B";
            // 
            // txtMctTarg1B
            // 
            this.txtMctTarg1B.Location = new System.Drawing.Point(304, 121);
            this.txtMctTarg1B.Name = "txtMctTarg1B";
            this.txtMctTarg1B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctTarg1B.Size = new System.Drawing.Size(46, 20);
            this.txtMctTarg1B.TabIndex = 71;
            this.txtMctTarg1B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Location = new System.Drawing.Point(246, 124);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(53, 14);
            this.label58.TabIndex = 70;
            this.label58.Text = "Target 1B";
            // 
            // txtMctMan1
            // 
            this.txtMctMan1.Location = new System.Drawing.Point(393, 18);
            this.txtMctMan1.Name = "txtMctMan1";
            this.txtMctMan1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctMan1.Size = new System.Drawing.Size(46, 20);
            this.txtMctMan1.TabIndex = 69;
            this.txtMctMan1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd1BR
            // 
            this.txtMctRtd1BR.Location = new System.Drawing.Point(331, 169);
            this.txtMctRtd1BR.Name = "txtMctRtd1BR";
            this.txtMctRtd1BR.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd1BR.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd1BR.TabIndex = 68;
            this.txtMctRtd1BR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd1BL
            // 
            this.txtMctRtd1BL.Location = new System.Drawing.Point(279, 169);
            this.txtMctRtd1BL.Name = "txtMctRtd1BL";
            this.txtMctRtd1BL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd1BL.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd1BL.TabIndex = 67;
            this.txtMctRtd1BL.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctPosn1B
            // 
            this.txtMctPosn1B.Location = new System.Drawing.Point(304, 145);
            this.txtMctPosn1B.Name = "txtMctPosn1B";
            this.txtMctPosn1B.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctPosn1B.Size = new System.Drawing.Size(46, 20);
            this.txtMctPosn1B.TabIndex = 66;
            this.txtMctPosn1B.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd1AR
            // 
            this.txtMctRtd1AR.Location = new System.Drawing.Point(331, 193);
            this.txtMctRtd1AR.Name = "txtMctRtd1AR";
            this.txtMctRtd1AR.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd1AR.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd1AR.TabIndex = 65;
            this.txtMctRtd1AR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctRtd1AL
            // 
            this.txtMctRtd1AL.Location = new System.Drawing.Point(279, 193);
            this.txtMctRtd1AL.Name = "txtMctRtd1AL";
            this.txtMctRtd1AL.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctRtd1AL.Size = new System.Drawing.Size(46, 20);
            this.txtMctRtd1AL.TabIndex = 64;
            this.txtMctRtd1AL.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtMctPosn1A
            // 
            this.txtMctPosn1A.Location = new System.Drawing.Point(304, 217);
            this.txtMctPosn1A.Name = "txtMctPosn1A";
            this.txtMctPosn1A.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtMctPosn1A.Size = new System.Drawing.Size(46, 20);
            this.txtMctPosn1A.TabIndex = 63;
            this.txtMctPosn1A.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(356, 21);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(36, 14);
            this.label50.TabIndex = 53;
            this.label50.Text = "MAN1";
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(383, 172);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(50, 14);
            this.label51.TabIndex = 52;
            this.label51.Text = "RTD 1BR";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Location = new System.Drawing.Point(221, 172);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(49, 14);
            this.label52.TabIndex = 51;
            this.label52.Text = "RTD 1BL";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(246, 148);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(47, 14);
            this.label53.TabIndex = 50;
            this.label53.Text = "Posn 1B";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(383, 196);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(51, 14);
            this.label54.TabIndex = 49;
            this.label54.Text = "RTD 1AR";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(221, 196);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(50, 14);
            this.label55.TabIndex = 48;
            this.label55.Text = "RTD 1AL";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(246, 220);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(48, 14);
            this.label56.TabIndex = 47;
            this.label56.Text = "Posn 1A";
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(235, 47);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(43, 14);
            this.label57.TabIndex = 46;
            this.label57.Text = "Track 1";
            // 
            // tabDataLog
            // 
            this.tabDataLog.Controls.Add(this.cbLogNightMode);
            this.tabDataLog.Controls.Add(this.label82);
            this.tabDataLog.Controls.Add(this.checkRecSFCTemp);
            this.tabDataLog.Controls.Add(this.checkRecSensors);
            this.tabDataLog.Controls.Add(this.btnStartStop);
            this.tabDataLog.Controls.Add(this.checkRecSys30);
            this.tabDataLog.Controls.Add(this.checkDNI);
            this.tabDataLog.Controls.Add(this.checkRecFCE);
            this.tabDataLog.Controls.Add(this.label7);
            this.tabDataLog.Controls.Add(this.label6);
            this.tabDataLog.Controls.Add(this.checkRecTrackState);
            this.tabDataLog.Controls.Add(this.checkRecHumidity);
            this.tabDataLog.Controls.Add(this.checkRecPCBTemp);
            this.tabDataLog.Controls.Add(this.checkRecManRTD);
            this.tabDataLog.Controls.Add(this.checkRecTrackRTD);
            this.tabDataLog.Controls.Add(this.checkRecPosn);
            this.tabDataLog.Controls.Add(this.btnClrD);
            this.tabDataLog.Controls.Add(this.btnSetD);
            this.tabDataLog.Controls.Add(this.btnClrC);
            this.tabDataLog.Controls.Add(this.btnSetC);
            this.tabDataLog.Controls.Add(this.btnClrB);
            this.tabDataLog.Controls.Add(this.btnSetB);
            this.tabDataLog.Controls.Add(this.btnClrA);
            this.tabDataLog.Controls.Add(this.btnSetA);
            this.tabDataLog.Controls.Add(this.label5);
            this.tabDataLog.Controls.Add(this.label3);
            this.tabDataLog.Controls.Add(this.label2);
            this.tabDataLog.Controls.Add(this.label1);
            this.tabDataLog.Controls.Add(this.groupBox2);
            this.tabDataLog.Controls.Add(this.groupBox1);
            this.tabDataLog.Location = new System.Drawing.Point(4, 23);
            this.tabDataLog.Name = "tabDataLog";
            this.tabDataLog.Size = new System.Drawing.Size(760, 409);
            this.tabDataLog.TabIndex = 5;
            this.tabDataLog.Text = "Data Logging";
            this.tabDataLog.UseVisualStyleBackColor = true;
            // 
            // cbLogNightMode
            // 
            this.cbLogNightMode.AutoSize = true;
            this.cbLogNightMode.Location = new System.Drawing.Point(232, 358);
            this.cbLogNightMode.Name = "cbLogNightMode";
            this.cbLogNightMode.Size = new System.Drawing.Size(114, 18);
            this.cbLogNightMode.TabIndex = 47;
            this.cbLogNightMode.Text = "Log MCT\'s at night";
            this.cbLogNightMode.UseVisualStyleBackColor = true;
            // 
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Location = new System.Drawing.Point(141, 35);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(14, 14);
            this.label82.TabIndex = 46;
            this.label82.Text = "D";
            // 
            // checkRecSFCTemp
            // 
            this.checkRecSFCTemp.AutoSize = true;
            this.checkRecSFCTemp.Checked = true;
            this.checkRecSFCTemp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecSFCTemp.Location = new System.Drawing.Point(480, 103);
            this.checkRecSFCTemp.Name = "checkRecSFCTemp";
            this.checkRecSFCTemp.Size = new System.Drawing.Size(159, 17);
            this.checkRecSFCTemp.TabIndex = 45;
            this.checkRecSFCTemp.Text = "Record SFC Temp/Humidity";
            this.checkRecSFCTemp.UseVisualStyleBackColor = true;
            // 
            // checkRecSensors
            // 
            this.checkRecSensors.AutoSize = true;
            this.checkRecSensors.Checked = true;
            this.checkRecSensors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecSensors.Location = new System.Drawing.Point(232, 179);
            this.checkRecSensors.Name = "checkRecSensors";
            this.checkRecSensors.Size = new System.Drawing.Size(169, 17);
            this.checkRecSensors.TabIndex = 44;
            this.checkRecSensors.Text = "Record End of Travel Sensors";
            this.checkRecSensors.UseVisualStyleBackColor = true;
            // 
            // btnStartStop
            // 
            this.btnStartStop.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartStop.Location = new System.Drawing.Point(386, 350);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(76, 26);
            this.btnStartStop.TabIndex = 43;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // checkRecSys30
            // 
            this.checkRecSys30.AutoSize = true;
            this.checkRecSys30.Checked = true;
            this.checkRecSys30.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecSys30.Location = new System.Drawing.Point(480, 31);
            this.checkRecSys30.Name = "checkRecSys30";
            this.checkRecSys30.Size = new System.Drawing.Size(93, 17);
            this.checkRecSys30.TabIndex = 42;
            this.checkRecSys30.Text = "Record Sys30";
            this.checkRecSys30.UseVisualStyleBackColor = true;
            // 
            // checkDNI
            // 
            this.checkDNI.AutoSize = true;
            this.checkDNI.Checked = true;
            this.checkDNI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDNI.Location = new System.Drawing.Point(480, 79);
            this.checkDNI.Name = "checkDNI";
            this.checkDNI.Size = new System.Drawing.Size(83, 17);
            this.checkDNI.TabIndex = 38;
            this.checkDNI.Text = "Record DNI";
            this.checkDNI.UseVisualStyleBackColor = true;
            // 
            // checkRecFCE
            // 
            this.checkRecFCE.AutoSize = true;
            this.checkRecFCE.Checked = true;
            this.checkRecFCE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecFCE.Location = new System.Drawing.Point(480, 55);
            this.checkRecFCE.Name = "checkRecFCE";
            this.checkRecFCE.Size = new System.Drawing.Size(103, 17);
            this.checkRecFCE.TabIndex = 37;
            this.checkRecFCE.Text = "Record FCE I/O";
            this.checkRecFCE.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(476, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 19);
            this.label7.TabIndex = 36;
            this.label7.Text = "SFC Data Log";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(229, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 19);
            this.label6.TabIndex = 35;
            this.label6.Text = "MCT Data Log";
            // 
            // checkRecTrackState
            // 
            this.checkRecTrackState.AutoSize = true;
            this.checkRecTrackState.Checked = true;
            this.checkRecTrackState.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecTrackState.Location = new System.Drawing.Point(232, 35);
            this.checkRecTrackState.Name = "checkRecTrackState";
            this.checkRecTrackState.Size = new System.Drawing.Size(139, 17);
            this.checkRecTrackState.TabIndex = 34;
            this.checkRecTrackState.Text = "Record Tracking States";
            this.checkRecTrackState.UseVisualStyleBackColor = true;
            // 
            // checkRecHumidity
            // 
            this.checkRecHumidity.AutoSize = true;
            this.checkRecHumidity.Checked = true;
            this.checkRecHumidity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecHumidity.Location = new System.Drawing.Point(232, 155);
            this.checkRecHumidity.Name = "checkRecHumidity";
            this.checkRecHumidity.Size = new System.Drawing.Size(104, 17);
            this.checkRecHumidity.TabIndex = 33;
            this.checkRecHumidity.Text = "Record Humidity";
            this.checkRecHumidity.UseVisualStyleBackColor = true;
            // 
            // checkRecPCBTemp
            // 
            this.checkRecPCBTemp.AutoSize = true;
            this.checkRecPCBTemp.Checked = true;
            this.checkRecPCBTemp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecPCBTemp.Location = new System.Drawing.Point(232, 131);
            this.checkRecPCBTemp.Name = "checkRecPCBTemp";
            this.checkRecPCBTemp.Size = new System.Drawing.Size(120, 17);
            this.checkRecPCBTemp.TabIndex = 32;
            this.checkRecPCBTemp.Text = "Record PCB Temps";
            this.checkRecPCBTemp.UseVisualStyleBackColor = true;
            // 
            // checkRecManRTD
            // 
            this.checkRecManRTD.AutoSize = true;
            this.checkRecManRTD.Checked = true;
            this.checkRecManRTD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecManRTD.Location = new System.Drawing.Point(232, 107);
            this.checkRecManRTD.Name = "checkRecManRTD";
            this.checkRecManRTD.Size = new System.Drawing.Size(175, 17);
            this.checkRecManRTD.TabIndex = 31;
            this.checkRecManRTD.Text = "Record Record Manifold RTD\'s";
            this.checkRecManRTD.UseVisualStyleBackColor = true;
            // 
            // checkRecTrackRTD
            // 
            this.checkRecTrackRTD.AutoSize = true;
            this.checkRecTrackRTD.Checked = true;
            this.checkRecTrackRTD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecTrackRTD.Location = new System.Drawing.Point(232, 83);
            this.checkRecTrackRTD.Name = "checkRecTrackRTD";
            this.checkRecTrackRTD.Size = new System.Drawing.Size(139, 17);
            this.checkRecTrackRTD.TabIndex = 30;
            this.checkRecTrackRTD.Text = "Record Tracking RTD\'s";
            this.checkRecTrackRTD.UseVisualStyleBackColor = true;
            // 
            // checkRecPosn
            // 
            this.checkRecPosn.AutoSize = true;
            this.checkRecPosn.Checked = true;
            this.checkRecPosn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRecPosn.Location = new System.Drawing.Point(232, 59);
            this.checkRecPosn.Name = "checkRecPosn";
            this.checkRecPosn.Size = new System.Drawing.Size(106, 17);
            this.checkRecPosn.TabIndex = 29;
            this.checkRecPosn.Text = "Record Positions";
            this.checkRecPosn.UseVisualStyleBackColor = true;
            // 
            // btnClrD
            // 
            this.btnClrD.Location = new System.Drawing.Point(144, 321);
            this.btnClrD.Name = "btnClrD";
            this.btnClrD.Size = new System.Drawing.Size(36, 23);
            this.btnClrD.TabIndex = 28;
            this.btnClrD.Text = "Clr";
            this.btnClrD.UseVisualStyleBackColor = true;
            this.btnClrD.Click += new System.EventHandler(this.btnClrD_Click);
            // 
            // btnSetD
            // 
            this.btnSetD.Location = new System.Drawing.Point(144, 292);
            this.btnSetD.Name = "btnSetD";
            this.btnSetD.Size = new System.Drawing.Size(36, 23);
            this.btnSetD.TabIndex = 27;
            this.btnSetD.Text = "Set";
            this.btnSetD.UseVisualStyleBackColor = true;
            this.btnSetD.Click += new System.EventHandler(this.btnSetD_Click);
            // 
            // btnClrC
            // 
            this.btnClrC.Location = new System.Drawing.Point(102, 321);
            this.btnClrC.Name = "btnClrC";
            this.btnClrC.Size = new System.Drawing.Size(36, 23);
            this.btnClrC.TabIndex = 26;
            this.btnClrC.Text = "Clr";
            this.btnClrC.UseVisualStyleBackColor = true;
            this.btnClrC.Click += new System.EventHandler(this.btnClrC_Click);
            // 
            // btnSetC
            // 
            this.btnSetC.Location = new System.Drawing.Point(102, 292);
            this.btnSetC.Name = "btnSetC";
            this.btnSetC.Size = new System.Drawing.Size(36, 23);
            this.btnSetC.TabIndex = 25;
            this.btnSetC.Text = "Set";
            this.btnSetC.UseVisualStyleBackColor = true;
            this.btnSetC.Click += new System.EventHandler(this.btnSetC_Click);
            // 
            // btnClrB
            // 
            this.btnClrB.Location = new System.Drawing.Point(60, 321);
            this.btnClrB.Name = "btnClrB";
            this.btnClrB.Size = new System.Drawing.Size(36, 23);
            this.btnClrB.TabIndex = 24;
            this.btnClrB.Text = "Clr";
            this.btnClrB.UseVisualStyleBackColor = true;
            this.btnClrB.Click += new System.EventHandler(this.btnClrB_Click);
            // 
            // btnSetB
            // 
            this.btnSetB.Location = new System.Drawing.Point(60, 292);
            this.btnSetB.Name = "btnSetB";
            this.btnSetB.Size = new System.Drawing.Size(36, 23);
            this.btnSetB.TabIndex = 23;
            this.btnSetB.Text = "Set";
            this.btnSetB.UseVisualStyleBackColor = true;
            this.btnSetB.Click += new System.EventHandler(this.btnSetB_Click);
            // 
            // btnClrA
            // 
            this.btnClrA.Location = new System.Drawing.Point(18, 321);
            this.btnClrA.Name = "btnClrA";
            this.btnClrA.Size = new System.Drawing.Size(36, 23);
            this.btnClrA.TabIndex = 22;
            this.btnClrA.Text = "Clr";
            this.btnClrA.UseVisualStyleBackColor = true;
            this.btnClrA.Click += new System.EventHandler(this.btnClrA_Click);
            // 
            // btnSetA
            // 
            this.btnSetA.Location = new System.Drawing.Point(18, 292);
            this.btnSetA.Name = "btnSetA";
            this.btnSetA.Size = new System.Drawing.Size(36, 23);
            this.btnSetA.TabIndex = 21;
            this.btnSetA.Text = "Set";
            this.btnSetA.UseVisualStyleBackColor = true;
            this.btnSetA.Click += new System.EventHandler(this.btnSetA_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(99, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 14);
            this.label5.TabIndex = 19;
            this.label5.Text = "C";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 14);
            this.label3.TabIndex = 18;
            this.label3.Text = "B";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 14);
            this.label2.TabIndex = 17;
            this.label2.Text = "A";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 19);
            this.label1.TabIndex = 16;
            this.label1.Text = "Select MCT\'s by String";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.numSfcLogRate);
            this.groupBox2.Controls.Add(this.radioSfcSeconds);
            this.groupBox2.Controls.Add(this.radioSfcMinutes);
            this.groupBox2.Location = new System.Drawing.Point(480, 261);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(138, 67);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SFC Log Rate";
            // 
            // numSfcLogRate
            // 
            this.numSfcLogRate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numSfcLogRate.Location = new System.Drawing.Point(6, 25);
            this.numSfcLogRate.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numSfcLogRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSfcLogRate.Name = "numSfcLogRate";
            this.numSfcLogRate.Size = new System.Drawing.Size(50, 26);
            this.numSfcLogRate.TabIndex = 6;
            this.numSfcLogRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numSfcLogRate.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // radioSfcSeconds
            // 
            this.radioSfcSeconds.AutoSize = true;
            this.radioSfcSeconds.Checked = true;
            this.radioSfcSeconds.Location = new System.Drawing.Point(62, 19);
            this.radioSfcSeconds.Name = "radioSfcSeconds";
            this.radioSfcSeconds.Size = new System.Drawing.Size(65, 17);
            this.radioSfcSeconds.TabIndex = 7;
            this.radioSfcSeconds.TabStop = true;
            this.radioSfcSeconds.Text = "seconds";
            this.radioSfcSeconds.UseVisualStyleBackColor = true;
            // 
            // radioSfcMinutes
            // 
            this.radioSfcMinutes.AutoSize = true;
            this.radioSfcMinutes.Location = new System.Drawing.Point(62, 43);
            this.radioSfcMinutes.Name = "radioSfcMinutes";
            this.radioSfcMinutes.Size = new System.Drawing.Size(61, 17);
            this.radioSfcMinutes.TabIndex = 8;
            this.radioSfcMinutes.Text = "minutes";
            this.radioSfcMinutes.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numMctLogRate);
            this.groupBox1.Controls.Add(this.radioMctSeconds);
            this.groupBox1.Controls.Add(this.radioMctMinutes);
            this.groupBox1.Location = new System.Drawing.Point(233, 261);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(138, 67);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mct Log Rate";
            // 
            // numMctLogRate
            // 
            this.numMctLogRate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMctLogRate.Location = new System.Drawing.Point(6, 25);
            this.numMctLogRate.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numMctLogRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMctLogRate.Name = "numMctLogRate";
            this.numMctLogRate.Size = new System.Drawing.Size(50, 26);
            this.numMctLogRate.TabIndex = 2;
            this.numMctLogRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numMctLogRate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // radioMctSeconds
            // 
            this.radioMctSeconds.AutoSize = true;
            this.radioMctSeconds.Checked = true;
            this.radioMctSeconds.Location = new System.Drawing.Point(62, 19);
            this.radioMctSeconds.Name = "radioMctSeconds";
            this.radioMctSeconds.Size = new System.Drawing.Size(65, 17);
            this.radioMctSeconds.TabIndex = 4;
            this.radioMctSeconds.TabStop = true;
            this.radioMctSeconds.Text = "seconds";
            this.radioMctSeconds.UseVisualStyleBackColor = true;
            // 
            // radioMctMinutes
            // 
            this.radioMctMinutes.AutoSize = true;
            this.radioMctMinutes.Location = new System.Drawing.Point(62, 43);
            this.radioMctMinutes.Name = "radioMctMinutes";
            this.radioMctMinutes.Size = new System.Drawing.Size(61, 17);
            this.radioMctMinutes.TabIndex = 5;
            this.radioMctMinutes.Text = "minutes";
            this.radioMctMinutes.UseVisualStyleBackColor = true;
            // 
            // tabTesting
            // 
            this.tabTesting.Controls.Add(this.resetAtMidnightCheckbox);
            this.tabTesting.Controls.Add(this.btnTestSoftReset);
            this.tabTesting.Controls.Add(this.btnTestField);
            this.tabTesting.Controls.Add(this.btnTestString);
            this.tabTesting.Controls.Add(this.btnTestMct485);
            this.tabTesting.Controls.Add(this.btnRamDump);
            this.tabTesting.Location = new System.Drawing.Point(4, 23);
            this.tabTesting.Name = "tabTesting";
            this.tabTesting.Size = new System.Drawing.Size(760, 409);
            this.tabTesting.TabIndex = 6;
            this.tabTesting.Text = "Testing";
            this.tabTesting.UseVisualStyleBackColor = true;
            // 
            // resetAtMidnightCheckbox
            // 
            this.resetAtMidnightCheckbox.AutoSize = true;
            this.resetAtMidnightCheckbox.Location = new System.Drawing.Point(107, 131);
            this.resetAtMidnightCheckbox.Name = "resetAtMidnightCheckbox";
            this.resetAtMidnightCheckbox.Size = new System.Drawing.Size(123, 18);
            this.resetAtMidnightCheckbox.TabIndex = 5;
            this.resetAtMidnightCheckbox.Text = "Reset Each Midnight";
            this.resetAtMidnightCheckbox.UseVisualStyleBackColor = true;
            this.resetAtMidnightCheckbox.Click += new System.EventHandler(this.resetAtMidnightCheckbox_CheckedChanged);
            // 
            // btnTestSoftReset
            // 
            this.btnTestSoftReset.Location = new System.Drawing.Point(15, 128);
            this.btnTestSoftReset.Name = "btnTestSoftReset";
            this.btnTestSoftReset.Size = new System.Drawing.Size(75, 23);
            this.btnTestSoftReset.TabIndex = 4;
            this.btnTestSoftReset.Text = "Soft Reset";
            this.btnTestSoftReset.UseVisualStyleBackColor = true;
            this.btnTestSoftReset.Click += new System.EventHandler(this.btnTestSoftReset_Click);
            // 
            // btnTestField
            // 
            this.btnTestField.Location = new System.Drawing.Point(15, 99);
            this.btnTestField.Name = "btnTestField";
            this.btnTestField.Size = new System.Drawing.Size(75, 23);
            this.btnTestField.TabIndex = 3;
            this.btnTestField.Text = "FieldInit";
            this.btnTestField.UseVisualStyleBackColor = true;
            this.btnTestField.Click += new System.EventHandler(this.btnTestField_Click);
            // 
            // btnTestString
            // 
            this.btnTestString.Location = new System.Drawing.Point(15, 70);
            this.btnTestString.Name = "btnTestString";
            this.btnTestString.Size = new System.Drawing.Size(75, 23);
            this.btnTestString.TabIndex = 2;
            this.btnTestString.Text = "StringInit";
            this.btnTestString.UseVisualStyleBackColor = true;
            this.btnTestString.Click += new System.EventHandler(this.btnTestString_Click);
            // 
            // btnTestMct485
            // 
            this.btnTestMct485.Location = new System.Drawing.Point(15, 41);
            this.btnTestMct485.Name = "btnTestMct485";
            this.btnTestMct485.Size = new System.Drawing.Size(75, 23);
            this.btnTestMct485.TabIndex = 1;
            this.btnTestMct485.Text = "Mct485Init";
            this.btnTestMct485.UseVisualStyleBackColor = true;
            this.btnTestMct485.Click += new System.EventHandler(this.btnTestMct485_Click);
            // 
            // btnRamDump
            // 
            this.btnRamDump.Location = new System.Drawing.Point(15, 12);
            this.btnRamDump.Name = "btnRamDump";
            this.btnRamDump.Size = new System.Drawing.Size(75, 23);
            this.btnRamDump.TabIndex = 0;
            this.btnRamDump.Text = "RAM Dump";
            this.btnRamDump.UseVisualStyleBackColor = true;
            this.btnRamDump.Click += new System.EventHandler(this.btnRamDump_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(141, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 14);
            this.label4.TabIndex = 20;
            this.label4.Text = "D";
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.ItemHeight = 14;
            this.lstResults.Location = new System.Drawing.Point(283, 442);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(493, 116);
            this.lstResults.TabIndex = 1;
            // 
            // btnFieldTestOnSun
            // 
            this.btnFieldTestOnSun.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFieldTestOnSun.Location = new System.Drawing.Point(183, 512);
            this.btnFieldTestOnSun.Name = "btnFieldTestOnSun";
            this.btnFieldTestOnSun.Size = new System.Drawing.Size(93, 23);
            this.btnFieldTestOnSun.TabIndex = 55;
            this.btnFieldTestOnSun.Text = "Test On Sun";
            this.btnFieldTestOnSun.UseVisualStyleBackColor = true;
            this.btnFieldTestOnSun.Click += new System.EventHandler(this.btnFieldTestOnSun_Click);
            // 
            // btnFieldShutdown
            // 
            this.btnFieldShutdown.Location = new System.Drawing.Point(85, 512);
            this.btnFieldShutdown.Name = "btnFieldShutdown";
            this.btnFieldShutdown.Size = new System.Drawing.Size(92, 23);
            this.btnFieldShutdown.TabIndex = 54;
            this.btnFieldShutdown.Text = "End of Day";
            this.btnFieldShutdown.UseVisualStyleBackColor = true;
            this.btnFieldShutdown.Click += new System.EventHandler(this.btnFieldShutdown_Click);
            // 
            // btnFieldTest
            // 
            this.btnFieldTest.Location = new System.Drawing.Point(183, 483);
            this.btnFieldTest.Name = "btnFieldTest";
            this.btnFieldTest.Size = new System.Drawing.Size(93, 23);
            this.btnFieldTest.TabIndex = 53;
            this.btnFieldTest.Text = "Test On";
            this.btnFieldTest.UseVisualStyleBackColor = true;
            this.btnFieldTest.Click += new System.EventHandler(this.btnFieldTest_Click);
            // 
            // btnFieldOff
            // 
            this.btnFieldOff.Location = new System.Drawing.Point(84, 483);
            this.btnFieldOff.Name = "btnFieldOff";
            this.btnFieldOff.Size = new System.Drawing.Size(93, 23);
            this.btnFieldOff.TabIndex = 52;
            this.btnFieldOff.Text = "Field OFF";
            this.btnFieldOff.UseVisualStyleBackColor = true;
            this.btnFieldOff.Click += new System.EventHandler(this.btnFieldOff_Click);
            // 
            // txtFieldState
            // 
            this.txtFieldState.Location = new System.Drawing.Point(183, 457);
            this.txtFieldState.Name = "txtFieldState";
            this.txtFieldState.Size = new System.Drawing.Size(93, 20);
            this.txtFieldState.TabIndex = 51;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(183, 440);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(57, 14);
            this.label20.TabIndex = 50;
            this.label20.Text = "Field State";
            // 
            // txtStringState
            // 
            this.txtStringState.Location = new System.Drawing.Point(84, 457);
            this.txtStringState.Name = "txtStringState";
            this.txtStringState.Size = new System.Drawing.Size(93, 20);
            this.txtStringState.TabIndex = 49;
            // 
            // radioStringD
            // 
            this.radioStringD.AutoSize = true;
            this.radioStringD.Location = new System.Drawing.Point(16, 526);
            this.radioStringD.Name = "radioStringD";
            this.radioStringD.Size = new System.Drawing.Size(63, 18);
            this.radioStringD.TabIndex = 3;
            this.radioStringD.Text = "String D";
            this.radioStringD.UseVisualStyleBackColor = true;
            this.radioStringD.CheckedChanged += new System.EventHandler(this.radioStringD_CheckedChanged);
            // 
            // radioStringC
            // 
            this.radioStringC.AutoSize = true;
            this.radioStringC.Location = new System.Drawing.Point(16, 502);
            this.radioStringC.Name = "radioStringC";
            this.radioStringC.Size = new System.Drawing.Size(63, 18);
            this.radioStringC.TabIndex = 2;
            this.radioStringC.Text = "String C";
            this.radioStringC.UseVisualStyleBackColor = true;
            this.radioStringC.CheckedChanged += new System.EventHandler(this.radioStringC_CheckedChanged);
            // 
            // radioStringB
            // 
            this.radioStringB.AutoSize = true;
            this.radioStringB.Location = new System.Drawing.Point(16, 478);
            this.radioStringB.Name = "radioStringB";
            this.radioStringB.Size = new System.Drawing.Size(63, 18);
            this.radioStringB.TabIndex = 1;
            this.radioStringB.Text = "String B";
            this.radioStringB.UseVisualStyleBackColor = true;
            this.radioStringB.CheckedChanged += new System.EventHandler(this.radioStringB_CheckedChanged);
            // 
            // radioStringA
            // 
            this.radioStringA.AutoSize = true;
            this.radioStringA.Checked = true;
            this.radioStringA.Location = new System.Drawing.Point(16, 454);
            this.radioStringA.Name = "radioStringA";
            this.radioStringA.Size = new System.Drawing.Size(63, 18);
            this.radioStringA.TabIndex = 0;
            this.radioStringA.TabStop = true;
            this.radioStringA.Text = "String A";
            this.radioStringA.UseVisualStyleBackColor = true;
            this.radioStringA.CheckedChanged += new System.EventHandler(this.radioStringA_CheckedChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label101
            // 
            this.label101.Location = new System.Drawing.Point(0, 0);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(100, 23);
            this.label101.TabIndex = 0;
            // 
            // label102
            // 
            this.label102.Location = new System.Drawing.Point(0, 0);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(100, 23);
            this.label102.TabIndex = 0;
            // 
            // label103
            // 
            this.label103.Location = new System.Drawing.Point(0, 0);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(100, 23);
            this.label103.TabIndex = 0;
            // 
            // label104
            // 
            this.label104.Location = new System.Drawing.Point(0, 0);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(100, 23);
            this.label104.TabIndex = 0;
            // 
            // label105
            // 
            this.label105.Location = new System.Drawing.Point(0, 0);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(100, 23);
            this.label105.TabIndex = 0;
            // 
            // label106
            // 
            this.label106.Location = new System.Drawing.Point(0, 0);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(100, 23);
            this.label106.TabIndex = 0;
            // 
            // label107
            // 
            this.label107.Location = new System.Drawing.Point(0, 0);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(100, 23);
            this.label107.TabIndex = 0;
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.Location = new System.Drawing.Point(82, 440);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(63, 14);
            this.label88.TabIndex = 56;
            this.label88.Text = "String State";
            // 
            // btnFieldTestOff
            // 
            this.btnFieldTestOff.Location = new System.Drawing.Point(85, 541);
            this.btnFieldTestOff.Name = "btnFieldTestOff";
            this.btnFieldTestOff.Size = new System.Drawing.Size(92, 23);
            this.btnFieldTestOff.TabIndex = 57;
            this.btnFieldTestOff.Text = "Test Off";
            this.btnFieldTestOff.UseVisualStyleBackColor = true;
            this.btnFieldTestOff.Click += new System.EventHandler(this.btnFieldTestOff_Click);
            // 
            // btnFieldTestShutdown
            // 
            this.btnFieldTestShutdown.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFieldTestShutdown.Location = new System.Drawing.Point(184, 541);
            this.btnFieldTestShutdown.Name = "btnFieldTestShutdown";
            this.btnFieldTestShutdown.Size = new System.Drawing.Size(92, 23);
            this.btnFieldTestShutdown.TabIndex = 58;
            this.btnFieldTestShutdown.Text = "Test End Day";
            this.btnFieldTestShutdown.UseVisualStyleBackColor = true;
            this.btnFieldTestShutdown.Click += new System.EventHandler(this.btnFieldTestShutdown_Click);
            // 
            // frmMain
            // 
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.btnFieldTestShutdown);
            this.Controls.Add(this.btnFieldTestOff);
            this.Controls.Add(this.label88);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.tabCtrl);
            this.Controls.Add(this.txtStringState);
            this.Controls.Add(this.txtFieldState);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.btnFieldTestOnSun);
            this.Controls.Add(this.radioStringA);
            this.Controls.Add(this.btnFieldShutdown);
            this.Controls.Add(this.radioStringB);
            this.Controls.Add(this.btnFieldTest);
            this.Controls.Add(this.btnFieldOff);
            this.Controls.Add(this.radioStringC);
            this.Controls.Add(this.radioStringD);
            this.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = new System.Drawing.Point(21, 28);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SFC USB          updated 7/12/2011";
            this.Closed += new System.EventHandler(this.frmMain_Closed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tabCtrl.ResumeLayout(false);
            this.tabSFC.ResumeLayout(false);
            this.tabSFC.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabString.ResumeLayout(false);
            this.tabString.PerformLayout();
            this.tabMCTfw.ResumeLayout(false);
            this.tabMCTfw.PerformLayout();
            this.tabMctParam.ResumeLayout(false);
            this.tabMctParam.PerformLayout();
            this.tabMctControl.ResumeLayout(false);
            this.tabMctControl.PerformLayout();
            this.tabDataLog.ResumeLayout(false);
            this.tabDataLog.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSfcLogRate)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMctLogRate)).EndInit();
            this.tabTesting.ResumeLayout(false);
            this.tabTesting.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //  This GUID must match the GUID in the device's INF file.
        //  To create a GUID in Visual Studio, click Tools > Create GUID.

        private const String WINUSB_DEMO_GUID_STRING = "{58D07210-27C1-11DD-BD0B-0800200C9A66}";
        private const String SFC_USB_GUID_STRING = "{A36C9E24-2DE9-4CA6-8EA7-B8D7E0C1BDDB}";
        private IntPtr deviceNotificationHandle;
        private Boolean myDeviceDetected = false;
        private DeviceManagement myDeviceManagement = new DeviceManagement();
        private String myDevicePathName;
        private WinUsbDevice myWinUsbDevice = new WinUsbDevice();

        internal frmMain frmMy;

        ///  <summary>
        ///  Define a class of delegates with the same parameters as 
        ///  WinUsbDevice.ReadViaBulkTransfer and WinUsbDevice.ReadViaInterruptTransfer.
        ///  Used for asynchronous reads from the device.
        ///  </summary>

        private delegate void ReadFromDeviceDelegate
            (Byte pipeID,
            UInt32 bufferLength,
            ref Byte[] buffer,
            ref UInt32 lengthTransferred,
            ref Boolean success);

        ///  <summary>
        ///  Define a delegate with the same parameters as AccessForm.
        ///  Used in accessing the application's form from a different thread.
        ///  </summary>

        private delegate void MarshalToForm(String action, String textToAdd);

        ///  <summary>
        ///  Performs various application-specific functions that
        ///  involve accessing the application's form.
        ///  </summary>
        ///  
        ///  <param name="action"> A String that names the action to perform on the form. </param>
        ///  <param name="formText"> Text to display on the form or an empty String. </param>
        ///  
        /// <remarks>
        ///  In asynchronous calls to WinUsb_ReadPipe, the callback function 
        ///  uses this routine to access the application's form, which runs in 
        ///  a different thread.
        /// </remarks>
        /// 
        private void AccessForm(String action, String formText)
        {
            try
            {
                //  Select an action to perform on the form:

                switch (action)
                {
                    case "AddItemToListBox":

                        if (lstResults.Items.Count >= 500)
                        {
                            lstResults.Items.Clear();
                            lstResults.Items.Add("List full, clearing");
                            lstResults.SelectedIndex = 0;
                        }
                        lstResults.Items.Add(formText);

                        break;
                    case "AddItemToTextBox":

                        //txtAddr0.SelectedText = formText + "\r\n";

                        break;
                    case "EnableCmdSendandReceiveViaBulkTransfers":

                        //btnWriteReg0.Enabled = true;
                        //btnWriteReg0.Focus();

                        break;
                    case "EnableCmdSendandReceiveViaInterruptTransfers":

                        //btnScript0.Enabled = true;
                        //btnScript0.Focus();

                        break;
                    case "ScrollToBottomOfListBox":

                        lstResults.SelectedIndex = lstResults.Items.Count - 1;

                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Display the device's speed in the list box.
        ///  </summary>
        ///  
        ///  <remarks>
        ///  Precondition: device speed was obtained by calling WinUsb_QueryDeviceInformation
        ///  and stored in myDevInfo. 
        ///  </remarks >

        private void DisplayDeviceSpeed()
        {
            String speed = "";

            myWinUsbDevice.QueryDeviceSpeed();

            try
            {
                switch (myWinUsbDevice.myDevInfo.devicespeed)
                {
                    case 1:
                        speed = "low";
                        break;
                    case 2:
                        speed = "full";
                        break;
                    case 3:
                        speed = "high";
                        break;
                }

                lstResults.Items.Add("Device speed = " + speed);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  If a device with the specified device interface GUID hasn't been previously detected,
        ///  look for it. If found, open a handle to the device.
        ///  </summary>
        ///  
        ///  <returns>
        ///  True if the device is detected, False if not detected.
        ///  </returns>

        private Boolean FindMyDevice()
        {
            Boolean deviceFound;
            String devicePathName = "";
            Boolean lastDevice;
            Boolean success;

            try
            {
                if (!(myDeviceDetected))
                {

                    //  Convert the device interface GUID String to a GUID object: 

                    System.Guid winUsbDemoGuid =
                        new System.Guid(SFC_USB_GUID_STRING);

                    // Fill an array with the device path names of all attached devices with matching GUIDs.

                    deviceFound = myDeviceManagement.FindDeviceFromGuid
                        (winUsbDemoGuid,
                        ref devicePathName);

                    if (deviceFound == true)
                    {
                        success = myWinUsbDevice.GetDeviceHandle(devicePathName);

                        if (success)
                        {
                            lstResults.Items.Add("Device detected:");

                            ScrollToBottomOfListBox();

                            myDeviceDetected = true;

                            // Save DevicePathName so OnDeviceChange() knows which name is my device.

                            myDevicePathName = devicePathName;
                        }
                        else
                        {
                            // There was a problem in retrieving the information.

                            myDeviceDetected = false;
                            myWinUsbDevice.CloseDeviceHandle();
                        }
                    }

                    if (myDeviceDetected)
                    {

                        // The device was detected.
                        // Register to receive notifications if the device is removed or attached.

                        success = myDeviceManagement.RegisterForDeviceNotifications
                            (myDevicePathName,
                            frmMy.Handle,
                            winUsbDemoGuid,
                            ref deviceNotificationHandle);

                        if (success)
                        {
                            myWinUsbDevice.InitializeDevice();

                            //Commented out due to unreliable response from WinUsb_QueryDeviceInformation.                            
                            //DisplayDeviceSpeed(); 
                        }
                    }
                    else
                    {
                        lstResults.Items.Add("Device not found.");
                        //btnWriteReg0.Enabled = true;
                        //btnScript0.Enabled = true;
                    }
                }
                else
                {
                    // don't fill up the list box! lstResults.Items.Add("Device detected.");
                }

                ScrollToBottomOfListBox();

                return myDeviceDetected;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Retrieves received data from a bulk endpoint.
        ///  This routine is called automatically when myWinUsbDevice.ReadViaBulkTransfer
        ///  returns. The routine calls several marshaling routines to access the main form.       
        ///  </summary>
        ///  
        ///  <param name="ar"> An object containing status information about the 
        ///  asynchronous operation.</param>
        ///  
        private void GetReceivedBulkData(IAsyncResult ar)
        {
            UInt32 bytesRead = 0;
            System.Text.ASCIIEncoding myEncoder = new System.Text.ASCIIEncoding();
            Byte[] receivedDataBuffer;
            String receivedtext = "";
            Boolean success = false;

            try
            {
                receivedDataBuffer = null;

                // Define a delegate using the IAsyncResult object.

                ReadFromDeviceDelegate deleg =
                    ((ReadFromDeviceDelegate)(ar.AsyncState));

                // Get the IAsyncResult object and the values of other paramaters that the
                // BeginInvoke method passed ByRef.

                deleg.EndInvoke
                    (ref receivedDataBuffer,
                    ref bytesRead,
                    ref success, ar);


                // Display the received data in the form's list box.
                //                bulkEnabled = true;
                if ((ar.IsCompleted && success))
                {
                    if (rxBuffer[ MCTCommand.USB_PACKET_PID] == txBuffer[ MCTCommand.USB_PACKET_PID])
                    {
                        cmdComplete = true;
                    }
                    else
                    {
                        cmdComplete = true;
                        cmdFailed = true;
                    }
                }
                else
                {
                    MyMarshalToForm("AddItemToListBox", "The attempt to read bulk data has failed.");
                    cmdFailed = true;
                    //myDeviceDetected = false;
                }

                MyMarshalToForm("ScrollToBottomOfListBox", "");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Retrieves received data from an interrupt endpoint.
        ///  This routine is called automatically when myWinUsbDevice.ReadViaInterruptTransfer
        ///  returns. The routine calls several marshaling routines to access the main form.
        ///  Converts received bytes to hex strings for displaying.
        ///  </summary>
        ///  
        ///  <param name="ar"> An object containing status information about the 
        ///  asynchronous operation.</param>

        private void GetReceivedInterruptData(IAsyncResult ar)
        {
            String byteValue = "";
            UInt32 bytesRead = 0;
            Byte[] receivedDataBuffer = null;
            Boolean success = false;

            try
            {
                // Define a delegate using the IAsyncResult object.

                ReadFromDeviceDelegate deleg = ((ReadFromDeviceDelegate)(ar.AsyncState));

                // Get the IAsyncResult object and the values of other paramaters that the
                // BeginInvoke method passed ByRef.

                deleg.EndInvoke
                    (ref receivedDataBuffer,
                    ref bytesRead,
                    ref success, ar);

                // Display the received data in the form's list box.

                if ((ar.IsCompleted && success))
                {
                    MyMarshalToForm("AddItemToListBox", "Data received via interrupt transfer:");

                    MyMarshalToForm("AddItemToListBox", " Received Data:");

                    for (Int32 i = 0; i <= receivedDataBuffer.GetUpperBound(0); i++)
                    {
                        // Convert the byte value to a 2-character hex String.

                        byteValue = String.Format("{0:X2} ", receivedDataBuffer[i]);

                        MyMarshalToForm("AddItemToListBox", " " + byteValue);
                    }
                }
                else
                {
                    MyMarshalToForm("AddItemToListBox", "The attempt to read interrupt data has failed.");
                    myDeviceDetected = false;
                }

                MyMarshalToForm("ScrollToBottomOfListBox", "");

                // Enable requesting another transfer.

                MyMarshalToForm("EnableCmdSendandReceiveViaInterruptTransfers", "");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Initializes elements on the form.
        ///  </summary>

        private void InitializeDisplay()
        {
            String byteValue;

            try
            {
                if (!((myWinUsbDevice.IsWindowsXpOrLater())))
                {
                    lstResults.Items.Add("The operating system is not Windows XP or later.");
                    lstResults.Items.Add("The WinUsb driver requires Windows XP or later.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        ///  <summary>
        ///  Enables accessing a form from another thread 
        ///  </summary>
        ///  
        ///  <param name="action"> A String that names the action to perform on the form. </param>
        ///  <param name="textToDisplay"> Text that the form displays or uses for 
        ///  another purpose. Actions that don't use text ignore this parameter. </param>

        private void MyMarshalToForm(String action, String textToDisplay)
        {
            object[] args = { action, textToDisplay };
            MarshalToForm MarshalToFormDelegate = null;

            try
            {
                //  The AccessForm routine contains the code that accesses the form.

                MarshalToFormDelegate = new MarshalToForm(AccessForm);

                //  Execute AccessForm, passing the parameters in args.

                base.Invoke(MarshalToFormDelegate, args);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Called when a WM_DEVICECHANGE message has arrived,
        ///  indicating that a device has been attached or removed.
        ///  </summary>
        ///  
        ///  <param name="m"> A message with information about the device. </param>

        internal void OnDeviceChange(Message m)
        {
            try
            {
                if ((m.WParam.ToInt32() == DeviceManagement.DBT_DEVICEARRIVAL))
                {

                    //  If WParam contains DBT_DEVICEARRIVAL, a device has been attached.
                    //  Find out if it's the device we're communicating with.

                    if (myDeviceManagement.DeviceNameMatch(m, myDevicePathName))
                    {
                        lstResults.Items.Add("My device attached.");
                    }

                }
                else if ((m.WParam.ToInt32() ==
                    DeviceManagement.DBT_DEVICEREMOVECOMPLETE))
                {
                    //  If WParam contains DBT_DEVICEREMOVAL, a device has been removed.
                    //  Find out if it's the device we're communicating with.

                    if (myDeviceManagement.DeviceNameMatch(m, myDevicePathName))
                    {

                        lstResults.Items.Add("My device removed.");

                        //  Set MyDeviceDetected False so on the next data-transfer attempt,
                        //  FindMyDevice() will be called to look for the device 
                        //  and get a new handle.

                        frmMy.myDeviceDetected = false;
                    }
                }

                ScrollToBottomOfListBox();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Initiates a read operation from a bulk IN endpoint.
        ///  To enable reading without blocking the main thread, uses an asynchronous delegate.
        ///  </summary>
        ///  
        ///  <remarks>
        ///  To enable reading more than 64 bytes (with device firmware support), increase bytesToRead.
        ///  </remarks> 

        private void ReadDataViaBulkTransfer()
        {

            IAsyncResult ar = null;
            //Byte[] buffer = new Byte[64];
            UInt32 bytesRead = 0;
            //UInt32 bytesToRead = System.Convert.ToUInt32(64);
            Boolean success = false;

            //  Define a delegate for the ReadViaBulkTransfer method of WinUsbDevice.

            ReadFromDeviceDelegate MyReadFromDeviceDelegate =
                new ReadFromDeviceDelegate(myWinUsbDevice.ReadViaBulkTransfer);

            try
            {
                //  The BeginInvoke method calls MyWinUsbDevice.ReadViaBulkTransfer to attempt 
                //  to read data. The method has the same parameters as ReadViaBulkTransfer,
                //  plus two additional parameters:
                //  GetReceivedBulkData is the callback routine that executes when 
                //  ReadViaBulkTransfer returns.
                //  MyReadFromDeviceDelegate is the asynchronous delegate object.

                ar = MyReadFromDeviceDelegate.BeginInvoke
                    (System.Convert.ToByte(myWinUsbDevice.myDevInfo.bulkInPipe),
                    64,
                    ref rxBuffer,
                    ref bytesRead,
                    ref success,
                    new AsyncCallback(GetReceivedBulkData),
                    MyReadFromDeviceDelegate);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Initiates a read operation from an interrupt IN endpoint.
        ///  To enable reading without blocking the main thread, uses an asynchronous delegate.
        ///  </summary>
        ///  
        ///  <remarks>
        ///  To enable reading more than 2 bytes (with device firmware support), increase bytesToRead.
        ///  </remarks>

        private void ReadDataViaInterruptTransfer()
        {
            IAsyncResult ar = null;
            Byte[] buffer = new Byte[2];
            UInt32 bytesRead = 0;
            UInt32 bytesToRead = System.Convert.ToUInt32(2);
            Boolean success = false;

            try
            {
                //  Define a delegate for the ReadViaInterruptTransfer method of WinUsbDevice.

                ReadFromDeviceDelegate MyReadFromDeviceDelegate = new ReadFromDeviceDelegate(myWinUsbDevice.ReadViaInterruptTransfer);

                //  The BeginInvoke method calls MyWinUsbDevice.ReadViaInterruptTransfer to attempt 
                //  to read data. The method has the same parameters as ReadViaInterruptTransfer,
                //  plus two additional parameters:
                //  GetReceivedInterruptData is the callback routine that executes when 
                //  ReadViaInterruptTransfer returns.
                //  MyReadFromDeviceDelegate is the asynchronous delegate object.

                ar = MyReadFromDeviceDelegate.BeginInvoke
                    (System.Convert.ToByte(myWinUsbDevice.myDevInfo.interruptInPipe),
                    bytesToRead,
                    ref buffer,
                    ref bytesRead,
                    ref success,
                    new AsyncCallback(GetReceivedInterruptData),
                    MyReadFromDeviceDelegate);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Scroll to the bottom of the list box and trim if needed.
        ///  </summary>

        private void ScrollToBottomOfListBox()
        {
            //try
            {
                lstResults.SelectedIndex = lstResults.Items.Count - 1;

                // If the list box is getting too large, trim its contents.

                if (lstResults.Items.Count > 1000)
                {
                    while(lstResults.Items.Count > 800)                    
                    {
                        lstResults.Items.RemoveAt(0); // remove first item, others will slide down.
                    }
                }
            }
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }

        ///  <summary>
        ///  Initiates sending data via a bulk transfer, then receiving data via a bulk transfer.
        ///  </summary>

        private void SendAndReceiveViaBulkTransfers()
        {
            try
            {
                Boolean success;
                String formText = "";
                System.Text.ASCIIEncoding myEncoder = new System.Text.ASCIIEncoding();

                // If the device hasn't been detected, was removed, or timed out on a previous attempt
                // to access it, look for the device.
                myDeviceDetected = FindMyDevice();

                if (myDeviceDetected)
                {
                    success = myWinUsbDevice.SendViaBulkTransfer
                        (ref txBuffer,
                        txBuffer[ MCTCommand.USB_PACKET_LEN]);

                    if (success)
                    {
                        formText = "Data sent via bulk transfer.";
                    }
                    else
                    {
                        formText = "Bulk OUT transfer failed.";
                    }

                    AccessForm("AddItemToListBox", formText);

                    ReadDataViaBulkTransfer();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        ///  <summary>
        ///  Perform actions that must execute when the program ends.
        ///  </summary>

        private void Shutdown()
        {
            try
            {
                myWinUsbDevice.CloseDeviceHandle();

                myDeviceManagement.StopReceivingDeviceNotifications
                    (deviceNotificationHandle);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Perform actions that must execute when the program starts.
        ///  </summary>

        private void Startup()
        {
            try
            {
                myWinUsbDevice = new WinUsbDevice();
                InitializeDisplay();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Calls a routine to initiate a Control Read transfer. (Data stage is device to host.)
        ///  </summary>

        private void cmdControlReadTransfer_Click(System.Object sender, System.EventArgs e)
        {
            //InitiateControlInTransfer();
        }


        ///  <summary>
        ///  Calls a routine to initiate a Control Write transfer. (Data stage is host to device.)
        ///  </summary>
        ///   
        private void cmdControlWriteTransfer_Click(System.Object sender, System.EventArgs e)
        {
            //InitiateControlOutTransfer();
        }

        ///  <summary>
        ///  Search for a specific device.
        ///  </summary>

        private void cmdFindDevice_Click(System.Object sender, System.EventArgs e)
        {
            try
            {
                FindMyDevice();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Calls a routine to exchange data via bulk transfers.
        ///  </summary>

        private void cmdSendAndReceiveViaBulkTransfers_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                // Don't allow another transfer request until this one completes.

                //btnWriteReg0.Enabled = false;

                SendAndReceiveViaBulkTransfers();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Calls a routine to exchange data via interrupt transfers.
        ///  </summary>
        ///  
        private void cmdSendAndReceiveViaInterruptTransfers_Click(System.Object sender, System.EventArgs e)
        {
            try
            {
                // Don't allow another transfer request until this one completes.

                //btnWriteReg0.Enabled = false;

                //SendAndReceiveViaInterruptTransfers();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Perform shutdown operations.
        ///  </summary>

        private void frmMain_Closed(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                Shutdown();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        ///  <summary>
        ///  Perform startup operations.
        ///  </summary>

        private void frmMain_Load(System.Object eventSender, System.EventArgs eventArgs)
        {
            try
            {
                frmMy = this;
                Startup();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        ///  <summary>
        ///  Overrides WndProc to enable checking for and handling
        ///  WM_DEVICECHANGE messages.
        ///  </summary>
        ///  
        ///  <param name="m"> A Windows message.
        ///  </param> 
        ///  
        protected override void WndProc(ref Message m)
        {
            try
            {
                // The OnDeviceChange routine processes WM_DEVICECHANGE messages.

                if (m.Msg == DeviceManagement.WM_DEVICECHANGE)
                {
                    OnDeviceChange(m);
                }
                // Let the base form process the message.

                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (updatingMctFlash)
            {
                return;
            }
            // don't let timer interrupt itself
            timer1.Enabled = false;

            if (countdownPollString > 0)
            {
                countdownPollString--;
            }
            else
            {
                countdownPollString = 100;
                flagGetString = true;
            }
            if (countdownPollLog > 0)
            {
                countdownPollLog--;
            }
            else
            {
                countdownPollLog = 1;
                if (btnStartStop.Text == "Stop")
                {
                    if (pollDataValid)
                    {
                        if (DateTime.Now.CompareTo(dateTimeMct) > 0)
                        {
                            // interval elapsed, record data
                            dateTimeMct = dateTimeMct.AddMilliseconds(intervalMct);
                            WriteMctFiles();
                        }
                        if (DateTime.Now.CompareTo(dateTimeSfc) > 0)
                        {
                            // interval elapsed, record data
                            dateTimeSfc = dateTimeSfc.AddMilliseconds(intervalSfc);
                            WriteSfcFiles();
                        }
                    }
                }
                PollNextItem();
            }
            if (flagRamDump)
            {
                flagRamDump = false;
                lstResults.Items.Add("Starting RAM dump");
                StreamWriter ramFile = File.AppendText("Files\\SFC Ram Dump.txt");
                string line;
                line = DateTime.Now.Year.ToString() + ',';
                line += DateTime.Now.Month.ToString() + ',';
                line += DateTime.Now.Day.ToString() + ' ';
                line += DateTime.Now.Hour.ToString() + ':';
                line += DateTime.Now.Minute.ToString();
                ramFile.WriteLine(line);
                ramFile.WriteLine(txtVersString.Text);
                for (uint addr = 0xA0000000; addr < 0xA0010000; addr += 16)
                {
                    txBuffer[ MCTCommand.USB_PACKET_DATA + 0] = (byte)(addr >> 24);
                    txBuffer[ MCTCommand.USB_PACKET_DATA + 1] = (byte)(addr >> 16);
                    txBuffer[ MCTCommand.USB_PACKET_DATA + 2] = (byte)(addr >> 8);
                    txBuffer[ MCTCommand.USB_PACKET_DATA + 3] = (byte)addr;
                    SendTxMessage(0, 0, MCTCommand.USB_RESP_MEMORY);
                    line = addr.ToString("X8") + ":";
                    for (int i = 0; i < 16; i++)
                    {
                        line += " " + rxBuffer[ MCTCommand.USB_PACKET_DATA + 5 + i].ToString("X2");
                    }
                    ramFile.WriteLine(line);
                }
                ramFile.Close();
                lstResults.Items.Add("Done with RAM dump");
            }
            else if (flagTestBtn)
            {
                flagTestBtn = false;
                SendTxMessage(0, 0, MCTCommand.USB_CMD_TEST);
                System.Threading.Thread.Sleep(2000);
            }
            //Reset SFC every midnight.
            else if (DateTime.Now > dateTimeResetSFC)
            {
                dateTimeResetSFC = DateTime.Today.AddDays(1); // set up for tomorrow.
              
                MyMarshalToForm("AddItemToListBox", "Test - Midnight Reset");
                // perform reset of the SFC. Then schedule a new one in 24 hours
                testBtnValue = 4; // magic number for a reset
                SendTxMessage(0, 0, MCTCommand.USB_CMD_TEST);
                System.Threading.Thread.Sleep(2000); // wait a couple seconds for it to take
            }
            else if (flagGetString)
            {
                flagGetString = false;
                // read String info
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_STRING);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_RESP_FIELD_STATE);
            }
            else if (flagSetFieldOff)
            {
                flagSetFieldOff = false;
                lstResults.Items.Add("Setting field to OFF");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_OFF;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
            }
            else if (flagSetFieldShutdown)
            {
                flagSetFieldShutdown = false;
                lstResults.Items.Add("Setting field to END OF DAY");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_END_OF_DAY;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
            }
            else if (flagSetFieldTest)
            {
                flagSetFieldTest = false;
                lstResults.Items.Add("Setting field to TEST");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
                //System.Threading.Thread.Sleep(1000);
                //                for (int i = 0; i < 4; i++)
                //                {
                //                    byte radio = WhichStringRadio();
                //                    radio++;
                //                    if (radio >= 4) radio = 0;
                //                    SetStringRadio(radio);
                //                    Application.DoEvents();
                StringWaitState(sender, e, STRING_ACTIVE);
                //                }
            }
            else if (flagSetFieldTestOnSun)
            {
                flagSetFieldTestOnSun = false;
                lstResults.Items.Add("Setting field to ON SUN");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST_ON_SUN;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
                for (int i = 0; i < 4; i++)
                {
                    byte radio = WhichStringRadio();
                    radio++;
                    if (radio >= 4) radio = 0;
                    SetStringRadio(radio);
                    Application.DoEvents();
                    StringWaitState(sender, e, STRING_ACTIVE);
                }
            }
            else if (flagSetFieldTestOff)
            {
                flagSetFieldTestOff = false;
                lstResults.Items.Add("Setting field to TEST OFF");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST_OFF;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
            }
            else if (flagSetFieldTestShutdown)
            {
                flagSetFieldTestShutdown = false;
                lstResults.Items.Add("Setting field to TEST END OF DAY");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST_END_OF_DAY;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
            }
            else if (flagSetFieldLogging)
            {
                flagSetFieldLogging = false;
                 // NightMode
                if (cbLogNightMode.Checked)
                {
                    // only set field to FIELD_LOGGING when night mode is checked
                    lstResults.Items.Add("Setting field to LOGGING MODE");
                    txBuffer[MCTCommand.USB_PACKET_DATA] = FIELD_LOGGING;
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
                }
            }
            else if (flagSetFieldLoggingOff)
            {
                flagSetFieldLoggingOff = false;
                lstResults.Items.Add("Setting field to LOGGING OFF");
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_LOGGING_OFF;
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
            }
            else if (flagHomeString)
            {
                flagHomeString = false;
                lstResults.Items.Add("Homing all MCT's on this string");
                if (tabCtrl.SelectedIndex == tabString.TabIndex)
                    pollMct = (byte)(Convert.ToInt16(txtNumMct.Text) + 0x40);
                else if (cbMctPollAddr.Items.Count > 0)
                    pollMct = (byte)(cbMctPollAddr.SelectedIndex + 1);
                else
                    return;
                // set posn for Mirror 1
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x03, 2395);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set posn for Mirror 2
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x13, 2395); //TODO figure out what this mystery is
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 1
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x03, 0); //TODO figure out what this mystery is
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 2
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x13, 0); //TODO figure out what this mystery is
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagStowString)
            {
                flagStowString = false;
                lstResults.Items.Add("Stowing all MCT's on this string");
                if (tabCtrl.SelectedIndex == tabString.TabIndex)
                    pollMct = (byte)(Convert.ToInt16(txtNumMct.Text) + 0x40);
                else if (cbMctPollAddr.Items.Count > 0)
                    pollMct = (byte)(cbMctPollAddr.SelectedIndex + 1);
                else
                    return;

                // set posn for Mirror 1
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x03, 0);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set posn for Mirror 2
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x13, 0);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 1
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x03, 2395); //TODO figure out what this mystery is
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 2
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x13, 2395); //TODO figure out what this mystery is
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctHome1)
            {
                flagMctHome1 = false;
             }
            else if (flagMctHome2)
            {
                flagMctHome2 = false;
                // set posn for Mirror 2
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x13, 2395);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 2
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x13, 0);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctStow1)
            {
                flagMctStow1 = false;
                // set posn for Mirror 1
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x03, 0);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 1
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x03, 2395);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

            }
            else if (flagMctStow2)
            {
                flagMctStow2 = false;
                // set posn for Mirror 2
                txBuffer = MCTCommand.POSN_WRITE(pollMct, txPid, 0x13, 0);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);

                // set target for Mirror 1
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x13, 2395);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctMoveSteps1)
            {
                flagMctMoveSteps1 = false;
                // set posn for Mirror 1
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x03, Convert.ToUInt16(txtMctMoveSteps1.Text));
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctMoveSteps2)
            {
                flagMctMoveSteps2 = false;
                // set posn for Mirror 2
                txBuffer = MCTCommand.TARGET_WRITE(pollMct, txPid, 0x13, Convert.ToUInt16(txtMctMoveSteps2.Text));
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctChangeTrack1)
            {
                flagMctChangeTrack1 = false;
                if (tabCtrl.SelectedIndex == tabString.TabIndex)
                    pollMct = (byte)(Convert.ToInt16(txtNumMct.Text) + 0x40);
                else if (cbMctPollAddr.Items.Count > 0)
                    pollMct = (byte)(cbMctPollAddr.SelectedIndex + 1);
                else
                    return;
                // set tracking for Mirror 1
                txBuffer = MCTCommand.TRACK(pollMct, txPid, MIRROR1, trackNum[cbMctTrack1.SelectedIndex]);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagMctChangeTrack2)
            {
                flagMctChangeTrack2 = false;
                if (tabCtrl.SelectedIndex == tabString.TabIndex)
                    pollMct = (byte)(Convert.ToInt16(txtNumMct.Text) + 0x40);
                else if (cbMctPollAddr.Items.Count > 0)
                    pollMct = (byte)(cbMctPollAddr.SelectedIndex + 1);
                else
                    return;

                // set tracking for Mirror 2
                txBuffer = MCTCommand.TRACK(pollMct, txPid, MIRROR2, trackNum[cbMctTrack2.SelectedIndex]);           
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                Mct485WaitResp(WhichStringRadio(), pollMct);
            }
            else if (flagWriteAllParam)
            {
                flagWriteAllParam = false;
                lstResults.Items.Add("Writing all Parameters");
                // write all params to all MCT's
                for (int i = 0; i < PARAM_NUM_VALUES && i < numParamEntries; i++)
                {
                    if (paramEnable[i].Checked)
                    {
                        lstResults.Items.Add("Writing param " + paramLabel[i]);
                        for (byte j = 1; j <= mctMaxAddr[WhichStringRadio()]; j++)
                        {
                            txBuffer = MCTCommand.PARAM_WRITE(j, txPid, mctMaxAddr[WhichStringRadio()], paramNum[i], Convert.ToInt16(paramDefaultText[i].Text));
                            SendTxMessage(WhichStringRadio(), j, MCTCommand.USB_CMD_SEND_MCT485);
                            Mct485WaitResp(WhichStringRadio(), j);
                        }
                    }
                }
                lstResults.Items.Add("Done writing");
            }
            else if (flagWriteParam != 0) // flagWriteParam contains the number of the MCT we need to write to.
            {
                byte mctToWrite = (byte)(flagWriteParam);
                lstResults.Items.Add("Writing Parameters to MCT #" + mctToWrite.ToString());
                // write all params to MCT
                for (int i = 0; i < PARAM_NUM_VALUES && i < numParamEntries; i++)
                {
                    if (paramEnable[i].Checked)
                    {
                        lstResults.Items.Add("Writing param " + paramLabel[i]);
                        txBuffer = MCTCommand.PARAM_WRITE(mctToWrite, txPid, (byte)(flagWriteParam), (byte)paramNum[i], Convert.ToInt16(paramDefaultText[i].Text));
                        SendTxMessage(WhichStringRadio(), mctToWrite, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), mctToWrite);
                    }
                }
                flagWriteParam = 0;
                lstResults.Items.Add("Done writing");
            }
            else if (flagReadAllParam)
            {
                flagReadAllParam = false;
                lstResults.Items.Add("Reading all Parameters");

                System.Threading.Thread.Sleep(100);
                
                for (byte mct = 1; mct <= mctMaxAddr[WhichStringRadio()]; mct++)
                {
                    for (int i = 0; i < PARAM_NUM_VALUES; i++)
                    {
                        if (tabCtrl.SelectedIndex != tabMctParam.TabIndex)
                        {
                            i = PARAM_NUM_VALUES;
                            mct = 250;
                            break;
                        }
                        txBuffer = MCTCommand.PARAM_READ(mct, txPid, (byte)(paramNum[i]));
                        SendTxMessage(WhichStringRadio(), mct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), mct);
                    }
                }
                lstResults.Items.Add("Done reading");
            }
            else if (flagSetClock)
            {
                flagSetClock = false;
                txBuffer[ MCTCommand.USB_PACKET_DATA] = (byte)DateTime.Now.Second;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 1] = (byte)DateTime.Now.Minute;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 2] = (byte)DateTime.Now.Hour;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 3] = (byte)DateTime.Now.Day;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 4] = (byte)DateTime.Now.Month;
                txBuffer[ MCTCommand.USB_PACKET_DATA + 5] = (byte)(DateTime.Now.Year - 2000);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_RTC);
            }
            else if (flagSetDesiccant)
            {
                flagSetDesiccant = false;
                SendTxMessage(0, 0, MCTCommand.USB_CMD_DESICCANT);
            }
            else if (paramStartRead < paramStopRead)
            {
                if (paramStartRead < 128)
                {
                    txBuffer[ MCTCommand.USB_PACKET_DATA] = (byte)paramStartRead;
                    SendTxMessage(0, 0, MCTCommand.USB_RESP_SFC_PARAM);
                    paramStartRead += 10;
                }
                else
                {
                    paramStartRead = 0;
                    paramStopRead = 0;
                }
            }
            else if (paramStartWrite < paramStopWrite)
            {
                if (paramStartWrite < 128)
                {
                    int i;
                    txBuffer[ MCTCommand.USB_PACKET_DATA] = (byte)paramStartWrite;
                    i = 0;
                    while (i < 10 && paramStartWrite < paramStopWrite)
                    {
                        txBuffer[ MCTCommand.USB_PACKET_DATA + i * 4 + 1] = (byte)(sfcParam[paramStartWrite] >> 24);
                        txBuffer[ MCTCommand.USB_PACKET_DATA + i * 4 + 2] = (byte)(sfcParam[paramStartWrite] >> 16);
                        txBuffer[ MCTCommand.USB_PACKET_DATA + i * 4 + 3] = (byte)(sfcParam[paramStartWrite] >> 8);
                        txBuffer[ MCTCommand.USB_PACKET_DATA + i * 4 + 4] = (byte)sfcParam[paramStartWrite];
                        paramStartWrite++;
                        i++;
                    }
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = (byte)( MCTCommand.USB_PACKET_DATA + i * 4 + 3);
                    SendTxMessage(0, 0, MCTCommand.USB_CMD_SFC_PARAM);
                }
                else
                {
                    paramStartWrite = 0;
                    paramStopWrite = 0;
                }
            }
            else if (tabCtrl.SelectedIndex == tabString.TabIndex)
            {
                if (pollMct == 0)
                {
                    if (pollType == POLL_CHANNELS)
                    {
                        // read String info
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_STRING);
                        pollType = POLL_MIRRORS;
                    }
                    else
                    {
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_RESP_FIELD_STATE);
                        pollMct = 1;
                        pollType = POLL_CHANNELS;
                    }
                }
                else if (stringState[WhichStringRadio()] == STRING_ACTIVE)
                {
                    if (pollMct <= mctMaxAddr[WhichStringRadio()])
                    {
                        if (pollType == POLL_CHANNELS)
                        {
                            SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_CHAN);
                            pollType = POLL_MIRRORS;
                        }
                        else if (pollType == POLL_MIRRORS)
                        {
                            SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_MIRRORS);
                            pollType = POLL_CHANNELS;
                            pollMct++;
                        }
                    }
                    else
                    {
                        pollMct = 0;
                    }
                }
                else
                {
                    // keep polling string info
                    pollMct = 0;
                }
                if (pollMct > mctMaxAddr[WhichStringRadio()])
                {
                    pollMct = 0;
                }
            }
            else if (tabCtrl.SelectedIndex == tabSFC.TabIndex)
            {
                // read version and FCE values
                if (pollType <= POLL_SFC_VERSION)
                {
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_VSTRING);
                    pollType = POLL_FCE;
                }
                else if (pollType == POLL_FCE)
                {
                    // read String info
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_FCE);
                    pollType = POLL_RTU;
                }
                else if (pollType == POLL_RTU)
                {
                    // read String info
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_RTU);
                    pollType = POLL_RTC;
                }
                else if (pollType == POLL_RTC)
                {
                    // read String info
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_RESP_RTC);
                    pollType = POLL_DESICCANT;
                }
                else
                {
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_RESP_DESICCANT);
                    pollType = POLL_FCE;
                }
            }
            else if (tabCtrl.SelectedIndex == tabMCTfw.TabIndex)
            {
                // fw update
                if (flagGetMctVersions)
                {
                    byte mct;
                    flagGetMctVersions = false;
                    countGetMctVersions++;
                    if (countGetMctVersions % 1000 == 0)
                    {
                        lstResults.Items.Add("GetVersions count = " + countGetMctVersions.ToString());
                    }
                    for (mct = 1; mct <= mctMaxAddr[WhichStringRadio()]; mct++)
                    {
                        textMctMver[mct - 1].Text = "";
                        textMctSver[mct - 1].Text = "";
                        checkMctMvalid[mct - 1].Checked = false;
                        checkMctSvalid[mct - 1].Checked = false;

                        txBuffer = MCTCommand.GET_STRING(mct, txPid);
                        SendTxMessage(WhichStringRadio(), mct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), mct);


                        txBuffer = MCTCommand.SLAVE_GET_STRING(mct, txPid);
                        SendTxMessage(WhichStringRadio(), mct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), mct);
                    }
                    while (mct < 10)
                    {
                        textMctMver[mct - 1].Text = "";
                        textMctSver[mct - 1].Text = "";
                        checkMctMvalid[mct - 1].Checked = false;
                        checkMctSvalid[mct - 1].Checked = false;
                        mct++;
                    }
                }
            }
            else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex)
            {
                if (cbMctPollAddr.Items.Count > 0)
                {
                    if (cbMctPollAddr.SelectedIndex < 0)
                    {
                        cbMctPollAddr.SelectedIndex = 0;
                    }
                    pollMct = (byte)(cbMctPollAddr.SelectedIndex + 1);
                    if (pollType == POLL_CHANNELS)
                    {
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_CHAN);
                        pollType = POLL_MIRRORS;
                    }
                    else if (pollType == POLL_MIRRORS)
                    {
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_MIRRORS);
                        pollType++;
                        pollMct++;
                    }
                    else if (pollType == POLL_POSN1 || pollType == POLL_POSN2)
                    {
                        if (pollType == POLL_POSN1)
                            txBuffer = MCTCommand.POSN_READ(pollMct, txPid, 0x03);
                        else
                            txBuffer = MCTCommand.POSN_READ(pollMct, txPid, 0x13);
                        
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), pollMct);
                        pollType++;
                    }
                    else if (pollType == POLL_TARGET1 || pollType == POLL_TARGET2)
                    {
                        if (pollType == POLL_TARGET1)
                            txBuffer = MCTCommand.TARGET_READ(pollMct,txPid, 0x03);
                        else
                            txBuffer = MCTCommand.TARGET_READ(pollMct, txPid, 0x13);

                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), pollMct);
                        pollType++;
                    }
                    else if (pollType == POLL_ERROR1 || pollType == POLL_ERROR2)
                    {
                        txBuffer = MCTCommand.PARAM_READ(pollMct, txPid, (byte)(126 + pollType - POLL_ERROR1));
                        SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(WhichStringRadio(), pollMct);
                        pollType++;
                    }
                    else
                    {
                        pollType = POLL_CHANNELS;
                    }
                }
                else
                {
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_STRING);
                    pollType = POLL_MIRRORS;
                }
            }

            timer1.Enabled = true;
        }

        public void SendTxMessage(byte str, byte mct, byte cmd)
        {
            txBuffer[ MCTCommand.USB_PACKET_CMD] = cmd;
            txBuffer[ MCTCommand.USB_PACKET_STR] = str;
            Boolean usbGetResp = true;
            switch (txBuffer[ MCTCommand.USB_PACKET_CMD])
            {
                case MCTCommand.USB_CMD_GET_VSTRING:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = mct;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_GET_CHAN:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = mct;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_GET_MIRRORS:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = mct;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_GET_STRING:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = 0;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_FIELD_STATE:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = 0;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 8;
                    break;
                case MCTCommand.USB_RESP_FIELD_STATE:
                    txBuffer[ MCTCommand.USB_PACKET_CMD] = MCTCommand.USB_CMD_FIELD_STATE; //??
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = 0;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_GET_FCE:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = 0;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_GET_RTU:
                    txBuffer[ MCTCommand.USB_PACKET_MCT] = mct;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_SEND_MCT485:
                    break;
                case MCTCommand.USB_CMD_GET_MCT485:
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_CMD_DESICCANT:
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 9;
                    txBuffer[ MCTCommand.USB_PACKET_DATA] = desiccantNewState;
                    txBuffer[ MCTCommand.USB_PACKET_DATA+1] = desiccantNewOutputs;
                    break;
                case MCTCommand.USB_CMD_RTC:
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 13;
                    break;
                case MCTCommand.USB_CMD_SFC_PARAM:
                    // length is set in the paramStartWrite, paramStopWrite section above
                    break;
                case MCTCommand.USB_CMD_TEST:
                    usbGetResp = false;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 8;
                    txBuffer[ MCTCommand.USB_PACKET_DATA] = (byte)testBtnValue;
                    break;
                case MCTCommand.USB_RESP_RTC:
                    txBuffer[ MCTCommand.USB_PACKET_CMD] = MCTCommand.USB_CMD_RTC;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_RESP_DESICCANT:
                    txBuffer[ MCTCommand.USB_PACKET_CMD] = MCTCommand.USB_CMD_DESICCANT;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 7;
                    break;
                case MCTCommand.USB_RESP_SFC_PARAM:
                    txBuffer[ MCTCommand.USB_PACKET_CMD] = MCTCommand.USB_CMD_SFC_PARAM;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 8;
                    break;
                case MCTCommand.USB_RESP_MEMORY:
                    txBuffer[ MCTCommand.USB_PACKET_CMD] = MCTCommand.USB_CMD_MEMORY;
                    txBuffer[ MCTCommand.USB_PACKET_LEN] = 11;
                    break;
            }

            txBuffer[ MCTCommand.USB_PACKET_PID] = ++txPid;
            TxCrcCalc(0, (byte)(txBuffer[ MCTCommand.USB_PACKET_LEN] - 2));

            if (SendUsbCmd(ref txBuffer, usbGetResp))
            {
                ProcessUsbResp();
            }
            else
            {
                // some problem with USB - may be that device is not detected. Lets wait a quarter second
                //System.Threading.Thread.Sleep(250);
                rxBuffer[63] = 0; //???
            }
        }

        public void ProcessUsbResp() {
            int str = rxBuffer[ MCTCommand.USB_PACKET_STR];
            int mct = rxBuffer[ MCTCommand.USB_PACKET_MCT];
            if (mct <= 0 || mct > 10)
            {
                mct = 0;
            }
            else
            {
                mct--;
            }
            switch (rxBuffer[ MCTCommand.USB_PACKET_CMD])
            {
                case MCTCommand.USB_RESP_GET_VSTRING: // 0x97
                    txtVersString.Text = "";
                    for (int i = 0; i < 20; i++)
                    {
                        if (rxBuffer[ MCTCommand.USB_PACKET_DATA + i] != '\0')
                        {
                            txtVersString.Text += (char)rxBuffer[ MCTCommand.USB_PACKET_DATA + i];
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case MCTCommand.USB_RESP_GET_CHAN:    // 0xB3
                    if (rxBuffer[ MCTCommand.USB_PACKET_MCT] > 0 && rxBuffer[ MCTCommand.USB_PACKET_MCT] <= 10)
                    {
                        mctChan[str, mct, MCT_RTD_TRACKLEFT_1A] = RespChan(5 + 2 * MCT_RTD_TRACKLEFT_1A);
                        mctChan[str, mct, MCT_RTD_TRACKRIGHT_1A] = RespChan(5 + 2 * MCT_RTD_TRACKRIGHT_1A);
                        mctChan[str, mct, MCT_RTD_TRACKLEFT_1B] = RespChan(5 + 2 * MCT_RTD_TRACKLEFT_1B);
                        mctChan[str, mct, MCT_RTD_TRACKRIGHT_1B] = RespChan(5 + 2 * MCT_RTD_TRACKRIGHT_1B);
                        mctChan[str, mct, MCT_RTD_MANIFOLD_1] = RespChan(5 + 2 * MCT_RTD_MANIFOLD_1);
                        mctChan[str, mct, MCT_HUMIDITY1] = RespChan(5 + 2 * MCT_HUMIDITY1);
                        mctChan[str, mct, MCT_LOCAL_TEMPA] = RespChan(5 + 2 * MCT_LOCAL_TEMPA);
                        mctChan[str, mct, MCT_RTD_TRACKLEFT_2A] = RespChan(5 + 2 * MCT_RTD_TRACKLEFT_2A);
                        mctChan[str, mct, MCT_RTD_TRACKRIGHT_2A] = RespChan(5 + 2 * MCT_RTD_TRACKRIGHT_2A);
                        mctChan[str, mct, MCT_RTD_TRACKLEFT_2B] = RespChan(5 + 2 * MCT_RTD_TRACKLEFT_2B);
                        mctChan[str, mct, MCT_RTD_TRACKRIGHT_2B] = RespChan(5 + 2 * MCT_RTD_TRACKRIGHT_2B);
                        mctChan[str, mct, MCT_RTD_MANIFOLD_2] = RespChan(5 + 2 * MCT_RTD_MANIFOLD_2);
                        mctChan[str, mct, MCT_HUMIDITY2] = RespChan(5 + 2 * MCT_HUMIDITY2);
                        mctChan[str, mct, MCT_LOCAL_TEMPB] = RespChan(5 + 2 * MCT_LOCAL_TEMPB);
                        if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio() && pollMct == rxBuffer[ MCTCommand.USB_PACKET_MCT])
                        {
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_1A, txtMctRtd1AL);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_1A, txtMctRtd1AR);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_1B, txtMctRtd1BL);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_1B, txtMctRtd1BR);
                            DrawMctDeg(5 + 2 * MCT_RTD_MANIFOLD_1, txtMctMan1);
                            DrawMctDeg(5 + 2 * MCT_LOCAL_TEMPA, txtMctLocalTempA);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_2A, txtMctRtd2AL);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_2A, txtMctRtd2AR);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_2B, txtMctRtd2BL);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_2B, txtMctRtd2BR);
                            DrawMctDeg(5 + 2 * MCT_RTD_MANIFOLD_2, txtMctMan2);
                            DrawMctDeg(5 + 2 * MCT_LOCAL_TEMPB, txtMctLocalTempB);
                            DrawMctHumid(5 + 2 * MCT_HUMIDITY1, txtMctHumid1);
                            DrawMctHumid(5 + 2 * MCT_HUMIDITY2, txtMctHumid2);
                        }
                        else if (str == WhichStringRadio())
                        {
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_1A, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD1AL]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_1A, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD1AR]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_1B, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD1BL]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_1B, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD1BR]);
                            DrawMctDeg(5 + 2 * MCT_RTD_MANIFOLD_1, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD1MAN]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_2A, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD2AL]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_2A, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD2AR]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKLEFT_2B, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD2BL]);
                            DrawMctDeg(5 + 2 * MCT_RTD_TRACKRIGHT_2B, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD2BR]);
                            DrawMctDeg(5 + 2 * MCT_RTD_MANIFOLD_2, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_RTD2MAN]);
                        }
                    }
                    break;
                case MCTCommand.USB_RESP_GET_MIRRORS: // 0xC2
                    if (rxBuffer[ MCTCommand.USB_PACKET_MCT] > 0 && rxBuffer[ MCTCommand.USB_PACKET_MCT] <= 10)
                    {
                        mctTrack[str, mct, MIRROR1] = rxBuffer[5];
                        mctTrack[str, mct, MIRROR2] = rxBuffer[11];
                        mctSensor[str, mct, MIRROR1] = rxBuffer[6];
                        mctSensor[str, mct, MIRROR2] = rxBuffer[12];
                        mctPosn[str, mct, POSN1A] = RespChan(7);
                        mctPosn[str, mct, POSN1B] = RespChan(9);
                        mctPosn[str, mct, POSN2A] = RespChan(13);
                        mctPosn[str, mct, POSN2B] = RespChan(15);
                        if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio() && pollMct == rxBuffer[ MCTCommand.USB_PACKET_MCT])
                        {
                            DrawComboTrack(5, cbMctTrack1);
                            DrawComboTrack(11, cbMctTrack2);
                            DrawMctMdeg(7, txtMctPosn1A);
                            DrawMctMdeg(9, txtMctPosn1B);
                            DrawMctMdeg(13, txtMctPosn2A);
                            DrawMctMdeg(15, txtMctPosn2B);
                            checkMctHome1A.Checked = ((rxBuffer[6] & 0x01) != 0);
                            checkMctStow1A.Checked = ((rxBuffer[6] & 0x02) != 0);
                            checkMctHome1B.Checked = ((rxBuffer[6] & 0x10) != 0);
                            checkMctStow1B.Checked = ((rxBuffer[6] & 0x20) != 0);
                            checkMctHome2A.Checked = ((rxBuffer[12] & 0x01) != 0);
                            checkMctStow2A.Checked = ((rxBuffer[12] & 0x02) != 0);
                            checkMctHome2B.Checked = ((rxBuffer[12] & 0x10) != 0);
                            checkMctStow2B.Checked = ((rxBuffer[12] & 0x20) != 0);
                        }
                        else if (str == WhichStringRadio())
                        {
                            DrawstrMctTextTrack(5, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_TRACK1);
                            DrawstrMctTextTrack(11, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_TRACK2);
                            //DrawstrMctTextMdeg(7, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN1A);
                            //DrawstrMctTextMdeg(9, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN1B);
                            //DrawstrMctTextMdeg(13, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN2A);
                            //DrawstrMctTextMdeg(15, rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN2B);
                            DrawMctMdeg(7, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN1A]);
                            DrawMctMdeg(9, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN1B]);
                            DrawMctMdeg(13, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN2A]);
                            DrawMctMdeg(15, strMctText[rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1, TXT_POSN2B]);
                        }
                    }
                    break;
                case MCTCommand.USB_RESP_GET_STRING:
                    mctMaxAddr[str] = rxBuffer[5];
                    if (tabCtrl.SelectedIndex == tabString.TabIndex && str == WhichStringRadio())
                    {
                        txtNumMct.Text = mctMaxAddr[str].ToString();
                    }
                    else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio())
                    {
                        if (mctMaxAddr[str] != rxBuffer[5])
                        {
                            SetTabMctAddrCombo(mctMaxAddr[str]);
                        }
                    }
                    stringState[str] = rxBuffer[6];
                    if (str == WhichStringRadio())
                    {
                        DrawStringState();
                    }
                    break;
                case MCTCommand.USB_RESP_FIELD_STATE:
                    fieldState = rxBuffer[5];
                    DrawFieldState();
                    break;
                case MCTCommand.USB_RESP_GET_FCE:
                    fceInputState = rxBuffer[5];
                    fceOutputState = rxBuffer[7];
                    for (int i = 0; i < 7; i++)
                    {
                        fceAD[i] = RespChan(8 + 2 * i);
                    }
                    fieldDNI = RespChan(22);
                    fceRtd = RespChan(24);
                    DrawFCE();
                    break;
                case MCTCommand.USB_RESP_GET_RTU:
                    for (int i = 0; i < 4; i++)
                    {
                        rtu[i] = RespChan(7 + i * 4);
                    }
                    DrawRTU();
                    break;
                case MCTCommand.USB_RESP_SEND_MCT485:
                    break;
                case MCTCommand.USB_RESP_GET_MCT485:
                    if ((rxBuffer[ MCTCommand.USB_PACKET_MCT] & 0x80) == 0x80)
                    {
                        // response address should have the host bit set
                        rxBuffer[ MCTCommand.USB_PACKET_MCT] &= 0x7F;
                        mct = rxBuffer[ MCTCommand.USB_PACKET_MCT] - 1;
                    }
                    else
                    {
                        rxBuffer[ MCTCommand.USB_PACKET_MCT] = 0;
                        mct = 0;
                    }
                    switch (rxBuffer[8])
                    {
                        case MCTCommand.MCT_RESP_GET_STRING:
                            if (rxBuffer[ MCTCommand.USB_PACKET_MCT] > 0 && rxBuffer[ MCTCommand.USB_PACKET_MCT] <= 10 && rxBuffer[7] == 48 && rxBuffer[ MCTCommand.USB_PACKET_LEN] == 55)
                            {
                                checkMctMvalid[mct].Checked = ((rxBuffer[9] & MCT_STAT_APP_VALID) == MCT_STAT_APP_VALID);
                                checkMctSvalid[mct].Checked = ((rxBuffer[9] & MCT_STAT_SAPP_VALID) == MCT_STAT_SAPP_VALID);
                                if (rxBuffer[10] == 0)
                                {
                                    textMctMver[mct].Text = "";
                                    for (int i = 0; i < 40 && rxBuffer[11 + i] != '\0'; i++)
                                    {
                                        textMctMver[mct].Text += (char)rxBuffer[11 + i];
                                    }
                                }
                                else if (rxBuffer[10] == 1)
                                {
                                    textMctSver[mct].Text = "";
                                    for (int i = 0; i < 40 && rxBuffer[11 + i] != '\0'; i++)
                                    {
                                        textMctSver[mct].Text += (char)rxBuffer[11 + i];
                                    }
                                }
                            }
                            else if (rxBuffer[ MCTCommand.USB_PACKET_LEN] > 7)
                            {
                                string s;
                                s = "ERR - Get Vers:";
                                for (int i = 0; i < rxBuffer[ MCTCommand.USB_PACKET_LEN]; i++) {
                                    s += ' ';
                                    s += rxBuffer[i].ToString("X2");
                                }
                                lstResults.Items.Add(s);
                            }
                            break;
                        case MCTCommand.MCT_RESP_POSN:
                            if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 19)
                            {
                                // incorrect length, ignore packet
                            }
                            else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio())
                            {
                                if (pollType == POLL_POSN1)
                                {
                                    DrawMctSteps(11, txtMctSteps1A);
                                    DrawMctSteps(13, txtMctSteps1B);
                                }
                                else if (pollType == POLL_POSN2)
                                {
                                    DrawMctSteps(11, txtMctSteps2A);
                                    DrawMctSteps(13, txtMctSteps2B);
                                }
                            }
                            break;
                        case MCTCommand.MCT_RESP_TARGET:
                            if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 19)
                            {
                                // incorrect length, ignore packet
                            }
                            else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio())
                            {
                                if (pollType == POLL_TARGET1)
                                {
                                    DrawMctSteps(11, txtMctTarg1A);
                                    DrawMctSteps(13, txtMctTarg1B);
                                }
                                else if (pollType == POLL_TARGET2)
                                {
                                    DrawMctSteps(11, txtMctTarg2A);
                                    DrawMctSteps(13, txtMctTarg2B);
                                }
                            }
                            break;
                        case MCTCommand.MCT_RESP_PARAM:
                            byte mctAddr = rxBuffer[5];
                            if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 17)
                            {
                                // incorrect length, ignore packet
                            }
                            else if (mctAddr > 0x8A || mctAddr < 1)
                            {
                                // address to high for individual response, CMD was broadcast
                            }
                            else if (tabCtrl.SelectedIndex == tabMctParam.TabIndex && str == WhichStringRadio())
                            {
                                mctAddr = (byte)(mctAddr & 0x0F);
                                if (mctAddr <= mctMaxAddr[WhichStringRadio()])
                                {
                                    for (int i = 0; i < numParamEntries && i < PARAM_NUM_VALUES; i++)
                                    {
                                        if (rxBuffer[10] == paramNum[i])
                                        {
                                            // found matching parameter
                                            int val = rxBuffer[11];
                                            val *= 256;
                                            val += rxBuffer[12];
                                            paramText[mctAddr - 1, i].Text = val.ToString();
                                        }
                                    }
                                }
                            }
                            else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex && str == WhichStringRadio())
                            {
                                int val = rxBuffer[11];
                                val *= 256;
                                val += rxBuffer[12];
                                if (rxBuffer[10] == 126)
                                    DrawError(val, txtMctError1);
                                else if (rxBuffer[10] == 127)
                                    DrawError(val, txtMctError2);
                            }
                            break;
                    }
                    break;
                case MCTCommand.USB_RESP_RTC:
                    if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 19)
                    {
                        // incorrect length, ignore packet
                    }
                    else if (tabCtrl.SelectedIndex == tabSFC.TabIndex)
                    {
                        if (rxBuffer[ MCTCommand.USB_PACKET_DATA + 2] == 0) {
                            txtRTC.Text = "12";
                        }
                        else if (rxBuffer[ MCTCommand.USB_PACKET_DATA + 2] <= 12) {
                            txtRTC.Text = rxBuffer[ MCTCommand.USB_PACKET_DATA + 2].ToString("D2");
                        }
                        else {
                            txtRTC.Text = (rxBuffer[ MCTCommand.USB_PACKET_DATA + 2]-12).ToString("D2");
                        }
                        txtRTC.Text += ":" + rxBuffer[ MCTCommand.USB_PACKET_DATA + 1].ToString("D2");
                        txtRTC.Text += ":" + rxBuffer[ MCTCommand.USB_PACKET_DATA].ToString("D2");
                        if (rxBuffer[ MCTCommand.USB_PACKET_DATA + 2] < 12) {
                            txtRTC.Text += "am";
                        }
                        else {
                            txtRTC.Text += "pm";
                        }
                        txtRTC.Text += " " + rxBuffer[ MCTCommand.USB_PACKET_DATA + 4].ToString("D2");
                        txtRTC.Text += "-" + rxBuffer[ MCTCommand.USB_PACKET_DATA + 3].ToString("D2");
                        txtRTC.Text += "-20" + rxBuffer[ MCTCommand.USB_PACKET_DATA + 5].ToString("D2");
                        sfcPcbTemp = RespChan( MCTCommand.USB_PACKET_DATA + 6);
                        sfcThermistor = RespChan( MCTCommand.USB_PACKET_DATA + 8);
                        sfcHumidity = RespChan( MCTCommand.USB_PACKET_DATA + 10);
                        DrawSfcTemps();
                    }
                    break;
                case MCTCommand.USB_RESP_DESICCANT:
                    if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 9)
                    {
                        // incorrect length, ignore packet
                    }
                    else if (tabCtrl.SelectedIndex == tabSFC.TabIndex)
                    {
                        flagDesiccantResp = true;
                        if (rxBuffer[ MCTCommand.USB_PACKET_DATA] == DESICCANT_OFF)
                            radioDesOff.Checked = true;
                        else if (rxBuffer[ MCTCommand.USB_PACKET_DATA] == DESICCANT_DRYING)
                            radioDesDrying.Checked = true;
                        else if (rxBuffer[ MCTCommand.USB_PACKET_DATA] == DESICCANT_REGEN)
                            radioDesRegen.Checked = true;
                        else if (rxBuffer[ MCTCommand.USB_PACKET_DATA] == DESICCANT_CLOSED)
                            radioDesClosed.Checked = true;
                        else if (rxBuffer[ MCTCommand.USB_PACKET_DATA] == DESICCANT_MANUAL)
                            radioDesManual.Checked = true;
                        cbFan.Checked = ((rxBuffer[ MCTCommand.USB_PACKET_DATA+1] & FCE_OUT_FAN) != 0);
                        cbValve.Checked = ((rxBuffer[ MCTCommand.USB_PACKET_DATA+1] & FCE_OUT_VALVE) != 0);
                        cbHeat.Checked = ((rxBuffer[ MCTCommand.USB_PACKET_DATA+1] & FCE_OUT_HEAT) != 0);
                        // flush out the CheckChanged events before clearing the flagDesiccantResp flag
                        Application.DoEvents();
                        flagDesiccantResp = false;
                    }
                    break;
                case MCTCommand.USB_RESP_SFC_PARAM:
                    if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 47)
                    {
                        // incorrect length, ignore packet
                    }
                    else
                    {
                        int paramNum = rxBuffer[ MCTCommand.USB_PACKET_DATA];
                        int i = 0;
                        while (paramNum < 128 && i < 40)
                        {
                            sfcParam[paramNum] = rxBuffer[ MCTCommand.USB_PACKET_DATA + i + 1];
                            sfcParam[paramNum] *= 256;
                            sfcParam[paramNum] += rxBuffer[ MCTCommand.USB_PACKET_DATA + i + 2];
                            sfcParam[paramNum] *= 256;
                            sfcParam[paramNum] += rxBuffer[ MCTCommand.USB_PACKET_DATA + i + 3];
                            sfcParam[paramNum] *= 256;
                            sfcParam[paramNum] += rxBuffer[ MCTCommand.USB_PACKET_DATA + i + 4];
                            UpdateParam(paramNum);
                            paramNum++;
                            i += 4;
                        }
                    }
                    break;
            }
        }

        public void TxCrcCalc(byte offset, byte len)
        {
            uint crc = 0x1D0F;
            for (byte i = 0; i < len; i++)
            {
                crc = (byte)(crc >> 8) | ((crc & 0xFF) << 8);
                crc ^= txBuffer[i + offset];
                crc ^= (crc & 0xFF) >> 4;
                crc ^= (crc << 8) << 4;
                crc ^= ((crc & 0xFF) << 4) << 1;
            }
            txBuffer[offset + len] = (byte)(crc >> 8);
            txBuffer[offset + len + 1] = (byte)crc;
        }

        private Boolean SendUsbCmd(ref Byte [] sendBuffer, Boolean getResponse)
        {
            int loopCount;

            try
            {
                Boolean success;

                cmdComplete = false;
                cmdFailed = false;
                cmdTries = 0;
                myDeviceDetected = FindMyDevice();


                if (myDeviceDetected)
                {
                    Array.Clear(rxBuffer,0,rxBuffer.Length);
                    success = myWinUsbDevice.SendViaBulkTransfer
                    (ref sendBuffer,
                    sendBuffer[2]);
                    packetWrCount++;
                }
                else
                {
                    cmdComplete = true;
                    cmdFailed = true;
                    return false;
                }

                if (success)
                {
                    //formText = String.Format("Wrote reg[{0}] = [{1}]", reg, val);
                    //formText = "Data sent via bulk transfer.";
                }
                else
                {
                    MyMarshalToForm("AddItemToListBox", "Bulk OUT transfer failed.");
                    cmdComplete = true;
                    cmdFailed = true;
                    return false;
                }
                if (!getResponse)
                {
                    return true;
                }
                do
                {
                    ReadDataViaBulkTransfer();
                    packetRdCount++;
                    cmdTries++;
                    loopCount = 0;
                    while (loopCount < 200)
                    {
                        if (cmdComplete)
                        {
                            return success;
                        }
                        else if (cmdFailed)
                        {
                            cmdFailed = false;
                            break;
                        }
                        loopCount++;
                        // delay for USB messages to clear 
                        System.Threading.Thread.Sleep(1);
                        Application.DoEvents();
                    }
                } while (cmdTries < 3);
                cmdComplete = true;
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public void DrawstrMctTextDeg(byte index, int mct, byte row)
        {
            String s;

            int chan = rxBuffer[index];
            chan *= 256;
            chan += rxBuffer[index + 1];

            s = (chan / 10).ToString() + "." + (chan % 10).ToString();
            strMctText[mct, row].Text = s;
        }
        public void DrawMctDeg(byte index, TextBox tb)
        {
            String s;
            int deg;

            deg = rxBuffer[index];
            deg *= 256;
            deg += rxBuffer[index + 1];
            s = "";
            if (rxBuffer[index] >= 0x80)
            {
                deg = 65536 - deg;
                s = "-";
            }

            s = s + (deg / 10).ToString() + "." + (deg % 10).ToString();
            tb.Text = s;
        }
        public void DrawMctSteps(byte index, TextBox tb)
        {
            String s;

            int chan = rxBuffer[index];
            chan *= 256;
            chan += rxBuffer[index + 1];

            s = chan.ToString();
            tb.Text = s;
        }
        public void DrawMctHumid(byte index, TextBox tb) {
            String s;
            int humid;

            humid = rxBuffer[index];
            humid *= 256;
            humid += rxBuffer[index + 1];
            s = (humid / 10).ToString() + "." + (humid % 10).ToString() + "%";
            tb.Text = s;
        }
        public void DrawSfcTemps()
        {
            String s;

            s = (sfcPcbTemp / 10).ToString() + "." + (sfcPcbTemp % 10).ToString() + "C";
            txtSfcPcbTemp.Text = s;
            if (sfcThermistor == 32767)
            {
                txtSfcThermistor.Text = "short";
            }
            else if (sfcThermistor == -32768)
            {
                txtSfcThermistor.Text = "open";
            }
            else
            {
                s = (sfcThermistor / 10).ToString() + "." + (sfcThermistor % 10).ToString() + "C";
                txtSfcThermistor.Text = s;
            }
            if (sfcHumidity > 0 && sfcHumidity < 1001)
            {
                s = (sfcHumidity / 10).ToString() + "." + (sfcHumidity % 10).ToString() + "%";
                txtSfcHumidity.Text = s;
            }
            else
            {
                txtSfcHumidity.Text = "open";
            }
        }
        public void DrawstrMctTextTrack(byte index, int mct, byte row)
        {
            for (int i = 0; i < numTrackEntries; i++)
            {
                if (rxBuffer[index] == trackNum[i])
                {
                    // found a match
                    strMctText[mct, row].Text = trackName[i];
                    return;
                }
                strMctText[mct, row].Text = "#" + rxBuffer[index].ToString();
            }
#if FALSE
            switch (rxBuffer[index]) {
                case TRACK_OFF:
                    strMctText[mct, row].Text = "OFF";
                    break;
                case TRACK_HOME:
                    strMctText[mct, row].Text = "HOME";
                    break;
                case TRACK_SETTLE:
                    strMctText[mct, row].Text = "SETTLE";
                    break;
                case TRACK_FIND_EDGE_FWD:
                    strMctText[mct, row].Text = "FIND EDGE FWD";
                    break;
                case TRACK_FIND_EDGE_REV:
                    strMctText[mct, row].Text = "FIND EDGE REV";
                    break;
                case TRACK_GO_TO_MIDDLE:
                    strMctText[mct, row].Text = "GO MIDDLE";
                    break;
                case TRACK_MIDDLE:
                    strMctText[mct, row].Text = "MIDDLE";
                    break;
                case TRACK_END:
                    strMctText[mct, row].Text = "END";
                    break;
                default:
                    strMctText[mct, row].Text = "#" + rxBuffer[index].ToString();
                    break;
            }
#endif
        }
        public void DrawComboTrack(byte index, ComboBox cb)
        {
            if (cb.Focused)
            {
                return;
            }
            for (int i = 0; i < numTrackEntries; i++)
            {
                if (rxBuffer[index] == trackNum[i])
                {
                    // found a match
                    if (cb.SelectedIndex != i)
                    {
                        // only change if different, confuses change event
                        cb.SelectedIndex = i;
                    }
                    return;
                }
                //cb.SelectedIndex = 0;
            }
        }
        public void DrawstrMctTextMdeg(byte index, int mct, byte row)
        {
            String s;

            int chan = rxBuffer[index];
            chan *= 256;
            chan += rxBuffer[index + 1];

            s = (chan / 100).ToString() + "." + (chan % 100).ToString("D2");
            strMctText[mct, row].Text = s;
        }
        public void DrawMctMdeg(byte index, TextBox tb)
        {
            String s;

            int chan = rxBuffer[index];
            chan *= 256;
            chan += rxBuffer[index + 1];

            s = (chan / 100).ToString() + "." + (chan % 100).ToString("D2");
            tb.Text = s;
        }
        public void DrawStringState()
        {
            switch (stringState[WhichStringRadio()])
            {
                case STRING_POWER_OFF: // = 0;
                    txtStringState.Text = "Power OFF";
                    break;
                case STRING_POWER_UP: // = 1;
                    txtStringState.Text = "Powering Up";
                    break;
                case STRING_INIT_PING0: // = 2;
                    txtStringState.Text = "INIT Ping 0";
                    break;
                case STRING_INIT_SET_ADDR: // = 3;
                    txtStringState.Text = "INIT Set Addr";
                    break;
                case STRING_INIT_SET_DIR: // = 4;
                    txtStringState.Text = "Init Set Dir";
                    break;
                case STRING_INIT_PING_ADDR: // = 5;
                    txtStringState.Text = "Init Ping Addr";
                    break;
                case STRING_GET_INFO: // = 6
                    txtStringState.Text = "Get Info";
                    break;
                case STRING_ACTIVE: // = 7;
                    txtStringState.Text = "Active";
                    break;
                default:
                    txtStringState.Text = "Unknown";
                    break;
            }
        }
        public void DrawFieldState()
        {
            switch (fieldState)
            {
                case FIELD_OFF: // = 0;
                    txtFieldState.Text = "Field OFF";
                    break;
                case FIELD_PUMP_ON: // = 1;
                    txtFieldState.Text = "Pump ON";
                    break;
                case FIELD_GO_ON_SUN: // = 2;
                    txtFieldState.Text = "Go On Sun";
                    break;
                case FIELD_OPERATE: // = 3;
                    txtFieldState.Text = "Operate";
                    break;
                case FIELD_END_OF_DAY: // = 4;
                    txtFieldState.Text = "End of Day";
                    break;
                case FIELD_TEST: // = 5;
                    txtFieldState.Text = "TEST";
                    break;
                case FIELD_TEST_ON_SUN: // = 6;
                    txtFieldState.Text = "Go On Sun";
                    break;
                case FIELD_TEST_OPERATE: // = 7;
                    txtFieldState.Text = "Operate";
                    break;
                case FIELD_TEST_UPDATE: // = 8;
                    txtFieldState.Text = "Update";
                    break;
                case FIELD_TEST_END_OF_DAY: // = 9;
                    txtFieldState.Text = "Test End of Day";
                    break;
                case FIELD_TEST_OFF: // = 10;
                    txtFieldState.Text = "Test Off";
                    break;
                case FIELD_LOGGING: // = 11;
                    txtFieldState.Text = "Logging";
                    break;
                case FIELD_LOGGING_OFF: // = 12;
                    txtFieldState.Text = "Logging Off";
                    break;
                default:
                    txtFieldState.Text = "Unknown";
                    break;
            }
        }
        public void DrawFCE()
        {
            txtFceInStates.Text = "0x" + rxBuffer[5].ToString("X2");
            txtFceBusIn.Text = "0x" + rxBuffer[6].ToString("X2");
            txtFceBusOut.Text = "0x" + rxBuffer[7].ToString("X2");
            if ((rxBuffer[7] & 0x01) != 0)
            {
                txtFceOut1.Text = "ON";
            }
            else
            {
                txtFceOut1.Text = "OFF";
            }
            if ((rxBuffer[7] & 0x02) != 0)
            {
                txtFceOut2.Text = "ON";
            }
            else
            {
                txtFceOut2.Text = "OFF";
            }
            if ((rxBuffer[7] & 0x04) != 0)
            {
                txtFceOut3.Text = "ON";
            }
            else
            {
                txtFceOut3.Text = "OFF";
            }
            if ((rxBuffer[7] & 0x08) != 0)
            {
                txtFceOut4.Text = "ON";
            }
            else
            {
                txtFceOut4.Text = "OFF";
            }
            int temp;
            temp = rxBuffer[8];
            temp *= 256;
            temp += rxBuffer[9];
            txtFceIn1.Text = temp.ToString();
            temp = rxBuffer[10];
            temp *= 256;
            temp += rxBuffer[11];
            txtFceIn2.Text = temp.ToString();
            temp = rxBuffer[12];
            temp *= 256;
            temp += rxBuffer[13];
            txtFceIn3.Text = temp.ToString();
            temp = rxBuffer[14];
            temp *= 256;
            temp += rxBuffer[15];
            txtFceIn4.Text = temp.ToString();
            temp = rxBuffer[16];
            temp *= 256;
            temp += rxBuffer[17];
            txtFceIn5.Text = temp.ToString();
            temp = rxBuffer[18];
            temp *= 256;
            temp += rxBuffer[19];
            txtFceRtdAD.Text = temp.ToString();
            temp = rxBuffer[20];
            temp *= 256;
            temp += rxBuffer[21];
            txtFceFlowSw.Text = temp.ToString();
            temp = rxBuffer[22];
            temp *= 256;
            temp += rxBuffer[23];
            txtFceDNI.Text = temp.ToString();
            temp = rxBuffer[24];
            temp *= 256;
            temp += rxBuffer[25];
            txtFceRtd.Text = (temp / 10).ToString() + "." + (temp % 10).ToString() + "C";
        }
        public void DrawRTU()
        {
            int addr;
            int val;

            for (int i = 0; i < 10; i++)
            {
                addr = rxBuffer[5 + i * 4];
                addr *= 256;
                addr += rxBuffer[6 + i * 4];
                val = rxBuffer[7 + i * 4];
                val *= 256;
                val += rxBuffer[8 + i * 4];
                if (addr == 40003)
                {
                    txtSys30W.Text = val.ToString();
                }
                if (addr == 40015)
                {
                    txtSys30lph.Text = val.ToString();
                }
                if (addr == 40024)
                {
                    val /= 10;
                    txtSys30Supply.Text = (val / 10).ToString() + "." + (val % 10).ToString() + "C";
                }
                if (addr == 40025)
                {
                    val /= 10;
                    txtSys30Return.Text = (val / 10).ToString() + "." + (val % 10).ToString() + "C";
                }
            }
        }
        public byte WhichStringRadio()
        {
            if (radioStringB.Checked)
            {
                return 1;
            }
            if (radioStringC.Checked)
            {
                return 2;
            }
            if (radioStringD.Checked)
            {
                return 3;
            }
            return 0;
        }
        public void SetStringRadio(byte radio)
        {
            if (radio == 0) radioStringA.Checked = true;
            if (radio == 1) radioStringB.Checked = true;
            if (radio == 2) radioStringC.Checked = true;
            if (radio == 3) radioStringD.Checked = true;
        }

        private void radioStringA_CheckedChanged(object sender, EventArgs e)
        {
            ClearStringInfo();
            ClearParamInfo();
            flagGetString = true;
        }
        public void ClearStringInfo()
        {
            pollMct = 0;
            txtNumMct.Text = "";
            txtStringState.Text = "";
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < NUM_STRING_BOXES; j++)
                {
                    strMctText[i, j].Text = "";
                }
            }
        }
        public void ClearParamInfo()
        {
            if (tabCtrl.SelectedIndex == tabMctParam.TabIndex)
            {
                flagReadAllParam = true;
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < PARAM_NUM_VALUES; j++)
                    {
                        paramText[i, j].Text = "";
                    }
                }
            }
        }

        private void radioStringB_CheckedChanged(object sender, EventArgs e)
        {
            ClearStringInfo();
            ClearParamInfo();
            flagGetString = true;
        }

        private void radioStringC_CheckedChanged(object sender, EventArgs e)
        {
            ClearStringInfo();
            ClearParamInfo();
            flagGetString = true;
        }

        private void radioStringD_CheckedChanged(object sender, EventArgs e)
        {
            ClearStringInfo();
            ClearParamInfo();
            flagGetString = true;
        }

        private void btnFieldOff_Click(object sender, EventArgs e)
        {
            flagSetFieldOff = true;
        }

        private void btnFieldTest_Click(object sender, EventArgs e)
        {
            flagSetFieldTest = true;
        }

        private void btnFieldShutdown_Click(object sender, EventArgs e)
        {
            flagSetFieldShutdown = true;
        }

        private void btnFieldTestOnSun_Click(object sender, EventArgs e)
        {
            flagSetFieldTestOnSun = true;
        }

        private void btnUpdateMctMaster_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            byte stringStart = (byte)cbFwStartString.SelectedIndex;
            byte stringStop = (byte)cbFwStopString.SelectedIndex;
            if (stringStart > stringStop)
            {
                MyMarshalToForm("AddItemToListBox", "Error: StringStart must be <= StringStop");
                return;
            }
            byte mctStart = (byte)cbFwStartMct.SelectedIndex;
            byte mctStop = (byte)cbFwStopMct.SelectedIndex;
            if (mctStart > mctStop)
            {
                MyMarshalToForm("AddItemToListBox", "Error: MctStart must be <= MctStop");
                return;
            }
            //openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "S-Records (*.s19)|*.s19";
            //openFileDialog1.FilterIndex = 1;
            //openFileDialog1.RestoreDirectory = true;

            //if (frmMain.openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //  try
            //{
            if (File.Exists(txtFwMaster.Text))
            {
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_OFF;
                MyMarshalToForm("AddItemToListBox", "Setting field to FIELD_OFF");
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
                Application.DoEvents();
                for (byte stringNum = stringStart; stringNum <= stringStop; stringNum++)
                {
                    SetStringRadio(stringNum);
                    StringWaitState(sender, e, STRING_POWER_OFF);
                }
                System.Threading.Thread.Sleep(200);

                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST_UPDATE;
                MyMarshalToForm("AddItemToListBox", "Setting field to FIELD_UPDATE");
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                for (byte stringNum = 0; stringNum < 4; stringNum++)
                {
                    SetStringRadio(stringNum);
                    StringWaitState(sender, e, STRING_ACTIVE);
                }
                SetStringRadio(stringStart);
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);

                for (byte stringNum = stringStart; stringNum <= stringStop; stringNum++)
                {
                    SetStringRadio(stringNum);
                    SendTxMessage(stringNum, pollMct, MCTCommand.USB_CMD_GET_STRING);
                    SendTxMessage(stringNum, pollMct, MCTCommand.USB_RESP_FIELD_STATE);
                    if (mctStart == 0)
                    {
                        mctStart = 1;
                        mctStop = mctMaxAddr[stringNum];
                    }
                    if (mctStop > mctMaxAddr[stringNum])
                    {
                        mctStop = mctMaxAddr[stringNum];
                    }
                    for (byte mctNum = mctStart; mctNum <= mctStop; mctNum++)
                    {
                        StreamReader sRecord = File.OpenText("Files" + txtFwMaster.Text);
                        FwMctMaster(sender, e, stringNum, mctNum, sRecord);
                        sRecord.Close();
                    }
                }
                MyMarshalToForm("AddItemToListBox", "Update MCT Master Firmware complete.");
            }
            else
            {
                MyMarshalToForm("AddItemToListBox", "Error: Could not read file from disk");
            }
            //}
            //catch (Exception ex)
            //{
            //MyMarshalToForm("AddItemToListBox", "Error: Could not read file from disk. Original error: " + ex.Message);
            //}
            //}

        }
        public void FwMctMaster(object sender, EventArgs e, byte stringNum, byte mctNum, StreamReader sRec)
        {
            MyMarshalToForm("AddItemToListBox", "Updating String " + (char)('A' + stringNum) + ", MCT #" + mctNum.ToString());
            // Jump to Boot flash
            MyMarshalToForm("AddItemToListBox", "Jump to Boot");

            txBuffer = MCTCommand.JUMP_TO_BOOT(mctNum, txPid);
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);

            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            // erase flash
            updatingMctFlash = true; // stop polling

            txBuffer = MCTCommand.ERASE_APP(mctNum, txPid);
            MyMarshalToForm("AddItemToListBox", "Erasing App flash");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);

            System.Threading.Thread.Sleep(2000);
            Application.DoEvents();

            String line;
            int tries;
            int addr;
            byte flash;
            MyMarshalToForm("AddItemToListBox", "Writing App flash");
            Application.DoEvents();

            // clear  copy of the flash memory. debugging(?)
            for (addr = 0x7C00; addr < 0x10000; addr++)
            {
                mctFlashMem[addr] = (byte)0xFF;
            }

            while ((line = sRec.ReadLine()) != null)
            {
                char[] record = line.ToCharArray(0, line.Length);
                if (record[0] == 'S' && record[1] == '1' && line.Length >= 12)
                {
                    for (tries = 0; tries < 3; tries++)
                    {

                        txBuffer = MCTCommand.PROG_APP(txPid, mctNum, ref record, ref mctFlashMem);                        
                        string address = "Writing Address: " + line.Substring(4, 4);
                        MyMarshalToForm("AddItemToListBox", address);

                        SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
                        Mct485WaitResp(stringNum, mctNum);

                        if (rxBuffer[ MCTCommand.USB_PACKET_LEN] != 0x11 && rxBuffer[10] != 1)
                        {
                            if (tries < 2)
                            {
                                MyMarshalToForm("AddItemToListBox", "retry - Flash Write @" + record[4] + record[5] + record[6] + record[7]);
                            }
                            else
                            {
                                MyMarshalToForm("AddItemToListBox", "ERROR - Flash Write @" + record[4] + record[5] + record[6] + record[7]);
                                return;
                            }
                        }
                        else
                        {
                            tries = 3;
                        }
                    }
                }
            }

            // calculate cksum
            UInt32 cksum = 0xFFFFFFFF;
            for (addr = GetFlashUShort(APP_START_VECTOR); addr < BOOT_START; addr += 4)
            {
                cksum -= GetFlashUInt32(addr);
            }

            txBuffer = MCTCommand.PROG_APP_CHECKSUM(txPid, mctNum, cksum);
            
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            //txBuffer[5] = mctNum;
            //txBuffer[6] = txPid;
            //txBuffer[7] = 6;
            //txBuffer[8] = MCTCommand.MCT_CMD_FLASH_CKSUM;
            //TxCrcCalc(5, 4);
            //txBuffer[ MCTCommand.USB_PACKET_MCT] = mctNum;
            //txBuffer[ MCTCommand.USB_PACKET_LEN] = 13;
            //MyMarshalToForm("AddItemToListBox", "Calculating App Cksum");
            //SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            //Mct485WaitResp(stringNum, mctNum);
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000);

            // restart app
            txBuffer = MCTCommand.JUMP_TO_APP(mctNum, txPid);
            MyMarshalToForm("AddItemToListBox", "Restarting MCTmaster App");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);

            Application.DoEvents();
            System.Threading.Thread.Sleep(100);
            
            updatingMctFlash = false; // restart polling
        }

        public ushort GetFlashUShort(int addr)
        {
            ushort val = mctFlashMem[addr];
            val <<= 8;
            val += mctFlashMem[addr+1];
            return val;
        }
        public UInt32 GetFlashUInt32(int addr)
        {
            UInt32 val = mctFlashMem[addr];
            val <<= 8;
            val += mctFlashMem[addr + 1];
            val <<= 8;
            val += mctFlashMem[addr + 2];
            val <<= 8;
            val += mctFlashMem[addr + 3];
            return val;
        }

        private void btnUpdateMctSlave_Click(object sender, EventArgs e)
        {
            byte stringStart = (byte)cbFwStartString.SelectedIndex;
            byte stringStop = (byte)cbFwStopString.SelectedIndex;
            if (stringStart > stringStop)
            {
                MyMarshalToForm("AddItemToListBox", "Error: StringStart must be <= StringStop");
                return;
            }
            byte mctStart = (byte)cbFwStartMct.SelectedIndex;
            byte mctStop = (byte)cbFwStopMct.SelectedIndex;
            if (mctStart > mctStop)
            {
                MyMarshalToForm("AddItemToListBox", "Error: MctStart must be <= MctStop");
                return;
            }

            if (File.Exists(txtFwSlave.Text))
            {
                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_OFF;
                MyMarshalToForm("AddItemToListBox", "Setting field to FIELD_OFF");
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);

                txBuffer[ MCTCommand.USB_PACKET_DATA] = FIELD_TEST_UPDATE;
                MyMarshalToForm("AddItemToListBox", "Setting field to FIELD_UPDATE");
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_FIELD_STATE);

                for (byte stringNum = stringStart; stringNum <= stringStop; stringNum++)
                {
                    SetStringRadio(stringNum);
                    StringWaitState(sender, e, STRING_ACTIVE);
                }

                for (byte stringNum = stringStart; stringNum <= stringStop; stringNum++)
                {
                    SendTxMessage(stringNum, pollMct, MCTCommand.USB_CMD_GET_STRING);
                    SendTxMessage(stringNum, pollMct, MCTCommand.USB_RESP_FIELD_STATE);
                    if (mctStart == 0)
                    {
                        mctStart = 1;
                        mctStop = mctMaxAddr[stringNum];
                    }
                    if (mctStop > mctMaxAddr[stringNum])
                    {
                        mctStop = mctMaxAddr[stringNum];
                    }
                    for (byte mctNum = mctStart; mctNum <= mctStop; mctNum++)
                    {
                        StreamReader sRecord = File.OpenText("\\Files\\" + txtFwSlave.Text);
                        FwMctSlave(sender, e, stringNum, mctNum, sRecord);
                        sRecord.Close();
                    }
                }
                MyMarshalToForm("AddItemToListBox", "Update MCT Slave Firmware complete.");
            }
            else
            {
                MyMarshalToForm("AddItemToListBox", "Error: Could not read file from disk");
            }
        }
        public void FwMctSlave(object sender, EventArgs e, byte stringNum, byte mctNum, StreamReader sRec)
        {
            MyMarshalToForm("AddItemToListBox", "Updating String " + (char)('A' + stringNum) + ", MCT #" + mctNum.ToString());

            // stop slave polling
            txBuffer = MCTCommand.SLAVE_MODE(mctNum, txPid, 0x80);
            MyMarshalToForm("AddItemToListBox", "Stop slave polling");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            // Jump to Boot flash
            txBuffer = MCTCommand.SLAVE_JUMP_TO_BOOT(mctNum, txPid);
            MyMarshalToForm("AddItemToListBox", "Slave Jump to Boot");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            System.Threading.Thread.Sleep(100);
            Application.DoEvents();

            // erase flash
            txBuffer = MCTCommand.SLAVE_ERASE_APP(mctNum, txPid);

            MyMarshalToForm("AddItemToListBox", "Erasing Slave App flash");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);

            System.Threading.Thread.Sleep(1000);
            Application.DoEvents();

            String line;
            MyMarshalToForm("AddItemToListBox", "Writing Slave App flash");
            Application.DoEvents();

            //Send each line of the source file as a packet and wait for response from each.
            while ((line = sRec.ReadLine()) != null)
            {
                char[] record = line.ToCharArray(0, line.Length);
                // make sure header of the file is reasonable
                if (record[0] == 'S' && record[1] == '1' && line.Length >= 12)
                {
                    // data
                    txBuffer = MCTCommand.SLAVE_PROG_APP(txPid, mctNum, ref record);
                    string address = "Writing Address: " + line.Substring(4, 4);
                    MyMarshalToForm("AddItemToListBox", address);
                    SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
                    Mct485WaitResp(stringNum, mctNum);
                }
            }

            // All done writing new app, 
            // ask for checksum

            txBuffer = MCTCommand.SLAVE_FLASH_CKSUM(txPid, mctNum);        
            MyMarshalToForm("AddItemToListBox", "Calculating Slave App Cksum");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000); // Why does it sleep, precious?

            // restart app

            txBuffer = MCTCommand.SLAVE_JUMP_TO_APP(txPid, mctNum);

            MyMarshalToForm("AddItemToListBox", "Restarting MCTslave App");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            // restart slave polling
            txBuffer = MCTCommand.SLAVE_MODE(mctNum, txPid, 0x1f);
            MyMarshalToForm("AddItemToListBox", "Restarting slave polling");
            SendTxMessage(stringNum, mctNum, MCTCommand.USB_CMD_SEND_MCT485);
            Mct485WaitResp(stringNum, mctNum);
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);
        }

        private void btnHomeString_Click(object sender, EventArgs e)
        {
            flagHomeString = true;
        }

        private void btnStowString_Click(object sender, EventArgs e)
        {
            flagStowString = true;
        }

        private void cbMctPollAddr_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void tabCtrl_TabIndexChanged(object sender, EventArgs e)
        {
            flagGetString = true;
        }
        public void SetTabMctAddrCombo(int addr)
        {
            cbMctPollAddr.Items.Clear();
            for (int i = 1; i <= addr; i++)
            {
                cbMctPollAddr.Items.Add(i.ToString());
            }
            if (addr > 0)
            {
                cbMctPollAddr.SelectedIndex = addr - 1;
            }
        }

        private void cbMctTrack1_SelectedIndexChanged(object sender, EventArgs e)
        {
            flagMctChangeTrack1 = true;
        }

        private void btnMctStow1_Click(object sender, EventArgs e)
        {
            flagMctStow1 = true;
        }

        private void btnMctHome1_Click(object sender, EventArgs e)
        {
            flagMctHome1 = true;
        }

        private void btnMctMoveDeg1_Click(object sender, EventArgs e)
        {
            flagMctMoveDeg1 = true;
        }

        private void btnMctMoveSteps1_Click(object sender, EventArgs e)
        {
            flagMctMoveSteps1 = true;
        }

        private void cbMctTrack2_SelectedIndexChanged(object sender, EventArgs e)
        {
            flagMctChangeTrack2 = true;
        }

        private void btnMctStow2_Click(object sender, EventArgs e)
        {
            flagMctStow2 = true;
        }

        private void btnMctHome2_Click(object sender, EventArgs e)
        {
            flagMctHome2 = true;
        }

        private void btnMctMoveDeg2_Click(object sender, EventArgs e)
        {
            flagMctMoveDeg2 = true;
        }

        private void btnMctMoveSteps2_Click(object sender, EventArgs e)
        {
            flagMctMoveSteps2 = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            flagSetFieldOff = true;
        }

        private void btnMctSetFieldTest_Click(object sender, EventArgs e)
        {
            flagSetFieldTest = true;
        }

        private void tabCtrl_SelectedIndexChanged(object sender, EventArgs e)
        {
            flagSetFieldOff = false;
            flagSetFieldTest = false;
            flagSetFieldShutdown = false;
            flagSetFieldTestOnSun = false;
            flagHomeString = false;
            flagStowString = false;
            flagMctChangeTrack1 = false;
            flagMctHome1 = false;
            flagMctStow1 = false;
            flagMctMoveSteps1 = false;
            flagMctMoveDeg1 = false;
            flagMctChangeTrack2 = false;
            flagMctHome2 = false;
            flagMctStow2 = false;
            flagMctMoveSteps2 = false;
            flagMctMoveDeg2 = false;
            flagGetString = true;
            if (tabCtrl.SelectedIndex == tabMctParam.TabIndex)
            {
                ClearParamInfo();
                flagReadAllParam = true;
            }
            if (tabCtrl.SelectedIndex == tabString.TabIndex)
            {
                ClearStringInfo();
            }
            else if (tabCtrl.SelectedIndex == tabMctControl.TabIndex)
            {
                SetTabMctAddrCombo(mctMaxAddr[WhichStringRadio()]);
                cbMctTrack1.Items.Clear();
                cbMctTrack2.Items.Clear();
                for (int i = 0; i < numTrackEntries; i++)
                {
                    cbMctTrack1.Items.Add(trackName[i]);
                    cbMctTrack2.Items.Add(trackName[i]);
                }
                if (cbMctPollAddr.Items.Count > 0)
                {
                    if (cbMctPollAddr.SelectedIndex < 0)
                    {
                        cbMctPollAddr.SelectedIndex = 0;
                    }
                }
            }
        }

        private void btnWriteAllParam_Click(object sender, EventArgs e)
        {
            flagWriteAllParam = true;
        }

        private void btnRealAllParam_Click(object sender, EventArgs e)
        {
            flagReadAllParam = true;
        }
        public Boolean StringWaitState(object sender, EventArgs e, byte newState)
        {
            for (int count = 0; count < 12000; count++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
                SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_CMD_GET_STRING);
                if (stringState[WhichStringRadio()] == newState || stringState[WhichStringRadio()] == STRING_POWER_OFF)
                {
                    SendTxMessage(WhichStringRadio(), pollMct, MCTCommand.USB_RESP_FIELD_STATE);
                    return true;
                }
            }
            return false;
        }
        public Boolean StringWaitStateAll(object sender, EventArgs e, byte newState)
        {
            for (byte stringNum = 0; stringNum < 4; stringNum++) {
                for (int count = 0; count < 5000; count++)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(1);
                    SendTxMessage(stringNum, pollMct, MCTCommand.USB_CMD_GET_STRING);
                    if (stringState[stringNum] == newState || stringState[stringNum] == STRING_POWER_OFF)
                    {
                        SendTxMessage(stringNum, pollMct, MCTCommand.USB_RESP_FIELD_STATE);
                        if (stringNum == 3)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public Boolean Mct485WaitResp(byte str, byte mct)
        {
            for (int count = 0; count < 500; count++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
                SendTxMessage(str, mct, MCTCommand.USB_CMD_GET_MCT485);
                if (rxBuffer[ MCTCommand.USB_PACKET_LEN] > 7)
                {
                    return true;
                }
            }
            return false;
        }

        private void btnGetMctVersions_Click(object sender, EventArgs e)
        {
            flagGetMctVersions = true;
            countGetMctVersions = 0;
        }
        public void DrawError(int err, TextBox tb)
        {
            for (int i = 0; i < numErrorEntries; i++)
            {
                if (err == errorNum[i])
                {
                    tb.Text = errorName[i];
                    return;
                }
            }
            tb.Text = "#" + err.ToString();
        }

        private void btnSetClock_Click(object sender, EventArgs e)
        {
            flagSetClock = true;
        }

        private void btnSetA_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[0, i].Checked = true;
            }
        }

        private void btnSetB_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[1, i].Checked = true;
            }
        }

        private void btnSetC_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[2, i].Checked = true;
            }
        }

        private void btnSetD_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[3, i].Checked = true;
            }
        }

        private void btnClrA_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[0, i].Checked = false;
            }
        }

        private void btnClrB_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[1, i].Checked = false;
            }
        }

        private void btnClrC_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[2, i].Checked = false;
            }
        }

        private void btnClrD_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                checkMctStore[3, i].Checked = false;
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {

            string logDirectory = "Logs\\"; //subfolder from application current directory
            Directory.CreateDirectory(logDirectory);

            if (btnStartStop.Text == "Start")
            {
                // turn on logging mode
                flagSetFieldLogging = true;
                // collect a cycle of data first
                pollDataValid = false;
                // convert sec to msec
                intervalMct = (int)numMctLogRate.Value * 1000;
                if (radioMctMinutes.Checked)
                {
                    intervalMct *= 60;
                }
                dateTimeMct = DateTime.Now.AddMilliseconds(intervalMct);
                // convert sec to msec
                intervalSfc = (int)numSfcLogRate.Value * 1000;
                if (radioSfcMinutes.Checked)
                {
                    intervalSfc *= 60;
                }
                dateTimeSfc = DateTime.Now.AddMilliseconds(intervalSfc);
                // open files
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (checkMctStore[i, j].Checked)
                        {
                            // close open files
                            string fName = logDirectory + "MCT" + (j + 1).ToString() + " Str" + (char)((byte)'A' + i) + ' ';
                            fName += DateTime.Now.Year.ToString() + '-';
                            fName += DateTime.Now.Month.ToString() + '-';
                            fName += DateTime.Now.Day.ToString() + ".txt";
                            mctFile[i, j] = File.AppendText(fName);
                            WriteMctHeader(i, j);
                        }
                    }
                }
                WriteMctFiles();
                string sfcName = logDirectory + "SFC data ";
                sfcName += DateTime.Now.Year.ToString() + '-';
                sfcName += DateTime.Now.Month.ToString() + '-';
                sfcName += DateTime.Now.Day.ToString() + ".txt";
                sfcFile = File.AppendText(sfcName);
                WriteSfcHeader();
                SetControlStates(false);
                lstResults.Items.Add("Starting data collection");
                btnStartStop.Text = "Stop";
            }
            else
            {
                flagSetFieldLoggingOff = true;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        // close open files
                        if (checkMctStore[i, j].Checked)
                        {
                            mctFile[i, j].Close();
                        }
                    }
                }
                sfcFile.Close();
                SetControlStates(true);
                lstResults.Items.Add("Stopping data collection");
                btnStartStop.Text = "Start";
            }
        }
        public void SetControlStates(Boolean enable)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    checkMctStore[i, j].Enabled = enable;
                }
            }
            numMctLogRate.Enabled = enable;
            radioMctMinutes.Enabled = enable;
            radioMctSeconds.Enabled = enable;
            numSfcLogRate.Enabled = enable;
            radioSfcMinutes.Enabled = enable;
            radioSfcSeconds.Enabled = enable;
            checkRecTrackState.Enabled = enable;
            checkRecPosn.Enabled = enable;
            checkRecTrackRTD.Enabled = enable;
            checkRecManRTD.Enabled = enable;
            checkRecPCBTemp.Enabled = enable;
            checkRecHumidity.Enabled = enable;
            checkRecSensors.Enabled = enable;
            checkRecFCE.Enabled = enable;
            checkRecSys30.Enabled = enable;
            checkDNI.Enabled = enable;
            checkRecSFCTemp.Enabled = enable;
            btnClrA.Enabled = enable;
            btnClrB.Enabled = enable;
            btnClrC.Enabled = enable;
            btnClrD.Enabled = enable;
            btnSetA.Enabled = enable;
            btnSetB.Enabled = enable;
            btnSetC.Enabled = enable;
            btnSetD.Enabled = enable;
            // end of NightMode changes
        }
        public void WriteMctHeader(int i, int j)
        {
            string header;

            header = "Date/Time";
            if (checkRecTrackState.Checked)
            {
                header += ",Track State 1,Track State 2";
            }
            if (checkRecPosn.Checked)
            {
                header += ",POSN1A,POSN1B,POSN2A,POSN2B";
            }
            if (checkRecTrackRTD.Checked)
            {
                header += ",RTD1AL,RTD1AR,RTD1BL,RTD1BR,RTD2AL,RTD2AR,RTD2BL,RTD2BR";
            }
            if (checkRecManRTD.Checked)
            {
                header += ",MAN1,MAN2";
            }
            if (checkRecPCBTemp.Checked)
            {
                header += ",TEMP A,TEMP B";
            }
            if (checkRecHumidity.Checked)
            {
                header += ",HUMID 1,HUMID 2";
            }
            if (checkRecSensors.Checked)
            {
                header += ",SENSOR 1,SENSOR 2";
            }
            mctFile[i, j].WriteLine(header);
        }
        public void WriteSfcHeader()
        {
            string header;

            header = "Date/Time";
            if (checkRecSys30.Checked)
            {
                header += ",Watts,L/Hr,Inlet Temp, Outlet Temp";
            }
            if (checkRecFCE.Checked)
            {
                header += ",FCE Inputs,FCE Outputs";
            }
            if (checkDNI.Checked)
            {
                header += ",W/m2";
            }
            if (checkRecSFCTemp.Checked)
            {
                header += ",SFC Temp,Enc Temp,Enc Humidity";
            }
            sfcFile.WriteLine(header);
            WriteSfcFiles();
        }
        public void WriteMctFiles()
        {
            int i, j, k;
            string header;

            // NightMode
            if (fieldState == FIELD_OFF)
            {
                // nothing to log while MCT's are off
                return;
            }

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 10; j++)
                {
                    if (mctMaxAddr[i] > j && checkMctStore[i, j].Checked)
                    {
                        //header = DateTime.Now.Year.ToString() + '-';
                        //header += DateTime.Now.Month.ToString() + '-';
                        //header += DateTime.Now.Day.ToString() + ' ';
                        //header += DateTime.Now.Hour.ToString() + ':';
                        //header += DateTime.Now.Minute.ToString() + ':';
                        //header += DateTime.Now.Second.ToString();
                        header = DateTime.Now.ToString("G");
                        if (checkRecTrackState.Checked)
                        {
                            for (k = 0; k < numTrackEntries; k++)
                            {
                                if (mctTrack[i, j, MIRROR1] == trackNum[k])
                                {
                                    header += "," + trackName[k];
                                    break;
                                }
                            }
                            if (k == numTrackEntries)
                            {
                                header += "," + mctTrack[i, j, MIRROR1].ToString();
                            }
                            for (k = 0; k < numTrackEntries; k++)
                            {
                                if (mctTrack[i, j, MIRROR2] == trackNum[k])
                                {
                                    header += "," + trackName[k];
                                    break;
                                }
                            }
                            if (k == numTrackEntries)
                            {
                                header += "," + mctTrack[i, j, MIRROR2].ToString();
                            }
                        }
                        if (checkRecPosn.Checked)
                        {
                            for (k = 0; k < 4; k++)
                            {
                                header += "," + DrawMdeg(mctPosn[i, j, k]);
                            }
                        }
                        if (checkRecTrackRTD.Checked)
                        {
                            //header += ",RTD1AL,RTD1AR,RTD1BL,RTD1BR,RTD2AL,RTD2AR,RTD2BL,RTD2BR";
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKLEFT_1A]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKRIGHT_1A]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKLEFT_1B]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKRIGHT_1B]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKLEFT_2A]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKRIGHT_2A]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKLEFT_2B]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_TRACKRIGHT_2B]);
                        }
                        if (checkRecManRTD.Checked)
                        {
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_MANIFOLD_1]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_RTD_MANIFOLD_2]);
                        }
                        if (checkRecPCBTemp.Checked)
                        {
                            header += "," + DrawDeg(mctChan[i, j, MCT_LOCAL_TEMPA]);
                            header += "," + DrawDeg(mctChan[i, j, MCT_LOCAL_TEMPB]);
                        }
                        if (checkRecHumidity.Checked)
                        {
                            header += "," + DrawHumid(mctChan[i, j, MCT_HUMIDITY1]);
                            header += "," + DrawHumid(mctChan[i, j, MCT_HUMIDITY2]);
                        }
                        if (checkRecSensors.Checked)
                        {
                            header += "," + mctSensor[i, j, MIRROR1].ToString("X2");
                            header += "," + mctSensor[i, j, MIRROR2].ToString("X2");
                        }
                        mctFile[i, j].WriteLine(header);
                        mctFile[i, j].Flush();
                    }
                }
            }
        }
        public void WriteSfcFiles()
        {
            string header;

            header = DateTime.Now.ToString("G");
            if (checkRecSys30.Checked)
            {
                //header += ",Watts,L/Hr,Inlet Temp, Outlet Temp";
                header += "," + rtu[0].ToString();
                header += "," + rtu[1].ToString();
                header += "," + (rtu[2] / 100).ToString() + "." + (rtu[2] % 100).ToString("D2");
                header += "," + (rtu[3] / 100).ToString() + "." + (rtu[2] % 100).ToString("D2");
            }
            if (checkRecFCE.Checked)
            {
                //header += ",FCE Inputs,FCE Outputs";
                header += "," + fceInputState.ToString("X2");
                header += "," + fceOutputState.ToString("X2");
            }
            if (checkDNI.Checked)
            {
                //header += ",W/m2";
                header += "," + fieldDNI.ToString();
            }
            if (checkRecSFCTemp.Checked)
            {
                //header += ",SFC Temp,Enc Temp,Enc Humidity";
                header += "," + (sfcPcbTemp / 10).ToString() + "." + (sfcPcbTemp % 10).ToString() + "C";
                if (sfcThermistor == 32767)
                {
                    header += ",short";
                }
                else if (sfcThermistor == -32768)
                {
                    header += ",open";
                }
                else
                {
                    header += "," + (sfcThermistor / 10).ToString() + "." + (sfcThermistor % 10).ToString() + "C";
                }
                if (sfcHumidity > 0 && sfcHumidity < 1001)
                {
                    header += "," + (sfcHumidity / 10).ToString() + "." + (sfcHumidity % 10).ToString() + "%";
                }
                else
                {
                    header += ",open";
                }
            }
            sfcFile.WriteLine(header);
            sfcFile.Flush();
        }
        public int RespChan(int index)
        {
            int chan = rxBuffer[index];
            chan *= 256;
            chan += rxBuffer[index + 1];
            if ((chan & 0x8000) != 0)
            {
                chan -= 65536;
            }
            return chan;
        }
        public void PollNextItem()
        {
            if (!cmdComplete)
            {
                return;
            }
            switch (pollState)
            {
                case POLL_LOG_FIELD_STATE: // 0
                    SendTxMessage(0, 0, MCTCommand.USB_RESP_FIELD_STATE);
                    pollState = POLL_LOG_FCE;
                    break;

                case POLL_LOG_FCE:      // 1
                    SendTxMessage(0, 0, MCTCommand.USB_CMD_GET_FCE);
                    pollState = POLL_LOG_RTU;
                    break;

                case POLL_LOG_RTU:      // 2
                    SendTxMessage(0, 0, MCTCommand.USB_CMD_GET_RTU);
                    pollState = POLL_LOG_SFC_TEMP;
                    break;

                case POLL_LOG_SFC_TEMP: // 3
                    pollState = POLL_LOG_STRING;
                    pollStringNum = 0;
                    break;

                case POLL_LOG_STRING:   // 4
                    SendTxMessage(pollStringNum, 0, MCTCommand.USB_CMD_GET_STRING);
                    pollStringNum++;
                    if (pollStringNum >= 4)
                    {
                        pollStringNum = 0;
                        pollMctNum = 0;
                        if (!checkMctStore[0, 0].Checked || mctMaxAddr[0] == 0)
                        {
                            pollState = POLL_LOG_FIELD_STATE;
                        }
                        else
                        {
                            pollState = POLL_LOG_MCT_POSN;
                        }
                    }
                    break;

                case POLL_LOG_MCT_POSN: // 5
                    SendTxMessage(pollStringNum, (byte)(pollMctNum + 1), MCTCommand.USB_CMD_GET_MIRRORS);
                    if (!NextPollMct())
                    {
                        pollState = POLL_LOG_MCT_CHAN;
                    }
                    break;

                case POLL_LOG_MCT_CHAN: // 6
                    SendTxMessage(pollStringNum, (byte)(pollMctNum+1), MCTCommand.USB_CMD_GET_CHAN);
                    if (!NextPollMct())
                    {
                        pollState = POLL_LOG_FIELD_STATE;
                        pollDataValid = true;
                    }
                    break;

                default:
                    pollState = POLL_LOG_FIELD_STATE;
                    break;
            }
        }
        public Boolean NextPollMct()
        {
            pollMctNum++;
            while (pollStringNum < 4)
            {
                while (pollMctNum < mctMaxAddr[pollStringNum])
                {
                    if (checkMctStore[pollStringNum, pollMctNum].Checked)
                    {
                        // found next MCT to poll
                        return true;
                    }
                    pollMctNum++;
                }
                pollStringNum++;
                pollMctNum = 0;
            }
            // wrapped around, done
            pollStringNum = 0;
            pollMctNum = 0;
            return false;
        }
        public string DrawMdeg(int n)
        {
            string s;

            s = (n / 100).ToString() + "." + (n % 100).ToString("D2");

            return s;
        }
        public string DrawDeg(int n)
        {
            string s;

            if (n < 32767)
            {
                s = (n / 10).ToString() + "." + (n % 10).ToString();
            }
            else
            {
                s = "N/A";
            }

            return s;
        }
        public string DrawHumid(int n)
        {
            string s;

            if (n <= 1000)
            {
                s = (n / 10).ToString() + "." + (n % 10).ToString() + "%";
            }
            else
            {
                s = "N/A";
            }

            return s;
        }

        private void btnFieldTestOff_Click(object sender, EventArgs e)
        {
            flagSetFieldTestOff = true;
        }

        private void btnFieldTestShutdown_Click(object sender, EventArgs e)
        {
            flagSetFieldTestShutdown = true;
        }

        private void cbFan_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }

        private void cbValve_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }

        private void cbHeat_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }
        public void DesiccantNewValues() {
            desiccantNewState = DESICCANT_MANUAL;
            desiccantNewOutputs = 0;
            if (cbFan.Checked)
                desiccantNewOutputs |= FCE_OUT_FAN;
            if (cbValve.Checked)
                desiccantNewOutputs |= FCE_OUT_VALVE;
            if (cbHeat.Checked)
                desiccantNewOutputs |= FCE_OUT_HEAT;
        }

        private void radioDesOff_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            desiccantNewState = DESICCANT_OFF;
            flagSetDesiccant = true;
        }

        private void radioDesDrying_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            desiccantNewState = DESICCANT_DRYING;
            flagSetDesiccant = true;
        }

        private void radioDesRegen_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            desiccantNewState = DESICCANT_REGEN;
            flagSetDesiccant = true;
        }

        private void radioDesClosed_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            desiccantNewState = DESICCANT_CLOSED;
            flagSetDesiccant = true;
        }

        private void radioDesManual_CheckedChanged(object sender, EventArgs e)
        {
            if (flagDesiccantResp)
                // don't send out a message if the change was due to an SFC response
                return;
            desiccantNewState = DESICCANT_MANUAL;
            flagSetDesiccant = true;
        }

        private void btnFan_Click(object sender, EventArgs e)
        {
            cbFan.Checked = !cbFan.Checked;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }

        private void btnValve_Click(object sender, EventArgs e)
        {
            cbValve.Checked = !cbValve.Checked;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }

        private void btnHeat_Click(object sender, EventArgs e)
        {
            cbHeat.Checked = !cbHeat.Checked;
            DesiccantNewValues();
            flagSetDesiccant = true;
        }

        private void btnDesReadTimes_Click(object sender, EventArgs e)
        {
            paramStartRead = PARAM_DESICCANT_T1;
            paramStopRead = PARAM_DESICCANT_T3;
        }

        private void btnDesWriteTimes_Click(object sender, EventArgs e)
        {
            sfcParam[PARAM_DESICCANT_T1] = (ulong)(cbDryingHr.SelectedIndex * 256 + cbDryingMin.SelectedIndex * 5);
            sfcParam[PARAM_DESICCANT_T2] = (ulong)(cbRegenHr.SelectedIndex * 256 + cbRegenMin.SelectedIndex * 5);
            sfcParam[PARAM_DESICCANT_T3] = (ulong)(cbClosedHr.SelectedIndex * 256 + cbClosedMin.SelectedIndex * 5);
            paramStartWrite = PARAM_DESICCANT_T1;
            paramStopWrite = PARAM_DESICCANT_T3+1;
        }
        public void UpdateParam(int paramNum)
        {
            switch (paramNum)
            {
                case PARAM_DESICCANT_T1:
                    if ((sfcParam[paramNum] >> 8) < 24)
                    {
                        cbDryingHr.SelectedIndex = (int)(sfcParam[paramNum] >> 8);
                    }
                    if ((sfcParam[paramNum] & 0xFF) < 60)
                    {
                        cbDryingMin.SelectedIndex = (int)((sfcParam[paramNum] & 0xFF)/ 5);
                    }
                    break;
                case PARAM_DESICCANT_T2:
                    if ((sfcParam[paramNum] >> 8) < 24)
                    {
                        cbRegenHr.SelectedIndex = (int)(sfcParam[paramNum] >> 8);
                    }
                    if ((sfcParam[paramNum] & 0xFF) < 60)
                    {
                        cbRegenMin.SelectedIndex = (int)((sfcParam[paramNum] & 0xFF) / 5);
                    }
                    break;
                case PARAM_DESICCANT_T3:
                    if ((sfcParam[paramNum] >> 8) < 24)
                    {
                        cbClosedHr.SelectedIndex = (int)(sfcParam[paramNum] >> 8);
                    }
                    if ((sfcParam[paramNum] & 0xFF) < 60)
                    {
                        cbClosedMin.SelectedIndex = (int)((sfcParam[paramNum] & 0xFF)/ 5);
                    }
                    break;
                case PARAM_DESICCANT_REGEN_HUMIDITY:
                    txtDesRegenHumid.Text = (sfcParam[PARAM_DESICCANT_REGEN_HUMIDITY] / 10).ToString() +
                                           "." +
                                           (sfcParam[PARAM_DESICCANT_REGEN_HUMIDITY] % 10).ToString();
                    break;
                case PARAM_DESICCANT_DUTY1_TEMP:
                    txtDesTempDuty1.Text = (sfcParam[PARAM_DESICCANT_DUTY1_TEMP] / 10).ToString() +
                                           "." +
                                           (sfcParam[PARAM_DESICCANT_DUTY1_TEMP] % 10).ToString();
                    break;
                case PARAM_DESICCANT_DUTY2_TEMP:
                    txtDesTempDuty2.Text = (sfcParam[PARAM_DESICCANT_DUTY2_TEMP] / 10).ToString() +
                                           "." +
                                           (sfcParam[PARAM_DESICCANT_DUTY2_TEMP] % 10).ToString();
                    break;
                case PARAM_DESICCANT_DUTY3_TEMP:
                    txtDesTempDuty3.Text = (sfcParam[PARAM_DESICCANT_DUTY3_TEMP] / 10).ToString() +
                                           "." +
                                           (sfcParam[PARAM_DESICCANT_DUTY3_TEMP] % 10).ToString();
                    break;
                case PARAM_DESICCANT_MIN_FAN_TEMP:
                    txtTempMinFan.Text = (sfcParam[PARAM_DESICCANT_MIN_FAN_TEMP] / 10).ToString() +
                                           "." +
                                           (sfcParam[PARAM_DESICCANT_MIN_FAN_TEMP] % 10).ToString();
                    break;
            }
        }

        private void btnReadTemps_Click(object sender, EventArgs e)
        {
            paramStartRead = PARAM_DESICCANT_REGEN_HUMIDITY;
            paramStopRead = PARAM_DESICCANT_MIN_FAN_TEMP + 1;
        }

        private void btnWriteTimes_Click(object sender, EventArgs e)
        {
            sfcParam[PARAM_DESICCANT_REGEN_HUMIDITY] = MyParseUInt32(txtDesRegenHumid.Text);
            sfcParam[PARAM_DESICCANT_DUTY1_TEMP] = MyParseUInt32(txtDesTempDuty1.Text);
            sfcParam[PARAM_DESICCANT_DUTY2_TEMP] = MyParseUInt32(txtDesTempDuty2.Text);
            sfcParam[PARAM_DESICCANT_DUTY3_TEMP] = MyParseUInt32(txtDesTempDuty3.Text);
            sfcParam[PARAM_DESICCANT_MIN_FAN_TEMP] = MyParseUInt32(txtTempMinFan.Text);
            paramStartWrite = PARAM_DESICCANT_REGEN_HUMIDITY;
            paramStopWrite = PARAM_DESICCANT_MIN_FAN_TEMP + 1;
        }
        UInt32 MyParseUInt32(string s)
        {
            UInt32 val;

            try
            {
                val = (UInt32)(10 * float.Parse(s));
            }
            catch (FormatException)
            {
                val = 0;
            }
            return val;
        }

        private void btnWriteParam1_Click(object sender, EventArgs e)
        {
            flagWriteParam = 1;
        }

        private void btnWriteParam2_Click(object sender, EventArgs e)
        {
            flagWriteParam = 2;
        }

        private void btnWriteParam3_Click(object sender, EventArgs e)
        {
            flagWriteParam = 3;
        }

        private void btnWriteParam4_Click(object sender, EventArgs e)
        {
            flagWriteParam = 4;
        }

        private void btnWriteParam5_Click(object sender, EventArgs e)
        {
            flagWriteParam = 5;
        }

        private void btnWriteParam6_Click(object sender, EventArgs e)
        {
            flagWriteParam = 6;
        }

        private void btnWriteParam7_Click(object sender, EventArgs e)
        {
            flagWriteParam = 7;
        }

        private void btnWriteParam8_Click(object sender, EventArgs e)
        {
            flagWriteParam = 8;
        }

        private void btnWriteParam9_Click(object sender, EventArgs e)
        {
            flagWriteParam = 9;
        }

        private void btnWriteParam10_Click(object sender, EventArgs e)
        {
            flagWriteParam = 10;
        }

        private void btnRamDump_Click(object sender, EventArgs e)
        {
            flagRamDump = true;
        }

        private void btnTestMct485_Click(object sender, EventArgs e)
        {
            flagTestBtn = true;
            MyMarshalToForm("AddItemToListBox", "Test - MCT485 Init");
            testBtnValue = 1;
        }

        private void btnTestString_Click(object sender, EventArgs e)
        {
            flagTestBtn = true;
            MyMarshalToForm("AddItemToListBox", "Test - String Init");
            testBtnValue = 2;
        }

        private void btnTestField_Click(object sender, EventArgs e)
        {
            flagTestBtn = true;
            MyMarshalToForm("AddItemToListBox", "Test - Field Init");
            testBtnValue = 3;
        }

        private void btnTestSoftReset_Click(object sender, EventArgs e)
        {
            flagTestBtn = true;
            MyMarshalToForm("AddItemToListBox", "Test - Soft Reset");
            testBtnValue = 4;
        }

        private void resetAtMidnightCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (resetAtMidnightCheckbox.Checked)
            {
                //Schedule the next reset for midnight tomorrow
                dateTimeResetSFC = DateTime.Today.AddDays(1); //.Today is today's date, with the time component set to 00:00:00.
            }
            else // unchecked
            {
                // set next reset for the year 10,000
                dateTimeResetSFC = DateTime.MaxValue;
            }
        }
    }
}
