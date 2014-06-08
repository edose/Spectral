Spectral
===============================
*Eric Dose :: Bois d'Arc Observatory, Kansas  ::  June 2014*

This repository supports my June 13, 2014 presentation *Toward Millimagnitude Photometric Calibration* before the SAS (Society for Astronomical Science) 2014 conference in Ontario, California, USA.

Contents
--------------------
This repository provides:

- simulation code (in C#, file extension .cs) for multiplying photon flux spectra and then producing for each combination of light source, atmospheric conditions, airmass, telescope, filter, and detector: (1) a computed Instrumental Magnitude, and (2) an expected exo-atmospheric magnitude within a bandpass. [directory "Simulation"]
- statistical code (in R, file extension .R) for comparing the Instrumental Magnitudes and bandpass magnitude, using a specific model formula. This is the calibration phase, and the intent is to simulate the extraction of photometric values for: Zero-point, extinction, and system transform. There could also be higher-order terms, and if mixed-model regression is used there will be "random effects" that account for and remove certain systematic errors [directory "Statistics"]
- a few input and output data files as examples [directory "Data"]
- the slides shown during the June 2014 presentation [in PDF format, file: SAS Presentation 2014.pdf]

The accompanying paper is copyrighted and will be published in the *Proceedings for the 33rd Annual Conference of the Society for Astronomical Sciences*, probably available August 2014 via http://www.socastrosci.org/publications.html .

Disclosure of my own attitudes/biases:

- Nothing beats experimental data properly planned, skillfully taken, honestly analyzed. Here observation will serve as experimentation, especially astronomical observation including photometry.
- For some systems, experiments are possible but too hard, expensive, or disruptive, as in social sciences, economics, and space travel planning. Computational simulation can get you a shortcut to learning something anyway, and to doing better experiments when that time comes.
- Other systems are just too inaccessible for experiments, for example, how a supernova starts. Or how cosmological structure developed over 15 billion years. Simulations can help here, if only to decide what observations to make.
- Finally, there's this project's case: one can and should do experiments to verify a model's description of reality, but alas current technology just can't give data that's good enough. In deciding on photometric calibration models, we want to know how best to collect and model photometric data, but real data has too much noise to make these decisions generally. What's needed is a way to compute how systems would work with noise levels below what's possible experimentally, then to add the noise back in--in this case noise of various kinds--to determine accurately the effects of model choices under realistic conditions. It's also true that computation is much quicker than experiment--well, if you don't count years of software development.

If you plan to examine the code...
--------------------------------------

**The C# code (directory "Simulation")** performs two simulations: (1) Simulation of photon flux passing through an optical stack (atmosphere, telescope, etc), and (2) simulation of photon flux passing through a reference passband. These parallel simulations begin with and operate on photon flux (not energy flux) per wavelength per unit time, and both simulations result in a magnitude. But the first simulation models a physical optical stack, and the second simply models the effect of a reference passband on the photon flux.

As given in this repository, the simulation code is actually invoked through unit test functions run by NUnit--typically I would edit the test or simulation code in my right monitor and run NUnit to produce text files of data in my left monitor. An extremely fast way to work. The simulation engine code is in repository directory CS_code/Simulation, and the calling NUnit test code is in repository directory CS_code/UnitTest.

There's no main C# program to run, here--I ran everything through NUnit. However, as all the C# code offered here is packaged as classes or static methods, it would be simple to write a main program simply constructing the needed objects and doing computations simply as a string of function calls.

**The R code (directory "Statistics")** is extremely dense, as R code tends to be. R treats entire matrices, data frames, and vectors as single variables, so a single line of code can do a lot. R also has wonderful graphics; in this work I use the inspiring *ggplot2* package. There's also no main R program to run (R doesn't have them)--I ran everything as function calls typed into the 100% excellent RStudio environment. I can't recommend R enough--not only for final statistics (for which is is the world reference standard, outside the US at least), but also for all the data cleaning, selecting, and just plain munging required to get the data ready in the first place. In R there's no rift between exploratory coding and production coding--you play around with the data in RStudio's interactive command line view until the data does what you need, then you cut and paste into a function that you can run over and over. Brilliant. 

Have a look at the R code. If a function call isn't obvious, check its online documentation. With that, most programmers will figure out my R code in about 2 minutes.

By the way, all the software needed for this project was free: Visual Studio for C#, NUnit for unit testing, Notepad++ for text management, the R framework, RStudio as a R control panel, all R statistics packages, ggplot2 for R graphics and Inkscape for vector graphics post-processing, SourceTree for git repository management, GitHub web site for this repository, and even web site rst.ninjs.org to construct the reStructuredText documentation file you're reading now. I didn't pay for software until I wrote the final paper and made the final presentation.

*Note: I do not plan to refine or extend this code anytime soon. You are welcome to use the code in any manner consistent with the license (file LICENSE of this repository) which basically says Be Fair.*

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

- **star** is a stellar light source, which is represented as photon flux at wavelengths from 300 nm to 1300 nm [an object of my C# class PFluxPerArea].
- **reflector** is used when the observed light source is a reflector of stellar light, for example, an asteroid. A reflector has its own reflectance spectrum, operationally identical to a filter [object of class Filter].
- **airPaths** is a previously computed vector of atmospheric transmission spectra at user-specified airmasses. Computation of the atmospheric transmission spectra is performed by SMARTS2 software, which is invoked by the C# code in 3_Atmosphere.cs. This atmospheric simulation was by far the most difficult part of the optical stack simulation to get right, even with the SMARTS2 software, and it takes at least 95% of the simulation computing time [array of objects of class AirPath, which are produced by a factory method of class Atmosphere].
- **filterAtScopeFront** simulates covering the front of the scope with a filter material. I've never used this, rather nullified its presence by simply using an object with transmission=1 at all wavelengths [object of class Filter].
- **telescope** object performs two functions: (1) acts as an optical filter, and (2) transforms the flux-per-area incoming photon flux [object of class PFluxPerArea] to an absolute photon flux [object of class PFluxApertured], multiplying the first flux by the telescope's aperture area in a factory method that delivers the PFluxApertured object [object of class Telescope].
- **filterAtDetectorFront** simulates the usual filter (e.g., Johnson V) between the telescope and detector. This is the main tool for coercing the system spectrum to be as close as possible to the target passband spectrum [object of class Filter].
- **detector** simulates the detector as a perfect photon counter behind a filter with a transmission spectrum identical to the quantum efficiency of the actual detector. The output is in counts per second, which is easily converted to the required Instrumental Magnitude [object of class Detector].

2. Passband magnitude simulation:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

After the optical stack simulation, the passband simulation is easy: multiply the light source's photon flux spectrum [object of class PFluxArea] by the reference passband spectrum [object of class Passband], and normalize against star Vega defined as magnitude 0 in the same passband.

3. Statistics:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

All statistical code is in the R language. I almost always ran code as a function call from RStudio's Console window, the code itself showing in the window immediately above that. That's it.

(By the way, C, Java, and python folks, here's a R quirk: within R variable names like s.image, the period has no significance, it's only another character like underscore.)

Some of the files:

**Experiments.txt:** summary log of the experiments and very terse results from experiment blocks 001-013 supporting the SAS paper. Your guidebook to this work.

**platform.R:** Ignore get.cs(), it's not used. The function SAS2014.grey.theme() was used to define the graphics theme for R graphics included in the presentation file.

**plot_001a.R:** a very typical example plot function plot_001a(), which used ggplot2 and platform.R to construct a simple color index plot, then save it as a vector file to be edited in the wonderful Inkscape open-source vector-graphics editing program.

**001c_cs.txt:** a text-file copy of the C# unit test (NUnit) code that generated the simulation output file **001c_rawflux.txt**, which served as input to plot function plot_001c() in file **001c_plot.R**. A bit baroque I guess--makes more sense in a repeated workflow.

**002a_plot.R**, **002b_plot.R**, **003_plotmags.R**, **005_plotmagsBV.R:** typical plot functions using the wonderful ggplot2 package.

And I'll include the entire 010-013 set of files (see file **Experiments** for a log of these runs). It's a lot of near-repetition, but that's what this project was like. I kept all the individual runs partly for the sake of ensuring reproducibility, but mostly to ease later debugging, though so far none has been needed after the fact.

If you're looking at the code, you'll see that two functions are key to many of the R code files: lm() and lmer(). Function lm() is linear regression, classical, much more capable than I've needed so far. Function lmer(), though, is the piece de resistance: mixed-model regression. Please don't ask me to explain it, rather have a look at https://en.wikipedia.org/wiki/Mixed_model . It is wonderful. It allows a dependent variable (here, Instrument Magnitude) to be described by both "fixed effects" [which are just standard predictors as in the function lm()] and "random effects" which allow for the extraction of pseudo-random *that is shared among subsets* of the data points, for example, it can extract noise shared by all photometric targets in each image, as when shutter timing is erratic.

4. Data:
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
In the *Data* directory are example files used as input to the simulations. I've left out certain filters to stay clear of copyright issues; others I've left in as "fair use", and I very much doubt the vendors will complain about my including their data with their company name as part of the file names. I hope what's here gives you a healthy start; suitable spectral data are tedious but not impossible to find, and you only have to find them once.

Project History *(so far)*
----------------------------

In December 2007, certain weird turns in corporate life caused me to leave Chicago to over-winter in beautiful but tragically cloudy Connecticut. With no sky to observe, I scoured a copy of Brian Warner's new *A Practical Guide to Lightcurve Photometry and Analysis* several times, cover to cover. However many pots of coffee, and however brilliantly Brian described the range of current practices, I didn't get it. I was sure there had to exist a unified approach, some master model formula and ideal data set design from which one could choose a subset sufficient to one's own need. I started exploratory coding. I imagined it would take a month or two. It took six years.

Late 2013, I still had only the barest skeleton of a unified photometric calibration approach, but it was robust, it linked well with R, and I decided it was time to present the world what I had. The June 2014 presentation and this repository are the tangible fruits. Whereas the *intangible* fruits are all the work I have yet to do on this--and it's a lot.

At this writing--before the SAS conference and the critiquing I'm sure this work will cause--the greatest promise for continuing this work is actually in a side-effect I found. It proved possible to extract most of the per-image noise I had added as a "random effect" in mixed-model regression. So if there is a shutter timing problem, or more realistically if there is thin cirrus moving across the field of view over long exposures, most of this error can be removed. It's the kind of thing that ensemble comp stars are supposed to solve, but this is a much more elegant and robust approach. One can plot this per-image "random effect" as a variable over time that can yield a data QC check. Wow. I think that validating this with experimental photometric data is a separate project for the coming year. Call it **Project Cirrus**.

Conclusions
-----------

Have a look at the last slides in the presentation PDF. No point in duplicating them here.

Side-trip: Coding Languages
-----------------------------

I'm already getting dragged kicking and screaming (backwards) into the Python era. The way to get things done now is by collaboration, and Python is apparently the language of (rather poor) choice. 

**Python** utterly lacks encapsulation, which makes it fake-object-oriented and perfect for API abuse. It lacks typing or compilation, which is great for  script kiddies makes it run fingernails-scraping-a-blackboard slowly. A 3,000-line system becomes hopeless. Python is supposed to be beautifully formatted, but whatever, that's only a matter of taste--it's like judging one's photometric data by the decor inside your cold room.

If it weren't for Cython, I wouldn't bother. At least PyCharm and py.test make for an organized and visually soothing IDE, so OK that's something good. 

In the end, I guess Python's the worst scientific coding language except for 1-2 others. There's this tantrum out there in favor of free and open-source, and against corporation-driven tools. I can sympathize and have definitely benefited from the movement, even within this project. But the perpetrators pointedly ignore the casualties, and to me it goes too far. C#--a truly excellent technical programming language--is now considered the plague. Even Java--which is just as cross-platform as Python ever was, is cleanly object-oriented, and is 20-100 times faster than Python--even Java is apparently too closely tied to one company's fate to be considered. Of course, the Pythonistas (their own term) ignore their own Derp-trance over Apple, the most closed and corporate environment of them all. These free-thinking coders insist you follow the religious rules, and code that does is "Pythonic". Python started just after 1984 and depends completely on a Benevolent Dictator for Life, and always has done--which Python True Believers consider a feature, not a bug. It has always been at war with Java, Brother. The satire writes itself.

**C#** and even Java beautifully satisfy the scientific community's most common need for objects: a data block that answers for itself. You can construct a block of data that computes and returns any number of its own properties, and you can code it such that no one can screw it up even if they try to. If you link to some tested code, it will give the right answer. Period. You have to compile it in, yes--but hey if you want to use Python for anything more scientific than dancing-baby GIFs or Shopping Carts, you have to compile Cython code anyway, so where's the advantage.

Python's advantage is in sharing code. I'll grant it that. Microsoft has always been blind to oncoming social applications, and C# will pay the price for that. Rather, we all will.

**R** is underused in the physical sciences. Shame on us. The statisticians and social scientists have all the fun. And I'll wager that's the first time *that* sentence has ever been written.

2014-2015 Plans at Bois d'Arc Observatory
------------------------------------------

My, there are dozens of things to do, but I'll mention here only the ones likely to actually get done.

- Gotta get some experimental data. I have a soon-to-be-automated 11" SCT which is more than large enough for this work, and the first thing is to track Landolt stars across the sky, and down below 30 degrees altitude. It's the only way to test mixed-model regression, and to be sure how much help additional regression terms can lend.

- Experimental data to support some of the conclusions and suggestions at the end of the paper and presentation.

- Project Cirrus: get experimental data to test the per-image noise model. My previous simulations suggest that one can extract per-image data down to millimagnitude levels. I doubt the per-noise model will show up on very clear nights, but it shouldn't be hard to test it as well on nights with thin cirrus passing. And here's a cheat--I have dark skies at Bois d'Arc, but Topeka's light dome is just bright enough on the horizon to light up cirrus slightly. If the per-image noise that shows up in mixed-model regression as a random effect correlates with cirrus as measured by image background brightness on long exposures, that pretty solidly supports the case.

----

*[end]*

