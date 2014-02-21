﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nest.Resolvers.Converters;
using Newtonsoft.Json;
using Elasticsearch.Net;

namespace Nest
{
	public interface IIndexSettingsResponse : IResponse
	{
		IndexSettings Settings { get; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(IndexSettingsResponseConverter))]
	public class IndexSettingsResponse : BaseResponse, IIndexSettingsResponse
	{
		[JsonIgnore]
		public IndexSettings Settings
		{
			get { return Nodes.HasAny() ? Nodes.First().Value : null; }
		}

		[JsonIgnore]
		public Dictionary<string, IndexSettings> Nodes { get; set; }

	}
}
