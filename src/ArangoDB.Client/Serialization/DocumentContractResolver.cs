﻿using ArangoDB.Client.Common.Newtonsoft.Json;
using ArangoDB.Client.Common.Newtonsoft.Json.Serialization;
using ArangoDB.Client.Common.Utility;
using ArangoDB.Client.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.Client.Serialization
{
    public class DocumentContractResolver : DefaultContractResolver
    {
        IArangoDatabase db;
        public DocumentContractResolver(IArangoDatabase db) 
            : base(true) 
        {
            this.db = db;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            IList<JsonProperty> convertedProperties = new List<JsonProperty>();
            foreach (var p in properties)
            {
                IDocumentPropertySetting documentProperty = null;
                db.SharedSetting.Collection.ChangeDocumentPropertyForType(type, p.UnderlyingName, x => documentProperty = x);
                if (documentProperty != null)
                {
                    if (documentProperty.IgnoreProperty)
                        continue;
                }

                p.PropertyName = db.SharedSetting.Collection.ResolvePropertyName(type, p.UnderlyingName);

                if (p.PropertyName == "_key" || p.PropertyName == "_id" || p.PropertyName == "_rev")
                    p.NullValueHandling = NullValueHandling.Ignore;

                convertedProperties.Add(p);
            }

            return convertedProperties;
        }
    }
}
