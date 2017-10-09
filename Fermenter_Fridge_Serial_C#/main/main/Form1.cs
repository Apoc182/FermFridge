using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using main;
using System.IO.Ports;

namespace main {
    public partial class Form1 : Form {

        //Store the correct serial port
        SerialPort arduino;
        int numOfInputs = 3; //To avoid a magic number. Just defines the amount of text boxes.

        public Form1() {
            InitializeComponent();
        }

        

        private void Form1_Load(object sender, EventArgs e) {
           
            
        }

        private void connectButton_Click(object sender, EventArgs e) {
            connectButton.Enabled = false;
            status.Text = "Finding FermFridge...";
            arduino = serialFunctions.FindArduino();
            tempBox.Text = serialFunctions.ReturnValues("temp").ToString();
            varBox.Text = serialFunctions.ReturnValues("var").ToString();
            timeBox.Text = serialFunctions.ReturnValues("time").ToString();
            if (arduino == null) {
                MessageBox.Show("Fermentation Fridge not connected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                status.Text = "Unable to locate. Is it plugged in?";
                connectButton.Enabled = true;
            }
            else {
                connectButton.Enabled = false;
                sendButton.Enabled = true;
                tempBox.Enabled = true;
                varBox.Enabled = true;
                timeBox.Enabled = true;
                status.Text = "Connected.";
            }

        }

        /// <summary>
        /// Checks if valid data entered and if so, sends it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendButton_Click(object sender, EventArgs e) {
            status.Text = "Updating Values...";

            int[] send = new int[numOfInputs];
            if (!Int32.TryParse(tempBox.Text, out send[0])) { send[0] = serialFunctions.ReturnValues("temp"); }
            if (!Int32.TryParse(varBox.Text, out send[1])) { send[1] = serialFunctions.ReturnValues("var"); }
            if (!Int32.TryParse(timeBox.Text, out send[2])) { send[2] = serialFunctions.ReturnValues("time"); }
           
            
            serialFunctions.sendToFF(send, arduino);
            tempBox.Text = serialFunctions.ReturnValues("temp").ToString();
            varBox.Text = serialFunctions.ReturnValues("var").ToString();
            timeBox.Text = serialFunctions.ReturnValues("time").ToString();
            status.Text = "Updating Values...Success!";
        }
    }
}
