using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace main {
    static class serialFunctions {

        //Initialize variables for Fermentation.
        //NOTE: BYTE 0: TEMP, BYTE 1: VAR, BYTE 2: CHECK

        static int temperature = 0;
        static int variance = 0;
        static int checkTime = 0;
        static int numOfBytes = 3; // This is the number of individual bytes that represent the values above.

        /// <summary>
        /// Function returns the stored values that have been recieved from FF
        /// </summary>
        /// <param name="which">Specifies which value is required.</param>
        /// <returns></returns>
        public static int ReturnValues(string which) {
            switch (which) {
                case "temp":
                    return temperature;
                case "var":
                    return variance;
                case "time":
                    return checkTime;
                default:
                    return 0;
            }
        } 

        /// <summary>
        /// Finds the port the FF is connected to.
        /// </summary>
        /// <returns>The port object.</returns>
        public static SerialPort FindArduino() {

            //Stores all available ports.
            string[] comPorts = SerialPort.GetPortNames();

            //Iterates through ports, sending a character and waiting a response.
            foreach (string port in comPorts) {
                SerialPort sp = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
                sp.Handshake = Handshake.None;
                sp.Open();
                sp.ReadTimeout = 2000; //Tells the program to wait two seconds to establish connection.
                sp.Write("B");
                Thread.Sleep(3000);
                int[] results = new int[numOfBytes];
                results = ReadFromSerialPort(sp, numOfBytes);
                

                if (results != null) {
                    updateVars(results);
                    return sp;
                }

                

            }

            return null;

        }

        /// <summary>
        /// Reads the bytes from the serial buffer and returns them as an int.
        /// </summary>
        /// <param name="serialPort">The port to check</param>
        /// <param name="numToRead">The number of bytes to read</param>
        /// <returns></returns>
        public static int[] ReadFromSerialPort(SerialPort serialPort, int numToRead) {
            byte[] buffer = new byte[numToRead];
            int offset = 0;
            int read;

            try {
                while (numToRead > 0 && (read = serialPort.Read(buffer, offset, numToRead)) > 0) {
                    offset += read;
                    numToRead -= read;
                }
            }
            catch (System.TimeoutException) {
                return null;
                

            }

            int[] bytesAsInts = Array.ConvertAll(buffer, c => (int)c); //Converts from bytes to ints to keep external code simple.

            return bytesAsInts;


        }


        /// <summary>
        /// Updates the stored values.
        /// </summary>
        /// <param name="values">An array of values from the FF</param>
        public static void updateVars(int[] values) {

            temperature = values[0];
            variance = values[1];
            checkTime = values[2];

        }

        /// <summary>
        /// Sends new information to the FF
        /// </summary>
        /// <param name="toSend">The int values</param>
        /// <param name="serial">The port it is on</param>
        public static void sendToFF(int[] toSend, SerialPort serial) {
            byte[] b = new byte[toSend.Length];

            for (int i = 0; i < toSend.Length; i++)
                b[i] = (byte)toSend[i];

            serial.Write(b, 0, toSend.Length);
            Thread.Sleep(2000);
            int[] results = new int[numOfBytes];
            results = ReadFromSerialPort(serial, numOfBytes);
            updateVars(results);

        }

    }
}
