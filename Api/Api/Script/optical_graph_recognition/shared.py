"""Module containing global constants, functions, ..."""

import numpy as np


class Mode:
    # Input mode indicates visual properties of given graph photo
    GRID_BG = 1     # Hand drawn on grid/lined piece of paper (grid/lined notebook etc.)
    CLEAN_BG = 2    # Hand drawn on empty uniform color background (on board, empty piece of paper, editor (paint))
    PRINTED = 3     # Printed (e.g. from paper, publication, book...)
    AUTO = 4        # Automatically chosen mode based on average background distance to objects

    @staticmethod
    def get_mode(cli_arg: str):
        """
        Resolves Mode code from command line input string
        :param cli_arg: command line argument indicating Mode for processing
        :return: Mode for processing
        """
        if cli_arg == "GRID_BG":
            return Mode.GRID_BG
        elif cli_arg == "CLEAN_BG":
            return Mode.CLEAN_BG
        elif cli_arg == "PRINTED":
            return Mode.PRINTED
        elif cli_arg == "AUTO":
            return Mode.AUTO
        else:
            print("1: Mode \""+cli_arg+"\" is not a viable mode.")
            return -1


class Color:
    # Logical colors
    OBJECT = 255
    BG = 0

    # Physical colors - BGR
    BLUE = (255, 0, 0)
    GREEN = (0, 255, 0)
    RED = (0, 0, 255)
    BLACK = (0, 0, 0)
    GRAY = (127, 127, 127)
    WHITE = (255, 255, 255)
    YELLOW = (0, 255, 255)
    ORANGE = (0, 140, 255)


class Kernel:
    k3 = np.ones((3, 3), dtype=np.uint8)
    k5 = np.ones((5, 5), dtype=np.uint8)
    k7 = np.ones((7, 7), dtype=np.uint8)
