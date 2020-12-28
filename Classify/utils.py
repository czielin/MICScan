import json


def json_equals(json_obj1, json_obj2):
    """Test equality of json objects"""
    return json.dumps(json_obj1, sort_keys=True) == json.dumps(json_obj2, sort_keys=True)
