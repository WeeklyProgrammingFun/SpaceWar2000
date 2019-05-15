from Pybot import Pybot
import logging
import sys

logger = logging.getLogger(__name__)
handler = logging.StreamHandler()
#handler = logging.FileHandler("Pybot.log", mode='w', encoding=None, delay=False)
formatter = logging.Formatter('[%(asctime)s] %(name)-12s %(levelname)-8s --> %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)
mybot = Pybot()

def make_moves(self, state):
    moves = "MOVE "
    friendly_planets = state.get_all_friendly_planets()
    #TODO get geometrical mean
    target = state.get_closest_target()
    for planet in friendly_planets:
        #TODO a planet should know its own index.  That would make this easier.
        moves = moves + "L {} {} {} ".format(str(planet[0]), str(target), str(planet[1]))
    moves = moves + "E"
    logger.debug(moves)
    moves = moves + "\n"
    print(moves)
    sys.stdout.flush()

mybot.make_moves = make_moves

mybot.run()