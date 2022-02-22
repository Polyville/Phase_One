using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
# Activity IDs:
# 0) setup camp
# 1) rest                   (unused/reserved)
# 2) sleep                  (restore 5 energy)
# 3) eat                    (restore 5 hunger per food)
# 4) sell                   (visit vendor and sell all items)
# 5) buy food               (visit vendor and buy food)
# 6 - 9) n/a                (unused/reserved)

# 10) wood gathering
# 11) stone mining
# 12) items gathering
# 13) treasure hunting

# 14-20) n/a                (unused/reserved)
*/

public class ServerPlayer 
{
    public int PlayerId; // unique ID
    public string PlayerName; // name
    public int PlayerEnergy; // energy
    public int PlayerHunger; // hunger

    public float PlayerMoney; // UGT balance
    public int PlayerFood; // number of food items;
    public List<ServerItem> PlayerInventory; // inventory
    public int PlayerMaxInventorySpace;  // max inventory space (default 10)
    public List<int> PlayerSalesQuality; // keeps track of quality of items sold to vendor
    public int PlayerFoodBought;  // how many food items were bought from vendor
    public int PlayerSleepCycles; 

    public List<int> PlayerSkillsGathering; // gathering skills
    public List<int> PlayerSkillsTreasure; // treasure skills
    public List<int> PlayerSkillsGeneral; // general skills
   
    public int PlayerActivity; // current activity
    public int PlayerRound; // helps with debug


    public void ResetStats()
    {
        PlayerEnergy = 10;
        PlayerHunger = 10;
        PlayerMoney = 0;
        PlayerFood = 0;
        PlayerInventory.Clear();
        PlayerFoodBought = 0;
        PlayerSalesQuality = new List<int>() { 0, 0, 0, 0 };
        PlayerSleepCycles = 0;
        PlayerActivity = 0;
        PlayerRound = 0;
    }

    // returns # of remaining inventory slots
    public int GetRemainingInventorySpace()
    {
        return PlayerMaxInventorySpace - PlayerInventory.Count;
    }

    // sell all items
    public void SellAllItems()
    {
        float soldAmt = 0f;
        foreach (ServerItem item in PlayerInventory)
        {
            soldAmt += (item.ItemSellPrice * item.ItemQuantity);
            PlayerSalesQuality[item.ItemQuality] += 1;
        }

        PlayerMoney += soldAmt;
        PlayerInventory.Clear();

        // SKILL BONUS
        if(PlayerSkillsGeneral[4] == 1)
        {
            int bonusprices = PlayerSkillsGeneral[4] + PlayerSkillsGeneral[5];
            PlayerMoney += (soldAmt * (bonusprices / 10));
        }     
        
    }

    // add items to inventory
    public void AddInventoryItem(ServerItem item)
    {
        // if item can stack
        if(item.ItemType >= 0 && item.ItemType <= 2)
        {
            int x = PlayerInventory.FindIndex(i => i.ItemName == item.ItemName);
            if (x >= 0)
            {
                PlayerInventory[x].ItemQuantity += item.ItemQuantity;
            }
            else
            {
                PlayerInventory.Add(item);
            }
        }
        else
        {
            PlayerInventory.Add(item);
        }
    }

    // assign work or activity for the next round
    public void AssignWork(int workId)
    {
        PlayerActivity = workId;
    }

    public void BuyFood(int quantity)
    {
        PlayerFoodBought += quantity;
        PlayerFood += quantity;
        PlayerMoney -= (quantity * 5); // each food is 5 UGT       
    }

    public void EatOneFood()
    {
        PlayerFood -= 1;
        PlayerHunger += 5; // each food restores 5 hunger

        // SKILL BONUS: 50% chance to restore 1 energy while eating
        if (PlayerSkillsGeneral[0] == 1 && Random.Range(0, 100) < 50)
            PlayerEnergy += 1;

    }
    public void SleepOneCycle()
    {
        PlayerSleepCycles += 1;
        PlayerEnergy += 5; // restore 5 energy

        // SKILL BONUS: 75% chance to restore 2 extra energy while sleeping
        if (PlayerSkillsGeneral[1] == 1 && Random.Range(0, 100) < 75)
            PlayerEnergy += 2;

        // Max Energy is 10
        PlayerEnergy = (PlayerEnergy > 10) ? 10 : PlayerEnergy;

    }
    public int GetTreasureSkillBase()
    {
        return PlayerSkillsTreasure.Take(4).Sum() * 5; // each point is worth 5%
    }
    public int GetGatheringSkillExtraItemChance()
    {
        // calculate extra item bonus chance
        int retval = 0;
        retval = PlayerSkillsGathering[0] == 1 ? 10 : retval;
        retval = PlayerSkillsGathering[1] == 1 ? 15 : retval;
        retval = PlayerSkillsGathering[2] == 1 ? 20 : retval;
        retval = PlayerSkillsGathering[3] == 1 ? 25 : retval;
        return retval;
    }

    public bool IsSkillSelected(int skillIndex, int treeId)
    {
        bool retval = false;
        if (treeId == 1)
            return PlayerSkillsGathering[skillIndex] == 1;

        if (treeId == 2)
            return PlayerSkillsTreasure[skillIndex] == 1;

        if (treeId == 3)
            return PlayerSkillsGeneral[skillIndex] == 1;

        return retval;
    }

    public string GetFinalStats()
    {
        string outStr = "";
        outStr += "<color=#F88C33>TOTAL EARNED: </color>" + PlayerMoney + " UGT \n";

        outStr += "<color=#F88C33>TOTAL Items Sold: </color>" + PlayerSalesQuality.Sum() + "\n";
        outStr += "<color=#F88C33>NORMAL Sold: </color>" + PlayerSalesQuality[0] + "\n";
        outStr += "<color=#F88C33>RARE Sold: </color>" + PlayerSalesQuality[1] + "\n";
        outStr += "<color=#F88C33>EPIC Sold: </color>" + PlayerSalesQuality[2] + "\n";
        outStr += "<color=#F88C33>LEGENDARY Sold: </color>" + PlayerSalesQuality[3] + "\n";

        outStr += "<color=#F88C33>SLEEP Cycles: </color>" + PlayerSleepCycles + " \n";
        outStr += "<color=#F88C33>FOOD Bought: </color>" + PlayerFoodBought + " <size=70%>(spent: "+ (PlayerFoodBought*5) +" UGT)</size> \n";

        return outStr;

    }

    public void PrintDebugStats()
    {
        Debug.Log("==================================");
        Debug.Log("ROUND #" + PlayerRound);
        Debug.Log("Activity: " + PlayerActivity);
        Debug.Log("Money: " + PlayerMoney);
        Debug.Log("Inventory worth: " + PlayerInventory.Sum(item => item.ItemQuantity * item.ItemSellPrice).ToString());
        Debug.Log("Food: " + PlayerFood);
        Debug.Log("Food bought: " + PlayerFoodBought);
        Debug.Log("Slept: " + PlayerSleepCycles + " rounds");
        Debug.Log("Hunger: " + PlayerHunger);
        Debug.Log("Energy: " + PlayerEnergy);
        Debug.Log("==================================");
    }
}
