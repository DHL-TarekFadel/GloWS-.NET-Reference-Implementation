﻿using System.Collections.Generic;
using GloWS_REST_Library.Objects.Common.Response;
using Newtonsoft.Json;

namespace GloWS_REST_Library.Objects.RateQuery.Response
{
    public class Provider {
        [JsonProperty("@code")]
        public string Code { get; set; }

        public List<Notification> Notification { get; set; }

        public List<Service> Service { get; set; }
    }
}
