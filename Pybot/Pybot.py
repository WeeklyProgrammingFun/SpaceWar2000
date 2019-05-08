import sys
import logging

logger = logging.getLogger(__name__)
handler = logging.FileHandler("Pybot.log", mode='w', encoding=None, delay=False)
formatter = logging.Formatter('[%(asctime)s] %(name)-12s %(levelname)-8s --> %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)

class Planet:
    def __init__(self, x, y, owner, ships, growth):
        self.x = x
        self.y = y
        self.owner = owner
        self.ships = ships
        self.growth = growth
        logger.debug("PLANET CREATED: x:{} y:{} owner:{} ships:{} growth:{}".format(x, y, owner, ships, growth))

    def log(self):
        logger.debug("PLANET ( x:{} y:{} owner:{} ships:{} growth:{} )".format(
            self.x, self.y, self.owner, self.ships, self.growth))     

class Fleet:
    def __init__(self, owner, ships, source, dest, turnsRemaining):
        self.owner = owner
        self.ships = ships
        self.source = source
        self.dest = dest
        self.turnsRemaining = turnsRemaining
        logger.debug("FLEET CREATED: owner:{} ships:{} source:{} dest:{} turns:{}".format(owner, ships, source, dest, turnsRemaining))

    def log(self):
        logger.debug("FLEET ( owner:{} ships:{} source:{} dest:{} turns:{} )".format(
            self.owner, self.ships, self.source, self.dest, self.turnsRemaining))
    

class State:
    def __init__(self):
        logger.debug("STATE CREATED")        
        self.planets = []
        self.fleets = []

    def create_planet(self, x, y, owner, ships, growth):
        logger.debug("CREATING PLANET: {}".format(len(self.planets)))
        self.planets.append(Planet(x, y, owner, ships, growth))

    def create_fleet(self, owner, ships, source, dest, turnsRemaining):
        logger.debug("CREATING FLEET: {}".format(len(self.fleets)))
        self.fleets.append(Fleet(owner, ships, source, dest, turnsRemaining))

    def get_first_enemy_planet(self):
        #TODO fix logger format to put function in log
        logger.debug("GETTING FIRST ENEMY PLANET")
        for index, planet in enumerate(self.planets):
            #TODO magic number
            if planet.owner == str(2):
                logger.debug("FIRST ENEMY IS PLANET {}".format(index))
                return index

    def get_first_neutral_planet(self):
        """ Returns first neutral planets index or False if no neutral planets found """
        #TODO This could probably be merged with get_first_enemy_planet()
        logger.debug("GETTING FIRST NEUTRAL PLANET")
        for index, planet in enumerate(self.planets):
            #TODO magic number
            if planet.owner == str(0):
                logger.debug("FIRST NEUTRAL IS PLANET {}".format(index))
                return index
        return False                

    def get_all_friendly_planets(self):
        logger.debug("GETTING FRIENDLY PLANETS")
        planets = []
        for index, planet in enumerate(self.planets):
            planet.log()
            #TODO magic number
            if planet.owner == str(1):
                logger.debug("PLANET IS FRIENDLY")
                planets.append([index, planet.ships])
        return planets

class Pybot:

    def make_moves(self, state):
        moves = "MOVE "
        friendly_planets = state.get_all_friendly_planets()
        target = state.get_first_neutral_planet()
        if target is False: #TODO have to use "is" because 0 == False.  Is there a better way to do this?
            target = state.get_first_enemy_planet()
        for planet in friendly_planets:
            #TODO a planet should know its own index.  That would make this easier.
            moves = moves + "L {} {} {} ".format(str(planet[0]), str(target), str(planet[1]))
        moves = moves + "E"
        logger.debug(moves)
        moves = moves + "\n"
        print(moves)
        sys.stdout.flush()

    #TODO can this be taken out???
    def parse_state(self, state):
        logger.debug("DATA INCOMING")
        spaceState = State()
        state_i = 0
        planet_length = 6
        fleet_length = 6
        while state_i < len(state):
            #TODO use and instead of nested if?
            if state[state_i] == "P":
                if (state_i + planet_length) <= len(state):
                    spaceState.create_planet(*state[state_i+1:state_i + planet_length])
                    state_i = state_i + planet_length
            #TODO use and instead of nested if?
            elif state[state_i] == "F" and (state_i + fleet_length) <= len(state):
                if (state_i + fleet_length) <= len(state):
                    spaceState.create_fleet(*state[state_i+1:state_i + fleet_length])                    
                    state_i = state_i + fleet_length
            else:
                logger.debug("DONE WITH STATE")
                break
        return spaceState

    def state_loop(self, state):
        logger.debug("DATA INCOMING")
        planet_length = 6
        fleet_length = 6
        
        spaceState = State()
        #TODO fix these "True" loops
        while True:
            logger.debug("State: {}".format(state))
            #TODO use and instead of nested if?
            if state[0] == "P":
                if planet_length <= len(state):
                    spaceState.create_planet(*state[1:planet_length])
            #TODO use and instead of nested if?
            elif state[0] == "F":
                if (fleet_length) <= len(state):
                    spaceState.create_fleet(*state[1:fleet_length])
            elif state[0] == "E":
                return spaceState                
            else:
                # If State doesn't begin with P, F, or E, input is broken
                logger.debug("DONE WITH STATE")
                return spaceState
            state = sys.stdin.readline().split()



    def parse_input(self, data_in):
        logger.debug("Parsing: {}".format(data_in))
        if data_in.split()[0] == "START":
            logger.debug("GAME STARTING")
            logger.debug("ENEMY: {}".format(data_in.split()[1]))
            logger.debug("SEED: {}".format(data_in.split()[2]))
        if data_in.split()[0] == "STATE":
            state = self.state_loop(data_in.split("STATE")[1].split())
            self.make_moves(state)
        if data_in.split()[0] == "QUIT":
            return False
        return True
     
    def run(self):
        while self.parse_input(sys.stdin.readline()):
            logger.debug("MAIN LOOP")