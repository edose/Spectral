﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    [TestFixture]
    class _RunSpectral {
                                        // *****-----------------------------------------*****
                                        // ***** For 013 series of expts, March 3, 2014. *****
                                        // *****-----------------------------------------*****
        [Test]
        public void ForR() {
            // User's working method to use Spectral for its intended purposes.
            // Operate via NUnit, check RunSpectral/ForR, click Run.

            //***** Construct stars; choose one option.
            //Star[] stars = MakeStars_BlackBody(); // may also enter star temps, magnitude.
            Star[] stars = MakeStars_PASP(); // from PASP paper (Pickles).

            //***** Setup the atmosphere & apparatus.
            Filter reflectingBody  = new Filter();       // = absent.
            Atmosphere atm = new Atmosphere("Farpoint");           // = exo-atmosphere.
            //double[] zenithAngles  = new double[] { 0 };
            int nAirmasses = 81;
            double[] airmasses = new double[nAirmasses];
            for (int i=0; i<nAirmasses; i++){
                airmasses[i] = 1.2 + 0.01 * (double)i; // i.e., over range 1.2-2 by 0.01.
            }
            Filter frontFilter     = new Filter();       // = absent.
            Telescope scope        = new Telescope(@"Meade 14 UHTC");
            //Filter scopeFilter     = new Filter();                  //
            Filter scopeFilter     = new Filter(@"V Bessell Optec", 1.0, 1);  //
            //Filter scopeFilter     = new Filter(@"Green Baader", 1, 1);   //
            //Filter scopeFilter     = new Filter(@"Sloan G");        //
            //Filter scopeFilter     = new Filter(@"Asahi 540 100", 2, 1);  //
            //Filter scopeFilter     = new Filter(@"Asahi 540 060 "); //
            Detector det           = new Detector(@"SBIG STL 1001E"); //
            //Detector det           = new Detector(@"SBIG ST 8XE");  //
            double expSeconds      = 1.0;

            //***** The default usage for Inst Mag perturbed by anything.
            //***** Write data to console (probably for use in NUnit, then pick up from clipboard).
            Write_AirmassSeries(stars, reflectingBody, atm, airmasses, frontFilter, scope,
                scopeFilter, det, expSeconds);

        } // .ForR().

        // ***********************************************************************************

        // Factory method for Star[] vector, Pure Black Body option.
        private Star[] MakeStars_BlackBody (double[] starTempsK, Passband pbNormalization, double mag) {
            Star[] stars = new Star[starTempsK.Length];
            for(int iStar=0; iStar<stars.Length; iStar++) {
                stars[iStar] = new Star(
                    new PFluxPerArea(starTempsK[iStar], pbNormalization, mag),
                    String.Copy(starTempsK[iStar].ToString("####0K")));
            } // for iStar.
            AssertStarsValidity(stars);
            return stars;
        } // end MakeStars_BlackBody().

        private Star[] MakeStars_BlackBody (double[] starTempsK) {
            return MakeStars_BlackBody(starTempsK, new Passband("V Bessell 1990"), 10.0);
        } // end MakeStars_BlackBody() overload.
        private Star[] MakeStars_BlackBody () {
            double[] defaultStarTempsK = new double[] { 53000, 25000, 17200, 13300, 
                10700, 9300, 8100, 7200, 6500, 5900, 5450, 5040, 4700, 4400, 4100, 3870, 3630, 
                3440, 3260, 3100, 2950, 2820};
            return MakeStars_BlackBody(defaultStarTempsK);
        } // end MakeStars_BlackBody() overload.

        // Factory method for Star[] vector, PASP star set option.
        private Star[] MakeStars_PASP () {
            // Use lists, because we start without knowing the number of PASP stars.
            // ~ Clumsy because from historical code; better would be List<Star> -> ToArray().
            List<PFluxPerArea> starList = new List<PFluxPerArea>();
            List<string> ID_List = new List<string>();

            string ukfilenamesFile = @"C:/Dev/Spectral/Docs_RawData/PFlux/PASP/ukfilenames.txt"; // all 131.
            // string ukfilenamesFile = 
            //    @"C:/Dev/Spectral/Docs_RawData/PFlux/PASP/ukfilenames_NoRed.txt"; // 107 w/V-I < 1.6.
            using(StreamReader sr = File.OpenText(ukfilenamesFile)) {
                string line = sr.ReadLine();
                while(line != null) {
                    string thisStarName = String.Concat("PASP ", line.Trim()).Trim();
                    PFluxPerArea thisStar = new PFluxPerArea(thisStarName);
                    if(thisStar.IsValid) {
                        starList.Add(thisStar);
                        ID_List.Add(thisStarName);
                    } // if.
                    else { Console.WriteLine(thisStarName + ": thisStar.IsValid=FALSE"); }
                    line = sr.ReadLine();
                } // while.
            } // using.                
            // Now convert lists to returnable array of stars.
            Star[] stars = new Star[starList.Count];
            for (int iStar=0; iStar<stars.Length; iStar++) {
                stars[iStar] = new Star(starList[iStar], ID_List[iStar]);            
            } // for iStar.
            AssertStarsValidity(stars);
            return stars;
        } // end MakeStars_PASP().

        private void AssertStarsValidity(Star[] stars) {
            foreach(Star star in stars) {
                Assert.That(star.PFlux.IsValid);
                Assert.That((star.ID.Trim()).Length >= 1);
            }
        } // end AssertStarsValidity().

        private void Write_AirmassSeries (Star[] stars, Filter reflectingBody, 
            Atmosphere atm, double[] airmasses, 
            Filter frontFilter, Telescope scope, Filter scopeFilter, Detector det, double expSeconds) {
            Assert.That(airmasses.Length >= 1);
            double[] zenithAngles = new double[airmasses.Length];
            for (int i=0; i<airmasses.Length; i++){
                Assert.That(airmasses[i] >= 1);
                zenithAngles[i] = (180.0/Math.PI) * Math.Acos(1/airmasses[i]);
            }
            Write_ZenithAngleSeries(stars, reflectingBody, atm, zenithAngles,
                frontFilter, scope, scopeFilter, det, expSeconds);
        }

        private void Write_ZenithAngleSeries(Star[] stars, Filter reflectingBody, 
            Atmosphere atm, double[] zenithAngles, 
            Filter frontFilter, Telescope scope, Filter scopeFilter, Detector det, double expSeconds) {
            Assert.That(stars.Length >= 1);
            Assert.That(zenithAngles.Length >= 1);
            Assert.That(reflectingBody.IsValid);
            Assert.That(atm.IsValid);
            Assert.That(frontFilter.IsValid);
            Assert.That(scope.IsValid);
            Assert.That(scopeFilter.IsValid);
            Assert.That(det.IsValid);
            Assert.That(expSeconds > 0.0);
            AirPath[] airPaths = atm.MakeAirPathVector(zenithAngles);

            Passband pbB = new Passband("B Bessell 2005"); // Color-index passband.
            Passband pbV = new Passband("V Bessell 2005"); // Color-index passband.
            Passband pbR = new Passband("R Bessell 2005"); // Color-index passband.
            Passband pbI = new Passband("I Bessell 2005"); // Color-index passband.
            Passband pbB90 = new Passband("B Bessell 1990"); // Color-index passband.
            Passband pbV90 = new Passband("V Bessell 1990"); // Color-index passband.
            Passband pbR90 = new Passband("R Bessell 1990"); // Color-index passband.
            Passband pbI90 = new Passband("I Bessell 1990"); // Color-index passband.
            Passband pbTarget = pbV90;

            // Write conditions of this simulation at top of output.
            Console.WriteLine("Series of " 
                + stars.Length.ToString() + " stars through "
                + zenithAngles.Length.ToString() + " zenith angles "
                + "at '" + atm.SiteName.Trim() + "'");
            if(reflectingBody.RelativeThickness == 1.0 && reflectingBody.FractionCoverage == 1.0)
                Console.WriteLine("   Reflecting body: " + reflectingBody.FilterName + " (1,1)");
            else
                Console.WriteLine("   Reflecting body: " + reflectingBody.FilterName
                    + " (" + reflectingBody.RelativeThickness.ToString("0.000000")
                    + ", " + reflectingBody.FractionCoverage.ToString("0.000000") + ")");
            if(frontFilter.RelativeThickness == 1.0 && frontFilter.FractionCoverage == 1.0)
                Console.WriteLine("   Front filter:    " + frontFilter.FilterName + " (1,1)");
            else
                Console.WriteLine("   Front filter:    " + frontFilter.FilterName
                    + " (" + frontFilter.RelativeThickness.ToString("0.000000")
                    + ", " + frontFilter.FractionCoverage.ToString("0.000000") + ")");
            Console.WriteLine("   Telescope:       " + scope.TelescopeName);
            if(scopeFilter.RelativeThickness == 1.0 && scopeFilter.FractionCoverage == 1.0)
                Console.WriteLine("   Scope filter:    " + scopeFilter.FilterName + " (1,1)");
            else
                Console.WriteLine("   Scope filter:    " + scopeFilter.FilterName
                    + " (" + scopeFilter.RelativeThickness.ToString("0.000000")
                    + ", " + scopeFilter.FractionCoverage.ToString("0.000000") + ")");
            Console.WriteLine("   Detector:        " + det.DetectorName);
            // Write header line for table (data frame).
            Console.WriteLine("\nstarID      \tstarMagV90\t  CI.BV90 \t  CI.VR90 \t  CI.VI90 " + 
                "\timageID \t   ZA   \tSecantZA\t   InstMag");
            for(int iStar=0; iStar< stars.Length; iStar++) {
                PFluxPerArea starPFlux = stars[iStar].PFlux;
                string starID = stars[iStar].ID;
                double starMagV90 = starPFlux.Magnitude(pbTarget);
                double CI_BV90 = starPFlux.ColorIndex(pbB90, pbV90);
                double CI_VR90 = starPFlux.ColorIndex(pbV90, pbR90);
                double CI_VI90 = starPFlux.ColorIndex(pbV90, pbI90);                
                double[] counts = Spectral.MakeCountsVector(stars[iStar].PFlux,
                    reflectingBody, airPaths, frontFilter, scope, scopeFilter, det, expSeconds);
                //Console.WriteLine(counts[0].ToString("##0.00000")); // debug only.
                
                // Write detector response, at each angle, to NUnit's "Text Output" window.
                for(int iAngle=0; iAngle<zenithAngles.Length; iAngle++) {
                    double instrumentMagnitude = -2.5 * Math.Log10(counts[iAngle]/expSeconds);
                    string imageID = String.Concat("image", iAngle.ToString("000"));
                    double secantZenithAngle = 1.0/Math.Cos(Math.PI*zenithAngles[iAngle]/180.0);
                    Console.WriteLine(
                        stars[iStar].ID.PadRight(12) + "\t"
                        + starMagV90.ToString("#0.000000") + "\t"
                        + CI_BV90.ToString("#0.000000") + "\t"
                        + CI_VR90.ToString("#0.000000") + "\t"
                        + CI_VI90.ToString("#0.000000") + "\t"
                        + imageID + "\t"
                        + zenithAngles[iAngle].ToString("##0.000").PadLeft(7) + "\t"
                        + secantZenithAngle.ToString("#0.0000000").PadLeft(8) + "\t"
                        + instrumentMagnitude.ToString("##0.000000").PadLeft(11)); // one data frame row.
                } // for iAngle.
            } // for iStar.
        } // end Write_ZenithAngleSeries().

        private class Star {
            // Container class (nested within class RunSpectral,
            //    one object to hold one star's photon spectrum (PFlux) and string ID, 
            //    and typically in an array of these objects.
            // E. Dose   Auburn, KS   January 12, 2014.
            private PFluxPerArea pflux;
            private string id_string;
            // CONSTRUCTOR.
            public Star (PFluxPerArea pflux_in, string id_string_in) {
                this.pflux = pflux_in;
                this.id_string = String.Copy(id_string_in);
            } // constructor.
            // PROPERTIES.
            public PFluxPerArea PFlux { get { return pflux;     } }
            public string       ID    { get { return id_string; } }
        } // end class Star.

    } // class RunSpectral.
} // namespace.
