import pytest

from utils import json_equals


@pytest.fixture
def kevin_json_dict():
    return {"name": "Kevin", "city": "San Diego"}


@pytest.fixture
def chris_json_dict():
    return {"name": "Chris", "city": "Los Angeles", "children": ['Arianna']}


@pytest.fixture
def chris_json_str():
    return '{"name": "Chris", "city": "Los Angeles", "children": ["Arianna"]}'


@pytest.fixture
def chris_json_str2():
    # identical str value as chris_json_str, but different reference
    return '{"name": "Chris", "city": "Los Angeles", "children": ["Arianna"]}'


@pytest.fixture
def kevin_json_str():
    return '{"name": "Kevin", "city": "San Diego"}'


def test_json_equals_1(kevin_json_dict, chris_json_dict):
    """Test for json_equals. Unequal json dictionaries."""
    assert not json_equals(kevin_json_dict, chris_json_dict)


def test_json_equals_2(kevin_json_str, chris_json_str):
    """Test for json_equals. Unequal json strings."""
    assert not json_equals(chris_json_str, kevin_json_str)


def test_json_equals_3(chris_json_str, chris_json_str2):
    """Test for json_equals. Equal json strings."""
    assert chris_json_str == chris_json_str2  # values are equal
    assert chris_json_str is not chris_json_str2  # not the same reference
    assert json_equals(chris_json_str, chris_json_str2)


def test_json_equals_4(kevin_json_dict):
    """Test for json_equals. Equal json dictionaries."""
    kevin_json_dict2 = kevin_json_dict.copy()
    assert kevin_json_dict == kevin_json_dict2  # values are equal
    assert kevin_json_dict is not kevin_json_dict2  # not the same reference
    assert json_equals(kevin_json_dict, kevin_json_dict2)
