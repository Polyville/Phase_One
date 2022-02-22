import typing
from operator import itemgetter
from random import Random, random

from .server_item import ServerItem


class ServerPlayer:
    def __init__(self, name):

        self.player_name = name
        self.player_id = 0
        self.player_energy = 10
        self.player_hunger = 10
        
        self.player_money = 0
        self.player_inventory = {}
        self.player_max_inventory = 10
        self.player_food = 0
        
        self.player_sales_quality = [0,0,0,0,0]
        self.player_food_bought = 0
        self.player_sleep_cycles = 0

        self.player_skills_gathering = [0, 0, 0, 0, 0, 0]
        self.player_skills_treasure = [0, 0, 0, 0, 0, 0, 0]
        self.player_skills_general = [0, 0, 0, 0, 0, 0]

        self.player_activity = 0
        self.player_round = 0

        self.work_completed = 0

    def reset_stats(self):
        
        self.player_energy = 10
        self.player_hunger = 10
        self.player_money = 0
        self.player_food = 0

        self.player_inventory.clear()
        self.player_max_inventory = 10

        self.player_sales_quality = [0,0,0,0,0]
        self.player_food_bought = 0
        self.player_sleep_cycles = 0

        self.player_skills_gathering = [0, 0, 0, 0, 0, 0]
        self.player_skills_treasure = [0, 0, 0, 0, 0, 0, 0]
        self.player_skills_general = [0, 0, 0, 0, 0, 0]

        self.player_activity = 0
        self.player_round = 0


    def get_remaining_inventory_space(self) -> int:
        return self.player_max_inventory - len(self.player_inventory)

    def sell_all_items(self):

        amt_sold = 0
        helper_item_ct = 0

        for k in self.player_inventory:
            helper_item_ct += self.player_inventory[k].itemquantity
            amt_sold += (self.player_inventory[k].itemquantity * self.player_inventory[k].itemsellprice)
            self.player_sales_quality[self.player_inventory[k].itemquality] += 1
            
        self.player_money += amt_sold
        self.player_inventory.clear()


    # ADD item to inventory
    def add_inventory_item(self, item: ServerItem):
        
        # item can stack
        if item.itemtype >= 0 and item.itemtype <= 2:
            if item.itemname in self.player_inventory:
                self.player_inventory[item.itemname].itemquantity += item.itemquantity
            else:
                self.player_inventory[item.itemname] = item
        else:
            self.player_inventory[item.itemname] = item
        

    # assign work for next round
    def assign_work(self, work_id):
        self.player_activity: int = work_id

        if work_id >= 10 and work_id <= 13:
            self.work_completed += 1


    def eat_one_food(self):
        self.player_food -= 1
        self.player_hunger += 5

        # SKILL BONUS: 50% chance to restore 1 energy while eating
        if self.player_skills_general[0] == 1 and Random.randrange(0,100) < 50:
            self.player_energy += 1


    def buy_food(self, quantity):
        self.player_food_bought += quantity
        self.player_food += quantity
        self.player_money -= (quantity * 5) # each food costs 5 UGT
        


    def sleep_one_cycle(self):
        self.player_sleep_cycles += 1
        self.player_energy += 5

        # SKILL BONUS: 75% chance to restore 2 additional energy while sleeping
        if self.player_skills_general[1] == 1 and Random.randrange(0,100) < 75:
            self.player_energy += 2
        
        # MAX energy is 10
        self.player_energy = self.player_energy if self.player_energy <= 10 else 10


    def get_treasure_skill_base(self) -> int:
        # each point is worth 5%
        return sum(self.player_skills_treasure[0:3]) * 5


    def get_gathering_skill_extra_item_chance(self) -> int:
        retval = 0

        retval = 10 if self.player_skills_gathering[0] == 1 else 0
        retval = 15 if self.player_skills_gathering[1] == 1 else 0
        retval = 20 if self.player_skills_gathering[2] == 1 else 0
        retval = 25 if self.player_skills_gathering[3] == 1 else 0
        return retval


    def __str__(self) -> str:
        txt = ""
        txt += "Round {} energy {} hunger {} money {} inventory {} activity {} food {} ".format(self.player_round, self.player_energy, self.player_hunger, self.player_money, len(self.player_inventory), self.player_activity, self.player_food)
        return txt    

