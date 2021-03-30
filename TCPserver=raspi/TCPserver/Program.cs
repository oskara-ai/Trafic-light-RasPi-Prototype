using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TCPserver.Models;

namespace TCPserver
{
    class MainClass
    {
        public static void Main()
        {
            TcpListener server = null;
            try
            {
                // set tcp listener
				int port = 1234;
                //IPAddress[] localAddr = Dns.GetHostAddresses("");
				//IPAddress address = IPAddress.Parse("");
                IPAddress address = IPAddress.Any;
                
                //server = new TcpListener(localAddr[0], port);
				server = new TcpListener(address, port);

                //Start listening for client request
                server.Start();

                //buffer for reading data
                Byte[] bytes = new Byte[5068];
                String data = null;
                ExecuteCommand("gpio mode 23 out"); // green
                ExecuteCommand("gpio mode 24 out"); // yellow
                ExecuteCommand("gpio mode 25 out"); // red
                
                //Entering listening loop
                while(true)
                {
                    Console.WriteLine("Waiting for connection");

                    //preform a blocking call to accept request.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;
                    string sendData = null;
                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;
                    
                    // Loop for receive all the data sen by client.
                    while((i = stream.Read(bytes, 0, bytes.Length)) !=0)
                    {
                        //Translate data bytes to ASCII string
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);
                        List<SequenceSelect> SS = new List<SequenceSelect>();
                        try
                        {
                            SS = JsonConvert.DeserializeObject<List<SequenceSelect>>(data);
                            ExecuteCommand("gpio mode 23 out"); // green
                            ExecuteCommand("gpio mode 24 out"); // yellow
                            ExecuteCommand("gpio mode 25 out"); // red
                            foreach(var com in SS)
                            {
                                OverRide Work = new OverRide();
                                
                                if(com.Led == "green"){
                                    Work.Green = com.Command;
                                }
                                else{
                                    Work.Green = 0;
                                }
                                if (com.Led == "red")
                                {
                                    Work.Red = com.Command;
                                }
                                else
                                {
                                    Work.Red = 0;
                                }
                                if (com.Led == "yellow")
                                {
                                    Work.Yellow = com.Command;
                                }
                                else
                                {
                                    Work.Yellow = 0;
                                }
                                
                                var dm = Task.Run(async delegate{
                                    await Task.Delay(com.Timer);
                                });
                                dm.Wait();
                                sendData = Parser(JsonConvert.SerializeObject(Work));
                            }
                        }
                        catch{
                            
                        }
                        //process data send by the client
                        if(SS.Count == 0){
                            sendData = Parser(data);
                        }

                        byte[] msg = Encoding.ASCII.GetBytes(sendData);

                        //send back a response
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Send: {0}", sendData);
                    }
                    //shutdown and en connection
                    client.Close();
                }
            }
            catch(SocketException e){
                Console.WriteLine("SocketException: {0}", e);
            }
            finally{
                // stop listening for new clients
                server.Stop();
            }
        }//end Main

        public static string Parser(string data)
        {
            OverRide ui = new OverRide();
            char[] c = { '"' };
            string[] data1 = data.Split(c);
            
            if(data1[3] == "OR")
            {
                ui = JsonConvert.DeserializeObject<OverRide>(data);
                
                if(ui.Green == 1)
                {
                    Console.WriteLine("Green on");
                    string status = ExecuteCommand("gpio read 23");
                    if(status == "0"){
                        Console.WriteLine(ExecuteCommand("gpio write 23 1"));
                    }
                    else if(status == "1"){
                        Console.WriteLine(ExecuteCommand("gpio write 23 0"));
                    }
                }

                else if(ui.Red == 1)
                {
                    Console.WriteLine("Red on");
                    string status = ExecuteCommand("gpio read 25");
                    if(status == "0"){
                        Console.WriteLine(ExecuteCommand("gpio write 25 1"));
                    }
                    else if (status == "1"){
                        Console.WriteLine(ExecuteCommand("gpio write 25 0"));
                    }
                }

                else if (ui.Yellow == 1)
                {
                    Console.WriteLine("Yellow on");
                    string status = ExecuteCommand("gpio read 24");
                    if (status == "0") {
                        Console.WriteLine(ExecuteCommand("gpio write 24 1"));
                    }
                    else if (status == "1"){
                        Console.WriteLine(ExecuteCommand("gpio write 24 0"));
                    }
                }
                ui = StatusCheck(ui);
                // ui variable update and send back
            }
            if(data == "StatusCheck")
            {
                ui = JsonConvert.DeserializeObject<OverRide>(data);
                ui = StatusCheck(ui);
            }
            return JsonConvert.SerializeObject(ui);
        }// end parser

        public static OverRide StatusCheck(OverRide ui)
        {
            ui.Green = int.Parse(ExecuteCommand("gpio read 23"));
            ui.Yellow = int.Parse(ExecuteCommand("gpio read 24"));
            ui.Red = int.Parse(ExecuteCommand("gpio read 25"));
            return ui;
        }//end StatusCheck

        public static string ExecuteCommand(string command)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \" ";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            
            while(!proc.StandardOutput.EndOfStream){
                return proc.StandardOutput.ReadLine();
            }
            return "No command";
        }//end ExecuteCommand
    }
}

