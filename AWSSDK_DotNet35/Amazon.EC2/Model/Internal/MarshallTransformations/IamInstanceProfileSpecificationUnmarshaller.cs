/*
 * Copyright 2010-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
using System.Collections.Generic;

using Amazon.EC2.Model;
using Amazon.Runtime.Internal.Transform;

namespace Amazon.EC2.Model.Internal.MarshallTransformations
{
     /// <summary>
     ///   IamInstanceProfileSpecification Unmarshaller
     /// </summary>
    internal class IamInstanceProfileSpecificationUnmarshaller : IUnmarshaller<IamInstanceProfileSpecification, XmlUnmarshallerContext>, IUnmarshaller<IamInstanceProfileSpecification, JsonUnmarshallerContext> 
    {
        public IamInstanceProfileSpecification Unmarshall(XmlUnmarshallerContext context) 
        {
            IamInstanceProfileSpecification iamInstanceProfileSpecification = new IamInstanceProfileSpecification();
            int originalDepth = context.CurrentDepth;
            int targetDepth = originalDepth + 1;
            
            if (context.IsStartOfDocument) 
               targetDepth += 1;
            
            while (context.Read())
            {
                if (context.IsStartElement || context.IsAttribute)
                {
                    if (context.TestExpression("arn", targetDepth))
                    {
                        iamInstanceProfileSpecification.Arn = StringUnmarshaller.GetInstance().Unmarshall(context);
                            
                        continue;
                    }
                    if (context.TestExpression("name", targetDepth))
                    {
                        iamInstanceProfileSpecification.Name = StringUnmarshaller.GetInstance().Unmarshall(context);
                            
                        continue;
                    }
                }
                else if (context.IsEndElement && context.CurrentDepth < originalDepth)
                {
                    return iamInstanceProfileSpecification;
                }
            }
                        


            return iamInstanceProfileSpecification;
        }

        public IamInstanceProfileSpecification Unmarshall(JsonUnmarshallerContext context) 
        {
            return null;
        }

        private static IamInstanceProfileSpecificationUnmarshaller instance;

        public static IamInstanceProfileSpecificationUnmarshaller GetInstance() 
        {
            if (instance == null) 
               instance = new IamInstanceProfileSpecificationUnmarshaller();

            return instance;
        }
    }
}
    