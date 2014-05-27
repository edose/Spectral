using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    //[TestFixtureSetUp]     public void testFixtureSetup() { }
    //[TestFixtureTearDown]  public void testFixtureTearDown() { }
    //[SetUp]                public void setup() { }
    //[TearDown]             public void tearDown() { }

    [TestFixture]
    class UnitTest_StandardizedSpectrum {
       
        [Test]
        public void Test_Constructors () {

            // *****  Test CONSTRUCTOR that assumes evenly spaced points.

            // Invalid if fewer than 4 y-values.
            {   double[] y_raw = new double[] { 0, 1, 2 };
                StandardizedSpectrum ss = new StandardizedSpectrum(250, 1600, y_raw);
                Assert.That(ss.IsValid == false);
            } // end code block.
            
            // Valid if 4 y-values.
            {   double[] y_raw = new double[] { 0, 1, 2, 5 };
                StandardizedSpectrum ss = new StandardizedSpectrum(250, 1600, y_raw);
                Assert.That(ss.IsValid);
            } // end code block.            
            
            // Invalid if nm_low >= nm_high.
            {
                double[] y_raw = new double[] { 0, 0, 0.1, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 };
                StandardizedSpectrum ss = new StandardizedSpectrum(1250, 1200, y_raw);
                Assert.That(ss.IsValid == false);
            } // end code block.

            // Valid on normal case.
            {   double[] y_raw = new double[] { 0, 0, 0.1, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 };
                StandardizedSpectrum ss = new StandardizedSpectrum(250.0, 1600, y_raw);
                Assert.That(ss.IsValid == true);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high.IsWithinTolerance(1300.0, 0.000001));
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Increment.IsWithinTolerance(0.1, 0.000001));
                Assert.That(ss.Y_Vector[5000].IsWithinTolerance(0.45, 0.03));
                Assert.That(ss.Y_Vector[4000]==ss.Y(4000));
                Assert.That(ss.MinY.IsWithinTolerance(0.0, 0.02));
                Assert.That(ss.MaxY.IsWithinTolerance(0.5, 0.02));
                Assert.That(ss.Sum.IsWithinTolerance(2400, 100));
                Assert.That((ss.Integral/(ss.Sum-0.5*(ss.Y_Vector[0]+ss.Y_Vector[ss.NPoints-1])))
                    .IsWithinTolerance(ss.Increment, 0.000001));
                Assert.That(ss.X(0) == ss.Nm_low);
                Assert.That(ss.X(ss.NPoints-1) == ss.Nm_high);
                Assert.That(ss.Y(0) == ss.Y_Vector[0]);
                Assert.That(ss.Y(ss.NPoints-1) == ss.Y_Vector[ss.NPoints-1]);
            } // end code block.

            // ***** Test CONSTRUCTOR that expects x_basis & photonEnergy.

            // Invalid if fewer than 4 y-values.
            {   double[] nm_basis = new double[] { 250, 300, 350 };
                double[] y_basis = new double[] { 0, 1, 3 };
                StandardizedSpectrum ss = new StandardizedSpectrum(nm_basis, y_basis);
                Assert.That(ss.IsValid == false);
            } // end code block.

            // Valid if 4 (or more) y-values.
            {   double[] nm_basis = new double[] { 250, 300, 350, 400 };
                double[] y_basis = new double[] { 0, 1, 3, 2 };
                StandardizedSpectrum ss = new StandardizedSpectrum(nm_basis, y_basis);
                Assert.That(ss.IsValid);
            } // end code block.

            // Invalid on unequal lengths of otherwise valid x_basis and photonEnergy.
            {   double[] nm_basis = 
                    new double[] { 250, 405, 533, 712, 855, 1000, 1150, 1300, 1431 }; // 9 values.
                double[] y_basis = 
                    new double[] { 0, 0, 0.1, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 }; // 10 values.
                StandardizedSpectrum ss = new StandardizedSpectrum(nm_basis, y_basis);
                Assert.That(ss.IsValid == false);
            } // end code block.

            // Must succeed on normal case, even with x and y basis arrays out of nm order.
            {   double[] nm_basis = 
                    new double[] { 250, 533, 405, 712, 855, 1000, 1150, 1300, 1431, 1600 }; // out of order.
                double[] y_basis = 
                    new double[] { 0, 0.1, 0, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 }; // this matches x_basis order.
                StandardizedSpectrum ss = new StandardizedSpectrum(nm_basis, y_basis);
                Assert.That(ss.IsValid == true);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high.IsWithinTolerance(1300.0, 0.000001));
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Increment.IsWithinTolerance(0.1, 0.000001));
                Assert.That(ss.Y_Vector[5000].IsWithinTolerance(0.45, 0.03));
                Assert.That(ss.Y_Vector[4000]==ss.Y(4000));
                Assert.That(ss.MinY.IsWithinTolerance(0.0, 0.02));
                Assert.That(ss.MaxY.IsWithinTolerance(0.5, 0.02));
                Assert.That(ss.Sum.IsWithinTolerance(2400, 100));
                Assert.That((ss.Integral/(ss.Sum-0.5*(ss.Y_Vector[0]+ss.Y_Vector[ss.NPoints-1])))
                    .IsWithinTolerance(ss.Increment, 0.000001));
                //Console.WriteLine(ssOriginal.MinY + " " + ssOriginal.MaxY);
            } // end code block.
            
            // ***** Test CONSTRUCTOR for constant y-value.
            // Valid for any y-value, positive or negative.
            {   double y_constant = 2.3525;
                StandardizedSpectrum ss= new StandardizedSpectrum(y_constant);
                Assert.That(ss.IsValid == true);
                Assert.That(ss.Nm_low == 300.0);
                Assert.That(ss.Nm_high.IsWithinTolerance(1300.0, 0.000001));
                Assert.That(ss.NPoints == 10001);
                Assert.That(ss.Increment.IsWithinTolerance(0.1, 0.000001));
                for (int i=0; i<ss.NPoints; i++) {
                    Assert.That(ss.Y(i) == y_constant);
                } // for iAngle
            } // end code block.

            // ***** Test CONSTRUCTOR on boolean value (of isValid).
            // Y-values are always zero, whether set valid or invalid.
            {   StandardizedSpectrum ssTrue= new StandardizedSpectrum(true);
                Assert.That(ssTrue.IsValid == true);
                for(int i=0; i<ssTrue.NPoints; i++) {
                    Assert.That(ssTrue.Y(i) == 0.0);
                } // for iAngle
                StandardizedSpectrum ssFalse= new StandardizedSpectrum(false);
                Assert.That(ssFalse.IsValid == false);
                for(int i=0; i<ssFalse.NPoints; i++) {
                    Assert.That(ssFalse.Y(i) == 0.0);
                } // for iAngle
            } // end code block.

        } // .Test_Constructors().

        [Test]
        public void Test_Methods() {
            // a bit of setup.
            double[] y_basis = new double[] { 0, 0, 0.1, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 };
            StandardizedSpectrum ss = new StandardizedSpectrum(250.0, 1600.0, y_basis);
            Assert.That(ss.IsValid);
            Assert.That(ss.Sum.IsWithinTolerance(2400, 100));

            // test .ClipToMinOf().
            {   StandardizedSpectrum ssClipped = ss.ClipToMinOf(0.122);
                Assert.That(ssClipped.IsValid);
                Assert.That(ss.MinY < 0.122);
                Assert.That(ssClipped.MinY == 0.122);
                Assert.That(ssClipped.Sum > ss.Sum);
            } // end code block.
            
            // test .ClipToMaxOf().
            {   StandardizedSpectrum ssClipped = ss.ClipToMaxOf(0.444);
                Assert.That(ssClipped.IsValid);
                Assert.That(ss.MaxY > 0.444);
                Assert.That(ssClipped.MaxY == 0.444);
                Assert.That(ssClipped.Sum < ss.Sum);
            } // end code block.
            
            // test .MultiplyBy( scalar ).
            {   StandardizedSpectrum ssMultiplied = ss.MultiplyBy(8.0);
                Assert.That(ssMultiplied.IsValid);
                Assert.That(ssMultiplied.MinY == 8.0 * ss.MinY);
                Assert.That(ssMultiplied.MaxY == 8.0 * ss.MaxY);
                Assert.That(ssMultiplied.Sum  == 8.0 * ss.Sum);
                Assert.That(ssMultiplied.MinY == 8.0 * ss.MinY);
                double[] y_ss  = ss.Y_Vector;
                double[] y_ssMultipled = ssMultiplied.Y_Vector;
                for(int i=0; i<ss.NPoints; i++)
                    Assert.That(y_ssMultipled[i] == 8.0 * y_ss[i]);
            } // end code block.
            
            // test .MultiplyBy( other StandardizedSpectrum ).
            {   StandardizedSpectrum ssX = new StandardizedSpectrum(400, 900,
                    new double[] { 3, 4, 5, 6, 9, 3, 2, 3, 5 });
                StandardizedSpectrum ssMultipled = ss.MultiplyBy(ssX);
                Assert.That(ssMultipled.IsValid);
                double[] y_ss = ss.Y_Vector;
                double[] y_ssX = ssX.Y_Vector;
                double[] y_ssMultiplied = ssMultipled.Y_Vector;
                for(int i=0; i<ss.NPoints; i++)
                    Assert.That(y_ssMultiplied[i] == y_ss[i] * y_ssX[i]);
            } // end code block.
            
            // test .Add( other StandardizedSpectrum ).
            {   StandardizedSpectrum ssAdd = new StandardizedSpectrum(400, 900,
                    new double[] { 3, 4, 5, 6, 9, 3, 2, 3, 5 }); 
                StandardizedSpectrum ssSum = ss.Add(ssAdd);
                Assert.That(ssSum.IsValid);
                double[] y_ss = ss.Y_Vector;
                double[] y_ssAdd = ssAdd.Y_Vector;
                double[] y_ssSum = ssSum.Y_Vector;
                for(int i=0; i<ss.NPoints; i++)
                    Assert.That(y_ssSum[i] == y_ss[i] + y_ssAdd[i]);
            } // end code block.

            // Test .Clone().
            {   double[] y = new double[] { 0, 0, 0.1, 0.3, 0.5, 0.4, 0.2, 0.1, 0, 0 };
                StandardizedSpectrum ssOriginal = new StandardizedSpectrum(250.0, 1600.0, y);
                Assert.That(ssOriginal.IsValid);
                Assert.That(ssOriginal.Sum.IsWithinTolerance(2400, 100));
                StandardizedSpectrum ssClone = ssOriginal.Clone();
                Assert.That(ssClone.IsValid);
                Assert.That(ssClone.Nm_low==ssOriginal.Nm_low);
                Assert.That(ssClone.Nm_high==ssOriginal.Nm_high);
                Assert.That(ssClone.Increment==ssOriginal.Increment);
                Assert.That(ssClone.NPoints==ssOriginal.NPoints);
                Assert.That(ssClone.Y(5000)==ssOriginal.Y(5000));
                Assert.That(ssClone.MinY==ssOriginal.MinY);
                Assert.That(ssClone.MaxY==ssOriginal.MaxY);
                Assert.That(ssClone.Y_Vector!=ssOriginal.Y_Vector);
            } // end code block.

        } // Test_Methods().

    } // class.
} // namespace.
