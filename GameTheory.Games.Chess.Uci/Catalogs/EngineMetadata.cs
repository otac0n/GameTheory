// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Uci.Catalogs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Describes the schema of a UCI engine METADATA file.
    /// </summary>
    public class EngineMetadata
    {
        /// <summary>
        /// Gets or sets the executable arguments.
        /// </summary>
        [JsonProperty("arguments", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        [JsonProperty("author", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the engine name.
        /// </summary>
        [JsonProperty("engineName", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string EngineName { get; set; }

        /// <summary>
        /// Gets or sets the executable file.
        /// </summary>
        [JsonProperty("executable")]
        public string Executable { get; set; }

        /// <summary>
        /// Gets or sets the JSON extension data.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken> Extensions { get; set; }
    }
}
