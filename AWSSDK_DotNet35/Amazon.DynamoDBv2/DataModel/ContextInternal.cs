﻿/*
 * Copyright 2012-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

using Amazon.Util;
using System.Globalization;

namespace Amazon.DynamoDBv2.DataModel
{
    public partial class DynamoDBContext
    {
        #region Versioning

        private static void SetNewVersion(ItemStorage storage)
        {
            if (!storage.Config.HasVersion) return;

            DynamoDBEntry versionEntry;
            Primitive version;
            string versionAttributeName = storage.Config.VersionPropertyStorage.AttributeName;

            if (storage.Document.TryGetValue(versionAttributeName, out versionEntry))
                version = versionEntry as Primitive;
            else
                version = null;

            if (version != null && version.Value != null)
            {
                if (version.Type != DynamoDBEntryType.Numeric) throw new InvalidOperationException("Version property must be numeric");
                PropertyStorage propertyStorage = storage.Config.VersionPropertyStorage;
                IncrementVersion(propertyStorage.MemberType, ref version);
            }
            else
            {
                version = new Primitive("0", true);
            }
            storage.Document[versionAttributeName] = version;
        }
        private static void IncrementVersion(Type memberType, ref Primitive version)
        {
            var memberTypeWrapper = TypeFactory.GetTypeInfo(memberType);
            if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Byte)))) version = version.AsByte() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(SByte)))) version = version.AsSByte() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(int)))) version = version.AsInt() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(uint)))) version = version.AsUInt() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(long)))) version = version.AsLong() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ulong)))) version = version.AsULong() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(short)))) version = version.AsShort() + 1;
            else if (memberTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ushort)))) version = version.AsUShort() + 1;
        }
        private static Document CreateExpectedDocumentForVersion(ItemStorage storage)
        {
            Document document = new Document();
            if (storage.Config.HasVersion)
            {
                string versionAttributeName = storage.Config.VersionPropertyStorage.AttributeName;
                if (storage.CurrentVersion == null)
                {
                    document[versionAttributeName] = null;
                }
                else
                {
                    document[versionAttributeName] = storage.CurrentVersion;
                }
            }
            return document;
        }

        #endregion

        #region Table methods

        internal Table GetTargetTableInternal<T>(DynamoDBOperationConfig operationConfig)
        {
            Type type = typeof(T);
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, this.Config);
            ItemStorageConfig storageConfig = StorageConfigCache.GetConfig(type, flatConfig);
            Table table = GetTargetTable(storageConfig, flatConfig);
            Table copy = table.Copy(Table.DynamoDBConsumer.DocumentModel);
            return table;
        }

        internal Table GetTargetTable(ItemStorageConfig storageConfig, DynamoDBFlatConfig flatConfig)
        {
            if (flatConfig == null)
                throw new ArgumentNullException("flatConfig");

            string tableName = GetTableName(storageConfig.TableName, flatConfig);
            Table table = GetTable(tableName);

            ValidateConfigAgainstTable(storageConfig, table);
            return table;
        }

        internal Table GetTable(string tableName)
        {
            Table table;
            lock (tablesMapLock)
            {
                if (!tablesMap.TryGetValue(tableName, out table))
                {
                    table = Table.LoadTable(Client, tableName, Table.DynamoDBConsumer.DataModel);
                    tablesMap[tableName] = table;
                }
            }
            return table;
        }

        internal static string GetTableName(string baseTableName, DynamoDBFlatConfig flatConfig)
        {
            if (flatConfig == null)
                throw new ArgumentNullException("flatConfig");

            string tableName = baseTableName;

            if (!string.IsNullOrEmpty(flatConfig.OverrideTableName))
                tableName = flatConfig.OverrideTableName;
            if (!string.IsNullOrEmpty(flatConfig.TableNamePrefix))
                tableName = flatConfig.TableNamePrefix + tableName;

            return tableName;
        }

        private static void ValidateConfigAgainstTable(ItemStorageConfig config, Table table)
        {
            CompareKeys(config, table, table.HashKeys, config.HashKeyPropertyNames, "hash");
            CompareKeys(config, table, table.RangeKeys, config.RangeKeyPropertyNames, "range");
        }

        private static void CompareKeys(ItemStorageConfig config, Table table, List<string> attributes, List<string> properties, string keyType)
        {
            if (attributes.Count != properties.Count)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, 
                    "Number of {0} keys on table {1} does not match number of hash keys on type {2}",
                    keyType, table.TableName, config.TargetTypeInfo.FullName));
            foreach (string hashProperty in properties)
            {
                PropertyStorage property = config.GetPropertyStorage(hashProperty);
                if (!attributes.Contains(property.AttributeName))
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, 
                        "Key property {0} on type {1} does not correspond to a {2} key on table {3}",
                        hashProperty, config.TargetTypeInfo.FullName, keyType, table.TableName));
            }
        }

        #endregion

        #region Marshalling/unmarshalling

        // Check if DynamoDBEntry is supported
        private static bool ShouldSave(DynamoDBEntry entry, bool ignoreNullValues)
        {
            if (entry == null)
            {
                if (ignoreNullValues)
                    return false;
                else
                    return true;
            }

            var primitive = entry as Primitive;
            if (primitive != null)
                return (primitive.Value != null);

            var primitiveList = entry as PrimitiveList;
            if (primitiveList != null)
                return (primitiveList.Entries != null && primitiveList.Entries.Count > 0);

            throw new InvalidOperationException("Unrecognized DynamoDBEntry object");
        }

        // Deserializing DynamoDB document into an object
        private T DocumentToObject<T>(ItemStorage storage)
        {
            Type type = typeof(T);
            return (T)DocumentToObject(type, storage);
        }
        private object DocumentToObject(Type objectType, ItemStorage storage)
        {
            if (storage == null) throw new ArgumentNullException("storage");

            if (storage.Document == null) return null;

            object instance = Utils.InstantiateConverter(objectType, this);
            PopulateInstance(storage, instance);
            return instance;
        }
        private static void PopulateInstance(ItemStorage storage, object instance)
        {
            ItemStorageConfig config = storage.Config;
            Document document = storage.Document;

            foreach (PropertyStorage propertyStorage in config.AllPropertyStorage)
            {
                string propertyName = propertyStorage.PropertyName;
                string attributeName = propertyStorage.AttributeName;

                DynamoDBEntry entry;
                if (document.TryGetValue(attributeName, out entry))
                {
                    if (ShouldSave(entry, true))
                    {
                        object value = FromDynamoDBEntry(propertyStorage, entry); // TODO: send entire propertyStorage into method, and store the collectionAdd MethodInfo in it

                        if (!TrySetValue(instance, propertyStorage.Member, value))
                        {
                            throw new InvalidOperationException("Unable to retrieve value from " + attributeName);
                        }
                    }

                    if (propertyStorage.IsVersion)
                        storage.CurrentVersion = entry as Primitive;
                }
            }
        }

        // Serializing an object into a DynamoDB document
        private ItemStorage ObjectToItemStorage<T>(T toStore, bool keysOnly, DynamoDBFlatConfig flatConfig)
        {
            if (toStore == null) return null;

            Type objectType = typeof(T);

            ItemStorageConfig config = StorageConfigCache.GetConfig(objectType, flatConfig);
            if (config == null) return null;

            ItemStorage storage = ObjectToItemStorage<T>(toStore, keysOnly, flatConfig.IgnoreNullValues.Value, config);
            return storage;
        }
        internal static ItemStorage ObjectToItemStorage<T>(T toStore, bool keysOnly, bool ignoreNullValues, ItemStorageConfig config)
        {
            ItemStorage storage = new ItemStorage(config);
            PopulateItemStorage(toStore, keysOnly, ignoreNullValues, storage);
            return storage;
        }
        private static void PopulateItemStorage(object toStore, bool keysOnly, bool ignoreNullValues, ItemStorage storage)
        {
            ItemStorageConfig config = storage.Config;
            Document document = storage.Document;

            foreach (PropertyStorage propertyStorage in config.AllPropertyStorage)
            {
                // if only keys are being serialized, skip non-key properties
                // still include version, however, to populate the storage.CurrentVersion field
                if (keysOnly && !propertyStorage.IsHashKey && !propertyStorage.IsRangeKey && !propertyStorage.IsVersion) continue;

                string propertyName = propertyStorage.PropertyName;
                string attributeName = propertyStorage.AttributeName;

                object value;
                if (TryGetValue(toStore, propertyStorage.Member, out value))
                {
                    DynamoDBEntry dbe = ToDynamoDBEntry(propertyStorage, value);

                    if (ShouldSave(dbe, ignoreNullValues))
                    {
                        if (propertyStorage.IsHashKey || propertyStorage.IsRangeKey || propertyStorage.IsVersion || propertyStorage.IsLSIRangeKey)
                        {
                            if (dbe is PrimitiveList)
                                throw new InvalidOperationException("Property " + propertyName + " is a hash key, range key or version property and cannot be PrimitiveList");
                        }
                        document[attributeName] = dbe;

                        if (propertyStorage.IsVersion)
                            storage.CurrentVersion = dbe as Primitive;
                    }
                }
                else
                    throw new InvalidOperationException("Unable to retrieve value from property " + propertyName);
            }
        }

        // DynamoDBEntry <--> Object
        private static object FromDynamoDBEntry(PropertyStorage propertyStorage, DynamoDBEntry value)
        {
            var converter = propertyStorage.Converter;
            if (converter != null)
                return converter.FromEntry(value);

            object output;
            var targetType = propertyStorage.MemberType;
            
            Primitive primitive = value as Primitive;
            if (primitive != null && TryFromPrimitive(targetType, primitive, out output))
                return output;

            PrimitiveList primitiveList = value as PrimitiveList;
            if (primitiveList != null && TryFromPrimitiveList(targetType, primitiveList, out output))
                return output;

            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Unable to convert attribute \"{0}\" to property \"{1}\" of type \"{2}\"",
                propertyStorage.AttributeName, propertyStorage.PropertyName, propertyStorage.MemberType.FullName));
        }
        private static DynamoDBEntry ToDynamoDBEntry(PropertyStorage propertyStorage, object value)
        {
            return ToDynamoDBEntry(propertyStorage, value, false);
        }
        private static DynamoDBEntry ToDynamoDBEntry(PropertyStorage propertyStorage, object value, bool canReturnPrimitiveInsteadOfList)
        {
            if (value == null)
                return null;

            var converter = propertyStorage.Converter;
            if (converter != null)
                return converter.ToEntry(value);

            var type = propertyStorage.MemberType;
            Primitive primitive;
            if (TryToPrimitive(type, value, out primitive))
                return primitive;

            DynamoDBEntry primitiveList;
            if (TryToPrimitiveList(type, value, canReturnPrimitiveInsteadOfList, out primitiveList))
                return primitiveList;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to convert field \"{0}\" of type \"{1}\", type is not primitive or primitive collection and does not define a converter",
                propertyStorage.PropertyName, value.GetType().FullName));

        }

        // PrimitiveList <--> List
        private static bool TryFromPrimitiveList(Type targetType, PrimitiveList value, out object output)
        {
            var targetTypeWrapper = TypeFactory.GetTypeInfo(targetType);
            Type elementType;
            if ((!Utils.ImplementsInterface(targetType, typeof(ICollection<>)) &&
                !Utils.ImplementsInterface(targetType, typeof(IList))) ||
                !Utils.CanInstantiate(targetType) ||
                !Utils.IsPrimitive(elementType = targetTypeWrapper.GetGenericArguments()[0]))
            {
                output = null;
                return false;
            }
            
            var collection = Utils.Instantiate(targetType);
            IList ilist = collection as IList;
            bool useIListInterface = ilist != null;

            MethodInfo collectionAdd = null;
            if (!useIListInterface)
            {
                collectionAdd = targetTypeWrapper.GetMethod("Add");
            }
            
            foreach (Primitive primitive in value.Entries)
            {
                object primitiveValue;
                if (TryFromPrimitive(elementType, primitive, out primitiveValue))
                {
                    if (useIListInterface)
                        ilist.Add(primitiveValue);
                    else
                        collectionAdd.Invoke(collection, new object[] { primitiveValue });
                }
                else
                {
                    output = null;
                    return false;
                }
            }

            output = collection;
            return true;
        }
        private static bool TryToPrimitiveList(Type type, object value, bool canReturnPrimitive, out DynamoDBEntry output)
        {
            var typeWrapper = TypeFactory.GetTypeInfo(type);
            Type elementType;
            if (!Utils.ImplementsInterface(type, typeof(ICollection<>)) ||
                !Utils.IsPrimitive(elementType = typeWrapper.GetGenericArguments()[0]))
            {
                output = null;
                return false;
            }

            IEnumerable enumerable = value as IEnumerable;

            Primitive primitive;
            // Strings are collections of chars, don't treat them as collections
            if (enumerable == null || value is string)
            {
                if (canReturnPrimitive &&
                    TypeFactory.GetTypeInfo(value.GetType()).IsAssignableFrom(TypeFactory.GetTypeInfo(elementType)) &&
                    TryToPrimitive(elementType, value, out primitive))
                {
                    output = primitive;
                    return true;
                }

                output = null;
                return false;
            }

            PrimitiveList primitiveList = new PrimitiveList();
            DynamoDBEntryType? listType = null;
            foreach (var item in enumerable)
            {
                if (TryToPrimitive(elementType, item, out primitive))
                {
                    if (listType.HasValue && listType.Value != primitive.Type)
                        throw new InvalidOperationException("List cannot contain a mix of different types");
                    listType = primitive.Type;

                    primitiveList.Entries.Add(primitive);
                }
                else
                {
                    output = null;
                    return false;
                }
            }
            primitiveList.Type = listType.GetValueOrDefault(DynamoDBEntryType.String);

            output = primitiveList;
            return true;
        }

        // Primitive <--> Value type
        private static bool TryFromPrimitive(Type targetType, Primitive value, out object output)
        {
            if (value.Value == null)
            {
                output = null;
                return true;
            }

            try
            {
                var targetTypeWrapper = TypeFactory.GetTypeInfo(targetType);
                if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Boolean)))) output = value.AsBoolean();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Byte)))) output = value.AsByte();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Char)))) output = value.AsChar();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(DateTime)))) output = value.AsDateTime();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Decimal)))) output = value.AsDecimal();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Double)))) output = value.AsDouble();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(int)))) output = value.AsInt();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(long)))) output = value.AsLong();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(SByte)))) output = value.AsSByte();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(short)))) output = value.AsShort();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Single)))) output = value.AsSingle();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(String)))) output = value.AsString();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(uint)))) output = value.AsUInt();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ulong)))) output = value.AsULong();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ushort)))) output = value.AsUShort();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Guid)))) output = value.AsGuid();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(byte[])))) output = value.AsByteArray();
                else if (targetTypeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(MemoryStream)))) output = value.AsMemoryStream();
                else
                {
                    output = null;
                    return false;
                }
                return true;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to cast value [{0}] of type [{1}] to type [{2}]",
                    value.Value,
                    value.Value.GetType().FullName,
                    targetType.FullName));
            }
        }
        private static bool TryToPrimitive(Type type, object value, out Primitive output)
        {
            try
            {
                var typeWrapper = TypeFactory.GetTypeInfo(type);
                if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Boolean)))) output = (Boolean)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Byte)))) output = (Byte)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Char)))) output = (Char)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(DateTime)))) output = (DateTime)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Decimal)))) output = (Decimal)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Double)))) output = (Double)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(int)))) output = (int)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(long)))) output = (long)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(SByte)))) output = (SByte)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(short)))) output = (short)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Single)))) output = (Single)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(String)))) output = (String)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(uint)))) output = (uint)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ulong)))) output = (ulong)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(ushort)))) output = (ushort)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(Guid)))) output = (Guid)value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(byte[])))) output = (byte[])value;
                else if (typeWrapper.IsAssignableFrom(TypeFactory.GetTypeInfo(typeof(MemoryStream)))) output = (MemoryStream)value;
                else
                {
                    output = null;
                    return false;
                }
                return true;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to cast value [{0}] of type [{1}] to type [{2}]",
                    value,
                    value == null ? "null" : value.GetType().FullName,
                    type.FullName));
            }
        }

        // Get/Set object properties
        private static bool TrySetValue(object instance, MemberInfo member, object value)
        {
            FieldInfo fieldInfo = member as FieldInfo;
            PropertyInfo propertyInfo = member as PropertyInfo;

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(instance, value);
                return true;
            }
            else if (propertyInfo != null)
            {
                propertyInfo.SetValue(instance, value, null);
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool TryGetValue(object instance, MemberInfo member, out object value)
        {
            FieldInfo fieldInfo = member as FieldInfo;
            PropertyInfo propertyInfo = member as PropertyInfo;

            if (fieldInfo != null)
            {
                value = fieldInfo.GetValue(instance);
                return true;
            }
            else if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(instance, null);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        // Query/Scan building
        private static ScanFilter ComposeScanFilter(IEnumerable<ScanCondition> conditions, ItemStorageConfig storageConfig)
        {
            ScanFilter filter = new ScanFilter();
            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    PropertyStorage propertyStorage = storageConfig.GetPropertyStorage(condition.PropertyName);
                    List<AttributeValue> attributeValues = new List<AttributeValue>();
                    foreach (var value in condition.Values)
                    {
                        var entry = ToDynamoDBEntry(propertyStorage, value, true);
                        if (entry == null)
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.InvariantCulture, "Unable to convert value corresponding to property [{0}] to DynamoDB representation", condition.PropertyName));

                        AttributeValue nativeValue = entry.ConvertToAttributeValue();
                        if (nativeValue != null)
                        {
                            attributeValues.Add(nativeValue);
                        }
                    }
                    filter.AddCondition(propertyStorage.AttributeName, condition.Operator, attributeValues);
                }
            }
            return filter;
        }

        private static QueryFilter ComposeQueryFilter(DynamoDBFlatConfig currentConfig, object hashKeyValue, IEnumerable<QueryCondition> conditions, ItemStorageConfig storageConfig, out List<string> indexNames)
        {
            if (hashKeyValue == null)
                throw new ArgumentNullException("hashKeyValue");

            // Set hash key property name
            // In case of index queries, if GSI, different key could be used
            string hashKeyProperty = storageConfig.HashKeyPropertyNames[0];
            hashKeyProperty = storageConfig.GetCorrectHashKeyProperty(currentConfig, hashKeyProperty);

            PropertyStorage propertyStorage = storageConfig.GetPropertyStorage(hashKeyProperty);
            string hashAttributeName = propertyStorage.AttributeName;

            DynamoDBEntry hashKeyEntry = ValueToDynamoDBEntry(propertyStorage, hashKeyValue);
            if (hashKeyEntry == null) throw new InvalidOperationException("Unable to convert hash key value for property " + hashKeyProperty);

            Document hashKey = new Document();
            hashKey[hashAttributeName] = hashKeyEntry;

            return ComposeQueryFilterHelper(currentConfig, hashKey, conditions, storageConfig, out indexNames);
        }

        private static string NO_INDEX = DynamoDBFlatConfig.DefaultIndexName;
        // This method composes the query filter and determines the possible indexes that the filter
        // may be used against. In the case where the condition property is also a RANGE key on the
        // table and not just on LSI/GSI, the potential index will be "" (absent).
        private static QueryFilter ComposeQueryFilterHelper(
            DynamoDBFlatConfig currentConfig,
            Document hashKey,
            IEnumerable<QueryCondition> conditions,
            ItemStorageConfig storageConfig,
            out List<string> indexNames)
        {
            if (hashKey == null)
                throw new ArgumentNullException("hashKey");

            if (storageConfig.HashKeyPropertyNames.Count != 1)
                throw new InvalidOperationException("Must have one hash key defined for the table " + storageConfig.TableName);
            if (storageConfig.RangeKeyPropertyNames.Count != 1 && storageConfig.IndexNameToGSIMapping.Count == 0)
                throw new InvalidOperationException("Must have one range key or a GSI index defined for the table " + storageConfig.TableName);

            QueryFilter filter = new QueryFilter();

            // Configure hash-key equality condition
            string hashKeyProperty = storageConfig.HashKeyPropertyNames[0];
            hashKeyProperty = storageConfig.GetCorrectHashKeyProperty(currentConfig, hashKeyProperty);
            PropertyStorage propertyStorage = storageConfig.GetPropertyStorage(hashKeyProperty);
            string attributeName = propertyStorage.AttributeName;
            DynamoDBEntry hashValue = hashKey[attributeName];
            filter.AddCondition(attributeName, QueryOperator.Equal, hashValue);

            indexNames = new List<string>();
            if (conditions != null)
            {
                foreach (QueryCondition condition in conditions)
                {
                    object[] conditionValues = condition.Values;
                    PropertyStorage conditionProperty = storageConfig.GetPropertyStorage(condition.PropertyName);
                    if (conditionProperty.IsLSIRangeKey || conditionProperty.IsGSIKey)
                        indexNames.AddRange(conditionProperty.IndexNames);
                    if (conditionProperty.IsRangeKey)
                        indexNames.Add(NO_INDEX);
                    List<AttributeValue> attributeValues = ConvertConditionValues(conditionValues, conditionProperty);
                    filter.AddCondition(conditionProperty.AttributeName, condition.Operator, attributeValues);
                }
            }
            if (currentConfig.QueryFilter != null)
            {
                foreach (ScanCondition condition in currentConfig.QueryFilter)
                {
                    object[] conditionValues = condition.Values;
                    PropertyStorage conditionProperty = storageConfig.GetPropertyStorage(condition.PropertyName);
                    List<AttributeValue> attributeValues = ConvertConditionValues(conditionValues, conditionProperty, true);
                    filter.AddCondition(conditionProperty.AttributeName, condition.Operator, attributeValues);
                }
            }
            return filter;
        }

        private static List<AttributeValue> ConvertConditionValues(object[] conditionValues, PropertyStorage conditionProperty, bool canReturnPrimitiveInsteadOfList = false)
        {
            List<AttributeValue> attributeValues = new List<AttributeValue>();
            foreach (var conditionValue in conditionValues)
            {
                DynamoDBEntry entry = ToDynamoDBEntry(conditionProperty, conditionValue, canReturnPrimitiveInsteadOfList);
                AttributeValue attributeValue = entry.ConvertToAttributeValue();
                attributeValues.Add(attributeValue);
            }
            return attributeValues;
        }

        private static string GetQueryIndexName(DynamoDBFlatConfig flatConfig, List<string> indexNames)
        {
            string specifiedIndexName = flatConfig.IndexName;

            // remove possible duplicate indexes
            indexNames = indexNames.Distinct(StringComparer.Ordinal).ToList();

            string inferredIndexName = null;
            if (string.IsNullOrEmpty(specifiedIndexName) && indexNames.Count == 1)
            {
                inferredIndexName = indexNames[0];
            }
            else if (indexNames.Contains(specifiedIndexName, StringComparer.Ordinal))
            {
                inferredIndexName = specifiedIndexName;
            }
            else if (string.IsNullOrEmpty(inferredIndexName) && indexNames.Count > 0)
                throw new InvalidOperationException("Local Secondary Index range key conditions are used but no index could be inferred from model. Specified index name = " + specifiedIndexName);

            // index is both specified and inferred
            if (!string.IsNullOrEmpty(specifiedIndexName) && !string.IsNullOrEmpty(inferredIndexName))
            {
                // check that the indexes are equal
                if (string.Equals(inferredIndexName, specifiedIndexName, StringComparison.Ordinal))
                    return inferredIndexName;
                else
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        "Specified index name {0} does not match with inferred index name {1}", specifiedIndexName, inferredIndexName));
            }

            if (!string.IsNullOrEmpty(inferredIndexName))
                return inferredIndexName;

            if (!string.IsNullOrEmpty(specifiedIndexName))
                return specifiedIndexName;

            return null;
        }

        private static List<QueryCondition> CreateQueryConditions(DynamoDBFlatConfig flatConfig, QueryOperator op, IEnumerable<object> values, ItemStorageConfig storageConfig)
        {
            string rangeKeyPropertyName;

            string indexName = flatConfig.IndexName;
            if (string.IsNullOrEmpty(indexName))
                rangeKeyPropertyName = storageConfig.RangeKeyPropertyNames.FirstOrDefault();
            else
                rangeKeyPropertyName = storageConfig.GetRangeKeyByIndex(indexName);

            List<QueryCondition> conditions = new List<QueryCondition>
            {
                new QueryCondition(rangeKeyPropertyName, op, values.ToArray())
            };
            return conditions;
        }

        // Key creation
        private static DynamoDBEntry ValueToDynamoDBEntry(PropertyStorage propertyStorage, object value)
        {
            var entry = ToDynamoDBEntry(propertyStorage, value);
            return entry;
        }
        private static void ValidateKey(Key key, ItemStorageConfig storageConfig)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (storageConfig == null) throw new ArgumentNullException("storageConfig");
            if (key.Count == 0) throw new InvalidOperationException("Key is empty");

            foreach (string hashKey in storageConfig.HashKeyPropertyNames)
            {
                string attributeName = storageConfig.GetPropertyStorage(hashKey).AttributeName;
                if (!key.ContainsKey(attributeName))
                    throw new InvalidOperationException("Key missing hash key " + hashKey);
            }
            foreach (string rangeKey in storageConfig.RangeKeyPropertyNames)
            {
                string attributeName = storageConfig.GetPropertyStorage(rangeKey).AttributeName;
                if (!key.ContainsKey(attributeName))
                    throw new InvalidOperationException("Key missing range key " + rangeKey);
            }
        }

        internal static Key MakeKey(object hashKey, object rangeKey, ItemStorageConfig storageConfig)
        {
            if (storageConfig.HashKeyPropertyNames.Count != 1)
                throw new InvalidOperationException("Must have one hash key defined for the table " + storageConfig.TableName);

            Key key = new Key();

            string hashKeyPropertyName = storageConfig.HashKeyPropertyNames[0];
            PropertyStorage hashKeyProperty = storageConfig.GetPropertyStorage(hashKeyPropertyName);

            DynamoDBEntry hashKeyEntry = ValueToDynamoDBEntry(hashKeyProperty, hashKey);
            if (hashKeyEntry == null) throw new InvalidOperationException("Unable to convert hash key value for property " + hashKeyPropertyName);
            key[hashKeyProperty.AttributeName] = hashKeyEntry.ConvertToAttributeValue();

            if (storageConfig.RangeKeyPropertyNames.Count > 0)
            {
                if (storageConfig.RangeKeyPropertyNames.Count != 1)
                    throw new InvalidOperationException("Must have one range key defined for the table");

                string rangeKeyPropertyName = storageConfig.RangeKeyPropertyNames[0];
                PropertyStorage rangeKeyProperty = storageConfig.GetPropertyStorage(rangeKeyPropertyName);

                DynamoDBEntry rangeKeyEntry = ValueToDynamoDBEntry(rangeKeyProperty, rangeKey);
                if (rangeKeyEntry == null) throw new InvalidOperationException("Unable to convert range key value for property " + rangeKeyPropertyName);
                key[rangeKeyProperty.AttributeName] = rangeKeyEntry.ConvertToAttributeValue();
            }

            ValidateKey(key, storageConfig);
            return key;
        }

        internal static Key MakeKey<T>(T keyObject, ItemStorageConfig storageConfig)
        {
            ItemStorage keyAsStorage = ObjectToItemStorage<T>(keyObject, true, true, storageConfig);
            if (storageConfig.HasVersion) // if version field is defined, it would have been returned, so remove before making the key
                keyAsStorage.Document[storageConfig.VersionPropertyStorage.AttributeName] = null;
            Key key = new Key(keyAsStorage.Document.ToAttributeMap());
            ValidateKey(key, storageConfig);
            return key;
        }

        // Searching
        internal class ContextSearch
        {
            public DynamoDBFlatConfig FlatConfig { get; set; }
            public Search Search { get; set; }

            public ContextSearch(Search search, DynamoDBFlatConfig flatConfig)
            {
                Search = search;
                FlatConfig = flatConfig;
            }
        }

        private IEnumerable<T> FromSearch<T>(ContextSearch cs)
        {
            if (cs == null) throw new ArgumentNullException("cs");

            // Configure search to not collect results
            cs.Search.CollectResults = false;

            ItemStorageConfig storageConfig = StorageConfigCache.GetConfig<T>(cs.FlatConfig);
            while (!cs.Search.IsDone)
            {
                List<Document> set = cs.Search.GetNextSetHelper(false);
                foreach (var document in set)
                {
                    ItemStorage storage = new ItemStorage(storageConfig);
                    storage.Document = document;
                    T instance = DocumentToObject<T>(storage);
                    yield return instance;
                }
            }

            // Reset search to allow retrieving items more than once
            cs.Search.Reset();
        }

        #endregion

        #region Scan/Query

        private ContextSearch ConvertScan<T>(IEnumerable<ScanCondition> conditions, DynamoDBOperationConfig operationConfig)
        {
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, this.Config);
            ItemStorageConfig storageConfig = StorageConfigCache.GetConfig<T>(flatConfig);
            ScanFilter filter = ComposeScanFilter(conditions, storageConfig);

            Table table = GetTargetTable(storageConfig, flatConfig);
            var scanConfig = new ScanOperationConfig
            {
                AttributesToGet = storageConfig.AttributesToGet,
                Select = SelectValues.SpecificAttributes,
                Filter = filter,
                ConditionalOperator = flatConfig.ConditionalOperator
            };
            Search scan = table.Scan(scanConfig);
            return new ContextSearch(scan, flatConfig);
        }

        private ContextSearch ConvertFromScan<T>(ScanOperationConfig scanConfig, DynamoDBOperationConfig operationConfig)
        {
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, Config);
            Table table = GetTargetTableInternal<T>(operationConfig);
            Search search = table.Scan(scanConfig);
            return new ContextSearch(search, flatConfig);
        }

        private ContextSearch ConvertFromQuery<T>(QueryOperationConfig queryConfig, DynamoDBOperationConfig operationConfig)
        {
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, Config);
            Table table = GetTargetTableInternal<T>(operationConfig);
            Search search = table.Query(queryConfig);
            return new ContextSearch(search, flatConfig);
        }

        private ContextSearch ConvertQueryByValue<T>(object hashKeyValue, QueryOperator op, IEnumerable<object> values, DynamoDBOperationConfig operationConfig)
        {
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, Config);
            ItemStorageConfig storageConfig = StorageConfigCache.GetConfig<T>(flatConfig);
            List<QueryCondition> conditions = CreateQueryConditions(flatConfig, op, values, storageConfig);
            ContextSearch query = ConvertQueryByValue<T>(hashKeyValue, conditions, operationConfig, storageConfig);
            return query;
        }

        private ContextSearch ConvertQueryByValue<T>(object hashKeyValue, IEnumerable<QueryCondition> conditions, DynamoDBOperationConfig operationConfig, ItemStorageConfig storageConfig = null)
        {
            DynamoDBFlatConfig flatConfig = new DynamoDBFlatConfig(operationConfig, Config);
            if (storageConfig == null)
                storageConfig = StorageConfigCache.GetConfig<T>(flatConfig);

            List<string> indexNames;
            QueryFilter filter = ComposeQueryFilter(flatConfig, hashKeyValue, conditions, storageConfig, out indexNames);
            return ConvertQueryHelper<T>(flatConfig, storageConfig, filter, indexNames);
        }
        private ContextSearch ConvertQueryHelper<T>(DynamoDBFlatConfig currentConfig, ItemStorageConfig storageConfig, QueryFilter filter, List<string> indexNames)
        {
            Table table = GetTargetTable(storageConfig, currentConfig);
            string indexName = GetQueryIndexName(currentConfig, indexNames);
            var queryConfig = new QueryOperationConfig
            {
                Filter = filter,
                ConsistentRead = currentConfig.ConsistentRead.Value,
                BackwardSearch = currentConfig.BackwardQuery.Value,
                IndexName = indexName,
                ConditionalOperator = currentConfig.ConditionalOperator
            };
            if (string.IsNullOrEmpty(indexName))
            {
                queryConfig.Select = SelectValues.SpecificAttributes;
                List<string> attributesToGet = storageConfig.AttributesToGet;
                queryConfig.AttributesToGet = attributesToGet;
            }
            else
            {
                queryConfig.Select = SelectValues.AllProjectedAttributes;
            }
            Search query = table.Query(queryConfig);

            return new ContextSearch(query, currentConfig);
        }

        private AsyncSearch<T> FromSearchAsync<T>(ContextSearch contextSearch)
        {
            return new AsyncSearch<T>(this, contextSearch);
        }

        #endregion
    }
}
