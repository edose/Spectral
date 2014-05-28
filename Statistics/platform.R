# Helper functions needed by other R code work in project Spectral.
# Eric Dose, Bois d'Arc Observatory, Kansas, USA -- January 2014.

# get.cs() is the first R step in a regression experiment.
#    It takes NUnit data (header lines & data frame) from Windows clipboard, 
#    and the C# code (), then returns a list object containing all 3.
#    (These are captured now, i.e. together, to ensure perfect synchronization.)
get.cs <- function(text.source="clipboard"){
  # First, add C# code to list.
  code.connection <- file("C:/Dev/Spectral/UnitTest_Spectral/RunSpectral.cs")
  code.cs <- readLines(code.connection)    # get C# code used to generate NUnit data.
  close(code.connection)
  # Next, add header lines and data frame to list.
  header.cs <- readLines(text.source, 8)   # get header lines
  df.cs <- read.delim(text.source, skip=8) # get data frame of observations
  # print ("now enter at console --> save(list_nnnx, file=\"nnnx.list\")")
  list.out <- list(code.cs=code.cs, header.cs=header.cs, df.cs=df.cs) # return as a list of 3.
}

choose.theme <- function(theme.chosen="grey") {
  if (theme.chosen=="grey")
    theme <- SAS2014.grey.theme()
  else
    theme <- SAS2014.bw.theme()
  theme # return this.
}

SAS2014.grey.theme <- function(){ # use NOW for SAS 2014 live presentation.
  th <- theme_grey()
  # th <- th + theme(plot.title=element_text(size=rel(1.4),face="bold",color="gray32"))
  th <- th + theme(axis.text=element_text(size=rel(1),color="gray44"))  # both axes.
  th <- th + theme(axis.title.x=element_text(angle=0,size=rel(1.2),face="italic",color="gray30",vjust=-0.25))
  th <- th + theme(axis.title.y=element_text(angle=0,size=rel(1.2),face="italic",color="gray30",hjust=1))
  th <- th + theme(legend.position="none")
}

#SAS2014.bw.theme <- function(){ # use later for publication. (but first: copy elements from SAS grey theme)
#  th <- theme_bw()
#  th <- th + theme(plot.title=element_text(size=rel(1.6),face="bold",color="gray32"))
#  th <- th + theme(axis.text=element_text(size=rel(1.4),color="gray55"))  # both axes.
#  th <- th + theme(axis.title.x=element_text(angle=0,size=rel(1.4),face="italic",color="gray50"))
#  th <- th + theme(axis.title.y=element_text(angle=0,size=rel(1.4),face="italic",color="gray50"))
#  th <- th + theme(legend.position="none")
#}

## write.log() is last step in a regression experiment. UNFINISHED.
##    It takes the list accumulated during this simulation and regression experiment,
##    and writes several log files containing input data, regression code, and output data.
#rite.log <- function(suffix, list.in){
#  SuffixIsOK = FALSE
#  if(class(suffix)=="character")
#    if(nchar(suffix)==3)
#      SuffixIsOK = TRUE
#  if (!SuffixIsOK){
#    stop("Suffix must be 3 characters.")
#  }
#  # if we get to here, suffix is OK. 
#  # now, check that the input list is OK:
#  
#}

