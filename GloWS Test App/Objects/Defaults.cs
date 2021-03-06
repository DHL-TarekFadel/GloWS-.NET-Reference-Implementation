﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GloWS_Test_App.Objects
{
    public class Defaults
    {
        public string ShipperAccountNumber { get; set; }

        public string BillingAccountNumber { get; set; }

        public string DutyAccountNumber { get; set; }

        public DefaultAddress From { get; set; }

        public DefaultAddress To { get; set; }

        public decimal? DeclaredValue { get; set; }

        public string DeclaredCurrency { get; set; }

        public string Contents { get; set; }

        public List<DefaultPiece> Pieces { get; set; }

        public string TrackingAWBNumber { get; set; }

        public DefaultEPOD EPOD { get; set; }
    }
}
