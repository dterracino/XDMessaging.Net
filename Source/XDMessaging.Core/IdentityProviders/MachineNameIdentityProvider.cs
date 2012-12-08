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

namespace XDMessaging.IdentityProviders
{
    public sealed class MachineNameIdentityProvider : IIdentityProvider
    {
        #region Public Methods

        public string GetUniqueId()
        {
            return Environment.MachineName;
        }

        #endregion
    }
}