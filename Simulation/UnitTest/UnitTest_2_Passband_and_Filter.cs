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
    class UnitTest_Passband {
    
        [Test]
        public void Test_Constructors() {
            // Verify static property.
            Assert.That(String.Compare(Passband.FolderName, @"C:\Dev\Spectral\Data\Passband\", true)==0);

            // Verify Invalid on non-existent Passband.
            {   string passbandName = @"This is a non-existent passband name";
                Passband pbBad = new Passband(passbandName);
                Assert.That(pbBad.IsValid == false);
            } // end code block.
            
            // Valid on valid Passband file.
            {   string passbandName = @"V Bessell 2005";
                Passband pbV = new Passband(passbandName);
                Assert.That(pbV.IsValid);

                Assert.That(String.Compare(pbV.PassbandName, passbandName, true)==0);
                Assert.That(pbV.StdSpectrum.IsValid);
                StandardizedSpectrum ss = pbV.StdSpectrum;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0);                                   // at 300 nm.
                Assert.That(ss.Y(3000).IsWithinTolerance(0.334, 0.002));   // at 600 nm.
                Assert.That(ss.Y(4200).IsWithinTolerance(0.002, 0.001));   // at 720 nm.
                Assert.That(ss.Y(4500)==0);                                // at 750 nm.
                Assert.That(ss.Y(10000)==0);                               // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY==1);
            } // end code block.

            // Valid on valid Passband file (test on a second file).
            {   string passbandName = @"B Bessell 2005";
                Passband pbB = new Passband(passbandName);
                Assert.That(pbB.IsValid);
                Assert.That(String.Compare(pbB.PassbandName, passbandName, true)==0);
                Assert.That(pbB.StdSpectrum.IsValid);
                StandardizedSpectrum ss = pbB.StdSpectrum;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0);                                   // at 300 nm.
                Assert.That(ss.Y(1000).IsWithinTolerance(0.947, 0.002));   // at 400 nm.
                Assert.That(ss.Y(1200)==1);                                // at 420 nm.
                Assert.That(ss.Y(2500).IsWithinTolerance(0.008, 0.001));   // at 550 nm.
                Assert.That(ss.Y(2600)==0);                                // at 560 nm.
                Assert.That(ss.Y(10000)==0);                               // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY==1);
            } // end code block.

        } // .Test_Constructors().

        [Test]
        public void Test_ActOn(){
            // A bit of setup.
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
            Assert.That(pflux0.IsValid);
            string passbandName = @"V Bessell 2005";
            Passband pbV = new Passband(passbandName);
            Assert.That(pbV.IsValid);
            Assert.That(pbV.StdSpectrum.IsValid);

            // Test .ActOn(PFluxPerArea).
            {   PFluxPerArea pflux = pbV.ActOn(pflux0);
                Assert.That(pflux.IsValid);
                Assert.That(String.Compare(pflux.PFluxName, pfluxName, true)==0);
                
                StandardizedSpectrum ss = pflux.StdSpectrum;
                Assert.That(ss.IsValid);
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0); // at 300 nm.
                Assert.That(ss.Y(1700)==0); // at 470 nm
                Assert.That(ss.Y(1800).IsWithinTolerance(0.033*1.27e8,0.003E8)); // at 480 nm.
                Assert.That(ss.Y(2300).IsWithinTolerance(1*1.059e8, 0.01E8)); // at 530 nm.
                Assert.That(ss.Y(4300).IsWithinTolerance(0.001*5.55E7, 0.0004E7)); // at 730 nm.
                Assert.That(ss.Y(4400)==0); // at 740 nm.
                Assert.That(ss.Y(10000)==0); // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(1.094E8, 0.05E8));
            } // end code block.

            // Then test .Acton(PFluxApertured) [never actually needed?].
            {   double apertureArea = 0.9;
                PFluxApertured pfluxAp = new PFluxApertured(pflux0, apertureArea);
                PFluxApertured pfluxApV = pbV.ActOn(pfluxAp);
                Assert.That(pfluxApV.IsValid);
                Assert.That(String.Compare(pfluxApV.PFluxName, pfluxName, true)==0);

                StandardizedSpectrum ssApV = pfluxApV.StdSpectrum;
                Assert.That(ssApV.IsValid);
                Assert.That(ssApV.NPoints == 10001);
                Assert.That(ssApV.Nm_low == 300.0);
                Assert.That(ssApV.Nm_high == 1300.0);
                Assert.That(ssApV.Y(0)==0);    // at 300 nm.
                Assert.That(ssApV.Y(1700)==0); // at 470 nm
                Assert.That(ssApV.Y(1800)
                    .IsWithinTolerance(apertureArea*0.033*1.27e8, 0.003E8));  // at 480 nm.
                Assert.That(ssApV.Y(2300)
                    .IsWithinTolerance(apertureArea*1*1.059e8, 0.01E8));      // at 530 nm.
                Assert.That(ssApV.Y(4300)
                    .IsWithinTolerance(apertureArea*0.001*5.55E7, 0.0004E7)); // at 730 nm.
                Assert.That(ssApV.Y(4400)==0); // at 740 nm.
                Assert.That(ssApV.Y(10000)==0); // at 1300 nm.
                Assert.That(ssApV.MinY==0);
                Assert.That(ssApV.MaxY.IsWithinTolerance(apertureArea*1.094E8, 0.05E8));
            } // end code block.
       } // .Test_ActOn().

        // [Test] // uncomment this line to verify read-in zero-mag fluxes from passband files.
        public void Verify_PFluxAtMagZero() {
            string pfluxName = @"Vega (calspec)";
            PFluxPerArea pfluxVega = new PFluxPerArea(pfluxName);
            Assert.That(pfluxVega.IsValid);

            string pbName;
            Passband pb;
            double pCalc, pReadIn;
            
            pbName = "B Bessell 1990";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "B Bessell 2005";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "I Bessell 1990";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "I Bessell 2005";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "R Bessell 1990";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "R Bessell 2005";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "U Bessell 1990";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "U Bessell 2005";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "V Bessell 1990";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

            pbName = "V Bessell 2005";
            pb = new Passband(pbName);
            pCalc = pfluxVega.FluxInPassband(pb);
            pReadIn = pb.PFluxAtZeroMag;
            Assert.That(pCalc.IsWithinTolerance(pReadIn, pCalc/1000000.0));

        } // .Verify_PFluxAtMagZero()

    } // class UnitTest_Passband.

    [TestFixture]
    class UnitTest_Filter {

        [Test]
        public void Test_Constructors() {
            // Verify static property.
            Assert.That(String.Compare(Filter.FolderName, @"C:\Dev\Spectral\Data\Filter\", true)==0);
            
            // ***** Test usual constructor from file, default (=1) relativeThickness and fractionCoverage.

            // Verify Invalid on non-existent Filter.
            {   string filterName = @"This is a non-existent filter name";
                Filter filterX = new Filter(filterName);
                Assert.That(filterX.IsValid == false);
            } // end code block.
            
            // Test a valid case.
            {   string filterName = @"21 Orange";
                Filter f = new Filter(filterName);
                Assert.That(f.IsValid);
                Assert.That(String.Compare(f.FilterName, filterName, true)==0);
                Assert.That(f.RelativeThickness == 1);
                Assert.That(f.FractionCoverage == 1);
                Assert.That(f.StdSpectrumEffective.IsValid);
                StandardizedSpectrum ss = f.StdSpectrumEffective;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0);                                   // at 300 nm.
                Assert.That(ss.Y(3000).IsWithinTolerance(0.900, 0.002));   // at 600 nm.
                Assert.That(ss.Y(4200).IsWithinTolerance(0.9202, 0.001));  // at 720 nm.
                Assert.That(ss.Y(4500).IsWithinTolerance(0.9165, 0.001));  // at 750 nm.
                Assert.That(ss.Y(10000).IsWithinTolerance(0.8796, 0.001)); // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(0.9247,0.002));
            } // end code block.

            // A second valid case (probably not needed).
            {   string filterName = @"Green Baader";
                Filter f = new Filter(filterName);
                Assert.That(f.IsValid);
                Assert.That(String.Compare(f.FilterName, filterName, true)==0);
                Assert.That(f.RelativeThickness == 1);
                Assert.That(f.FractionCoverage == 1);
                StandardizedSpectrum ss = f.StdSpectrumEffective;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0);                                   // at 300 nm.
                Assert.That(ss.Y(2000).IsWithinTolerance(0.960, 0.002));   // at 500 nm.
                Assert.That(ss.Y(2750).IsWithinTolerance(0.669, 0.002));   // at 575 nm.
                Assert.That(ss.Y(2850).IsWithinTolerance(0.025, 0.003));   // at 585 nm.
                Assert.That(ss.Y(9000)==0);                                // at 1200 nm.
                Assert.That(ss.Y(10000)==0);                               // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(0.988,0.001));
            } // end code block.

            // ***** Test Constructor from file, Relative Thickness and Fraction Coverage Both not = 1.

            // Verify invalid if Relative Thickness < 0.
            {   string filterName = @"Green Baader";
                double RelativeThickness = -1; // invalid value
                double FractionCoverage = 0.451;
                Filter filterX = new Filter(filterName, RelativeThickness, FractionCoverage);
                Assert.That(filterX.IsValid == false);
            } // end code block.

            // Verify invalid if FractionCoverage < 0.
            {   string filterName = @"Green Baader";
                double RelativeThickness = 1;
                double FractionCoverage = -0.451;
                Filter filterX = new Filter(filterName, RelativeThickness, FractionCoverage);
                Assert.That(filterX.IsValid == false);
            } // end code block.

            // Verify invalid if FractionCoverage > 1.
            {   string filterName = @"Green Baader";
                double RelativeThickness = 1;
                double FractionCoverage = 1.451;
                Filter filterX = new Filter(filterName, RelativeThickness, FractionCoverage);
                Assert.That(filterX.IsValid == false);
            } // end code block.

            // Valid case.
            {   string filterName = @"Green Baader";
                double RelativeThickness = 2.322;
                double FractionCoverage = 0.451;
                Filter f = new Filter(filterName, RelativeThickness, FractionCoverage);
                Assert.That(f.IsValid);
                Assert.That(f.RelativeThickness == RelativeThickness);
                Assert.That(f.FractionCoverage == FractionCoverage);
                Assert.That(String.Compare(f.FilterName, filterName, true)==0);
                Assert.That(f.StdSpectrumEffective.IsValid);
                StandardizedSpectrum ss = f.StdSpectrumEffective;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==(1-FractionCoverage));                // at 300 nm.
                Assert.That(ss.Y(1000)==(1-FractionCoverage));             // at 400 nm.
                Assert.That(ss.Y(2000).IsWithinTolerance(0.9592, 0.002));  // at 500 nm.
                Assert.That(ss.Y(2750).IsWithinTolerance(0.7263, 0.002));  // at 575 nm.
                Assert.That(ss.Y(2850).IsWithinTolerance(0.5491, 0.003));  // at 585 nm.
                Assert.That(ss.Y(9000)==(1-FractionCoverage));             // at 1200 nm.
                Assert.That(ss.Y(10000)==(1-FractionCoverage));            // at 1300 nm.
                Assert.That(ss.MinY==(1-FractionCoverage));
                Assert.That(ss.MaxY.IsWithinTolerance(0.9875, 0.0005));
            } // end code block.

            // ***** Test constructor with transmissionFraction only (neutral density).

            // Verify invalid for given transmissionFromSS < 0.
            {   Filter fNegative = new Filter(-0.2);
                Assert.That(fNegative.IsValid == false);
            } // end code block.

            // Verify invalid for given transmissionFromSS > 1.
            {   Filter fGreaterThanOne = new Filter(1.2);
                Assert.That(fGreaterThanOne.IsValid == false);
            } // end code block.

            // Test normal, valid case.
            {   double transmission = 0.44;
                Filter fT = new Filter(transmission);
                Assert.That(fT.IsValid);
                Assert.That(fT.RelativeThickness == 1);
                Assert.That(fT.FractionCoverage == 1);
                string startString = "Transmission=";
                Assert.That(fT.FilterName.StartsWith(startString));
                Assert.That(Convert.ToDouble(fT.FilterName.Substring(startString.Length)).IsWithinTolerance(transmission,0.001));
                Assert.That(fT.StdSpectrumEffective.IsValid);
                StandardizedSpectrum ss = fT.StdSpectrumEffective;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==transmission);     // at 300 nm.
                Assert.That(ss.Y(1000)==transmission);  // at 400 nm.
                Assert.That(ss.Y(2000)==transmission);  // at 500 nm.
                Assert.That(ss.Y(2750)==transmission);  // at 575 nm.
                Assert.That(ss.Y(2850)==transmission);  // at 585 nm.
                Assert.That(ss.Y(9000)==transmission);  // at 1200 nm.
                Assert.That(ss.Y(10000)==transmission); // at 1300 nm.
                Assert.That(ss.MinY==transmission);
            } // end code block.
            
            // ***** Test default constructor (which means "100% transmissionFromSS", = "no filter").
            {   Filter filterEmpty = new Filter();
                Assert.That(filterEmpty.IsValid); // this constructor always gives valid filter.
                Assert.That(filterEmpty.StdSpectrumEffective.IsValid);
                double[] Transmissions = filterEmpty.StdSpectrumInput.Y_Vector;
                Assert.That(Transmissions.Max() == 1.0);
                Assert.That(Transmissions.Min() == 1.0);
            } // end code block.

        } // .Test_Constructors().

        [Test]
        public void Test_ActOn() {
            // Test .ActOn(PFluxPerArea).
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
                Assert.That(pflux0.IsValid);
                string filterName = @"Green Baader";
                Filter fGreen = new Filter(filterName);
                Assert.That(fGreen.IsValid);
                PFluxPerArea pfluxGreen = fGreen.ActOn(pflux0);
                Assert.That(pfluxGreen.IsValid);
                Assert.That(String.Compare(pfluxGreen.PFluxName, pflux0.PFluxName)==0);
                Assert.That(pfluxGreen.StdSpectrum.IsValid);
                StandardizedSpectrum ss = pfluxGreen.StdSpectrum;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0); // at 300 nm.
                Assert.That(ss.Y(1000)==0); // at 400 nm
                Assert.That(ss.Y(2000).IsWithinTolerance(0.96*1.19E8,   0.96*1.19E8*0.01));   // at 500 nm.
                Assert.That(ss.Y(2500).IsWithinTolerance(0.958*9.88E7,  0.958*9.88E7*0.01));  // at 550 nm.
                Assert.That(ss.Y(2850).IsWithinTolerance(0.025*8.75E7, 0.025*8.75E7*0.01));  // at 585 nm.
                Assert.That(ss.Y(7200).IsWithinTolerance(0.002*3.14E7, 0.002*3.14E7*0.02));   // at 1020 nm.
                Assert.That(ss.Y(9000)==0); // at 1200 nm.
                Assert.That(ss.Y(10000)==0); // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(1.17E8, 0.02E8));
            } // end code block.

            // Test .ActOn(PFluxApertured).
            {   string pfluxName = @"Vega (calspec)";
                PFluxPerArea pflux0 = new PFluxPerArea(pfluxName);
                Assert.That(pflux0.IsValid);
                double apArea = 0.125;
                PFluxApertured pfluxAp = new PFluxApertured(pflux0, apArea);
                Assert.That(pfluxAp.IsValid);
                string filterName = @"Green Baader";
                Filter fGreen = new Filter(filterName);
                Assert.That(fGreen.IsValid); PFluxApertured pfluxGreen = fGreen.ActOn(pfluxAp);
                Assert.That(pfluxGreen.IsValid);
                Assert.That(String.Compare(pfluxGreen.PFluxName, pflux0.PFluxName)==0);

                StandardizedSpectrum ss = pfluxGreen.StdSpectrum;
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high == 1300.0);
                Assert.That(ss.Y(0)==0); // at 300 nm.
                Assert.That(ss.Y(1000)==0); // at 400 nm
                Assert.That(ss.Y(2000)
                    .IsWithinTolerance(0.96*1.19E8*apArea, 0.96*1.19E8*0.01*apArea));   // at 500 nm.
                Assert.That(ss.Y(2500)
                    .IsWithinTolerance(0.958*9.88E7*apArea, 0.958*9.88E7*0.01*apArea));  // at 550 nm.
                Assert.That(ss.Y(2850)
                    .IsWithinTolerance(0.025*8.75E7*apArea, 0.025*8.75E7*0.01*apArea));  // at 585 nm.
                Assert.That(ss.Y(7200)
                    .IsWithinTolerance(0.002*3.14E7*apArea, 0.002*3.14E7*0.02*apArea));   // at 1020 nm.
                Assert.That(ss.Y(9000)==0); // at 1200 nm.
                Assert.That(ss.Y(10000)==0); // at 1300 nm.
                Assert.That(ss.MinY==0);
                Assert.That(ss.MaxY.IsWithinTolerance(1.17E8*apArea, 0.02E8*apArea));
            } // end code block.
        } // .Test_ActOn().

    } // class UnitTest_Passband.

} // namespace.
