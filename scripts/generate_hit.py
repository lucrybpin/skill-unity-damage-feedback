"""Reproduce the bundled original synthetic impact; Python standard library only."""
from pathlib import Path
import math
import random
import struct
import wave


def generate(destination):
    sample_rate = 44100
    rng = random.Random(42)
    samples = []
    phase = 0.0
    low_noise = 0.0
    for i in range(int(sample_rate * 0.18)):
        t = i / sample_rate
        phase += 2 * math.pi * (65 + 150 * math.exp(-t * 65)) / sample_rate
        noise = rng.uniform(-1, 1)
        low_noise += .32 * (noise - low_noise)
        body = math.sin(phase) * math.exp(-t * 34)
        snap = (noise - low_noise) * math.exp(-t * 160)
        texture = low_noise * math.exp(-t * 45)
        attack = min(1, t / .001)
        tail = min(1, (.18 - t) / .01)
        value = attack * tail * (.62 * body + .24 * snap + .14 * texture)
        samples.append(struct.pack('<h', int(max(-1, min(1, value)) * 28000)))
    destination.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(destination), 'wb') as audio:
        audio.setnchannels(1)
        audio.setsampwidth(2)
        audio.setframerate(sample_rate)
        audio.writeframes(b''.join(samples))


if __name__ == '__main__':
    generate(Path(__file__).resolve().parents[1] / 'assets/UnityProject/Assets/UnityDamageFeedback/Audio/SyntheticHit.wav')
