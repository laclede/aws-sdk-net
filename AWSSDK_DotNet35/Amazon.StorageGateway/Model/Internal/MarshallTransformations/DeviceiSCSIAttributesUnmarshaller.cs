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
    using System.IO;
    using ThirdParty.Json.LitJson;
    using Amazon.StorageGateway.Model;
    using Amazon.Runtime.Internal.Transform;

    namespace Amazon.StorageGateway.Model.Internal.MarshallTransformations
    {
      /// <summary>
      /// DeviceiSCSIAttributesUnmarshaller
      /// </summary>
      internal class DeviceiSCSIAttributesUnmarshaller : IUnmarshaller<DeviceiSCSIAttributes, XmlUnmarshallerContext>, IUnmarshaller<DeviceiSCSIAttributes, JsonUnmarshallerContext>
      {
        DeviceiSCSIAttributes IUnmarshaller<DeviceiSCSIAttributes, XmlUnmarshallerContext>.Unmarshall(XmlUnmarshallerContext context)
        {
          throw new NotImplementedException();
        }

        public DeviceiSCSIAttributes Unmarshall(JsonUnmarshallerContext context)
        {
            context.Read();
            if (context.CurrentTokenType == JsonToken.Null) return null;
            DeviceiSCSIAttributes deviceiSCSIAttributes = new DeviceiSCSIAttributes();
        
        
            int targetDepth = context.CurrentDepth;
            while (context.ReadAtDepth(targetDepth))
            {
              
              if (context.TestExpression("TargetARN", targetDepth))
              {
                deviceiSCSIAttributes.TargetARN = StringUnmarshaller.GetInstance().Unmarshall(context);
                continue;
              }
  
              if (context.TestExpression("NetworkInterfaceId", targetDepth))
              {
                deviceiSCSIAttributes.NetworkInterfaceId = StringUnmarshaller.GetInstance().Unmarshall(context);
                continue;
              }
  
              if (context.TestExpression("NetworkInterfacePort", targetDepth))
              {
                deviceiSCSIAttributes.NetworkInterfacePort = IntUnmarshaller.GetInstance().Unmarshall(context);
                continue;
              }
  
              if (context.TestExpression("ChapEnabled", targetDepth))
              {
                deviceiSCSIAttributes.ChapEnabled = BoolUnmarshaller.GetInstance().Unmarshall(context);
                continue;
              }
  
            }
          
            return deviceiSCSIAttributes;
        }

        private static DeviceiSCSIAttributesUnmarshaller instance;
        public static DeviceiSCSIAttributesUnmarshaller GetInstance()
        {
            if (instance == null)
                instance = new DeviceiSCSIAttributesUnmarshaller();
            return instance;
        }
    }
}
  
