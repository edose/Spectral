using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using EVD.Astro.Spectral;

//***** class Atmosphere.
//*****    Represents a site and atmosphere. An AirPath is made from this object and a zenith angle.
//*****    Rewritten to use SMARTS program v. 2.9.5 to construct transmission spectra for AirPath objects.
//*****    Eric Dose, Topeka, Kansas
//*****    Split from child class AirPath December 29, 2012.
//*****    Passed all unit tests January 21, 2013.

namespace EVD.Astro.Spectral {
    public class Atmosphere {

        // The following was thought to be needed for running SMARTS program.
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private bool isValid;
        private bool isExoAtmospheric;
        private string siteName;
        private static string siteFolder = @"C:\Dev\Spectral\Data\Site\";
        private static string smartsFolder = @"C:\Astro\SMARTS\SMARTS_295_PC\";
        private static string smartsInputFilename = @"smarts295.inp.txt";
        private static string smartsExeFilename = @"smarts295bat.exe";
        private static string smartsOutputFilename = @"smarts295.ext.txt";
        // Site fields.
        private double latitudeDeg, elevationMeters, heightMeters;
        private int    pollutionModelCode;
        private string aerosolModelCode;
        // Weather fields (default values).
        private double airTempAtSiteC, relHumidityAtSitePct;
        private string seasonCode;
        private double meanDailyTempAtSiteC;
        private double precipWaterVaporAboveSiteCm;
        private double ozoneAbundSeaLevelAtmCm;
        private double visibilityKm;

        //readonly double E_BASIS_TO_MAG_BASIS = 1.085736204758129569; // conversion, e-based opt.dens. to magnitude.

        // CONSTRUCTOR from Site file. Unlike all other constructor files, this file
        //    contains site information & default weather information (some of which
        //    will generally be replace for a given run.
        public Atmosphere(string siteName) {
            isExoAtmospheric = false;
            this.siteName = siteName;
            // get visibility from atmosphere file.
            if(ReadSiteFile() == false)    { isValid = false; return; }
            if(AllInputsAreValid == false) { isValid = false; return; }
            isValid = true;
        } // .constructor().

        // EXO-ATMOSPHERE Constructor added & unit tested Feb 6, 2014.
        public Atmosphere () {
            isExoAtmospheric = true;
            siteName = "[No Atmosphere]";
            // the site information is generally meaningless, but values needed.
            latitudeDeg = 0;
            elevationMeters = 0;
            heightMeters = 0;
            pollutionModelCode = 0;
            aerosolModelCode = "";
            airTempAtSiteC = 0;
            relHumidityAtSitePct = 0;
            seasonCode = "";
            meanDailyTempAtSiteC = 0;
            precipWaterVaporAboveSiteCm = 0;
            ozoneAbundSeaLevelAtmCm = 0;
            visibilityKm = 0;
            isValid = true;  // this is a valid case.
        } // .constructor() for Exo-atmosphere case.

        private bool ReadSiteFile() {
            // format of Site file:
            //    SiteName
            //    Latitude/deg, Elevation/lambda, Height/lambda
            //    PollutionModelCode (integer 1-5, typically 1)
            //    AerosolModelCode (text, as accepted by SMARTS)
            //    ...and then these lines giving *default* weather conditions.
            //    AirTempAtSite/C, RelHumAtSite/%, SeasonString, AvgDailyAirTemp/C
            //    PrecipWaterVaporAboveSite/cm
            //    OzoneColumnarAbundanceAboveSite/(atm*cm)
            //    VisibilityAtSite/km (airport-style)
            //    ... thus *7* data lines are required (after the site name line).

            int nDataLinesRequired = 7;
            string fullFilename = SpectralInputFile.Find(siteFolder, siteName);
            if (fullFilename.Trim().Length == 0) return false;
            string[] dataLines= new string[nDataLinesRequired];
            using (StreamReader sr = File.OpenText(fullFilename)) {
                string line = sr.ReadLine(); // skip the siteName.
                line = sr.ReadLine();        // prime the reading loop.

                // Read text lines into a string array.
                int nDataLinesFound = 0;
                while((line!=null) && (nDataLinesFound < nDataLinesRequired)) {
                    if(String.Compare(line, 0, "//", 0, 2) != 0) {
                        dataLines[nDataLinesFound] = String.Copy(line);
                        nDataLinesFound++;
                    }
                    line = sr.ReadLine();
                } // while.
                if(nDataLinesFound != nDataLinesRequired) return false;
            } // using sr.

            // for each line, get good tokens and extract and store data from them.
            string[] goodTokens;
            goodTokens = ExtractGoodTokens(dataLines[0]);
            if(goodTokens.Length < 3) return false;
            bool convertedOK = Double.TryParse(goodTokens[0], out latitudeDeg);
            if(convertedOK == false) return false;
            double elevationKm, heightKm;
            convertedOK = Double.TryParse(goodTokens[1], out elevationKm);
            if(convertedOK == false) return false;
            elevationMeters = 1000.0 * elevationKm;
            convertedOK = Double.TryParse(goodTokens[2], out heightKm);
            if(convertedOK == false) return false;
            heightMeters = 1000.0 * heightKm;

            goodTokens = ExtractGoodTokens(dataLines[1]);
            if(goodTokens.Length < 1) return false;
            convertedOK = Int32.TryParse(goodTokens[0], out pollutionModelCode);
            if(pollutionModelCode < 1 || pollutionModelCode > 5) return false;

            goodTokens = ExtractGoodTokens(dataLines[2]);
            if(goodTokens.Length < 1) return false;
            aerosolModelCode = TrimAnySingleQuotes(String.Copy(goodTokens[0]));
            if(aerosolModelCode.Length < 2) return false;

            goodTokens = ExtractGoodTokens(dataLines[3]);
            convertedOK = Double.TryParse(goodTokens[0], out airTempAtSiteC);
            if(convertedOK == false) return false;
            convertedOK = Double.TryParse(goodTokens[1], out relHumidityAtSitePct);
            if(convertedOK == false) return false;
            seasonCode = TrimAnySingleQuotes(String.Copy(goodTokens[2]));
            if(String.Compare(seasonCode, "WINTER")!=0 && String.Compare(seasonCode, "SUMMER")!=0) return false;
            convertedOK = Double.TryParse(goodTokens[3], out meanDailyTempAtSiteC);
            if(convertedOK == false) return false;

            goodTokens = ExtractGoodTokens(dataLines[4]);
            convertedOK = Double.TryParse(goodTokens[0], out precipWaterVaporAboveSiteCm);
            if(convertedOK == false) return false;

            goodTokens = ExtractGoodTokens(dataLines[5]);
            convertedOK = Double.TryParse(goodTokens[0], out ozoneAbundSeaLevelAtmCm);
            if(convertedOK == false) return false;

            goodTokens = ExtractGoodTokens(dataLines[6]);
            convertedOK = Double.TryParse(goodTokens[0], out visibilityKm);
            if(convertedOK == false) return false;
  
            return true;
        } // .ReadSiteFile().

        private string[] ExtractGoodTokens (string s) {
            List<string> goodTokens = new List<string>(); // easier to append than an array;
            string[] rawTokens = s.Trim().Split(' ', '\t', ',');
            for (int i=0; i<rawTokens.Length; i++) {
                string candidateToken = rawTokens[i].Trim();
                if (candidateToken.Length > 0) {
                    goodTokens.Add(candidateToken);
                }
            }
            return goodTokens.ToArray();
        } // .ExtractGoodTokens().

        // Factory method for new AirPath object.
        public AirPath MakeAirPathAtZenithAngle (double zenithAngle) {
            // first, if exo-atmospheric case, return spectrum with transmission = 1 (100% transparent).
            // [this addition added and unit-tested February 6, 2014.]
            if (isExoAtmospheric == true) {
                return new AirPath(this, zenithAngle, new StandardizedSpectrum(1.0));
            }
            // for zenith angles > 90 deg or < -90 deg, return an invalid AirPath.
            if (Math.Abs(zenithAngle) > 90.0) {
                return new AirPath(this, zenithAngle, new StandardizedSpectrum(false));
            }
            // for valid zenith angles, store the input value but calculate airpath using its abs value.
            return new AirPath(this, zenithAngle, AtmosphericTransmissionAtZenithAngle(Math.Abs(zenithAngle)));
        } // .MakeAirPathAtZenithAngle().

        public AirPath[] MakeAirPathVector(double[] zenithAngles) {
            AirPath[] airPathVector = new AirPath[zenithAngles.Length];
            for(int i=0; i<airPathVector.Length; i++) {
                airPathVector[i] = this.MakeAirPathAtZenithAngle(zenithAngles[i]);
            } // for i.
            return airPathVector;
        } // static Spectral.MakeAirPathVector().

        private StandardizedSpectrum AtmosphericTransmissionAtZenithAngle(double zenithAngle) {
            // EVD kludge [1/20/2013]: since SMARTS software gives slightly unstable readings for 
            //    zenith angles below 0.5-1.0 degree (very near zenith), we don't send such zenith angles
            //    directly to SMARTS. Instead we send 1.0 degree and then correct our Standardized Spectrum
            //    for the cosine factor. This is a very small correction, but probably a responsible one.
            double minimumSmartsZenithAngle = 1.0; // must be 0 to 90, inclusive.
            double smartsZenithAngle;
            double skyZenithAngle = Math.Abs(zenithAngle);
            smartsZenithAngle = Math.Max(skyZenithAngle, minimumSmartsZenithAngle);

            WriteSmartsInputFile(smartsZenithAngle);    // generates input file smarts295.inp.txt
            RunSmarts295ToMakeExtFile();          // generates output file smarts295.ext.txt.
            
            // [Comment out this block whenever not needed for testing [added 1/20/2013].]
            // save input and 2 output files under names prefixed with zenithAngle.
            //{   string fullOldInputFilename = Path.Combine(smartsFolder, "smarts295.inp.txt");
            //    string fullOldExtFilename = Path.Combine(smartsFolder, "smarts295.ext.txt");
            //    string fullOldOutFilename = Path.Combine(smartsFolder, "smarts295.out.txt");
            //    string prefix = zenithAngle.ToString("#0.000deg.");
            //    string fullNewInputFilename = Path.Combine(smartsFolder, String.Concat(prefix, ".smarts295.inp.txt"));
            //    string fullNewExtFilename = Path.Combine(smartsFolder, String.Concat(prefix, ".smarts295.ext.txt"));
            //    string fullNewOutFilename = Path.Combine(smartsFolder, String.Concat(prefix, ".smarts295.out.txt"));
            //    File.Copy(fullOldInputFilename, fullNewInputFilename, true);
            //    File.Copy(fullOldExtFilename, fullNewExtFilename, true);
            //    File.Copy(fullOldOutFilename, fullNewOutFilename, true);
            //    Console.WriteLine("written: " + prefix);
            //} // end temporary code block.

            StandardizedSpectrum ssRaw = MakeRawStdSpectrumFromExtFile();  // from file smarts295.ext.txt.
            StandardizedSpectrum ssTrue;

            // EVD kludge 1/20/2013: if sky zenith angle < minimum, correct the spectrum.
            //    This is needed because SMARTS software is not very stable at zenith angles < 1 degree.
            if (smartsZenithAngle == skyZenithAngle) {
                ssTrue = ssRaw; // no correction done.
                //Console.WriteLine("   zenithAngle=" + zenithAngle.ToString("#0.000") + " no correction.");
            } // if.
            else {
                // here, perform correction because SMARTS was given a proxy zenithAngle for stability.
                double absorbanceCorrectionFactor = (1.0 / Math.Cos(skyZenithAngle*Math.PI/180.0)) 
                    / (1.0 / Math.Cos(smartsZenithAngle*Math.PI/180.0));
                double[] transmissionSmarts = ssRaw.Y_Vector;
                double[] transmissionTrue = new double[transmissionSmarts.Length];
                for (int i=0; i<transmissionTrue.Length; i++) {
                    double absorbanceSmarts  = -Math.Log10(transmissionSmarts[i]);
                    double absorbanceTrue = absorbanceSmarts * absorbanceCorrectionFactor;
                    transmissionTrue[i] = Math.Pow(10.0, -absorbanceTrue);
                } // for i (transmission-correction loop).
                ssTrue = new StandardizedSpectrum(ssRaw.Nm_low, ssRaw.Nm_high, transmissionTrue);
                //Console.WriteLine("   zenithAngle=" + zenithAngle.ToString("#0.000") 
                //    + " -> absorbanceCorrectionFactor=" + absorbanceCorrectionFactor.ToString()
                //    + " smartsZenithAngle=" + smartsZenithAngle.ToString("#0.000") + " deg.");
            } // else.
            return ssTrue;
        } // .AtmosphericTransmissionAtZenithAngle().

        private void WriteSmartsInputFile(double zenithAngle) {
            string fullFilename = Path.Combine(smartsFolder, smartsInputFilename);
            //Console.WriteLine("WriteSmartsInputFile::fullFilename >" + fullFilename + "<");
            using (FileStream fs = new FileStream(fullFilename, FileMode.Create, FileAccess.Write)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                // File structure is from SMARTS295_Users_Manual_PC.pdf.
                // Card 1 [comment; use underscores rather than spaces]:
                sw.WriteLine("'Site_" + siteName + ",_zA=" + zenithAngle.ToString("##0.000").Trim()
                    + "_deg_(Spectral.Atmosphere)'");
                // Cards 2 & 2a:
                sw.WriteLine("2");
                sw.WriteLine(latitudeDeg.ToString("##0.000") + " " 
                    + (elevationMeters/1000.0).ToString("#0.000") + " "
                    + (heightMeters/1000.0).ToString("#0.000"));
                // Cards 3 & 3a:
                sw.WriteLine("0");
                sw.WriteLine(airTempAtSiteC.ToString("##0.000") + " "
                    + relHumidityAtSitePct.ToString("##0.000") + " "
                    + "'" + seasonCode + "' "
                    + meanDailyTempAtSiteC.ToString("##0.000"));
                // Cards 4 & 4a:
                sw.WriteLine("0");
                sw.WriteLine(precipWaterVaporAboveSiteCm.ToString("#0.000"));
                // Cards 5 & 5a:
                sw.WriteLine("0"); sw.WriteLine("1  " + ozoneAbundSeaLevelAtmCm.ToString("#0.000"));
                // Cards 6 & 6a:
                sw.WriteLine("0"); sw.WriteLine(pollutionModelCode.ToString("#0"));
                // Card 7:
                sw.WriteLine("384 ! Card 7: CO2 ppmv for 2014");
                //Card 7a:
                sw.WriteLine("0");
                // Card 8:
                sw.WriteLine(aerosolModelCode);
                // Cards 9 & 9a:
                sw.WriteLine("4"); sw.WriteLine(visibilityKm.ToString("##0.000"));
                // Card 10:
                sw.WriteLine("51"); // backscatter='dry long grass'; no card 10a for this option.
                // Card 10b:
                sw.WriteLine("1");
                // Card 10c: Tilt angle must be *apparent* zenith angle, so to look directly at object.
                sw.WriteLine("51 " + (ApparentZenithAngle(zenithAngle)).ToString("#0.000") + " 180");
                // Card 11:
                sw.WriteLine("290 1310 1 1367");
                // Cards 12, 12a, 12b, 12c:
                sw.WriteLine("2"); sw.WriteLine("290 1310 0.5"); sw.WriteLine("7");
                sw.WriteLine("21 15 16 17 18 19 20");
                // Cards 13, 14, 15, & 16:
                sw.WriteLine("0"); sw.WriteLine("0"); sw.WriteLine("0"); sw.WriteLine("0");
                // Cards 17:
                sw.WriteLine("1"); 
                // Card 17a: SMARTS requires apparent elevation, not geometric/astronomic angle.
                sw.WriteLine((90.0-ApparentZenithAngle(zenithAngle)).ToString("#0.000") + " 180");
                } // using sw.
            } // using fs.

        } // .WriteSmartsInputFile().

        private void RunSmarts295ToMakeExtFile () {
            File.Delete(Path.Combine(smartsFolder, "smarts295.ext.txt")); // to prevent SMARTS error.
            File.Delete(Path.Combine(smartsFolder, "smarts295.out.txt")); //       "
            string fullFilename = Path.Combine(smartsFolder, smartsExeFilename);
            Process smarts = new Process();
            smarts.StartInfo.FileName = fullFilename;
            smarts.StartInfo.UseShellExecute = false;
            smarts.StartInfo.WorkingDirectory = smartsFolder; // so that working dir is local to .exe.
            Thread.Sleep(100);  // make sure files are deleted before starting process.
            //Console.WriteLine("\nstart RunSmarts295ToMakeExtFile::fullFilename >" + fullFilename + "<");
            smarts.Start();
            //Console.WriteLine("after RunSmarts295ToMakeExtFile::fullFilename >" + fullFilename + "<");
            Thread.Sleep(100);
            IntPtr p = smarts.MainWindowHandle;
            Thread.Sleep(100);
            ShowWindow(p, 1);
            Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");  // the Enter keypress that SMARTS expects when it's finished.
            //Console.WriteLine("after SendKeys.Send({ENTER})");
            Thread.Sleep(100);             // a chance to settle down (0.1->0.5 sec, on March 3 2014).
        } // .RunSmarts295ToMakeExtFile().

        private StandardizedSpectrum MakeRawStdSpectrumFromExtFile() {
            // read in .ext.txt file, then return standard spectrum.
            int nDataLinesRequired = 4;
            string fullFilename = Path.Combine(smartsFolder, smartsOutputFilename);
            //Console.WriteLine("smartsOutput >" + fullFilename + "<");

            List<double> nm_list = new List<double>();
            List<double> t_list  = new List<double>();
            string[] goodTokens;
            using (StreamReader sr = File.OpenText(fullFilename)) {
                string line = sr.ReadLine(); // skip the header line.
                line = sr.ReadLine();        // prime the reading loop.
                // reading loop.
                while (line!=null) {
                    goodTokens = ExtractGoodTokens(line);
                    if(goodTokens.Length >= 2) {
                        double nm, t;
                        bool convertedOK = Double.TryParse(goodTokens[0], out nm);
                        if(convertedOK == false) break;
                        convertedOK = Double.TryParse(goodTokens[1], out t);
                        if(convertedOK == false) break;
                        nm_list.Add(nm);
                        t_list.Add(t);
                        line = sr.ReadLine();
                    } // if.                
                } // while.            
            } // using sr.

            double[] nm_basis = nm_list.ToArray();
            double[] t_basis  = t_list.ToArray();
            //Console.WriteLine(nm_basis.Length.ToString() + " lines read from .ext file.");
            StandardizedSpectrum ss;
            if (nm_basis.Length>=nDataLinesRequired && t_basis.Length==nm_basis.Length) {
                ss = new StandardizedSpectrum(nm_basis, t_basis).ClipToMaxOf(1).ClipToMinOf(0);                
            } else {
                Console.WriteLine("XXX MakeRawStdSpectrumFromExtFile() returns SS(false)!");
                ss = new StandardizedSpectrum(false);
            }
            return ss;
        } // .MakeRawStdSpectrumFromExtFile().

        private string TrimAnySingleQuotes (string s_input) {
            string s = String.Copy(s_input);
            if(s.EndsWith("'")) s = s.Substring(0, s.Length-1);
            if(s.StartsWith("'")) s = s.Substring(1);
            return s;
        } // .TrimAnySingleQuotes().


        private double ApparentZenithAngle(double geometricZenithAngle) {
            // Method from Astronomical Almanac 2009, page B 8.
            // Note: fractionOfCorrection=0.95 added March 6 2014 for SMARTS295 stability.
            double T = airTempAtSiteC;
            double P = 1013.25 * Math.Exp(-elevationMeters/7996); // local pressure/millibars.
            double a = 90.0-geometricZenithAngle; // geometric altitude above horizon.
            double correction;
            if (geometricZenithAngle < 75.0) {
                // normal case, accurate to ca. 0.1 arcminutes below 75 deg zenith angle.
                correction = 0.00452 * P * (Math.Tan(geometricZenithAngle*Math.PI/180.0))/(273.15+T);
            } else {
                correction = P * (0.1594 + 0.0196*a + 0.00002*a*a)/((273.15+T)*(1+0.505*a+0.0845*a*a));           
            }
            double fractionOfCorrection = 0.95; // lessen correction; SMARTS295 is more stable this way.
            //Console.WriteLine(">>> " + fractionOfCorrection.ToString("#0.000000") + ", " +
            //    (geometricZenithAngle - fractionOfCorrection * correction).ToString("#0.000000"));
            return (geometricZenithAngle - fractionOfCorrection * correction);
        } // .ApparentZenithAngle().

        // PROPERTIES.
        public bool IsValid              { get { return isValid; } }
        public bool IsExoAtmospheric     { get { return isExoAtmospheric; } }
        public string SiteName           { get { return siteName; } }
        public static string SiteFolder   { get { return siteFolder; } }
        public static string SmartsFolder { get { return smartsFolder; } }
        public static string SmartsInputFilename { get { return smartsInputFilename; } }
        public static string SmartsExeFilename   { get { return smartsExeFilename; } }
        public static string SmartsOutputFilename { get { return smartsOutputFilename; } }
        // for site fields.
        public double Latitude           { get { return latitudeDeg; } }
        public double Elevation          { get { return elevationMeters; } }
        public double Height             { get { return heightMeters; } }
        public int    PollutionModelCode { get { return pollutionModelCode; } }
        public string AerosolModelCode   { get { return aerosolModelCode; } }
        // for weather fields.
        public double AirTemperatureAtSite {
            get { return airTempAtSiteC; }
            set { airTempAtSiteC = value; }
        }
        public double RelativeHumidity {
            get { return relHumidityAtSitePct; }
            set { relHumidityAtSitePct = value; }
        }
        public string SeasonCode {
            get { return String.Copy(seasonCode); }
            set { seasonCode = String.Copy(value); }
        }
        public double MeanDailyTemperatureAtSite {
            get { return meanDailyTempAtSiteC; }
            set { meanDailyTempAtSiteC = value; }
        }
        public double PrecipWaterVaporAboveSite {
            get { return precipWaterVaporAboveSiteCm; }
            set { precipWaterVaporAboveSiteCm = value; }
        }
        public double OzoneAbundance {
            get { return ozoneAbundSeaLevelAtmCm; }
            set { ozoneAbundSeaLevelAtmCm = value; }
        }
        public double Visibility {
            get { return visibilityKm; }
            set { visibilityKm = value; }
        }
        public bool AllInputsAreValid    { get {
            // All limits are within limits placed by SMARTS 2.9.5 (see its manual).
            bool allValid = true; // default
            if(String.IsNullOrEmpty(siteName) == true)         allValid = false;
            if(Math.Abs(latitudeDeg) > 90.0)                   allValid = false;
            if(elevationMeters < -500.0 || 
                elevationMeters > 8000.0)                      allValid = false;
            if(heightMeters < -10.0 || heightMeters > 100.0)   allValid = false;
            if(pollutionModelCode < 1 || 
                pollutionModelCode > 5)                        allValid = false;
            if(airTempAtSiteC < -100.0 || 
                airTempAtSiteC > 50.0)                         allValid = false;
            if(relHumidityAtSitePct > 100.0 || 
                relHumidityAtSitePct < 0.0)                    allValid = false;
            if(String.Compare(seasonCode, "WINTER")!=0 && 
                String.Compare(seasonCode, "SUMMER")!=0)       allValid = false;
            if(meanDailyTempAtSiteC < -100.0 ||
                meanDailyTempAtSiteC > 50.0)                   allValid = false;
            if(precipWaterVaporAboveSiteCm < 0.0 || 
                precipWaterVaporAboveSiteCm > 12.0)            allValid = false;
            if(ozoneAbundSeaLevelAtmCm < 0.0 || 
                ozoneAbundSeaLevelAtmCm > 1.0)                 allValid = false;
            if(visibilityKm < 1.0 || visibilityKm > 600.0)     allValid = false;
            return allValid;
            } 
        } // .AllInputsAreValid property.
        
    } // class Atmosphere.

    //***** class AirPath.
    //*****    Represents a site and atmosphere through which light must travel at a zenith angle.
    //*****    Generates StandardizedSpectrum object for optical transmission at a given zenithAngle.
    //*****    May only be constructed from parent Atmosphere object via .MakeAirPathAtZenithAngle().
    //*****    Eric Dose, Topeka, Kansas
    //*****    Split from parent class Atmosphere December 29, 2012.
    //*****    Passed all Unit Tests December 29, 2012.

    public class AirPath {
        private bool isValid;
        StandardizedSpectrum stdSpectrum;
        double zenithAngle;
        // FIELDS. We duplicate these for safety, in case parent atmosphere object is destructed, etc.
        // Site fields.
        private string siteName;
        private double latitudeDeg, elevationMeters, heightMeters;
        private int    pollutionModelCode;
        private string aerosolModelCode;
        // Weather fields (default values).
        private double airTempAtSiteC, relHumidityAtSitePct;
        private string seasonCode;
        private double meanDailyTempAtSiteC;
        private double precipWaterVaporAboveSiteCm;
        private double ozoneAbundSeaLevelAtmCm;
        private double visibilityKm;

        // CONSTRUCTOR.
        internal AirPath (Atmosphere atm, double zenithAngle_path, StandardizedSpectrum stdSpectrum_input) {
            stdSpectrum = stdSpectrum_input.Clone();
            this.siteName = String.Copy(atm.SiteName);
            this.zenithAngle = zenithAngle_path;
            this.latitudeDeg = atm.Latitude;
            this.elevationMeters = atm.Elevation;
            this.heightMeters = atm.Height;
            this.pollutionModelCode = atm.PollutionModelCode;
            this.aerosolModelCode = atm.AerosolModelCode;
            this.airTempAtSiteC = atm.AirTemperatureAtSite;
            this.relHumidityAtSitePct = atm.RelativeHumidity;
            this.seasonCode = String.Copy(atm.SeasonCode);
            this.meanDailyTempAtSiteC = atm.MeanDailyTemperatureAtSite;
            this.precipWaterVaporAboveSiteCm = atm.PrecipWaterVaporAboveSite;
            this.ozoneAbundSeaLevelAtmCm = atm.OzoneAbundance;
            this.visibilityKm = atm.Visibility;

            //Console.WriteLine("atm.IsValid(" + this.zenithAngle.ToString("##0.00") + ")= " + atm.IsValid);
            //Console.WriteLine("stdSpectrum.IsValid(" + 
            //    this.zenithAngle.ToString("##0.00") + ")= " + stdSpectrum.IsValid);
            this.isValid = atm.IsValid && stdSpectrum.IsValid;
        } // Constructor.

        public PFluxPerArea ActOn(PFluxPerArea pfluxIn) {
            return new PFluxPerArea(pfluxIn, pfluxIn.StdSpectrum.MultiplyBy(stdSpectrum));
        } // .ActOn().        

        public double Y(int index) { return stdSpectrum.Y(index); }
        
        // PROPERTIES. We duplicate these for safety, in case parent atmosphere object is destructed, etc.
        public bool IsValid { get { return isValid; } }
        public string SiteName { get { return siteName; } }
        public StandardizedSpectrum StdSpectrum { get { return stdSpectrum.Clone(); } }
        public double[] Y_Vector { get { return (double[])stdSpectrum.Y_Vector.Clone(); } }
        // for site fields.
        public double Latitude { get { return latitudeDeg; } }
        public double Elevation { get { return elevationMeters; } }
        public double Height { get { return heightMeters; } }
        public int    PollutionModelCode { get { return pollutionModelCode; } }
        public string AerosolModelCode { get { return aerosolModelCode; } }
        public double ZenithAngleDegrees { get { return zenithAngle; } }
        // for weather fields.
        // These are read-only here as they are set by Atmosphere and then immutable.
        public double AirTemperatureAtSite { get { return airTempAtSiteC; } }
        public double RelativeHumidity { get { return relHumidityAtSitePct; } }
        public string SeasonCode { get { return String.Copy(seasonCode); } }
        public double MeanDailyTemperatureAtSite { get { return meanDailyTempAtSiteC; } }
        public double PrecipWaterVaporAboveSite { get { return precipWaterVaporAboveSiteCm; } }
        public double OzoneAbundance { get { return ozoneAbundSeaLevelAtmCm; } }
        public double Visibility { get { return visibilityKm; } }

    } // class AirPath.

} // namespace.
