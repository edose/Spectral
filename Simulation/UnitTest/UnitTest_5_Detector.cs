using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    
    [TestFixture]
    public class UnitTest_Detector {
    
        [Test]
        public void Test_Constructor() {
            // Verify static property.
            Assert.That(String.Compare(Detector.FolderName, @"C:\Dev\Spectral\Data\Detector\") == 0);

            // Invalid on non-existent Detector.
            {   string detectorNameBad = @"This is a non-existent passband name";
                Detector detBad = new Detector(detectorNameBad);
                Assert.That(detBad.IsValid == false);
            } // end code block.
            
            // Test constructor: Detector from file.
            {   string detectorName = @"SBIG 6303E";
                Detector det = new Detector(detectorName);
                Assert.That(det.IsValid);
                Assert.That(String.Compare(det.DetectorName, detectorName, true)==0);

                StandardizedSpectrum ss = det.QE;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0) == 0);       // at 300 nm.
                Assert.That(ss.Y(3000) == 0.64); // at 600 nm.
                Assert.That(ss.Y(4200) == 0.51); // at 720 nm.
                Assert.That(ss.Y(7000) == 0.05); // at 1000 nm.
                Assert.That(ss.Y(10000) == 0);   // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(0.67, 0.01));
                Assert.That(det.PeakQE == ss.MaxY);
            } // end code block.
        } // .Test_Constructor().
        
        [Test]
        public void Test_Outputs () {
            
            // Test .CountRateFromPFlux().
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux = new PFluxPerArea(pfluxName);
                Assert.That(pflux.IsValid);
                string detectorName = @"SBIG 6303E";
                Detector det = new Detector(detectorName);
                Assert.That(det.IsValid);
                double apertureArea = 0.000001;
                PFluxApertured pfluxAp = new PFluxApertured(pflux, apertureArea);
                double countRate = det.CountRateFromPFlux(pfluxAp);
                Assert.That(countRate.IsWithinTolerance(20700e6*apertureArea, 1000e6*apertureArea));
            } // end code block.

            // Test .InstrumentalMagnitudeFromPFlux().
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux = new PFluxPerArea(pfluxName);
                Assert.That(pflux.IsValid);
                string detectorName = @"SBIG 6303E";
                Detector det = new Detector(detectorName);
                Assert.That(det.IsValid);
                double apertureArea = 0.000001;
                PFluxApertured pfluxAp = new PFluxApertured(pflux, apertureArea);
                double countRate = det.CountRateFromPFlux(pfluxAp);
                double instrumMag = det.InstrumentalMagnitudeFromPFlux(pfluxAp);
                // Check equivalence in both directions.
                Assert.That(countRate.IsWithinTolerance(Math.Pow(10.0,-instrumMag/2.5),
                    Math.Max(1.0,0.001*countRate)));
                Assert.That(instrumMag.IsWithinTolerance(-2.5*Math.Log10(countRate),0.001));
            } // end code block.
        } // .Test_InstrMag().

    } // class UnitTest_Detector.

} // namespace.
