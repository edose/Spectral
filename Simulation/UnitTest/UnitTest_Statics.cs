using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    [TestFixture]
    class UnitTest_Statics {

        [Test]
        public void Test_StaticMethods() {

#region Test Spectral.MakeCountsVector()
            // Test static method Spectral.MakeCountsVector().
            // Testing *only* vector-making here; 
            //    factory method (& constructor) are tested within class definitions.

            // first, a bit of setup.
            Atmosphere atm = new Atmosphere("$TestSiteOnly");
            Assert.That(atm.IsValid);
            PFluxPerArea star = new PFluxPerArea(@"Vega (calspec)");
            Filter noFilter = new Filter(1.0);
            Telescope scope = new Telescope(@"Meade 14 UHTC");
            Filter dirtyFilter = new Filter(0.8);
            Detector det = new Detector(@"SBIG 6303E");

            // Verify entire vector is non-positive if any zenith angle has abs value > 90 degrees.
            {   double[] zenithAngles = new double[] { 90.1, 30.0, 0.1 };
                AirPath[] airPaths = atm.MakeAirPathVector(zenithAngles);
                double[] counts = Spectral.MakeCountsVector(star, 
                    noFilter, airPaths, noFilter, scope, dirtyFilter, det, 0.10);
                Assert.That(counts.Length == airPaths.Length);
                for(int iAngle=0; iAngle<zenithAngles.Length; iAngle++) {
                    //Console.WriteLine(iAngle.ToString() + ": " + counts[iAngle].ToString());
                    Assert.That(counts[iAngle] <= 0.0);
                } // for iAngle.
            } // end code block.

            // Verify vector values when zenith Angles are all valid.
            {   double[] zenithAngles = new double[] { 89.0, 60.0, 30.0, 0.1, 0.0 };
                AirPath[] airPaths = atm.MakeAirPathVector(zenithAngles);
                double[] counts = Spectral.MakeCountsVector(star,
                    noFilter, airPaths, noFilter, scope, dirtyFilter, det, 0.10);
                Assert.That(counts.Length == airPaths.Length);
                double[] expectedCounts = new double[] { 5170000, 
                    82500000, 95800000, 98600000, 98600000 };
                for(int iAngle=0; iAngle<zenithAngles.Length; iAngle++) {
                    Console.WriteLine(iAngle.ToString() + ": " + counts[iAngle].ToString());
                    Assert.That(counts[iAngle].IsWithinTolerance(expectedCounts[iAngle], 1e5));
                } // for iAngle.
            } // end code block
#endregion Test Spectral.MakeCountsVector()

            //// Test static method Spectral.GetCountsExoAtmosphere().
            //// use same setup as above for Spectral.MakeCountsVector()

            //{   double counts = Spectral.GetCountsExoAtmosphere(star, noFilter, noFilter, 
            //        scope, noFilter, det, 0.1);
            //    double expectedCounts = 150000000.0;
            //    Assert.That(counts.IsWithinTolerance(expectedCounts, 0.001*expectedCounts));
            //} // end code block.

        } // .Test_StaticMethods().



    } // class UnitTest_Statics.
} // namespace.
