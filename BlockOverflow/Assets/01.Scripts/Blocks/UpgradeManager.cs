using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour 
{
    public FSM<UpgradeManager> StateMachine { get; private set; }
    
    private Inventory inventory;
    
    [SerializeField] RewardBlockWeightTable blockWeightTable;
    [SerializeField] RewardBlocks blockTable;

    [SerializeField] private Block selectedBlock;

    [SerializeField] private Vector3 blockGeneratePos;
    [SerializeField] private float blockGenerateSpread;
    
    [SerializeField] private float transitionTime;
    
    [SerializeField] RewardCameraConroller cameraConroller;

    private bool interactable;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        StateMachine = new FSM<UpgradeManager>(this);
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
        private float beginAnimDuration = 0.5f;
        
        public override void OnBegin(UpgradeManager owner)
        {
            owner.interactable = false;
            
            //블록 선택하여 가져오기
            owner.GenerateRewardBlocks();
            
            DOVirtual.DelayedCall(beginAnimDuration, () => owner.interactable = true);
        }

        public override void OnUpdate(UpgradeManager owner)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame && owner.interactable)
            {
                owner.cameraConroller.SetCameraByState<InventorySetState>(owner.transitionTime, () => Set<InventorySetState>());
            }
        }

        public override void OnEnd(UpgradeManager owner)
        {
            Debug.Log("Exiting Reward State");
        }
    }
    
    public class InventorySetState : State<UpgradeManager>
    {
        public override void OnBegin(UpgradeManager owner)
        {
            Debug.Log("Entering Inventory State");
            owner.interactable = true;
        }

        public override void OnUpdate(UpgradeManager owner)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
                Debug.Log("to battle scene");
        }

        public override void OnEnd(UpgradeManager owner)
        {
            Debug.Log("Exiting Inventory State");
        }
    }

    private void GenerateRewardBlocks()
    {
        int generateCnt = 3;
        var blocks = GetRewardBlocks(generateCnt);
        for (int i = 0; i < blocks.Count; i++)
        {
            float diff = i - (float)(generateCnt - 1) / 2;
            Vector3 offset = new Vector3(diff * blockGenerateSpread, 0, 0);
            Instantiate(blocks[i], blockGeneratePos + offset, Quaternion.identity);
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
        
        // foreach (var obj in blockList)
        //     obj.GetComponent<BlockAnimatior>().FirstAppearAnim();

        return blockList;
    }
}
