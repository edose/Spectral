using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ELib;
using NUnit.Framework;
using EVD.Astro.Spectral;

// These unit tests depend on proper files existing in testFolder.

namespace EVD.Astro.Spectral {
    class UnitTest_SpectralInputFile {
        //[TestFixtureSetUp]     public void testFixtureSetup() { }
        //[TestFixtureTearDown]  public void testFixtureTearDown() { }
        //[SetUp]                public void setup() { }
        //[TearDown]             public void tearDown() { }

        string testFolder = @"C:\Dev\Spectral\Data\$UnitTestOnly\";

        [Test]
        // static method .Find() is depended upon by class constructor, so test it first.
        public void Try_Static_Find() {

            // If file simply doesn'transmissionFromYVector exist, .File() must return zero-length string.
            {   string testItemName = "No Such Item Name As This";
                string foundFullFilename = SpectralInputFile.Find(testFolder, testItemName);
                Assert.That(String.Compare(foundFullFilename, "", true) == 0);
            } // end code block.

            // If item name in multiple files, return file name of any one of the files.
            {   string testItemName = "= 20 Spectrum Points.";
                string foundFullFilename = SpectralInputFile.Find(testFolder, testItemName);
                bool isCorrectlyFound = (String.Compare(foundFullFilename, Path.Combine(testFolder,
                    "TwentySpectrumPoints.txt"), true) == 0) ||
                    (String.Compare(foundFullFilename, Path.Combine(testFolder,
                    "TwentySpectrumPoints_Duplicate.txt"), true) == 0);
                Assert.That(isCorrectlyFound);
            } // end code block.

            // Normal case: must return full file name.
            {   string testItemName = "= Item Name Only";
                string foundFullFilename = SpectralInputFile.Find(testFolder, testItemName);
                // Must return full file name for normal case.
                Assert.That(String.Compare(foundFullFilename, Path.Combine(testFolder, 
                    "ItemNameOnly.txt"), true) == 0);
                // Must not return "" for folder.
                Assert.That(String.Compare(foundFullFilename, Path.Combine("",
                    "ItemNameOnly.txt"), true) != 0); 
            } // end code block.
         } // .Try_Static_Find().

        [Test]
        public void Try_ConstructFromGoodFile () {

            // Verify case with no header lines.
            {   string testItemName = "= 20 Spectrum Points No Header.";
                SpectralInputFile sif20x = new SpectralInputFile(testFolder, testItemName, 0);
                Assert.NotNull(sif20x);
                Assert.That(sif20x.IsValid);
                Assert.That(String.Compare(sif20x.FullFilename, Path.Combine(testFolder, "NoHeaderLine.txt"))==0);
                Assert.That(sif20x.HeaderDataLines.Length == 0);
                Assert.That(sif20x.NSpectrumPoints == 20);
                Assert.That(sif20x.Nm_Vector.Length == 20);
                Assert.That(sif20x.Y_Vector.Length == 20);
                Assert.That(sif20x.Nm_Vector[18] == 620); // note: the 19th point is index 18.
                Assert.That(sif20x.Y_Vector[18].IsWithinTolerance(0.18, 0.000001));
            } // end code block.            
            
            // Verify case with numerous header lines.
            {   string testItemName = "= 20 Spectrum Points.";
                SpectralInputFile sif20 = new SpectralInputFile(testFolder, testItemName, 2);
                Assert.NotNull(sif20);
                Assert.That(sif20.IsValid);
                bool isCorrectlyFound = (String.Compare(sif20.FullFilename, Path.Combine(testFolder,
                    "TwentySpectrumPoints.txt"), true) == 0) ||
                    (String.Compare(sif20.FullFilename, Path.Combine(testFolder,
                    "TwentySpectrumPoints_Duplicate.txt"), true) == 0);
                Assert.That(isCorrectlyFound);
                Assert.That(String.Compare(sif20.HeaderDataLines[0], "First header line.", true) == 0);
                Assert.That(String.Compare(sif20.HeaderDataLines[1], "Second header line.", true) == 0);
                Assert.That(sif20.HeaderDataLines.Length == 2);
                Assert.That(sif20.NSpectrumPoints == 20);
                Assert.That(sif20.Nm_Vector.Length == 20);
                Assert.That(sif20.Y_Vector.Length == 20);
                Assert.That(sif20.Nm_Vector[18] == 620); // note: the 19th point is index 18.
                Assert.That(sif20.Y_Vector[18].IsWithinTolerance(0.18, 0.000001));
            } // end code block.
            
            // Verify case for very large file (very many data lines).
            {   string testItemName = "Vega (calspec) TEST ONLY";
                SpectralInputFile sif20x = new SpectralInputFile(testFolder, testItemName, 0);
                Assert.NotNull(sif20x);
                Assert.That(sif20x.IsValid);
                Assert.That(String.Compare(sif20x.FullFilename, Path.Combine(testFolder, "Vega_calspec_TestOnly.txt"))==0);
                Assert.That(sif20x.HeaderDataLines.Length == 0);
                Assert.That(sif20x.NSpectrumPoints == 8846);
                Assert.That(sif20x.Nm_Vector.Length == 8846);
                Assert.That(sif20x.Y_Vector.Length == 8846);
                Assert.That(sif20x.Nm_Vector[18].IsWithinTolerance(91.684, 0.01));
                Assert.That(sif20x.Y_Vector[18].IsWithinTolerance(6.85E-15, 0.02E-15));
                Assert.That(sif20x.Y_Vector[8845].IsWithinTolerance(1.26E-19, 0.02E-19));
            } // end code block.        
        } // .Try_ConstructFromGoodFile ().

        [Test]
        public void Try_ConstructFromDefectiveFiles() {
            // Not valid if item name is zero-length.
            {   string testItemName = "";
                SpectralInputFile sifEmpty = new SpectralInputFile(testFolder, testItemName, 2);
                Assert.NotNull(sifEmpty);
                Assert.That(sifEmpty.IsValid == false);
            } // end code block.

            // Not valid if item name does not exist in any file in folder.
            {   string testItemName = "Not Exist";
                SpectralInputFile sifNotExist = new SpectralInputFile(testFolder, testItemName, 2);
                Assert.NotNull(sifNotExist);
                Assert.That(sifNotExist.IsValid == false);
            } // end code block.

            // Not valid if too few header lines requested.
            {   string testItemName = "= 20 Spectrum Points."; // this file has 2 header lines, not 1.
                SpectralInputFile sifHeader1 = new SpectralInputFile(testFolder, testItemName, 1);
                Assert.NotNull(sifHeader1);
                Assert.That(sifHeader1.IsValid == false);
            } // end code block.

            // Valid but incorrect (missing one spectrum point) if too many header lines requested.
            {   string testItemName = "= 20 Spectrum Points."; // this file has 2 header lines, not 3.
                SpectralInputFile sifHeader3 = new SpectralInputFile(testFolder, testItemName, 3);
                Assert.NotNull(sifHeader3);
                Assert.That(sifHeader3.IsValid == true); // x of 1st spectrum point read as a header line.
                Assert.That(sifHeader3.NSpectrumPoints == 19); // one fewer than actual.
            } // end code block.

            // Invalid if any spectrum point has lower x than the previous point.
            {   string testItemName = "= Non-monotonically increasing."; // x not monotonically increasing.
                SpectralInputFile sif4 = new SpectralInputFile(testFolder, testItemName, 3);
                Assert.NotNull(sif4);
                Assert.That(sif4.IsValid == false);
            } // end code block.

        } // .Try_ConstructFromDefectiveFiles().
    } // class.
} // namespace.
