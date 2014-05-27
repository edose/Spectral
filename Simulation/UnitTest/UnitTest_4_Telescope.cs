using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    
    [TestFixture]
    class UnitTest_Telescope {

        [Test]
        public void Test_Constructor() {
            // Verify static property.
            Assert.That(String.Compare(Telescope.FolderName, @"C:\Dev\Spectral\Data\Telescope\") == 0);

            // Invalid if bad telescope name.
            {   string telescopeNameBad = @"this is not a telescope name";
                Telescope telBad = new Telescope(telescopeNameBad);
                Assert.That(telBad.IsValid == false);
            } // end code block.

            // Valid case, telescope from file.
            {   string telescopeName = @"Meade 14 UHTC";
                Telescope tel = new Telescope(telescopeName);
                Assert.That(tel.IsValid);
                Assert.That(String.Compare(tel.TelescopeName, telescopeName, true)==0);
                Assert.That(tel.ApertureDiameter.IsWithinTolerance(0.3556,0.0001));
                Assert.That(tel.PercentDiameterObstructed.IsWithinTolerance(35, 0.1));
                double expectedApertureArea = Math.PI*((0.3556/2.0).Squared()) 
                    * (1.0-(tel.PercentDiameterObstructed/100.0).Squared());
                Assert.That(tel.ApertureArea.IsWithinTolerance(expectedApertureArea, 0.001));
                StandardizedSpectrum ss = tel.StdSpectrum;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0);                                   // at 300 nm.
                Assert.That(ss.Y(3000).IsWithinTolerance(0.900, 0.001));   // at 600 nm.
                Assert.That(ss.Y(4200).IsWithinTolerance(0.848, 0.001));   // at 720 nm.
                Assert.That(ss.Y(10000)==0.5);                             // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(0.91, 0.005));        
            } // end code block
        } // .Test_Constructor().

        [Test]
        public void Test_ActOn() {
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pflux = new PFluxPerArea(pfluxName);
            Assert.That(pflux.IsValid); 
            string telescopeName = @"Meade 14 UHTC";
            Telescope tel = new Telescope(telescopeName);
            Assert.That(tel.IsValid);

            PFluxApertured pfluxAfterScope = tel.ActOn(pflux);
            Assert.That(pfluxAfterScope.IsValid);
            Assert.That(String.Compare(pfluxAfterScope.PFluxName, pfluxName, true)==0);
            Assert.That(pfluxAfterScope.ApertureArea == tel.ApertureArea);

            StandardizedSpectrum ssAfterScope = pfluxAfterScope.StdSpectrum;
            Assert.That(ssAfterScope.NPoints == 10001);
            Assert.That(ssAfterScope.Nm_low == 300.0);
            Assert.That(ssAfterScope.Nm_high == 1300.0);
            for (int i=0; i<ssAfterScope.NPoints; i+=500) {
                double expectedFlux = tel.ApertureArea * pflux.StdSpectrum.Y(i) * tel.StdSpectrum.Y(i);
                Assert.That(expectedFlux.IsWithinTolerance(ssAfterScope.Y(i),0.001*expectedFlux));
            } // for iAngle.
            Assert.That(ssAfterScope.MaxY.IsWithinTolerance(1.093E7,0.01E7));
            Assert.That(ssAfterScope.MinY.IsWithinTolerance(0,1000));
        } // .Test_ActOn().

    } // class UnitTest_Telescope.
} // namespace.
