﻿using GloWS_REST_Library.Objects.Common;
using GloWS_REST_Library.Objects.Plumbing.Attributes;
using Newtonsoft.Json;

namespace GloWS_REST_Library.Objects.RateQuery {
    public class RequestedPackages {

        [PositiveInteger]
        [JsonProperty("@number")]
        public int PieceNumber { get; set; }

        public Weight Weight { get; set; }

        public Dimensions Dimensions { get; set; }
    }
}