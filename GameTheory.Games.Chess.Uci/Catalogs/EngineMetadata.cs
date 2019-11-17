// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Catalogs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class EngineMetadata
    {
        [JsonProperty("arguments", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Arguments { get; set; }

        [JsonProperty("author", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Author { get; set; }

        [JsonProperty("engineName", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string EngineName { get; set; }

        [JsonProperty("executable")]
        public string Executable { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extensions { get; set; }
    }
}
