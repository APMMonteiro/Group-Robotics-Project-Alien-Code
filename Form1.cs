using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;     // added to use serial port features
using System.IO.Ports;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace GoodStuff
{
    public partial class Form1 : Form
    {
        //Global serialPort
        SerialPort serialPort;

        //Global counting variables
        int distanceReadingRequestsCounter = 0;     //Stores the number of times distance readings where requested
        int colorReadingRequestsCounter = 0;        //Stores the number of color distance readings where requested
        const int MAX_COLOR_READING_REQUESTS = 30; //Maximum amount of color reading requests allow before stopping loop

        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort("COM3", 115200);
            
            try
            {
                serialPort.Open();
                //serialPort.DiscardInBuffer();
                Console.WriteLine("Successfully opened serial port, COM3 at 115200 baud rate.");
                debugTextBox.AppendText("Successfully opened serial port, COM3 at 115200 baud rate.\n");
                debugTextBox.AppendText("\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Exception Thrown ===================================== Exception Thrown");
                Console.WriteLine(ex);
                MessageBox.Show("COM port not detected", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string command = customTB.Text;
            serialPort.WriteLine(command);
            Console.WriteLine("Using a custom command.");
            Console.WriteLine("Sending command: " + command);
            debugTextBox.AppendText("Using a custom command.\n");
            debugTextBox.AppendText("Sending command: " + command + "\n");
            debugTextBox.AppendText("\n");

            if (command[0].Equals('r'))
            {
                //if the command is a read, output must be read 
                System.Threading.Thread.Sleep(1000);
                string sensorReading = serialPort.ReadExisting();
                Console.WriteLine("A read was requested from the mbed, output is: " + sensorReading);
                debugTextBox.AppendText("A read was requested from the mbed, output is: " + sensorReading);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Starts the program

            //Gets picked color
            string chosenColor = getRadioColor();
            if (chosenColor.Equals("no"))
            {
                Console.WriteLine("Please select a color.");
                debugTextBox.AppendText("Please select a color.\n");
                debugTextBox.AppendText("\n");
                return;
            }

            findAndYeetColor(chosenColor);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Starts the loop which requests distance sensor data to detect alien
            int dist = 1000;

            //gets color chosen and doesn't let the loop start if no color was picked
            string chosenColor = getRadioColor();
            if (chosenColor.Equals("no"))
            {
                Console.WriteLine("Please select a color.");
                debugTextBox.AppendText("Please select a color.\n");
                debugTextBox.AppendText("\n");
                return;
            }

            Console.WriteLine("Awaiting for turtle to arrive.");
            debugTextBox.AppendText("Awaiting for turtle to arrive.");

            //the sensor reads from 255 to 0, 255 if there's nothing close enough to it
            //loops until requests until it recognises the turtle as being in position
            while (dist > 200 && dist != 0) 
            {
                dist = getSensorDistance();
                System.Threading.Thread.Sleep(500);
            }

            Console.WriteLine("Turtle detected, proceeding in 3 seconds.");
            debugTextBox.AppendText("Turtle detected, proceeding.");
            System.Threading.Thread.Sleep(3000);

            findAndYeetColor(chosenColor);
        }

        private string getRadioColor()
        {
            //Checks which radio button was pressed
            //Returns appropriate color "white", "yellow", "red", "green", "blue"
            //Or "no" if no radio button was selected
            if (radioButtonWhite.Checked == true)
            {
                Console.WriteLine("White was picked. \n");
                debugTextBox.AppendText("White was picked. \n");
                return "white";
            }
            else if (radioButtonYellow.Checked == true)
            {
                Console.WriteLine("Yellow was picked. \n");
                debugTextBox.AppendText("Yellow was picked. \n");
                return "yellow";
            }
            else if (radioButtonRed.Checked == true)
            {
                Console.WriteLine("Red was picked. \n");
                debugTextBox.AppendText("Red was picked. \n");
                return "red";
            }
            else if (radioButtonGreen.Checked == true)
            {
                Console.WriteLine("Green was picked. \n");
                debugTextBox.AppendText("Green was picked. \n");
                return "green";
            }
            else if (radioButtonBlue.Checked == true)
            {
                Console.WriteLine("Blue was picked. \n");
                debugTextBox.AppendText("Blue was picked. \n");
                return "blue";
            }
            else
            {
                Console.WriteLine("No radio button selected, nothing happens..\n");
                debugTextBox.AppendText("No radio button selected, nothing happens..\n");
                debugTextBox.AppendText("\n");
                return "no";
            }
        }

        private void findAndYeetColor(string color)
        {
            //Pushes the chosen block
            debugTextBox.AppendText("Looking for " + color + ". \n");
            Console.WriteLine("Looking for " + color);
            for (int p = 1; p < 10; p+=2)
            {
                //For each position the blocks should be in, p1 p3 p5 p7 p9
                
                //Moves shoulder servo to position
                serialPort.WriteLine("csp" + p);
                Console.WriteLine("Sent command 'csp{0}'", p);
                Console.WriteLine("Moving arm to position " + p + ". \n");
                debugTextBox.AppendText("Moving arm to position " + p + ". \n");
                System.Threading.Thread.Sleep(1000);


                //Checks if the block matches chosen color and pushes it if it does
                string colorSensed = getSensorColor();
                if (colorSensed.Equals(color))
                {
                    Console.WriteLine("Color found.\n");
                    debugTextBox.AppendText("Color found.\n");
                    yeetBlock();
                    break;
                }
                else
                {
                    Console.WriteLine("This isn't {0} this is {1}!", color, colorSensed);
                    debugTextBox.AppendText("Couldn't find color.\n");
                }

            }

            //Move arm back to starting position
            Console.WriteLine("Returning to start.\n");
            debugTextBox.AppendText("Returning to start.\n");
            Console.WriteLine("Sent command 'csp1'");
            serialPort.WriteLine("csp1");

        }

        private void yeetBlock()
        {
            Console.WriteLine("Pushing the block.\n");
            debugTextBox.AppendText("Pushing the block.\n");
            //Moves elbow servo to push block
            Console.WriteLine("Sent command 'e'");
            serialPort.WriteLine("e");
        }

        private int getSensorDistance()
        {
            //Will request a reading from the sensor
            //Populates labels with reading data
            //Returns integer value of the distance between 255 to 0
            //where 255 means it doesnt detect nothing in front while 0 is upclose and personal

            int dist = 1000;

            //Requests a distance reading from the mbed
            distanceReadingRequestsCounter++; //Incremeants the counter
            label19.Text = distanceReadingRequestsCounter.ToString();
            Console.WriteLine("Requesting distance reading, try #{0}.", distanceReadingRequestsCounter);
            debugTextBox.AppendText("Requesting distance reading, try #" + distanceReadingRequestsCounter);
            serialPort.WriteLine("rd");
            Console.WriteLine("Sent command 'rd'");
            System.Threading.Thread.Sleep(1000);

            //Reads from the mbed
            string sensorReading = serialPort.ReadExisting();
            Console.WriteLine("Distance sensor reading is: " + sensorReading);
            debugTextBox.AppendText("Distance sensor reading is: " + sensorReading);
            label15.Text = sensorReading;

            while (!Int32.TryParse(sensorReading, out dist))
                //while the convertion is unsuccessful
                //meaning if the reading isnt an integer
            {
                Console.WriteLine("Int32.TryParse unsuccessful, requesting another distance reading.");
                debugTextBox.AppendText("Couldn't read distance sensor, retrying.");
                getSensorDistance(); //recursive
            }



            return dist;
        }

        private string getSensorColor()
        {
            //Will request a reading from the sensor
            //Populates labels with reading data
            //Returns guess for which block is in front of it

            //Discards buffer in case anything else is there
            //serialPort.DiscardInBuffer();

            //Asks for a color reading from the mbed
            colorReadingRequestsCounter++; //Incremeants the counter
            label18.Text = colorReadingRequestsCounter.ToString();
            Console.WriteLine("Requesting color reading, try #{0}.", colorReadingRequestsCounter);
            serialPort.WriteLine("rt");
            Console.WriteLine("Sent command 'rt'");
            System.Threading.Thread.Sleep(1000);

            //Reads from the mbed
            string sensorReading = serialPort.ReadExisting();
            Console.WriteLine("Color sensor readings are: " + sensorReading);
            

            while (sensorReading == null || sensorReading.Equals("0,0,0,0"))
            {
                //Repeat in case reading wasn't successful
                colorReadingRequestsCounter++;
                label18.Text = colorReadingRequestsCounter.ToString();
                Console.WriteLine("Color readings unsuccessful, retrying");
                Console.WriteLine("Requesting color reading, try #{0}.", colorReadingRequestsCounter);
                debugTextBox.AppendText("Color readings unsuccessful, retrying.");
                serialPort.WriteLine("rt");
                Console.WriteLine("Sent command 'rt'");
                System.Threading.Thread.Sleep(1000);
                sensorReading = serialPort.ReadExisting();
                Console.WriteLine("Color sensor readings are: " + sensorReading);
                if (colorReadingRequestsCounter > MAX_COLOR_READING_REQUESTS)
                {
                    Console.WriteLine("Failed color reading too many times, stopping.");
                    debugTextBox.AppendText("Failed color reading too many times, stopping.");
                    return "fail";
                }
            }

            debugTextBox.AppendText("Sensor readings are: " + sensorReading);
            label12.Text = sensorReading;

            //Converts the 16 bit crgb to 8 bit rgb
            int[] decRGB = new int[3];
            //"sensorReading" is a csv value, so its first converted into an int array with the separated values and then converted to rgb
            decRGB = convertToDecRGB(convertCSVstringToIntArray(sensorReading));

            //Prints those values to label5, 8 bit values, to console and textbox
            string stringRGB = string.Join(", ", decRGB);
            debugTextBox.AppendText("decRGB = " + string.Join(" ", decRGB));
            Console.WriteLine("decRGB = {0}", string.Join(" ", decRGB));
            label5.Text = stringRGB;

            //Print it to color on the window
            pictureBox1.BackColor = Color.FromArgb(decRGB[0], decRGB[1], decRGB[2]);

            //Guess what color block it is
            string block = guessBlock(convertCSVstringToIntArray(sensorReading));
            debugTextBox.AppendText("Guessing it's " + block);
            Console.WriteLine("Guessing it's " + block);
            label9.Text = block;

            while (block.Equals("can't guess"))
            {
                block = getSensorColor();
            }

            //Return color
            return block;
        }

        private int[] convertToDecRGB(int[] badCRGB)
        {
            //Param is a 4 element int array
            //Converts the input to a 3 element float array
            float[] badCRGBfloat = new float[4];
            badCRGBfloat = Array.ConvertAll<int, float>(badCRGB, x => (float)x);

            //Takes in the 4 element CRGB int array from the CRGB sensor and returns an 8 bit equivalent 3 element array
            float metric = badCRGBfloat[0];    //The clear value from the sensor
            float[] goodRGBValues = new float[3];   //Will hold the converted RGB values
            for (int i = 0; i <= 2; i++)
            {
                //Scales each color value to clears value
                goodRGBValues[i] = badCRGBfloat[i + 1] / metric;
                Console.WriteLine("converter stuff % {0} = {1}", i, goodRGBValues[i]);
                //Rescales them in porpotion to 256
                goodRGBValues[i] = goodRGBValues[i] * 256;
                Console.WriteLine("converter stuff % {0} = {1}", i, goodRGBValues[i]);
            }

            //Converts the value back to int to output
            int[] goodRGBValuesInt = new int[3];
            goodRGBValuesInt = Array.ConvertAll(goodRGBValues, x => (int)x);
            return goodRGBValuesInt;
        }

        private int[] convertCSVstringToIntArray(string s)
        {
            //Splits the given string into a string array with the 4 values separated
            string[] stringCRGBReadings = new string[4];
            stringCRGBReadings = s.Split(',');
            Console.WriteLine("stringCRGBReadings = {0} {1} {2} {3}", stringCRGBReadings);

            //Converts the string array into an int array
            int[] intCRGBReadings = new int[4];
            try
            {
                intCRGBReadings = Array.ConvertAll<string, int>(stringCRGBReadings, int.Parse);
            }
            catch (System.FormatException fe)
            {
                Console.WriteLine("stringCRGBReadings = {0} {1} {2} {3}", stringCRGBReadings);
                Console.WriteLine(fe);
            }
            
            Console.WriteLine("intCRGBReadings = {0} ", string.Join(" ", intCRGBReadings));

            return intCRGBReadings;
        }

        private string guessBlock(int[] crgb)
        {
            //Hard coded block color guesser
            //Returns the color of the block in string
            //Returns "can't guess" if that's the case
            string guess = "can't guess";
            
            //Questions
            bool isClearLessThan150 = false; //used to assume it's not looking at anything
            bool isClearGreaterThan6000 = false;
            bool isClearAtLeastTwiceRed = false;
            bool isClearAtLeastTwiceGreen = false;
            bool isClearAtLeastTwiceBlue = false;
            bool isBlueLessThanHalfRed = false;
            bool isBlueLessThanp8Green = false;
            bool isGreenLessThanp7Red = false;
            bool isGreenLessThanp85Red = false;
            bool isGreenLessThanBlue = false;
            bool isRedLessThanp7Green = false;
            bool isRedLessThanp9Blue = false;

            //Checking which of these decleared booleans are true
            if (crgb[0] < 150)              //is clear less than 150
            {
                Console.WriteLine("Clear is less than 150.");
                isClearLessThan150 = true;
            }

            if (crgb[0] > 6000)             //is clear greater than 6000          
            {
                Console.WriteLine("Clear is greater than 6000.");
                isClearGreaterThan6000 = true;
            }

            if (crgb[0] / 2 > crgb[1])        //is half of clear greater than red
            {
                Console.WriteLine("Clear is at least twice red.");
                isClearAtLeastTwiceRed = true;
            }

            if (crgb[0] / 2 > crgb[2])      //is half of clear greater than green
            {
                Console.WriteLine("Clear is at least twice green.");
                isClearAtLeastTwiceGreen = true;
            }

            if (crgb[0] / 2 > crgb[3])      //is half of clear greater than blue
            {
                Console.WriteLine("Clear is at least twice blue.");
                isClearAtLeastTwiceBlue = true;
            }

            if (crgb[3] * 2 < crgb[1])         //is double of blue less than red
            {
                Console.WriteLine("Blue is less than half red.");
                isBlueLessThanHalfRed = true;
            }

            if (crgb[3] < crgb[2] * 0.8)        //is double of blue less than green
            {
                Console.WriteLine("Blue is less than half green.");
                isBlueLessThanp8Green = true;
            }

            if (crgb[2] < crgb[1] * 0.7)      //is green less than red * 0.7
            {
                Console.WriteLine("Green is less than .8 red");
                isGreenLessThanp7Red = true;
            }

            if (crgb[2] < crgb[1] * 0.85)      //is green less than red * 0.85
            {
                Console.WriteLine("Green is less than .8 red");
                isGreenLessThanp85Red = true;
            }

            if (crgb[2] < crgb[3])    //is green less than blue
            {
                Console.WriteLine("Green is less than .8 blue");
                isGreenLessThanBlue = true;
            }

            if (crgb[1] < crgb[2] * 0.7)      //is red less than 0.7 green
            {
                Console.WriteLine("Red is less than .7 green");
                isRedLessThanp7Green = true;
            }

            if (crgb[1] < crgb[3] * 0.9)      //is red less than 0.9 blue
            {
                Console.WriteLine("Red is less than .9 blue");
                isRedLessThanp9Blue = true;
            }

            //Checking if the block is white
            if ((isClearGreaterThan6000 ||
                (isClearAtLeastTwiceRed && isClearAtLeastTwiceGreen && isClearAtLeastTwiceBlue) //is clear twice all the other colors
                && (!isBlueLessThanHalfRed || !isBlueLessThanp8Green))//if blue is a lot less than red and green usually the block is yellow
                && !isGreenLessThanp85Red                                //distinguishes between green and white
                && !isClearLessThan150)                                 //Not having anything in front of it is very similar to white so adding this in case
            {
                guess = "white";
            }

            //Checking if block is yellow
            if (isClearAtLeastTwiceGreen && isClearAtLeastTwiceBlue //is clear twice blue and green? found that red is close to half clear so not safe to use that one
                && isBlueLessThanHalfRed && isBlueLessThanp8Green //found this to be the case when looking at yellow
                && !isClearLessThan150)                             //Not having anything in front of it is very similar to white so adding this in case     
            {
                guess = "yellow";
            }

            //Checking if block is red
            if (isGreenLessThanp7Red        //is green less than 0.7 red   
                && isBlueLessThanHalfRed    //is blue less than half red
                && !isClearLessThan150)     //is clear above 150
            {
                guess = "red";
            }

            //Checking if block is green
            if (isBlueLessThanp8Green       //is blue less than 0.8 green
                && isRedLessThanp7Green     //is red less than 0.7 green
                && !isClearLessThan150)     //is clear above 150
            {
                guess = "green";
            }

            if (isGreenLessThanBlue         //is green less than blue
                && isRedLessThanp9Blue      //is red less than 0.9 blue
                && !isClearLessThan150)     //is clear above 150
            {
                guess = "blue";
            }

            Console.Write("Guess is " + guess);
            debugTextBox.AppendText("Guess is " + guess);
            return guess;
        }
    }
}
