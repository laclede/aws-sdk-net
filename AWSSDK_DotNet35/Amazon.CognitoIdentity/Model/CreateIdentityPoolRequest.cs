/*
 * Copyright 2010-2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using System.IO;

using Amazon.Runtime;
using Amazon.Runtime.Internal;

namespace Amazon.CognitoIdentity.Model
{
    /// <summary>
    /// Container for the parameters to the CreateIdentityPool operation.
    /// Creates a new identity pool. The identity pool is a store of user identity information
    /// that is specific to your AWS account.
    /// </summary>
    public partial class CreateIdentityPoolRequest : AmazonCognitoIdentityRequest
    {
        private bool? _allowUnauthenticatedIdentities;
        private string _identityPoolName;
        private Dictionary<string, string> _supportedLoginProviders = new Dictionary<string, string>();


        /// <summary>
        /// Gets and sets the property AllowUnauthenticatedIdentities. 
        /// <para>
        /// TRUE if the identity pool supports unauthenticated logins.
        /// </para>
        /// </summary>
        public bool AllowUnauthenticatedIdentities
        {
            get { return this._allowUnauthenticatedIdentities.GetValueOrDefault(); }
            set { this._allowUnauthenticatedIdentities = value; }
        }

        // Check to see if AllowUnauthenticatedIdentities property is set
        internal bool IsSetAllowUnauthenticatedIdentities()
        {
            return this._allowUnauthenticatedIdentities.HasValue; 
        }


        /// <summary>
        /// Gets and sets the property IdentityPoolName. 
        /// <para>
        /// A string that you provide.
        /// </para>
        /// </summary>
        public string IdentityPoolName
        {
            get { return this._identityPoolName; }
            set { this._identityPoolName = value; }
        }

        // Check to see if IdentityPoolName property is set
        internal bool IsSetIdentityPoolName()
        {
            return this._identityPoolName != null;
        }


        /// <summary>
        /// Gets and sets the property SupportedLoginProviders. 
        /// <para>
        /// Optional key:value pairs mapping provider names to provider app IDs.
        /// </para>
        /// </summary>
        public Dictionary<string, string> SupportedLoginProviders
        {
            get { return this._supportedLoginProviders; }
            set { this._supportedLoginProviders = value; }
        }

        // Check to see if SupportedLoginProviders property is set
        internal bool IsSetSupportedLoginProviders()
        {
            return this._supportedLoginProviders != null && this._supportedLoginProviders.Count > 0; 
        }

    }
}