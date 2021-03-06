﻿using GloWS_REST_Library;
using GloWS_REST_Library.Objects;
using GloWS_REST_Library.Objects.Common;
using GloWS_REST_Library.Objects.Ship;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using GloWS_REST_Library.Objects.Exceptions;
using GloWS_REST_Library.Objects.Ship.Response;

namespace GloWS_Test_App.REST
{
    public partial class Ship : Form
    {
        private List<string> _productCodes = new List<string> { "", "0", "1", "2", "3", "4", "5", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private string GloWS_Request = string.Empty;
        private string GloWS_Response = string.Empty;

        private bool _logoAvailable;
        private byte[] _logoData;
        private bool _invoiceAvailable;
        private byte[] _invoiceData;

        private List<string> _generatedTempFiles = new List<string>();

        //private string[] doxProducts = {}

        public Ship()
        {
            InitializeComponent();
        }

        private void Ship_Load(object sender, EventArgs e)
        {
            cmbProductCode.DataSource = _productCodes;
            cmbShipmentDimsUOM.DataSource = new[] { "", "CM", "IN" };
            cmbShipmentWeightUOM.DataSource = new[] { "", "KG", "LB" };

            if (null != Common.Defaults)
            {
                Common.ApplyDefault(ref txtShipperAccountNumber, Common.Defaults.ShipperAccountNumber);
                Common.ApplyDefault(ref txtDutyAccountNumber, Common.Defaults.DutyAccountNumber);
                Common.ApplyDefault(ref txtShipmentDeclaredValue, Common.Defaults.DeclaredValue, 0M);
                Common.ApplyDefault(ref txtShipmentDeclaredValueCurrency, Common.Defaults.DeclaredCurrency);
                Common.ApplyDefault(ref txtShipmentContents, Common.Defaults.Contents);

                if (null != Common.Defaults.From)
                {
                    var from = Common.Defaults.From;
                    Common.ApplyDefault(ref txtShipperCompany, from.CompanyName);
                    Common.ApplyDefault(ref txtShipperName, from.PersonName);
                    Common.ApplyDefault(ref txtShipperAddress1, from.Address1);
                    Common.ApplyDefault(ref txtShipperAddress2, from.Address2);
                    Common.ApplyDefault(ref txtShipperAddress3, from.Address3);
                    Common.ApplyDefault(ref txtShipperCity, from.City);
                    Common.ApplyDefault(ref txtShipperState, from.USState);
                    Common.ApplyDefault(ref txtShipperPostalCode, from.PostalCode);
                    Common.ApplyDefault(ref txtShipperCountry, from.CountryCode);
                    Common.ApplyDefault(ref txtShipperMobileNumber, from.MobileNumber);
                    Common.ApplyDefault(ref txtShipperEMailAddress, from.EMailAddress);
                }

                if (null != Common.Defaults.To)
                {
                    var to = Common.Defaults.To;
                    Common.ApplyDefault(ref txtConsigneeCompany, to.CompanyName);
                    Common.ApplyDefault(ref txtConsigneeName, to.PersonName);
                    Common.ApplyDefault(ref txtConsigneeAddress1, to.Address1);
                    Common.ApplyDefault(ref txtConsigneeAddress2, to.Address2);
                    Common.ApplyDefault(ref txtConsigneeAddress3, to.Address3);
                    Common.ApplyDefault(ref txtConsigneeCity, to.City);
                    Common.ApplyDefault(ref txtConsigneeState, to.USState);
                    Common.ApplyDefault(ref txtConsigneePostalCode, to.PostalCode);
                    Common.ApplyDefault(ref txtConsigneeCountry, to.CountryCode);
                    Common.ApplyDefault(ref txtConsigneeMobileNumber, to.MobileNumber);
                    Common.ApplyDefault(ref txtConsigneeEMailAddress, to.EMailAddress);
                }

                if (null != Common.Defaults.Pieces
                    && Common.Defaults.Pieces.Any())
                {
                    var piece = Common.Defaults.Pieces.First();

                    Common.ApplyDefault(ref cmbShipmentDimsUOM, piece.DimUnit, "CM");
                    Common.ApplyDefault(ref cmbShipmentWeightUOM, piece.WeightUnit, "KG");

                    Common.ApplyDefault(ref txtShipmentWeight, piece.Weight);
                    Common.ApplyDefault(ref txtShipmentHeight, piece.Height);
                    Common.ApplyDefault(ref txtShipmentWidth, piece.Width);
                    Common.ApplyDefault(ref txtShipmentDepth, piece.Depth);

                    if (null != piece.Height
                        && null != piece.Width
                        && null != piece.Depth)
                    {
                        decimal preDivisor = (decimal) (piece.Height * piece.Width * piece.Depth);

                        txtShipmentDimWeight.Text = $"{preDivisor / 5000:#,##0.00}";
                    }
                }
            }
            else
            {
                cmbShipmentDimsUOM.SelectedItem = "CM";
                cmbShipmentWeightUOM.SelectedItem = "KG";
            }

            cmbProductCode.SelectedItem = "P";
        }

        private void Ship_FormClosing(object sender, FormClosingEventArgs e)
        {            
            try
            {
                foreach (string tempFilename in _generatedTempFiles)
                {
                    File.Delete(tempFilename);
                }
            }
            catch (IOException)
            {
                if (DialogResult.No == MessageBox.Show("Cannot delete temporary AWB file(s). By closing this form, no automatic cleanup will happen. Do you still want to exit?"
                                                        , "Files still open"
                                                        , MessageBoxButtons.YesNo
                                                        , MessageBoxIcon.Warning
                                                        , MessageBoxDefaultButton.Button2))
                {
                    e.Cancel = true;
                }
            }
        }

        private void BtnShip_Click(object sender, EventArgs e)
        {
            ClearInputErrors();

            // Validate all our inputs
            if (!ValidateInputs())
            {
                MessageBox.Show("Please fix the issues on the inputs marked in red.");
                return;
            }

            // All validation done, send the data to GloWS.
#pragma warning disable IDE0017 // Simplify object initialization
            try
            {
                this.Enabled = false;

                // Reset AWB link label
                llblAWB.Tag = null;
                llblAWB.LinkBehavior = LinkBehavior.NeverUnderline;
                llblAWB.ForeColor = Color.Black;
                llblAWB.Font = new Font(llblAWB.Font, FontStyle.Regular);

                // Determine if this is a dox or non-dox shipment
                bool isDox = !(new[] { "3", "4", "8", "E", "F", "H", "J", "M", "P", "Q", "V", "Y" }).Contains(cmbProductCode.SelectedValue.ToString());
                bool isDomestic = "N" == cmbProductCode.Text;

                CreateShipmentRequest req = new CreateShipmentRequest();
                
#pragma warning restore IDE0017 // Simplify object initialization

                if (cbxShipmentRequestPickup.Checked)
                {
                    req.Data.ShipmentInfo.DropOffType = Enums.DropOffType.RequestCourier;
                }
                else
                {
                    req.Data.ShipmentInfo.DropOffType = Enums.DropOffType.RegularPickup;
                }

                string[] pltCountries = new string[] { "AE", "SA", "US"};
                bool isPLT = false;

                /*** PLT ***/
                if (!isDomestic
                    && !isDox
                    && _invoiceAvailable
                    && (pltCountries.Contains(txtShipperCountry.Text)
                        || pltCountries.Contains(txtConsigneeCountry.Text))
                    )
                {
                    req.Data.ShipmentInfo.PaperlessTradeEnabled = true;
                    req.Data.ShipmentInfo.PaperlessTradeImage = _invoiceData;
                    isPLT = true;
                }

                req.Data.ShipmentInfo.ProductCode = cmbProductCode.SelectedValue.ToString();
                
                // SU = Standard (american) Units (LB, IN); SI = Standard International (KG, CM)
                if ("KG" == cmbShipmentWeightUOM.SelectedValue.ToString())
                {
                    req.Data.ShipmentInfo.UnitOfMeasurement = Enums.UnitOfMeasurement.SI;
                }
                else
                {
                    req.Data.ShipmentInfo.UnitOfMeasurement = Enums.UnitOfMeasurement.SU;
                }

                // If the billing element is defined (it should be used anyway) then there is no need for the
                // generic shipmentInfo.Account element to be populated.
                //shipmentInfo.Account = txtShipperAccountNumber.Text;
                req.Data.ShipmentInfo.Billing = new BillilngInfo(txtShipperAccountNumber.Text, txtShipperAccountNumber.Text, Enums.AccountRole.Shipper);

                if (!isDomestic && !isDox)
                {
                    // We have a non-dox shipment
                    req.Data.ShipmentInfo.CurrencyCode = txtShipmentDeclaredValueCurrency.Text;
                }

                req.Data.ShipmentInfo.LabelFormat = Enums.LabelFormat.PDF;

                req.Data.ShipmentInfo.NumberOfPieces = 1;

                DateTime timestamp;

                if (DateTime.Now.TimeOfDay > new TimeSpan(18, 00, 00))
                {
                    timestamp = DateTime.Now.AddDays(1);
                    timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 10, 0, 0);
                }
                else
                {
                    timestamp = DateTime.Now.AddMinutes(10);
                }

                req.Data.Timestamp = timestamp;
                //.RequestedShipment.ShipTimestamp = $"{timestamp:s} GMT{timestamp:zzz}";

                if (!IsBlank(txtDutyAccountNumber))
                {
                    req.Data.TermsOfTrade = Enums.TermsOfTrade.DDP;
                }
                else
                {
                    req.Data.TermsOfTrade = Enums.TermsOfTrade.DDU;
                }


                if (!isDox && !isDomestic)
                {
                    req.Data.CustomsInformation.ShipmentType = Enums.ShipmentType.NonDocuments;
                    Commodity commodities = new Commodity();
                    commodities.CustomsValue = 20M;
                    commodities.COO = "AE";
                    commodities.ShipmentContents = "Test Commoditiy";
                    commodities.NumberOfPieces = 1;
                    commodities.UnitPrice = 10M;
                    commodities.Quantity = "2";
                    req.Data.CustomsInformation.Commodities = commodities;
                }
                else
                {
                    req.Data.CustomsInformation.ShipmentType = Enums.ShipmentType.Documents;
                }


                /*** SHIPPER ***/
                AddressData shipper = new AddressData();

                if (!IsBlank(txtShipperCompany))
                {
                    shipper.Contact.CompanyName = txtShipperCompany.Text;
                }
                else
                {
                    shipper.Contact.CompanyName = txtShipperName.Text;
                }
                shipper.Contact.PersonName = txtShipperName.Text;
                shipper.Contact.EMailAddress = txtShipperEMailAddress.Text;
                shipper.Contact.PhoneNumber = txtShipperMobileNumber.Text;

                shipper.Address.AddressLine1 = txtShipperAddress1.Text;
                shipper.Address.AddressLine2 = txtShipperAddress2.Text;
                shipper.Address.AddressLine3 = txtShipperAddress3.Text;
                shipper.Address.CityName = txtShipperCity.Text;
                shipper.Address.USStateCode = txtShipperState.Text;
                shipper.Address.CountryCode = txtShipperCountry.Text;
                shipper.Address.PostalOrZipCode = txtShipperPostalCode.Text;

                req.Data.ShipmentAddresses.Shipper = shipper;

                /*** CONSIGNEE ***/
                AddressData consignee = new AddressData();

                if (!IsBlank(txtConsigneeCompany))
                {
                    consignee.Contact.CompanyName = txtConsigneeCompany.Text;
                }
                else
                {
                    consignee.Contact.CompanyName = txtConsigneeName.Text;
                }
                consignee.Contact.PersonName = txtConsigneeName.Text;
                consignee.Contact.EMailAddress = txtConsigneeEMailAddress.Text;
                consignee.Contact.PhoneNumber = txtConsigneeMobileNumber.Text;

                consignee.Address.AddressLine1 = txtConsigneeAddress1.Text;
                consignee.Address.AddressLine2 = txtConsigneeAddress2.Text;
                consignee.Address.AddressLine3 = txtConsigneeAddress3.Text;
                consignee.Address.CityName = txtConsigneeCity.Text;
                consignee.Address.USStateCode = txtConsigneeState.Text;
                consignee.Address.CountryCode = txtConsigneeCountry.Text;
                consignee.Address.PostalOrZipCode = txtConsigneePostalCode.Text;

                req.Data.ShipmentAddresses.Consignee = consignee;

                /*** PICKUP ***/
                if (cbxShipmentRequestPickup.Checked)
                {
                    req.Data.ShipmentAddresses.Pickup = req.Data.ShipmentAddresses.Shipper;
                }

                /*** PIECES ***/

                Package singlePackage = new Package
                {
                    Number = 1,
                    Weight = decimal.Parse($"{txtShipmentWeight.Text}")
                };

                int height = GetDimension(ref txtShipmentHeight);
                int width = GetDimension(ref txtShipmentWidth);
                int depth = GetDimension(ref txtShipmentDepth);

                if (height > 0
                    && width > 0
                    && depth > 0)
                {
                    // Keep the divisor at 5000.0, this forces .NET to treate the result as a float and not an integer.
                    singlePackage.Dimensions = new Dimensions(height, width, depth);
                }

                singlePackage.PieceContents = req.Data.CustomsInformation.Commodities.ShipmentContents;

                singlePackage.CustomerReferences = $"{DateTime.Now.Ticks}";

                req.Data.Packages.PackageList.Add(singlePackage);

                /*** SPECIAL SEVICES ***/

                req.Data.ShipmentInfo.SpecialServices = new SpecialServices
                {
                    Service = new List<SpecialService>()
                };

                if (isPLT)
                {
                    req.Data.ShipmentInfo.SpecialServices.Service.Add(new SpecialService("WY"));
                }

                if (!req.Data.ShipmentInfo.SpecialServices.Service.Any())
                {
                    req.Data.ShipmentInfo.SpecialServices = null;
                }

                /*** GENERATE SHIPMENT ***/
                GloWS glows = new GloWS_REST_Library.GloWS(Common.username, Common.password, (Common.IsProduction ? Common.restProductionBaseUrl : Common.restTestingBaseUrl));

                CreateShipmentResponse resp;

                try
                {
                    resp = glows.RequestShipment(req);
                }
                catch (GloWSValidationException gvx)
                {
                    MessageBox.Show(gvx.Message, "GVX");
                    txtResultAWB.Text = "VALIDATION ERROR!";
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "EX");
                    txtResultAWB.Text = "ERROR!";
                    return;
                }

                GloWS_Request = glows.LastJSONRequest;
                GloWS_Response = glows.LastJSONResponse;

                if (null == resp || null == resp.Data)
                {
                    MessageBox.Show("There was an error in generating the shipment.");
                    return;
                }

                // Display our results

                txtResultAWB.Text = (resp.Data.AWB ?? String.Empty);
                txtResultBookingReferenceNumber.Text = (resp.Data.BookingReferenceNumber ?? string.Empty);

                if (null != resp.Data.Pieces
                    && resp.Data.Pieces.Any())
                {
                    string prefix = string.Empty;
                    string pieces = string.Empty;

                    foreach (Piece piece in resp.Data.Pieces)
                    {
                        pieces += $"{prefix}{piece.LPN}";
                        prefix = Environment.NewLine;
                    }

                    txtResultPieces.Text = pieces;
                }
                else
                {
                    txtResultPieces.Text = string.Empty;
                }

                if (null != resp.Data.Labels
                    && resp.Data.Labels.Any())
                {
                    int i = 0;
                    // Prepare our files for viewing
                    foreach (ShipmentImage label in resp.Data.Labels)
                    {
                        var tempFilename = Common.GetTempFilenameWithExtension(txtResultAWB.Text, label.ImageFormat);
                        File.WriteAllBytes(tempFilename, label.ImageData);
                        _generatedTempFiles.Add(tempFilename);

                        if (0 == i++)
                        {
                            llblAWB.Tag = tempFilename;
                            llblAWB.LinkBehavior = LinkBehavior.AlwaysUnderline;
                            llblAWB.ForeColor = Color.White;
                            llblAWB.Font = new Font(llblAWB.Font, FontStyle.Bold);
                        }
                    }
                }
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void ClearInputErrors()
        {

            /*** SHIPPER ***/
            ClearError(ref txtShipperName);
            ClearError(ref txtShipperMobileNumber);
            ClearError(ref txtShipperAddress1);
            ClearError(ref txtShipperCountry);
            ClearError(ref txtShipperCity);
            ClearError(ref txtShipperPostalCode);

            /*** CONSIGNEE ***/
            ClearError(ref txtConsigneeName);
            ClearError(ref txtConsigneeMobileNumber);
            ClearError(ref txtConsigneeAddress1);
            ClearError(ref txtConsigneeCountry);
            ClearError(ref txtConsigneeCity);
            ClearError(ref txtConsigneePostalCode);

            /*** SHIPMENT ***/
            ClearError(ref txtShipmentContents);
            ClearError(ref txtShipmentWeight);
        }

        private bool ValidateInputs()
        {
            bool isValid = true;

            /*** SHIPPER ***/
            if (IsBlank(txtShipperName))
            {
                IndicateError(ref txtShipperName);
                isValid = false;
            }
            if (IsBlank(txtShipperMobileNumber)) {
                IndicateError(ref txtShipperMobileNumber);
                isValid = false;
            }
            if (IsBlank(txtShipperAddress1))
            {
                IndicateError(ref txtShipperAddress1);
                isValid = false;
            }
            if (IsBlank(txtShipperCountry))
            {
                IndicateError(ref txtShipperCountry);
                isValid = false;
            }
            if (IsBlank(txtShipperCity) && IsBlank(txtShipperPostalCode))
            {
                IndicateError(ref txtShipperCity, "Either city or postal code must have a value");
                IndicateError(ref txtShipperPostalCode, "Either city or postal code must have a value");
                isValid = false;
            }
            if (!IsBlank(txtShipperState) && IsBlank(txtShipperPostalCode)) {
                IndicateError(ref txtShipperPostalCode, "Postal/Zip Code must have a value for country US");
                isValid = false;
            }

            /*** CONSIGNEE ***/
            if (IsBlank(txtConsigneeName))
            {
                IndicateError(ref txtConsigneeName);
                isValid = false;
            }
            if (IsBlank(txtConsigneeMobileNumber))
            {
                IndicateError(ref txtConsigneeMobileNumber);
                isValid = false;
            }
            if (IsBlank(txtConsigneeAddress1))
            {
                IndicateError(ref txtConsigneeAddress1);
                isValid = false;
            }
            if (IsBlank(txtConsigneeCountry))
            {
                IndicateError(ref txtConsigneeCountry);
                isValid = false;
            }
            if (IsBlank(txtConsigneeCity) && IsBlank(txtConsigneePostalCode))
            {
                IndicateError(ref txtConsigneeCity, "Either city or postal code must have a value");
                IndicateError(ref txtConsigneePostalCode, "Either city or postal code must have a value");
                isValid = false;
            }
            if (!IsBlank(txtConsigneeState) && IsBlank(txtConsigneePostalCode))
            {
                IndicateError(ref txtConsigneePostalCode, "Postal/Zip Code must have a value for country US");
                isValid = false;
            }

            /*** SHIPMENT ***/
            if (IsBlank(txtShipmentContents))
            {
                IndicateError(ref txtShipmentContents);
                isValid = false;
            }
            if (IsBlank(txtShipmentWeight))
            {
                IndicateError(ref txtShipmentWeight);
                isValid = false;
            }

            return isValid;
        }

        private bool IsBlank(TextBox tbx)
        {
            if (string.IsNullOrWhiteSpace(tbx.Text))
            {
                return true;
            }
            return false;
        }

        private void ClearError(ref TextBox tbx)
        {
            tbx.ForeColor = Color.Black;
            tbx.BackColor = Color.White;
            ToolTip1.SetToolTip(tbx, string.Empty);
        }

        private void IndicateError(ref TextBox tbx, string msg = "Field is required.")
        {
            tbx.ForeColor = Color.White;
            tbx.BackColor = Color.Red;
            ToolTip1.SetToolTip(tbx, msg);
        }

        private void CalculateDImWeight(object sender, EventArgs e)
        {
            int height = GetDimension(ref txtShipmentHeight);
            int width = GetDimension(ref txtShipmentWidth);
            int depth = GetDimension(ref txtShipmentDepth);

            if (height > 0
                && width > 0
                && depth > 0)
            {
                // Keep the divisor at 5000.0, this forces .NET to treate the result as a float and not an integer.
                txtShipmentDimWeight.Text = $"{(height * width * depth) / 5000.0}";
            }
            else
            {
                txtShipmentDimWeight.Text = string.Empty;
            }
        }

        private int GetDimension(ref TextBox tbx)
        {
            ClearError(ref tbx);

            if (IsBlank(tbx))
            {
                return -1;
            }
            if (!Regex.IsMatch(tbx.Text, "^\\d+$"))
            {
                IndicateError(ref tbx, "Please enter a valid whole number.");
                return 0;
            }
            try
            {
                int dim = int.Parse(tbx.Text);
                if (0 >= dim)
                {
                    IndicateError(ref tbx, "Please enter a non-zero positive whole number.");
                    return 0;
                }
                return dim;
            }
            catch
            {
                return 0;
            }
        }

        private void BtnViewRequest_Click(object sender, EventArgs e)
        {
            JSONViewer frm = new JSONViewer(GloWS_Request);
            frm.ShowDialog();
        }

        private void BtnViewResponse_Click(object sender, EventArgs e)
        {
            JSONViewer frm = new JSONViewer(GloWS_Response);
            frm.ShowDialog();
        }

        private void LlblAWB_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(llblAWB.Tag.ToString()))
            {
                System.Diagnostics.Process.Start(llblAWB.Tag.ToString());
            }
        }

        private void BtnUploadLogo_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png";
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                _logoData = File.ReadAllBytes(dlg.FileName);
                _logoAvailable = true;
                lblLogoUploaded.Text = "Loaded!";
                lblLogoUploaded.Font = new Font(lblLogoUploaded.Font, FontStyle.Bold);
            }
        }

        private void BtnUploadInvoice_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png|PDF Files|*.pdf";
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                _invoiceData = File.ReadAllBytes(dlg.FileName);
                _invoiceAvailable = true;
                lblInvoiceUploaded.Text = "Loaded!";
                lblInvoiceUploaded.Font = new Font(lblLogoUploaded.Font, FontStyle.Bold);
            }
        }

        private void CmbProductCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbProductCode.SelectedValue)
            {
                case "1":
                case "2":
                case "5":
                case "7":
                case "9":
                case "B":
                case "C":
                case "D":
                case "G":
                case "I":
                case "K":
                case "L":
                case "N":
                case "O":
                case "R":
                case "S":
                case "T":
                case "U":
                case "W":
                case "X":
                    txtShipmentDeclaredValue.Enabled = false;
                    txtShipmentDeclaredValueCurrency.Enabled = false;
                    break;
                default:
                    txtShipmentDeclaredValue.Enabled = true;
                    txtShipmentDeclaredValueCurrency.Enabled = true;
                    break;
            }
        }
    }
}
