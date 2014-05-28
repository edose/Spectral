plot_005_magsBV <- function(plot.style="grey") {
  # PLOT PASP stars: Color Index ~ TempK.
  require(ggplot2)
  require(stringr)
  source("platform.R")
  # fit output lists "list_005x" must be already in environment.
  dfa <- x005a_list[["df.cs"]]
  dfb <- x005b_list[["df.cs"]]
  dfc <- x005c_list[["df.cs"]]
  dfd <- x005d_list[["df.cs"]]
  dfe <- x005e_list[["df.cs"]]
  dff <- x005f_list[["df.cs"]]
  dfa <- dfa[order(dfa$CI.BV90),] # <-- sort by color index.
  dfb <- dfb[order(dfb$CI.BV90),]
  dfc <- dfc[order(dfc$CI.BV90),]
  dfd <- dfd[order(dfd$CI.BV90),]
  dfe <- dfe[order(dfe$CI.BV90),]
  dff <- dff[order(dff$CI.BV90),]
  dfa$filter <- "none"
  dfb$filter <- "V Bessel"
  dfc$filter <- "Baader Green"
  dfd$filter <- "Sloan G"
  dfe$filter <- "Asahi 540 100"
  dff$filter <- "Asahi 540 60"
  df <- rbind(dfa,dfb,dfc,dfd,dfe,dff)
  # df <- rbind(dfb,dfc)
  # df <- rbind(dfb)
  df$filter <- as.factor(df$filter)
  df$Mag <- df$InstMag - df$starMagV90 + 10 # normalized to mag V = 10.
  # print(df)
  p <- ggplot(df, aes(x=CI.BV90,y=Mag,group=filter,color=filter))
  p <- p + geom_point(alpha=1, size=1.5)
  p <- p + geom_line()
  # p <- p + scale_color_gradient2(low="blue",mid=("yellow1"),midpoint=0.65,high=("red"))
  p <- p + scale_color_manual(values=c("none"="#00bdc2","V Bessel"="#f563e3","Baader Green"="#00b834",
    "Sloan G"="#619cff","Asahi 540 100"="#f8756b","Asahi 540 60"="#b59e00"))
  p <- p + xlab("Color Index (B-V)")
  p <- p + ylab("Instr  \nMag  ")
  if (plot.style=="grey")  
    p <- p + SAS2014.grey.theme()
  else
    p <- p + SAS2014.bw.theme()
  # put here: post-SAS-style tweaks:
  p <- p + xlim(-0.75,1.8)    # make room at left for line labels
  p <- p + scale_y_reverse() # because lower magnitudes mean higher brightness.  
  
  ggsave(filename="005_plotmagsBV.svg", width=9, height=6, unit="in", dpi=200)
  p
}

