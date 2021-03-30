using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TraficLight.Models;
using System.IO;
using Newtonsoft.Json;

namespace TraficLight
{
    public partial class MainWindow : Window
    {

        private static OverRide orTimed = new OverRide();
        private AddSequence AddSequ = new AddSequence();

        public MainWindow()
        {
            InitializeComponent();
            UpdateList();
            
        }
        // Get info about Led:s state
        private void ButGetInfo_Click(object sender, RoutedEventArgs e)
        {
            OverRide ui = new OverRide();
            string msg = JsonConvert.SerializeObject(ui);
            StatusCheck(msg);
        }
        // show messagebox that contains instructions
        private void Instruction_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("----------------------------------------\n" +
                "----------------Instruction-------------------\n" +
                "----------------------------------------------\n" + 
                "LEDS condition: Gives leds status at the moment \n" +
                "Sequence name: User defined name for the control sequence\n" +
                "Delay in seconds: User defined delay for the leds (before LED turn on\n" +
                "Choose LED that you want to turn on \n" +
                " then click AddNew that adds command to the list\n" +
                "-----------Don't try to write manually--------\n" +
                "Double clicking sequence list starts command \n" +
                "By choosing sequence you can see time thats required to run command in list below \n" +
                "When making sequences all of the leds are turned off and they are controlled either on or off\n" +
                "depending on leds status when command is coming\n" +
                "Example LED: Green Timer:1 2x (1s Green led on -- 1s Green led off\n" +
                "\n" );
        }
        // change state of green led
        private void Green_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OverRide ui = new OverRide
            {
                Green = 1,
                Red = 0,
                Yellow = 0
            };
            string msg = JsonConvert.SerializeObject(ui);
            StatusCheck(msg);
        }
        // change state of yellow led
        private void Yellow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OverRide ui = new OverRide
            {
                Green = 0,
                Red = 0,
                Yellow = 1
            };
            string msg = JsonConvert.SerializeObject(ui);
            StatusCheck(msg);
        }
        // change state of red led
        private void Red_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OverRide ui = new OverRide
            {
                Green = 0,
                Red = 1,
                Yellow = 0
            };
            string msg = JsonConvert.SerializeObject(ui);
            StatusCheck(msg);
        }
        // Delete button
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if(CombinationsList.SelectedIndex != -1)
            {
                string filepath = AppDomain.CurrentDomain.BaseDirectory + $"Combinations/" 
                    + CombinationsList.Items[CombinationsList.SelectedIndex].ToString();
                if (File.Exists(filepath))
                    File.Delete(filepath);
                CombinationsList.Items.RemoveAt(CombinationsList.SelectedIndex);
            }
        }
        // Add sequence to the list
        private void AddCombinationRow_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxAdd.Text != "") { TextBoxAdd.Text += ","; }
            LEDsequence ls = new LEDsequence
            {
                Led = ((ListBoxItem)LedToAdd.SelectedItem).Content.ToString(),
                Timer = (int)(double.Parse(addIntervall.Text) * 1000),
                Command = 1
            };
            TextBoxAdd.Text += JsonConvert.SerializeObject(ls);
        }
        // save sequences 
        private void SaveBut_Click(object sender, RoutedEventArgs e)
        {
            AddSequ.name = CombinationName.Text;
            TextBoxAdd.Text += "]";
            TextBoxAdd.Text = $"[{TextBoxAdd.Text}";
            AddSequ.Combinations = JsonConvert.DeserializeObject<List<LEDsequence>>(TextBoxAdd.Text);

            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Combinations");
            using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + $"Combinations/{AddSequ.name}.jjj"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, AddSequ);
            }
            UpdateList();
            TextBoxAdd.Text = "";
            CombinationName.Text = "";
            addIntervall.Text = "";
        }

        //run sequence with double click
        private void CombinationsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if((sender as ListBox).SelectedItem is string filename)
            {
                string msg;

                using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory 
                    + $"Combinations/{filename}"))
                {
                    msg = r.ReadToEnd();
                }
                AddSequence ads = JsonConvert.DeserializeObject<AddSequence>(msg);
                List<LEDsequence> ls = new List<LEDsequence>();
                ls = ads.Combinations;

                StatusCheck(JsonConvert.SerializeObject(ls));
            }
        }
        // Show run time with ListView
        private void CombinationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((sender as ListBox).SelectedItem is string filename)
            {
                string msg;

                using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory 
                    + $"Combinations/{filename}"))
                {
                    msg = r.ReadToEnd();    // lukee tiedostosta valitun sekvenssin

                }
                AddSequence ads = JsonConvert.DeserializeObject<AddSequence>(msg);
                List<LEDsequence> ls = new List<LEDsequence>();
                ls = ads.Combinations;
                int Lasting = 0;
                foreach (var i in ls)
                {
                    Lasting += i.Timer;
                }
                LastingTime.Content = $"Run time {(double)Lasting / 1000} sec";

            }
        }
        
        public void StatusCheck(String message)
        {
            try
            {

                //using hardcode ip address for raspi
                string server = ""; 
                MainWindow ErrorTextBox = this;
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 1234;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                // Buffer to store the response bytes.
                data = new Byte[5024];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //ErrorSection.Text += responseData;

                char[] c = { '"' };
                string[] check = responseData.Split(c);
                if (check[3] == "OR")
                {
                    OverRide or = new OverRide();
                    or = JsonConvert.DeserializeObject<OverRide>(responseData);
                    LightStatusUpdate(or);
                }
                stream.Close();
                client.Close();
                // Close everything.


                ErrorTextBox.Update(responseData);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            Console.WriteLine("\n Press enter to continue");
            Console.Read();
        }

        public void Update (string msg)
        {
            ErrorSection.Text += msg;
        }
        //update lights
        private void LightStatusUpdate (OverRide ovr)
        {
            if (ovr.Green == 1)
            {
                Green.Fill = Brushes.Green;
            }
            else
            {
                Green.Fill = Brushes.LightGreen;
            }
            if (ovr.Red == 1)
            {
                Red.Fill = Brushes.Red;
            }
            else
            {
                Red.Fill = Brushes.LightPink;
            }
            if (ovr.Yellow == 1)
            {
                Yellow.Fill = Brushes.Yellow;
            }
            else
            {
                Yellow.Fill = Brushes.LightYellow;
            }
        }
        // update saved sequence list
        private void UpdateList()
        {
            CombinationsList.Items.Clear();
            string[] fileEntries = Directory.GetFileSystemEntries(AppDomain.CurrentDomain.BaseDirectory
                + "Combinations").Select(s => Path.GetFileName(s)).ToArray();
            foreach (string fileName in fileEntries)
                CombinationsList.Items.Add(fileName);
        }
    }
}
