using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

// Event handler: https://msdn.microsoft.com/en-us/library/system.eventhandler(v=vs.110).aspx 

namespace Mcdonalds {
    class Rfid {

        #region Vars

        private static SerialPort serial = null;
        private string com_port = string.Empty;
        private int baudrate = 0;
        private Parity parity = 0;
        private int data_bits = 0;
        private StopBits stop_bits = 0;
        private bool reading = false;
        #endregion

        #region Constructor

        public Rfid(string com_port, int baudrate = 9600, Parity parity = Parity.None, int data_bits = 8, StopBits stop_bits = StopBits.One) {
            this.com_port = com_port;
            this.baudrate = baudrate;
            this.parity = parity;
            this.data_bits = data_bits;
            this.stop_bits = stop_bits;
        }
        #endregion

        #region Event

        public event EventHandler<TagDetectedEventArgs> Tag_detected;

        /// <summary>
        /// Invoke event handler when tag id is detected
        /// </summary>
        /// <param name="e"></param>
        protected virtual void On_tag_detected(TagDetectedEventArgs e) {
            Tag_detected?.Invoke(this, e);
        }
        #endregion

        #region Init and open

        /// <summary>
        /// Start reaging RFID tag id
        /// </summary>
        /// <returns></returns>
        public bool Open_read() {

            // Serial setup and check port
            if (!Check_com_port(com_port)) {
                return false;
            }

            // Init serial
            serial = new SerialPort(com_port, baudrate, parity, data_bits, stop_bits);

            // Begin communications 
            serial.Open();

            // Thread to read the tag id
            new Thread(new ThreadStart(Read_tag_id)).Start();
            reading = true;

            // Set 0x02 mode to read card ID
            serial.Write(new byte[] { 0x02 }, 0, 1);
            return true;
        }

        /// <summary>
        /// Check if COM port exists
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool Check_com_port(string port) {
            if (Array.FindIndex(SerialPort.GetPortNames(), p => p == port) != -1) {
                return true;
            }
            return false;
        }
        #endregion

        #region Read tag id

        /// <summary>
        /// Get tag ID
        /// </summary>
        private void Read_tag_id() {
            List<byte> id = new List<byte>();
            DateTime reach = DateTime.Now;

            // Read serial
            while (true) {

                // Ceck if serial port is open
                if (reading) {

                    // Check if there is somethig to read
                    if (serial.BytesToRead > 0) {
                        byte value = (byte)serial.ReadByte();

                        // Check if not char error => last read residue
                        if (value != 255) {

                            // Tag id = 4 digits(bytes)
                            id.Add(value);
                            if (id.Count() == 4) {

                                // Add detection delay => avoid multiple detection 
                                if (DateTime.Now >= reach) {

                                    // Call handler => tag id detected
                                    TagDetectedEventArgs args = new TagDetectedEventArgs {
                                        Tag_id = string.Join("-", id.ToArray())
                                    };
                                    On_tag_detected(args);
                                    reach = DateTime.Now.AddSeconds(1);
                                }
                                id.Clear();
                            }
                        }
                    }
                } else { break; }
            }
        }
        #endregion

        #region Close

        /// <summary>
        /// Close serial port (RFID tag reading)
        /// </summary>
        public void Close() {
            reading = false;
            if (serial.IsOpen) {
                serial.Close();
            }
        }
        #endregion
    }
    #region Even handler class

    public class TagDetectedEventArgs : EventArgs {
        public string Tag_id { get; set; }
    }
    #endregion
}
