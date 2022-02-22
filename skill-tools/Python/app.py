
import os
import sys
import random
import datetime
import time
import typing
from data.server_item import ServerItem
from data.server_player import ServerPlayer 
import change_skills as chsk

CURRENT_ROUND = 0
MAX_ROUNDS = 4 * 24 * 28

dict_server_players: ServerPlayer = {}
list_treasure_prices = [25, 50, 150, 450]


def create_players():
    for i in range(1, 11):
        pname = "UnskilledPlayer-{}".format(i)
        player: ServerPlayer = ServerPlayer(pname)
        player.player_id = i
        dict_server_players[pname] = player
        
    skilled_player: ServerPlayer = ServerPlayer("SkilledPlayer-0")
    skilled_player.player_id = 0

    if chsk.general_skill_c == 1:
        skilled_player.player_max_inventory = 30

    skilled_player.player_skills_gathering = chsk.g_skills
    skilled_player.player_skills_treasure = chsk.t_skills
    skilled_player.player_skills_general = chsk.c_skills

    dict_server_players[skilled_player.player_name] = skilled_player
    



def generate_random_int(p_name, i_min, i_max) -> int:
    # generate random number
    # using timestamp and player name for test
    # this is replaced by first tx in the latest mainnet block + player NFT id
    seed = "{}{}{}".format(datetime.datetime.now(), p_name, random.randint(1,5000))
    random.seed(seed)
    rnd = random.randrange(i_min,i_max)
    return rnd


def get_random_work_id(p_name):
    rnd_int: int = generate_random_int(p_name, 10, 14)
    return rnd_int

def receive_round_reward(p_name):
    aid: int = int(dict_server_players[p_name].player_activity)

    # reward gathering
    if aid >= 10 and aid <= 12:
        reward_base_item(p_name, aid)
    
    # reward treasure
    if aid == 13:
        reward_treasure_item(p_name, aid)

    #reward sleep
    if aid == 2:
        reward_sleep(p_name, aid)

    # reward eating
    if aid == 3:
        reward_eating(p_name, aid)

    # reward buying food (and sell all items)
    if aid == 5:
        reward_buing_food(p_name, aid)

    # reward selling items
    if aid == 4:
        reward_selling_items(p_name, aid)

    
def player_health_check(p_name):

    # CHECK ENERGY
    if dict_server_players[p_name].player_energy <= 2:
        return 2

    # CHECK HUNGER
    if dict_server_players[p_name].player_hunger <= 1:
        if dict_server_players[p_name].player_food < 2:
            return 5
        else:
            return 3

    # CHECK INVENTORY SPACE
    if dict_server_players[p_name].get_remaining_inventory_space() <= 0:
        return 4

    # ALL CHECK PASSED
    return 0



def reward_base_item(p_name, p_acid):
    # REWARD WOOD, STONE or OTHER ITEM (ActivityId 10, 11, 12)
    aid = p_acid - 10
    itemnames = ["Wood", "Stone", "Item"]

    item: ServerItem = ServerItem(p_acid + CURRENT_ROUND)
    item.itemname = itemnames[aid]
    item.itemquantity = 1
    item.itemquality = 0
    item.itemsellprice = 10
    item.itemtype = aid

    # SKILL BONUS: extra item
    extrachance: int = dict_server_players[p_name].get_gathering_skill_extra_item_chance()
    if extrachance > 0 and generate_random_int(p_name, 0, 100) < extrachance:
        dict_server_players[p_name].add_inventory_item(item)
    

    # SKILL BONUS: upgrade to RARE item
    if dict_server_players[p_name].player_skills_gathering[4] == 1 and generate_random_int(0,100) < 10:
        item.itemname = "{}_RARE".format(itemnames[aid])
        item.itemquality = 1
        item.itemsellprice = 20
        dict_server_players[p_name].add_inventory_item(item)
    else:
        dict_server_players[p_name].add_inventory_item(item)
    
    # SKILL BONUS: 25% chance to receive 1 food
    if dict_server_players[p_name].player_skills_general[3] == 1 and generate_random_int(0,100) < 25:
        dict_server_players[p_name].player_food += 1
    

    # SKILL BONUS: 10% chance to restore one energy
    if dict_server_players[p_name].player_skills_general[5] == 1 and generate_random_int(0,100) < 10:
        dict_server_players[p_name].player_energy += 1


def reward_treasure_item(p_name, p_acid):
    # calculate chances
    basechance = 20
    rarechance = 1
    epichance = 0
    legendarychance = 0

    # SKILL BONUS: extra % 
    basechance += dict_server_players[p_name].get_treasure_skill_base()
    rarechance += (dict_server_players[p_name].player_skills_treasure[4] * 25)
    epichance += (dict_server_players[p_name].player_skills_treasure[5] * 5)
    legendarychance += dict_server_players[p_name].player_skills_treasure[6]

    baseitemreceived: bool = generate_random_int(p_name, 0,100) < basechance

    if baseitemreceived:
        # UPGRADE?

        finalquality: int = 0
        finalquality = 1 if generate_random_int(p_name, 0,100) < rarechance else 0
        finalquality = 2 if generate_random_int(p_name, 0,100) < epichance else 0
        finalquality = 3 if generate_random_int(p_name, 0,100) < legendarychance else 0

        # create item
        item: ServerItem = ServerItem(CURRENT_ROUND)
        item.itemname = "Treasure-{}".format(CURRENT_ROUND)
        item.itemquality = finalquality
        item.itemsellprice = list_treasure_prices[finalquality]
        item.itemtype = 3
        item.itemquantity = 1

        dict_server_players[p_name].add_inventory_item(item)


def reward_sleep(p_name, p_acid):
    dict_server_players[p_name].sleep_one_cycle()

def reward_eating(p_name, p_acid):
    dict_server_players[p_name].eat_one_food()

def reward_buing_food(p_name, p_acid):
    # sell all items
    reward_selling_items(p_name, p_acid)
    # buy 5 food 
    dict_server_players[p_name].buy_food(5) 

def reward_selling_items(p_name, p_acid):
    # SELL ALL ITEMS
    dict_server_players[p_name].sell_all_items()


def print_sim_results():
    for k in dict_server_players:
        if k != "SkilledPlayer-0":
            print("{} finished with {} UGT".format(k, dict_server_players[k].player_money))
    
    skp = dict_server_players["SkilledPlayer-0"]
    print("=== YOU: SkilledPlayer-0 finished with {} UGT ===".format(skp.player_money))
    print("Earned: {} UGT".format(skp.player_money))
    print("Rounds: {}".format(skp.player_round))
    print("Food bought: {}".format(skp.player_food_bought))
    print("Food bought: {}".format(skp.player_food_bought))
    print("Total work completed: {}".format(skp.work_completed))


def sim_loop():

    global CURRENT_ROUND 

    while CURRENT_ROUND <= MAX_ROUNDS:
        for k in dict_server_players:
            dict_server_players[k].player_round = CURRENT_ROUND

            # REDUCE ENERGY AND HUNGER
            dict_server_players[k].player_energy -= 1
            dict_server_players[k].player_hunger -= 1

            # REWARD LAST ROUND
            receive_round_reward(k)

            # HEALTH CHECK
            health_check_result = player_health_check(k)

            if health_check_result != 0:
                dict_server_players[k].assign_work(health_check_result)
            else:
                dict_server_players[k].assign_work(get_random_work_id(k))
            
        # NEW CYCLE
        CURRENT_ROUND += 1

    # DONE: Sell ALL items
    for k in dict_server_players:
        dict_server_players[k].sell_all_items()
    

    # == FINISHED ===
    print_sim_results()


def main():
    create_players()
    sim_loop()
    


if __name__ == "__main__":
    main() 
