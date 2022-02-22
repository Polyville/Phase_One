using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SimulationManager : MonoBehaviour
{
    // UI
    public TextMeshProUGUI label_skillcount;
    public TextMeshProUGUI label_skillcost;
    // UI results
    public TextMeshProUGUI label_resultsothers;
    public TextMeshProUGUI label_resultsplayer;
    public Button restartButton;

    // SKILL UI
    public List<Image> skillSelectorsGathering;
    public List<Image> skillSelectorsTreasure;
    public List<Image> skillSelectorsGeneral;

    private AudioSource asrc; // play audio 

    private int ROUND = 1;  // current round 
    private int MAXROUNDS = 4 * 24 * 28; // maximum number of rounds
    private List<int> treasurePrices = new List<int>() { 20, 50, 150, 450 };
    private List<ServerPlayer> PlayerList = new List<ServerPlayer>();  // list of players (max 10)


    void Start()
    {
        asrc = GetComponent<AudioSource>();
        MAXROUNDS = (24 * 4 * 28);
        InitPlayers();
        //InitSimulation();
    }

    // RESET everything and start
    public void RestartSim()
    {
        restartButton.enabled = false;
        ROUND = 1;
        foreach (ServerPlayer p in PlayerList)
        {
            p.ResetStats();
        }
        label_resultsothers.text = "";
        label_resultsplayer.text = "";

        InitSimulation();
    }

    public void InitSimulation()
    {
        // SKILL BONUS: extra inventory
        if (PlayerList[0].PlayerSkillsGeneral[2] == 1)
            PlayerList[0].PlayerMaxInventorySpace = 30;
        else
            PlayerList[0].PlayerMaxInventorySpace = 10;

        // START SIM
        SimLoop();
    }

    public void SimLoop()
    {

        while (ROUND <= MAXROUNDS)
        {
            for (int i = 0; i < PlayerList.Count; i++)
            {
                PlayerList[i].PlayerRound = ROUND;

                // REDUCE ENERGY and HUNGER
                PlayerList[i].PlayerHunger -= 1;
                PlayerList[i].PlayerEnergy -= 1;

                // REWARD Last Round Work
                ReceiveRoundReward(i);

                // HEALTH CHECK
                int healthchcekresult = CheckPlayerHealth(i);

                if (healthchcekresult != 0)
                {
                    // player did NOT pass the check
                    PlayerList[i].AssignWork(healthchcekresult);
                }
                else
                {
                    // players passed, assign random work
                    int nextworkid = GetRandomWorkId(i);
                    PlayerList[i].AssignWork(nextworkid);
                }
            }

            ROUND += 1;
        }

        // SELL ALL ITEMS
        foreach (ServerPlayer pl in PlayerList)
        {
            pl.SellAllItems();
        }


        // SIMULATION FINISHED
        //DebugPrintResults();
        StartCoroutine(PrintSimResults(0.5f));

    }


    // TEST ONLY
    private void DebugPrintResults()
    {
        foreach (ServerPlayer pl in PlayerList)
        {
            Debug.Log(pl.PlayerName + ": " + pl.PlayerMoney.ToString() + " UGT");
        }
    }

    // CREATE 10 Players for testing
    private void InitPlayers()
    {
        PlayerList.Clear();

        for (int i = 0; i < 10; i++)
        {
            ServerPlayer p = new ServerPlayer()
            {
                PlayerId = 0,
                PlayerName = "UnskilledPlayer-" + i.ToString(),
                PlayerEnergy = 10,
                PlayerHunger = 10,
                PlayerMoney = 0,
                PlayerFood = 0,
                PlayerInventory = new List<ServerItem>(),
                PlayerFoodBought = 0,
                PlayerSalesQuality = new List<int>() { 0, 0, 0, 0 },
                PlayerSleepCycles = 0,
                PlayerMaxInventorySpace = 10,
                PlayerSkillsGathering = new List<int>() { 0, 0, 0, 0, 0, 0 },
                PlayerSkillsTreasure = new List<int>() { 0, 0, 0, 0, 0, 0, 0 },
                PlayerSkillsGeneral = new List<int>() { 0, 0, 0, 0, 0, 0 },
                PlayerActivity = 0,
                PlayerRound = 0
            };

            PlayerList.Add(p);
        }

        PlayerList[0].PlayerName = "SKILLED_Player-0";
    }

    // handles clicks from skill UI
    public void AssignSkill(int skillIndex)
    {

        asrc.Play();

        Color32 green = new Color32(0, 255, 52, 56);
        Color32 black = new Color32(0, 0, 0, 56);

        if (skillIndex >= 10 && skillIndex < 20)
        {
            if (!PlayerList[0].IsSkillSelected(skillIndex - 10, 1))
            {
                if (skillIndex - 10 > 0 && !PlayerList[0].IsSkillSelected(skillIndex - 10 - 1, 1))
                    return;  // PREVIOUS SKILL NEEDS TO HAVE 1 POINT
                PlayerList[0].PlayerSkillsGathering[skillIndex - 10] = 1;
                skillSelectorsGathering[skillIndex - 10].color = green;
            }
            else
            {
                PlayerList[0].PlayerSkillsGathering[skillIndex - 10] = 0;
                skillSelectorsGathering[skillIndex - 10].color = black;
            }
        }

        if (skillIndex >= 20 && skillIndex < 30)
        {
            if (!PlayerList[0].IsSkillSelected(skillIndex - 20, 2))
            {
                if (skillIndex - 20 > 0 && !PlayerList[0].IsSkillSelected(skillIndex - 20 - 1, 2))
                    return;  // PREVIOUS SKILL NEEDS TO HAVE 1 POINT

                PlayerList[0].PlayerSkillsTreasure[skillIndex - 20] = 1;
                skillSelectorsTreasure[skillIndex - 20].color = green;
            }
            else
            {
                PlayerList[0].PlayerSkillsTreasure[skillIndex - 20] = 0;
                skillSelectorsTreasure[skillIndex - 20].color = black;
            }
        }

        if (skillIndex >= 30 && skillIndex < 40)
        {
            if (!PlayerList[0].IsSkillSelected(skillIndex - 30, 3))
            {
                if (skillIndex - 30 > 0 && !PlayerList[0].IsSkillSelected(skillIndex - 30 - 1, 3))
                    return;  // PREVIOUS SKILL NEEDS TO HAVE 1 POINT
                PlayerList[0].PlayerSkillsGeneral[skillIndex - 30] = 1;
                skillSelectorsGeneral[skillIndex - 30].color = green;
            }
            else
            {
                PlayerList[0].PlayerSkillsGeneral[skillIndex - 30] = 0;
                skillSelectorsGeneral[skillIndex - 30].color = black;
            }
        }

        // UI 
        int count_selected_skills = PlayerList[0].PlayerSkillsGeneral.Sum() + PlayerList[0].PlayerSkillsTreasure.Sum() + PlayerList[0].PlayerSkillsGathering.Sum();
        label_skillcount.text = "Skills Selected: " + count_selected_skills;
        label_skillcost.text = "Skill Cost: " + (count_selected_skills * 200) + " <size=70%>UGT</size>";


    }

    // RETURNS next work ID
    private int GetRandomWorkId(int plrId)
    {
        int retval = 0;
        // on live server this is based 1st TX of the latest mainnet ethereum block
        System.Random rnd = new System.Random(Guid.NewGuid().GetHashCode() + plrId + ROUND + PlayerList[plrId].PlayerInventory.Count);
        retval = rnd.Next(10, 14);
        return retval;
    }

    // REWARD FOR CURRENT ACTVITY
    private void ReceiveRoundReward(int PlrId)
    {
        int activityId = PlayerList[PlrId].PlayerActivity;

        // reward gathering
        if (activityId >= 10 && activityId <= 12)
            RewardBaseItem(PlrId, activityId);

        // reward treasure
        if (activityId == 13)
            RewardTreasureItem(PlrId, activityId);

        // reward sleep
        if (activityId == 2)
            RewardSleep(PlrId, activityId);

        // reward eating
        if (activityId == 3)
            RewardEating(PlrId, activityId);

        // reward buying food (players also sells everything in their inventory)
        if (activityId == 5)
            RewardBuyingFood(PlrId, activityId);

        // reward selling items
        if (activityId == 4)
            RewardSellingItems(PlrId, activityId);


    }


    // CHECK HUNGER, ENERGY, FOOD and INVENTORY 
    // before assigning next work
    private int CheckPlayerHealth(int PlrId)
    {
        // CHECK ENERGY
        if (PlayerList[PlrId].PlayerEnergy <= 2)
        {
            return 2; // player needs to sleep
        }

        // CHECK HUNGER
        if (PlayerList[PlrId].PlayerHunger <= 1)
        {
            if (PlayerList[PlrId].PlayerFood < 2)
                return 5; // LOW on food: visit vendor
            else
                return 3; // player needs to EAT food
        }

        // CHECK INVENTORY
        if (PlayerList[PlrId].GetRemainingInventorySpace() <= 0)
            return 4; // inventory is FULL, time to sell

        // PASSED ALL HEATH CHECKS
        return 0;
    }

    private void RewardTreasureItem(int PlrId, int ActivityId)
    {
        int basechance = 20;
        int rarechance = 1;
        int epichance = 0;
        int legendarychance = 0;


        // SKILL BONUS
        basechance += PlayerList[PlrId].GetTreasureSkillBase();
        rarechance += (PlayerList[PlrId].PlayerSkillsTreasure[4] * 25);
        epichance += (PlayerList[PlrId].PlayerSkillsTreasure[5] * 5);
        legendarychance += (PlayerList[PlrId].PlayerSkillsTreasure[6] * 1);

        bool baseitemreceived = UnityEngine.Random.Range(0, 100) < basechance;

        if (baseitemreceived)
        {
            // UPGRADE (?)
            int finalQuality = 0;
            finalQuality = (UnityEngine.Random.Range(0, 100) < rarechance) ? 1 : finalQuality; // upgrade to rare
            finalQuality = (UnityEngine.Random.Range(0, 100) < epichance) ? 2 : finalQuality; // upgrade to epic
            finalQuality = (UnityEngine.Random.Range(0, 100) < legendarychance) ? 3 : finalQuality; // upgrade to legendary

            // create the item
            ServerItem item = new ServerItem()
            {
                ItemId = ROUND,
                ItemName = "Treasure-" + ROUND.ToString(),
                ItemQuality = finalQuality,
                ItemSellPrice = treasurePrices[finalQuality],
                ItemType = 3,
                ItemQuantity = 1
            };

            // add to inventory
            PlayerList[PlrId].AddInventoryItem(item);
        }
    }
    private void RewardBaseItem(int PlrId, int ActivityId)
    {
        // REWARD WOOD, STONE or OTHER ITEM (ActivityId 10, 11, 12)
        int aid = ActivityId - 10;
        string[] itemnames = { "Wood", "Stone", "Item" };

        ServerItem item = new ServerItem()
        {
            ItemId = ROUND,
            ItemName = itemnames[aid],
            ItemQuantity = 1,
            ItemSellPrice = 10, // 10 UGT normal item
            ItemType = aid,
            ItemQuality = 0
        };

        // SKILL BONUS: extra item
        int extraitemchance = PlayerList[PlrId].GetGatheringSkillExtraItemChance();
        if (extraitemchance > 0 && UnityEngine.Random.Range(0, 100) < extraitemchance)
        {
            PlayerList[PlrId].AddInventoryItem(item);
        }



        // SKILL BONUS: upgrade to RARE
        if (PlayerList[PlrId].PlayerSkillsGathering[4] == 1 && UnityEngine.Random.Range(0, 100) < 10)
        {
            item.ItemName = itemnames[aid] + "_RARE";
            item.ItemQuality = 1;
            item.ItemSellPrice = 25; // 25 UGT RARE item
            PlayerList[PlrId].AddInventoryItem(item);
        }
        else
        {
            // only ADD 1st item - no upgrade
            PlayerList[PlrId].AddInventoryItem(item);
        }

        // SKILL BONUS: 25% chance to receive 1 food
        if (PlayerList[PlrId].PlayerSkillsGeneral[3] == 1 && UnityEngine.Random.Range(0, 100) < 25)
        {
            PlayerList[PlrId].PlayerFood += 1;
        }

        // SKILL BONUS: 10% chance to restore one energy
        if (PlayerList[PlrId].PlayerSkillsGeneral[5] == 1 && UnityEngine.Random.Range(0, 100) < 10)
        {
            PlayerList[PlrId].PlayerEnergy += 1;
        }


    }
    private void RewardSellingItems(int PlrId, int ActivityId)
    {
        // SELL ALL ITEMS 
        PlayerList[PlrId].SellAllItems();
    }
    private void RewardBuyingFood(int PlrId, int ActivityId)
    {
        // sell items first
        RewardSellingItems(PlrId, ActivityId);
        // buys food x5
        PlayerList[PlrId].BuyFood(5);
    }
    private void RewardEating(int PlrId, int ActivityId)
    {
        // eat one food item / restore 5 hunger
        PlayerList[PlrId].EatOneFood();
    }
    private void RewardSleep(int PlrId, int ActivityId)
    {
        // sleep ONE cycle
        PlayerList[PlrId].SleepOneCycle();
    }

    // PRINT RESULTS
    private IEnumerator PrintSimResults(float delay)
    {
        for (int i = 1; i < PlayerList.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            label_resultsothers.text = label_resultsothers.text + " " + PlayerList[i].PlayerName + ": " + PlayerList[i].PlayerMoney + " UGT\n";
        }

        label_resultsplayer.text = PlayerList[0].GetFinalStats();
        restartButton.enabled = true;
    }

    public void PlayButtonAudio()
    {
        asrc.Play();
    }

    public void OpenWeb()
    {
        Application.OpenURL("https://polyville.life/");
    }
}