plot_001c <- function(plot.style="grey") {
  # PLOT a few blackbody stars from C# output.
  require(ggplot2)
  require(grid)
  source("platform.R")
  # input data written to clipboard by ForR() method [Spectral(C#)+NUnit].
  # clipboard contents then captured to list object list_nnnx by get.cs() [R].
  # file "nnnx.list" saved by running save(list_nnnx, file="nnnx.list") [R].
  df <- read.delim("001c_rawflux.txt", sep="", skip=1) # sep="" means white space.
  df$TempK = as.factor(df$TempK)
  p <- ggplot(df, aes(x=nm, y=flux, group=TempK, color=TempK))
  p <- p + geom_line()
  p <- p + xlab("wavelength (nm)")
  p <- p + ylab("Photon\nFlux")
  #p <- p + annotate("text",x=380,y=47000,label="53,000 K",color="grey20") +
  #  annotate("text",x=785,y=47000,label="2,820 K",color="grey20")
  if (plot.style=="grey")  
    p <- p + SAS2014.grey.theme()
  else
    p <- p + SAS2014.bw.theme()
  # put here: any post-SAS-style tweaks.
  p
  ggsave(filename="001c_plot.svg", width=9, height=6, unit="in", dpi=200)
}
