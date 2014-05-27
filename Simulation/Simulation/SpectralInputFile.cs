using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

//***** class SpectralInputFile.
//*****    Reads and parses an input file of spectral data, holds and serves that data on demand.
//*****    Eric Dose, Topeka, Kansas.
//*****    Written 2011 - 2012.
//*****    Passed all unit tests December 3, 2012.
//
//***** Proper SpectralInputFile always has format:
//*****    itemName [ must = same as file name minus ".txt" ]
//*****    header line(s) [count is given in call to constructor]
//*****    spectrum lines [at least two required even if not used; must be monotonically incr in nm]

namespace EVD.Astro.Spectral {
    public class SpectralInputFile {
        // FIELDS, private, with default values.
        string fullFilename = null;
        string[] headerDataLines;
        int nSpectrumPoints = 0;
        double[] nm_vector = null;
        double[] y_vector = null;
        bool isValid = false;

        // CONSTRUCTOR.
        public SpectralInputFile (string folderName, string itemName, int nHeaderLines) {
            //Console.WriteLine("entering SIF constructor, foldername >" + folderName + "<, itemName >" + itemName + "<");
            fullFilename = Find(folderName, itemName);
            //Console.WriteLine("\nfullFilename found >" + fullFilename + "<");
            if (fullFilename.Trim().Length == 0) { 
                isValid = false;
                return; 
            }

            List<double> nmList = new List<double>(); // Lists are easy to append.
            List<double> yList  = new List<double>();
            using (StreamReader sr = File.OpenText(fullFilename)) {
                string line = sr.ReadLine();  // skip the itemName line.
                line = sr.ReadLine();         // prime the loop.

                // read header data lines.
                headerDataLines = new string[nHeaderLines];
                int nHeaderLinesFound = 0;
                while ((line!=null) && (nHeaderLinesFound < nHeaderLines)) {
                    if (String.Compare(line, 0, "//", 0, 2) != 0) {
                        headerDataLines[nHeaderLinesFound] = String.Copy(line);
                        nHeaderLinesFound++;
                    }
                    //Console.WriteLine("#HeaderDataLines=" + nHeaderLinesFound);
                    line = sr.ReadLine();
                } // while.

                // read spectral data lines.
                nSpectrumPoints = 0;
                string[] rawTokens;
                string[] goodTokens = new string[2];
                while (line!=null) {
                    //Console.WriteLine("\nline read >" + line + "<");
                    if(String.Compare(line, 0, "//", 0, 2) != 0) {
                        rawTokens = line.Trim().Split(' ','\t',',');
                        int goodTokensFound = 0;
                        //Console.WriteLine("# rawTokens=" + rawTokens.Length);
                        //for(int i=0; i<Math.Min(2, rawTokens.Length); i++) {
                        for(int i=0; i<rawTokens.Length; i++) {
                            //if(nSpectrumPoints < 5) {
                            //    Console.WriteLine("trimmedRawToken>" + rawTokens[i].Trim() + "<");
                            //}
                            if(rawTokens[i].Trim().Length > 0) {
                                goodTokens[goodTokensFound] = rawTokens[i].Trim();
                                //Console.WriteLine("goodToken[" + goodTokensFound + "] = " + 
                                //    goodTokens[goodTokensFound]);
                                goodTokensFound++;
                            } // if rawTokens[i].
                            if(goodTokensFound >= 2) break;
                        } // for i.
                        //Console.WriteLine("finish >" + line + "< " + rawTokens.Length + " rawTokens, " + 
                        //    goodTokensFound + " good tokens.");
                        if(goodTokensFound >= 2) {
                            double nm, y;
                            bool nmConvertedOK = Double.TryParse(goodTokens[0], out nm);
                            bool yConvertedOK  = Double.TryParse(goodTokens[1], out y);
                            if((nmConvertedOK == false) || (yConvertedOK == false)) { isValid = false; return; }
                            nmList.Add(Convert.ToDouble(goodTokens[0]));
                            yList.Add(Convert.ToDouble(goodTokens[1]));
                            nSpectrumPoints++;
                        if (nSpectrumPoints >= 2) {
                               if(nmList[nSpectrumPoints-1] <= nmList[nSpectrumPoints-2]) {
                                   isValid = false; return;  // enforce: nm values are monotonically increasing.
                               }
                           }
                        } // if goodTokensFound.
                    } // if.
                    line = sr.ReadLine();
                } // while.    
            } // using sr.
            nm_vector = nmList.ToArray();
            y_vector  = yList.ToArray();
            //Console.WriteLine("end of SIF(): nSpectrumPoints = " + nSpectrumPoints);
            isValid = (nSpectrumPoints >= 4);
        } // constructor.

        // PROPERTIES.
        public string   FullFilename        { get { return (string)fullFilename.Clone(); } }
        public string[] HeaderDataLines { get { return (string[])headerDataLines.Clone(); } }
        public int      NSpectrumPoints { get { return nSpectrumPoints; } }
        public double[] Nm_Vector       { get { return (double[])nm_vector.Clone(); } }
        public double[] Y_Vector        { get { return (double[])y_vector.Clone(); } }
        public bool     IsValid         { get { return isValid; } }
                
        // METHODS.
        public static string Find(string folderName, string itemName) {
            DirectoryInfo d = new DirectoryInfo(folderName);
            FileInfo[] files = d.GetFiles("*.txt");
            foreach(FileInfo f in files) {
                using(StreamReader sr = File.OpenText(f.FullName)) {
                    string firstLine = sr.ReadLine();
                    if (firstLine!=null) {
                        if(String.Compare(firstLine.Trim() , itemName.Trim(),
                            StringComparison.OrdinalIgnoreCase) == 0) {
                            return f.FullName;
                        } // if compare.
                    } // if not null.
                } // using sr.
            } // foreach f.
            return ""; // zero-length string indicates file was not found.
        } // static .Find().
    } // class.
} // namespace.
