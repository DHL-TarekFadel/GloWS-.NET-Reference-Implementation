﻿using GloWS_REST_Library.Objects.Common;
using GloWS_REST_Library.Objects.Plumbing.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
namespace GloWS_REST_Library.Objects.Ship
{
    public class ShipmentInfo
    {
        //[JsonProperty("@provider")]
        //public string Provider => "DHL";

        /// <summary>
        /// Possible values:
        /// - REGULAR_PICKUP
        /// The pickup location is already served by a regular courier and an additional pickup does not need to be considered for this service
        /// - REQUEST_COURIER
        /// The rating response returns products for which the pickup capability is given, based on ShipmentTimeStamp.
        /// </summary>
        [Required]
        [JsonProperty("DropOffType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.DropOffType DropOffType { get; set; }

        /// <summary>
        /// The shipping product requested for this shipment, corresponding to the DHL Global Product codes.
        /// </summary>
        [Required]
        [StringLength(1)]
        [JsonProperty("ServiceType")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Required if the "Billing" section is not defined.
        /// </summary>
        [StringLength(9)]
        [JsonProperty("Account", NullValueHandling = NullValueHandling.Ignore)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Contains specific billing information (if paied by receiver/3rd party)
        /// </summary>
        [ValidateObject]
        [JsonProperty("Billing")]
        public BillilngInfo Billing { get; set; }

        /// <summary>
        /// Special Services (ex: Insurance, Saturday Delivery, etc.)
        /// </summary>
        [ValidateObject]
        [JsonProperty("SpecialServices", NullValueHandling = NullValueHandling.Ignore)]
        public SpecialServices SpecialServices { get; set; }

        /// <summary>
        /// The currency of the monetary values presented in the request.
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [JsonProperty("Currency")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The unit of measurement for the dimensions of the package.
        /// </summary>
        [Required]
        [JsonProperty("UnitOfMeasurement")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.UnitOfMeasurement UnitOfMeasurement { get; set; }

        /// <summary>
        /// The shipment identification number does not need to be transmitted in the request as the operation will assign a new number and return it in the response. Only used when UseOwnShipmentdentificationNumber set to Y and this feature enabled within client profile
        /// </summary>
        [StringLength(10, MinimumLength = 10)]
        [JsonProperty("ShipmentIdentificationNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnAWBNumber { get; set; }

        /// <summary>
        /// You can request your own AWB range from your DHL Express IT consultant and store these locally in your integration however this is not needed as if you leave this tag then DHL will automatically assign the AWB centrally.
        /// In addition this special function needs to be enabled for your username by your DHL Express IT Consultant.
        /// </summary>
        [JsonProperty("UseOwnShipmentIdentificationNumber", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.YesNo? UseOwnAWBNumber { get; set; }

        /// <summary>
        /// Number of packages which need to be processed.
        /// </summary>
        [PositiveInteger]
        [JsonProperty("PackagesCount")]
        public int NumberOfPieces { get; set; }

        [JsonProperty("SendPackage", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.YesNo? SendPackage { get; set; }

        [JsonProperty("LabelType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Enums.LabelFormat? LabelFormat { get; set; }

        /// <summary>
        /// Any valid DHL Express label template (please contact your DHL Express IT representative for a list of labels) – If this node is left blank then the default DHL ecommerce label template will be used.
        /// </summary>
        [StringLength(20)]
        [JsonProperty("LabelTemplate", NullValueHandling = NullValueHandling.Ignore)]
        public string LabelTemplate { get; set; }

        /// <summary>
        /// Any valid DHL Express archive label template (please contact your DHL Express IT representative for a list of labels) – If this node is left blank then the default DHL ecommerce label template will be used.
        /// </summary>
        [StringLength(20)]
        [JsonProperty("ArchiveLabelTemplate", NullValueHandling = NullValueHandling.Ignore)]
        public string ArchiveLabelTemplate { get; set; }

        /// <summary>
        /// Enable Paperless Trade (PLT) for this shipment
        /// </summary>
        [JsonProperty("PaperlessTradeEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool PaperlessTradeEnabled { get; set; }

        [JsonProperty("PaperlessTradeImage", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] PaperlessTradeImage { get; set; }

        public ShipmentInfo() { }

        public ShipmentInfo(Enums.DropOffType dropOffType)
        {
            this.DropOffType = dropOffType;
        }
    }
}