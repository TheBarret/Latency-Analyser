# PingTracker
Tracks and plots latency data and processes it through a FFT model (Work in progress)

Features:
- Identifying periodic issues

If there are any periodic issues affecting latency, they will show up as peaks at corresponding frequencies in the FFT.
For example, if there's a process that runs every 10 seconds and slows down the network, you would see a peak at the 0.1 Hz frequency.

- Identifying persistent problems

If the latency is consistently high, this will appear as a peak at the 0Hz frequency (often called the DC component in FFT results).
If you see a high DC component, it could be an indication of a persistent problem with latency.

- Identifying random issues

If there are a lot of random, non-periodic issues affecting latency, these will show up as a wide, flat spectrum in the FFT.
If you see a spectrum that doesn't have distinct peaks but is high across a wide range of frequencies, it could mean that there's a lot of random jitter in the latency.

- FFT-Derived Delay (Phase Delay and Group Delay)

These are derived from the Fourier Transform of a signal, revealing how the phase of the frequency components changes from one sample to another.
A rapid phase shift for a particular frequency component could imply delay. 

- Phase Delay

The delay that a specific frequency component of the signal experiences.

- Group Delay

The derivative of the phase response with respect to frequency. It measures the rate of change of the phase delay and is often considered as a time delay.

How to intiatlize the hub:

```
Me.Hub = New Hub(Me.Vp.ClientRectangle, <samplerate(128+), New Latency("<hostname>"))

Me.Hub.Devices.Add(New Transformer(Me.Hub, New Fourier(<Window Function>))
Me.Hub.Devices.Add(New Transformer(Me.Hub, New Spectrogram(<Window Function>)))
```

![image](https://github.com/TheBarret/PingTracker/assets/25234371/47beb1a5-06be-4233-b9ac-0397eb078ff2)
