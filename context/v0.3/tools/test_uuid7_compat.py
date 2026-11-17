import unittest
import uuid
import warnings
from unittest.mock import patch
import uuid7_compat


class TestUUID7(unittest.TestCase):
    def test_generation(self):
        with warnings.catch_warnings():
            warnings.simplefilter("ignore", DeprecationWarning)
            ids = [uuid7_compat.uuid7() for _ in range(1000)]
        self.assertEqual(len(set(ids)), 1000)
        self.assertTrue(all(x.version == 7 and x.variant == uuid.RFC_4122 for x in ids))
        self.assertTrue(all(abs((x.int >> 80) - __import__("time").time_ns() // 1_000_000) < 10000 for x in ids))

    def test_builtin_preferred(self):
        marker = uuid.UUID("0199aabb-ccdd-7abc-8def-123456789abc")
        with patch.object(uuid7_compat.uuid, "uuid7", return_value=marker, create=True) as native:
            self.assertEqual(uuid7_compat.uuid7(), marker)
            native.assert_called_once()

    def test_invalid_native_rejected(self):
        with patch.object(uuid7_compat.uuid, "uuid7", return_value=uuid.uuid4(), create=True):
            with self.assertRaises(ValueError):
                uuid7_compat.uuid7()


if __name__ == "__main__":
    unittest.main()
