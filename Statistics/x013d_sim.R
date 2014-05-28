sim <- function(df_raw, n_fits, magSigma, imageSigma=0, starSigma=0) {
  # df_raw from load("x013a_raw"), then df_raw<-x013a_raw[["df.cs"]]
  require(lme4)
  set.seed(22)
  # get star names by color range.
  stars_low <- as.character(unique(df_raw[df_raw$CI.VI90 <= 0.4,]$starID))
  stars_mid <- as.character(unique(df_raw[df_raw$CI.VI90 > 0.4 & df_raw$CI.VI90 <= 1.0,]$starID))
  stars_high <- as.character(unique(df_raw[df_raw$CI.VI90 > 1.0 & df_raw$CI.VI90 < 1.6,]$starID))
  n_stars_low <- 3
  n_stars_mid <- 1
  n_stars_high <- 3
  # get airmass values by airmass range.
  airmasses_low <- unique(df_raw[df_raw$SecantZA <= 1.3,]$SecantZA)
  airmasses_mid <- unique(df_raw[df_raw$SecantZA >= 1.5 & df_raw$SecantZA <= 1.6,]$SecantZA)
  airmasses_high <- unique(df_raw[df_raw$SecantZA >= 1.9 & df_raw$SecantZA <= 2,]$SecantZA)
  n_airmasses_low <- 2
  n_airmasses_mid <- 1
  n_airmasses_high <- 2
  # prepare empty results df of proper shape and length.
  zeroes <- rep(0,n_fits)
  s <- zeroes            ## std dev (residual) after all regression effects.
  s.image <- zeroes      ## variance accounted for by imageID random effect.
  s.star <- zeroes       ## variance accounted for by starID random effect. 
  intercept <- zeroes    ## intercept of regression.
  tform <- zeroes        ## transform coefficient of system (color index)
  extinct <- zeroes      ## extinction coeff. of sky x system
  ZP <- zeroes           ## zero-point at zero color index and zero extinction.
  df_results = data.frame(s, s.image, s.star, intercept, tform, extinct, ZP)
  # df_results = data.frame(s, var_image, var_star, intercept, tform, extinct, ZP)
  offset.CI = -0.4        # approx = - mean CI ; rough centering, each fit
  offset.SecantZA = -1.5  # approx = - mean airmass ; rough centering, each fit
  # loop of fits.
  for (fit in 1:n_fits) {
    # sample rows for this fit.
    starIDs <- c( 
      sample(stars_low, size=n_stars_low),
      sample(stars_mid, size=n_stars_mid),
      sample(stars_high, size=n_stars_high)  )
    airmasses <- c(
      sample(airmasses_low, size=n_airmasses_low),
      sample(airmasses_mid, size=n_airmasses_mid),
      sample(airmasses_high, size=n_airmasses_high)  )
    df_fit <- df_raw[df_raw$starID %in% starIDs & df_raw$SecantZA %in% airmasses, ]
    # add errors & offsets.
    df_fit <- addSigmas(df_fit, magSigma, imageSigma, starSigma)
    df_fit$CI <- df_fit$CI.VI90 + offset.CI
    df_fit$AM <- df_fit$SecantZA + offset.SecantZA
    # run fit.
    mod <- lmer(InstMag ~ CI + AM + (1|imageID) + (1|starID), data=df_fit)
    #mod <- lmer(MagInstRaw ~ (1|imageID) + (1|starID) + CI + AM, 
    #           data=df.fit) # comprehensive model from which to choose items.
    # stop("cause evd said so")
    # extract wanted data, write in correct line of df_results.
    s <- sigma(mod)
    s.image <- sd(ranef(mod)$imageID[,1])
    s.star <- sd(ranef(mod)$starID[,1])
    intercept <- getME(mod, "beta")[1]
    tform <- getME(mod, "beta")[2]
    extinct <- getME(mod, "beta")[3]
    ZP <- intercept + offset.CI*tform + offset.SecantZA*extinct
    df_results[fit,] <- c(s, s.image, s.star, intercept, tform, extinct, ZP)
  } ## for fit repetition.
  # return(mod)
  print (report(df_results))
  print ("====> Now do this: sav(r,'xnnnx')",quote=FALSE)
  return (df_results)
}

addSigmas <- function (df_in, magSigma, imageSigma, starSigma) {
  df <- df_in
  if (magSigma > 0) {
    df$InstMag = df$InstMag + rnorm(nrow(df),0,magSigma)
  }
  if (imageSigma > 0) {
    for (image in unique(df$imageID)) { 
      rows = (df$imageID == image)
     df$InstMag[rows] <- df$InstMag[rows] + rnorm(1,0,imageSigma)
   }
  }
  if (starSigma > 0) {
    for (star in unique(df$starID)) {
      rows = (df$starID == star)
      df$InstMag[rows] <- df$InstMag[rows] + rnorm(1,0,starSigma)   
    }
  }
  return (df)
}

report <- function(r) {
  # r = results from sim()
  s <- as.data.frame(sapply(r,my_summary))
  s_sd <- as.data.frame(t(sapply(r,sd)))
  rownames(s_sd) <- "St.Dev"
  #s_rsd <- rr["St.Dev",] / rr["Mean",]
  #rownames(s_rsd) <- "RSD"
  s <- rbind(s,s_sd)
}

my_summary <- function(r) {
  summary(r,digits=7)  
}

sav <- function(r,list_name) {
  list.out <- list(results=r, report=report(r))
  save(list.out, file=paste(list_name,"_list",sep=""))
  
  
}