using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVD.Astro.Spectral;

//***** class Detector.
//*****    Represents a CCD detector, integrating incoming PFluxApertured to count rate.
//*****    Eric Dose, Topeka, Kansas
//*****    Written 2007-2008, mods 2012-3.
//*****    Passed all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {
    public class Detector : IDetector {
        private bool isValid;
        private string detectorName;
        private static string detectorFolder = @"C:\Dev\Spectral\Data\Detector\";
        private string detectorFilename;
        private StandardizedSpectrum stdSpectrum;  // here, representing quantum efficiency.

        // CONSTRUCTOR by reading file.
        public Detector (string detectorName) {
            this.detectorName = detectorName;
            SpectralInputFile siFile = new SpectralInputFile(detectorFolder, detectorName, 0);
            if(siFile.IsValid == false) { isValid = false; return; }
            detectorFilename = siFile.FullFilename;
            stdSpectrum = (new StandardizedSpectrum(siFile.Nm_Vector, siFile.Y_Vector))
                .ClipToMinOf(0).ClipToMaxOf(1);
            isValid = stdSpectrum.IsValid;
        } // Constructor.

        // METHODS.
        public double CountRateFromPFlux(PFluxApertured pfluxAp) {
            return pfluxAp.StdSpectrum.MultiplyBy(this.stdSpectrum).Integral;
        } // .CountRateFromPFlux().
        public double InstrumentalMagnitudeFromPFlux(PFluxApertured pfluxAp) {
            return -2.5 * Math.Log10(CountRateFromPFlux(pfluxAp));
        } // .InstrumentalMagnitudeFromPFlux().

        // PROPERTIES.
        public bool IsValid { get { return isValid; } }
        public string DetectorName { get { return detectorName; } }
        public static string FolderName { get { return detectorFolder; } }
        public StandardizedSpectrum QE 
            { get { return (StandardizedSpectrum)stdSpectrum.Clone(); } }
        public double PeakQE { get { return stdSpectrum.MaxY; } }

    } // class Detector.
} // namespace.
