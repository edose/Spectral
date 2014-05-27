using System;
using EVD.Astro.Spectral;

namespace EVD.Astro.Spectral {
    public interface IFilter {
        PFluxPerArea ActOn(PFluxPerArea pfluxPerArea);
        PFluxApertured ActOn(PFluxApertured pfluxApertured);
    }
    public interface IDetector {
        double CountRateFromPFlux(PFluxApertured pfluxApertured);
        double InstrumentalMagnitudeFromPFlux(PFluxApertured pfluxApertured);
    }
    public interface IAperturedFilter {
        // apertures a flux per unit area to an apertured (absolute) flux.
        PFluxApertured ActOn(PFluxPerArea pfluxPerArea);
    }
} // namespace.
