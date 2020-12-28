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

from utils import json_equals


class ClassifyScript:
    def __init__(self, path, args, classified_file_path):
        self.path = path
        self.args = args
        self.classified_file_path = classified_file_path


def run_script(script_path, args):
    """Execute script."""
    argument_string = ' '.join(args)
    command = f'python {script_path} {argument_string}'
    print('\nExecuting:\n', command, '\n')
    return not os.system(command)


def script_equals(script1, script2, output_equality_method='json'):
    """Test equality of scripts."""
    scripts_ok = run_script(script1.path, script1.args) and run_script(script2.path, script2.args)
    output_equal = True

    if scripts_ok:
        if output_equality_method == 'json':
            with open(script1.classified_file_path) as json_file1:
                json_obj1 = json.load(json_file1)
            with open(script2.classified_file_path) as json_file2:
                json_obj2 = json.load(json_file2)
            output_equal = json_equals(json_obj1, json_obj2)

    return scripts_ok and output_equal


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

    script1 = ClassifyScript(args.script1_path, script1_args, args.classified_file_path1)
    script2 = ClassifyScript(args.script2_path, script2_args, args.classified_file_path2)

    equal = script_equals(script1, script2)
    output = 'Scripts equal.' if equal else 'Scripts NOT equal.'

    print(output)

    return 0


if __name__ == '__main__':
    sys.exit(main())
