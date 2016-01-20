using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace Com.Aote.Utils
{
    //读下串口工具
    public class SerialPort:IDisposable
    {
       
        #region Enum
        public enum StopBits
        {
            None,
            One,
            Two,
            OnePointFive,
        }

        public enum Parity
        {
            None,
            Odd,
            Even,
            Mark,
            Space,
        }
        #endregion
        #region Fields
        /// <summary> 
        /// The baud rate at which the communications device operates. 
        /// </summary> 
        private readonly int iBaudRate;

        /// <summary> 
        /// The number of bits in the bytes to be transmitted and received. 
        /// </summary> 
        private readonly byte byteSize;

        /// <summary> 
        /// The system handle to the serial port connection ('file' handle). 
        /// </summary> 
        private IntPtr pHandle = IntPtr.Zero;

        /// <summary> 
        /// The parity scheme to be used. 
        /// </summary> 
        private readonly Parity parity;

        /// <summary> 
        /// The name of the serial port to connect to. 
        /// </summary> 
        private readonly string sPortName;

        /// <summary> 
        /// The number of bits in the bytes to be transmitted and received. 
        /// </summary> 
        private readonly StopBits stopBits;
        public bool Opened = false;
        #endregion

        #region Constructor
        /// <summary> 
        /// Creates a new instance of SerialCom. 
        /// </summary> 
        /// <param>The name of the serial port to connect to</param> 
        /// <param>The baud rate at which the communications device operates</param> 
        /// <param>The number of stop bits to be used</param> 
        /// <param>The parity scheme to be used</param> 
        /// <param>The number of bits in the bytes to be transmitted and received</param> 
        public SerialPort(string portName, int baudRate, StopBits stopBits, Parity parity, byte byteSize)
        {
            if (stopBits == StopBits.None)
                throw new ArgumentException("stopBits cannot be StopBits.None", "stopBits");
            if (byteSize < 5 || byteSize > 8)
                throw new ArgumentOutOfRangeException("The number of data bits must be 5 to 8 bits.", "byteSize");
            if (baudRate < 110 || baudRate > 256000)
                throw new ArgumentOutOfRangeException("Invalid baud rate specified.", "baudRate");
            if ((byteSize == 5 && stopBits == StopBits.Two) || (stopBits == StopBits.OnePointFive && byteSize > 5))
                throw new ArgumentException("The use of 5 data bits with 2 stop bits is an invalid combination, " +
                    "as is 6, 7, or 8 data bits with 1.5 stop bits.");

            this.sPortName = portName;
            this.iBaudRate = baudRate;
            this.byteSize = byteSize;
            this.stopBits = stopBits;
            this.parity = parity;
        }

        /// <summary> 
        /// Creates a new instance of SerialCom. 
        /// </summary> 
        /// <param>The name of the serial port to connect to</param> 
        /// <param>The baud rate at which the communications device operates</param> 
        /// <param>The number of stop bits to be used</param> 
        /// <param>The parity scheme to be used</param> 
        public SerialPort(string portName, int baudRate, StopBits stopBits, Parity parity)
            : this(portName, baudRate, stopBits, parity, 8)
        {

        }
        #endregion

        #region Open
        /// <summary> 
        /// Opens and initializes the serial connection. 
        /// </summary> 
        /// <returns>Whether or not the operation succeeded</returns> 
        public bool Open()
        {
            pHandle = CreateFile(this.sPortName, FileAccess.ReadWrite, FileShare.None,
                IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (pHandle == IntPtr.Zero) return false;

            if (ConfigureSerialPort())
            {
                Opened = true;
                return true;
            }
            else
            {
                Dispose();
                return false;
            }
        }
        #endregion

        #region Write
        /// <summary> 
        /// Transmits the specified array of bytes. 
        /// </summary> 
        /// <param>The bytes to write</param> 
        /// <returns>The number of bytes written (-1 if error)</returns> 
        public int Write(byte[] data)
        {
            FailIfNotConnected();
            if (data == null) return 0;

            int bytesWritten;
            if (WriteFile(pHandle, data, data.Length, out bytesWritten, 0))
                return bytesWritten;
            return -1;
        }

        /// <summary> 
        /// Transmits the specified string. 
        /// </summary> 
        /// <param>The string to write</param> 
        /// <returns>The number of bytes written (-1 if error)</returns> 
        public int Write(int data)
        {
            FailIfNotConnected();

            // convert the string to bytes 
            byte[] bytes=new byte[1];
            //十六进制转十进制
            string value = Convert.ToString(data, 10);
            bytes[0] = byte.Parse(value);
            return Write(bytes);
        }

        public static int String2Hex(string str, byte[] senddata)
        {
            int hexdata, lowhexdata;
            int hexdatalen = 0;
            int len = str.Length;
            //senddata = new byte[len / 2];
            for (int i = 0; i < len; )
            {
                char lstr, hstr = str[i];
                if (hstr == ' ')
                {
                    i++;
                    continue;
                }
                i++;
                if (i >= len)
                    break;
                lstr = str[i];
                hexdata = ConvertHexChar(hstr);
                lowhexdata = ConvertHexChar(lstr);
                if ((hexdata == 16) || (lowhexdata == 16))
                {
                    break;
                }
                else
                {
                    hexdata = hexdata * 16 + lowhexdata;
                }
                i++;
                senddata[hexdatalen] = (byte)hexdata;
                hexdatalen++;
            }
            //senddata.SetSize(hexdatalen);
            return hexdatalen;
        }

        //这是一个将字符转换为相应的十六进制值的函数
        //好多C语言书上都可以找到
        //功能：若是在0-F之间的字符，则转换为相应的十六进制字符，否则返回-1
        public static int ConvertHexChar(char ch)
        {
            //if((ch>='0')&&(ch<='9'))
            //{
            //    return ch-'0x30';
            //}
            //else if((ch>='A')&&(ch<='F'))
            //return ch-'A'+10;
            //else if((ch>='a')&&(ch<='f'))
            //return ch-'a'+10;
            //else return '-1';
            string hex = Convert.ToString((int)ch, 16);
            return int.Parse(hex);
        }

        /// <summary> 
        /// Transmits the specified string and appends the carriage return to the end 
        /// if it does not exist. 
        /// </summary> 
        /// <remarks> 
        /// Note that the string must end in '\r\n' before any serial device will interpret the data 
        /// sent. For ease of programmability, this method should be used instead of Write() when you 
        /// want to automatically execute the specified command string. 
        /// </remarks> 
        /// <param>The string to write</param> 
        /// <returns>The number of bytes written (-1 if error)</returns> 
        public int WriteLine(int data)
        {
            
            return Write(data);
        }
        #endregion

        #region Read
        /// <summary> 
        /// Reads any bytes that have been received and writes them to the specified array. 
        /// </summary> 
        /// <param>The array to write the read data to</param> 
        /// <returns>The number of bytes read (-1 if error)</returns> 
        public int Read(byte[] data)
        {
            FailIfNotConnected();
            if (data == null) return 0;

            int bytesRead;
            if (ReadFile(pHandle, data, data.Length, out bytesRead, 0))
                return bytesRead;
            return -1;
        }

        /// <summary> 
        /// Reads any data that has been received as a string. 
        /// </summary> 
        /// <param>The maximum number of bytes to read</param> 
        /// <returns>The data received (null if no data)</returns> 
        public string ReadString(int maxBytesToRead)
        {
            if (maxBytesToRead < 1) throw new ArgumentOutOfRangeException("maxBytesToRead");

            byte[] bytes = new byte[maxBytesToRead];
            int numBytes = Read(bytes);
            //string data = ASCIIEncoding.ASCII.GetString(bytes, 0, numBytes); 
            string data = Encoding.UTF8.GetString(bytes, 0, numBytes);
            return data;
        }
        #endregion

        #region Dispose Utils
        /// <summary> 
        /// Disconnects and disposes of the SerialCom instance. 
        /// </summary> 
        public void Dispose()
        {
            if (pHandle != IntPtr.Zero)
            {
                CloseHandle(pHandle);
                pHandle = IntPtr.Zero;
            }
        }

        /// <summary> 
        /// Flushes the serial I/O buffers. 
        /// </summary> 
        /// <returns>Whether or not the operation succeeded</returns> 
        public bool Flush()
        {
            FailIfNotConnected();

            const int PURGE_RXCLEAR = 0x0008; // input buffer 
            const int PURGE_TXCLEAR = 0x0004; // output buffer 
            return PurgeComm(pHandle, PURGE_RXCLEAR | PURGE_TXCLEAR);
        }
        #endregion

        public void Close()
        {
            if (pHandle != IntPtr.Zero)
            {
                CloseHandle(pHandle);
                Opened = false;
            }
        }

        #region Private Helpers
        /// <summary> 
        /// Configures the serial device based on the connection parameters pased in by the user. 
        /// </summary> 
        /// <returns>Whether or not the operation succeeded</returns> 
        private bool ConfigureSerialPort()
        {
            DCB serialConfig = new DCB();
            if (GetCommState(pHandle, ref serialConfig))
            {
                // setup the DCB struct with the serial settings we need 
                serialConfig.BaudRate = (uint)this.iBaudRate;
                serialConfig.ByteSize = this.byteSize;
                serialConfig.fBinary = 1; // must be true 
                serialConfig.fDtrControl = 1; // DTR_CONTROL_ENABLE "Enables the DTR line when the device is opened and leaves it on." 
                serialConfig.fAbortOnError = 0; // false 
                serialConfig.fTXContinueOnXoff = 0; // false

                serialConfig.fParity = 1; // true so that the Parity member is looked at 
                switch (this.parity)
                {
                    case Parity.Even:
                        serialConfig.Parity = 2;
                        break;
                    case Parity.Mark:
                        serialConfig.Parity = 3;
                        break;
                    case Parity.Odd:
                        serialConfig.Parity = 1;
                        break;
                    case Parity.Space:
                        serialConfig.Parity = 4;
                        break;
                    case Parity.None:
                    default:
                        serialConfig.Parity = 0;
                        break;
                }
                switch (this.stopBits)
                {
                    case StopBits.One:
                        serialConfig.StopBits = 0;
                        break;
                    case StopBits.OnePointFive:
                        serialConfig.StopBits = 1;
                        break;
                    case StopBits.Two:
                        serialConfig.StopBits = 2;
                        break;
                    case StopBits.None:
                    default:
                        throw new ArgumentException("stopBits cannot be StopBits.None");
                }

                if (SetCommState(pHandle, ref serialConfig))
                {
                    // set the serial connection timeouts 
                    COMMTIMEOUTS timeouts = new COMMTIMEOUTS();
                    timeouts.ReadIntervalTimeout = 1;
                    timeouts.ReadTotalTimeoutMultiplier = 0;
                    timeouts.ReadTotalTimeoutConstant = 0;
                    timeouts.WriteTotalTimeoutMultiplier = 0;
                    timeouts.WriteTotalTimeoutConstant = 0;
                    if (SetCommTimeouts(pHandle, ref timeouts))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary> 
        /// Helper that throws a InvalidOperationException if we don't have a serial connection. 
        /// </summary> 
        private void FailIfNotConnected()
        {
            if (pHandle == IntPtr.Zero)
                throw new InvalidOperationException("You must be connected to the serial port before performing this operation.");
        }
        #endregion

        #region Native Helpers
        #region Native structures
        /// <summary> 
        /// Contains the time-out parameters for a communications device. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        struct COMMTIMEOUTS
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        /// <summary> 
        /// Defines the control setting for a serial communications device. 
        /// </summary> 
        [StructLayout(LayoutKind.Sequential)]
        struct DCB
        {
            public int DCBlength;
            public uint BaudRate;
            public uint Flags;
            public ushort wReserved;
            public ushort XonLim;
            public ushort XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public sbyte XonChar;
            public sbyte XoffChar;
            public sbyte ErrorChar;
            public sbyte EofChar;
            public sbyte EvtChar;
            public ushort wReserved1;
            public uint fBinary;
            public uint fParity;
            public uint fOutxCtsFlow;
            public uint fOutxDsrFlow;
            public uint fDtrControl;
            public uint fDsrSensitivity;
            public uint fTXContinueOnXoff;
            public uint fOutX;
            public uint fInX;
            public uint fErrorChar;
            public uint fNull;
            public uint fRtsControl;
            public uint fAbortOnError;
        }
        #endregion

        #region Native Methods
        // Used to get a handle to the serial port so that we can read/write to it. 
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFile(string fileName,
           [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
           [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
           IntPtr securityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
           int flags,
           IntPtr template);

        // Used to close the handle to the serial port. 
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        // Used to get the state of the serial port so that we can configure it. 
        [DllImport("kernel32.dll")]
        static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

        // Used to configure the serial port. 
        [DllImport("kernel32.dll")]
        static extern bool SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

        // Used to set the connection timeouts on our serial connection. 
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCommTimeouts(IntPtr hFile, ref COMMTIMEOUTS lpCommTimeouts);

        // Used to read bytes from the serial connection. 
        [DllImport("kernel32.dll")]
        static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer,
           int nNumberOfBytesToRead, out int lpNumberOfBytesRead, int lpOverlapped);

        // Used to write bytes to the serial connection. 
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer,
            int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped);

        // Used to flush the I/O buffers. 
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool PurgeComm(IntPtr hFile, int dwFlags);
        #endregion
        #endregion 
    }

    class HexCon
    {
        // 把十六进制字符串转换成字节型和把字节型转换成十六进制字符串 converter hex string to byte and byte to hex string 
        public static string ByteToString(byte[] InBytes)
        {
            string StringOut = "";
            foreach (byte InByte in InBytes)
            {
                StringOut = StringOut + String.Format("{0:X2} ", InByte);
            }
            return StringOut;
        }
        public static byte[] StringToByte(string InString)
        {
            string[] ByteStrings;
            ByteStrings = InString.Split(" ".ToCharArray());
            byte[] ByteOut;
            ByteOut = new byte[ByteStrings.Length - 1];
            for (int i = 0; i == ByteStrings.Length - 1; i++)
            {
                ByteOut[i] = Convert.ToByte(("0x" + ByteStrings[i]));
            }
            return ByteOut;
        }
    }
}
