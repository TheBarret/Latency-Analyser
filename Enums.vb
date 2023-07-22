Public Enum WFunction
    'The simplest type of window where all the data points are given equal weight, it provides the best resolution but worst spectral leakage
    None = 0
    'Good balance between frequency resolution and spectral leakage. It's useful in many general-purpose signal processing applications
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
End Enum

Public Enum PType
    Cpu
    Memory
    Disk
    Network
End Enum