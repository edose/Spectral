plot_002a <- function(plot.style="grey") {
  # PLOT a few blackbody stars from C# output.
  require(ggplot2)
  require(grid)
  source("platform.R")
  # input data written to clipboard by ForR() method [Spectral(C#)+NUnit].
  # clipboard contents then captured to list object list_nnnx by get.cs() [R].
  # file "nnnx.list" saved by running save(list_nnnx, file="nnnx.list") [R].
  df <- read.delim("002a_raw.txt", sep="", skip=2) # sep="" means white space.
  df$Alt = as.factor(df$Alt)
  df <- df[df$Alt %in% c(90,60,30,20,10),]
  p <- ggplot(df, aes(x=nm, y=Transm, group=Alt, color=Alt))
  p <- p + geom_line(size=0.4)
  p <- p + xlab("wavelength (nm)")
  p <- p + ylab("Transmission   \nfraction   ")
  p <- p + choose.theme("grey") 
  # put here: any post-SAS-style tweaks.
  p
  ggsave(filename="002a_plot.svg", width=9, height=6, unit="in", dpi=200)
}
