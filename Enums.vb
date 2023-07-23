Public Enum WFunction
    ' The simplest type of window where all the data points are given equal weight,
    ' it provides the best resolution but worst spectral leakage
    None = 0

    ' B-Spline window, a piecewise-defined polynomial function that has minimal support with given degree,
    ' smoothness, and domain partition
    BSpline

    ' Parzen window, a type of window function that is non-negative, symmetric, and piecewise smooth.
    ' It is typically used in non-parametric statistics
    Parzen

    ' Polynomial window, a type of window function that is a polynomial of a certain degree.
    ' It is used when a specific polynomial shape is desired
    Polynomial

    ' Nuttall window, a type of window function that has extremely low side lobes,
    ' making it useful when frequency content near the signal frequency needs to be minimized
    NuttallBm

    ' Flat top window, a type of window function that is designed to have the smallest possible amplitude error
    ' when used in a Fourier transform
    FlatTop

    ' Tukey window, a type of window function that is essentially a rectangular window with a portion of a cosine wave at its ends.
    ' It is used when a compromise between a rectangular and a Hann window is needed
    Tukey

    ' Planck-taper window, a type of window function that is a Planck function tapering from 0 to 1.
    ' It is used when a smooth transition from zero to one is needed
    PlanckTaper

    ' Hanning, a good balance between frequency resolution and spectral leakage.
    ' It's useful in many general-purpose signal processing applications
    Hanning

    ' The Blackman window further reduces spectral leakage compared to the Hamming window, but at the expense of frequency resolution,
    ' it's useful when you need to suppress side lobes even further
    Blackman

    ' The Bartlett window function basically forms a triangular shape which is non-zero from the left endpoint to the right endpoint of the window.
    ' It's simple to compute and implement. The Bartlett window offers a reasonable compromise between the amount of spectral leakage and
    ' the resolution of the frequency estimation. It has reduced "scalloping loss
    Bartlett

    ' The Gaussian window has good frequency resolution and moderate spectral leakage.
    ' It's often used in applications like filter design and spectral analysis where a compromise between resolution and leakage is necessary
    Gaussian

    ' Kaiser window, a type of window function that provides a parameter to adjust the trade-off between the main lobe width
    ' and side lobe level, which is useful in many signal processing applications
    Kaiser
End Enum
