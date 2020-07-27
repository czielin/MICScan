"""
usage: classify_integration.py [-h]
                               script1_path
                               script2_path
                               model_path
                               features_file_path
                               vocabulary_path
                               classified_file_path1
                               classified_file_path2
"""

from argparse import ArgumentParser
import json
import os
import sys


class Script:
    def __init__(self, path, args, classified_file_path):
        self.path = path
        self.args = args
        self.classified_file_path = classified_file_path


def json_equals(path1, path2):
    """Test equality of json files"""
    with open(path1) as json_file1:
        json_obj1 = json.load(json_file1)
    with open(path2) as json_file2:
        json_obj2 = json.load(json_file2)

    return json.dumps(json_obj1, sort_keys=True) == json.dumps(json_obj2, sort_keys=True)


def run_script(script_path, args):
    """Execute script."""
    argument_string = ' '.join(args)
    command = f'python {script_path} {argument_string}'
    print('\nExecuting:\n', command, '\n')
    return not os.system(command)


def script_equals(script1, script2):
    """Test equality of scripts."""
    return run_script(script1.path, script1.args) and run_script(script2.path, script2.args) \
        and json_equals(script1.classified_file_path, script2.classified_file_path)


def main():
    parser = ArgumentParser()
    parser.add_argument(('script1_path'))
    parser.add_argument(('script2_path'))
    parser.add_argument('model_path')
    parser.add_argument('features_file_path')
    parser.add_argument('vocabulary_path')
    parser.add_argument('classified_file_path1')
    parser.add_argument('classified_file_path2')

    args = parser.parse_args()

    script1_args = [
        args.model_path,
        args.features_file_path,
        args.vocabulary_path,
        args.classified_file_path1
    ]

    script2_args = [
        args.model_path,
        args.features_file_path,
        args.vocabulary_path,
        args.classified_file_path2
    ]

    script1 = Script(args.script1_path, script1_args, args.classified_file_path1)
    script2 = Script(args.script2_path, script2_args, args.classified_file_path2)

    equal = script_equals(script1, script2)
    output = 'Scripts equal.' if equal else 'Scripts NOT equal.'

    print(output)

    return 0


if __name__ == '__main__':
    sys.exit(main())
