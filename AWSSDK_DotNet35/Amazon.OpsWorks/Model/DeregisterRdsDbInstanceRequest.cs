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

namespace Amazon.OpsWorks.Model
{
    /// <summary>
    /// Container for the parameters to the DeregisterRdsDbInstance operation.
    /// Deregisters an Amazon RDS instance.
    /// </summary>
    public partial class DeregisterRdsDbInstanceRequest : AmazonOpsWorksRequest
    {
        private string _rdsDbInstanceArn;


        /// <summary>
        /// Gets and sets the property RdsDbInstanceArn. 
        /// <para>
        /// The Amazon RDS instance's ARN.
        /// </para>
        /// </summary>
        public string RdsDbInstanceArn
        {
            get { return this._rdsDbInstanceArn; }
            set { this._rdsDbInstanceArn = value; }
        }

        // Check to see if RdsDbInstanceArn property is set
        internal bool IsSetRdsDbInstanceArn()
        {
            return this._rdsDbInstanceArn != null;
        }

    }
}