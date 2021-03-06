﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace GloWS_REST_Library.Objects.Ship
{
    public class DangerousGoods
    {
        /// <summary>
        /// Valid DHL Express Dangerous good content id
        /// </summary>
        [Required]
        [StringLength(3)]
        [JsonProperty("ContentId")]
        public string ContentId { get; set; }

        /// <summary>
        /// This is a numeric string with up to 7 char (i.e. 1000,00 or 1000.00)
        /// </summary>
        [StringLength(7, MinimumLength = 1)]
        [JsonProperty("DryIceTotalNetWeight")]
        public string DryIceTotalNetWeightString
        {
            get { return $"{this.DryIceTotalNetWeight:###0.00}"; }
            set { this.DryIceTotalNetWeight = decimal.Parse(value); }
        }

        /// <summary>
        /// Net weight of dry ice in the shipment
        /// </summary>
        [Range(0.1d, 1000d)]
        [JsonIgnore]
        public decimal DryIceTotalNetWeight { get; set; }

        /// <summary>
        /// Comma separated UN codes – eg. “UN-7843268473”, “7843268473,123”
        /// </summary>
        [JsonProperty("UNCode")]
        public string UNCode { get; set; }
    }
}