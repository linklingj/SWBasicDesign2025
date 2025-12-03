using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UpgradeManager : SerializedMonoBehaviour 
{
    public FSM<UpgradeManager> StateMachine { get; private set; }
    
    [Header("Data")]
    [SerializeField] RewardBlockWeightTable blockWeightTable;
    [SerializeField] RewardBlocks blockTable;
    [SerializeField] BlockAnimatorData animData;
    
    [Header("Preset")]
    [SerializeField] Dictionary<string, Block> presetBlocks;
    
    [Header("UI")]
    [SerializeField] BlocksUI blocksUI;
    [SerializeField] GameObject toNextButton;
    [SerializeField] private Dictionary<MapType, GameObject> bgObjects;
    
    [Header("Sound")]
    [SerializeField] AudioData upgradeBGM;

    
    RewardCameraConroller cameraConroller;
    Inventory inventory;
    
    PlayerData playerData;
    int loserIndex;
    
    List<Block> rewardBlocks = new List<Block>();
    Block selectedBlock;
    private bool interactable;

    private void Awake()
    {
        StateMachine = new FSM<UpgradeManager>(this);
        inventory = GetComponent<Inventory>();
        cameraConroller = FindFirstObjectByType<RewardCameraConroller>();
        
        foreach (var bg in bgObjects)
            bg.Value.SetActive(false);
        bgObjects[GameManager.Instance.currentMapType].SetActive(true);
    }
    
    private void Start()
    {
        AudioPlayer.Instance.PlayBGM(upgradeBGM);
        StateMachine.Set<RewardState>();
        StateMachine.Update();
        Time.timeScale = 1;
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public class RewardState : State<UpgradeManager> {
        public override void OnBegin(UpgradeManager owner)
        {
            owner.interactable = false;
            owner.toNextButton.SetActive(false);
            owner.loserIndex = GameManager.Instance.GetPreviousLoser();
            GameManager.Instance.GetPlayerData(out owner.playerData, owner.loserIndex);
            owner.inventory.LoadFromPlayerData(owner.playerData, (string id) => owner.presetBlocks[id]);
            
            owner.blocksUI.SetTitleText(owner.loserIndex);
            owner.blocksUI.SetUpgradeText(owner.inventory.blocks);
            
            owner.GenerateRewardBlocks();
            
            DOVirtual.DelayedCall(owner.animData.appearDuration, () => owner.interactable = true, false);
        }

        public override void OnUpdate(UpgradeManager owner) { }

        public IEnumerator BlockSelected(UpgradeManager owner)
        {
            owner.interactable = false;
            //선택 블록 애니메이션 -> 카메라 애니메이션 -> 상태 전환
            yield return new WaitForSeconds(owner.animData.selectDuration);
            owner.cameraConroller.SetCameraByState<InventorySetState>(owner.animData.inventoryTransitionDuration, () => Set<InventorySetState>());
        }

        public override void OnEnd(UpgradeManager owner) { }
    }
    
    public class InventorySetState : State<UpgradeManager>
    {
        public override void OnBegin(UpgradeManager owner)
        {
            owner.interactable = true;
        }

        public override void OnUpdate(UpgradeManager owner)
        {
            if (owner.selectedBlock != null && owner.selectedBlock.IsPlaced && !owner.toNextButton.activeSelf)
            {
                owner.BlockPlaced();
            }
        }

        public override void OnEnd(UpgradeManager owner)
        {
            
        }
    }

    private void GenerateRewardBlocks()
    {
        var blocks = GetRewardBlocks();
        for (int i = 0; i < blocks.Count; i++)
        {
            rewardBlocks.Add(Instantiate(blocks[i]).GetComponent<Block>());
            rewardBlocks[i].gameObject.SetActive(false);
        }
        
        StartCoroutine(ShowRewardBlocks());
    }
    
    private IEnumerator ShowRewardBlocks()
    {
        for (int i = 0; i < rewardBlocks.Count; i++)
        {
            rewardBlocks[i].gameObject.SetActive(true);
            rewardBlocks[i].Appear(i, 3);
            blocksUI.SetRewardText(i, rewardBlocks[i]);
            yield return new WaitForSeconds(0.3f);
        }
    }

    // 패배자에게 주어지는 보상 블록 랜덤 생성
    private List<GameObject> GetRewardBlocks(int cnt = 3)
    {
        var blockList = new List<GameObject>();
        for (int i = 0; i < cnt; i++)
        {
            var rarity = blockWeightTable.GetBlockRarity(blockList.Count);
            int attempts = 0;
            int maxAttempts = 10;
            while (attempts < maxAttempts)
            {
                var newBlock = blockTable.GetBlockByRarity(rarity);
                if (!blockList.Contains(newBlock))
                {
                    blockList.Add(newBlock);
                    break;
                }
                attempts++;
            }
        }
        
        //특수 케이스
        while (blockList.Count < cnt)
        {
            var newBlock = blockTable.GetBlockByRarity(blockWeightTable.GetBlockRarity(blockList.Count));
            blockList.Add(newBlock);
        }

        return blockList;
    }

    public void SelectBlock(Block block)
    {
        if (!interactable) return;
        var state = StateMachine.State as RewardState;
        if (state != null)
        {
            selectedBlock = block;
            foreach (var b in rewardBlocks)
            {
                if (b == block)
                    b.Selected();
                else
                    b.NotSelected();
            }
            StartCoroutine(state.BlockSelected(this));
        }
    }

    public void BlockPlaced()
    {
        toNextButton.SetActive(true);
        blocksUI.SetUpgradeText(inventory.blocks);
    }
    
    public void NextRound()
    {
        inventory.SaveToPlayerData(playerData);
        inventory.UpadatePlayerStats(playerData, (string id) => presetBlocks[id]);
        
        GameManager.Instance.BattleStart();
    }

    public void OnCountdownFinished()
    {
        GameManager.Instance.ToResult();
    }
}
