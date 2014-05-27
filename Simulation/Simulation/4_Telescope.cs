using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVD.Astro.Spectral;

//***** class Telescope.
//*****    Represents a telescope, effectively generates an aperture+filter object for
//*****    optical transmission. Operates on per-area flux to give apertured flux.
//*****    Eric Dose, Topeka, Kansas
//*****    Written 2011-2012.
//*****    Passes all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {
    public class Telescope : IAperturedFilter {
        private bool isValid;
        private double apertureDiameter;
        private double percentDiameterObstructed;
        private double apertureArea;
        private string telescopeName;
        private static string telescopeFolder = @"C:\Dev\Spectral\Data\Telescope\";
        private StandardizedSpectrum stdSpectrum;  // representing transmission spectrum.

        // CONSTRUCTOR from file, with diameter (lambda) and percent obstruction also from file.
        public Telescope (string telescopeName) {
            this.telescopeName = telescopeName;
            //Console.WriteLine(telescopeName);
            SpectralInputFile siFile = new SpectralInputFile(telescopeFolder, telescopeName, 1);
            //Console.WriteLine("siFile.FullFilename >" + siFile.FullFilename + "<");
            if(siFile.FullFilename.Length == 0) {isValid = false; return; }
            //Console.WriteLine("numPoints= " + siFile.NSpectrumPoints.ToString());
            stdSpectrum = (new StandardizedSpectrum(siFile.Nm_Vector, siFile.Y_Vector))
                .ClipToMinOf(0).ClipToMaxOf(1);
            //Console.WriteLine(stdSpectrum.IsValid.ToString());
            if(stdSpectrum.IsValid == false) { isValid = false; return; }
            // parse the header line to give diameter & percent obstruction.
            string headerLine = siFile.HeaderDataLines[0];
            string[] goodTokens = new string[2];
            int goodTokensFound = 0;
            string[] rawTokens = headerLine.Trim().Split(' ', '\t', ',');
            for (int i=0; i<rawTokens.Length; i++) {
                if(rawTokens[i].Trim().Length > 0) {
                    goodTokens[goodTokensFound] = rawTokens[i].Trim();
                    goodTokensFound++;
                } // if rawTokens[i].
                if(goodTokensFound >= 2) break;
            } // for i.
            if(goodTokensFound != 2) { isValid = false; return; }
            bool parseOK;
            parseOK = Double.TryParse(goodTokens[0], out apertureDiameter);
            if(parseOK == false) { isValid = false; return; }
            parseOK = Double.TryParse(goodTokens[1], out percentDiameterObstructed);
            if(parseOK == false) { isValid = false; return; }
            apertureArea = (Math.PI * apertureDiameter * apertureDiameter / 4.0) * 
                (1.0 - Math.Pow((percentDiameterObstructed/100.0), 2));
            isValid = true;
        } // constructor.

        // METHODS.
        public PFluxApertured ActOn (PFluxPerArea pfluxPerArea) {
            PFluxApertured pfluxAp = new PFluxApertured(pfluxPerArea, this.apertureArea);
            StandardizedSpectrum newStdSpectrum = pfluxAp.StdSpectrum.MultiplyBy(stdSpectrum);
            return new PFluxApertured(pfluxAp, newStdSpectrum);
        } // .ActOn().

        // PROPERTIES.
        public bool IsValid { get { return isValid; } }
        public double ApertureDiameter { get { return apertureDiameter; } }
        public double PercentDiameterObstructed { get { return percentDiameterObstructed; } }
        public double ApertureArea { get { return apertureArea; } }
        public string TelescopeName { get { return telescopeName; } }
        public static string FolderName { get { return telescopeFolder; } }
        public StandardizedSpectrum StdSpectrum 
            { get { return (StandardizedSpectrum)stdSpectrum.Clone(); } }
    } // class.
} // namespace.
