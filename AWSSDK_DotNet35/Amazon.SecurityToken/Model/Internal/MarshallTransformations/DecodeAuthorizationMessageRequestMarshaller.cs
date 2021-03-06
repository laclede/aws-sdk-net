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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using Amazon.SecurityToken.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
namespace Amazon.SecurityToken.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// DecodeAuthorizationMessage Request Marshaller
    /// </summary>       
    public class DecodeAuthorizationMessageRequestMarshaller : IMarshaller<IRequest, DecodeAuthorizationMessageRequest>
    {
        public IRequest Marshall(DecodeAuthorizationMessageRequest publicRequest)
        {
            IRequest request = new DefaultRequest(publicRequest, "Amazon.SecurityToken");
            request.Parameters.Add("Action", "DecodeAuthorizationMessage");
            request.Parameters.Add("Version", "2011-06-15");

            if(publicRequest != null)
            {
                if(publicRequest.IsSetEncodedMessage())
                {
                    request.Parameters.Add("EncodedMessage", StringUtils.FromString(publicRequest.EncodedMessage));
                }
            }
            return request;
        }
    }
}