# PingTracker
Tracks and plots latency data and processes it through a FFT model (Work in progress)

![image](https://github.com/TheBarret/PingTracker/assets/25234371/559cf174-fe23-4f9c-8d7e-64f2b16581ed)
(screenshots are from a newer version im testing and are usually different in look but will be uploaded soon)

# Advantages

- Identify Patterns

In the frequency domain, it's easier to identify patterns that may not be apparent in the time domain. For example, if there's a regular spike in latency at certain intervals, this will show up as a peak at a corresponding frequency.
Noise Reduction: The frequency domain can help in identifying and reducing noise. If the latency data contains random fluctuations, these can be identified as high-frequency noise and filtered out, leaving the more meaningful, low-frequency trends.

- Bandwidth Utilization

By looking at the frequency domain, you can get a better understanding of the bandwidth utilization. If there are a lot of high-frequency components, it means that the data is changing rapidly, which could indicate a high utilization of bandwidth.

- System Performance Analysis: Frequency analysis can help in identifying system performance issues. For instance, if there's a peak at a certain frequency, it could indicate a periodic task that's causing a delay.

Predictive Analysis: Frequency domain data can be used for predictive analysis. For example, if the frequency data shows a regular pattern, it can be used to predict future behavior.

- FFT-Derived Delay (Phase Delay and Group Delay)

These are derived from the Fourier Transform of a signal, revealing how the phase of the frequency components changes from one sample to another.
A rapid phase shift for a particular frequency component could imply delay. 

- Phase Delay

The delay that a specific frequency component of the signal experiences.

- Group Delay

The derivative of the phase response with respect to frequency. It measures the rate of change of the phase delay and is often considered as a time delay.


# Window Functions (Filters)

- None

The simplest type of window where all the data points are given equal weight, it provides the best resolution but worst spectral leakage

- Hanning

Good balance between frequency resolution and spectral leakage. It's useful in many general-purpose signal processing applications

- Blackman

The Blackman window further reduces spectral leakage compared to the Hamming window, but at the expense of frequency resolution,
it's useful when you need to suppress side lobes even further

- Bartlett 

The Bartlett window function basically forms a triangular shape which is non-zero from the left endpoint to the right endpoint of the window.
It's simple to compute and implement. The Bartlett window offers a reasonable compromise between the amount of spectral leakage and the resolution of the frequency estimation.
It has reduced "scalloping loss

