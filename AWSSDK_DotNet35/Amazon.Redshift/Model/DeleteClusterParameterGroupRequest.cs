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

namespace Amazon.Redshift.Model
{
    /// <summary>
    /// Container for the parameters to the DeleteClusterParameterGroup operation.
    /// Deletes a specified Amazon Redshift parameter group.             <note>You
    /// cannot delete a parameter group if it is associated with a cluster.</note>
    /// </summary>
    public partial class DeleteClusterParameterGroupRequest : AmazonRedshiftRequest
    {
        private string _parameterGroupName;


        /// <summary>
        /// Gets and sets the property ParameterGroupName. 
        /// <para>
        ///         The name of the parameter group to be deleted.        
        /// </para>
        ///         
        /// <para>
        /// Constraints:
        /// </para>
        ///         <ul>            <li>Must be the name of an existing cluster parameter group.</li>
        ///            <li>Cannot delete a default cluster parameter group.</li>        </ul>
        /// </summary>
        public string ParameterGroupName
        {
            get { return this._parameterGroupName; }
            set { this._parameterGroupName = value; }
        }

        // Check to see if ParameterGroupName property is set
        internal bool IsSetParameterGroupName()
        {
            return this._parameterGroupName != null;
        }

    }
}