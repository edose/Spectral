x011a_fit <- function(list.in, item.noise.mag = 0.0, image.noise.mag = 0.0, fix.seed=TRUE) {
  # Workflow: run C#/NUnit; copy to clipboard; nnna_raw<-get.cs(); nnna_list<-fit_nnna(nnna_raw).
  # Model: InstMag ~ CI (~centered) + airmass(~centered)
  # parms: list.in is list obtained from get.ForR(), 
  #    noise.mag = added Gaussian noise per data point (sigma in magnitudes).
  #    image.noise.mag = added Gaussian noise per image (sigma in magnitudes).
  #    fix.seed = TRUE if random-number seed fixed (for same results per run;
  #       this should be FALSE for bootstrapping, repeated calls, e.g.) 
  #       N.B.: if fit() is called repeatedly by another fn, that fn should first set.seed().
  # This fn appends to input these list items: 
  #    summary of model output (itself a list), and
  #    text lines representing this function (fit.R)
  # This fn return this expanded list.
  # ---------------------------------------------------------
  df <- list.in[["df.cs"]]    # double bracket to un-list item (to create a data.frame)
  df <- df[df$CI.VI90 < 1.6,]  # eliminate very red stars.
#  df <- df[df$SecantZA <= 2.0,] # keep images only at Airmass 2 or less.
  Mag <- df$InstMag - df$starMagV90 # correct Mag for inherent star mag
  if (item.noise.mag > 0) {
    if (fix.seed == TRUE)
      set.seed(234) # arbitrary but reproducible seed.
    Mag <- Mag + rnorm(nrow(df),0,item.noise.mag) # add noise to all stars.
  } # if.
  if (image.noise.mag > 0) {
    if (fix.seed == TRUE)
      set.seed(135) # arbitrary but reproducible seed.
    imageList <- unique(df$imageID)
    for (image in imageList) {
      rows = (df$imageID == image) # select all rows belonging to this image.
      fluctuation.this.image = rnorm(1,0,image.noise.mag)
      Mag[rows] <- Mag[rows] + fluctuation.this.image # add same error to all stars in this image.
    } # for
  } # if.
  Centering.CI <- mean(df$CI.VI90)         # to center CI values.
  CI <- df$CI.VI90 - Centering.CI          # NB: we use color index V-I, not B-V.
  Centering.SecantZA <- mean(df$SecantZA)  # to center Airmass values.
  Airmass <- df$SecantZA - Centering.SecantZA
  model <- lm(formula = Mag ~ CI + Airmass) # THE MODEL.
  list.out <- list.in
  # Capture and append to list the R Code generating raw data (this .R file)
    connection <- file("011a_fit.R")           # *this* code file as text
    list.out$code.fit <- readLines(connection) # record this code file as next list item.
    close(connection)
  # Capture and append to list the fit's results.
    list.out$summary <- summary(model,correlation=TRUE) # append model results as next list item.
    print (list.out$summary)                            # print model results to screen.
  list.out$model <- model
  save(list.out, file="011a_list")  # saves updated list as a file.
  return (list.out)                 # returns as a list.
}
