using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using EVD.Astro.Spectral;

//***** class .
//*****    Photon Flux per unit wavelength, either per area or absolute (through aperture of known area).
//*****    Eric Dose, Topeka, Kansas.
//*****    Written 2011 - 2012.
//*****    Passed all unit tests January 20, 2013.

namespace EVD.Astro.Spectral {
    public static class Spectral {

        public static double[] MakeCountsVector (
            PFluxPerArea star, Filter reflector,
            AirPath[] airPaths, Filter filterAtScopeFront,
            Telescope telescope, Filter filterAtDetectorFront, Detector detector,
            double seconds) {
            
            double[] countsVector = new double[airPaths.Length];
            // first, test all inputs, return zeroes if any is invalid.
            bool allInputsAreValid = 
                star.IsValid && reflector.IsValid;
            //Console.WriteLine("before=" + allInputsAreValid.ToString());
            foreach(AirPath ap in airPaths) allInputsAreValid &= ap.IsValid;
            //Console.WriteLine("after =" + allInputsAreValid.ToString());
            allInputsAreValid &= filterAtScopeFront.IsValid && telescope.IsValid 
                && filterAtDetectorFront.IsValid && (seconds >= 0.0);
            //Console.WriteLine("star.IsValid=" + star.IsValid.ToString() +
            //    " reflector.IsValid=" + reflector.IsValid.ToString() +
            //    " filterAtScopeFront.IsValid=" + filterAtScopeFront.IsValid.ToString() +
            //    " telescope.IsValid=" + telescope.IsValid.ToString() +
            //    " filterAtDetectorFront.IsValid=" + filterAtDetectorFront.IsValid.ToString() +
            //    " seconds=" + seconds.ToString());
            //Console.WriteLine("allInputsAreValid = " + allInputsAreValid.ToString());
            if(allInputsAreValid == false) return countsVector; // with default values of zero.
            
            // from here, all inputs are valid, so compute vector of counts.
            for(int i = 0; i<airPaths.Length; i++) {
                countsVector[i] = 
                    star
                    .Through(reflector)
                    .Through(airPaths[i])
                    .Through(filterAtScopeFront)
                    .Through(telescope)
                    .Through(filterAtDetectorFront)
                    .CountRate(detector)
                    * seconds;
            } // for i.
            return countsVector;
        } // static Spectral.MakeCountsVector().

        //public static double GetCountsExoAtmosphere(
        //    PFluxPerArea star, Filter reflector,
        //    Filter filterAtScopeFront,
        //    Telescope telescope, Filter filterAtDetectorFront, Detector detector,
        //    double seconds) {

        //    // first, test all inputs, return zeroes if any is invalid.
        //    bool allInputsAreValid = 
        //        star.IsValid && reflector.IsValid;
        //    allInputsAreValid &= filterAtScopeFront.IsValid && telescope.IsValid 
        //        && filterAtDetectorFront.IsValid && (seconds >= 0.0);
        //    if(allInputsAreValid == false) return 0.0; // default value of zero.

        //    // from here, all inputs are valid, so compute vector of counts.
        //        double counts = 
        //            star
        //            .Through(reflector)
        //            .Through(filterAtScopeFront)
        //            .Through(telescope)
        //            .Through(filterAtDetectorFront)
        //            .CountRate(detector)
        //            * seconds;
        //    return counts; // scalar, since zenithAngles meaningless when exoatmosphere.
        //} // static Spectral.GetCountsExoatmosphere().


    } // static class Spectral.

} // namespace.
