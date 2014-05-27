using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using EVD.Astro.Spectral;

//***** class group rawPFlux.
//*****    Photon Flux per unit wavelength, either per area or absolute (through aperture of known area).
//*****    Eric Dose, Topeka, Kansas.
//*****    Written 2011 - 2012.
//*****    Passed all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {

    public abstract class PFluxAbstract {
        protected const double H = 6.62606957E-34; // in J*s (SI)
        protected const double C = 299792458;      // in lambda/s (SI)
        protected const double K = 1.3806488E-23;  // in J/K (SI)

        // FIELDS (abstract class).
        protected bool isValid = false; // default value.
        protected string pfluxName = "default value from abstract class";
        protected static string pfluxFolder = @"C:\Dev\Spectral\Data\PFlux\";
        protected StandardizedSpectrum stdSpectrum;

        // PROPERTIES (abstract class).
        public bool IsValid               { get { return isValid; } }
        public static string FolderName   { get { return String.Copy(pfluxFolder); } } // STATIC.
        public string PFluxName           { get { return String.Copy(pfluxName); } }
        public StandardizedSpectrum StdSpectrum { get {
            return (StandardizedSpectrum)stdSpectrum.Clone();
            }
        }
    } // class PFluxAbstract.

    public class PFluxPerArea : PFluxAbstract {
        // PFluxPerArea constructors are more difficult and numerous than PFluxApertured constructors, 
        //    since these represent light sources, and PFluxApertured objects are derived only.
        // Units in object are: wavelength in nm, Pflux in photons/s/nm/m2.
        
        public PFluxPerArea (string pfluxName) {
            // This method reads data from one .txt file to make one PFluxPerArea object.
            // File's first line must be pfluxName, the PFlux object's name (may well != filename).
            // Comment line: any line after first line beginning with //.
            // Data in 2 columns: (1) wavelength in nm, (2) PFlux in photons/s/nm/m2.
            this.pfluxName = pfluxName;
            SpectralInputFile siFile = new SpectralInputFile(pfluxFolder, pfluxName, 0);
            if (siFile.IsValid == false) {
                isValid = false;
                stdSpectrum = new StandardizedSpectrum(false); // dummy spectrum.
                return; 
            }
            stdSpectrum = new StandardizedSpectrum(siFile.Nm_Vector, siFile.Y_Vector);
            isValid = stdSpectrum.IsValid;
        } // constructor.
        
        public PFluxPerArea (double tempK, Passband pb, double magPassband) {
            if((tempK > 60000.0) || (tempK < 500.0)) {isValid = false; return; }
            if(pb.IsValid == false) { isValid = false; return; }
            if((magPassband > 30.0) || (magPassband < -10.0)) { isValid = false; return; }
            // make raw StdSpectrum photon flux per wavelength, at each wavelength.
            StandardizedSpectrum ssDummy = new StandardizedSpectrum(0.0); // to extract nm basis.
            int nPoints = ssDummy.NPoints;
            double[] nm = new double[nPoints];
            double[] rawPFlux  = new double[nPoints];
            for(int i=0; i<nPoints; i++) {
                nm[i] = ssDummy.X(i);
                double lambda = nm[i] * 1E-9; // wavelength in meters
                // 1.84E-17 steradians for star of ~ 1 milliarcsec radius (e.g., star).
                //    this is normalized away, below.
                rawPFlux[i] = (ssDummy.Increment * 1.0E-9) * (2.0 * C / (lambda*lambda*lambda*lambda)) 
                    * (1/(Math.Exp(H*C/(lambda*K*tempK))-1)) * 1.84E-17; // in photons/s/m2/0.1nm
            } // for i.
            StandardizedSpectrum ssRaw = new StandardizedSpectrum (nm, rawPFlux);

            // now, normalize to target mag at passband (similar to .NormalizeToPassbandMag()).
            double rawTotalFluxPb    = ssRaw.MultiplyBy(pb.StdSpectrum).Integral;
            double targetTotalFluxPb = pb.PFluxAtZeroMag * Math.Pow(10.0, (-magPassband/2.5));
            double factor = targetTotalFluxPb / rawTotalFluxPb;
            this.stdSpectrum = ssRaw.MultiplyBy(factor);
            isValid = this.stdSpectrum.IsValid;
        } // constructor.

        // CONSTRUCTOR for new object, using old object except using new flux StandardizedSpectrum.
        public PFluxPerArea (PFluxPerArea pfluxPerArea_Old, StandardizedSpectrum stdSpectrum_New) {
            pfluxName = pfluxPerArea_Old.pfluxName;
            stdSpectrum = stdSpectrum_New;
            isValid = pfluxPerArea_Old.IsValid && stdSpectrum_New.IsValid;        
        } // constructor.
               
        // METHODS.
        // IFilter covers classes: Filter.
        public PFluxPerArea Through(IFilter filter) { 
            return filter.ActOn(this);
        }
        // IFilterApertured covers class: Telescope.
        public PFluxApertured Through(IAperturedFilter aperturedFilter) {
            return aperturedFilter.ActOn(this);
        }
        public PFluxPerArea Through(AirPath airPath) {
            return airPath.ActOn(this);
        }

        public PFluxPerArea MultiplyBy(double factor) {
            return new PFluxPerArea(this, this.StdSpectrum.MultiplyBy(factor));
        }
        public PFluxPerArea ChangeMagnitudeBy(double magChange) {
            double factor = Math.Pow(10.0, -magChange/2.5);
            return this.MultiplyBy(factor);        
        }
        public double FluxInPassband (Passband pb) {
            return this.Through(pb).stdSpectrum.Integral;
        }
        public PFluxPerArea NormalizeToPassbandMag(Passband pb, double targetMag) {
            double magBefore = this.Magnitude(pb);
            double magChange = targetMag - magBefore;
            double factor = Math.Pow(10.0, -magChange/2.5);
            return this.MultiplyBy(factor);
        }
        public double Magnitude (Passband pb) {
            return -2.5 * Math.Log10(this.FluxInPassband(pb) / pb.PFluxAtZeroMag );
        }
        public double ColorIndex (Passband pb1, Passband pb2) {
            return this.Magnitude(pb1)-this.Magnitude(pb2);
        }

        // raw form, does not normalize flux to a magnitude (mag will increase on decreased temp).
        public PFluxPerArea ChangeBBTemp(double tempBefore, double tempAfter, Passband pb, 
            double targetMag) {
            // make StdSpectrum of factor at each wavelength.
            StandardizedSpectrum ssDummy = new StandardizedSpectrum(0.0); // to extract nm basis.
            int nPoints = ssDummy.NPoints;
            double[] nm = new double[nPoints];
            double[] factor  = new double[nPoints];
            for(int i=0; i<nPoints; i++) {
                nm[i] = ssDummy.X(i);
                double hnu = H * C / (nm[i]/1E9); // Energy (H*nu = H*C/wavelength) in SI units.
                factor[i] = ( 1.0/(Math.Exp(hnu/(K*tempAfter))-1.0)) 
                          / ( 1.0/(Math.Exp(hnu/(K*tempBefore))-1.0));
            }
            StandardizedSpectrum ssFactor = new StandardizedSpectrum(nm, factor);
            PFluxPerArea pfluxNewTemp = new PFluxPerArea(this, this.stdSpectrum.MultiplyBy(ssFactor));
            return pfluxNewTemp.NormalizeToPassbandMag(pb, targetMag);
        }


        // PROPERTIES.
        // (Additional properties are inherited from abstract class PFluxAbstract.)
        public double TotalPhotonFlux { get { return this.stdSpectrum.Integral; } }
    
    } // class [PFluxPerArea].

    //*********************************************************************************************
    public class PFluxApertured : PFluxAbstract {
        // FIELD specific to PFluxApertured.
        double apertureArea; // in meters^2

        // CONSTRUCTOR for new PFluxApertured object, using PFluxPerArea and aperture area (in meters^2).
        public PFluxApertured (PFluxPerArea pfluxPerArea, double apertureArea) {
            pfluxName = pfluxPerArea.PFluxName;
            stdSpectrum = pfluxPerArea.StdSpectrum.MultiplyBy(apertureArea);
            this.apertureArea = apertureArea;
            isValid = stdSpectrum.IsValid && (apertureArea >= 0);
        } // constructor.

        // CONSTRUCTOR for new object, using old object except using new flux StandardizedSpectrum.
        public PFluxApertured (PFluxApertured pfluxApertured_Old, StandardizedSpectrum stdSpectrum_New) {
            pfluxName = pfluxApertured_Old.PFluxName;
            stdSpectrum = stdSpectrum_New.Clone();
            apertureArea = pfluxApertured_Old.ApertureArea;
            isValid = pfluxApertured_Old.IsValid && stdSpectrum_New.IsValid && apertureArea >= 0;
        } // constructor.

        // PROPERTIES.
        // (Additional properties are inherited from abstract class PFluxAbstract.)
        public double ApertureArea { get { return apertureArea; } }
        public double TotalPhotonFlux { get { return this.stdSpectrum.Integral; } }

        // METHODS.
        // IAperture does not apply to PFluxApertured.
        // IFilter covers classes: Filter, Passband, Atmosphere.
        // ... Passband and Atmosphere do not apply to PFluxApertured, however.
        // Consider restricting this to Filter (rather than to IFilter).
        public PFluxApertured Through(IFilter filter) {
            return filter.ActOn(this);
        }

        public double CountRate(IDetector detector) {
            return detector.CountRateFromPFlux(this);
        }
        public double InstrumentalMagnitude(IDetector detector) {
            return detector.InstrumentalMagnitudeFromPFlux(this);
        }

    } // class [PFluxApertured].

} // namespace.
