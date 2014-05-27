using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ELib; // for supporting class CubicSpline.

//***** class StandardizedSpectrum.
//***** Passed all unit tests December 3, 2012, Topeka, Kansas.

namespace EVD.Astro.Spectral {

    public class StandardizedSpectrum {
        // StandardizedSpectrum objects ARE immutable.
        double nm_low, nm_high; // lowest and highest wavelength, in nanometers
        int nPoints;            // number of points, ~ 1 + (nm_high_raw-nm_low_raw)/increment
        double increment;       // in nm.
        double[] y;             // values (transmission or flux) at each wavelength, in incr order.
        bool isValid;

        // Define width and resolution of standardized spectrum.
        readonly double NM_LOW_STD   = 300;   // nanometers
        readonly double NM_INCR_STD  = 0.1;   //   "
        readonly int    NM_COUNT_STD = 10001; // so that nm range is 300-1300, inclusive of ends.

        /// <summary>CONSTRUCTOR for user-supplied points that are evenly spaced in nm.</summary>
        /// <param name="nm_low_raw">nm corresponding to first y-value.</param>
        /// <param name="nm_high_raw">nm corresponding to last y value.</param>
        /// <param name="y_raw">array of all y values, ordered in nm.</param>
        public StandardizedSpectrum(double nm_low_raw, double nm_high_raw, double[] y_raw) {
            int nPoints_raw = y_raw.Length;
            if ((nm_high_raw <= nm_low_raw) || ( nPoints_raw < 4 )) {
                y=null; isValid=false; return;
            }
            double increment_raw = (nm_high_raw-nm_low_raw) / ((double)(nPoints_raw-1));
            nm_low = NM_LOW_STD;
            nm_high = NM_LOW_STD + ((double)(NM_COUNT_STD-1)) * NM_INCR_STD;
            increment = NM_INCR_STD;
            nPoints = NM_COUNT_STD;
            y = Standardize(nm_low_raw, increment_raw, y_raw);
            isValid = true;
        }

        /// <summary>CONSTRUCTOR for user-supplied points that need not be evenly spaced in nm.</summary>
        /// <param name="nm">array of all y values, ordered in nm.</param>
        /// <param name="photonEnergy">array of all y values, ordered in nm.</param>
        public StandardizedSpectrum(double[] nm_basis, double[] y_basis) {
         // This constructor for points not necessarily evenly spaced. X and Y vectors as input.
             if ((nm_basis.Length != y_basis.Length) || (nm_basis.Length < 4) || (y_basis.Length < 4)) {
                y=null; isValid=false; return;
            }
            nm_low = NM_LOW_STD;
            nm_high = NM_LOW_STD + ((double)(NM_COUNT_STD-1)) * NM_INCR_STD;
            increment = NM_INCR_STD;
            nPoints = NM_COUNT_STD;
            y = Standardize(nm_basis, y_basis);
            isValid = true;
        }

        /// <summary>CONSTRUCTOR for standard (e.g., utility) spectrum with uniform y-values.</summary>
        /// <param name="y_constant">y value to apply to all points in standard spectrum.</param>
        public StandardizedSpectrum(double y_constant) {
            nm_low = NM_LOW_STD;
            nm_high = NM_LOW_STD + ((double)(NM_COUNT_STD-1)) * NM_INCR_STD;
            increment = NM_INCR_STD;
            nPoints = NM_COUNT_STD;
            y = new double[NM_COUNT_STD];
            for(int i=0; i<nPoints; i++) {
                y[i] = y_constant;
            }
            isValid = true;
        }

        // CONSTRUCTOR used to generate an invalid StandardizedSpectrum 
        //    (e.g., for cloning an invalid StandardizedSpectrum,
        //    or pro forma to set IsValid to false when a containing object is known invalid).
        public StandardizedSpectrum (bool isValid_FromUser) {
            SetStandardNmVector();
            y = new double[NM_COUNT_STD];
            this.isValid = isValid_FromUser;
        }

        /// <summary>PRIVATE CONSTRUCTOR; a helper for Cloning etc of existing objects.
        /// This assumes that photonEnergy vector already maps to standard x (nm) basis.</summary>
        /// <param name="photonEnergy">array of all y values, ordered in nm.</param>
        private StandardizedSpectrum(double[] y_basis) {
            SetStandardNmVector();
            y = new double[NM_COUNT_STD];
            for(int i=0; i<nPoints; i++) {
                if(i < y_basis.Length) y[i] = y_basis[i];
                else y[i] = 0;
            }
            isValid = true;
        }

        // METHODS.
        private void SetStandardNmVector () {
            nm_low = NM_LOW_STD;
            nm_high = NM_LOW_STD + ((double)(NM_COUNT_STD-1)) * NM_INCR_STD;
            increment = NM_INCR_STD;
            nPoints = NM_COUNT_STD;
        }

        // PROPERTIES.
        public bool IsValid      { get { return isValid; } }
        public double Nm_low     { get { return nm_low; } }
        public double Nm_high    { get { return nm_high; } }
        public int NPoints       { get { return nPoints; } }
        public double Increment  { get { return increment; } } // in nm.
        public double[] Y_Vector { get { return (double[])y.Clone(); } }
        public double MinY       { get { return y.Min(); } }
        public double MaxY       { get { return y.Max(); } }
        public double Sum        { get { return y.Sum(); } }
        public double Integral   { get { return (y.Sum() - (y[0]+y[nPoints-1])/2.0) * increment; } }

        /// <summary>Clone this spectrum. Uses Y_Vector to prevent back-contamination.</summary>
        /// <returns>A StandardizedSpectrum object.</returns>
        public StandardizedSpectrum Clone() {
            if(this.IsValid)
                return new StandardizedSpectrum(this.Y_Vector);
            else
                return new StandardizedSpectrum(false);
        }

        // methods (to return single standardized X or Y values).
        public double X (int index) { return nm_low + ((double)(index)) * increment; }
        public double Y (int index) { return y[index]; }

        // methods (to return new StandardizedSpectrum object with modified Y values).
        public StandardizedSpectrum ClipToMaxOf(double newMaxValue) {
            double[] y_new = new double[nPoints];
            for(int i=0; i<nPoints; ++i)
                y_new[i] = Math.Min(y[i], newMaxValue);
            return new StandardizedSpectrum(y_new);
        }
        public StandardizedSpectrum ClipToMinOf(double newMinValue) {
            double[] y_new = new double[nPoints];
            for(int i=0; i<nPoints; ++i)
                y_new[i] = Math.Max(y[i], newMinValue);
            return new StandardizedSpectrum(y_new);
        }
        public StandardizedSpectrum MultiplyBy(double multiplier) {
            double[] y_new = new double[nPoints];
            for(int i=0; i<nPoints; ++i)
                y_new[i] = y[i] * multiplier;
            return new StandardizedSpectrum(y_new);
        }
        public StandardizedSpectrum MultiplyBy(StandardizedSpectrum multiplierVector) {
            double[] y_multiplier = multiplierVector.Y_Vector;
            double[] y_new = new double[nPoints];
            for(int i=0; i<nPoints; ++i)
                y_new[i] = y[i] * y_multiplier[i];
            return new StandardizedSpectrum(y_new);
        }
        public StandardizedSpectrum Add(StandardizedSpectrum addVector) { // rarely used.
            double[] y_add = addVector.Y_Vector;
            double[] y_new = new double[nPoints];
            for(int i=0; i<nPoints; ++i)
                y_new[i] = y[i] + y_add[i];
            return new StandardizedSpectrum(y_new);
        }

        /// <summary>Interpolate raw nm basis of spectrum to put it on standardized scale.</summary>
        private double[] Standardize(double nm_low_raw, double nm_incr_raw,
            double[] spectrumRaw) {
            // This version for evenly-spaced (in nm) input spectrum to be put on standard nm scale.
            // ... usually this will be calculated/synthetic data, or data from a transmission table, etc.
            if(spectrumRaw==null) return new double[0];
            if(spectrumRaw.Length < 3) return new double [0];
            // construct spline data.
            double[] x_basis = new double[spectrumRaw.Length];
            for(int i=0; i<x_basis.Length; i++)
                x_basis[i] = nm_low_raw + ((double)(i))*nm_incr_raw;
            double[] y_basis = (double[])spectrumRaw.Clone();
            return Standardize(x_basis, y_basis);
        } // Standardize(), from evenly spaced raw data.

        private double[] Standardize(double[] x_basis, double[] y_basis) {
            // This version for unevenly-spaced (in nm) input spectrum to be put on standard nm scale.
            // ... usually this will be data read from a spectral file, e.g., Calspec.
            // ... Elib.CubicSpline will accept non-increasing x_basis, so long as x_basis and photonEnergy
            //     arrays match element by element (it resorts x & y internally; user's arrays unchanged).
            if(x_basis==null || y_basis==null) return new double[0];
            if((x_basis.Length!=y_basis.Length) || (x_basis.Length<3)) return new double[0];
            int basisLength = x_basis.Length;
            var spline = new CubicSpline(x_basis, y_basis);
            // Compute and fill in Y-values at all standardized wavelengths.
            double[] spectrumStd = new double[NM_COUNT_STD];
            for (int i=0; i<NM_COUNT_STD; i++) {
                double thisX = NM_LOW_STD + ((double)(i))*NM_INCR_STD;
                if      (thisX < x_basis[0]) {spectrumStd[i] = y_basis[0];}
                else if (thisX > x_basis[basisLength-1]) {spectrumStd[i] = y_basis[basisLength-1];}
                else {spectrumStd[i] = spline.valueAt(thisX);}            
            } // for i.

            return spectrumStd;
        } // Standardize() from arbitrarily-spaced raw data.

    } // class [StandardSpectrum].

} // namespace [EVD.Astro.Spectral].
