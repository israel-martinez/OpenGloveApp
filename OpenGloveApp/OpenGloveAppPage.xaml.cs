﻿using Xamarin.Forms;
using OpenGloveApp.Models;
using System;
using System.Collections.Generic;
using OpenGloveApp.OpenGloveAPI;
using OpenGloveApp.Server;

namespace OpenGloveApp
{
    public partial class OpenGloveAppPage : ContentPage
    {
        // Publisher for UI commands to bluetoothManager
        public event EventHandler<BluetoothEventArgs> BluetoothMessageSended;
        public bool isMotorActive = false;
        public bool isMotorInitialize = false;
        public const int INITIALIZE_MOTORS = 1;
        public const int ACTIVATE_MOTORS = 2;
        public const int DISABLE_MOTORS = 3;
        public const int FLEXOR_READ = 4;

        public const int EVALUATION_DONE = 1000;
        public const int FLEXOR_EVALUATION = 500;
        public const int MOTOR_EVALUATION = 501;
        public const int IMU_EVALUATION = 502;

        // Vibe board: (+11 y -12), (+10 y -15), (+9 y -16), (+3 y -2), (+6, -8)
        public static List<int> mPins = new List<int> { 11, 12, 10, 15, 9, 16, 3, 2, 6, 8 };
        public static List<string> mValuesON = new List<string> { "HIGH", "LOW", "HIGH", "LOW", "HIGH", "LOW", "HIGH", "LOW", "HIGH", "LOW" };
        public static List<string> mValuesOFF = new List<string> { "LOW", "LOW", "LOW", "LOW", "LOW", "LOW", "LOW", "LOW", "LOW", "LOW" };

        // Flexor pins: 17 and  + and -
        public List<int> mFlexorPins = new List<int> { 17 };
        public List<int> mFlexorMapping = new List<int> { 8 };
        public List<string> mFlexorPinsMode = new List<string> { "OUTPUT" };

        public OpenGlove mOpenGlove = new OpenGlove();
        public OpenGloveServer mServer = new OpenGloveServer("ws://127.0.0.1:8181");
            
        public OpenGloveAppPage()
        {
            InitializeComponent();
            buttonActivateMotor.Text = "Motor OFF";
            mServer.Start();
        }

        //Subcriber method
        public void OnBluetoothMessage(object source, BluetoothEventArgs e)
        {
            // Handler for message from bluetooth ConnectedThread
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e.Message != null)
                {
                    double value = double.Parse(e.Message);
                    progressBar_flexor_value.Progress = (value / 300);
                    label_flexor_value.Text = e.Message;
                }
            });
        }

        // Method to raise event
        protected virtual void OnBluetoothMessageSended(int what, IEnumerable<int> pins, IEnumerable<string> values)
        {
            BluetoothMessageSended(this, new BluetoothEventArgs()
            { What = what, Pins = pins, ValuesON = values, ValuesOFF = values });
        }

        void ShowBoundedDevices_Clicked(object sender, System.EventArgs e)
        {
            listViewBoundedDevices.ItemsSource = mOpenGlove.GetAllPairedDevices();
        }

        void ButtonActivateMotor_Clicked(object sender, System.EventArgs e)
        {
            if (!isMotorInitialize)
            {
                OnBluetoothMessageSended(INITIALIZE_MOTORS, mPins, mValuesOFF);
                isMotorInitialize = true;
            }

            if (isMotorActive)
            {
                buttonActivateMotor.Text = "Motor OFF";
                OnBluetoothMessageSended(DISABLE_MOTORS, mPins, mValuesOFF);
                isMotorActive = false;
            }
            else
            {
                buttonActivateMotor.Text = "Motor ON";
                OnBluetoothMessageSended(ACTIVATE_MOTORS, mPins, mValuesON);
                isMotorActive = true;
            }
        }

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            listViewBoundedDevices.SelectedItem = null;
        }

        async void Handle_ItemTappedAsync(object sender, ItemTappedEventArgs e)
        {
            var device = e.Item as BluetoothDevices;
            bool connect = await DisplayAlert("Try Connecting", $" Device: {device.Name} \n MAC Address: {device.Address}", "Connect", "Cancel");

            //Blocking call
            if (connect)
                mOpenGlove.OpenDeviceConnection(this, device);
        }
    }
}
