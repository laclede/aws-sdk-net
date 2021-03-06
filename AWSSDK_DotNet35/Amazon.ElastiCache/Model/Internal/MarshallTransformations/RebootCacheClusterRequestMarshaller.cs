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

using Amazon.ElastiCache.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
namespace Amazon.ElastiCache.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// RebootCacheCluster Request Marshaller
    /// </summary>       
    public class RebootCacheClusterRequestMarshaller : IMarshaller<IRequest, RebootCacheClusterRequest>
    {
        public IRequest Marshall(RebootCacheClusterRequest publicRequest)
        {
            IRequest request = new DefaultRequest(publicRequest, "Amazon.ElastiCache");
            request.Parameters.Add("Action", "RebootCacheCluster");
            request.Parameters.Add("Version", "2014-07-15");

            if(publicRequest != null)
            {
                if(publicRequest.IsSetCacheClusterId())
                {
                    request.Parameters.Add("CacheClusterId", StringUtils.FromString(publicRequest.CacheClusterId));
                }
                if(publicRequest.IsSetCacheNodeIdsToReboot())
                {
                    int publicRequestlistValueIndex = 1;
                    foreach(var publicRequestlistValue in publicRequest.CacheNodeIdsToReboot)
                    {
                        request.Parameters.Add("CacheNodeIdsToReboot" + "." + "member" + "." + publicRequestlistValueIndex, StringUtils.FromString(publicRequestlistValue));
                        publicRequestlistValueIndex++;
                    }
                }
            }
            return request;
        }
    }
}