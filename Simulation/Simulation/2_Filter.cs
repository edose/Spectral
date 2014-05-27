using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVD.Astro.Spectral;

//***** class Filter.
//*****    A photometric filter of known transmission spectrum, optionally modified with
//*****       fractional coverage and relative thickness.
//*****    Eric Dose, Topeka, Kansas.
//*****    Written 2011 - 2012.
//*****    Passed all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {
    public class Filter : IFilter {
        private bool isValid;
        private string filterName;
        private static string filterFolder = @"C:\Dev\Spectral\Data\Filter\";
        private StandardizedSpectrum stdSpectrumInput;
        private StandardizedSpectrum stdSpectrumEffective; // this filter's effective transmission spectrum.
        private double relativeThickness;
        private double fractionCoverage;

        // CONSTRUCTOR by reading from SpectralInputFile, with rel. thickness and fraction coverage.
        public Filter (string filterName, double relativeThickness, double fractionCoverage) {
            this.filterName = filterName;
            this.relativeThickness = relativeThickness;
            this.fractionCoverage = fractionCoverage;
            if((relativeThickness < 0) || (fractionCoverage < 0) || (fractionCoverage > 1)) { 
                isValid = false; 
                return; 
            }
            SpectralInputFile siFile = new SpectralInputFile(filterFolder, filterName, 0);
            if(siFile.IsValid == false) { isValid = false; return; }
            stdSpectrumInput = (new StandardizedSpectrum(siFile.Nm_Vector, siFile.Y_Vector))
                .ClipToMinOf(0).ClipToMaxOf(1);

            // handle the usual, simple case, then return.
            if ((relativeThickness == 1) && (fractionCoverage == 1)) {
                stdSpectrumEffective = stdSpectrumInput;
                isValid = stdSpectrumEffective.IsValid;
                return;
            }

            // handle all other cases.
            double[] newY_Vector = new double[stdSpectrumInput.NPoints];
            for (int i=0; i<stdSpectrumInput.NPoints; i++)
                newY_Vector[i] = (1.0 - fractionCoverage) + // for the open area, if any...
                    (fractionCoverage * Math.Pow(stdSpectrumInput.Y(i),relativeThickness)); // for covered area.
            stdSpectrumEffective = new StandardizedSpectrum(stdSpectrumInput.Nm_low, 
                stdSpectrumInput.Nm_high, newY_Vector);
            isValid = stdSpectrumEffective.IsValid;
        } // Constructor.

        // CONSTRUCTOR : for the usual case with rel thickness and fraction coverage both = 1.
        public Filter (string filterName) : this(filterName, 1, 1) { }

        // CONSTRUCTOR : no arguments means no filter (100% transmission).
        public Filter () : this(1.0) { }

        // CONSTRUCTOR : for neutral density (equal transmission across all wavelengths).
        public Filter (double transmissionFraction) {
            relativeThickness = 1;
            fractionCoverage = 1;
            // return invalid Filter on any invalid transmissionFraction.
            if (transmissionFraction < 0 || transmissionFraction > 1) {
                filterName = "Transmission invalid (given value = " + transmissionFraction.ToString();
                stdSpectrumEffective = new StandardizedSpectrum (0);
                isValid = false;
                return;
            }
            // following is for the valid case.
            filterName = "Transmission=" + transmissionFraction.ToString();
            stdSpectrumInput = (new StandardizedSpectrum(transmissionFraction)).ClipToMinOf(0).ClipToMaxOf(1);
            stdSpectrumEffective = stdSpectrumInput;
            isValid = true;
        } // Constructor(transmissionFraction).


        // METHODS: 
        public PFluxPerArea ActOn(PFluxPerArea pfluxPerArea) {
            StandardizedSpectrum ssPFlux = pfluxPerArea.StdSpectrum;
            return new PFluxPerArea(pfluxPerArea, ssPFlux.MultiplyBy(stdSpectrumEffective));
        } // .ActOn(PFluxPerArea).
        public PFluxApertured ActOn(PFluxApertured pfluxApertured) {
            StandardizedSpectrum ssPFlux = pfluxApertured.StdSpectrum;
            return new PFluxApertured(pfluxApertured, ssPFlux.MultiplyBy(stdSpectrumEffective));
        } // .ActOn(PFluxApertured).
        
        // PROPERTIES:
        public bool IsValid { get { return isValid; } }
        public static string FolderName { get { return filterFolder; } }
        public string FilterName { get { return filterName; } }
        public double RelativeThickness { get { return relativeThickness; } }
        public double FractionCoverage { get { return fractionCoverage; } }
        public StandardizedSpectrum StdSpectrumInput {
            get { return (StandardizedSpectrum)stdSpectrumInput.Clone(); } }
        public StandardizedSpectrum StdSpectrumEffective {
            get { return (StandardizedSpectrum)stdSpectrumEffective.Clone(); } }
                
    } // class Filter.
} // namespace.
