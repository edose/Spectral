plot_001a <- function(){
  require(ggplot2)
  source("platform.R")
  load("RefData/PASP.CI")  # df of color indices of PASP stars.
  df <- PASP.CI
  p <- ggplot(df, aes(x=Type.gen,y=CI.BV90)) 
  p <- p + ggtitle("Model stars (Pickles, 1998)")
  p <- p + geom_point(position=position_jitter(width=0.1,height=0),alpha=0.8,size=5,
                      aes(color=CI.BV90))
  p <- p + scale_color_gradient2(low="blue",mid=("yellow1"),midpoint=0.65,high=("red"))
  p <- p + xlab("")
  p <- p + ylab("Color\nIndex\n(B-V) ")
  # p <- p + annotate("text", label="131 simulated star spectra",x="B",y=1.575)
#  p <- p + SAS2014.bw.theme() #############################################
  p <- p + SAS2014.grey.theme() #############################################
  p <- p + theme(axis.title.x=element_blank()) # no x-axis label (or space for it)
  p <- p + theme(plot.title=element_blank())
  p # <-- to plot in RStudio's Plots window.
  ggsave(filename="001a_plot.svg", width=9, height=6, unit="in", dpi=72)
}