﻿using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace GloWS_REST_Library.Objects.RateQuery {
    public class Address {
        /// <summary>
        /// Street name and number
        /// </summary>
        [Required]
        [StringLength(35)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetLines { get; set; }

        /// <summary>
        /// Street name should be sent as separate attribute if feasible.
        /// </summary>
        [StringLength(35)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetName { get; set; }

        /// <summary>
        /// Street number should be sent as separate attribute, if feasible.
        /// </summary>
        [StringLength(15)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetNumber { get; set; }

        /// <summary>
        /// Additional address information
        /// </summary>
        [StringLength(35)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetLines2 { get; set; }

        /// <summary>
        /// Additional address information
        /// </summary>
        [StringLength(35)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetLines3 { get; set; }

        /// <summary>
        /// City name
        /// </summary>
        [Required]
        [StringLength(35)]
        public string City { get; set; }

        /// <summary>
        /// 2 Letter State code for the USA only
        /// </summary>
        [StringLength(2)]
        [JsonProperty("StateOrProvinceCode", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        public string USSateCode { get; set; }

        /// <summary>
        /// Postal code / zipcode
        /// </summary>
        [StringLength(12)]
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string PostalCode { get; set; }

        /// <summary>
        /// ISO 2 character codes of the country.
        /// </summary>
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string CountryCode { get; set; }
    }
}