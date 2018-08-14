﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fleck;
using OpenGloveApp.AppConstants;
using OpenGloveApp.CustomEventArgs;
using OpenGloveApp.OpenGloveAPI;

namespace OpenGloveApp.Server
{
    public class OpenGloveServer: Server
    {
        public static event EventHandler<WebSocketEventArgs> WebSocketMessageReceived;

        private string url;
        private WebSocketServer server; // sample "ws://127.0.0.1:7070"
        private List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        private static Dictionary<string, IWebSocketConnection> webSocketByDeviceName = new Dictionary<string, IWebSocketConnection>();
        public static Dictionary<string, OpenGlove> OpenGloveByDeviceName = new Dictionary<string, OpenGlove>();


        public OpenGloveServer(string url)
        {
            this.url = url;
        }

        public void StartWebsockerServer()
        {
            FleckLog.Level = LogLevel.Debug;
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Debug.WriteLine("Open WebSocket!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                { 
                    Debug.WriteLine("Close WebSocket!");
                    allSockets.Remove(socket);
                    webSocketByDeviceName.ContainsValue(socket);
                };
                socket.OnMessage = message =>
                {
                    HandleMessage(socket, message);
                };
            });
        }

        public void CloseAllSockets()
        {
            allSockets.ToList().ForEach(s => s.Close());
        }

        override public void Start()
        {
            this.server = new WebSocketServer(url);
            ConfigureServer();
            StartWebsockerServer();
        }

        override public void Stop()
        {
            CloseAllSockets();
            server.Dispose(); //this method does not allow more incoming connections, and disconnect the server. So it is necessary to disconnect all sockets first to cut off all communication
            allSockets.Clear();
        }

        override public void ConfigureServer()
        {
            server.RestartAfterListenError = true;
        }

        /* WebSocket format message:  Action;DeviceName;Intensity;Regions
         * sample: Activate;OpenGloveIZQ;255;0,2,6,7,29
         * Action:      one of this Actions : Activate, StartCaptureData, StopCaptureData,
         * DeviceName:  Name of bluetooth device to send the command
         * Intensity:   0 to 255
         * Regions:     a list of regions to activate (int)
        */
        // Handle Message from WebSocket Client
        private void HandleMessage(IWebSocketConnection socket, string message)
        {
            Debug.WriteLine(message);
            string[] words;
            int MinCountMessageSplit = 2; // ACTION;DEVICE
            int MaxCountMessageSplit = 4; // ACTION;DEVICE;REGION;VALUE

            if (message != null)
            {
                try
                {
                    words = message.Split(';');
                    int count = words.Length;

                    int action = -1;
                    string deviceName = null;
                    string regions = null;
                    string values = null; // intensities or pins

                    if(count >= MinCountMessageSplit && count <= MaxCountMessageSplit)
                    {
                        if(count == 2)
                        {
                            action = Int32.Parse(words[0]);
                            deviceName = words[1];
                        }

                        if (count == 3)
                        {
                            action = Int32.Parse(words[0]);
                            deviceName = words[1];
                            regions = words[2];
                        }

                        if(count == 4)
                        {
                            action = Int32.Parse(words[0]);
                            deviceName = words[1];
                            regions = words[2];
                            values = words[3]; // intensities or pins
                        }

                        OnWebSocketMessageReceived(socket, message, action, deviceName, regions, values);
                    }
                }
                catch
                {
                    Debug.WriteLine("WebSocketServer ERROR: BAD FORMAT in HandleMessage");
                }
            }

        }

        // Method for raise event to subcribers (aka. Connected Thread on Communication implementation iOS/Android of OpenGlove devices)
        protected virtual void OnWebSocketMessageReceived(IWebSocketConnection socket, string message, int what, string deviceName, string regions, string values)
        {
            int Region = -1;
            List<int> Regions = null;
            List<int> Intensities = null;
            int Pin = -1;
            List<int> Pins = null;
            List<string> Values = null;
            try {
                switch (what)
                {
                    case (int)OpenGloveActions.StartCaptureData:
                        webSocketByDeviceName.Add(deviceName, socket);
                        break;

                    case (int)OpenGloveActions.StopCaptureData:
                        webSocketByDeviceName.Remove(deviceName);
                        break;

                    case (int)OpenGloveActions.AddFlexor:
                        Region = Int32.Parse(regions);
                        Pin = Int32.Parse(values);
                        WebSocketMessageReceived?.Invoke(this, new WebSocketEventArgs()
                        { What = what, DeviceName = deviceName, Message = message, Region = Region, Pin = Pin });
                        break;

                    case (int)OpenGloveActions.AddFlexors:
                        Regions = regions.Split(',').Select(int.Parse).ToList();
                        Pins = values.Split(',').Select(int.Parse).ToList();
                        WebSocketMessageReceived?.Invoke(this, new WebSocketEventArgs()
                        { What = what, DeviceName = deviceName, Message = message, Regions = Regions, Pins = Pins });
                        break;

                    case (int)OpenGloveActions.RemoveFlexor:
                        Region = Int32.Parse(regions);
                        WebSocketMessageReceived?.Invoke(this, new WebSocketEventArgs()
                        { What = what, DeviceName = deviceName, Message = message, Region = Region });
                        break;

                    case (int)OpenGloveActions.RemoveFlexors:
                        Regions = regions.Split(',').Select(int.Parse).ToList();
                        WebSocketMessageReceived?.Invoke(this, new WebSocketEventArgs()
                        { What = what, DeviceName = deviceName, Message = message, Regions = Regions });
                        break;

                    case (int)OpenGloveActions.ActivateActuators:
                        // Transform string to list of regions and intensities
                        //List<int> regions = words[2].Split(',').Select(int.Parse).ToList();
                        //List<string> instensities = words[3].Split(',').ToList();
                        //List<Actuator> actuators = 

                        //OnWebSocketDataReceived(OpenGloveActions.ActivateActuators, words[1], );
                        break;
                    default:
                        socket.Send("You said: " + message); // test echo message
                        break;
                }

                WebSocketMessageReceived?.Invoke(this, new WebSocketEventArgs()
                {
                    What = what,
                    DeviceName = deviceName,
                    Message = message,
                    Region = Region,
                    Regions = Regions,
                    Intensities = Intensities,
                    Pin = Pin,
                    Pins = Pins,
                    Values = Values
                });                
            } 
            catch 
            {
                Debug.WriteLine("WebSocketServer ERROR: BAD FORMAT OR TYPE DATA in OnWebSocketMessageReceived");
            }

        }

        // Method for subcribe to messages from OpenGlove Devices
        public static void OnBluetoothMessage(object source, BluetoothEventArgs e)
        {
            SendDataIfWebSocketRequestedDataFromDevice(e);
        }

        public static void SendDataIfWebSocketRequestedDataFromDevice(BluetoothEventArgs e)
        {
            if (e.Message != null)
            {
                //Debug.WriteLine($" [{e.DeviceName}] OpenGloveServer.OnBluetoothMessage: {e.Message}");
                if (webSocketByDeviceName.ContainsKey(e.DeviceName))
                    webSocketByDeviceName[e.DeviceName].Send(e.Message);
            }
        }

    }
}