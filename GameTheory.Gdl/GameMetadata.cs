// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Gdl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class GameMetadata
    {
        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("rulesheet")]
        public string RuleSheet { get; set; }

        [JsonProperty("stylesheet", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StyleSheet { get; set; }

        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Description { get; set; }

        [JsonProperty("user_interface", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string UserInterface { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Extensions { get; set; }
    }
}
