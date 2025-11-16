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
    
    RewardCameraConroller cameraConroller;
    Inventory inventory;
    
    PlayerData playerData;
    int winnerIndex;
    
    List<Block> rewardBlocks = new List<Block>();
    private bool interactable;

    private void Awake()
    {
        StateMachine = new FSM<UpgradeManager>(this);
        inventory = GetComponent<Inventory>();
        cameraConroller = FindFirstObjectByType<RewardCameraConroller>();
    }
    
    private void Start()
    {
        StateMachine.Set<RewardState>();
        StateMachine.Update();
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public class RewardState : State<UpgradeManager> {
        public override void OnBegin(UpgradeManager owner)
        {
            owner.interactable = false;
            owner.winnerIndex = GameManager.Instance.GetPreviousWinner();
            GameManager.Instance.GetPlayerData(out owner.playerData, owner.winnerIndex);
            owner.inventory.LoadFromPlayerData(owner.playerData, (string id) => owner.presetBlocks[id]);
            
            
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
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                owner.inventory.SaveToPlayerData(owner.playerData);
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
            rewardBlocks.Add(Instantiate(blocks[i]).GetComponent<Block>());
        
        // 보여주기
        for (int i = 0; i < rewardBlocks.Count; i++)
            rewardBlocks[i].Appear(i, 3);
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
}
