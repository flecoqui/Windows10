//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;

using Windows.Storage.Streams;

namespace Companion
{
    class CompanionClient
    {
        
        const string cMulticastAddress = "239.11.11.11";
        const string cPort = "1919";
        const string cQUESTION = "??";
        const string cEQUAL = "=";
        const string cMULTICASTCOMMAND = "M-COMMAND";
        const string cUNICASTCOMMAND = "U-COMMAND";

        public const string commandOpen = "OPENMEDIA";
        public const string commandOpenPlaylist = "OPENPLAYLIST";
        public const string commandPlay = "PLAY";
        public const string commandStop = "STOP";
        public const string commandPause = "PAUSE";
        public const string commandPlayPause = "PLAYPAUSE";
        public const string commandSelect = "SELECT";
        public const string commandPlus = "PLUS";
        public const string commandMinus = "MINUS";
        public const string commandFullWindow = "FULLWINDOW";
        public const string commandFullScreen = "FULLSCREEN";
        public const string commandWindow = "WINDOW";
        public const string commandMute = "MUTE";
        public const string commandVolumeUp = "VOLUMEUP";
        public const string commandVolumeDown = "VOLUMEDOWN";
        public const string commandUp = "UP";
        public const string commandDown = "DOWN";
        public const string commandLeft = "LEFT";
        public const string commandRight = "RIGHT";
        public const string commandEnter = "ENTER";


        public const string parameterID = "ID";
        public const string parameterContent = "CONTENT";
        public const string parameterPosterContent = "POSTERCONTENT";
        public const string parameterStart = "START";
        public const string parameterDuration = "DURATION";
        public const string parameterIndex = "INDEX";

        private DatagramSocket msocketSend;
        private DatagramSocket msocketRecv;
        public string mAddress { get; set; }
        public string mPort { get; set; }
        public string mInterfaceAddress { get; set; }

        public int mLastIDReceived;
        public int mLastIDSent;
        public delegate void CompanionEventHandler<TSender, TCommand, TParameter>(TSender sender, TCommand Command, TParameter Parameter);
        public event CompanionEventHandler<CompanionClient, string, Dictionary<string, string>> MessageReceived;


        public CompanionClient()
        {
            msocketSend = null;
            msocketRecv = null;
            mAddress = cMulticastAddress;
            mPort = cPort;
            recvInitialized = false;
            sendInitialized = false;
            mInterfaceAddress = string.Empty;
            mLastIDReceived = 0;
            mLastIDSent = 0;
        }
        public async Task<bool> InitializeRecv()
        {
            recvInitialized = false;
            try
            {
                if (msocketRecv != null)
                {
                    msocketRecv.MessageReceived -= UDPMulticastMessageReceived;
                    msocketRecv.Dispose();
                    msocketRecv = null;
                }
                msocketRecv = new DatagramSocket();
                msocketRecv.MessageReceived += UDPMulticastMessageReceived;
                NetworkAdapter adapter = GetDefaultNetworkAdapter();
                if (adapter != null)
                    await msocketRecv.BindServiceNameAsync(mPort);
  //              await msocketRecv.BindServiceNameAsync(mPort, adapter);
                else
                    await msocketRecv.BindServiceNameAsync(mPort);
                HostName mcast = new HostName(mAddress);
                msocketRecv.JoinMulticastGroup(mcast);
                mLastIDReceived = 0;
                recvInitialized = true;

            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while listening: " + e.Message);
                recvInitialized = false;
            }
            return recvInitialized;
        }
        public  NetworkAdapter GetDefaultNetworkAdapter()
        {
            NetworkAdapter adapter = null;
            if (!string.IsNullOrEmpty(mInterfaceAddress))
                adapter = GetNetworkAdapterByIPAddress(mInterfaceAddress);
            if (adapter == null)
            {
                ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (connectionProfile != null)
                    adapter = connectionProfile.NetworkAdapter;
            }
            return adapter;

        }
        public static NetworkAdapter GetNetworkAdapterByIPAddress(string ipAddress)
        {

            NetworkAdapter adapter = null;
            var hostnames = NetworkInformation.GetHostNames();

            foreach (var hn in hostnames)
            {

                if (hn.IPInformation != null && hn.DisplayName == ipAddress)
                {
                    adapter = hn.IPInformation.NetworkAdapter;
                    break;
                }
            }
            return adapter;
        }

        private bool recvInitialized;
        public bool IsReceiving()
        {
            return recvInitialized;
        }
        private bool sendInitialized;
        public bool IsSending()
        {
            return sendInitialized;
        }
        public void StopRecv()
        {
            try
            {
                if (msocketRecv != null)
                {
                    msocketRecv.MessageReceived -= UDPMulticastMessageReceived;
                    msocketRecv.Dispose();
                    msocketRecv = null;
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while stopping listening: " + e.Message);
            }
        }
        public void StopSend()
        {
            try
            {
                if (msocketSend != null)
                {
                //    msocketSend.MessageReceived -= UDPMulticastMessageReceived;
                    msocketSend.Dispose();
                    msocketSend = null;
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while stopping listening: " + e.Message);
            }
        }
        string GetCommand(string input)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(input))
            {
                if (input.StartsWith(cMULTICASTCOMMAND + cQUESTION))
                {
                    if ((cMULTICASTCOMMAND + cQUESTION).Length < input.Length)
                    {
                        int start = input.IndexOf(cQUESTION, (cMULTICASTCOMMAND + cQUESTION).Length);
                        if (start != -1)
                        {
                            result = input.Substring((cMULTICASTCOMMAND + cQUESTION).Length, start - (cMULTICASTCOMMAND + cQUESTION).Length);
                            result = result.Trim();
                        }
                        else
                        {
                            result = input.Substring((cMULTICASTCOMMAND + cQUESTION).Length);
                            result = result.Trim();

                        }
                    }
                }
            }
            return result;
        }
        public Dictionary<string,string> GetParametersFromCommandLine(string input)
        {
            string result = string.Empty;
            Dictionary<string, string> parameters = null;
            if (!string.IsNullOrEmpty(input))
            {
                if (input.StartsWith(cMULTICASTCOMMAND + cQUESTION))
                {
                    int start = input.IndexOf(cQUESTION, (cMULTICASTCOMMAND + cQUESTION).Length);
                    if (start != -1)
                    {
                        result = input.Substring(start + (cQUESTION).Length);
                        result = result.Trim();
                        string[] separator = new string[1];
                        separator[0] = cQUESTION;
                        string[] array = result.Split(separator,StringSplitOptions.None);
                        foreach(var s in array)
                        {
                            int index = s.IndexOf(cEQUAL);
                            if(index >0)
                            {
                                string key = s.Substring(0, index);
                                string value = s.Substring(index + 1);
                                if (parameters == null)
                                    parameters = new Dictionary<string, string>();
                                parameters.Add(key, value);
                            }
                        }
                        return parameters;
                    }
                }
            }
            return null;
        }
        public Dictionary<string, string> GetParametersFromString(string input)
        {
            Dictionary<string, string> parameters = null;
            if (!string.IsNullOrEmpty(input))
            {
                string[] separator = new string[1];
                separator[0] = cQUESTION;
                string[] array = input.Split(separator, StringSplitOptions.None);
                foreach (var s in array)
                {
                    int index = s.IndexOf(cEQUAL);
                    if (index > 0)
                    {
                        string key = s.Substring(0, index);
                        string value = s.Substring(index + 1);
                        if (parameters == null)
                            parameters = new Dictionary<string, string>();
                        parameters.Add(key, value);
                    }
                }
                return parameters;
            }
            return null;
        }
        private void ParseMessage(string Message)
        { 
            if(!string.IsNullOrEmpty(Message))
            {
                System.Diagnostics.Debug.WriteLine("Message received: " + Message);
                string Command = GetCommand(Message);
                if (!string.IsNullOrEmpty(Command))
                {
                    Dictionary<string,string> Parameter = GetParametersFromCommandLine(Message);
                    if ((Parameter != null) && (Parameter.ContainsKey(parameterID)))
                    {
                        string value;
                        Parameter.TryGetValue(parameterID, out value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            int receivedID = 0;
                            if (int.TryParse(value, out receivedID) == true)
                            {
                                if (receivedID != mLastIDReceived)
                                {
                                    mLastIDReceived = receivedID;
                                    Parameter.Remove(parameterID);
                                    if (MessageReceived != null)
                                        MessageReceived(this, Command, Parameter);
                                }
                            }
                        }
                    }
                }
            }
        }

        void UDPMulticastMessageReceived(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
            try
            {

                uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                string message = eventArguments.GetDataReader().ReadString(stringLength);
                ParseMessage(message);
            }
            catch (Exception exception)
            {
                SocketErrorStatus socketError = SocketError.GetStatus(exception.HResult);
                if (socketError == SocketErrorStatus.ConnectionResetByPeer)
                {
                }
                else if (socketError != SocketErrorStatus.Unknown)
                {
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task<bool> InitializeSend()
        {
            sendInitialized = false;
            try
            {
                if (msocketSend != null)
                {
                    //    msocketSend.MessageReceived -= UDPMulticastMessageReceived;
                    msocketSend.Dispose();
                    msocketSend = null;
                }
                msocketSend = new DatagramSocket();
              //  msocketSend.MessageReceived += UDPMulticastMessageReceived;

                NetworkAdapter adapter = GetDefaultNetworkAdapter();
                if (adapter != null)
                    await msocketSend.BindServiceNameAsync("", adapter);
                mLastIDSent = 0;
                sendInitialized = true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while initializing: " + e.Message);
                sendInitialized = false;
            }
            return sendInitialized;

        }
        public async Task<bool> Send(string buffer)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Message Sent: " + buffer);
                if (msocketSend == null)
                    await InitializeSend();

                HostName mcast = new HostName(mAddress);
                DataWriter writer = new DataWriter(await msocketSend.GetOutputStreamAsync(mcast, mPort));

                if (writer != null)
                {
                    writer.WriteString(buffer);
                    uint result = await writer.StoreAsync();
                    bool bresult = await writer.FlushAsync();
                    writer.DetachStream();
                    writer.Dispose();
                    return true;
                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    return false;
                }
                return false;
            }
            return false;

        }
        public async Task<bool> SendCommand(string command, Dictionary<string, string> parameter)
        {
            bool bResult = false;
            mLastIDSent = (mLastIDSent > 9999 ? 0 : mLastIDSent + 1);

            string c = cMULTICASTCOMMAND + cQUESTION + command + cQUESTION + parameterID + cEQUAL + (mLastIDSent).ToString();
            if ((parameter!=null) &&
                (parameter.Count > 0))
            {
                foreach  (var value in parameter)
                {
                    c += cQUESTION + value.Key + cEQUAL + value.Value;
                }
            }
            bResult = await Send(c);
            return bResult;
        }
    }
}
