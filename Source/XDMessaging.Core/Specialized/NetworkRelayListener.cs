﻿/*=============================================================================
*
*	(C) Copyright 2011, Michael Carlisle (mike.carlisle@thecodeking.co.uk)
*
*   http://www.TheCodeKing.co.uk
*  
*	All rights reserved.
*	The code and information is provided "as-is" without waranty of any kind,
*	either expressed or implied.
*
*=============================================================================
*/
using System;
using TheCodeKing.Utils.Contract;
using XDMessaging.Entities;
using XDMessaging.Messages;

namespace XDMessaging.Specialized
{
    /// <summary>
    ///   The implementation used to listen for and relay network messages for all
    ///   instances of IXDListener.
    /// </summary>
    internal sealed class NetworkRelayListener : IXDListener
    {
        #region Constants and Fields

        /// <summary>
        ///   The factory instance used to create broadcast instances in order to re-send network messages natively.
        /// </summary>
        private readonly IXDBroadcaster nativeBroadcast;

        private readonly IXDListener nativeListener;

        /// <summary>
        ///   The instance of MailSlot used to receive network messages from other machines.
        /// </summary>
        private readonly IXDListener propagateListener;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Default constructor.
        /// </summary>
        /// <param name = "nativeBroadcast"></param>
        /// <param name = "nativeListener"></param>
        /// <param name = "propagateListener"></param>
        /// <param name = "mode"></param>
        internal NetworkRelayListener(IXDBroadcaster nativeBroadcast,
                                      IXDListener nativeListener,
                                      IXDListener propagateListener, XDTransportMode mode)
        {
            Validate.That(nativeBroadcast).IsNotNull();
            Validate.That(nativeListener).IsNotNull();
            Validate.That(propagateListener).IsNotNull();

            this.nativeBroadcast = nativeBroadcast;
            this.propagateListener = propagateListener;
            this.nativeListener = nativeListener;
            // listen on the network channel for this mode
            this.propagateListener.RegisterChannel(string.Concat(mode, NetworkRelayBroadcaster.RelayChannel));
            this.propagateListener.MessageReceived += OnNetworkMessageReceived;
            this.nativeListener.MessageReceived += OnMessageReceived;
        }

        #endregion

        #region Events

        public event Listeners.XDMessageHandler MessageReceived;

        #endregion

        #region Implemented Interfaces

        #region IDisposable

        /// <summary>
        ///   Implementation of IDisposable used to clean up the listener instance.
        /// </summary>
        public void Dispose()
        {
            if (nativeListener != null)
            {
                nativeListener.Dispose();
            }
        }

        #endregion

        #region IXDListener

        public void RegisterChannel(string channelName)
        {
            nativeListener.RegisterChannel(channelName);
        }

        public void UnRegisterChannel(string channelName)
        {
            nativeListener.UnRegisterChannel(channelName);
        }

        #endregion

        #endregion

        #region Methods

        private void OnMessageReceived(object sender, XDMessageEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        /// <summary>
        ///   Handles messages received from other machines on the network and dispatches them locally.
        /// </summary>
        /// <param name = "sender"></param>
        /// <param name = "e"></param>
        private void OnNetworkMessageReceived(object sender, XDMessageEventArgs e)
        {
            if (e.DataGram.IsValid)
            {
                TypedDataGram<NetworkRelayMessage> dataGram = e.DataGram;
                // don't relay if the message was broadcast on this machine
                if (dataGram != null && dataGram.IsValid && dataGram.Message.MachineName != Environment.MachineName)
                {
                    // rebroadcast locally
                    nativeBroadcast.SendToChannel(dataGram.Message.Channel, dataGram.Message.Message);
                }
            }
        }

        #endregion
    }
}