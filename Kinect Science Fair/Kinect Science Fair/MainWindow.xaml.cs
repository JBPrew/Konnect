using System; //import necessary packages
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect; //must be included in references after downloading Kinect 1.8 SDK
using Kinect_Science_FairML.Model;
using System.Reflection;
using Microsoft.ML;
using System.Speech.Synthesis;

namespace Kinect_Science_Fair
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectSensor; //declares the variable the kinect sensor will be stored in

        string connectedStatus; //declares the string the connection status will be storesd in

        /// Bitmap that will hold color information
        private WriteableBitmap colorBitmap;

        /// Intermediate storage for the depth data received from the camera
        private DepthImagePixel[] depthPixels;

        /// Intermediate storage for the depth data converted to color
        private byte[] colorPixels;



        /// Bitmap that will hold color information
        private WriteableBitmap colorBitmapRGBStream;

        /// Intermediate storage for the color data received from the camera
        private byte[] colorPixelsRGBStream;



        public MainWindow()
        {
            InitializeComponent(); //loads main window
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) //code to be run when window is loaded
        {
            DiscoverKinectSensor(); //calls the DiscoverKinectSensor function which discovers the connected kinect sensor and calls InitializeKinect which XXX
          
        }

        private void DiscoverKinectSensor() //discovers the kinect sensor on startup
        {
            // if (kinectSensor != null) //if a kinect sensor is attatched the program will cycle through all connected KinectSensors until one is connected. This sensor is then stored in kinectSensor
            //{
            foreach (KinectSensor sensor in KinectSensor.KinectSensors) //cycles through all available kinect sensors
            {
                if (sensor.Status == KinectStatus.Connected) //if the available kinect sensor is connected then it is stored in kinectSensor
                {
                    // Found one, set our sensor to this
                    kinectSensor = sensor;
                    connectedStatus = "Connected"; //set the connected status
                    break; //breaks for loop
                }
            }
                //}
                //else //if no kinect sensor is attatched the connected status is set to unconnected and the method is ended by use of a return
            if (kinectSensor == null)
            {
                connectedStatus = "Found no Kinect Sensors connected to USB"; //sets the connected status
                status.Text = "Status: " + connectedStatus; //sets the status textbox in the xaml to the connected status
                return;
            }



            status.Text = "Status: " + connectedStatus; //sets the status textbox in the xaml to the connected status
            // Init the found and connected device
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect(); //initializes the discovered kinect
            }

        }

        bool InitializeKinect()
        {
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); //enables the depth image stram

            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinectSensor_DepthFrameReady);
            //kinectSensor.DepthFrameReady += this.kinectSensor_DepthFrameReady; //event handler for when a depthFrame is produced and ready to be manipulated

            // Allocate space to put the depth pixels we'll receive
            this.depthPixels = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength]; 

            // Allocate space to put the color pixels we'll create
            this.colorPixels = new byte[kinectSensor.DepthStream.FramePixelDataLength * sizeof(int)];

            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(kinectSensor.DepthStream.FrameWidth, kinectSensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.DepthImageXAML.Source = this.colorBitmap; //sets the source for the image where the depth stream will be shown


            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //event handler for when a colorFrame is produced and ready to be manipulated

            // Allocate space to put the pixels we'll receive
            this.colorPixelsRGBStream = new byte[kinectSensor.ColorStream.FramePixelDataLength];

            // This is the bitmap we'll display on-screen
            this.colorBitmapRGBStream = new WriteableBitmap(kinectSensor.ColorStream.FrameWidth, kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            // Set the image we display to point to the bitmap where we'll put the image data
            this.ColorImageXAML.Source = colorBitmapRGBStream; //sets the source for the image where the color stream will be shown


            // Add an event handler to be called whenever there is new color frame data
            //kinectSensor.ColorFrameReady += this.kinectSensor_ColorFrameReady;
            kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);


            try
            {
                kinectSensor.Start(); //starts up the kinect
            }
            catch
            {
                connectedStatus = "Unable to start the Kinect Sensor"; //sets the connected status
                status.Text = "Status: " + connectedStatus; //sets the status textbox in the xaml to the connected status
                return false; //exits out of the method with a false bool showing that the kinect did not start up right
            }
            return true; //exits out of the method with a true bool showing that the kinect started up correctly
        }

        //short[] depthData;
        int heightPixel = 480; //sets the height and width of the image as a variable for easier manipulation
        int widthPixel = 640;
        int frameCounter = 0;
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {

                    // Copy the pixel data from the image to a temporary array
                    depthImageFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthPixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthPixels[i].Depth;

                        byte intensity = (byte)(depth == 0 || depth > 4095 ? 0 : 255 - (byte)((depth / 4095.0f) * 255.0f));


                        // Write out blue byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.colorPixels[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.colorPixels[colorPixelIndex++] = intensity;

                        ++colorPixelIndex;
                    }

                    /*
                    for (int x = 0; x <= widthPixel; x += 80)
                    {
                        switch (x / 80)
                        {
                            case 0:
                                {
                                    for (int y = 0; y < heightPixel; y++)
                                    {
                                        short depthPixel = depthPixels[x + y * widthPixel].Depth;
                                        short[] depthDataFarLeft = new short[heightPixel];
                                        depthDataFarLeft.Append(depthPixel);

                                    }
                                    break;
                                }
                        }
                    }
                    */

                    
                    // Write the pixel data into our bitmap to be displayed
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);

                    Status_Checker();

                    if (frameCounter == 15)
                    {
                        realTimeDataCollection();
                        frameCounter = 0;
                    }

                    frameCounter++;
                }
            }
        }

        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixelsRGBStream);

                    // Write the pixel data into our bitmap to be displayed
                    this.colorBitmapRGBStream.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmapRGBStream.PixelWidth, this.colorBitmapRGBStream.PixelHeight),
                        this.colorPixelsRGBStream,
                        this.colorBitmapRGBStream.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //SaveDepthData(depthData);
            kinectSensor.Stop();

        }

        void Status_Checker() //checks the status of the kinect and sets the textbox in the xaml
        {
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        connectedStatus = "Status: Connected";
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        connectedStatus = "Status: Disconnected";
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        connectedStatus = "Status: Connect the power";
                        break;
                    }
                default:
                    {
                        connectedStatus = "Status: Error";
                        break;
                    }
            }

            status.Text = "Status: " + connectedStatus; 
        }

        int position; //declares an int variable for the postition of the training to be stored in
        int x;
        int y;
        void ExportToCSV_Clicked(object sender, RoutedEventArgs e) //uses for loops that cycle between 7 different vertical line profiles and log the data for that line
        {
            if (kinectSensor != null)
            {
                for (x = 80; x < widthPixel; x += 80) //goes across the line profiles by going from 80 , 160, 240, 320, 400, 480, 560
                {
                    position = x / 80; //changes postion depending on which line profile is getting scanned down
                    switch (position) //switch case depending on position to be stored in different short arrays for the line profile
                    {
                        case 1:
                            {
                                short[] depthDataFarLeft = new short[heightPixel]; //creates a new short with a length of the number of pixels on the line profile
                                for (y = 0; y < heightPixel; y++) //goes down the line profile
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataFarLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataFarLeft[y] = UnknownDepthFinder(x, y);
                                    }

                                }
                                DataToCSVTraining(depthDataFarLeft); //goes to the DataToCSVTraining to get added to a csv used to train the neural network
                                break;
                            }
                        case 2:
                            {
                                short[] depthDataLeft = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataLeft[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataLeft);
                                break;
                            }
                        case 3:
                            {
                                short[] depthDataCloseLeft = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCloseLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCloseLeft[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataCloseLeft);
                                break;
                            }
                        case 4:
                            {
                                short[] depthDataCenter = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCenter[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCenter[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataCenter);
                                break;
                            }
                        case 5:
                            {
                                short[] depthDataCloseRight = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCloseRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCloseRight[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataCloseRight);
                                break;
                            }
                        case 6:
                            {
                                short[] depthDataRight = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataRight[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataRight);
                                break;
                            }
                        case 7:
                            {
                                short[] depthDataFarRight = new short[heightPixel];
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataFarRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataFarRight[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                DataToCSVTraining(depthDataFarRight);
                                break;
                            }
                    }
                }
            }
        }
        /*
        short unkownDepthFinder(short unkownPixel)
        {
            short known = 0;
            short total = 1;
            ArrayList bandDepths = new ArrayList();
            short bandNum = 1;
            // Mode Verison
            do
            {
                for(int xSearch = -bandNum + x; xSearch > bandNum + x; xSearch++)
                {
                    for(int ySearch = -bandNum + y; ySearch > bandNum + y; ySearch++)
                    {
                        try
                        {
                            if(depthPixels[(xSearch) + (ySearch * widthPixel)].IsKnownDepth)
                            {
                                bandDepths.Add(depthPixels[(xSearch) + (ySearch * widthPixel)].Depth);
                                known++;
                                
                            }
                            
                        }
                        catch (System.IndexOutOfRangeException e)  // CS0168
                        {
                            // Set IndexOutOfRangeException to the new exception's InnerException.
                            throw new System.ArgumentOutOfRangeException("index parameter is out of range.", e);
                        }
                        total++;
                    }
                }
                bandNum++;
            } while ((known/total) >= 0.5);
            foreach (int i in bandDepths)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("END");
            /*
            short[] bandDepthsArray = bandDepths.ToArray(typeof(short)) as short[];
            short estimatedPixel = bandDepthsArray.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
            
            return mode;
        }
        */

        public short UnknownDepthFinder(int x, int y)
        {
            // /*
            short known = 0;
            short n = 3; //input number of pixels you want to search for before drawing conclusions / Lower is quicker but less accurate

            List<int> bandDepths = new List<int> { };
            short bandNum = 1;
            do
            {
                for(int xSearch = -bandNum + x; xSearch < bandNum + x; xSearch++)
                {
                    for(int ySearch = -bandNum + y; ySearch < bandNum + y; ySearch++)
                    {
                        if((ySearch >= 0 && ySearch <= 479) && (xSearch >= 0 && xSearch <= 639))
                        {
                            if (depthPixels[xSearch + (ySearch * widthPixel)].IsKnownDepth)
                            {
                                bandDepths.Add(depthPixels[(xSearch) + (ySearch * widthPixel)].Depth);
                                known++;
                            }
                        }
                    }
                }
                bandNum++;
            } while (known < n && bandNum <= 3); //input number of pixels you want to search for before drawing conclusions / Lower is quicker but less accurate


            short average;
            if (bandDepths.Count >= 1)
            {
                average = (short)(Convert.ToInt16(bandDepths.Average()));
            }
            else
            {
                average = 0;
            }
            return average;
        }
        string target;
        void DataToCSVTraining(short[] depthData)
        {
            
            determineTarget(position); //sets what the actual target

            string directory = "C:/Kinect Science Fair/Depth Data/"; //sets the directory for the file to be stored in

            string path = System.IO.Path.Combine(directory, "KinectDataRaw" + ".csv"); //creates the full file path for the csv file

            using (System.IO.StreamWriter sw = //makes the writer in append mode XXXXXXXXXXXXXXXXXXXXX consider making streamwriter at the beggining so it does not have to keep reopening
            new System.IO.StreamWriter(path, true))
            {
                //writes header (Already included in CSV file)
                // sw.WriteLine("Target, 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,313,314,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354,355,356,357,358,359,360,361,362,363,364,365,366,367,368,369,370,371,372,373,374,375,376,377,378,379,380,381,382,383,384,385,386,387,388,389,390,391,392,393,394,395,396,397,398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415,416,417,418,419,420,421,422,423,424,425,426,427,428,429,430,431,432,433,434,435,436,437,438,439,440,441,442,443,444,445,446,447,448,449,450,451,452,453,454,455,456,457,458,459,460,461,462,463,464,465,466,467,468,469,470,471,472,473,474,475,476,477,478,479");
                
                //writes what the actual target is for training
                sw.Write(target + ",");
                
                //search the depth data and add it to the file
                sw.WriteLine(string.Join(",", depthData));
                if (position == 1)
                {
                    
                    foreach (var item in depthData)
                    {
                        Console.Write(item.ToString() + ",");
                    }
                    
                }
                //dispose of sw
                sw.Close();
            }


        }

        void determineTarget(int positionNum)
        {
            switch (positionNum) //switch case using the position num to determine what the actual object is for the position when the export to csv is clicked. Used to train Neural Network
            {
                //postion orders are reversed because kinect registers a reversed image that was flipped on the application to better reflect what was being shown
                case 7:
                    {
                        target = FarLeftDropdown.SelectionBoxItem.ToString(); //gets the data from the selection box converts it to a string and stores it in the variable target
                        break; //breaks the switch case
                    }
                case 6:
                    {
                        target = LeftDropdown.SelectionBoxItem.ToString();
                        break;
                    }
                case 5:
                    {
                        target = CloseLeftDropdown.SelectionBoxItem.ToString();
                        break;
                    }
                case 4:
                    {
                        target = CenterDropdown.SelectionBoxItem.ToString();
                        break;
                    }
                case 3:
                    {
                        target = CloseRightDropdown.SelectionBoxItem.ToString();
                        break;
                    }
                case 2:
                    {
                        target = RightDropdown.SelectionBoxItem.ToString();
                        break;
                    }
                case 1:
                    {
                        target = FarRightDropdown.SelectionBoxItem.ToString();
                        break;
                    }
            }
        }

        short[] depthDataFarLeft = new short[480]; //creates a new short with a length of the number of pixels on the line profile
        short[] depthDataLeft = new short[480];
        short[] depthDataCloseLeft = new short[480];
        short[] depthDataCenter = new short[480];
        short[] depthDataCloseRight = new short[480];
        short[] depthDataRight = new short[480];
        short[] depthDataFarRight = new short[480];

        string FarLeftPrediction;
        string LeftPrediction;
        string CloseLeftPrediction;
        string CenterPrediction;
        string CloseRightPrediction;
        string RightPrediction;
        string FarRightPrediction;

        short counter = 0;

        /*//with smoothing real-time
        void realTimeDataCollection()
        {
            if (kinectSensor != null)
            {
                for (x = 80; x < widthPixel; x += 80) //goes across the line profiles by going from 80 , 160, 240, 320, 400, 480, 560
                {
                    position = x / 80; //changes postion depending on which line profile is getting scanned down
                    switch (position) //switch case depending on position to be stored in different short arrays for the line profile
                    {
                        case 1:
                            {
                                for (y = 0; y < heightPixel; y++) //goes down the line profile
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataFarLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataFarLeft[y] = UnknownDepthFinder(x, y);
                                    }

                                }
                                float[] depthDataFarLeftMinMax =  realTimeMinMax(depthDataFarLeft);
                                FarLeftPrediction = realTimeFeedback(depthDataFarLeftMinMax);
                                feedbackDisplay(FarLeftPrediction); 
                                break;
                            }
                        case 2:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataLeft[y] = UnknownDepthFinder(x, y);
                                    }
                                }
                                float[] depthDataLeftMinMax =  realTimeMinMax(depthDataLeft);
                                LeftPrediction = realTimeFeedback(depthDataLeftMinMax);
                                feedbackDisplay(LeftPrediction); 
                                break;
                            }
                        case 3:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCloseLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCloseLeft[y] = UnknownDepthFinder(x, y);
                                    }    
                                }
                                float[] depthDataCloseLeftMinMax = realTimeMinMax(depthDataCloseLeft);
                                CloseLeftPrediction = realTimeFeedback(depthDataCloseLeftMinMax);
                                feedbackDisplay(CloseLeftPrediction); 
                                break;
                            }
                        case 4:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCenter[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCenter[y] = UnknownDepthFinder(x, y);
                                    }    
                                }
                                float[] depthDataCenterMinMax = realTimeMinMax(depthDataCenter);
                                CenterPrediction = realTimeFeedback(depthDataCenterMinMax);
                                feedbackDisplay(CenterPrediction); 
                                break;
                            }
                        case 5:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataCloseRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataCloseRight[y] = UnknownDepthFinder(x, y);
                                    }                                    }
                                float[] depthDataCloseRightMinMax = realTimeMinMax(depthDataCloseRight);
                                CloseRightPrediction = realTimeFeedback(depthDataCloseRightMinMax);
                                feedbackDisplay(CloseRightPrediction); 
                                break;
                            }
                        case 6:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataRight[y] = UnknownDepthFinder(x, y);
                                    }    
                                }
                                float[] depthDataRightMinMax = realTimeMinMax(depthDataRight);
                                RightPrediction = realTimeFeedback(depthDataRightMinMax);
                                feedbackDisplay(RightPrediction); 
                                break;
                            }
                        case 7:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    if (depthPixels[x + y * widthPixel].IsKnownDepth)
                                    {
                                        depthDataFarRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                    }
                                    else
                                    {
                                        depthDataFarRight[y] = UnknownDepthFinder(x, y);
                                    }    
                                }
                                float[] depthDataFarRightMinMax = realTimeMinMax(depthDataFarRight);
                                FarRightPrediction = realTimeFeedback(depthDataFarRightMinMax);
                                feedbackDisplay(FarRightPrediction); 
                                break;
                            }
                    }
                }
                var synthesizer = new SpeechSynthesizer();
                synthesizer.SetOutputToDefaultAudioDevice();
                if (counter % 7 == 0)
                {
                    synthesizer.SpeakAsync(Hierarchy());
                    counter = 0; //each counter is .5 seconds
                }
                counter++;
            }
          
        }
        */

        //without smoothing real time
        void realTimeDataCollection()
        {
            if (kinectSensor != null)
            {
                for (x = 80; x < widthPixel; x += 80) //goes across the line profiles by going from 80 , 160, 240, 320, 400, 480, 560
                {
                    position = x / 80; //changes postion depending on which line profile is getting scanned down
                    switch (position) //switch case depending on position to be stored in different short arrays for the line profile
                    {
                        case 1:
                            {
                                for (y = 0; y < heightPixel; y++) //goes down the line profile
                                {
                                    depthDataFarLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataFarLeftMinMax = realTimeMinMax(depthDataFarLeft);
                                FarLeftPrediction = realTimeFeedback(depthDataFarLeftMinMax);
                                feedbackDisplay(FarLeftPrediction);
                                break;
                            }
                        case 2:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataLeftMinMax = realTimeMinMax(depthDataLeft);
                                LeftPrediction = realTimeFeedback(depthDataLeftMinMax);
                                feedbackDisplay(LeftPrediction);
                                break;
                            }
                        case 3:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataCloseLeft[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width                                       }
                                }
                                float[] depthDataCloseLeftMinMax = realTimeMinMax(depthDataCloseLeft);
                                CloseLeftPrediction = realTimeFeedback(depthDataCloseLeftMinMax);
                                feedbackDisplay(CloseLeftPrediction);
                                break;
                            }
                        case 4:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataCenter[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataCenterMinMax = realTimeMinMax(depthDataCenter);
                                CenterPrediction = realTimeFeedback(depthDataCenterMinMax);
                                feedbackDisplay(CenterPrediction);
                                break;
                            }
                        case 5:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataCloseRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataCloseRightMinMax = realTimeMinMax(depthDataCloseRight);
                                CloseRightPrediction = realTimeFeedback(depthDataCloseRightMinMax);
                                feedbackDisplay(CloseRightPrediction);
                                break;
                            }
                        case 6:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataRightMinMax = realTimeMinMax(depthDataRight);
                                RightPrediction = realTimeFeedback(depthDataRightMinMax);
                                feedbackDisplay(RightPrediction);
                                break;
                            }
                        case 7:
                            {
                                for (y = 0; y < heightPixel; y++)
                                {
                                    depthDataFarRight[y] = depthPixels[x + y * widthPixel].Depth; //adds the depth of the pixel to the short array and finds the depth of the particular pixel on the line profile by using its index found by x + y * width       
                                }
                                float[] depthDataFarRightMinMax = realTimeMinMax(depthDataFarRight);
                                FarRightPrediction = realTimeFeedback(depthDataFarRightMinMax);
                                feedbackDisplay(FarRightPrediction);
                                break;
                            }
                    }
                }
                var synthesizer = new SpeechSynthesizer();
                synthesizer.SetOutputToDefaultAudioDevice();
                if (counter % 7 == 0)
                {
                    synthesizer.SpeakAsync(Hierarchy());
                    counter = 0; //each counter is .5 seconds
                }
                counter++;
            }

        }
        
        float[] realTimeMinMax(short[] depthData)
        {
            float max = depthData.Max();
            float min = 999999;
            foreach(short i in depthData)
            {
                if (min > i && i > 0)
                {
                    min = i;
                }
            }
            float[] depthDataMinMax = new float[heightPixel];
            for (int i = 0; i < 480; i++)
            {
                if (depthData[i] != 0)
                {
                    depthDataMinMax[i] = (depthData[i] - min) / (max - min);
                }
            }
            return depthDataMinMax;
        }
        string realTimeFeedback(float[] depthDataMinMaxParam)
        {
            // Add input data
            var input = new ModelInput();
            PropertyInfo[] properties = typeof(ModelInput).GetProperties();
            
            
            for (int i = 1; i < 481; i++)
            {
                properties[i].SetValue(input, depthDataMinMaxParam[i-1]);
            }
            // Load model and predict output of sample data
            ModelOutput result = ConsumeModel.Predict(input);
            string prediction = result.Prediction;
            return prediction;
        }

        void feedbackDisplay(string prediction)
        {
            switch (position) //switch case using the position num to determine what the actual object is for the position when the export to csv is clicked. Used to train Neural Network
            {
                //postion orders are reversed because kinect registers a reversed image that was flipped on the application to better reflect what was being shown
                case 7:
                    {
                        FarLeftStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break; //breaks the switch case
                    }
                case 6:
                    {
                        LeftStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
                case 5:
                    {
                        CloseLeftStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
                case 4:
                    {
                        CenterStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
                case 3:
                    {
                        CloseRightStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
                case 2:
                    {
                        RightStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
                case 1:
                    {
                        FarRightStatusLabel.Text = prediction;
                        // synthesizer.Speak(prediction);
                        break;
                    }
            }
        }

        short[] depthObjects = new short[7];
        string[] predictionObjects = new string[7];
        string[] obstacleOrder = { "Upstairs", "Downstairs", "Known Obstacle", "Wall without Floor" };
        string Hierarchy()
        {
            predictionObjects = getPredictions();
            depthObjects = getDepths(predictionObjects);


            List<short> hierarchy = new List<short>();
            short position;
            int flag = 0;
            for (position = 0; position < 7; position++)
            {
                if (predictionObjects[position] != "No Obstacle" && predictionObjects[position] != "Wall with Floor" && predictionObjects[position] != "Floor")
                {
                    for (int i = 0; i < hierarchy.Count; i++)
                    {
                        if (depthObjects[position] < depthObjects[hierarchy[i]])
                        {
                            hierarchy.Insert(i, position);
                            flag = 1;
                            break;
                        }
                    }
                    if (flag == 0)
                    {
                        hierarchy.Add(position);
                    }
                    flag = 0;
                }
            }
            if (hierarchy.Count == 0)
            {
                Console.WriteLine("Count Issue 0");
                return "No Obstacle";
            }

            if (hierarchy.Count >= 2)
            {
                if (!(depthObjects[hierarchy[0]] > depthObjects[hierarchy[1]] * 1.15))
                {
                    if (predictionObjects[hierarchy[0]].Equals(predictionObjects[hierarchy[1]]))
                    {
                        //center proximity
                        int distance1 = Math.Abs(hierarchy[0] - 3);
                        int distance2 = Math.Abs(hierarchy[1] - 3);

                        if (distance1 != distance2)
                        {
                            //reorder
                            short temp = hierarchy[0];
                            hierarchy[0] = hierarchy[1];
                            hierarchy[1] = temp;
                        }
                    }
                    
                    else
                    {
                        //obstacle type condition
                        foreach (string elem in obstacleOrder)
                        {
                            if (predictionObjects[hierarchy[0]].Equals(elem))
                            {
                                break;
                            }

                            else if (predictionObjects[hierarchy[1]].Equals(elem))
                            {
                                short temp = hierarchy[0];
                                hierarchy[0] = hierarchy[1];
                                hierarchy[1] = temp;
                                break;
                            }
                        }
                       
                    }
                    
                }
            }

            Console.Write("Hierarchy: ");
            foreach (short pos in hierarchy)
            {
                Console.Write(depthObjects[pos] + " " + predictionObjects[pos]);
            }
            Console.WriteLine();
            if (depthObjects[hierarchy[0]] > 3000)
            {
                Console.WriteLine("Count Too Large");
                return "No Obstacle";
            }
            
            else if (depthObjects[hierarchy[0]] < 100)
            {
                Console.WriteLine("count too small");
                return "No Obstacle";
            }
            
            if (hierarchy.Count == 1)
            {
                Console.WriteLine("FAXC");
                string returnStatementCount1 = predictionObjects[hierarchy[0]] + " " + (int) (depthObjects[hierarchy[0]] / 10);
                return returnStatementCount1;
            }

            string returnStatement = predictionObjects[hierarchy[0]] + " " + (int) (depthObjects[hierarchy[0]] / 10);
            return returnStatement;
            //analyze depth (determine depth TBA) (Create list of depth orders)
            //analyze prediction (use prediciton from model) (Analyze list index 0 and 1 to determine if they are close (if they are then run hierarchy of predictions))

            //prozimity to center

            //return callout w depth or wo depth
        }


        short[] getDepths(String[] predictions)
        { //change to array
            short[] depthTemp = new short[7];
            depthTemp[0] = 0;
            depthTemp[1] = 0;
            depthTemp[2] = 0;
            depthTemp[3] = 0;
            depthTemp[4] = 0;
            depthTemp[5] = 0;
            depthTemp[6] = 0;

            short[][] allDepths = new short[7][];
            allDepths[0] = depthDataFarLeft;
            allDepths[1] = depthDataLeft;
            allDepths[2] = depthDataCloseLeft;
            allDepths[3] = depthDataCenter;
            allDepths[4] = depthDataCloseRight;
            allDepths[5] = depthDataRight;
            allDepths[6] = depthDataFarRight;

            for (int i = 0; i < 7; i++)
            {
                string predictionTarget = predictions[i];

                switch (predictionTarget)
                {
                    case "Known Obstacle":
                        short test = 0;
                        for (int j = 479; j >= 4; j--)
                        {
                            if ((allDepths[i][j] != 0) && allDepths[i][j] + 200 < allDepths[i][j - 4])
                            {
                                test = allDepths[i][j];
                                break;
                            }
                        }

                        int sumTest = 0;
                        short countTest = 1;
                        bool changed = false;
                        for (int j = 0; j < 480; j++)
                        {
                            sumTest += allDepths[i][j];
                            Console.Write(allDepths[i][j] + " ");
                            if (allDepths[i][j] != 0)
                            {
                                if (changed)
                                {
                                    countTest++;
                                }
                                else
                                {
                                    changed = true;
                                }
                            }
                        }
                        short average = (short)(sumTest / countTest);

                        Console.WriteLine();
                        Console.WriteLine(test);
                        Console.WriteLine(average);
                        Console.WriteLine(sumTest);
                        Console.WriteLine(countTest);
                        if (average < test || test == 0)
                        {
                            depthTemp[i] = average;
                        }
                        else
                        {
                            depthTemp[i] = test;
                        }

                        int countTemp = 0;
                        for (int k = 0; k < 192;  k++)
                        {
                            if (allDepths[i][k] == 0)
                            {
                                countTemp++;
                            }
                        }
                        if (countTemp> 150)
                        {
                            predictionObjects[i] = "Floor";
                            Console.WriteLine(predictionObjects[i]);
                            depthTemp[i] = 0;
                        }
                        break;
                    case "Upstairs":
                        for (int j = 479; j >= 4; j--)
                        {
                            if ((allDepths[i][j] != 0) && allDepths[i][j] + 200 < allDepths[i][j - 4])
                            {
                                depthTemp[i] = allDepths[i][j];
                                break;
                            }
                        }
                        break;
                    case "Downstairs":
                        short min = allDepths[i][0];
                        for (int j = 1; j < 480; j++)
                        {
                            if (min != 0 && min > allDepths[i][j])
                            {
                                min = allDepths[i][j];
                            }
                        }
                        depthTemp[i] = min;
                        break;
                    case "Wall without Floor":
                        int sum = 0;
                        short count = 0;
                        for (int j = 0; j < 480; j++)
                        {
                            sum += allDepths[i][j];
                            if (allDepths[i][j] != 0)
                            {
                                count++;
                            }
                        }
                        depthTemp[i] = (short)(sum / count);
                        break;
                }

                //algorithms


                //wall w floor
                /*
                else if (predictionTarget.Equals("Wall with Floor"))
                {
                    short max = allDepths[i][0];
                    for (int j = 1; j < 480; j++)
                    {
                        if (max < allDepths[i][j])
                        {
                            max = allDepths[i][j];
                        }
                    }
                    depthTemp[i] = max;
                }
                */
            }
            return depthTemp;
        }
        string[] getPredictions()
        {
            string[] predictionTemp = new string[7];
            predictionTemp[0] = FarLeftPrediction;
            predictionTemp[1] = LeftPrediction;
            predictionTemp[2] = CloseLeftPrediction;
            predictionTemp[3] = CenterPrediction;
            predictionTemp[4] = CloseRightPrediction;
            predictionTemp[5] = RightPrediction;
            predictionTemp[6] = FarRightPrediction;
            return predictionTemp;
        }
    }
}