plot_003_mags <- function(plot.style="grey") {
  # PLOT Blackbody stars: Color Index ~ TempK.
  require(ggplot2)
  require(stringr)
  source("platform.R")
  # fit output lists "list_003x" already in environment.
  dfa <- x003a_list[["df"]]
  dfb <- x003b_list[["df"]]
  dfc <- x003c_list[["df"]]
  dfd <- x003d_list[["df"]]
  dfe <- x003e_list[["df"]]
  dff <- x003f_list[["df"]]
  dfa$filter <- "none"
  dfb$filter <- "V Bessel"
  dfc$filter <- "Baader Green"
  dfd$filter <- "Sloan G"
  dfe$filter <- "Asahi 540 100"
  dff$filter <- "Asahi 540 60"
  df <- rbind(dfa,dfb,dfc,dfd,dfe,dff)
  df$Mag <- df$InstMag - df$starMagV90 + 10
  # print(df)
  p <- ggplot(df, aes(x=CI.BV90,y=Mag,group=filter,color=filter))
  p <- p + geom_point(alpha=1, size=2)
  p <- p + geom_line()
  # p <- p + scale_color_gradient2(low="blue",mid=("yellow1"),midpoint=0.65,high=("red"))
  p <- p + xlab("Color Index (B-V)")
  p <- p + ylab("Instr  \nMag  ")
  if (plot.style=="grey")  
    p <- p + SAS2014.grey.theme()
  else
    p <- p + SAS2014.bw.theme()
  # put here: post-SAS-style tweaks:
  p <- p + xlim(-0.75,1.8)    # make room at left for line labels
  p <- p + scale_y_reverse() # because lower magnitudes mean higher brightness.  
  
  ggsave(filename="003_plotmags.svg", width=9, height=6, unit="in", dpi=200)
  # p
}

