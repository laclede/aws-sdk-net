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

using Amazon.Redshift.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
namespace Amazon.Redshift.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// CreateClusterParameterGroup Request Marshaller
    /// </summary>       
    public class CreateClusterParameterGroupRequestMarshaller : IMarshaller<IRequest, CreateClusterParameterGroupRequest>
    {
        public IRequest Marshall(CreateClusterParameterGroupRequest publicRequest)
        {
            IRequest request = new DefaultRequest(publicRequest, "Amazon.Redshift");
            request.Parameters.Add("Action", "CreateClusterParameterGroup");
            request.Parameters.Add("Version", "2012-12-01");

            if(publicRequest != null)
            {
                if(publicRequest.IsSetDescription())
                {
                    request.Parameters.Add("Description", StringUtils.FromString(publicRequest.Description));
                }
                if(publicRequest.IsSetParameterGroupFamily())
                {
                    request.Parameters.Add("ParameterGroupFamily", StringUtils.FromString(publicRequest.ParameterGroupFamily));
                }
                if(publicRequest.IsSetParameterGroupName())
                {
                    request.Parameters.Add("ParameterGroupName", StringUtils.FromString(publicRequest.ParameterGroupName));
                }
            }
            return request;
        }
    }
}