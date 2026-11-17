"""UUIDv7 generation for Aletheon AID (RFC 9562).

Python >= 3.14: standard uuid.uuid7().
Python < 3.14: deprecated compatibility implementation using stdlib only.

Generation does not register an AID. The caller MUST check registry uniqueness
and durably write the individual file, registry, and issuance log.
"""
from __future__ import annotations

import secrets
import time
import uuid


def uuid7() -> uuid.UUID:
    """Generate RFC 9562 UUIDv7; return a UUID object, not a registered AID."""
    builtin = getattr(uuid, "uuid7", None)
    if builtin is not None:
        result = builtin()
    else:
        timestamp_ms = time.time_ns() // 1_000_000
        if not 0 <= timestamp_ms < (1 << 48):
            raise OverflowError("UUIDv7 timestamp outside 48-bit range")
        # RFC 9562: unix_ts_ms(48) | ver(4) | rand_a(12) | var(2) | rand_b(62).
        rand_a = secrets.randbits(12)
        rand_b = secrets.randbits(62)
        bits = (timestamp_ms << 80) | (0x7 << 76) | (rand_a << 64) | (0b10 << 62) | rand_b
        result = uuid.UUID(int=bits)
    if result.version != 7 or result.variant != uuid.RFC_4122:
        raise ValueError("Generator returned an invalid UUIDv7")
    return result


if __name__ == "__main__":
    print(uuid7())
