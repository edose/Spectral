Spectral
===============================
*Eric Dose :: Bois d'Arc Observatory, Kansas  ::  June 2014*

This repository supports my June 14, 2014 presentation *Toward Millimagnitude Photometric Calibration* before the SAS (Society for Astronomical Science) 2014 conference in Ontario, California, USA.

Contents
--------------------
This repository provides:

- simulation code (in C#, file extension .cs) for multiplying photon flux spectra and then producing for each combination of light source, atmospheric conditions, airmass, telescope, filter, and detector: (1) a computed Instrumental Magnitude, and (2) an expected exo-atmospheric magnitude within a bandpass. [directory "Simulation"]
- statistical code (in R, file extension .R) for comparing the Instrumental Magnitudes and bandpass magnitude, using a specific model formula. This is the calibration phase, and the intent is to simulate the extraction of photometric values for: Zero-point, extinction, and system transform. [directory "Statistics"]
- a few input and output data files as examples [directory "Data"]
- the slides shown during the June 2014 presentation [file: SAS Presentation 2014.pdf]

The accompanying paper is copyrighted and will be published in the *Proceedings for the 33rd Annual Conference of the Society for Astronomical Sciences*, probably available August 2014 via http://www.socastrosci.org/publications.html .

Disclosure of my own attitudes/biases:

- Nothing beats experimental data properly planned, skillfully taken, honestly analyzed. I'll count observation as experimentation, especially astronomical observation including photometry.
- For some systems, experiments are possible but too hard, expensive, or disruptive, as in social sciences, economics, and space travel planning. Computational simulation can get you a shortcut to learning something anyway.
- Other systems are just too inaccessible for experiments, for example, how a supernova starts. Or how cosmological structure developed over 15 billion years. Simulations can help here, if only to decide what observations to make.
- Finally, there's this project's case: one can and should do experiments to verify a model's description of reality, but alas current technology just can't give data that's good enough. In deciding on photometric calibration models, we want to know how best to collect and model photometric data, but real data has too much noise to make these decisions generally. What's needed is a way to compute how systems would work with noise levels below what's possible experimentally, then to add the noise back in--in this case noise of various kinds--to determine accurately the effects of model choices under realistic conditions. It's also true that computation is much quicker than experiment--well, if you don't count years of software development.

If you plan to examine the code...
--------------------------------------

**The C# code (directory "Simulation")** performs two simulations: (1) Simulation of photon flux passing through an optical stack (atmosphere, telescope, etc), and (2) simulation of photon flux passing through a reference passband. Both simulations begin with and operate on photon flux (not energy flux) per wavelength per unit time, and both simulations result in a magnitude. But the first simulation models a physical optical stack, and the second simply models the effect of a reference passband on the photon flux.

As given in this repository, the simulation code is actually invoked through unit test functions run by NUnit--typically I would edit the test or simulation code in my right monitor and run NUnit to produce text files of data in my left monitor. An extremely fast way to work. The simulation engine code is in repository directory CS_code/Simulation, and the calling NUnit test code is in repository directory CS_code/UnitTest.

There's no main C# program to run--I ran everything through NUnit. However, as all the C# code offered here is packaged as classes or static methods, it would be simple to write a main program simply constructing the needed objects and doing computations simply as a string of function calls.

**The R code (directory "Statistics")** is extremely dense, as R code tends to be. R treats entire matrices, data frames, and vectors as single variables, so a single line of code can do a lot. R also has wonderful graphics; in this work I use the inspiring *ggplot2* package. There's also no main R program to run (R doesn't have them)--I ran everything as function calls typed into the 100% excellent RStudio environment. I can't recommend R enough--not only for final statistics (for which is is the world reference standard, outside the US at least), but also for all the data cleaning, selecting, and just plain munging required to get the data ready in the first place. In R there's no rift between exploratory coding and production coding--you play around with the data in RStudio's interactive command line view until the data does what you need, then you cut and paste into a function that you can run over and over. Brilliant. 

Have a look at the R code. If a function call isn't obvious, check its online documentation. With that, most programmers will figure out my R code in about 2 minutes.

By the way, all the software needed for this project was free: Visual Studio for C#, NUnit for unit testing, Notepad++ for text management, the R framework, RStudio as R control panel, ggplot2 for R graphics, all R statistics packages, GitHub for this repository, even web site rst.ninjs.org to construct the reStructuredText documentation file you're reading now. I didn't pay for software until I wrote the final paper and made the final presentation.

*Note: I do not plan to refine or extend this code anytime soon. You are welcome to use the code in any manner consistent with the license (file "LICENSE" of this repository) which basically says Be Fair.*

1. Optical stack simulation:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

We think of an optical stack as having an astronomical light source outside our atmosphere, optionally a reflector, then the atmosphere, then a telescope, then optionally a filter, and finally a detector.

With C#'s being a true object-oriented, strongly encapsulated language, it is safe to invoke the thousands of lines of code with this call: ::

            # from Statics.cs, MakeCountsVector()
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

Notes:

- **star** is a stellar light source, which is represented as photon flux at wavelengths from 300 nm to 1300 nm [an object of C# class PFluxPer Area].
- **reflector** is used when the observed light source is a reflector of stellar light, for example, an asteroid. A reflector has its own reflectance spectrum, operationally identical to a filter [object of class Filter].
- **airPaths** is a previously computed vector of atmospheric transmission spectra at user-specified airmasses. Computation of the atmospheric transmission spectra is performed by SMARTS2 software, which is invoked by the C# code in 3_Atmosphere.cs. This atmospheric simulation was by far the most difficult part of the optical stack simulation to get right, even with the SMARTS2 software, and it takes > 95% of the simulation computing time [array of objects of class AirPath, which are produced by a factory method of class Atmosphere].
- **filterAtScopeFront** simulates covering the front of the scope with a filter material. I've never used this, rather nullified its presence by simply using an object with transmission=1 at all wavelengths [object of class Filter].
- **telescope** object performs two functions: (1) acts as an optical filter, and (2) transforms the flux-per-area incoming photon flux [object of class PFluxPerArea] to an absolute photon flux [object of class PFluxApertured], multiplying the first flux by the telescope's aperture area in a factory method that delivers the PFluxApertured object [object of class Telescope].
- **filterAtDetectorFront** simulates the usual filter (e.g., Johnson V) between the telescope and detector. This is the main tool for coercing the system spectrum to be as close as possible to the target passband spectrum [object of class Filter].
- **detector** simulates the detector as a perfect photon counter behind a filter with a transmission spectrum identical to the quantum efficiency of the actual detector. The output is in counts per second, which is easily converted to the required Instrumental Magnitude [object of class Detector].

2. Passband magnitude simulation:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

After the optical stack simulation, the passband simulation is easy: multiply the light source's photon flux spectrum [object of class PFluxArea] by the reference passband spectrum [object of class Passband], and normalize against star Vega defined as magnitude 0 in the same passband.

3. Statistics:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Description of R code & mixed-model regression go here.

Project History *(so far)*
----------------------------

In December 2007, the vagaries of corporate life and caused me to leave Chicago and visual observing to over-winter in beautiful but woefully cloudy Connecticut. With no sky to observe, and over numerous pots of coffee, I scoured a copy of Brian Warner's new *A Practical Guide to Lightcurve Photometry and Analysis* several times, cover to cover. However brilliantly Brian described the range of current practices, I didn't get it. I was sure there had to be a unified approach, a master model equation and ideal data set design from which one could choose a subset satisfying one's own need. I started coding. I imagined it would take a month or two.

Almost six years later, in late 2013, I had only the barest skeleton of a unified photometric calibration approach, but I decided that I'd better start organizing to present what I had. The June 2014 presentation and this repository are the tangible fruits. The *intangible* fruits are the work yet to do--and it's a lot.



Conclusions
-----------

[Conclusions from the SAS paper go here.]

Side-trip: Coding Language
-----------------------------

I'm already getting dragged kicking and screaming (backwards) into the Python era. The way to get things done now is by collaboration, and Python is apparently the language of (rather poor) choice. It lacks encapsulation, which makes it fake-object-oriented and perfect for API abuse. It's suitable for 10-line scripts if that's all you want to do, but a 3,000-line system becomes hopeless, turns into one huge mud flat. Python is fingernails-scraping-a-blackboard slow, and if it weren't for Cython, I wouldn't bother. At least PyCharm and py.test make for an organized and visually soothing IDE, so OK that's something good.

In the end, I guess Python's the worst language except for most of the others. There's this rebellion out there in favor of free and open-source, and against corporation-driven tools. I can sympathize and have definitely benefited from the movement, even within this project. But there are casualties, and to me it goes too far. C#--a truly excellent technical programming language--is now right out, and even Java--which is just as cross-platform as Python, cleanly object-oriented, and 20-100 times faster than Python--is apparently too closely tied to one company's fate. Although I must note that many of these same people go all Derp-trance over Apple, the most closed and corporate environment of them all. Python is dependent on a Benevolent Dictator for Life, and Python True Believers consider this a feature, not a bug. The satire writes itself.

C# beautifully satisfies the scientific community's most common need for objects: a data block that answers for itself. You can construct a block of data that computes and returns any number of its own properties, and you can code it such that no one can screw it up even if they try to. If you link to it, it gives the right answer. Period. You have to compile it in, yes--but hey if you want to use Python for anything more than 2+2 or Shopping Carts, you have to compile Cython anyway, so...

R is underused in the scientific world. The statisticians and social scientists have all the fun (and that's probably the first time *that* sentence has ever been written).


2014-2015 Plans at Bois d'Arc Observatory
------------------------------------------

[extension to experimental data; Project Cirrus/mixed-model regression]
