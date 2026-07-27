"""Deterministically generate the project's minimalist audio palette.

Only Python's standard library is used. Re-running this file replaces the WAV
content in place, while Unity keeps the same asset GUIDs and SoundLibrary links.
"""

from __future__ import annotations

import math
import random
import struct
import wave
from pathlib import Path


SAMPLE_RATE = 44_100
ROOT = Path(__file__).resolve().parents[2]
SFX_DIR = ROOT / "Assets" / "Audio" / "SFX"
MUSIC_DIR = ROOT / "Assets" / "Audio" / "Music"
TAU = math.tau


def clamp(value: float) -> float:
    return max(-1.0, min(1.0, value))


def smoothstep(value: float) -> float:
    value = max(0.0, min(1.0, value))
    return value * value * (3.0 - 2.0 * value)


def envelope(t: float, duration: float, attack: float, release: float) -> float:
    attack_gain = smoothstep(t / max(attack, 1e-6))
    release_gain = smoothstep((duration - t) / max(release, 1e-6))
    return min(attack_gain, release_gain)


def write_mono(path: Path, samples: list[float], peak: float = 0.92) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    maximum = max((abs(sample) for sample in samples), default=1.0)
    gain = peak / maximum if maximum > peak else 1.0
    payload = b"".join(
        struct.pack("<h", int(clamp(sample * gain) * 32767.0))
        for sample in samples
    )
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(payload)


def write_stereo(path: Path, samples: list[tuple[float, float]], peak: float = 0.9) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    maximum = max(
        (max(abs(left), abs(right)) for left, right in samples),
        default=1.0,
    )
    gain = peak / maximum if maximum > 0.0 else 1.0
    payload = b"".join(
        struct.pack(
            "<hh",
            int(clamp(left * gain) * 32767.0),
            int(clamp(right * gain) * 32767.0),
        )
        for left, right in samples
    )
    with wave.open(str(path), "wb") as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(payload)


def make_click(seed: int, tone: float) -> list[float]:
    rng = random.Random(seed)
    duration = 0.052
    count = int(duration * SAMPLE_RATE)
    phases = [rng.uniform(0.0, TAU) for _ in range(3)]
    resonances = (185.0 + tone, 315.0 + tone * 0.55, 505.0 + tone * 0.25)
    low_noise = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        low_noise += 0.12 * (noise - low_noise)
        nail = (noise - low_noise) * math.exp(-t * 310.0)
        wood = sum(
            math.sin(TAU * frequency * t + phase)
            for frequency, phase in zip(resonances, phases)
        ) / len(resonances)
        body = wood * math.exp(-t * 72.0)
        release = smoothstep((duration - t) / 0.018)
        result.append((0.62 * nail + 0.48 * body) * release)
    return result


def make_marker(seed: int, pressure: float) -> list[float]:
    rng = random.Random(seed)
    duration = 0.34
    count = int(duration * SAMPLE_RATE)
    fast = 0.0
    slow = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        fast += 0.22 * (noise - fast)
        slow += 0.035 * (noise - slow)
        friction = fast - slow
        texture = 0.78 + 0.22 * math.sin(TAU * (11.0 + pressure) * t)
        paper = math.sin(TAU * 168.0 * t + 0.7 * math.sin(TAU * 4.0 * t))
        gain = envelope(t, duration, 0.012, 0.045)
        result.append(gain * pressure * (0.70 * friction * texture + 0.035 * paper))
    return result


def make_scale(seed: int, base_frequency: float) -> list[float]:
    rng = random.Random(seed)
    duration = 0.27
    count = int(duration * SAMPLE_RATE)
    phase = rng.uniform(0.0, TAU)
    low_noise = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        phase += TAU * base_frequency / SAMPLE_RATE
        low_noise += 0.045 * (rng.uniform(-1.0, 1.0) - low_noise)
        body = math.sin(phase)
        contact = low_noise * math.exp(-t * 42.0)
        soft_tail = low_noise * math.exp(-t * 8.0)
        release = smoothstep((duration - t) / 0.075)
        result.append(
            release
            * (
                0.54 * body * math.exp(-t * 11.0)
                + 0.20 * contact
                + 0.06 * soft_tail
            )
        )
    return result


def make_eraser(seed: int, stroke_rate: float) -> list[float]:
    rng = random.Random(seed)
    duration = 0.46
    count = int(duration * SAMPLE_RATE)
    fast = 0.0
    slow = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        fast += 0.32 * (noise - fast)
        slow += 0.045 * (noise - slow)
        band = fast - slow
        strokes = 0.48 + 0.52 * abs(math.sin(TAU * stroke_rate * t))
        low_rub = math.sin(TAU * 92.0 * t + 0.5 * math.sin(TAU * 6.0 * t))
        gain = envelope(t, duration, 0.018, 0.07)
        result.append(gain * (0.52 * band * strokes + 0.035 * low_rub))
    return result


def make_impact(seed: int, base_frequency: float) -> list[float]:
    rng = random.Random(seed)
    duration = 0.19
    count = int(duration * SAMPLE_RATE)
    phase = 0.0
    low_noise = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        progress = t / duration
        frequency = base_frequency * (1.0 - 0.58 * smoothstep(progress))
        phase += TAU * frequency / SAMPLE_RATE
        low_noise += 0.08 * (rng.uniform(-1.0, 1.0) - low_noise)
        thud = math.sin(phase) + 0.30 * math.sin(0.51 * phase)
        transient = low_noise * math.exp(-t * 42.0)
        gain = envelope(t, duration, 0.0025, 0.055) * math.exp(-t * 10.0)
        result.append(gain * (0.70 * thud + 0.24 * transient))
    return result


def circular_distance(a: float, b: float, period: float) -> float:
    raw = abs(a - b) % period
    return min(raw, period - raw)


def periodic_frequency(frequency: float, duration: float) -> float:
    return round(frequency * duration) / duration


def make_music() -> list[tuple[float, float]]:
    duration = 32.0
    count = int(duration * SAMPLE_RATE)
    chord_centres = [0.0, 4.0, 8.0, 12.0, 16.0, 20.0, 24.0, 28.0]
    chords = [
        (164.81, 246.94, 329.63),
        (146.83, 220.00, 293.66),
        (130.81, 196.00, 246.94),
        (146.83, 220.00, 329.63),
        (164.81, 246.94, 293.66),
        (130.81, 196.00, 293.66),
        (146.83, 246.94, 329.63),
        (164.81, 220.00, 293.66),
    ]
    melody = [329.63, 293.66, 246.94, 220.00, 246.94, 293.66, 196.00, 220.00,
              329.63, 246.94, 293.66, 220.00, 196.00, 246.94, 293.66, 246.94]
    chord_frequencies = [
        tuple(periodic_frequency(frequency, duration) for frequency in chord)
        for chord in chords
    ]
    melody_frequencies = [
        periodic_frequency(frequency, duration) for frequency in melody
    ]
    drone_left = periodic_frequency(82.41, duration)
    drone_right = periodic_frequency(123.47, duration)
    result: list[tuple[float, float]] = []

    for index in range(count):
        t = index / SAMPLE_RATE
        left = 0.0
        right = 0.0

        # Quiet continuous foundation. Frequencies are quantized to whole loop cycles.
        left += 0.075 * math.sin(TAU * drone_left * t)
        right += 0.070 * math.sin(TAU * drone_right * t + 0.35)

        # Circular crossfades make the last chord blend into the first at the seam.
        for chord_index, centre in enumerate(chord_centres):
            distance = circular_distance(t, centre, duration)
            if distance >= 4.0:
                continue
            weight = 0.5 + 0.5 * math.cos(math.pi * distance / 4.0)
            frequencies = chord_frequencies[chord_index]
            left += weight * (
                0.032 * math.sin(TAU * frequencies[0] * t + 0.2)
                + 0.022 * math.sin(TAU * frequencies[2] * t + 1.1)
            )
            right += weight * (
                0.030 * math.sin(TAU * frequencies[1] * t + 0.7)
                + 0.020 * math.sin(TAU * frequencies[2] * t + 1.6)
            )

        # Muted two-second pulses; the final pulse tail wraps into the loop start.
        for event_index, frequency in enumerate(melody_frequencies):
            event_time = event_index * 2.0
            age = (t - event_time) % duration
            if age > 2.8:
                continue
            pulse = math.exp(-age * 2.15) * smoothstep(age / 0.018)
            tone = math.sin(TAU * frequency * age)
            overtone = 0.16 * math.sin(TAU * frequency * 2.0 * age + 0.4)
            pan = -0.22 if event_index % 2 == 0 else 0.22
            left += pulse * (tone + overtone) * 0.026 * (1.0 - pan)
            right += pulse * (tone + overtone) * 0.026 * (1.0 + pan)

        # Very small periodic breathing keeps the bed alive without dramatic changes.
        breath = 0.91 + 0.09 * math.sin(TAU * t / 8.0 - math.pi / 2.0)
        result.append((left * breath, right * breath))

    return result


def main() -> None:
    assets = {
        SFX_DIR / "ui_click_01.wav": make_click(101, 0.0),
        SFX_DIR / "ui_click_02.wav": make_click(102, 12.0),
        SFX_DIR / "figure_fill_01.wav": make_marker(201, 0.90),
        SFX_DIR / "figure_fill_02.wav": make_marker(202, 0.82),
        SFX_DIR / "figure_scale_01.wav": make_scale(301, 78.0),
        SFX_DIR / "figure_scale_02.wav": make_scale(302, 84.0),
        SFX_DIR / "damage_erase_01.wav": make_eraser(401, 8.0),
        SFX_DIR / "damage_erase_02.wav": make_eraser(402, 8.8),
        SFX_DIR / "damage_impact_01.wav": make_impact(501, 122.0),
        SFX_DIR / "damage_impact_02.wav": make_impact(502, 115.0),
        SFX_DIR / "damage_impact_03.wav": make_impact(503, 128.0),
    }
    for path, samples in assets.items():
        write_mono(path, samples)

    write_stereo(MUSIC_DIR / "minimal_background_loop.wav", make_music(), peak=0.58)
    print(f"Generated {len(assets)} SFX and 1 seamless music loop.")


if __name__ == "__main__":
    main()
