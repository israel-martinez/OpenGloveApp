﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenGloveApp.OpenGloveAPI
{
    /// <summary>
    /// Provide methods to generate communication message with the glove, following the OpenGlove communication protocol
    /// </summary>
    public class MessageGenerator
    {
        /// <summary>
        /// Symbol that separate digits in the message
        /// </summary>
        private string separator = ",";
        /// <summary>
        /// Symbol that indicate the end of the message
        /// </summary>
        private string terminal = "s";
        /// <summary>
        /// Number that identifies the message like initializeMotor function in the control software
        /// </summary>
        private string initializeMotorFunctionNumber = "1";
        /// <summary>
        ///  Number that identifies the message like activateMotor function in the control software
        /// </summary>
        private string activateMotorFunctionNumber = "2";
        /// <summary>
        /// Number that identifies the message like analogRead function in the control software
        /// </summary>
        private string analogReadFunctionNumber = "3";
        /// <summary>
        /// Number that identifies the message like digitalRead function in the control software
        /// </summary>
        private string digitialReadFunctionNumber = "4";
        /// <summary>
        /// Number that identifies the message like pinMode function in the control software
        /// </summary>
        private string pinModeInputFunctionNumber = "6";
        /// <summary>
        /// Number that identifies the message like digitalWrite function in the control software
        /// </summary>
        private string digitalWriteInputFunctionNumber = "7";
        /// <summary>
        /// Number that identifies the message like analogWrite function in the control software
        /// </summary>
        private string analogWriteInputFunctionNumber = "8";

        /// <summary>
        /// Description
        /// </summary>
        private string addFlexorInputFunctionNumber = "10";
        /// <summary>
        /// Description
        /// </summary>
        private string removeFlexorInputFunctionNumber = "11";
        /// <summary>
        /// Description
        /// </summary>
        private string calibrateFlexorsInputFunctionNumber = "12";
        /// <summary>
        /// Description
        /// </summary>
        private string setThresholdInputFunctionNumber = "13";
        /// <summary>
        /// Description
        /// </summary>
        private string resetFlexorsInputFunctionNumber = "14";
        /// <summary>
        /// Description
        /// </summary>
        private string startIMUFunctionNumber = "20";
        /// <summary>
        /// Description
        /// </summary>
        private string setIMUStatusFunctionNumber = "21";
        /// <summary>
        /// Description
        /// </summary>
        private string setRawDataFunctionNumber = "22";
        /// <summary>
        /// Description
        /// </summary>
        private string setLoopDelayNumber = "0";


        /// <summary>
        /// Generate a message to initialize pins like motors in the control software
        /// </summary>
        /// <param name="pins">List of pins that  are initialized</param>
        /// <returns>A string with the "initializeMotor" format specified in the OpenGlove communication protocol</returns>
        public string InitializeMotor(IEnumerable<int> pins)
        {
            if (pins.Count() == 0)
            {
                throw new System.ArgumentException("List must have at least one element");
            }
            var initializeMessage = new StringBuilder();

            initializeMessage.Append(initializeMotorFunctionNumber + separator + pins.Count());

            foreach (int pin in pins)
            {
                var message = separator + pin;
                initializeMessage.Append(message);
            }

            initializeMessage.Append(terminal);


            return initializeMessage.ToString();

        }
        /// <summary>
        /// Generate a message to activate motors. Each motor is activated with the value in the same index
        /// </summary>
        /// <param name="pins">List of pins where are connected the motors</param>
        /// <param name="values">List with the intensities to activate the motors. It can be "HIGH" or "LOW" in digital mode or a number between 0 and 255 in analog mode</param>
        /// <returns>A string with the "activateMotor" format specified in the OpenGlove communication protocol</returns>
        public string ActivateMotor(IEnumerable<int> pins, IEnumerable<string> values)
        {

            if (pins.Count() != values.Count())
            {
                throw new System.ArgumentException("Lists length must be equal");
            }

            var activateMessage = new StringBuilder();

            activateMessage.Append(activateMotorFunctionNumber + separator + pins.Count());

            for (var i = 0; i < pins.Count(); i++)
            {
                var value = "";

                if (values.ElementAt(i) == "HIGH")
                {
                    value = "-1";
                }

                else if (values.ElementAt(i) == "LOW")
                {
                    value = "-2";
                }

                else
                {
                    try
                    {

                        var valueAux = Int32.Parse(values.ElementAt(i));

                        if ((valueAux < 256) && (valueAux >= 0))
                        {
                            value = valueAux.ToString();
                        }

                        else
                        {
                            throw new ArgumentException("Values must be between 0 and 255");
                        }

                    }


                    catch (System.FormatException e)
                    {
                        Debug.WriteLine(e.StackTrace);
                        throw new ArgumentException("Invalid value " + values.ElementAt(i));
                    }

                }

                var message = separator + pins.ElementAt(i) + separator + value;
                activateMessage.Append(message);
            }

            activateMessage.Append(terminal);

            return activateMessage.ToString();

        }
        /// <summary>
        /// Generate a message to read from an analog pin
        /// </summary>
        /// <param name="pin">Number of the pin to be read</param>
        /// <returns>A string with the "analogRead" format specified in the OpenGlove communication protocol</returns>
        public string AnalogRead(int pin)
        {
            string message = analogReadFunctionNumber + separator + pin + terminal;
            return message;
        }
        /// <summary>
        /// Generate a message to read from a digital pin
        /// </summary>
        /// <param name="pin">Number of the pin to be read</param>
        /// <returns> A string with the "digitalRead" format specified in the OpenGlove communication protocol</returns>
        public string DigitalRead(int pin)
        {
            string message = digitialReadFunctionNumber + separator + pin + terminal;
            return message;
        }
        /// <summary>
        /// Generate a message to initialize a pin in input or output mode
        /// </summary>
        /// <param name="pin">Number of the pin to be initialized</param>
        /// <param name="mode">Mode to initialize the pin, it can be "INPUT" or "OUTPUT"</param>
        /// <returns>A string with the "pinMode" format specified in the OpenGlove communication protocol</returns>
        public string PinMode(int pin, string mode)
        {
            string modeProtocol = "";

            if (mode == "INPUT")
            {
                modeProtocol = "1";
            }

            else if (mode == "OUTPUT")
            {
                modeProtocol = "2";
            }

            else
            {
                throw new System.ArgumentException(mode + " is not a valid mode");
            }

            string message = pinModeInputFunctionNumber + separator + "1" + separator + pin + separator
                + modeProtocol + terminal;
            return message;
        }
        /// <summary>
        /// Generate a message to initialize a pin in input or output mode. Each pin is initialized with the mode in the same index
        /// </summary>
        /// <param name="pins">List with the numbers of the pins to be initialized</param>
        /// <param name="modes">List with the modes to initialize the pins, it can be "INPUT" or "OUTPUT"</param>
        /// <returns>A string with the "pinMode" format specified in the OpenGlove communication protocol</returns>
        public string PinMode(IEnumerable<int> pins, IEnumerable<string> modes)
        {
            if (pins.Count() != modes.Count())
            {
                throw new System.ArgumentException("Collections length must be equal");
            }

            var pinModeMessage = new StringBuilder();
            pinModeMessage.Append(pinModeInputFunctionNumber + separator + pins.Count());

            for (var i = 0; i < pins.Count(); i++)
            {
                string modeProtocol = "";

                if (modes.ElementAt(i) == "INPUT")
                {
                    modeProtocol = "1";
                }

                else if (modes.ElementAt(i) == "OUTPUT")
                {
                    modeProtocol = "2";
                }

                else
                {
                    throw new System.ArgumentException(modes.ElementAt(i) + " is not a valid mode");
                }

                var message = separator + pins.ElementAt(i) + separator + modeProtocol;
                pinModeMessage.Append(message);

            }

            pinModeMessage.Append(terminal);
            return pinModeMessage.ToString();

        }
        /// <summary>
        /// Generate a message to write a value in a digital pin
        /// </summary>
        /// <param name="pin">Number of the pin to be write</param>
        /// <param name="value">Value to be write in the pin, it can be "HIGH" or "LOW"</param>
        /// <returns>A string with the "digitalWrite" format specified in the OpenGlove communication protocol</returns>
        public string DigitalWrite(int pin, string value)
        {
            string valueProtocol = "";

            if (value == "LOW")
            {
                valueProtocol = "0";
            }

            else if (value == "HIGH")
            {
                valueProtocol = "1";
            }

            else
            {
                throw new System.ArgumentException(value + " is not a valid value");
            }

            string message = digitalWriteInputFunctionNumber + separator + "1" + separator +
                pin + separator + valueProtocol + terminal;
            return message;
        }
        /// <summary>
        /// Generate a message to write a value in a digital pin. Each pin is write with the value in the same index
        /// </summary>
        /// <param name="pins">List with the numbers of the pins to be write</param>
        /// <param name="values">Lists with the values to be write in the pins, it can be "HIGH" or "LOW"</param>
        /// <returns>A string with the "digitalWrite" format specified in the OpenGlove communication protocol</returns>
        public string DigitalWrite(IEnumerable<int> pins, IEnumerable<string> values)
        {
            if (pins.Count() != values.Count())
            {
                throw new System.ArgumentException("Collections length must be equal");
            }

            var digitalWriteMessage = new StringBuilder();
            digitalWriteMessage.Append(digitalWriteInputFunctionNumber + separator + pins.Count());

            for (var i = 0; i < pins.Count(); i++)
            {
                string valueProtocol = "";

                if (values.ElementAt(i) == "LOW")
                {
                    valueProtocol = "0";
                }

                else if (values.ElementAt(i) == "HIGH")
                {
                    valueProtocol = "1";
                }

                else
                {
                    throw new System.ArgumentException(values.ElementAt(i) + " is not a valid value");
                }

                var message = separator + pins.ElementAt(i) + separator + valueProtocol;
                digitalWriteMessage.Append(message);

            }

            digitalWriteMessage.Append(terminal);
            return digitalWriteMessage.ToString();
        }
        /// <summary>
        /// Generate a message to write a PWM value in a digital pin
        /// </summary>
        /// <param name="pin">Number of the pin to be write</param>
        /// <param name="value">Value to be write in the pin, it can be from 0 to 255</param>
        /// <returns>A string with the "analogWrite" format specified in the OpenGlove communication protocol</returns>
        public string AnalogWrite(int pin, int value)
        {
            string message = analogWriteInputFunctionNumber + separator + "1" + separator + pin
                + separator + value + terminal;
            return message;
        }
        /// <summary>
        /// Generate a message to write a PWM value in a digital pin.  Each pin is write with the value in the same index
        /// </summary>
        /// <param name="pins">List with the numbers of the pins to be write</param>
        /// <param name="values">List with the values to be write in the pins, it can be from 0 to 255</param>
        /// <returns>A string with the "analogWrite" format specified in the OpenGlove communication protocol</returns>
        public string AnalogWrite(IEnumerable<int> pins, IEnumerable<int> values)
        {
            if (pins.Count() != values.Count())
            {
                throw new System.ArgumentException("Collections length must be equal");
            }

            var analogWriteMessage = new StringBuilder();
            analogWriteMessage.Append(analogWriteInputFunctionNumber + separator + pins.Count());

            for (var i = 0; i < pins.Count(); i++)
            {

                var message = separator + pins.ElementAt(i) + separator + values.ElementAt(i);
                analogWriteMessage.Append(message);

            }

            analogWriteMessage.Append(terminal);
            return analogWriteMessage.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin">Number of the pin</param>
        /// <param name="mapping">Mapping value, it can be from 0 to 10</param>
        /// <returns>A string with the "addFlexor" format specified in the OpenGlove communication protocol</returns>
        public string addFlexor(int pin, int mapping)
        {
            string message = addFlexorInputFunctionNumber + separator + pin
                + separator + mapping + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping">Mapping value, it can be from 0 to 10</param>
        /// <returns>A string with the "removeFlexor" format specified in the OpenGlove communication protocol</returns>
        public string removeFlexor(int mapping)
        {
            string message = removeFlexorInputFunctionNumber + separator + mapping + terminal;
            return message;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "removeFlexor" format specified in the OpenGlove communication protocol</returns>
        public string calibrateFlexors()
        {
            string message = calibrateFlexorsInputFunctionNumber + terminal;
            return message;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "removeFlexor" format specified in the OpenGlove communication protocol</returns>
        public string confirmCalibration()
        {
            string message = "e";
            return message;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>A string with the "setThreshold" format specified in the OpenGlove communication protocol</returns>
        public string setThreshold(int value)
        {
            string message = setThresholdInputFunctionNumber + separator + value + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "resetFlexors" format specified in the OpenGlove communication protocol</returns>
        public string resetFlexors()
        {
            string message = resetFlexorsInputFunctionNumber + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "startIMU" format specified in the OpenGlove communication protocol</returns>
        public string startIMU()
        {
            string message = startIMUFunctionNumber + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "setIMUStatus" format specified in the OpenGlove communication protocol</returns>
        public string setIMUStatus(int status)
        {
            string message = setIMUStatusFunctionNumber + separator + status + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string with the "set_RawData" format specified in the OpenGlove communication protocol</returns>
        public string setRawData(int status)
        {
            string message = setRawDataFunctionNumber + separator + status + terminal;
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>set loop delay Arduino</returns>
        public string setLoopDelay(int value)
        {
            string message = setLoopDelayNumber + separator + value + terminal;
            return message;
        }

    }
}
