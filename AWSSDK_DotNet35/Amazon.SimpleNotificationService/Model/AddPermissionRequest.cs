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

namespace Amazon.SimpleNotificationService.Model
{
    /// <summary>
    /// Container for the parameters to the AddPermission operation.
    /// Adds a statement to a topic's access control policy, granting access for the specified
    /// AWS accounts to the specified actions.
    /// </summary>
    public partial class AddPermissionRequest : AmazonSimpleNotificationServiceRequest
    {
        private List<string> _actionName = new List<string>();
        private List<string> _aWSAccountId = new List<string>();
        private string _label;
        private string _topicArn;


        /// <summary>
        /// Gets and sets the property ActionName. 
        /// <para>
        /// The action you want to allow for the specified principal(s).
        /// </para>
        ///     
        /// <para>
        /// Valid values: any Amazon SNS action name.
        /// </para>
        /// </summary>
        public List<string> ActionName
        {
            get { return this._actionName; }
            set { this._actionName = value; }
        }

        // Check to see if ActionName property is set
        internal bool IsSetActionName()
        {
            return this._actionName != null && this._actionName.Count > 0; 
        }


        /// <summary>
        /// Gets and sets the property AWSAccountId. 
        /// <para>
        /// The AWS account IDs of the users (principals) who will be given access to the specified
        ///    actions. The users must have AWS accounts, but do not need to be signed up    
        /// for this service. 
        /// </para>
        /// </summary>
        public List<string> AWSAccountId
        {
            get { return this._aWSAccountId; }
            set { this._aWSAccountId = value; }
        }

        // Check to see if AWSAccountId property is set
        internal bool IsSetAWSAccountId()
        {
            return this._aWSAccountId != null && this._aWSAccountId.Count > 0; 
        }


        /// <summary>
        /// Gets and sets the property Label. 
        /// <para>
        /// A unique identifier for the new policy statement.
        /// </para>
        /// </summary>
        public string Label
        {
            get { return this._label; }
            set { this._label = value; }
        }

        // Check to see if Label property is set
        internal bool IsSetLabel()
        {
            return this._label != null;
        }


        /// <summary>
        /// Gets and sets the property TopicArn. 
        /// <para>
        /// The ARN of the topic whose access control policy you wish to modify.
        /// </para>
        /// </summary>
        public string TopicArn
        {
            get { return this._topicArn; }
            set { this._topicArn = value; }
        }

        // Check to see if TopicArn property is set
        internal bool IsSetTopicArn()
        {
            return this._topicArn != null;
        }

    }
}