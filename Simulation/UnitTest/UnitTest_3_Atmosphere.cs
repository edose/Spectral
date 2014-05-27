using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {

    [TestFixture]
    class UnitTest_Atm_AirPath {

        [Test]
        public void Test_Atm_Constructor() {

            // Verify static properties.
            Assert.That(String.Compare(Atmosphere.SiteFolder, @"C:\Dev\Spectral\Data\Site\")==0);
            Assert.That(String.Compare(Atmosphere.SmartsFolder, @"C:\Astro\SMARTS\SMARTS_295_PC\")==0);
            Assert.That(String.Compare(Atmosphere.SmartsInputFilename, @"smarts295.inp.txt")==0);
            Assert.That(String.Compare(Atmosphere.SmartsExeFilename, @"smarts295bat.exe")==0);
            Assert.That(String.Compare(Atmosphere.SmartsOutputFilename, @"smarts295.ext.txt")==0);

            // Verify Exo-atmosphere case.
            Atmosphere atmExo = new Atmosphere();
            Assert.That(atmExo.IsValid == true);
            Assert.That(atmExo.IsExoAtmospheric == true);
            Assert.That(String.Compare(atmExo.SiteName,"[No Atmosphere]")==0);
            // the site information is generally meaningless, but values needed.
            Assert.That(atmExo.Latitude == 0);
            Assert.That(atmExo.Elevation == 0);
            Assert.That(atmExo.Height == 0);
            Assert.That(atmExo.PollutionModelCode == 0);
            Assert.That(String.Compare(atmExo.AerosolModelCode,"")==0);
            Assert.That(atmExo.AirTemperatureAtSite == 0);
            Assert.That(atmExo.RelativeHumidity == 0);
            Assert.That(String.Compare(atmExo.SeasonCode,"")==0);
            Assert.That(atmExo.MeanDailyTemperatureAtSite == 0);
            Assert.That(atmExo.PrecipWaterVaporAboveSite == 0);
            Assert.That(atmExo.OzoneAbundance == 0);
            Assert.That(atmExo.Visibility == 0);

            // Non-existent file must give invalid object.
            {   string atmNameBad = @"this name does not exist";
                Atmosphere atmBad = new Atmosphere(atmNameBad);
                Assert.That(atmBad.IsValid == false);
            } // end test block.

            // Normal valid case and simple properties.
            {   string siteName = "$TestSiteOnly";
                Atmosphere atm = new Atmosphere(siteName);
                Assert.That(atm.IsValid);
                Assert.That(String.Compare(atm.SiteName, siteName)==0);
                Assert.That(atm.Latitude.IsWithinTolerance(39.0, 0.001));
                Assert.That(atm.Elevation.IsWithinTolerance(400.0, 0.01));
                Assert.That(atm.Height.IsWithinTolerance(1.0, 0.001));
                Assert.That(atm.PollutionModelCode == 1);
                Assert.That(String.Compare(atm.AerosolModelCode, "S&F_RURAL")==0);
                Assert.That(atm.AirTemperatureAtSite == 5.0);
                Assert.That(atm.RelativeHumidity == 60.0);
                Assert.That(String.Compare(atm.SeasonCode, "WINTER")==0);
                Assert.That(atm.MeanDailyTemperatureAtSite == 12.0);
                Assert.That(atm.PrecipWaterVaporAboveSite.IsWithinTolerance(3.0, 0.000001));
                Assert.That(atm.OzoneAbundance.IsWithinTolerance(0.3341, 0.000001));
                Assert.That(atm.Visibility == 101.0);
                Assert.That(atm.AllInputsAreValid);
            } // end test block.

            // Test read-only properties AirPath.StdSpectrum and AirPath.Y_Vector.
            {   string siteName = "$TestSiteOnly";
                Atmosphere atm = new Atmosphere(siteName);  
                Assert.That(atm.IsValid);
                AirPath ap = atm.MakeAirPathAtZenithAngle(60.0);
                Assert.That(ap.IsValid);
                double[] transmissionFromYVector = ap.Y_Vector;
                StandardizedSpectrum ss = ap.StdSpectrum;
                double[] transmissionFromSS = ss.Y_Vector;
                Assert.That((transmissionFromYVector.Length == 10001) 
                    && (transmissionFromSS.Length == transmissionFromYVector.Length));
                Assert.That(transmissionFromYVector[2500].IsWithinTolerance(0.7372,0.0001));
                for (int i=0; i<transmissionFromSS.Length; i+=100) {
                    Assert.That(transmissionFromYVector[i]==transmissionFromSS[i]);
                }
            } // end test block.

            // Test settable Properties (twice each, to ensure asserts do not pass accidentally).
            {   string siteNameGood = "$TestSiteOnly";
                Atmosphere atm = new Atmosphere(siteNameGood);
                Assert.That(atm.IsValid);

                atm.AirTemperatureAtSite = -2.0;
                Assert.That(atm.AirTemperatureAtSite == -2.0);
                atm.AirTemperatureAtSite = -3.0;
                Assert.That(atm.AirTemperatureAtSite == -3.0);

                atm.RelativeHumidity = 88.0;
                Assert.That(atm.RelativeHumidity == 88.0);
                atm.RelativeHumidity = 87.0;
                Assert.That(atm.RelativeHumidity == 87.0);

                atm.SeasonCode = "XXX";
                Assert.That(String.Compare(atm.SeasonCode, "XXX") == 0);
                atm.SeasonCode = "SUMMER";
                Assert.That(String.Compare(atm.SeasonCode, "SUMMER") == 0);

                atm.MeanDailyTemperatureAtSite = 14.0; 
                Assert.That(atm.MeanDailyTemperatureAtSite == 14.0);
                atm.MeanDailyTemperatureAtSite = 13.0;
                Assert.That(atm.MeanDailyTemperatureAtSite == 13.0);

                atm.PrecipWaterVaporAboveSite = 1.1;
                Assert.That(atm.PrecipWaterVaporAboveSite == 1.1);
                atm.PrecipWaterVaporAboveSite = 1.5;
                Assert.That(atm.PrecipWaterVaporAboveSite == 1.5);

                atm.OzoneAbundance = 0.3;
                Assert.That(atm.OzoneAbundance == 0.3);
                atm.OzoneAbundance = 0.375;
                Assert.That(atm.OzoneAbundance == 0.375);

                atm.Visibility = 57.1;
                Assert.That(atm.Visibility == 57.1);
                atm.Visibility = 57.5;
                Assert.That(atm.Visibility == 57.5);
            } // end test block.

            // Verify that property .AllInputsAreValid is false for invalid inputs.
            {
                Atmosphere atm = new Atmosphere("$TestSiteOnly");
                Assert.That(atm.IsValid);

                atm.AirTemperatureAtSite = 100.0;
                Assert.That(atm.AirTemperatureAtSite == 100.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.AirTemperatureAtSite = -150.0;
                Assert.That(atm.AirTemperatureAtSite == -150.0);
                Assert.That(atm.AllInputsAreValid == false); 
                atm.AirTemperatureAtSite = -5.0;
                Assert.That(atm.AirTemperatureAtSite == -5.0);
                Assert.That(atm.AllInputsAreValid);

                atm.RelativeHumidity = 110.0;
                Assert.That(atm.RelativeHumidity == 110.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.RelativeHumidity = -5.0;
                Assert.That(atm.RelativeHumidity == -5.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.RelativeHumidity = 50.0;
                Assert.That(atm.RelativeHumidity == 50.0);
                Assert.That(atm.AllInputsAreValid);

                atm.SeasonCode = "BADBAD"; // not a season code.
                Assert.That(String.Compare(atm.SeasonCode, "BADBAD") == 0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.SeasonCode = "summer"; // in lower case, not good.
                Assert.That(String.Compare(atm.SeasonCode, "summer") == 0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.SeasonCode = "WINTER";
                Assert.That(String.Compare(atm.SeasonCode, "WINTER") == 0);
                Assert.That(atm.AllInputsAreValid);
                atm.SeasonCode = "SUMMER";
                Assert.That(String.Compare(atm.SeasonCode, "SUMMER") == 0);
                Assert.That(atm.AllInputsAreValid);

                atm.MeanDailyTemperatureAtSite = 80.0;
                Assert.That(atm.MeanDailyTemperatureAtSite == 80.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.MeanDailyTemperatureAtSite = -140.0;
                Assert.That(atm.MeanDailyTemperatureAtSite == -140.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.MeanDailyTemperatureAtSite = 10.0;
                Assert.That(atm.MeanDailyTemperatureAtSite == 10.0);
                Assert.That(atm.AllInputsAreValid);

                atm.PrecipWaterVaporAboveSite = -0.125;
                Assert.That(atm.PrecipWaterVaporAboveSite == -0.125);
                Assert.That(atm.AllInputsAreValid == false);
                atm.PrecipWaterVaporAboveSite = 13.0;
                Assert.That(atm.PrecipWaterVaporAboveSite == 13.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.PrecipWaterVaporAboveSite = 3.5;
                Assert.That(atm.PrecipWaterVaporAboveSite == 3.5);
                Assert.That(atm.AllInputsAreValid);

                atm.OzoneAbundance = -0.125;
                Assert.That(atm.OzoneAbundance == -0.125);
                Assert.That(atm.AllInputsAreValid == false);
                atm.OzoneAbundance = 1.125;
                Assert.That(atm.OzoneAbundance == 1.125);
                Assert.That(atm.AllInputsAreValid == false);
                atm.OzoneAbundance = 0.375;
                Assert.That(atm.OzoneAbundance == 0.375);
                Assert.That(atm.AllInputsAreValid);

                atm.Visibility = 0.5;
                Assert.That(atm.Visibility == 0.5);
                Assert.That(atm.AllInputsAreValid == false);
                atm.Visibility = 800.0;
                Assert.That(atm.Visibility == 800.0);
                Assert.That(atm.AllInputsAreValid == false);
                atm.Visibility = 55.0;
                Assert.That(atm.Visibility == 55.0);
                Assert.That(atm.AllInputsAreValid);
            } // end test block.

        } // .Test_Constructors_Properties().

        [Test]
        public void Test_AirPathFactoryMethods() {

            // ***** Test Atmosphere.MakeAirPathAtZenithAngle(), a Factory method for AirPath.
            
            // ***** Verify Exo-atmospheric case.
            // Verify Atmosphere object.
            Atmosphere exoAtm = new Atmosphere();
            Assert.That(exoAtm.IsValid == true);
            Assert.That(exoAtm.IsExoAtmospheric == true);
            // Verify AirPath object.
            {
                double zenithAngle = 44.0;
                AirPath exoAP = exoAtm.MakeAirPathAtZenithAngle(zenithAngle);
                Assert.That(exoAP.IsValid);
                Assert.That(exoAP.StdSpectrum.IsValid);
                Assert.That(String.Compare(exoAP.SiteName, "[No Atmosphere]")==0);
                Assert.That(exoAP.StdSpectrum.MaxY == 1.0);
                Assert.That(exoAP.StdSpectrum.MinY == 1.0);
            } // end code block.
            // Verify AirPath vector.
            {   double[] zenithAngles = new double[] { 70.0, 50.0};
                AirPath[] exoAPs = exoAtm.MakeAirPathVector(zenithAngles);
                Assert.That(exoAPs.Length == zenithAngles.Length);
                foreach (AirPath AP in exoAPs) {
                    Assert.That(AP.IsValid);
                    Assert.That(String.Compare(AP.SiteName, "[No Atmosphere]")==0);
                    Assert.That(AP.StdSpectrum.IsValid);
                    Assert.That(AP.StdSpectrum.MaxY == 1.0);
                    Assert.That(AP.StdSpectrum.MinY == 1.0);
                } // foreach.
            } // end code block.


            // ***** For all other, normal atmospheric cases:
            // Verify that SMARTS .ext output file is read properly.
            Atmosphere atmGood = new Atmosphere("$TestSiteOnly");
            Assert.That(atmGood.IsValid);

            // Test .MakeAirPathAtZenithAngle() for AirPath validity (vs zenith angle).
            {   double[] zenithAngles = new double[] { 90.1, 90.0, 89.9, 30.0, 0.1, 0.0, -0.1, -89.9, -90.0, -90.1 };
                foreach (double za in zenithAngles) {
                    AirPath a = atmGood.MakeAirPathAtZenithAngle(za);
                    Assert.That(a.ZenithAngleDegrees == za); // AirPath obj should store raw angle, not abs value.
                    bool expectedValidity = ((a.ZenithAngleDegrees <= 90.0) && (a.ZenithAngleDegrees >= -90.0));
                    Assert.That(a.IsValid == expectedValidity);
                } // foreach.
            } // end code block.
            
            // Test .MakeAirPathAtZenithAngle() for correct contents of AirPath objects.
            {   AirPath ap_0deg = atmGood.MakeAirPathAtZenithAngle(0.0);
                Assert.That(ap_0deg.IsValid);
                Assert.That(ap_0deg.ZenithAngleDegrees == 0.0);
                Assert.That(String.Compare(atmGood.SiteName, ap_0deg.SiteName)==0);
                Assert.That(atmGood.Latitude == ap_0deg.Latitude);
                Assert.That(atmGood.Elevation == ap_0deg.Elevation);
                Assert.That(atmGood.Height == ap_0deg.Height);
                Assert.That(atmGood.PollutionModelCode == ap_0deg.PollutionModelCode);
                Assert.That(String.Compare(atmGood.AerosolModelCode,ap_0deg.AerosolModelCode)==0);
                Assert.That(atmGood.AirTemperatureAtSite == ap_0deg.AirTemperatureAtSite);
                Assert.That(atmGood.RelativeHumidity == ap_0deg.RelativeHumidity);
                Assert.That(String.Compare(atmGood.SeasonCode, ap_0deg.SeasonCode)==0);
                Assert.That(atmGood.MeanDailyTemperatureAtSite == ap_0deg.MeanDailyTemperatureAtSite);
                Assert.That(atmGood.PrecipWaterVaporAboveSite == ap_0deg.PrecipWaterVaporAboveSite);
                Assert.That(atmGood.OzoneAbundance == ap_0deg.OzoneAbundance);
                Assert.That(atmGood.Visibility == ap_0deg.Visibility);            
                Assert.That(ap_0deg.Y(500).IsWithinTolerance(0.5003, 0.001));
                Assert.That(ap_0deg.Y(2560).IsWithinTolerance(0.8625, 0.001));
                Assert.That(ap_0deg.Y(8000).IsWithinTolerance(0.7409, 0.001));
                StandardizedSpectrum ss = ap_0deg.StdSpectrum;
                Assert.That(ap_0deg.Y_Vector.Sum()==ss.Sum);
                Assert.That(ap_0deg.Y(500)==ss.Y(500));
            } // end code block.

            // Test on a second zenith angle.
            {   AirPath ap_60deg = atmGood.MakeAirPathAtZenithAngle(60.0);
                Assert.That(ap_60deg.IsValid);
                Assert.That(ap_60deg.ZenithAngleDegrees == 60.0);
                Assert.That(ap_60deg.Y(500).IsWithinTolerance(0.2514, 0.001));
                Assert.That(ap_60deg.Y(2560).IsWithinTolerance(0.7445, 0.001));
                Assert.That(ap_60deg.Y(8000).IsWithinTolerance(0.6077, 0.001));
            } // end method test block.

            // Test .MakeAirPathVector(), iAngle.e., 
            //    verify AirPath objects match those from .MakeAirPathAtZenithAngle().
            //    Test *only* vector-making here; factory method already tested just above.
            {   Atmosphere atm = new Atmosphere("$TestSiteOnly");
                Assert.That(atm.IsValid);
                double[] zenithAngles = new double[]{ 90.1, 30.0, 0.1};
                AirPath[] airPaths = atm.MakeAirPathVector(zenithAngles);
                Assert.That(zenithAngles.Length == airPaths.Length);
                for (int iAngle=0; iAngle<zenithAngles.Length; iAngle++) {
                    AirPath apFromMAPV = airPaths[iAngle];
                    AirPath apFromFactoryMethod = atm.MakeAirPathAtZenithAngle(zenithAngles[iAngle]);
                    Assert.That(apFromFactoryMethod.IsValid == airPaths[iAngle].IsValid);
                    if (airPaths[iAngle].IsValid) {
                        Assert.That(apFromMAPV.SiteName.CompareTo(apFromFactoryMethod.SiteName)==0);
                        Assert.That(apFromMAPV.StdSpectrum.Sum == apFromFactoryMethod.StdSpectrum.Sum);
                    } // if.
                } // for iAngle.
            } // end code block.
        } // .Test_AirPathFactoryMethods().

        [Test]
        public void Test_AirPath_ActOn() {
            // Test AirPath.ActOn(PFluxPerArea).
            // Testing only pass-through (by comparison to pflux explicitly calculated from StdSpectrum).
            //     since TransmissionSpectrumAtZenithAngle() is tested separately against raw data.
            // Also, this exercises SMARTS several times to ensure it behaves properly.
            PFluxPerArea pflux = new PFluxPerArea(@"Vega (calspec)");
            Assert.That(pflux.IsValid);
            Atmosphere atm = new Atmosphere("$TestSiteOnly");
            Assert.That(atm.IsValid);
            AirPath ap;
            int iAt556 = (int)Math.Floor(0.5+(556.0-pflux.StdSpectrum.Nm_low)/pflux.StdSpectrum.Increment);
            double p0At556 = pflux.StdSpectrum.Y(iAt556);
            for (double degrees=0; degrees<=85.0; degrees+=10.0) {
                ap = atm.MakeAirPathAtZenithAngle(degrees);
                PFluxPerArea pfluxAfterAirPath = ap.ActOn(pflux); // <-- ActOn().
                Assert.That(pfluxAfterAirPath.IsValid);
                double pAtDegreesAt556 = pfluxAfterAirPath.StdSpectrum.Y(iAt556);
                double TAtDegreesAt556 = ap.Y(iAt556);
                Assert.That((pAtDegreesAt556).IsWithinTolerance(p0At556*TAtDegreesAt556, 
                    Math.Max(1.0,0.001*pAtDegreesAt556)));
            } // for degrees.
        } // .Test_AirPath_ActOn().

    } // class UnitTest_Atm_AirPath.

} // namespace.
