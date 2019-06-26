import sys
import logging

logger = logging.getLogger(__name__)
#handler = logging.StreamHandler()
handler = logging.FileHandler("Pybot.log", mode='w', encoding=None, delay=False)
formatter = logging.Formatter('[%(asctime)s] %(name)-12s %(levelname)-8s --> %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)
logger.setLevel(logging.DEBUG)

class Planet:
    def __init__(self, x, y, owner, ships, growth, index):
        self.x = x
        self.y = y
        self.owner = owner
        self.ships = ships
        self.growth = growth
        self.index = index
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
        self.planets.append(Planet(x, y, owner, ships, growth, len(self.planets)))

    def create_fleet(self, owner, ships, source, dest, turnsRemaining):
        logger.debug("CREATING FLEET: {}".format(len(self.fleets)))
        self.fleets.append(Fleet(owner, ships, source, dest, turnsRemaining))

    def get_distance(self, pair1, pair2):
        """
        Get the distance between two points
        """
        logger.debug("GET DISTANCE BETWEEN {} and {}".format(pair1, pair2))
        x = float(pair1[0]) - float(pair2[0])
        y = float(pair1[1]) - float(pair2[1])
        logger.debug("COMPUTING DISTANCE... with {} and {}".format(x, y))
        distance = ((x * x) + (y * y))
        logger.debug("DISTANCE IS {}".format(distance))
        return distance

    def get_closest_target(self, pair, old_targets=[]):
        logger.debug("GETTING CLOSEST TARGET")
        target = None
        distance = None
        for index, planet in enumerate(self.planets):
            #TODO magic number
            logger.debug("PLANET OWNER: {}".format(planet.owner))
            if planet.owner != str(1):
                new_distance = self.get_distance(pair, [planet.x, planet.y])
                logger.debug("FOUND A HOSTILE PLANET, DISTANCE: {}".format(new_distance))
                if (target == None or new_distance < distance) and str(index) not in old_targets:
                    logger.debug("AND IT'S CLOSER")
                    target = index
                    distance = self.get_distance(pair, [planet.x, planet.y])
        return target

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

    def who_owns(self, index, moves, turns):
        logger.debug("Who will own planet {}".format(str(index)))
        if self.planets[index].owner == str(1):
            # I already own the planet, return
            return str(1)
        #Get fleets from new moves
        engaged_fleets = self.get_newly_engaged_fleets(moves, index)            
        pop = self.planets[index].ships
        owner = self.planets[index].owner
        for fleet in self.fleets:
            if fleet.dest == index:
                engaged_fleets.append(fleet)
        logger.debug("{} fleets engaged".format(len(engaged_fleets)))
        for turn in range(turns):
            for fleet in engaged_fleets:
                #TODO remove fleets that land
                if fleet.turnsRemaining == turn:
                    # This fleet landed, do its action
                    if fleet.owner == owner:
                        pop = pop + fleet.ships
                    else:
                        pop = pop - fleet.ships
                if pop < 0:
                    logger.debug("{} now owns the planet".format(fleet.owner))
                    #TODO abs value?  OR is this good enough?
                    pop = pop * (-1)
                    owner = fleet.owner
        logger.debug("{} will own the planet".format(owner))
        return owner

    def get_newly_engaged_fleets(self, state, index):
        logger.debug("Getting newly engaged fleets...")
        planet_length = 6
        fleet_length = 6

        engaged_fleets = []
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
                logger.debug("Done getting fleets!")
                break
        
        for fleet in spaceState.fleets:
            if fleet.dest == index:
                engaged_fleets.append(fleet)
        logger.debug("Returning Newly Engaged Fleets!")
        return(engaged_fleets)




class Pybot:
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
            self.make_moves(self, state)
        if data_in.split()[0] == "QUIT":
            return False
        return True
     
    def run(self):
        while self.parse_input(sys.stdin.readline()):
            logger.debug("MAIN LOOP")