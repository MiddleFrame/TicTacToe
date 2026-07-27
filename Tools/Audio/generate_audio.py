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


def make_kick(seed: int) -> list[float]:
    rng = random.Random(seed)
    duration = 0.22
    count = int(duration * SAMPLE_RATE)
    phase = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        frequency = 48.0 + 62.0 * math.exp(-t * 32.0)
        phase += TAU * frequency / SAMPLE_RATE
        body = math.sin(phase) * math.exp(-t * 19.0)
        contact = rng.uniform(-1.0, 1.0) * math.exp(-t * 150.0)
        result.append((0.82 * body + 0.08 * contact) * smoothstep((duration - t) / 0.04))
    return result


def make_wood_rim(seed: int) -> list[float]:
    rng = random.Random(seed)
    duration = 0.105
    count = int(duration * SAMPLE_RATE)
    phases = [rng.uniform(0.0, TAU) for _ in range(3)]
    frequencies = (285.0, 465.0, 710.0)
    low_noise = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        low_noise += 0.10 * (noise - low_noise)
        transient = (noise - low_noise) * math.exp(-t * 180.0)
        wood = sum(
            math.sin(TAU * frequency * t + phase)
            for frequency, phase in zip(frequencies, phases)
        ) / len(frequencies)
        result.append(
            (0.34 * wood * math.exp(-t * 47.0) + 0.20 * transient)
            * smoothstep((duration - t) / 0.025)
        )
    return result


def make_brushed_hat(seed: int, duration: float = 0.055) -> list[float]:
    rng = random.Random(seed)
    count = int(duration * SAMPLE_RATE)
    low = 0.0
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        noise = rng.uniform(-1.0, 1.0)
        low += 0.24 * (noise - low)
        high = noise - low
        result.append(
            high
            * math.exp(-t * 55.0)
            * smoothstep((duration - t) / 0.018)
        )
    return result


def make_muted_bass(frequency: float, seed: int) -> list[float]:
    rng = random.Random(seed)
    duration = 0.32
    count = int(duration * SAMPLE_RATE)
    phase = rng.uniform(0.0, TAU)
    result: list[float] = []
    for index in range(count):
        t = index / SAMPLE_RATE
        pitch = frequency * (1.0 + 0.018 * math.exp(-t * 25.0))
        phase += TAU * pitch / SAMPLE_RATE
        tone = math.sin(phase) + 0.12 * math.sin(phase * 2.0 + 0.4)
        gain = smoothstep(t / 0.004) * math.exp(-t * 10.5)
        result.append(tone * gain * smoothstep((duration - t) / 0.055))
    return result


def make_string_pluck(frequency: float, seed: int) -> list[float]:
    rng = random.Random(seed)
    duration = 0.48
    count = int(duration * SAMPLE_RATE)
    period = max(2, int(SAMPLE_RATE / frequency))
    ring = [rng.uniform(-1.0, 1.0) for _ in range(period)]
    result: list[float] = []
    for index in range(count):
        position = index % period
        next_position = (position + 1) % period
        current = ring[position]
        ring[position] = 0.5 * (current + ring[next_position]) * 0.988
        t = index / SAMPLE_RATE
        pick = rng.uniform(-1.0, 1.0) * math.exp(-t * 115.0)
        release = smoothstep((duration - t) / 0.07)
        result.append((0.72 * current + 0.035 * pick) * release)
    return result


def mix_looped(
    left: list[float],
    right: list[float],
    sound: list[float],
    start_time: float,
    pan: float,
    gain: float,
) -> None:
    start = int(round(start_time * SAMPLE_RATE)) % len(left)
    left_gain = gain * math.sqrt(0.5 * (1.0 - max(-1.0, min(1.0, pan))))
    right_gain = gain * math.sqrt(0.5 * (1.0 + max(-1.0, min(1.0, pan))))
    for offset, sample in enumerate(sound):
        index = (start + offset) % len(left)
        left[index] += sample * left_gain
        right[index] += sample * right_gain


def make_music() -> list[tuple[float, float]]:
    duration = 32.0
    count = int(duration * SAMPLE_RATE)
    left = [0.0] * count
    right = [0.0] * count
    beat = 0.5  # 120 BPM
    bar_duration = beat * 4.0

    # C major / G / A minor / F major. The harmony is carried only by short
    # notes, never by a continuous pad or drone.
    progression = [
        (130.81, (329.63, 392.00, 523.25, 392.00)),
        (196.00, (293.66, 392.00, 493.88, 392.00)),
        (220.00, (329.63, 440.00, 523.25, 440.00)),
        (174.61, (261.63, 349.23, 440.00, 349.23)),
    ]

    kick = make_kick(710)
    rim_a = make_wood_rim(720)
    rim_b = make_wood_rim(721)
    hats = [make_brushed_hat(730 + index) for index in range(4)]
    bass_cache: dict[float, list[float]] = {}
    pluck_cache: dict[tuple[float, int], list[float]] = {}

    for bar in range(16):
        bar_start = bar * bar_duration
        root, notes = progression[bar % len(progression)]
        phrase_bar = bar % 8

        # A restrained groove with a short fill only at phrase boundaries.
        mix_looped(left, right, kick, bar_start, -0.04, 0.26)
        mix_looped(left, right, kick, bar_start + 2.0 * beat, 0.04, 0.21)
        if phrase_bar in (3, 7):
            mix_looped(left, right, kick, bar_start + 3.5 * beat, 0.10, 0.13)

        mix_looped(left, right, rim_a, bar_start + beat, 0.14, 0.34)
        mix_looped(left, right, rim_b, bar_start + 3.0 * beat, -0.12, 0.32)

        for eighth in range(8):
            # The first bar of each phrase is deliberately more open.
            if phrase_bar in (0, 4) and eighth % 2 != 0:
                continue
            hat_gain = 0.10 if eighth % 2 == 0 else 0.07
            hat_pan = -0.24 if eighth % 2 == 0 else 0.24
            mix_looped(
                left,
                right,
                hats[eighth % len(hats)],
                bar_start + eighth * beat * 0.5,
                hat_pan,
                hat_gain,
            )

        for bass_index, (offset_beats, frequency, gain) in enumerate(
            (
                (0.0, root, 0.14),
                (2.5, root * 1.5, 0.09),
            )
        ):
            if frequency not in bass_cache:
                bass_cache[frequency] = make_muted_bass(
                    frequency,
                    800 + bar % 4 * 10 + bass_index,
                )
            mix_looped(
                left,
                right,
                bass_cache[frequency],
                bar_start + offset_beats * beat,
                -0.06 if bass_index == 0 else 0.08,
                gain,
            )

        note_offsets = (0.75, 1.5, 2.75, 3.5)
        for note_index, (offset_beats, frequency) in enumerate(zip(note_offsets, notes)):
            # Keep the phrase opening sparse, then let the motif develop.
            if phrase_bar in (0, 4) and note_index in (1, 3):
                continue
            cache_key = (frequency, note_index)
            if cache_key not in pluck_cache:
                pluck_cache[cache_key] = make_string_pluck(
                    frequency,
                    900 + int(frequency) + note_index,
                )
            mix_looped(
                left,
                right,
                pluck_cache[cache_key],
                bar_start + offset_beats * beat,
                -0.30 + note_index * 0.20,
                0.29 if note_index < 2 else 0.25,
            )

    # A touch of soft saturation controls coincident transients without
    # flattening the dynamics.
    return [
        (math.tanh(sample_left * 1.15), math.tanh(sample_right * 1.15))
        for sample_left, sample_right in zip(left, right)
    ]


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

    write_stereo(MUSIC_DIR / "minimal_background_loop.wav", make_music(), peak=0.50)
    print(f"Generated {len(assets)} SFX and 1 seamless music loop.")


if __name__ == "__main__":
    main()
