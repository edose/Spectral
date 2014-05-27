using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVD.Astro.Spectral;

//***** class Passband.
//*****    A standard passband of known transmission spectrum, used as measure of photon
//*****       flux in a known range of wavelengths (i.e., a known passband).
//*****    Renamed to Passband (from Bandpass) 1/26/2013, for precision in naming.
//*****    Eric Dose, Topeka, Kansas.
//*****    Written 2011 - 2012.
//*****    Passed all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {
    public class Passband : IFilter {
        private bool isValid;
        private string passbandName;
        private static string passbandFolder = @"C:\Dev\Spectral\Data\Passband\";
        private string passbandFilename;
        private double pfluxAtZeroMag;
        private StandardizedSpectrum stdSpectrum;

        // CONSTRUCTOR by reading from SpectralInputFile.
        public Passband (string passbandName) {
            this.passbandName = passbandName;
            SpectralInputFile siFile = new SpectralInputFile(passbandFolder, passbandName, 1);
            if(siFile.IsValid == false) { isValid = false; return; }
            passbandFilename = siFile.FullFilename;
            stdSpectrum = (new StandardizedSpectrum(siFile.Nm_Vector, siFile.Y_Vector))
                .ClipToMinOf(0).ClipToMaxOf(1);
            // parse the header line to give pfluxAtZeroMag.
            string headerLine = siFile.HeaderDataLines[0];
            string[] goodTokens = new string[1];
            int goodTokensFound = 0;
            string[] rawTokens = headerLine.Trim().Split(' ', '\t', ',');
            for(int i=0; i<rawTokens.Length; i++) {
                if(rawTokens[i].Trim().Length > 0) {
                    goodTokens[goodTokensFound] = rawTokens[i].Trim();
                    goodTokensFound++;
                } // if rawTokens[i].
                if(goodTokensFound >= 1) break;
            } // for i.
            if(goodTokensFound != 1) { isValid = false; return; }
            bool parseOK;
            parseOK = Double.TryParse(goodTokens[0], out pfluxAtZeroMag);
            if(parseOK == false) { isValid = false; return; }
            isValid = stdSpectrum.IsValid && (pfluxAtZeroMag > 0.0);
        } // Constructor.

        // METHODS.
        public PFluxPerArea ActOn(PFluxPerArea pfluxPerArea) {
            StandardizedSpectrum ssPFlux    = pfluxPerArea.StdSpectrum;
            StandardizedSpectrum ssPassband = this.stdSpectrum;
            return new PFluxPerArea(pfluxPerArea, ssPFlux.MultiplyBy(ssPassband));
        } // .ActOn(PFluxPerArea).
        public PFluxApertured ActOn(PFluxApertured pfluxApertured) {
            StandardizedSpectrum ssPFlux     = pfluxApertured.StdSpectrum;
            StandardizedSpectrum ssPassband = this.stdSpectrum;
            return new PFluxApertured(pfluxApertured, ssPFlux.MultiplyBy(ssPassband));
        } // .ActOn(PFluxApertured).

        // PROPERTIES.
        public bool IsValid { get { return isValid; } }
        public static string FolderName   { get { return passbandFolder; } }
        public string PassbandName { get { return passbandName; } }
        public double PFluxAtZeroMag { get { return pfluxAtZeroMag; } }
        public StandardizedSpectrum StdSpectrum {
            get { return (StandardizedSpectrum)stdSpectrum.Clone(); }
        }
        
    } // class Passband.

} // namespace.
