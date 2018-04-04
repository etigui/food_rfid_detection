using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rfid {
    class Program {

        private static string com_port = "COM12";

        static void Main(string[] args) {
            Rfid rfid = new Rfid(com_port);

            rfid.Tag_detected += Tag_detected;

            if (rfid.Open_read()) {
                
            }
        }

        private static void Tag_detected(object sender, TagDetectedEventArgs e) {
            Console.WriteLine($"Tag ID: {e.Tag_id}");
        }
    }
}
