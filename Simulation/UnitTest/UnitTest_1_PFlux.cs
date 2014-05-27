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
    class UnitTest_PFluxPerArea {

        [Test]
        public void Test_Constructors() {
            // Test static property.
            Assert.That(String.Compare(PFluxPerArea.FolderName,
                @"C:\Dev\Spectral\Data\PFlux\", true)==0);
            
            // ***** Test constructor: PFluxPerArea from file (the usual constructor).

            // Verify failure on bad file name.
            {   string pfluxFailureName = @"FailureFileName"; // no file with this name exists.
                PFluxPerArea pfluxFailure = new PFluxPerArea(pfluxFailureName); // constructor under test.
                Assert.That(pfluxFailure.IsValid == false);
                Assert.That(pfluxFailure.StdSpectrum.IsValid == false);
            } // end code block.

            // Verify normal, valid case.
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName); // constructor under test.
                Assert.That(pflux0.IsValid);
                Assert.That(String.Compare(pflux0.PFluxName, pfluxName, true)==0);
                StandardizedSpectrum ss = pflux0.StdSpectrum;
                Assert.That(ss.IsValid);
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0).IsWithinTolerance(5.55e7, 0.05E7)); // at 300 nm.
                Assert.That(ss.Y(1000).IsWithinTolerance(1.67E8, 0.02E8)); // at 400 nm.
                Assert.That(ss.Y(10000).IsWithinTolerance(1.716E7, 0.01E7)); // at 1300 nm.
                Assert.That(ss.MinY.IsWithinTolerance(1.413e7, 0.01E7));
                Assert.That(ss.MaxY.IsWithinTolerance(1.74e8, 0.01E8));
            } // end code block.

            // Test constructor: PFluxPerArea from old PFluxPerArea with new spectrum inserted.
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName); // constructor under test.
                Assert.That(pflux0.IsValid);
                StandardizedSpectrum ss = pflux0.StdSpectrum;
                Assert.That(ss.IsValid);
                double factor = 7.0;
                PFluxPerArea pflux2 = new PFluxPerArea(pflux0, ss.MultiplyBy(factor));
                pflux0 = null;
                // pflux2 should be same as pfluxPerArea except factor * StandardizedSpectrum.
                Assert.That(pflux2.IsValid);
                Assert.That(String.Compare(pflux2.PFluxName, pfluxName, true)==0);
                StandardizedSpectrum ss2 = pflux2.StdSpectrum;
                Assert.That(ss2.IsValid);
                Assert.That(ss2.NPoints == 10001);
                Assert.That(ss2.Nm_low == 300.0);
                Assert.That(ss2.Nm_high == 1300.0);
                Assert.That(ss2.Y(0).IsWithinTolerance(factor*5.55e7, factor*0.05E7)); // at 300 nm.
                Assert.That(ss2.Y(1000).IsWithinTolerance(factor*1.67E8, factor*0.02E8)); // at 400 nm.
                Assert.That(ss2.Y(10000).IsWithinTolerance(factor*1.716E7, factor*0.01E7)); // at 1300 nm.
                Assert.That(ss2.MinY.IsWithinTolerance(factor*1.413e7, factor*0.01E7));
                Assert.That(ss2.MaxY.IsWithinTolerance(factor*1.74e8, factor*0.01E8));
            } // end code block.

            // Test constructor: PFluxPerArea from black body starNominal, given temp, passband, mag.
            {   double tempK = 9602.0;
                Passband pb = new Passband(@"V Bessell 2005");
                double magV = 10.0;
                PFluxPerArea pflux0 = new PFluxPerArea(tempK, pb, magV);
                Assert.That(pflux0.IsValid);
                double expectedFluxV = pb.PFluxAtZeroMag * Math.Pow(10.0, -magV/2.5);
                Assert.That(pflux0.Magnitude(pb).IsWithinTolerance(magV, 0.001));
                Assert.That(pflux0.Magnitude(new Passband(@"B Bessell 2005")).IsWithinTolerance(10.2,0.1));
                Assert.That(pflux0.Magnitude(new Passband(@"R Bessell 2005")).IsWithinTolerance(9.9, 0.1));
                Assert.That(pflux0.Magnitude(new Passband(@"I Bessell 2005")).IsWithinTolerance(9.8, 0.1));
                PFluxPerArea pG = new PFluxPerArea(4000.0, pb, magV); // starNominal is much redder thus cooler.
                Assert.That(pG.IsValid);
                Assert.That(pG.Magnitude(pb).IsWithinTolerance(magV, 0.001));
                Assert.That(pG.Magnitude(new Passband(@"R Bessell 2005")).IsWithinTolerance(9.3,0.1));
            } // end code block.

        } // Test_Constructors().

        [Test]
        public void Test_Through() {
            // here we test only each pass-through to .ActOn() (which is within Filter, Telescope, etc).
            // as each .ActOn() itself is unit tested in its own class.
            
            // first, a bit of setup.
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
            Assert.That(pflux0.IsValid);
            double transmission1 = 0.477;
            double transmission2 = 0.544;
            Filter testFilter1 = new Filter(transmission1);
            Filter testFilter2 = new Filter(transmission2);
            Passband pb1 = new Passband(@"B Bessell 2005");
            Atmosphere atm = new Atmosphere("Farpoint");
            AirPath ap = atm.MakeAirPathAtZenithAngle(45.0);
            Telescope tel1 = new Telescope(@"Meade 14 UHTC");

            // Flux through Filter must not differ for .Through() vs .ActOn().
            {   PFluxPerArea pfluxThrough = pflux0.Through(testFilter1);
                PFluxPerArea pfluxActOn   = testFilter1.ActOn(pflux0);
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    Assert.That(pfluxThrough.StdSpectrum.Y(i) == pfluxActOn.StdSpectrum.Y(i));
                } // for iAngle.
            } // end code block.

            // Flux through Passband must not differ for .Through() vs .ActOn().
            {   PFluxPerArea pfluxThrough = pflux0.Through(pb1);
                PFluxPerArea pfluxActOn   = pb1.ActOn(pflux0);
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    Assert.That(pfluxThrough.StdSpectrum.Y(i) == pfluxActOn.StdSpectrum.Y(i));
                } // for iAngle.
            } // end code block.

            // Flux must differ for 2 different filters.
            {   PFluxPerArea pfluxThrough = pflux0.Through(testFilter1);
                PFluxPerArea pfluxActOn   = testFilter2.ActOn(pflux0);
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    //Console.WriteLine(iAngle.ToString());
                    Assert.That(pfluxThrough.StdSpectrum.Y(i) != pfluxActOn.StdSpectrum.Y(i));
                } // for iAngle.
            } // end code block.

            // Flux through AirPath must not differ for .Through() vs .ActOn().
            {   PFluxPerArea pfluxThrough = pflux0.Through(ap);
                PFluxPerArea pfluxActOn   = ap.ActOn(pflux0);
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    Assert.That(pfluxThrough.StdSpectrum.Y(i) == pfluxActOn.StdSpectrum.Y(i));
                } // for iAngle.
            } // end code block.

            // Flux through Telescope (becomes apertured) must not differ for .Through() vs .ActOn().
            {   PFluxApertured pfluxThrough = pflux0.Through(tel1);
                PFluxApertured pfluxActOn = tel1.ActOn(pflux0);
                Assert.That(pfluxThrough.TotalPhotonFlux
                    .IsWithinTolerance(pfluxActOn.TotalPhotonFlux, 1E-6*pfluxThrough.TotalPhotonFlux));
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/97) {
                    Assert.That(pfluxThrough.StdSpectrum.Y(i)
                        .IsWithinTolerance(pfluxActOn.StdSpectrum.Y(i), 
                        1E-6*pfluxThrough.StdSpectrum.Y(i)));
                } // for iAngle.
            } // end code block.
            
        } // Test_Through().

        [Test]
        public void Test_Methods() {
            // Test in order: most elementary to dependent.

            //***** Test .TotalPhotonFlux property.
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
                Assert.That(pflux0.IsValid);
                double transmission1 = 0.477;
                Assert.That(pflux0.TotalPhotonFlux
                    .IsWithinTolerance(pflux0.StdSpectrum.Y_Vector.Sum()*pflux0.StdSpectrum.Increment,
                    100000000.0));
                Assert.That(pflux0.Through(new Filter(transmission1)).TotalPhotonFlux
                    .IsWithinTolerance(
                    pflux0.StdSpectrum.Y_Vector.Sum()* pflux0.StdSpectrum.Increment*transmission1,
                    100000000.0));
            } // end code block.

            //***** Test .MultiplyBy().
            {   string pfluxVega = @"Vega (calspec)";
                PFluxPerArea p1 = new PFluxPerArea(pfluxVega);
                PFluxPerArea p3 = p1.MultiplyBy(3.0);
                for(int i=50; i<p1.StdSpectrum.NPoints; i+=75) {
                    Assert.That(p3.StdSpectrum.Y(i) == p1.StdSpectrum.Y(i)*3.0); // test a few points.
                } // for iAngle.
                Assert.That(p3.TotalPhotonFlux
                    .IsWithinTolerance(3.0*p1.TotalPhotonFlux, 1e-6*p1.TotalPhotonFlux)); // test integral.
            } // end code block.
            
            //***** Test .FluxInPassband().
            {   string pfluxVega = @"Vega (calspec)";
                PFluxPerArea pRaw = new PFluxPerArea(pfluxVega);
                Assert.That(pRaw.IsValid);
                Passband pbU = new Passband("U Bessell 2005");
                Assert.That(pbU.IsValid);
                Assert.That(pRaw.FluxInPassband(pbU).IsWithinTolerance(4539985423.84509, 10.0));
                Passband pbR = new Passband("R Bessell 2005");
                Assert.That(pbR.IsValid);
                Assert.That(pRaw.FluxInPassband(pbR).IsWithinTolerance(10625775071.3368, 10.0));
            } // end code block.

            //***** Test .Magnitude().
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName); Assert.That(pflux0.IsValid);
                Passband pbU = new Passband("U Bessell 2005"); Assert.That(pbU.IsValid);
                Assert.That(pflux0.Magnitude(pbU).IsWithinTolerance(0.0, 1e-9));
                Passband pbR = new Passband("R Bessell 2005"); Assert.That(pbR.IsValid);
                Assert.That(pflux0.Magnitude(pbR).IsWithinTolerance(0.0, 1e-9));
                PFluxPerArea pflux15 = new PFluxPerArea(pflux0, pflux0.StdSpectrum.MultiplyBy(1E-6));
                Assert.That(pflux15.Magnitude(pbU).IsWithinTolerance(15.0, 1e-9));
                Assert.That(pflux15.Magnitude(pbR).IsWithinTolerance(15.0, 1e-9));
            } // end code block.

            //***** Test .ColorIndex()
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName); Assert.That(pflux0.IsValid);
                Passband pbV = new Passband("V Bessell 2005"); Assert.That(pbV.IsValid); 
                Passband pbR = new Passband("R Bessell 2005"); Assert.That(pbR.IsValid);
                Assert.That(pflux0.ColorIndex(pbV, pbR).IsWithinTolerance(0.0, 1e-6));
                PFluxPerArea pfluxFiltered = pflux0.Through(pbV);
                Assert.That(pfluxFiltered.ColorIndex(pbV,pbR)
                    .IsWithinTolerance((pfluxFiltered.Magnitude(pbV)-pfluxFiltered.Magnitude(pbR)),1e-6));
            } // end code block.

            //***** Test .ChangeMagnitudeBy().
            {   string pfluxVega = @"Vega (calspec)";
                PFluxPerArea p1 = new PFluxPerArea(pfluxVega);
                PFluxPerArea p1000 = p1.ChangeMagnitudeBy(-7.5); // p1 but 1000 X brighter.
                double factorExpected = Math.Pow(10.0, -(-7.5)/2.5); // = 1000.
                for(int i=50; i<p1.StdSpectrum.NPoints; i+=92) {  // test some points.
                    Assert.That(p1000.StdSpectrum.Y(i)
                        .IsWithinTolerance(p1.StdSpectrum.Y(i)*1000.0, p1.StdSpectrum.MaxY*0.0001));
                } // for iAngle.
                Assert.That(p1000.TotalPhotonFlux                // test integral.
                    .IsWithinTolerance(factorExpected*p1.TotalPhotonFlux, 1e-6*p1.TotalPhotonFlux));
            } // end code block.

            //***** Test .NormalizeToBandbassMag().
            {   PFluxPerArea pRaw = new PFluxPerArea(@"Vega (calspec)");
                Assert.That(pRaw.IsValid);
                Passband pbR = new Passband("R Bessell 2005");
                Assert.That(pbR.IsValid);
                double newMag = 5.0;
                PFluxPerArea pNew = pRaw.NormalizeToPassbandMag(pbR, newMag);
                double ratioExpected = Math.Pow(10.0, -newMag/2.5);
                Assert.That(pNew.TotalPhotonFlux                  // test integrated photon flux.
                    .IsWithinTolerance(ratioExpected*pRaw.TotalPhotonFlux,
                    0.000001*pNew.TotalPhotonFlux));
                for(int i=0; i<pNew.StdSpectrum.NPoints; i+=76) { // test at some points.
                    if(pNew.StdSpectrum.Y(i) > 0.0)
                        Assert.That(pNew.StdSpectrum.Y(i)
                            .IsWithinTolerance(ratioExpected*pRaw.StdSpectrum.Y(i), 
                            0.000001*pNew.StdSpectrum.Y(i)));
                } // for iAngle.
            } // end code block.


            //***** Test .ChangeBBTemp(tempBefore, tempAfter, passband).
            {   double tempK = 9602.0;
                Passband pbV = new Passband(@"V Bessell 2005");
                Passband pbR = new Passband(@"R Bessell 2005");
                double magV = 10.0;
                PFluxPerArea pflux9602K = new PFluxPerArea(tempK, pbV, magV);
                Assert.That(pflux9602K.IsValid);
                PFluxPerArea pflux6000K_viaChangeBBTemp = pflux9602K.ChangeBBTemp(9602.0, 6000.0, pbV, magV);
                PFluxPerArea pflux6000K_viaConstructor  = new PFluxPerArea(6000.0, pbV, magV);
                Assert.That(pflux6000K_viaChangeBBTemp.IsValid);
                Assert.That(pflux6000K_viaConstructor.IsValid);
                Assert.That(pflux6000K_viaConstructor.TotalPhotonFlux
                    .IsWithinTolerance(pflux6000K_viaChangeBBTemp.TotalPhotonFlux, 
                    1e-6*pflux6000K_viaChangeBBTemp.TotalPhotonFlux));
                Assert.That(pflux6000K_viaConstructor.Magnitude(pbV)
                    .IsWithinTolerance(pflux6000K_viaChangeBBTemp.Magnitude(pbV), 0.0001));
                Assert.That(pflux6000K_viaConstructor.Magnitude(pbR)
                    .IsWithinTolerance(pflux6000K_viaChangeBBTemp.Magnitude(pbR), 0.0001));
                Assert.That(pflux6000K_viaConstructor.ColorIndex(pbV,pbR)
                    .IsWithinTolerance(pflux6000K_viaChangeBBTemp.ColorIndex(pbV, pbR), 0.0001));
            } // end code block.

        } // Test_Methods().

    } // class [UnitTest_PFluxPerArea].

    [TestFixture]
    class UnitTest_PFluxApertured {

        [Test]
        public void Test_Constructors() {
            // Test static property.
            Assert.That(String.Compare(PFluxApertured.FolderName, @"C:\Dev\Spectral\Data\PFlux\", true)==0);

            // Test constructor: PFluxApertured from action of apertureArea on PFluxPerArea.
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
                Assert.That(pflux0.IsValid);
                double apertureArea = 0.125;
                PFluxApertured pfluxAp = new PFluxApertured(pflux0, apertureArea);
                Assert.That(pfluxAp.IsValid);
                Assert.That(String.Compare(pfluxAp.PFluxName, pflux0.PFluxName)==0);
                Assert.That(pfluxAp.ApertureArea == apertureArea);
                Assert.That(pfluxAp.StdSpectrum.IsValid);
                for(int index=0; index<pflux0.StdSpectrum.NPoints; index+=500) {
                    Assert.That(pfluxAp.StdSpectrum.Y(index)==pflux0.StdSpectrum.Y(index)*apertureArea);
                }
            } // end code block.

            // Fail on negative apertureArea.
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
                Assert.That(pflux0.IsValid);
                double apertureArea = -0.125;
                PFluxApertured pfluxNegativeAperture = new PFluxApertured(pflux0, apertureArea);
                Assert.That(pfluxNegativeAperture.IsValid == false);
                Assert.That(String.Compare(pfluxNegativeAperture.PFluxName, pflux0.PFluxName)==0);
                Assert.That(pfluxNegativeAperture.ApertureArea == apertureArea);
                Assert.That(pfluxNegativeAperture.StdSpectrum.IsValid);
            } // end code block.

            // Test constructor from old object except using new flux StandardizedSpectrum.
            {   string pfluxName = @"Vega (calspec)";
                PFluxApertured pfluxOld = new PFluxApertured(new PFluxPerArea(pfluxName), 0.5);
                Assert.That(pfluxOld.IsValid);
                double factor = 0.544;
                StandardizedSpectrum ssNew = pfluxOld.StdSpectrum.Clone().MultiplyBy(factor);
                PFluxApertured pfluxNew = new PFluxApertured(pfluxOld, ssNew);
                Assert.That(pfluxNew.IsValid);
                Assert.That(String.Compare(pfluxNew.PFluxName, pfluxOld.PFluxName)==0);
                Assert.That(pfluxNew.ApertureArea == pfluxOld.ApertureArea);
                Assert.That(pfluxNew.StdSpectrum.IsValid);
                Assert.That((pfluxNew.TotalPhotonFlux/pfluxOld.TotalPhotonFlux).IsWithinTolerance(factor, 0.001));
            } // end code block.
        } // .Test_Constructors().

        [Test]
        public void Test_Through() {
            // here we verify only that results from .Through() match those from Filter.ActOn().
            // Filter.ActOn() itself is tested in class Filter.
            
            // first, a bit of setup.
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
            PFluxApertured pfluxAp = new PFluxApertured(pflux0, 0.5);
            Assert.That(pfluxAp.IsValid);
            double transmission1 = 0.477;
            double transmission2 = 0.544;
            Filter testFilter1 = new Filter(transmission1);
            Filter testFilter2 = new Filter(transmission2);

            // .Through() and filter.ActOn() must give different results on using different filters.
            {   PFluxApertured pfluxThrough = pfluxAp.Through(testFilter1);
                PFluxApertured pfluxActOn   = testFilter2.ActOn(pfluxAp);
                bool isEquivalent = true; //default.
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    //Console.WriteLine(iAngle.ToString());
                    if(pfluxThrough.StdSpectrum.Y(i) != pfluxActOn.StdSpectrum.Y(i))
                        isEquivalent = false;
                }
                Assert.That(isEquivalent == false);
            } // end code block.

            // .Through() and filter.ActOn() must give same results on using the same filter.
            {   PFluxApertured pfluxThrough = pfluxAp.Through(testFilter1);
                PFluxApertured pfluxActOn   = testFilter1.ActOn(pfluxAp);
                bool isEquivalent = true; //default.
                for(int i=0; i<pflux0.StdSpectrum.NPoints; i+=pflux0.StdSpectrum.NPoints/100) {
                    if(pfluxThrough.StdSpectrum.Y(i) != pfluxActOn.StdSpectrum.Y(i))
                        isEquivalent = false;
                } // for iAngle.
                Assert.That(isEquivalent == true);
            } // end code block.
        } // Test_Through().

        [Test]
        public void Test_CountRate() {
            // here we test only the pass-through to Detector.CountRateFromPFlux(). 
            // Detector.CountRateFromPFlux() itself is tested in class Detector.
            
            // first, a bit of setup.
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
            PFluxApertured pfluxAp = new PFluxApertured(pflux0, 0.5);
            Assert.That(pfluxAp.IsValid);
            Detector testDetector1 = new Detector("SBIG 6303E");
            Detector testDetector2 = new Detector("SBIG STL 1301E");
            Assert.That(testDetector1.IsValid);
            Assert.That(testDetector2.IsValid);

            // .CountRate() and detector.CountRateFromPFlux() results must differ for different detectors.
            {   double countRateFromPFluxAp = pfluxAp.CountRate(testDetector1);
                double countRateFromDetector = testDetector2.CountRateFromPFlux(pfluxAp);
                Assert.That(countRateFromPFluxAp != countRateFromDetector);
            } // end code block.

            // .CountRate() and detector.CountRateFromPFlux() results must be same for same detector.
            {   double countRateFromPFluxAp = pfluxAp.CountRate(testDetector1);
                double countRateFromDetector = testDetector1.CountRateFromPFlux(pfluxAp);
                Assert.That(countRateFromPFluxAp == countRateFromDetector);
            } // end code block.
        } // Test_CountRate().

        [Test]
        public void Test_InstrumentalMagnitude() {
            // here we test only the pass-through to Detector.InstrumentalMagnitudeFromPFlux(). 
            // Detector.InstrumentalMagnitudeFromPFlux() itself is tested in class Detector.
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pfluxPerArea = new PFluxPerArea(pfluxName);
            PFluxApertured pfluxAp2 = new PFluxApertured(pfluxPerArea, 2);
            Assert.That(pfluxAp2.IsValid);
            Detector testDetector1 = new Detector("SBIG 6303E");
            Assert.That(testDetector1.IsValid);

            // Verify that Instrumental Magnitude is passed through properly.
            double countRateFromPFluxAp2 = pfluxAp2.InstrumentalMagnitude(testDetector1);
            double countRateFromDetector = testDetector1.InstrumentalMagnitudeFromPFlux(pfluxAp2);
            Assert.That(countRateFromPFluxAp2 == countRateFromDetector);

            // Verify Instrumental Magnitude changes for different flux.
            PFluxApertured pfluxAp3 = new PFluxApertured(pfluxPerArea, 3);
            Assert.That(pfluxAp3.IsValid);
            double countRateFromPFluxAp3 = pfluxAp3.InstrumentalMagnitude(testDetector1);
            Assert.That(countRateFromPFluxAp3 != countRateFromPFluxAp2);
        } // Test_InstrumentalMagnitude().

    } // class UnitTest_PFluxApertured.
} // namespace.
