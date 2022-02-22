# CHANGE SKILLS HERE
# Valid values are (int) 0 and 1
# All other values are ignored
# Please note that on live servers
# a skill can be activated only when 
# the previous one is active.

# Don't forget to save the file, then run "python3 app.py"


# GATHERING SKILLS
# gathering activities produce wood, stone and items 
# all gathering material "stacks" in the inventory (based on quality - NORMAL or RARE)
# Sell Prices are: 5 UGT NORMAL items, 10 UGH RARE items
# there is always 100% chance to get at least one item while gathering

gathering_skill_a = 1  # 10% chance to receive an extra item
gathering_skill_b = 1  # 15% chance to receive an extra item
gathering_skill_c = 1  # 20% chance to receive an extra item
gathering_skill_d = 1  # 25% chance to receive an extra item
gathering_skill_e = 1  # 10% chance to receive an additional RARE item (worth 10 UGT)
gathering_skill_f = 1  # 10% chance to restore 1 energy while gathering

# TREASURE HUNTING SKILLS
# Unlike gathering, treasure hunting has only a 20% base chance to produce anything.
# There is also 1% chance to upgrade your item to RARE quality.
# Treasure items are unique and don't stack in the inventory.
# However they can be very lucrative.
# Sell Prices are: 10 UGT NORMAL, 25 UGT RARE, 50 UGT EPIC, 200 UGT LEGENDARY 

treasure_skill_a = 1  # +5% chance to receive and item (this increases the base chance from 20 to 25%)
treasure_skill_b = 1  # +5% chance to receive and item (this increases the base chance from 20 to 30%)
treasure_skill_c = 1  # +5% chance to receive and item (this increases the base chance from 20 to 35%)
treasure_skill_d = 1  # +5% chance to receive and item (this increases the base chance from 20 to 40%)
treasure_skill_e = 1  # +25% chance to upgrade to RARE
treasure_skill_f = 1  # +5% chance to upgrade to EPIC
treasure_skill_g = 1  # +1% chance to upgrade to LEGENDARY

# GENERAL CHARACTER SKILLS

general_skill_a = 1  # 50% chance to restore 1 energy while eating
general_skill_b = 1  # 75% chance to restore 2 additional energy while sleeping
general_skill_c = 1  # +20 additional inventory space
general_skill_d = 1  # 25% chance to get 1 food from gathering
general_skill_e = 1  # +10% increased prices
general_skill_f = 1  # +10% increased prices 



# DO NOT EDIT
g_skills = [gathering_skill_a, gathering_skill_b, gathering_skill_c, gathering_skill_d, gathering_skill_e, gathering_skill_f]
t_skills = [treasure_skill_a, treasure_skill_b, treasure_skill_c, treasure_skill_d, treasure_skill_e, treasure_skill_f, treasure_skill_g]
c_skills = [general_skill_a, general_skill_b, general_skill_c, general_skill_d, general_skill_e, general_skill_f]
