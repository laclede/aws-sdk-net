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

using Amazon.EC2.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;

namespace Amazon.EC2.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// Assign Private Ip Addresses Request Marshaller
    /// </summary>       
    public class AssignPrivateIpAddressesRequestMarshaller : IMarshaller<IRequest, AssignPrivateIpAddressesRequest>
    {
        public IRequest Marshall(AssignPrivateIpAddressesRequest assignPrivateIpAddressesRequest)
        {
            IRequest request = new DefaultRequest(assignPrivateIpAddressesRequest, "AmazonEC2");
            request.Parameters.Add("Action", "AssignPrivateIpAddresses");
            request.Parameters.Add("Version", "2014-06-15");
            if (assignPrivateIpAddressesRequest != null && assignPrivateIpAddressesRequest.IsSetNetworkInterfaceId())
            {
                request.Parameters.Add("NetworkInterfaceId", StringUtils.FromString(assignPrivateIpAddressesRequest.NetworkInterfaceId));
            }
            if (assignPrivateIpAddressesRequest != null)
            {
                List<string> privateIpAddressesList = assignPrivateIpAddressesRequest.PrivateIpAddresses;

                int privateIpAddressesListIndex = 1;
                foreach (string privateIpAddressesListValue in privateIpAddressesList)
                { 
                    request.Parameters.Add("PrivateIpAddress." + privateIpAddressesListIndex, StringUtils.FromString(privateIpAddressesListValue));
                    privateIpAddressesListIndex++;
                }
            }
            if (assignPrivateIpAddressesRequest != null && assignPrivateIpAddressesRequest.IsSetSecondaryPrivateIpAddressCount())
            {
                request.Parameters.Add("SecondaryPrivateIpAddressCount", StringUtils.FromInt(assignPrivateIpAddressesRequest.SecondaryPrivateIpAddressCount));
            }
            if (assignPrivateIpAddressesRequest != null && assignPrivateIpAddressesRequest.IsSetAllowReassignment())
            {
                request.Parameters.Add("AllowReassignment", StringUtils.FromBool(assignPrivateIpAddressesRequest.AllowReassignment));
            }

            return request;
        }
    }
}
