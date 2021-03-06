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

namespace Amazon.EC2.Model
{
    /// <summary>
    /// <para>Describes VPN connection options.</para>
    /// </summary>
    public partial class VpnConnectionOptions
    {
        
        private bool? staticRoutesOnly;


        /// <summary>
        /// Indicates whether the VPN connection uses static routes only. Static routes must be used for devices that don't support BGP.
        ///  
        /// </summary>
        public bool StaticRoutesOnly
        {
            get { return this.staticRoutesOnly ?? default(bool); }
            set { this.staticRoutesOnly = value; }
        }

        // Check to see if StaticRoutesOnly property is set
        internal bool IsSetStaticRoutesOnly()
        {
            return this.staticRoutesOnly.HasValue;
        }
    }
}
